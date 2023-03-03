#pragma once

typedef enum Enum_Force_SInt8 {
    ENUM_FORCE_SINT8_DAY_UNKNOWN,
    ENUM_FORCE_SINT8_DAY_MONDAY,
    ENUM_FORCE_SINT8_DAY_TUESDAY,
    ENUM_FORCE_SINT8_DAY_WEDNESDAY,
    ENUM_FORCE_SINT8_DAY_THURSDAY,
    ENUM_FORCE_SINT8_DAY_FRIDAY,
    _ENUM_FORCE_SINT8 = 0x7F
} Enum_Force_SInt8;

FFI_API_DECL void Enum_Force_SInt8__print_Enum_Force_SInt8(const Enum_Force_SInt8 e)
{
    printf("%d\n", e); // Print used for testing
}

FFI_API_DECL Enum_Force_SInt8 Enum_ForceSInt8__return_Enum_Force_SInt8(const Enum_Force_SInt8 e)
{
    return e;
}