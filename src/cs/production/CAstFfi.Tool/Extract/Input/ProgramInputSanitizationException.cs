// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace CAstFfi.Extract.Input;

public sealed class ProgramInputSanitizationException : Exception
{
    public ProgramInputSanitizationException()
    {
    }

    public ProgramInputSanitizationException(string message)
        : base(message)
    {
    }

    public ProgramInputSanitizationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
