#pragma once

typedef enum Enum_Force_UInt64 {
    ENUM_FORCE_UINT64_DAY_UNKNOWN,
    ENUM_FORCE_UINT64_DAY_MONDAY,
    ENUM_FORCE_UINT64_DAY_TUESDAY,
    ENUM_FORCE_UINT64_DAY_WEDNESDAY,
    ENUM_FORCE_UINT64_DAY_THURSDAY,
    ENUM_FORCE_UINT64_DAY_FRIDAY,
    _ENUM_FORCE_UINT64 = 0xffffffff
} Enum_Force_UInt64;

FFI_API_DECL void Enum_Force_UInt64__print_Enum_Force_UInt64(const Enum_Force_UInt64 e)
{
    printf("%d\n", e); // Print used for testing
}

FFI_API_DECL Enum_Force_UInt64 Enum_Force_UInt64__return_Enum_Force_UInt64(const Enum_Force_UInt64 e)
{
    return e;
}