// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using bottlenoselabs.Common;
using CAstFfi.Data;
using CAstFfi.Features.Extract.Infrastructure.Clang;
using CAstFfi.Features.Extract.Input.Sanitized;
using Microsoft.Extensions.Logging;
using static bottlenoselabs.clang;

namespace CAstFfi.Features.Extract.Domain.Parse;

public sealed partial class Parser
{
    private readonly ILogger<Parser> _logger;
    private readonly Features.Extract.Domain.Parse.ClangArgumentsBuilder _clangArgumentsBuilder;

    public Parser(
        ILogger<Parser> logger,
        Features.Extract.Domain.Parse.ClangArgumentsBuilder clangArgumentsBuilder)
    {
        _logger = logger;
        _clangArgumentsBuilder = clangArgumentsBuilder;
    }

    public void CleanUp()
    {
        _clangArgumentsBuilder.CleanUp();
    }

    public CXTranslationUnit TranslationUnit(
        string filePath,
        TargetPlatform targetPlatform,
        ExtractParseOptions options,
        out ImmutableDictionary<string, string> linkedPaths,
        bool ignoreDiagnostics = false,
        bool keepGoing = false)
    {
        var argumentsBuilderResult = _clangArgumentsBuilder.Build(
            targetPlatform,
            options,
            false,
            false);

        linkedPaths = argumentsBuilderResult.LinkedPaths;
        var arguments = argumentsBuilderResult.Arguments;
        var argumentsString = string.Join(" ", arguments);

        if (!TryParseTranslationUnit(filePath, arguments, out var translationUnit, true, keepGoing))
        {
            var up = new ClangException();
            LogFailureInvalidArguments(filePath, argumentsString, up);
            throw up;
        }

        var isSuccess = true;
        if (!ignoreDiagnostics)
        {
            var clangDiagnostics = GetClangDiagnostics(translationUnit);
            var stringBuilder = new StringBuilder();

            if (!clangDiagnostics.IsDefaultOrEmpty)
            {
                foreach (var clangDiagnostic in clangDiagnostics)
                {
                    if (clangDiagnostic.IsErrorOrFatal)
                    {
                        isSuccess = false;
                    }

                    stringBuilder.AppendLine(clangDiagnostic.Message);
                }
            }

            var clangDiagnosticMessagesJoined = stringBuilder.ToString();

            if (isSuccess)
            {
                LogSuccessWithDiagnostics(filePath, argumentsString, clangDiagnosticMessagesJoined);
            }
            else
            {
                LogFailureWithDiagnostics(filePath, argumentsString, clangDiagnosticMessagesJoined);
            }
        }

        return translationUnit;
    }

    public ImmutableArray<MacroObjectCandidate> MacroObjectCandidates(
        CXTranslationUnit translationUnit,
        TargetPlatform targetPlatform,
        ExtractParseOptions options)
    {
        var argumentsBuilderResult = _clangArgumentsBuilder.Build(
            targetPlatform,
            options,
            false,
            true);
        var linkedPaths = argumentsBuilderResult.LinkedPaths;
        var cursors = MacroObjectCursors(translationUnit, options);
        var macroObjectCandidates = MacroObjectCandidates(options, cursors, linkedPaths);
        return macroObjectCandidates;
    }

    public ImmutableArray<CMacroObject> MacroObjects(
        ImmutableArray<MacroObjectCandidate> macroObjectCandidates,
        TargetPlatform targetPlatform,
        ExtractParseOptions options)
    {
        var argumentsBuilderResult = _clangArgumentsBuilder.Build(
            targetPlatform,
            options,
            true,
            false);
        var arguments = argumentsBuilderResult.Arguments;
        var filePath = WriteMacroObjectsFile(macroObjectCandidates);
        var macroObjects = Macros(options, arguments, filePath);

        File.Delete(filePath);
        var result = macroObjects.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        return result;
    }

    private ImmutableArray<CMacroObject> Macros(
        ExtractParseOptions options,
        ImmutableArray<string> arguments,
        string filePath)
    {
        var argumentsString = string.Join(" ", arguments);
        var parsedTranslationUnit = TryParseTranslationUnit(filePath, arguments, out var translationUnit, false, true);
        if (!parsedTranslationUnit)
        {
            var up = new ClangException();
            LogFailureInvalidArguments(filePath, argumentsString, up);
            throw up;
        }

        using var streamReader = new StreamReader(filePath);
        var result = MacroObjects(options, translationUnit, streamReader);
        clang_disposeTranslationUnit(translationUnit);
        return result;
    }

