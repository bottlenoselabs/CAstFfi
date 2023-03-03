#pragma once

typedef enum Enum_Force_SInt16 {
    ENUM_FORCE_SINT16_DAY_UNKNOWN,
    ENUM_FORCE_SINT16_DAY_MONDAY,
    ENUM_FORCE_SINT16_DAY_TUESDAY,
    ENUM_FORCE_SINT16_DAY_WEDNESDAY,
    ENUM_FORCE_SINT16_DAY_THURSDAY,
    ENUM_FORCE_SINT16_DAY_FRIDAY,
    _ENUM_FORCE_SINT16 = 0x7FFF
} Enum_Force_SInt16;

FFI_API_DECL void Enum_Force_SInt16__print_Enum_Force_SInt16(const Enum_Force_SInt16 e)
{
    printf("%d\n", e); // Print used for testing
}

FFI_API_DECL Enum_Force_SInt16 Enum_Force_SInt16__return_Enum_Force_SInt16(const Enum_Force_SInt16 e)
{
    return e;
}