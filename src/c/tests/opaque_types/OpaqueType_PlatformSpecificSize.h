#pragma once

typedef struct OpaqueType_PlatformSpecificSize
{
    union
    {
#if defined(FFI_TARGET_OS_LINUX) && FFI_TARGET_OS_LINUX == 1
        struct
        {
            int a;
        } platform_specific;
#elif defined(FFI_TARGET_OS_MACOS) && FFI_TARGET_OS_MACOS == 1
        struct
        {
            int a;
            int b;
        } platform_specific;
#elif defined(FFI_TARGET_OS_IOS) && FFI_TARGET_OS_IOS == 1
        struct
        {
            int a;
            int b;
            int c;
        } platform_specific;
#elif defined(FFI_TARGET_OS_WINDOWS) && FFI_TARGET_OS_WINDOWS == 1
        struct
        {
            int a;
            int b;
            int c;
            int d;
        } platform_specific;
#else
    #error "Unknown platform."
#endif
    } platform_union;
} OpaqueType_PlatformSpecificSize;

FFI_API_DECL int calc_OpaqueType_PlatformSpecificSize(const OpaqueType_PlatformSpecificSize data)
{
    int sum;
#if defined(FFI_TARGET_OS_LINUX) && FFI_TARGET_OS_LINUX == 1
    sum += data.platform_union.platform_specific.a;
#elif defined(FFI_TARGET_OS_MACOS) && FFI_TARGET_OS_MACOS == 1
    sum += data.platform_union.platform_specific.a;
    sum += data.platform_union.platform_specific.b;
#elif defined(FFI_TARGET_OS_IOS) && FFI_TARGET_OS_IOS == 1
    sum += data.platform_union.platform_specific.a;
    sum += data.platform_union.platform_specific.b;
    sum += data.platform_union.platform_specific.c;
#elif defined(FFI_TARGET_OS_WINDOWS) && FFI_TARGET_OS_WINDOWS == 1
    sum += data.platform_union.platform_specific.a;
    sum += data.platform_union.platform_specific.b;
    sum += data.platform_union.platform_specific.c;
    sum += data.platform_union.platform_specific.d;
#else
    #error "Unknown platform."
#endif
    return sum;
}