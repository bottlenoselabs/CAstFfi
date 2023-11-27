#pragma once

typedef enum EnumIgnored {
    ENUM_IGNORED_DAY_UNKNOWN,
    ENUM_IGNORED_DAY_MONDAY,
    ENUM_IGNORED_DAY_TUESDAY,
    ENUM_IGNORED_DAY_WEDNESDAY,
    ENUM_IGNORED_DAY_THURSDAY,
    ENUM_IGNORED_DAY_FRIDAY,
    _ENUM_IGNORED_SINT8 = 0x7F
} EnumIgnored;

FFI_API_DECL void EnumIgnored__print_EnumIgnored(const EnumIgnored e)
{
    printf("%d\n", e); // Print used for testing
}

FFI_API_DECL EnumIgnored EnumIgnored__return_EnumIgnored(const EnumIgnored e)
{
    return e;
}