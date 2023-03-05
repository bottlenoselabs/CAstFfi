// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CAstFfi.Data.Serialization;

public class CTargetPlatformJsonConverter : JsonConverter<CTargetPlatform>
{
    public override CTargetPlatform Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return CTargetPlatform.Unknown;
        }

        var result = new CTargetPlatform(value);
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        CTargetPlatform value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.TargetName);
    }
}
