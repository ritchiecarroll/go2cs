// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Runtime type representation.
global using nameOff = go.@internal.abi_package.NameOff;
global using typeOff = go.@internal.abi_package.TypeOff;
global using textOff = go.@internal.abi_package.TextOff;
global using _type = go.@internal.abi_package.Type;
global using uncommontype = go.@internal.abi_package.UncommonType;
global using interfacetype = go.@internal.abi_package.ΔInterfaceType;
global using maptype = go.@internal.abi_package.ΔMapType;
global using arraytype = go.@internal.abi_package.ΔArrayType;
global using chantype = go.@internal.abi_package.ChanType;
global using slicetype = go.@internal.abi_package.SliceType;
global using functype = go.@internal.abi_package.ΔFuncType;
global using ptrtype = go.@internal.abi_package.PtrType;
global using name = go.@internal.abi_package.ΔName;
global using structtype = go.@internal.abi_package.ΔStructType;

namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// rtype is a wrapper that allows us to define additional methods.
[GoType] partial struct Δrtype {
    public partial ref ж<@internal.abi_package.Type> Type { get; } // embedding is okay here (unlike reflect) because none of this is public
}

internal static @string @string(this Δrtype t) {
    @string s = t.nameOff(t.Str).Name();
    if ((abi.TFlag)(t.TFlag & abi.TFlagExtraStar) != 0) {
        return s[1..];
    }
    return s;
}

internal static ж<uncommontype> uncommon(this Δrtype t) {
    return t.Type.Uncommon();
}

internal static @string name(this Δrtype t) {
    if ((abi.TFlag)(t.TFlag & abi.TFlagNamed) == 0) {
        return ""u8;
    }
    @string s = t.@string();
    nint i = len(s) - 1;
    nint sqBrackets = 0;
    while (i >= 0 && (s[i] != (rune)'.' || sqBrackets != 0)) {
        switch (s[i]) {
        case (rune)']': {
            sqBrackets++;
            break;
        }
        case (rune)'[': {
            sqBrackets--;
            break;
        }}

        i--;
    }
    return s[(int)(i + 1)..];
}

// pkgpath returns the path of the package where t was defined, if
// available. This is not the same as the reflect package's PkgPath
// method, in that it returns the package path for struct and interface
// types, not just named types.
internal static @string pkgpath(this Δrtype t) {
    {
        var u = t.uncommon(); if (u != nil) {
            return t.nameOff((~u).PkgPath).Name();
        }
    }
    var exprᴛ1 = (abiꓸKind)(t.Kind_ & abi.KindMask);
    if (exprᴛ1 == abi.Struct) {
        var st = t.Type.Reinterpret<abi.Type, structtype>();
        return (~st).PkgPath.Name();
    }
    if (exprᴛ1 == abi.Interface) {
        var it = t.Type.Reinterpret<abi.Type, interfacetype>();
        return (~it).PkgPath.Name();
    }

    return ""u8;
}

// reflectOffs holds type offsets defined at run time by the reflect package.
//
// When a type is defined at run time, its *rtype data lives on the heap.
// There are a wide range of possible addresses the heap may use, that
// may not be representable as a 32-bit offset. Moreover the GC may
// one day start moving heap memory, in which case there is no stable
// offset that can be defined.
//
// To provide stable offsets, we add pin *rtype objects in a global map
// and treat the offset as an identifier. We use negative offsets that
// do not overlap with any compile-time module offsets.
//
// Entries are created by reflect.addReflectOff.

[GoType("dyn")] partial struct reflectOffsᴛ1 {
    internal mutex @lock;
    internal int32 next;
    internal map<int32, @unsafe.Pointer> m;
    internal map<@unsafe.Pointer, int32> minv;
}
internal static ж<reflectOffsᴛ1> ᏑreflectOffs = new StandardBox<reflectOffsᴛ1>(new reflectOffsᴛ1());
internal static ref reflectOffsᴛ1 reflectOffs => ref ᏑreflectOffs.Value;

