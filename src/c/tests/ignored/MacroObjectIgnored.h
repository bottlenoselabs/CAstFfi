#pragma once

#define MACRO_OBJECT_IGNORED 42

FFI_API_DECL int MacroObjectIgnored__return_int()
{
    return MACRO_OBJECT_IGNORED;
}