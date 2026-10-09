// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using @unsafe = unsafe_package;
using abi = global::go.@internal.abi_package;
using global::go.@internal;
using static global::go.@internal.reflectlite_package;

partial class reflectlite_internal_test_package {

// go2cs generated this placeholder — func Field is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func TField is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Field returns the i'th struct field.
public static global::go.@internal.reflectlite_package.ΔType StructFieldType(ж<abiꓸStructType> Ꮡt, nint i) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (i < 0 || i >= len(t.Fields)) {
        throw panic("reflect: Field index out of bounds");
    }
    var p = Ꮡ(t.Fields, i);
    return toType((~p).Typ);
}

// go2cs generated this placeholder — func Zero is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// ToInterface returns v's current value as an interface{}.
// It is equivalent to:
//
//	var i interface{} = (v's underlying value)
//
// It panics if the Value was obtained by accessing
// unexported struct fields.
public static any /*i*/ ToInterface(global::go.@internal.reflectlite_package.Value v) {
    return valueInterface(v);
}

public partial struct EmbedWithUnexpMeth {
}

internal static void f(this EmbedWithUnexpMeth _) {
}

internal partial interface pinUnexpMeth {
    void f();
}

internal static pinUnexpMeth pinUnexpMethI = ((pinUnexpMeth)new EmbedWithUnexpMeth(nil));

public static ж<byte> FirstMethodNameBytes(global::go.@internal.reflectlite_package.ΔType t) {
    _ = pinUnexpMethI;
    var ut = t.uncommon();
    if (ut == nil) {
        throw panic("type has no methods");
    }
    var m = ut.Methods()[0];
    var mname = t._<rtype>().nameOff(m.Name);
    if ((byte)(mname.DataChecked(0, nameFlagFieldˢ).Value & ((byte)(1 << (int)(2)))) == 0) {
        throw panic("method name does not have pkgPath *string");
    }
    return mname.Bytes;
}

public partial struct Buffer {
    internal slice<byte> buf;
}

} // end reflectlite_internal_test_package