    private ImmutableArray<CMacroObject> MacroObjects(
        ExtractParseOptions options, CXTranslationUnit translationUnit, StreamReader reader)
    {
        var builder = ImmutableArray.CreateBuilder<CMacroObject>();

        var translationUnitCursor = clang_getTranslationUnitCursor(translationUnit);
        var functionCursor = translationUnitCursor
            .GetDescendents(static (cursor, _) => IsFunctionWithMacroVariables(cursor)).FirstOrDefault();
        var compoundStatement = functionCursor.GetDescendents(static (cursor, _) => IsCompoundStatement(cursor))
            .FirstOrDefault();
        var declarationStatements =
            compoundStatement.GetDescendents(static (cursor, _) => IsDeclarationStatement(cursor));
        var readerLineNumber = 0;

        foreach (var declarationStatement in declarationStatements)
        {
            var variable = declarationStatement.GetDescendents(static (cursor, _) => IsVariable(cursor))
                .FirstOrDefault();
            var variableName = variable.Name();
            var macroName =
                variableName.Replace("variable_", string.Empty, StringComparison.InvariantCultureIgnoreCase);
            var cursor = variable.GetDescendents().FirstOrDefault();
            var macro = MacroObject(options, macroName, cursor, reader, ref readerLineNumber);
            if (macro == null)
            {
                continue;
            }

            builder.Add(macro);
        }

        return builder.ToImmutable();
    }

    private CMacroObject? MacroObject(
        ExtractParseOptions options,
        string name,
        CXCursor cursor,
        StreamReader reader,
        ref int readerLineNumber)
    {
        var type = clang_getCursorType(cursor);

        var value = EvaluateMacroValue(cursor, type);
        if (value == null)
        {
            return null;
        }

        var location = MacroLocation(cursor, reader, ref readerLineNumber, options.UserIncludeDirectories);

        var kind = MacroTypeKind(type);
        var typeName = type.Name();
        var sizeOf = (int)clang_Type_getSizeOf(type);
        var typeInfo = new CTypeInfo
        {
            Name = typeName,
            Kind = kind,
            SizeOf = sizeOf
        };

        var macro = new CMacroObject
        {
            Name = name,
            Value = value,
            TypeInfo = typeInfo,
            Location = location
        };

        return macro;
    }

    private static CKind MacroTypeKind(CXType type)
    {
        if (type.IsPrimitive())
        {
            return CKind.Primitive;
        }

        return type.kind switch
        {
            CXTypeKind.CXType_Typedef => CKind.TypeAlias,
            CXTypeKind.CXType_Enum => CKind.Enum,
            CXTypeKind.CXType_Pointer => CKind.Pointer,
            CXTypeKind.CXType_ConstantArray => CKind.Array,
            _ => CKind.Unknown
        };
    }

    private CLocation MacroLocation(
        CXCursor cursor,
        StreamReader reader,
        ref int readerLineNumber,
        ImmutableArray<string> userIncludeDirectories)
    {
        var location = cursor.GetLocation(includeDirectories: userIncludeDirectories);
        var locationCommentLineNumber = location!.Value.LineNumber - 1;

        if (readerLineNumber > locationCommentLineNumber)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            readerLineNumber = 0;
        }

        var line = string.Empty;
        while (readerLineNumber != locationCommentLineNumber)
        {
            line = reader.ReadLine() ?? string.Empty;
            readerLineNumber++;
        }

        var locationString = line.Trim().TrimStart('/').Trim();
        var lineIndex = locationString.IndexOf(':', StringComparison.InvariantCulture);
        var columnIndex = locationString.IndexOf(':', lineIndex + 1);
        var filePathIndex = locationString.IndexOf('(', StringComparison.InvariantCulture);

        int columnIndexEnd;
        if (filePathIndex == -1)
        {
            columnIndexEnd = locationString.Length;
        }
        else
        {
            columnIndexEnd = filePathIndex - 1;
        }

        var lineString = locationString[(lineIndex + 1).. columnIndex];
        var columnString = locationString[(columnIndex + 1).. columnIndexEnd];
        var fileNameString = locationString[..lineIndex];

