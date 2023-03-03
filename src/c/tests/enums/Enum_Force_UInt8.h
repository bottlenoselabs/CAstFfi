#pragma once

typedef enum Enum_Force_UInt8 {
    ENUM_FORCE_UINT8_DAY_UNKNOWN,
    ENUM_FORCE_UINT8_DAY_MONDAY,
    ENUM_FORCE_UINT8_DAY_TUESDAY,
    ENUM_FORCE_UINT8_DAY_WEDNESDAY,
    ENUM_FORCE_UINT8_DAY_THURSDAY,
    ENUM_FORCE_UINT8_DAY_FRIDAY,
    _ENUM_FORCE_UINT8 = 0xFF
} Enum_Force_UInt8;

FFI_API_DECL void Enum_Force_UInt8__print_Enum_Force_UInt8(const Enum_Force_UInt8 e)
{
    printf("%d\n", e); // Print used for testing
}

FFI_API_DECL Enum_Force_UInt8 Enum_Force_UInt8__return_Enum_Force_UInt8(const Enum_Force_UInt8 e)
{
    return e;
}