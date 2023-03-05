// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("CAstFfi.Tool")]

namespace CAstFfi.Data.Serialization;

public static class CJsonSerializer
{
    private static readonly CJsonSerializerContextTargetPlatform ContextTargetPlatform = new(new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    });

    private static readonly CJsonSerializerContextCrossPlatform ContextCrossPlatform = new(new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    });

    public static CAbstractSyntaxTreeTargetPlatform ReadAbstractSyntaxTreeTargetPlatform(string filePath)
    {
        var fullFilePath = Path.GetFullPath(filePath);
        var fileContents = File.ReadAllText(fullFilePath);
        var result = JsonSerializer.Deserialize(fileContents, ContextTargetPlatform.CAbstractSyntaxTreeTargetPlatform)!;
        FillNamesTargetPlatform(result);
        return result;
    }

    public static CAbstractSyntaxTreeCrossPlatform ReadAbstractSyntaxTreeCrossPlatform(string filePath)
    {
        var fullFilePath = Path.GetFullPath(filePath);
        var fileContents = File.ReadAllText(fullFilePath);
        var result = JsonSerializer.Deserialize(fileContents, ContextCrossPlatform.CAbstractSyntaxTreeCrossPlatform)!;
        FillNamesCrossPlatform(result);
        return result;
    }

    internal static void WriteAbstractSyntaxTreeTargetPlatform(CAbstractSyntaxTreeTargetPlatform abstractSyntaxTree, string filePath)
    {
        var fullFilePath = Path.GetFullPath(filePath);

        var outputDirectory = Path.GetDirectoryName(fullFilePath)!;
        if (string.IsNullOrEmpty(outputDirectory))
        {
            outputDirectory = AppContext.BaseDirectory;
            fullFilePath = Path.Combine(Environment.CurrentDirectory, fullFilePath);
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }

        var fileContents = System.Text.Json.JsonSerializer.Serialize(abstractSyntaxTree, ContextTargetPlatform.Options);

        using var fileStream = File.OpenWrite(fullFilePath);
        using var textWriter = new StreamWriter(fileStream);
        textWriter.Write(fileContents);
        textWriter.Close();
        fileStream.Close();
    }

    internal static void WriteAbstractSyntaxTreeCrossPlatform(CAbstractSyntaxTreeCrossPlatform abstractSyntaxTree, string filePath)
    {
        var fullFilePath = Path.GetFullPath(filePath);

        var outputDirectory = Path.GetDirectoryName(fullFilePath)!;
        if (string.IsNullOrEmpty(outputDirectory))
        {
            outputDirectory = AppContext.BaseDirectory;
            fullFilePath = Path.Combine(Environment.CurrentDirectory, fullFilePath);
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }

        var fileContents = System.Text.Json.JsonSerializer.Serialize(abstractSyntaxTree, ContextCrossPlatform.Options);

        using var fileStream = File.OpenWrite(fullFilePath);
        using var textWriter = new StreamWriter(fileStream);
        textWriter.Write(fileContents);
        textWriter.Close();
        fileStream.Close();
    }

    private static void FillNamesTargetPlatform(CAbstractSyntaxTreeTargetPlatform abstractSyntaxTree)
    {
        foreach (var keyValuePair in abstractSyntaxTree.MacroObjects)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Variables)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Functions)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Records)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Enums)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.TypeAliases)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.OpaqueTypes)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.FunctionPointers)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }
    }

    private static void FillNamesCrossPlatform(CAbstractSyntaxTreeCrossPlatform abstractSyntaxTree)
    {
        foreach (var keyValuePair in abstractSyntaxTree.MacroObjects)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Variables)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Functions)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Records)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.Enums)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.TypeAliases)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.OpaqueTypes)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }

        foreach (var keyValuePair in abstractSyntaxTree.FunctionPointers)
        {
            keyValuePair.Value.Name = keyValuePair.Key;
        }
    }
}