        var filePathString = filePathIndex == -1 ? fileNameString : locationString[(filePathIndex + 1)..^1];
        var lineNumber = int.Parse(lineString, CultureInfo.InvariantCulture);
        var lineColumn = int.Parse(columnString, CultureInfo.InvariantCulture);

        var actualLocation = new CLocation
        {
            FileName = fileNameString,
            FilePath = filePathString,
            LineNumber = lineNumber,
            LineColumn = lineColumn
        };
        return actualLocation;
    }

    private static string? EvaluateMacroValue(CXCursor cursor, CXType type)
    {
        var evaluateResult = clang_Cursor_Evaluate(cursor);
        var kind = clang_EvalResult_getKind(evaluateResult);
        string value;

        switch (kind)
        {
            case CXEvalResultKind.CXEval_UnExposed:
                return null;
            case CXEvalResultKind.CXEval_Int:
            {
                var canonicalType = clang_getCanonicalType(type);
                if (canonicalType.IsSignedPrimitive())
                {
                    var integerValueSigned = clang_EvalResult_getAsInt(evaluateResult);
                    value = integerValueSigned.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    var integerValueUnsigned = clang_EvalResult_getAsUnsigned(evaluateResult);
                    value = integerValueUnsigned.ToString(CultureInfo.InvariantCulture);
                }

                break;
            }

            case CXEvalResultKind.CXEval_StrLiteral or CXEvalResultKind.CXEval_CFStr:
            {
                var stringValueC = clang_EvalResult_getAsStr(evaluateResult);
                var stringValue = Marshal.PtrToStringAnsi(stringValueC)!;
                value = "\"" + stringValue + "\"";
                break;
            }

            case CXEvalResultKind.CXEval_Float:
            {
                var floatValue = clang_EvalResult_getAsDouble(evaluateResult);
                value = floatValue.ToString(CultureInfo.InvariantCulture);
                break;
            }

            default:
                throw new NotImplementedException();
        }

        clang_EvalResult_dispose(evaluateResult);
        return value;
    }

    private static bool IsVariable(CXCursor cxCursor)
    {
        return cxCursor.kind == CXCursorKind.CXCursor_VarDecl;
    }

    private static bool IsDeclarationStatement(CXCursor cursor)
    {
        return cursor.kind == CXCursorKind.CXCursor_DeclStmt;
    }

    private static bool IsCompoundStatement(CXCursor cursor)
    {
        return cursor.kind == CXCursorKind.CXCursor_CompoundStmt;
    }

    private static bool IsFunctionWithMacroVariables(CXCursor cursor)
    {
        var sourceLocation = clang_getCursorLocation(cursor);
        var isFromMainFile = clang_Location_isFromMainFile(sourceLocation) > 0;
        if (!isFromMainFile)
        {
            return false;
        }

        return cursor.kind == CXCursorKind.CXCursor_FunctionDecl;
    }

    private static string WriteMacroObjectsFile(ImmutableArray<MacroObjectCandidate> macroObjectCandidates)
    {
        var tempFilePath = Path.GetTempFileName();
        using var fileStream = File.OpenWrite(tempFilePath);
        using var writer = new StreamWriter(fileStream);

        var includeHeaderFilePaths = new HashSet<string>();

        foreach (var macroObject in macroObjectCandidates)
        {
            var includeHeaderFilePath = macroObject.Location!.Value.FullFilePath;
            if (includeHeaderFilePaths.Contains(includeHeaderFilePath))
            {
                continue;
            }

            includeHeaderFilePaths.Add(includeHeaderFilePath);
            writer.Write("#include \"");
            writer.Write(includeHeaderFilePath);
            writer.WriteLine("\"");
        }

        var codeStart = @"
int main(void)
{";
        writer.WriteLine(codeStart);

        foreach (var macroObjectCandidate in macroObjectCandidates)
        {
            if (macroObjectCandidate.Tokens.IsDefaultOrEmpty)
            {
                continue;
            }

            writer.WriteLine("\t// " + macroObjectCandidate.Location);
            writer.Write("\tauto variable_");
            writer.Write(macroObjectCandidate.Name);
            writer.Write(" = ");
            foreach (var token in macroObjectCandidate.Tokens)
            {
                writer.Write(token);
            }

            writer.WriteLine(";");
        }

        const string codeEnd = @"
}";
        writer.WriteLine(codeEnd);
        writer.Flush();
        writer.Close();

        return tempFilePath;
    }

    private ImmutableArray<MacroObjectCandidate> MacroObjectCandidates(
        ExtractParseOptions options, ImmutableArray<CXCursor> cursors, ImmutableDictionary<string, string> linkedPaths)
    {
        var macroObjectsBuilder = ImmutableArray.CreateBuilder<MacroObjectCandidate>();
        foreach (var cursor in cursors)
        {
            var macroObjectCandidate = MacroObjectCandidate(
                cursor,
                linkedPaths,
                options.UserIncludeDirectories);
            if (macroObjectCandidate == null)
            {
                continue;
            }

            macroObjectsBuilder.Add(macroObjectCandidate);
        }

        return macroObjectsBuilder.ToImmutable();
    }

    private MacroObjectCandidate? MacroObjectCandidate(
        CXCursor cursor,
        ImmutableDictionary<string, string> linkedFileDirectoryPaths,
        ImmutableArray<string>? userIncludeDirectories)
    {
        var name = cursor.Name();
        var location = cursor.GetLocation(null, linkedFileDirectoryPaths);

        // clang doesn't have a thing where we can easily get a value of a macro
        // we need to:
        //  1. get the text range of the cursor
        //  2. get the tokens over said text range
        //  3. go through the tokens to parse the value
        // this means we get to do token parsing ourselves, yay!
        // NOTE: The first token will always be the name of the macro
        var translationUnit = clang_Cursor_getTranslationUnit(cursor);
        string[] tokens;
        unsafe
        {
            var range = clang_getCursorExtent(cursor);
            var tokensC = (CXToken*)0;
            uint tokensCount = 0;

            clang_tokenize(translationUnit, range, &tokensC, &tokensCount);

            var isFlag = tokensCount is 0 or 1;
            if (isFlag)
            {
                clang_disposeTokens(translationUnit, tokensC, tokensCount);
                return null;
            }

            tokens = new string[tokensCount - 1];
            for (var i = 1; i < (int)tokensCount; i++)
            {
                var tokenString = clang_getTokenSpelling(translationUnit, tokensC[i]).String();

                // CLANG BUG?: https://github.com/FNA-XNA/FAudio/blob/b84599a5e6d7811b02329709a166a337de158c5e/include/FAPOBase.h#L90
                if (tokenString.StartsWith('\\'))
                {
                    tokenString = tokenString.TrimStart('\\');
                }

                if (tokenString.StartsWith("__", StringComparison.InvariantCulture) &&
                    tokenString.EndsWith("__", StringComparison.InvariantCulture))
                {
                    clang_disposeTokens(translationUnit, tokensC, tokensCount);
                    return null;
                }

                tokens[i - 1] = tokenString.Trim();
            }

            clang_disposeTokens(translationUnit, tokensC, tokensCount);
        }

        var result = new MacroObjectCandidate
        {
            Name = name,
            Tokens = tokens.ToImmutableArray(),
            Location = location
        };

        return result;
    }

    private static ImmutableArray<CXCursor> MacroObjectCursors(
        CXTranslationUnit translationUnit,
        ExtractParseOptions options)
    {
        var translationUnitCursor = clang_getTranslationUnitCursor(translationUnit);

        var isEnabledSingleHeader = options.IsEnabledSingleHeader;
        var cursors = translationUnitCursor.GetDescendents(
            (child, _) => IsMacroOfInterest(child, options),
            !isEnabledSingleHeader);

        return cursors;
    }

    private static bool IsMacroOfInterest(
        CXCursor cursor,
        ExtractParseOptions options)
    {
        var kind = clang_getCursorKind(cursor);
        if (kind != CXCursorKind.CXCursor_MacroDefinition)
        {
            return false;
        }

        var isMacroBuiltIn = clang_Cursor_isMacroBuiltin(cursor) > 0;
        if (isMacroBuiltIn)
        {
            return false;
        }

        if (!options.IsEnabledSystemDeclarations)
        {
            var locationSource = clang_getCursorLocation(cursor);
            var isMacroSystem = clang_Location_isInSystemHeader(locationSource) > 0;
            if (isMacroSystem)
            {
                return false;
            }
        }

        var isMacroFunction = clang_Cursor_isMacroFunctionLike(cursor) > 0;
        if (isMacroFunction)
        {
            return false;
        }

        var name = cursor.Name();
        if (name.StartsWith('_'))
        {
            return false;
        }

        // Assume that macro ending with "API_DECL" are not interesting for bindgen
        if (name.EndsWith("API_DECL", StringComparison.InvariantCulture))
        {
            return false;
        }

        // Assume that macros starting with names of the C helper macros are not interesting for bindgen
        if (name.StartsWith("FFI_TARGET_", StringComparison.InvariantCulture))
        {
            return false;
        }

        return true;
    }

    private static ImmutableArray<ClangDiagnostic> GetClangDiagnostics(CXTranslationUnit translationUnit)
    {
        var diagnosticsCount = (int)clang_getNumDiagnostics(translationUnit);
        var builder = ImmutableArray.CreateBuilder<ClangDiagnostic>(diagnosticsCount);

        var defaultDisplayOptions = clang_defaultDiagnosticDisplayOptions();
        for (uint i = 0; i < diagnosticsCount; ++i)
        {
            var clangDiagnostic = clang_getDiagnostic(translationUnit, i);
            var clangString = clang_formatDiagnostic(clangDiagnostic, defaultDisplayOptions);
            var diagnosticString = clangString.String();
            var severity = clang_getDiagnosticSeverity(clangDiagnostic);

            var diagnostic = new ClangDiagnostic
            {
                IsErrorOrFatal = severity is CXDiagnosticSeverity.CXDiagnostic_Error or CXDiagnosticSeverity.CXDiagnostic_Fatal,
                Message = diagnosticString
            };

            builder.Add(diagnostic);
        }

        return builder.ToImmutable();
    }

    private static unsafe bool TryParseTranslationUnit(
        string filePath,
        ImmutableArray<string> commandLineArgs,
        out CXTranslationUnit translationUnit,
        bool skipFunctionBodies = true,
        bool keepGoing = false)
    {
        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        uint options = 0x0 |
                       0x1 | // CXTranslationUnit_DetailedPreprocessingRecord
                       0x80 | // IncludeBriefCommentsInCodeCompletion
                       0x1000 | // CXTranslationUnit_IncludeAttributedTypes
                       0x2000 | // CXTranslationUnit_VisitImplicitAttributes
                       0x4000 | // CXTranslationUnit_IgnoreNonErrorsFromIncludedFiles
                       0x0;

        if (skipFunctionBodies)
        {
            options |= 0x40; // CXTranslationUnit_SkipFunctionBodies
        }

        if (keepGoing)
        {
            options |= 0x200; // CXTranslationUnit_KeepGoing
        }

        var index = clang_createIndex(0, 0);
        var cSourceFilePath = CString.FromString(filePath);
        var cCommandLineArgs = CStrings.CStringArray(commandLineArgs.AsSpan());

        CXErrorCode errorCode;
        fixed (CXTranslationUnit* translationUnitPointer = &translationUnit)
        {
            errorCode = clang_parseTranslationUnit2(
                index,
                cSourceFilePath,
                cCommandLineArgs,
                commandLineArgs.Length,
                (CXUnsavedFile*)IntPtr.Zero,
                0,
                options,
                translationUnitPointer);
        }

        var result = errorCode == CXErrorCode.CXError_Success;
        return result;
    }

    [LoggerMessage(0, LogLevel.Error,
        "- Failed. The arguments are incorrect or invalid. Path: {FilePath} ; Clang arguments: {Arguments}")]
    private partial void LogFailureInvalidArguments(string filePath, string arguments, Exception exception);

    [LoggerMessage(1, LogLevel.Debug,
        "- Success. Path: {FilePath} ; Clang arguments: {Arguments} ; Diagnostics: {DiagnosticMessagesJoined}")]
    private partial void LogSuccessWithDiagnostics(string filePath, string arguments, string diagnosticMessagesJoined);

    [LoggerMessage(2, LogLevel.Error,
        "- Failed. One or more Clang diagnostics are reported when parsing that are an error or fatal. Path: {FilePath} ; Clang arguments: {Arguments} ; Diagnostics: {DiagnosticMessagesJoined}")]
    private partial void LogFailureWithDiagnostics(string filePath, string arguments, string diagnosticMessagesJoined);
}