internal static void reflectOffsLock() {
    @lock(ᏑreflectOffs.of(reflectOffsᴛ1.Ꮡlock));
    if (raceenabled) {
        raceacquire(new @unsafe.Pointer(ᏑreflectOffs.of(reflectOffsᴛ1.Ꮡlock)));
    }
}

internal static void reflectOffsUnlock() {
    if (raceenabled) {
        racerelease(new @unsafe.Pointer(ᏑreflectOffs.of(reflectOffsᴛ1.Ꮡlock)));
    }
    unlock(ᏑreflectOffs.of(reflectOffsᴛ1.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNameOffsetOutOfˢ = "runtime: name offset out of range"u8;
internal static readonly @string runtimeNameOffsetBaseˢ = "runtime: name offset base pointer out of range"u8;

// resolveNameOff should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname resolveNameOff
internal static abiꓸName resolveNameOff(@unsafe.Pointer ptrInModule, nameOff off) {
    if (off == 0) {
        return new abiꓸName();
    }
    var @base = (uintptr)ptrInModule;
    for (var md = Ꮡfirstmoduledata; md != nil; md = md.Value.next) {
        if (@base >= (~md).types && @base < (~md).etypes) {
            var resΔ1 = (~md).types + (uintptr)(int32)off;
            if (resΔ1 > (~md).etypes) {
                println((@string)"runtime: nameOff"u8, ((Δhex)(uint64)(int32)off), (@string)"out of range"u8, ((Δhex)(uint64)(~md).types), (@string)"-"u8, ((Δhex)(uint64)(~md).etypes));
                @throw(runtimeNameOffsetOutOfˢ);
            }
            return new abiꓸName(Bytes: (ж<byte>)(uintptr)((@unsafe.Pointer)resΔ1));
        }
    }
    // No module found. see if it is a run time name.
    reflectOffsLock();
    var (res, found) = reflectOffs.m[(int32)off, ꟷ];
    reflectOffsUnlock();
    if (!found) {
        println((@string)"runtime: nameOff"u8, ((Δhex)(uint64)(int32)off), (@string)"base"u8, ((Δhex)(uint64)@base), (@string)"not in ranges:"u8);
        for (var next = Ꮡfirstmoduledata; next != nil; next = next.Value.next) {
            println((@string)"\ttypes"u8, ((Δhex)(uint64)(~next).types), (@string)"etypes"u8, ((Δhex)(uint64)(~next).etypes));
        }
        @throw(runtimeNameOffsetBaseˢ);
    }
    return new abiꓸName(Bytes: (ж<byte>)(uintptr)(res));
}

internal static abiꓸName nameOff(this Δrtype t, nameOff off) {
    return resolveNameOff(new @unsafe.Pointer(t.Type), off);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeTypeOffsetBaseˢ = "runtime: type offset base pointer out of range"u8;
internal static readonly @string runtimeTypeOffsetOutOfˢ = "runtime: type offset out of range"u8;

// resolveTypeOff should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname resolveTypeOff
internal static ж<_type> resolveTypeOff(@unsafe.Pointer ptrInModule, typeOff off) {
    if (off == 0 || off == -1) {
        // -1 is the sentinel value for unreachable code.
        // See cmd/link/internal/ld/data.go:relocsym.
        return default!;
    }
    var @base = (uintptr)ptrInModule;
    ж<moduledata> md = default!;
    for (var next = Ꮡfirstmoduledata; next != nil; next = next.Value.next) {
        if (@base >= (~next).types && @base < (~next).etypes) {
            md = next;
            break;
        }
    }
    if (md == nil) {
        reflectOffsLock();
        @unsafe.Pointer resΔ1 = reflectOffs.m[(int32)off];
        reflectOffsUnlock();
        if (resΔ1 == nil) {
            println((@string)"runtime: typeOff"u8, ((Δhex)(uint64)(int32)off), (@string)"base"u8, ((Δhex)(uint64)@base), (@string)"not in ranges:"u8);
            for (var next = Ꮡfirstmoduledata; next != nil; next = next.Value.next) {
                println((@string)"\ttypes"u8, ((Δhex)(uint64)(~next).types), (@string)"etypes"u8, ((Δhex)(uint64)(~next).etypes));
            }
            @throw(runtimeTypeOffsetBaseˢ);
        }
        return (ж<_type>)(uintptr)(resΔ1);
    }
    {
        var t = (~md).typemap[off]; if (t != nil) {
            return t;
        }
    }
    var res = (~md).types + (uintptr)(int32)off;
    if (res > (~md).etypes) {
        println((@string)"runtime: typeOff"u8, ((Δhex)(uint64)(int32)off), (@string)"out of range"u8, ((Δhex)(uint64)(~md).types), (@string)"-"u8, ((Δhex)(uint64)(~md).etypes));
        @throw(runtimeTypeOffsetOutOfˢ);
    }
    return (ж<_type>)(uintptr)((@unsafe.Pointer)res);
}

internal static ж<_type> typeOff(this Δrtype t, typeOff off) {
    return resolveTypeOff(new @unsafe.Pointer(t.Type), off);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeTextOffsetBaseˢ = "runtime: text offset base pointer out of range"u8;

internal static @unsafe.Pointer textOff(this Δrtype t, textOff off) {
    if (off == -1) {
        // -1 is the sentinel value for unreachable code.
        // See cmd/link/internal/ld/data.go:relocsym.
        return (@unsafe.Pointer)abi.FuncPCABIInternal(unreachableMethod);
    }
    var @base = (uintptr)t.Type;
    ж<moduledata> md = default!;
    for (var next = Ꮡfirstmoduledata; next != nil; next = next.Value.next) {
        if (@base >= (~next).types && @base < (~next).etypes) {
            md = next;
            break;
        }
    }
    if (md == nil) {
        reflectOffsLock();
        @unsafe.Pointer resΔ1 = reflectOffs.m[(int32)off];
        reflectOffsUnlock();
        if (resΔ1 == nil) {
            println((@string)"runtime: textOff"u8, ((Δhex)(uint64)(int32)off), (@string)"base"u8, ((Δhex)(uint64)@base), (@string)"not in ranges:"u8);
            for (var next = Ꮡfirstmoduledata; next != nil; next = next.Value.next) {
                println((@string)"\ttypes"u8, ((Δhex)(uint64)(~next).types), (@string)"etypes"u8, ((Δhex)(uint64)(~next).etypes));
            }
            @throw(runtimeTextOffsetBaseˢ);
        }
        return resΔ1;
    }
    var res = md.textAddr((uint32)(int32)off);
    return (@unsafe.Pointer)res;
}

internal static @string pkgPath(abiꓸName n) {
    if (n.Bytes == nil || (byte)(n.Data(0).Value & ((byte)(1 << (int)(2)))) == 0) {
        return ""u8;
    }
    var (i, l) = n.ReadVarint(1);
    nint off = 1 + i + l;
    if ((byte)(n.Data(0).Value & ((byte)(1 << (int)(1)))) != 0) {
        var (i2, l2) = n.ReadVarint(off);
        off += i2 + l2;
    }
    ref var nameOff = ref heap(new nameOff(), out var ᏑnameOff);
    copy((~(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(ᏑnameOff)))[..], (~array<byte>.AliasPointer(n.Data(off), 4))[..]);
    var pkgPathName = resolveNameOff(new @unsafe.Pointer(n.Bytes), nameOff);
    return pkgPathName.Name();
}

// typelinksinit scans the types from extra modules and builds the
// moduledata typemap used to de-duplicate type pointers.
internal static void typelinksinit() {
    if (firstmoduledata.next == nil) {
        return;
    }
    var typehash = new map<uint32, slice<ж<_type>>>(len(firstmoduledata.typelinks));
    var modules = activeModules();
    var prev = modules[0];
    foreach (var (_, md) in modules[1..]) {
        // Collect types from the previous module into typehash.
collect:
        foreach (var (_, tl) in (~prev).typelinks) {
            ж<_type> t = default!;
            if ((~prev).typemap == default!){
                t = (ж<_type>)(uintptr)((@unsafe.Pointer)((~prev).types + (uintptr)tl));
            } else {
                t = (~prev).typemap[((typeOff)tl)];
            }
            // Add to typehash if not seen before.
            var tlist = typehash[(~t).Hash];
            foreach (var (_, tcur) in tlist) {
                if (tcur == t) {
                    goto continue_collect;
                }
            }
            typehash[(~t).Hash] = append(tlist, t);
continue_collect:;
        }
break_collect:;
        if ((~md).typemap == default!) {
            // If any of this module's typelinks match a type from a
            // prior module, prefer that prior type by adding the offset
            // to this module's typemap.
            var tm = new map<typeOff, ж<_type>>(len((~md).typelinks));
            pinnedTypemaps = append(pinnedTypemaps, tm);
            md.Value.typemap = tm;
            foreach (var (_, tl) in (~md).typelinks) {
                var t = (ж<_type>)(uintptr)((@unsafe.Pointer)((~md).types + (uintptr)tl));
                foreach (var (_, candidate) in typehash[(~t).Hash]) {
                    var seen = new map<_typePair, EmptyStruct>{};
                    if (typesEqual(t, candidate, seen)) {
                        t = candidate;
                        break;
                    }
                }
                md.Value.typemap[((typeOff)tl)] = t;
            }
        }
        prev = md;
    }
}

[GoType] partial struct _typePair {
    internal ж<_type> t1;
    internal ж<_type> t2;
}

internal static Δrtype toRType(ж<abi.Type> Ꮡt) {
    return new Δrtype(Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeImpossibleTypeˢ = "runtime: impossible type kind"u8;

// typesEqual reports whether two types are equal.
//
// Everywhere in the runtime and reflect packages, it is assumed that
// there is exactly one *_type per Go type, so that pointer equality
// can be used to test if types are equal. There is one place that
// breaks this assumption: buildmode=shared. In this case a type can
// appear as two different pieces of memory. This is hidden from the
// runtime and reflect package by the per-module typemap built in
// typelinksinit. It uses typesEqual to map types from later modules
// back into earlier ones.
//
// Only typelinksinit needs this function.
internal static bool typesEqual(ж<_type> Ꮡt, ж<_type> Ꮡv, map<_typePair, EmptyStruct> seen) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var v = ref Ꮡv.DerefOrNull();

    var tp = new _typePair(Ꮡt, Ꮡv);
    {
        var (_, ok) = seen[tp, ꟷ]; if (ok) {
            return true;
        }
    }
    // mark these types as seen, and thus equivalent which prevents an infinite loop if
    // the two types are identical, but recursively defined and loaded from
    // different modules
    seen[tp] = new EmptyStruct();
    if (Ꮡt == Ꮡv) {
        return true;
    }
    var kind = (abiꓸKind)(t.Kind_ & abi.KindMask);
    if (kind != (abiꓸKind)(v.Kind_ & abi.KindMask)) {
        return false;
    }
    var (rt, rv) = (toRType(Ꮡt), toRType(Ꮡv));
    if (rt.@string() != rv.@string()) {
        return false;
    }
    var ut = Ꮡt.Uncommon();
    var uv = Ꮡv.Uncommon();
    if (ut != nil || uv != nil) {
        if (ut == nil || uv == nil) {
            return false;
        }
        @string pkgpatht = rt.nameOff((~ut).PkgPath).Name();
        @string pkgpathv = rv.nameOff((~uv).PkgPath).Name();
        if (pkgpatht != pkgpathv) {
            return false;
        }
    }
    if (abi.Bool <= kind && kind <= abi.Complex128) {
        return true;
    }
    var exprᴛ1 = kind;
    if (exprᴛ1 == abi.ΔString || exprᴛ1 == abi.UnsafePointer) {
        return true;
    }
    if (exprᴛ1 == abi.Array) {
        var at = Ꮡt.Reinterpret<_type, arraytype>();
        var av = Ꮡv.Reinterpret<_type, arraytype>();
        return typesEqual((~at).Elem, (~av).Elem, seen) && (~at).Len == (~av).Len;
    }
    if (exprᴛ1 == abi.Chan) {
        var ct = Ꮡt.Reinterpret<_type, chantype>();
        var cv = Ꮡv.Reinterpret<_type, chantype>();
        return (~ct).Dir == (~cv).Dir && typesEqual((~ct).Elem, (~cv).Elem, seen);
    }
    if (exprᴛ1 == abi.Func) {
        var ft = Ꮡt.FuncType();
        var fv = Ꮡv.FuncType();
        if ((~ft).OutCount != (~fv).OutCount || (~ft).InCount != (~fv).InCount) {
            return false;
        }
        var (tin, vin) = (ft.InSlice(), fv.InSlice());
        for (nint i = 0; i < len(tin); i++) {
            if (!typesEqual(tin[i], vin[i], seen)) {
                return false;
            }
        }
        var (tout, vout) = (ft.OutSlice(), fv.OutSlice());
        for (nint i = 0; i < len(tout); i++) {
            if (!typesEqual(tout[i], vout[i], seen)) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ1 == abi.Interface) {
        var it = Ꮡt.Reinterpret<_type, interfacetype>();
        var iv = Ꮡv.Reinterpret<_type, interfacetype>();
        if ((~it).PkgPath.Name() != (~iv).PkgPath.Name()) {
            return false;
        }
        if (len((~it).Methods) != len((~iv).Methods)) {
            return false;
        }
        foreach (var (i, _) in (~it).Methods) {
            var tm = Ꮡ((~it).Methods, i);
            var vm = Ꮡ((~iv).Methods, i);
            // Note the mhdr array can be relocated from
            // another module. See #17724.
            var tname = resolveNameOff(new @unsafe.Pointer(tm), (~tm).Name);
            var vname = resolveNameOff(new @unsafe.Pointer(vm), (~vm).Name);
            if (tname.Name() != vname.Name()) {
                return false;
            }
            if (pkgPath(tname) != pkgPath(vname)) {
                return false;
            }
            var tityp = resolveTypeOff(new @unsafe.Pointer(tm), (~tm).Typ);
            var vityp = resolveTypeOff(new @unsafe.Pointer(vm), (~vm).Typ);
            if (!typesEqual(tityp, vityp, seen)) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ1 == abi.Map) {
        var mt = Ꮡt.Reinterpret<_type, maptype>();
        var mv = Ꮡv.Reinterpret<_type, maptype>();
        return typesEqual((~mt).Key, (~mv).Key, seen) && typesEqual((~mt).Elem, (~mv).Elem, seen);
    }
    if (exprᴛ1 == abi.Pointer) {
        var pt = Ꮡt.Reinterpret<_type, ptrtype>();
        var pv = Ꮡv.Reinterpret<_type, ptrtype>();
        return typesEqual((~pt).Elem, (~pv).Elem, seen);
    }
    if (exprᴛ1 == abi.Slice) {
        var st = Ꮡt.Reinterpret<_type, slicetype>();
        var sv = Ꮡv.Reinterpret<_type, slicetype>();
        return typesEqual((~st).Elem, (~sv).Elem, seen);
    }
    if (exprᴛ1 == abi.Struct) {
        var st = Ꮡt.Reinterpret<_type, structtype>();
        var sv = Ꮡv.Reinterpret<_type, structtype>();
        if (len((~st).Fields) != len((~sv).Fields)) {
            return false;
        }
        if ((~st).PkgPath.Name() != (~sv).PkgPath.Name()) {
            return false;
        }
        foreach (var (i, _) in (~st).Fields) {
            var tf = Ꮡ((~st).Fields, i);
            var vf = Ꮡ((~sv).Fields, i);
            if ((~tf).Name.Name() != (~vf).Name.Name()) {
                return false;
            }
            if (!typesEqual((~tf).Typ, (~vf).Typ, seen)) {
                return false;
            }
            if ((~tf).Name.Tag() != (~vf).Name.Tag()) {
                return false;
            }
            if ((~tf).Offset != (~vf).Offset) {
                return false;
            }
            if ((~tf).Name.IsEmbedded() != (~vf).Name.IsEmbedded()) {
                return false;
            }
        }
        return true;
    }
    { /* default: */
        println((@string)"runtime: impossible type kind"u8, kind);
        @throw(runtimeImpossibleTypeˢ);
        return false;
    }

}

} // end runtime_package
