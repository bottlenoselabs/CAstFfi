#pragma once

typedef enum Enum_Force_UInt32 {
    ENUM_FORCE_UINT32_DAY_UNKNOWN,
    ENUM_FORCE_UINT32_DAY_MONDAY,
    ENUM_FORCE_UINT32_DAY_TUESDAY,
    ENUM_FORCE_UINT32_DAY_WEDNESDAY,
    ENUM_FORCE_UINT32_DAY_THURSDAY,
    ENUM_FORCE_UINT32_DAY_FRIDAY,
    _ENUM_FORCE_UINT32 = 0xFFFFFFFFUL
} Enum_Force_UInt32;

FFI_API_DECL void Enum_Force_UInt32__print_Enum_Force_UInt32(const Enum_Force_UInt32 e)
{
    printf("%d\n", e); // Print used for testing
}

FFI_API_DECL Enum_Force_UInt32 Enum_Force_UInt32__return_Enum_Force_UInt32(const Enum_Force_UInt32 e)
{
    return e;
}