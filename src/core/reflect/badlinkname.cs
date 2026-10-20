// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @internal;

partial class reflect_package {

// Widely used packages access these symbols using linkname,
// most notably:
//  - github.com/goccy/go-json
//  - github.com/goccy/go-reflect
//  - github.com/sohaha/zlsgo
//  - github.com/undefinedlabs/go-mpatch
//
// Do not remove or change the type signature.
// See go.dev/issue/67401
// and go.dev/issue/67279.

// ifaceIndir reports whether t is stored indirectly in an interface value.
// It is no longer used by this package and is here entirely for the
// linkname uses.
//
//go:linkname unusedIfaceIndir reflect.ifaceIndir
internal static bool unusedIfaceIndir(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    return !t.IsDirectIface();
}

//go:linkname valueInterface
// The compiler doesn't allow linknames on methods, for good reasons.
// We use this trick to push linknames of the methods.
// Do not call them in this package.

//go:linkname badlinkname_rtype_Align reflect.(*rtype).Align
internal static partial nint badlinkname_rtype_Align(ж<rtype> _);

//go:linkname badlinkname_rtype_AssignableTo reflect.(*rtype).AssignableTo
internal static partial bool badlinkname_rtype_AssignableTo(ж<rtype> _Δp0, ΔType _Δp1);

//go:linkname badlinkname_rtype_Bits reflect.(*rtype).Bits
internal static partial nint badlinkname_rtype_Bits(ж<rtype> _);

//go:linkname badlinkname_rtype_ChanDir reflect.(*rtype).ChanDir
internal static partial ΔChanDir badlinkname_rtype_ChanDir(ж<rtype> _);

//go:linkname badlinkname_rtype_Comparable reflect.(*rtype).Comparable
internal static partial bool badlinkname_rtype_Comparable(ж<rtype> _);

//go:linkname badlinkname_rtype_ConvertibleTo reflect.(*rtype).ConvertibleTo
internal static partial bool badlinkname_rtype_ConvertibleTo(ж<rtype> _Δp0, ΔType _Δp1);

//go:linkname badlinkname_rtype_Elem reflect.(*rtype).Elem
internal static partial ΔType badlinkname_rtype_Elem(ж<rtype> _);

//go:linkname badlinkname_rtype_Field reflect.(*rtype).Field
internal static partial StructField badlinkname_rtype_Field(ж<rtype> _Δp0, nint _Δp1);

//go:linkname badlinkname_rtype_FieldAlign reflect.(*rtype).FieldAlign
internal static partial nint badlinkname_rtype_FieldAlign(ж<rtype> _);

//go:linkname badlinkname_rtype_FieldByIndex reflect.(*rtype).FieldByIndex
internal static partial StructField badlinkname_rtype_FieldByIndex(ж<rtype> _Δp0, slice<nint> _Δp1);

//go:linkname badlinkname_rtype_FieldByName reflect.(*rtype).FieldByName
internal static partial (StructField, bool) badlinkname_rtype_FieldByName(ж<rtype> _Δp0, @string _Δp1);

//go:linkname badlinkname_rtype_FieldByNameFunc reflect.(*rtype).FieldByNameFunc
internal static partial (StructField, bool) badlinkname_rtype_FieldByNameFunc(ж<rtype> _Δp0, Func<@string, bool> _Δp1);

//go:linkname badlinkname_rtype_Implements reflect.(*rtype).Implements
internal static partial bool badlinkname_rtype_Implements(ж<rtype> _Δp0, ΔType _Δp1);

//go:linkname badlinkname_rtype_In reflect.(*rtype).In
internal static partial ΔType badlinkname_rtype_In(ж<rtype> _Δp0, nint _Δp1);

//go:linkname badlinkname_rtype_IsVariadic reflect.(*rtype).IsVariadic
internal static partial bool badlinkname_rtype_IsVariadic(ж<rtype> _);

//go:linkname badlinkname_rtype_Key reflect.(*rtype).Key
internal static partial ΔType badlinkname_rtype_Key(ж<rtype> _);

//go:linkname badlinkname_rtype_Kind reflect.(*rtype).Kind
internal static partial ΔKind badlinkname_rtype_Kind(ж<rtype> _);

//go:linkname badlinkname_rtype_Len reflect.(*rtype).Len
internal static partial nint badlinkname_rtype_Len(ж<rtype> _);

//go:linkname badlinkname_rtype_Method reflect.(*rtype).Method
internal static partial ΔMethod badlinkname_rtype_Method(ж<rtype> _Δp0, nint _Δp1);

//go:linkname badlinkname_rtype_MethodByName reflect.(*rtype).MethodByName
internal static partial (ΔMethod, bool) badlinkname_rtype_MethodByName(ж<rtype> _Δp0, @string _Δp1);

//go:linkname badlinkname_rtype_Name reflect.(*rtype).Name
internal static partial @string badlinkname_rtype_Name(ж<rtype> _);

//go:linkname badlinkname_rtype_NumField reflect.(*rtype).NumField
internal static partial nint badlinkname_rtype_NumField(ж<rtype> _);

//go:linkname badlinkname_rtype_NumIn reflect.(*rtype).NumIn
internal static partial nint badlinkname_rtype_NumIn(ж<rtype> _);

//go:linkname badlinkname_rtype_NumMethod reflect.(*rtype).NumMethod
internal static partial nint badlinkname_rtype_NumMethod(ж<rtype> _);

//go:linkname badlinkname_rtype_NumOut reflect.(*rtype).NumOut
internal static partial nint badlinkname_rtype_NumOut(ж<rtype> _);

//go:linkname badlinkname_rtype_Out reflect.(*rtype).Out
internal static partial ΔType badlinkname_rtype_Out(ж<rtype> _Δp0, nint _Δp1);

//go:linkname badlinkname_rtype_PkgPath reflect.(*rtype).PkgPath
internal static partial @string badlinkname_rtype_PkgPath(ж<rtype> _);

//go:linkname badlinkname_rtype_Size reflect.(*rtype).Size
internal static partial uintptr badlinkname_rtype_Size(ж<rtype> _);

//go:linkname badlinkname_rtype_String reflect.(*rtype).String
internal static partial @string badlinkname_rtype_String(ж<rtype> _);

//go:linkname badlinkname_rtype_ptrTo reflect.(*rtype).ptrTo
internal static partial ж<abi.Type> badlinkname_rtype_ptrTo(ж<rtype> _);

//go:linkname badlinkname_Value_pointer reflect.(*Value).pointer
internal static partial @unsafe.Pointer badlinkname_Value_pointer(ΔValue _);

} // end reflect_package
