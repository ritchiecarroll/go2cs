// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using token = global::go.go.token_package;
using types = global::go.go.types_package;
using godebug = global::go.@internal.godebug_package;
using pkgbits = global::go.@internal.pkgbits_package;
using slices = slices_package;
using strings = strings_package;
using constant = global::go.go.constant_package;
using global::go.@internal;
using global::go.go;

partial class gcimporter_package {

// A pkgReader holds the shared state for reading a unified IR package
// description.
partial struct pkgReader {
    public partial ref global::go.@internal.pkgbits_package.PkgDecoder PkgDecoder { get; }
    internal fakeFileSet fake;
    internal ж<types.Context> ctxt;
    internal map<@string, ж<types.Package>> imports; // previously imported packages, indexed by path
    // lazily initialized arrays corresponding to the unified IR
    // PosBase, Pkg, and Type sections, respectively.
    internal slice<@string> posBases; // position bases (i.e., file names)
    internal slice<ж<types.Package>> pkgs;
    internal slice<typesꓸType> typs;
    // laterFns holds functions that need to be invoked at the end of
    // import reading.
    internal slice<Action> laterFns;
    // ifaces holds a list of constructed Interfaces, which need to have
    // Complete called after importing is done.
    internal slice<ж<types.Interface>> ifaces;
}

// later adds a function to be invoked at the end of import reading.
internal static void later(this ref pkgReader pr, Action fn) {
    pr.laterFns = append(pr.laterFns, fn);
}

// readUnifiedPackage reads a package description from the given
// unified IR export data decoder.
internal static ж<types.Package> readUnifiedPackage(ж<token.FileSet> Ꮡfset, ж<types.Context> Ꮡctxt, map<@string, ж<types.Package>> imports, pkgbits.PkgDecoder input) {
    GoFrame ᒐ = default;
    try {
        input = input.ΔClone();

        ref var pr = ref heap<pkgReader>(out var Ꮡpr);
        pr = new pkgReader(
            PkgDecoder: input.ΔClone(),
            fake: new fakeFileSet(
                fset: Ꮡfset,
                files: new map<@string, ж<fileInfo>>()
            ),
            ctxt: Ꮡctxt,
            imports: imports,
            posBases: new slice<@string>(input.NumElems(pkgbits.RelocPosBase)),
            pkgs: new slice<ж<types.Package>>(input.NumElems(pkgbits.RelocPkg)),
            typs: new slice<typesꓸType>(input.NumElems(pkgbits.RelocType))
        );
        defer(Ꮡpr.of(pkgReader.Ꮡfake).setLines, ref ᒐ);
        var r = Ꮡpr.newReader(pkgbits.RelocMeta, pkgbits.PublicRootIdx, pkgbits.SyncPublic);
        var pkg = r.pkg();
        if (r.of(reader.ᏑDecoder).Version().Has(pkgbits.HasInit)) {
            r.of(reader.ᏑDecoder).Bool();
        }
        for ((nint i, nint n) = (0, r.of(reader.ᏑDecoder).Len()); i < n; i++) {
            // As if r.obj(), but avoiding the Scope.Lookup call,
            // to avoid eager loading of imports.
            r.of(reader.ᏑDecoder).Sync(pkgbits.SyncObject);
            if (r.of(reader.ᏑDecoder).Version().Has(pkgbits.DerivedFuncInstance)) {
                assert(!r.of(reader.ᏑDecoder).Bool());
            }
            (~r).p.objIdx(r.of(reader.ᏑDecoder).Reloc(pkgbits.RelocObj));
            assert(r.of(reader.ᏑDecoder).Len() == 0);
        }
        r.of(reader.ᏑDecoder).Sync(pkgbits.SyncEOF);
        foreach (var (_, fn) in pr.laterFns) {
            fn();
        }
        foreach (var (_, iface) in pr.ifaces) {
            iface.Complete();
        }
        // Imports() of pkg are all of the transitive packages that were loaded.
        slice<ж<types.Package>> imps = default!;
        foreach (var (_, imp) in pr.pkgs) {
            if (imp != nil && imp != pkg) {
                imps = append(imps, imp);
            }
        }
        slices.SortFunc(imps, (ж<types.Package> a, ж<types.Package> b) => strings.Compare(a.Path(), b.Path()));
        pkg.SetImports(imps);
        pkg.MarkComplete();
        return pkg;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// A reader holds the state for reading a single unified IR element
// within a package.
partial struct reader {
    public partial ref global::go.@internal.pkgbits_package.Decoder Decoder { get; }
    internal ж<pkgReader> p;
    internal ж<readerDict> dict;
}

// A readerDict holds the state for type parameters that parameterize
// the current unified IR element.
partial struct readerDict {
    // bounds is a slice of typeInfos corresponding to the underlying
    // bounds of the element's type parameters.
    internal slice<typeInfo> bounds;
    // tparams is a slice of the constructed TypeParams for the element.
    internal slice<ж<types.TypeParam>> tparams;
    // derived is a slice of types derived from tparams, which may be
    // instantiated while reading the current element.
    internal slice<derivedInfo> derived;
    internal slice<typesꓸType> derivedTypes; // lazily instantiated from derived
}

internal static ж<reader> newReader(this ж<pkgReader> Ꮡpr, pkgbits.RelocKind k, pkgbits.Index idx, pkgbits.SyncMarker marker) {
    return Ꮡ(new reader(
        Decoder: Ꮡpr.of(pkgReader.ᏑPkgDecoder).NewDecoder(k, idx, marker),
        p: Ꮡpr
    ));
}

internal static ж<reader> tempReader(this ж<pkgReader> Ꮡpr, pkgbits.RelocKind k, pkgbits.Index idx, pkgbits.SyncMarker marker) {
    return Ꮡ(new reader(
        Decoder: Ꮡpr.of(pkgReader.ᏑPkgDecoder).TempDecoder(k, idx, marker),
        p: Ꮡpr
    ));
}

internal static void retireReader(this ref pkgReader pr, ж<reader> Ꮡr) {
    pr.PkgDecoder.RetireDecoder(Ꮡr.of(reader.ᏑDecoder));
}

// @@@ Positions
internal static tokenꓸPos pos(this ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(reader.ᏑDecoder).Sync(pkgbits.SyncPos);
    if (!Ꮡr.of(reader.ᏑDecoder).Bool()) {
        return token.NoPos;
    }
    // TODO(mdempsky): Delta encoding.
    @string posBase = Ꮡr.posBase();
    nuint line = Ꮡr.of(reader.ᏑDecoder).Uint();
    nuint col = Ꮡr.of(reader.ᏑDecoder).Uint();
    return r.p.of(pkgReader.Ꮡfake).pos(posBase, (nint)line, (nint)col);
}

internal static @string posBase(this ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    return r.p.posBaseIdx(Ꮡr.of(reader.ᏑDecoder).Reloc(pkgbits.RelocPosBase));
}

internal static @string posBaseIdx(this ж<pkgReader> Ꮡpr, pkgbits.Index idx) {
    ref var pr = ref Ꮡpr.DerefOrNull();

    {
        @string bΔ1 = pr.posBases[idx]; if (bΔ1 != ""u8) {
            return bΔ1;
        }
    }
    @string filename = default!;
    {
        var r = Ꮡpr.tempReader(pkgbits.RelocPosBase, idx, pkgbits.SyncPosBase);
        // Within types2, position bases have a lot more details (e.g.,
        // keeping track of where //line directives appeared exactly).
        //
        // For go/types, we just track the file name.
        filename = r.of(reader.ᏑDecoder).String();
        if (r.of(reader.ᏑDecoder).Bool()){
        } else {
            // file base
            // Was: "b = token.NewTrimmedFileBase(filename, true)"
            // line base
            tokenꓸPos pos = r.pos();
            nuint line = r.of(reader.ᏑDecoder).Uint();
            nuint col = r.of(reader.ᏑDecoder).Uint();
            // Was: "b = token.NewLineBase(pos, filename, true, line, col)"
            _ = pos;
            _ = line;
            _ = col;
        }
        pr.retireReader(r);
    }
    @string b = filename;
    pr.posBases[idx] = b;
    return b;
}

// @@@ Packages
internal static ж<types.Package> pkg(this ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(reader.ᏑDecoder).Sync(pkgbits.SyncPkg);
    return r.p.pkgIdx(Ꮡr.of(reader.ᏑDecoder).Reloc(pkgbits.RelocPkg));
}

internal static ж<types.Package> pkgIdx(this ж<pkgReader> Ꮡpr, pkgbits.Index idx) {
    ref var pr = ref Ꮡpr.DerefOrNull();

    // TODO(mdempsky): Consider using some non-nil pointer to indicate
    // the universe scope, so we don't need to keep re-reading it.
    {
        var pkgΔ1 = pr.pkgs[idx]; if (pkgΔ1 != nil) {
            return pkgΔ1;
        }
    }
    var pkg = Ꮡpr.newReader(pkgbits.RelocPkg, idx, pkgbits.SyncPkgDef).doPkg();
    pr.pkgs[idx] = pkg;
    return pkg;
}

internal static ж<types.Package> doPkg(this ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    @string path = Ꮡr.of(reader.ᏑDecoder).String();
    var exprᴛ1 = path;
    if (exprᴛ1 == ""u8) {
        path = r.p.of(pkgReader.ᏑPkgDecoder).PkgPath();
    }
    else if (exprᴛ1 == "builtin"u8) {
        return default!; // universe
    }
    else if (exprᴛ1 == "unsafe"u8) {
        return types.Unsafe;
    }

    {
        var pkgΔ1 = (~r.p).imports[path]; if (pkgΔ1 != nil) {
            return pkgΔ1;
        }
    }
    @string name = Ꮡr.of(reader.ᏑDecoder).String();
    var pkg = types.NewPackage(path, name);
    r.p.Value.imports[path] = pkg;
    return pkg;
}

// @@@ Types
internal static typesꓸType typ(this ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    return r.p.typIdx(Ꮡr.typInfo(), r.dict);
}

internal static typeInfo typInfo(this ж<reader> Ꮡr) {
    Ꮡr.of(reader.ᏑDecoder).Sync(pkgbits.SyncType);
    if (Ꮡr.of(reader.ᏑDecoder).Bool()) {
        return new typeInfo(idx: ((pkgbits.Index)(int32)Ꮡr.of(reader.ᏑDecoder).Len()), derived: true);
    }
    return new typeInfo(idx: Ꮡr.of(reader.ᏑDecoder).Reloc(pkgbits.RelocType), derived: false);
}

internal static typesꓸType typIdx(this ж<pkgReader> Ꮡpr, typeInfo info, ж<readerDict> Ꮡdict) {
    ref var pr = ref Ꮡpr.DerefOrNull();
    ref var dict = ref Ꮡdict.DerefOrNull();

    var idx = info.idx;
    ж<typesꓸType> where = default!;
    if (info.derived){
        where = Ꮡ(dict.derivedTypes, idx);
        idx = dict.derived[idx].idx;
    } else {
        where = Ꮡ(pr.typs, idx);
    }
    {
        var typΔ1 = where.ValueSlot; if (typΔ1 != default!) {
            return typΔ1;
        }
    }
    typesꓸType typ = default!;
    {
        var r = Ꮡpr.tempReader(pkgbits.RelocType, idx, pkgbits.SyncTypeIdx);
        r.Value.dict = Ꮡdict;
        typ = r.doTyp();
        assert(typ != default!);
        pr.retireReader(r);
    }
    // See comment in pkgReader.typIdx explaining how this happens.
    {
        var prev = where.ValueSlot; if (prev != default!) {
            return prev;
        }
    }
    where.ValueSlot = typ;
    return typ;
}

internal static typesꓸType /*res*/ doTyp(this ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    {
        pkgbits.CodeType tag = ((pkgbits.CodeType)Ꮡr.of(reader.ᏑDecoder).Code(pkgbits.SyncType));
        var exprᴛ1 = tag;
        if (exprᴛ1 == pkgbits.TypeBasic) {
            return new types.BasicжΔType(types.Typ[Ꮡr.of(reader.ᏑDecoder).Len()]);
        }
        if (exprᴛ1 == pkgbits.TypeNamed) {
            var (obj, targs) = Ꮡr.obj();
            var name = obj._<ж<types.TypeName>>();
            if (len(targs) != 0) {
                var (t, _) = types.Instantiate((~r.p).ctxt, name.Type(), targs, false);
                return t;
            }
            return name.Type();
        }
        if (exprᴛ1 == pkgbits.TypeTypeParam) {
            return new types.TypeParamжΔType((~r.dict).tparams[Ꮡr.of(reader.ᏑDecoder).Len()]);
        }
        if (exprᴛ1 == pkgbits.TypeArray) {
            var lenΔ2 = (int64)Ꮡr.of(reader.ᏑDecoder).Uint64();
            return new types.ArrayжΔType(types.NewArray(Ꮡr.typ(), lenΔ2));
        }
        if (exprᴛ1 == pkgbits.TypeChan) {
            types.ChanDir dir = ((types.ChanDir)Ꮡr.of(reader.ᏑDecoder).Len());
            return new types.ChanжΔType(types.NewChan(dir, Ꮡr.typ()));
        }
        if (exprᴛ1 == pkgbits.TypeMap) {
            return new types.MapжΔType(types.NewMap(Ꮡr.typ(), Ꮡr.typ()));
        }
        if (exprᴛ1 == pkgbits.TypePointer) {
            return new types.PointerжΔType(types.NewPointer(Ꮡr.typ()));
        }
        if (exprᴛ1 == pkgbits.TypeSignature) {
            return new types.ΔSignatureжΔType(Ꮡr.signature(nil, default!, default!));
        }
        if (exprᴛ1 == pkgbits.TypeSlice) {
            return new types.SliceжΔType(types.NewSlice(Ꮡr.typ()));
        }
        if (exprᴛ1 == pkgbits.TypeStruct) {
            return new types.StructжΔType(Ꮡr.structType());
        }
        if (exprᴛ1 == pkgbits.TypeInterface) {
            return new types.InterfaceжΔType(Ꮡr.interfaceType());
        }
        if (exprᴛ1 == pkgbits.TypeUnion) {
            return new types.UnionжΔType(Ꮡr.unionType());
        }
        { /* default: */
            errorf("unhandled type tag: %v"u8, tag);
            throw panic("unreachable");
        }
    }

}

internal static ж<types.Struct> structType(this ж<reader> Ꮡr) {
    var fields = new slice<ж<types.Var>>(Ꮡr.of(reader.ᏑDecoder).Len());
    slice<@string> tags = default!;
    foreach (var (i, _) in fields) {
        tokenꓸPos pos = Ꮡr.pos();
        var (pkg, name) = Ꮡr.selector();
        var ftyp = Ꮡr.typ();
        @string tag = Ꮡr.of(reader.ᏑDecoder).String();
        var embedded = Ꮡr.of(reader.ᏑDecoder).Bool();
        fields[i] = types.NewField(pos, pkg, name, ftyp, embedded);
        if (tag != ""u8) {
            while (len(tags) < i) {
                tags = append(tags, ""u8);
            }
            tags = append(tags, tag);
        }
    }
    return types.NewStruct(fields, tags);
}

internal static ж<types.Union> unionType(this ж<reader> Ꮡr) {
    var terms = new slice<ж<typesꓸTerm>>(Ꮡr.of(reader.ᏑDecoder).Len());
    foreach (var (i, _) in terms) {
        terms[i] = types.NewTerm(Ꮡr.of(reader.ᏑDecoder).Bool(), Ꮡr.typ());
    }
    return types.NewUnion(terms);
}

internal static ж<types.Interface> interfaceType(this ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    var methods = new slice<ж<types.Func>>(Ꮡr.of(reader.ᏑDecoder).Len());
    var embeddeds = new slice<typesꓸType>(Ꮡr.of(reader.ᏑDecoder).Len());
    var @implicit = len(methods) == 0 && len(embeddeds) == 1 && Ꮡr.of(reader.ᏑDecoder).Bool();
    foreach (var (i, _) in methods) {
        tokenꓸPos pos = Ꮡr.pos();
        var (pkg, name) = Ꮡr.selector();
        var mtyp = Ꮡr.signature(nil, default!, default!);
        methods[i] = types.NewFunc(pos, pkg, name, mtyp);
    }
    foreach (var (i, _) in embeddeds) {
        embeddeds[i] = Ꮡr.typ();
    }
    var iface = types.NewInterfaceType(methods, embeddeds);
    if (@implicit) {
        iface.MarkImplicit();
    }
    // We need to call iface.Complete(), but if there are any embedded
    // defined types, then we may not have set their underlying
    // interface type yet. So we need to defer calling Complete until
    // after we've called SetUnderlying everywhere.
    //
    // TODO(mdempsky): After CL 424876 lands, it should be safe to call
    // iface.Complete() immediately.
    r.p.Value.ifaces = append((~r.p).ifaces, iface);
    return iface;
}

internal static ж<typesꓸSignature> signature(this ж<reader> Ꮡr, ж<types.Var> Ꮡrecv, slice<ж<types.TypeParam>> rtparams, slice<ж<types.TypeParam>> tparams) {
    Ꮡr.of(reader.ᏑDecoder).Sync(pkgbits.SyncSignature);
    var @params = Ꮡr.@params();
    var results = Ꮡr.@params();
    var variadic = Ꮡr.of(reader.ᏑDecoder).Bool();
    return types.NewSignatureType(Ꮡrecv, rtparams, tparams, @params, results, variadic);
}

internal static ж<types.Tuple> @params(this ж<reader> Ꮡr) {
    Ꮡr.of(reader.ᏑDecoder).Sync(pkgbits.SyncParams);
    var @params = new slice<ж<types.Var>>(Ꮡr.of(reader.ᏑDecoder).Len());
    foreach (var (i, _) in @params) {
        @params[i] = Ꮡr.param();
    }
    return types.NewTuple(@params.ꓸꓸꓸ);
}

internal static ж<types.Var> param(this ж<reader> Ꮡr) {
    Ꮡr.of(reader.ᏑDecoder).Sync(pkgbits.SyncParam);
    tokenꓸPos pos = Ꮡr.pos();
    var (pkg, name) = Ꮡr.localIdent();
    var typ = Ꮡr.typ();
    return types.NewParam(pos, pkg, name, typ);
}

// @@@ Objects
internal static (types.Object, slice<typesꓸType>) obj(this ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(reader.ᏑDecoder).Sync(pkgbits.SyncObject);
    if (Ꮡr.of(reader.ᏑDecoder).Version().Has(pkgbits.DerivedFuncInstance)) {
        assert(!Ꮡr.of(reader.ᏑDecoder).Bool());
    }
    var (pkg, name) = r.p.objIdx(Ꮡr.of(reader.ᏑDecoder).Reloc(pkgbits.RelocObj));
    var obj = pkgScope(pkg).Lookup(name);
    var targs = new slice<typesꓸType>(Ꮡr.of(reader.ᏑDecoder).Len());
    foreach (var (i, _) in targs) {
        targs[i] = Ꮡr.typ();
    }
    return (obj, targs);
}

internal static (ж<types.Package>, @string) objIdx(this ж<pkgReader> Ꮡpr, pkgbits.Index idx) {
    ref var pr = ref Ꮡpr.DerefOrNull();

    ж<types.Package> objPkg = default!;
    @string objName = default!;
    pkgbits.CodeObj tag = default!;
    {
        var rname = Ꮡpr.tempReader(pkgbits.RelocName, idx, pkgbits.SyncObject1);
        (objPkg, objName) = rname.qualifiedIdent();
        assert(objName != ""u8);
        tag = ((pkgbits.CodeObj)rname.of(reader.ᏑDecoder).Code(pkgbits.SyncCodeObj));
        pr.retireReader(rname);
    }
    if (tag == pkgbits.ObjStub) {
        assert(objPkg == nil || objPkg == types.Unsafe);
        return (objPkg, objName);
    }
    // Ignore local types promoted to global scope (#55110).
    {
        var (_, suffix) = splitVargenSuffix(objName); if (suffix != ""u8) {
            return (objPkg, objName);
        }
    }
    if (objPkg.Scope().Lookup(objName) == default!) {
        var dict = Ꮡpr.objDictIdx(idx);
        var r = Ꮡpr.newReader(pkgbits.RelocObj, idx, pkgbits.SyncObject1);
        r.Value.dict = dict;
        var objPkgʗ1 = objPkg;
        void declare(types.Object obj) {
            objPkgʗ1.Scope().Insert(obj);
        }
        var exprᴛ1 = tag;
        if (exprᴛ1 == pkgbits.ObjAlias) {
            tokenꓸPos pos = r.pos();
            slice<ж<types.TypeParam>> tparams = default!;
            if (r.of(reader.ᏑDecoder).Version().Has(pkgbits.AliasTypeParamNames)) {
                tparams = r.typeParamNames();
            }
            var typ = r.typ();
            declare(new types.TypeNameжObject(newAliasTypeName(pos, objPkg, objName, typ, tparams)));
        }
        else if (exprᴛ1 == pkgbits.ObjConst) {
            tokenꓸPos pos = r.pos();
            var typ = r.typ();
            var val = r.of(reader.ᏑDecoder).Value();
            declare(new types.ConstжObject(types.NewConst(pos, objPkg, objName, typ, val)));
        }
        else if (exprᴛ1 == pkgbits.ObjFunc) {
            tokenꓸPos pos = r.pos();
            var tparams = r.typeParamNames();
            var sig = r.signature(nil, default!, tparams);
            declare(new types.FuncжObject(types.NewFunc(pos, objPkg, objName, sig)));
        }
        else if (exprᴛ1 == pkgbits.ObjType) {
            tokenꓸPos pos = r.pos();
            var obj = types.NewTypeName(pos, objPkg, objName, default!);
            var named = types.NewNamed(obj, default!, default!);
            declare(new types.TypeNameжObject(obj));
            named.SetTypeParams(r.typeParamNames());
            var underlying = r.typ().Underlying();
            {
                var (iface, ok) = underlying._<ж<types.Interface>>(ᐧ); if (ok && iface.NumExplicitMethods() != 0) {
                    // If the underlying type is an interface, we need to
                    // duplicate its methods so we can replace the receiver
                    // parameter's type (#49906).
                    var methods = new slice<ж<types.Func>>(iface.NumExplicitMethods());
                    foreach (var (i, _) in methods) {
                        var fn = iface.ExplicitMethod(i);
                        var sig = fn.Type()._<ж<typesꓸSignature>>();
                        var recv = types.NewVar(fn.Pos(), fn.Pkg(), ""u8, new types.NamedжΔType(named));
                        methods[i] = types.NewFunc(fn.Pos(), fn.Pkg(), fn.Name(), types.NewSignature(recv, sig.Params(), sig.Results(), sig.Variadic()));
                    }
                    var embeds = new slice<typesꓸType>(iface.NumEmbeddeds());
                    foreach (var (i, _) in embeds) {
                        embeds[i] = iface.EmbeddedType(i);
                    }
                    var newIface = types.NewInterfaceType(methods, embeds);
                    r.Value.p.Value.ifaces = append((~(~r).p).ifaces, newIface);
                    underlying = new types.InterfaceжΔType(newIface);
                }
            }
            named.SetUnderlying(underlying);
            for ((nint i, nint n) = (0, r.of(reader.ᏑDecoder).Len()); i < n; i++) {
                named.AddMethod(r.method());
            }
        }
        else if (exprᴛ1 == pkgbits.ObjVar) {
            tokenꓸPos pos = r.pos();
            var typ = r.typ();
            declare(new types.VarжObject(types.NewVar(pos, objPkg, objName, typ)));
        }
        else { /* default: */
            throw panic("weird");
        }

    }
    return (objPkg, objName);
}

internal static ж<readerDict> objDictIdx(this ж<pkgReader> Ꮡpr, pkgbits.Index idx) {
    ref var pr = ref Ꮡpr.DerefOrNull();

    ref var dict = ref heap(new readerDict(), out var Ꮡdict);
    {
        var r = Ꮡpr.tempReader(pkgbits.RelocObjDict, idx, pkgbits.SyncObject1);
        {
            nint implicits = r.of(reader.ᏑDecoder).Len(); if (implicits != 0) {
                errorf("unexpected object with %v implicit type parameter(s)"u8, implicits);
            }
        }
        dict.bounds = new slice<typeInfo>(r.of(reader.ᏑDecoder).Len());
        foreach (var (i, _) in dict.bounds) {
            dict.bounds[i] = r.typInfo();
        }
        dict.derived = new slice<derivedInfo>(r.of(reader.ᏑDecoder).Len());
        dict.derivedTypes = new slice<typesꓸType>(len(dict.derived));
        foreach (var (i, _) in dict.derived) {
            dict.derived[i] = new derivedInfo(idx: r.of(reader.ᏑDecoder).Reloc(pkgbits.RelocType));
            if (r.of(reader.ᏑDecoder).Version().Has(pkgbits.DerivedInfoNeeded)) {
                assert(!r.of(reader.ᏑDecoder).Bool());
            }
        }
        pr.retireReader(r);
    }
    // function references follow, but reader doesn't need those
    return Ꮡdict;
}

internal static slice<ж<types.TypeParam>> typeParamNames(this ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(reader.ᏑDecoder).Sync(pkgbits.SyncTypeParamNames);
    // Note: This code assumes it only processes objects without
    // implement type parameters. This is currently fine, because
    // reader is only used to read in exported declarations, which are
    // always package scoped.
    if (len((~r.dict).bounds) == 0) {
        return default!;
    }
    // Careful: Type parameter lists may have cycles. To allow for this,
    // we construct the type parameter list in two passes: first we
    // create all the TypeNames and TypeParams, then we construct and
    // set the bound type.
    r.dict.Value.tparams = new slice<ж<types.TypeParam>>(len((~r.dict).bounds));
    foreach (var (i, _) in (~r.dict).bounds) {
        tokenꓸPos pos = Ꮡr.pos();
        var (pkg, name) = Ꮡr.localIdent();
        var tname = types.NewTypeName(pos, pkg, name, default!);
        r.dict.Value.tparams[i] = types.NewTypeParam(tname, default!);
    }
    var typs = new slice<typesꓸType>(len((~r.dict).bounds));
    foreach (var (i, bound) in (~r.dict).bounds) {
        typs[i] = r.p.typIdx(bound, r.dict);
    }
    // TODO(mdempsky): This is subtle, elaborate further.
    //
    // We have to save tparams outside of the closure, because
    // typeParamNames() can be called multiple times with the same
    // dictionary instance.
    //
    // Also, this needs to happen later to make sure SetUnderlying has
    // been called.
    //
    // TODO(mdempsky): Is it safe to have a single "later" slice or do
    // we need to have multiple passes? See comments on CL 386002 and
    // go.dev/issue/52104.
    var tparams = r.dict.Value.tparams;
    var tparamsʗ1 = tparams;
    var typsʗ1 = typs;
    r.p.later(() => {
        foreach (var (i, typ) in typsʗ1) {
            tparamsʗ1[i].SetConstraint(typ);
        }
    });
    return (~r.dict).tparams;
}

internal static ж<types.Func> method(this ж<reader> Ꮡr) {
    Ꮡr.of(reader.ᏑDecoder).Sync(pkgbits.SyncMethod);
    tokenꓸPos pos = Ꮡr.pos();
    var (pkg, name) = Ꮡr.selector();
    var rparams = Ꮡr.typeParamNames();
    var sig = Ꮡr.signature(Ꮡr.param(), rparams, default!);
    _ = Ꮡr.pos(); // TODO(mdempsky): Remove; this is a hacker for linker.go.
    return types.NewFunc(pos, pkg, name, sig);
}

internal static (ж<types.Package>, @string) qualifiedIdent(this ж<reader> Ꮡr) {
    return Ꮡr.ident(pkgbits.SyncSym);
}

internal static (ж<types.Package>, @string) localIdent(this ж<reader> Ꮡr) {
    return Ꮡr.ident(pkgbits.SyncLocalIdent);
}

internal static (ж<types.Package>, @string) selector(this ж<reader> Ꮡr) {
    return Ꮡr.ident(pkgbits.SyncSelector);
}

internal static (ж<types.Package>, @string) ident(this ж<reader> Ꮡr, pkgbits.SyncMarker marker) {
    Ꮡr.of(reader.ᏑDecoder).Sync(marker);
    return (Ꮡr.pkg(), Ꮡr.of(reader.ᏑDecoder).String());
}

// pkgScope returns pkg.Scope().
// If pkg is nil, it returns types.Universe instead.
//
// TODO(mdempsky): Remove after x/tools can depend on Go 1.19.
internal static ж<typesꓸScope> pkgScope(ж<types.Package> Ꮡpkg) {
    if (Ꮡpkg != nil) {
        return Ꮡpkg.Scope();
    }
    return types.Universe;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gotypesaliasˢ = "gotypesalias"u8;

// newAliasTypeName returns a new TypeName, with a materialized *types.Alias if supported.
internal static ж<types.TypeName> newAliasTypeName(tokenꓸPos pos, ж<types.Package> Ꮡpkg, @string name, typesꓸType rhs, slice<ж<types.TypeParam>> tparams) {
    // When GODEBUG=gotypesalias=1 or unset, the Type() of the return value is a
    // *types.Alias. Copied from x/tools/internal/aliases.NewAlias.
    var exprᴛ1 = godebug.New(gotypesaliasˢ).Value();
    if (exprᴛ1 == ""u8 || exprᴛ1 == "1"u8) {
        var tname = types.NewTypeName(pos, Ꮡpkg, name, default!);
        var a = types.NewAlias(tname, rhs); // form TypeName -> Alias cycle
        a.SetTypeParams(tparams);
        return tname;
    }

    assert(len(tparams) == 0);
    return types.NewTypeName(pos, Ꮡpkg, name, rhs);
}

} // end gcimporter_package
