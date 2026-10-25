// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using unsafeheader = go.@internal.unsafeheader_package;
using reflect = reflect_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using go.@internal;

[GoFieldAccess(typeof(unsafeheader.Slice), typeof(unsafeheader.String))]
partial class unsafeheader_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sliceˢ = "Slice"u8;
private static readonly @string stringˢ = "String"u8;

// TestTypeMatchesReflectType ensures that the name and layout of the
// unsafeheader types matches the corresponding Header types in the reflect
// package.
public static void TestTypeMatchesReflectType(ж<testing.T> Ꮡt) {
    Ꮡt.Run(sliceˢ, (ж<testing.T> tΔ1) => {
        testHeaderMatchesReflect(tΔ1, new unsafeheader.Slice(nil), new reflect.SliceHeader(nil));
    });

    Ꮡt.Run(stringˢ, (ж<testing.T> tΔ2) => {
        testHeaderMatchesReflect(tΔ2, new unsafeheader.String(nil), new reflect.StringHeader(nil));
    });
}

internal static void testHeaderMatchesReflect(ж<testing.T> Ꮡt, any header, any reflectHeader) {
    var h = reflect.TypeOf(header);
    var rh = reflect.TypeOf(reflectHeader);

    for (nint i = 0; i < h.NumField(); i++) {
        var f = h.Field(i);
        var (rf, ok) = rh.FieldByName(f.Name);
        if (!ok) {
            Ꮡt.Errorf("Field %d of %v is named %s, but no such field exists in %v"u8, i, h, f.Name, rh);
            continue;
        }
        if (!typeCompatible(f.Type, rf.Type)) {
            Ꮡt.Errorf("%v.%s has type %v, but %v.%s has type %v"u8, h, f.Name, f.Type, rh, rf.Name, rf.Type);
        }
        if (f.Offset != rf.Offset) {
            Ꮡt.Errorf("%v.%s has offset %d, but %v.%s has offset %d"u8, h, f.Name, f.Offset, rh, rf.Name, rf.Offset);
        }
    }

    if (h.NumField() != rh.NumField()) {
        Ꮡt.Errorf("%v has %d fields, but %v has %d"u8, h, h.NumField(), rh, rh.NumField());
    }
    if (h.Align() != rh.Align()) {
        Ꮡt.Errorf("%v has alignment %d, but %v has alignment %d"u8, h, h.Align(), rh, rh.Align());
    }
}

internal static reflectꓸType unsafePointerType = reflect.TypeOf(@unsafe.Pointer.OrTypedNil((@unsafe.Pointer)default!));
internal static reflectꓸType uintptrType = reflect.TypeOf((uintptr)0);

internal static bool typeCompatible(reflectꓸType t, reflectꓸType rt) {
    return AreEqual(t, rt) || (AreEqual(t, unsafePointerType) && AreEqual(rt, uintptrType));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloCheckptrˢ = "Hello, checkptr!"u8;

// TestWriteThroughHeader ensures that the headers in the unsafeheader package
// can successfully mutate variables of the corresponding built-in types.
//
// This test is expected to fail under -race (which implicitly enables
// -d=checkptr) if the runtime views the header types as incompatible with the
// underlying built-in types.
public static void TestWriteThroughHeader(ж<testing.T> Ꮡt) {
    Ꮡt.Run(sliceˢ, (ж<testing.T> tΔ1) => {
        var s = slice<byte>("Hello, checkptr!"u8)[..5];

        ref var alias = ref heap<slice<byte>>(out var Ꮡalias);
        var hdr = Ꮡalias.Reinterpret<slice<byte>, unsafeheader.Slice>();
        hdr.Data = @unsafe.Pointer.FromPinnedBox(Ꮡ(s, 0));
        hdr.Cap = cap(s);
        hdr.Len = len(s);

        if (!bytes.Equal(alias, s)) {
            tΔ1.Errorf("alias of %T(%q) constructed via Slice = %T(%q)"u8, s, s, alias, alias);
        }
        if (cap(alias) != cap(s)) {
            tΔ1.Errorf("alias of %T with cap %d has cap %d"u8, s, cap(s), cap(alias));
        }
    });

    Ꮡt.Run(stringˢ, (ж<testing.T> tΔ2) => {
        ref var s = ref heap<@string>(out var Ꮡs);
        s = helloCheckptrˢ;

        ref var alias = ref heap(new @string(), out var Ꮡalias);
        var hdr = Ꮡalias.Reinterpret<@string, unsafeheader.String>();
        hdr.Data = (Ꮡs.Reinterpret<@string, unsafeheader.String>()).Value.Data;
        hdr.Len = len(s);

        if (alias != s) {
            tΔ2.Errorf("alias of %q constructed via String = %q"u8, s, alias);
        }
    });
}

} // end unsafeheader_test_package
