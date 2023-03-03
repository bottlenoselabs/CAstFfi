#pragma once

typedef enum Enum_Force_SInt64 {
    ENUM_FORCE_SINT64_DAY_UNKNOWN,
    ENUM_FORCE_SINT64_DAY_MONDAY,
    ENUM_FORCE_SINT64_DAY_TUESDAY,
    ENUM_FORCE_SINT64_DAY_WEDNESDAY,
    ENUM_FORCE_SINT64_DAY_THURSDAY,
    ENUM_FORCE_SINT64_DAY_FRIDAY,
    _ENUM_FORCE_SINT64 = 0x7FFFFFFF
} Enum_Force_SInt64;

FFI_API_DECL void Enum_Force_SInt64__print_Enum_Force_SInt64(const Enum_Force_SInt64 e)
{
    printf("%d\n", e); // Print used for testing
}

FFI_API_DECL Enum_Force_SInt64 Enum_Force_SInt64__return_Enum_Force_SInt64(const Enum_Force_SInt64 e)
{
    return e;
}