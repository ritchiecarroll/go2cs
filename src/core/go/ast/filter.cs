// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using token = global::go.go.token_package;
using slices = slices_package;
using global::go.go;

partial class ast_package {

// ----------------------------------------------------------------------------
// Export filtering

// exportFilter is a special filter function to extract exported nodes.
internal static bool exportFilter(@string name) {
    return IsExported(name);
}

// FileExports trims the AST for a Go source file in place such that
// only exported nodes remain: all top-level identifiers which are not exported
// and their associated information (such as type, initial value, or function
// body) are removed. Non-exported fields and methods of exported types are
// stripped. The [File.Comments] list is not changed.
//
// FileExports reports whether there are exported declarations.
public static bool FileExports(ж<File> Ꮡsrc) {
    return filterFile(ref (Ꮡsrc).DerefOrNull(), new Func<@string, bool>(exportFilter), true);
}

// PackageExports trims the AST for a Go package in place such that
// only exported nodes remain. The pkg.Files list is not changed, so that
// file names and top-level package comments don't get lost.
//
// PackageExports reports whether there are exported declarations;
// it returns false otherwise.
public static bool PackageExports(ж<Package> Ꮡpkg) {
    return filterPackage(ref (Ꮡpkg).DerefOrNull(), new Func<@string, bool>(exportFilter), true);
}

// type ΔFilter is a methodless func type — rendered inline as its base delegate

// ----------------------------------------------------------------------------
// General filtering
internal static slice<ж<Ident>> filterIdentList(slice<ж<Ident>> list, Func<@string, bool> f) {
    nint j = 0;
    foreach (var (_, x) in list) {
        if (f((~x).Name)) {
            list[j] = x;
            j++;
        }
    }
    return list.slice(0, j);
}

// fieldName assumes that x is the type of an anonymous field and
// returns the corresponding field name. If x is not an acceptable
// anonymous field, the result is nil.
internal static ж<Ident> fieldName(Expr x) {
    switch (x.type()) {
    case ж<Ident> t: {
        return t;
    }
    case ж<SelectorExpr> t: {
        {
            var (_, ok) = (~t).X._<ж<Ident>>(ᐧ); if (ok) {
                return (~t).Sel;
            }
        }
        break;
    }
    case ж<StarExpr> t: {
        return fieldName((~t).X);
    }}
    return default!;
}

internal static bool /*removedFields*/ filterFieldList(ж<FieldList> Ꮡfields, Func<@string, bool> filter, bool export) {
    bool removedFields = default!;

    ref var fields = ref Ꮡfields.DerefOrNull();
    if (Ꮡfields == nil) {
        return false;
    }
    var list = fields.List;
    nint j = 0;
    foreach (var (_, f) in list) {
        var keepField = false;
        if (len((~f).Names) == 0){
            // anonymous field
            var name = fieldName((~f).Type);
            keepField = name != nil && filter((~name).Name);
        } else {
            nint n = len((~f).Names);
            f.Value.Names = filterIdentList((~f).Names, filter);
            if (len((~f).Names) < n) {
                removedFields = true;
            }
            keepField = len((~f).Names) > 0;
        }
        if (keepField) {
            if (export) {
                filterType((~f).Type, filter, export);
            }
            list[j] = f;
            j++;
        }
    }
    if (j < len(list)) {
        removedFields = true;
    }
    fields.List = list.slice(0, j);
    return removedFields;
}

internal static void filterCompositeLit(ref CompositeLit lit, Func<@string, bool> filter, bool export) {
    nint n = len(lit.Elts);
    lit.Elts = filterExprList(lit.Elts, filter, export);
    if (len(lit.Elts) < n) {
        lit.Incomplete = true;
    }
}

internal static slice<Expr> filterExprList(slice<Expr> list, Func<@string, bool> filter, bool export) {
    nint j = 0;
    foreach (var (_, exp) in list) {
        switch (exp.type()) {
        case ж<CompositeLit> x: {
            filterCompositeLit(ref (x).DerefOrNull(), filter, export);
            break;
        }
        case ж<KeyValueExpr> x: {
            {
                var (xΔ1, ok) = (~x).Key._<ж<Ident>>(ᐧ); if (ok && !filter((~xΔ1).Name)) {
                    continue;
                }
            }
            {
                var (xΔ2, ok) = (~x).Value._<ж<CompositeLit>>(ᐧ); if (ok) {
                    filterCompositeLit(ref (xΔ2).DerefOrNull(), filter, export);
                }
            }
            break;
        }}
        list[j] = exp;
        j++;
    }
    return list.slice(0, j);
}

internal static bool filterParamList(ж<FieldList> Ꮡfields, Func<@string, bool> filter, bool export) {
    ref var fields = ref Ꮡfields.DerefOrNull();

    if (Ꮡfields == nil) {
        return false;
    }
    bool b = default!;
    foreach (var (_, f) in fields.List) {
        if (filterType((~f).Type, filter, export)) {
            b = true;
        }
    }
    return b;
}

internal static bool filterType(Expr typ, Func<@string, bool> f, bool export) {
    switch (typ.type()) {
    case ж<Ident> t: {
        return f((~t).Name);
    }
    case ж<ParenExpr> t: {
        return filterType((~t).X, f, export);
    }
    case ж<ArrayType> t: {
        return filterType((~t).Elt, f, export);
    }
    case ж<StructType> t: {
        if (filterFieldList((~t).Fields, f, export)) {
            t.Value.Incomplete = true;
        }
        return len((~(~t).Fields).List) > 0;
    }
    case ж<FuncType> t: {
        var b1 = filterParamList((~t).Params, f, export);
        var b2 = filterParamList((~t).Results, f, export);
        return b1 || b2;
    }
    case ж<InterfaceType> t: {
        if (filterFieldList((~t).Methods, f, export)) {
            t.Value.Incomplete = true;
        }
        return len((~(~t).Methods).List) > 0;
    }
    case ж<MapType> t: {
        var b1 = filterType((~t).Key, f, export);
        var b2 = filterType((~t).Value, f, export);
        return b1 || b2;
    }
    case ж<ChanType> t: {
        return filterType((~t).Value, f, export);
    }}
    return false;
}

internal static bool filterSpec(Spec spec, Func<@string, bool> f, bool export) {
    switch (spec.type()) {
    case ж<ValueSpec> s: {
        s.Value.Names = filterIdentList((~s).Names, f);
        s.Value.Values = filterExprList((~s).Values, f, export);
        if (len((~s).Names) > 0) {
            if (export) {
                filterType((~s).Type, f, export);
            }
            return true;
        }
        break;
    }
    case ж<TypeSpec> s: {
        if (f((~(~s).Name).Name)) {
            if (export) {
                filterType((~s).Type, f, export);
            }
            return true;
        }
        if (!export) {
            // For general filtering (not just exports),
            // filter type even if name is not filtered
            // out.
            // If the type contains filtered elements,
            // keep the declaration.
            return filterType((~s).Type, f, export);
        }
        break;
    }}
    return false;
}

internal static slice<Spec> filterSpecList(slice<Spec> list, Func<@string, bool> f, bool export) {
    nint j = 0;
    foreach (var (_, s) in list) {
        if (filterSpec(s, f, export)) {
            list[j] = s;
            j++;
        }
    }
    return list.slice(0, j);
}

// FilterDecl trims the AST for a Go declaration in place by removing
// all names (including struct field and interface method names, but
// not from parameter lists) that don't pass through the filter f.
//
// FilterDecl reports whether there are any declared names left after
// filtering.
public static bool FilterDecl(Decl decl, Func<@string, bool> f) {
    return filterDecl(decl, f, false);
}

internal static bool filterDecl(Decl decl, Func<@string, bool> f, bool export) {
    switch (decl.type()) {
    case ж<GenDecl> d: {
        d.Value.Specs = filterSpecList((~d).Specs, f, export);
        return len((~d).Specs) > 0;
    }
    case ж<FuncDecl> d: {
        return f((~(~d).Name).Name);
    }}
    return false;
}

// FilterFile trims the AST for a Go file in place by removing all
// names from top-level declarations (including struct field and
// interface method names, but not from parameter lists) that don't
// pass through the filter f. If the declaration is empty afterwards,
// the declaration is removed from the AST. Import declarations are
// always removed. The [File.Comments] list is not changed.
//
// FilterFile reports whether there are any top-level declarations
// left after filtering.
public static bool FilterFile(ж<File> Ꮡsrc, Func<@string, bool> f) {
    return filterFile(ref (Ꮡsrc).DerefOrNull(), f, false);
}

internal static bool filterFile(ref File src, Func<@string, bool> f, bool export) {
    nint j = 0;
    foreach (var (_, d) in src.Decls) {
        if (filterDecl(d, f, export)) {
            src.Decls[j] = d;
            j++;
        }
    }
    src.Decls = src.Decls.slice(0, j);
    return j > 0;
}

// FilterPackage trims the AST for a Go package in place by removing
// all names from top-level declarations (including struct field and
// interface method names, but not from parameter lists) that don't
// pass through the filter f. If the declaration is empty afterwards,
// the declaration is removed from the AST. The pkg.Files list is not
// changed, so that file names and top-level package comments don't get
// lost.
//
// FilterPackage reports whether there are any top-level declarations
// left after filtering.
public static bool FilterPackage(ж<Package> Ꮡpkg, Func<@string, bool> f) {
    return filterPackage(ref (Ꮡpkg).DerefOrNull(), f, false);
}

internal static bool filterPackage(ref Package pkg, Func<@string, bool> f, bool export) {
    var hasDecls = false;
    foreach (var (_, src) in pkg.Files) {
        if (filterFile(ref (src).DerefOrNull(), f, export)) {
            hasDecls = true;
        }
    }
    return hasDecls;
}

[GoType("num:nuint")] partial struct MergeMode;

// ----------------------------------------------------------------------------
// Merging of package files
public static MergeMode FilterFuncDuplicates => /* 1 << iota */ 1;
public static MergeMode FilterUnassociatedComments => 2;
public static MergeMode FilterImportDuplicates => 4;

// nameOf returns the function (foo) or method name (foo.bar) for
// the given function declaration. If the AST is incorrect for the
// receiver, it assumes a function instead.
internal static @string nameOf(ref FuncDecl f) {
    {
        var r = f.Recv; if (r != nil && len((~r).List) == 1) {
            // looks like a correct receiver declaration
            var t = (~r).List[0].Value.Type;
            // dereference pointer receiver types
            {
                var (p, _) = t._<ж<StarExpr>>(ᐧ); if (p != nil) {
                    t = p.Value.X;
                }
            }
            // the receiver type must be a type name
            {
                var (p, _) = t._<ж<Ident>>(ᐧ); if (p != nil) {
                    return (~p).Name + "."u8 + (~f.Name).Name;
                }
            }
        }
    }
    // otherwise assume a function instead
    return (~f.Name).Name;
}

// separator is an empty //-style comment that is interspersed between
// different comment groups when they are concatenated into a single group
internal static ж<Comment> separator = Ꮡ(new Comment(token.NoPos, "//"u8));

// MergePackageFiles creates a file AST by merging the ASTs of the
// files belonging to a package. The mode flags control merging behavior.
public static ж<File> MergePackageFiles(ж<Package> Ꮡpkg, MergeMode mode) {
    ref var pkg = ref Ꮡpkg.DerefOrNull();

    // Count the number of package docs, comments and declarations across
    // all package files. Also, compute sorted list of filenames, so that
    // subsequent iterations can always iterate in the same order.
    nint ndocs = 0;
    nint ncomments = 0;
    nint ndecls = 0;
    var filenames = new slice<@string>(len(pkg.Files));
    ref var minPos = ref heap(new tokenꓸPos(), out var ᏑminPos);
    ref var maxPos = ref heap(new tokenꓸPos(), out var ᏑmaxPos);
    nint i = 0;
    foreach (var (filename, f) in pkg.Files) {
        filenames[i] = filename;
        i++;
        if ((~f).Doc != nil) {
            ndocs += len((~(~f).Doc).List) + 1; // +1 for separator
        }
        ncomments += len((~f).Comments);
        ndecls += len((~f).Decls);
        if (i == 0 || (~f).FileStart < minPos) {
            minPos = f.Value.FileStart;
        }
        if (i == 0 || (~f).FileEnd > maxPos) {
            maxPos = f.Value.FileEnd;
        }
    }
    slices.Sort<slice<@string>, @string>(filenames);
    // Collect package comments from all package files into a single
    // CommentGroup - the collected package documentation. In general
    // there should be only one file with a package comment; but it's
    // better to collect extra comments than drop them on the floor.
    ж<CommentGroup> doc = default!;
    ref var pos = ref heap(new tokenꓸPos(), out var Ꮡpos);
    if (ndocs > 0) {
        var list = new slice<ж<Comment>>(ndocs - 1); // -1: no separator before first group
        nint iΔ1 = 0;
        foreach (var (_, filename) in filenames) {
            var f = pkg.Files[filename];
            if ((~f).Doc != nil) {
                if (iΔ1 > 0) {
                    // not the first group - add separator
                    list[iΔ1] = separator;
                    iΔ1++;
                }
                foreach (var (_, c) in (~(~f).Doc).List) {
                    list[iΔ1] = c;
                    iΔ1++;
                }
                if ((~f).Package > pos) {
                    // Keep the maximum package clause position as
                    // position for the package clause of the merged
                    // files.
                    pos = f.Value.Package;
                }
            }
        }
        doc = Ꮡ(new CommentGroup(list));
    }
    // Collect declarations from all package files.
    slice<Decl> decls = default!;
    if (ndecls > 0) {
        decls = new slice<Decl>(ndecls);
        var funcs = new map<@string, nint>(); // map of func name -> decls index
        nint iΔ2 = 0; // current index
        nint n = 0; // number of filtered entries
        foreach (var (_, filename) in filenames) {
            var f = pkg.Files[filename];
            foreach (var (_, vᴛ1) in (~f).Decls) {
                var d = vᴛ1;

                if ((MergeMode)(mode & FilterFuncDuplicates) != 0) {
                    // A language entity may be declared multiple
                    // times in different package files; only at
                    // build time declarations must be unique.
                    // For now, exclude multiple declarations of
                    // functions - keep the one with documentation.
                    //
                    // TODO(gri): Expand this filtering to other
                    //            entities (const, type, vars) if
                    //            multiple declarations are common.
                    {
                        var (fΔ1, isFun) = d._<ж<FuncDecl>>(ᐧ); if (isFun) {
                            @string name = nameOf(ref (fΔ1).DerefOrNull());
                            {
                                var (j, exists) = funcs[name, ꟷ]; if (exists){
                                    // function declared already
                                    if (decls[j] != default! && (~decls[j]._<ж<FuncDecl>>()).Doc == nil){
                                        // existing declaration has no documentation;
                                        // ignore the existing declaration
                                        decls[j] = default!;
                                    } else {
                                        // ignore the new declaration
                                        d = default!;
                                    }
                                    n++; // filtered an entry
                                } else {
                                    funcs[name] = iΔ2;
                                }
                            }
                        }
                    }
                }
                decls[iΔ2] = d;
                iΔ2++;
            }
        }
        // Eliminate nil entries from the decls list if entries were
        // filtered. We do this using a 2nd pass in order to not disturb
        // the original declaration order in the source (otherwise, this
        // would also invalidate the monotonically increasing position
        // info within a single file).
        if (n > 0) {
            iΔ2 = 0;
            foreach (var (_, d) in decls) {
                if (d != default!) {
                    decls[iΔ2] = d;
                    iΔ2++;
                }
            }
            decls = decls.slice(0, iΔ2);
        }
    }
    // Collect import specs from all package files.
    slice<ж<ImportSpec>> imports = default!;
    if ((MergeMode)(mode & FilterImportDuplicates) != 0){
        var seen = new map<@string, bool>();
        foreach (var (_, filename) in filenames) {
            var f = pkg.Files[filename];
            foreach (var (_, imp) in (~f).Imports) {
                {
                    @string path = imp.Value.Path.Value.Value; if (!seen[path]) {
                        // TODO: consider handling cases where:
                        // - 2 imports exist with the same import path but
                        //   have different local names (one should probably
                        //   keep both of them)
                        // - 2 imports exist but only one has a comment
                        // - 2 imports exist and they both have (possibly
                        //   different) comments
                        imports = append(imports, imp);
                        seen[path] = true;
                    }
                }
            }
        }
    } else {
        // Iterate over filenames for deterministic order.
        foreach (var (_, filename) in filenames) {
            var f = pkg.Files[filename];
            imports = appendꓸꓸꓸ(imports, (~f).Imports);
        }
    }
    // Collect comments from all package files.
    slice<ж<CommentGroup>> comments = default!;
    if ((MergeMode)(mode & FilterUnassociatedComments) == 0) {
        comments = new slice<ж<CommentGroup>>(ncomments);
        nint iΔ3 = 0;
        foreach (var (_, filename) in filenames) {
            var f = pkg.Files[filename];
            iΔ3 += copy(comments.slice(iΔ3), (~f).Comments);
        }
    }
    // TODO(gri) need to compute unresolved identifiers!
    return Ꮡ(new File(doc, pos, NewIdent(pkg.Name), decls, minPos, maxPos, pkg.Scope, imports, default!, comments, ""u8));
}

} // end ast_package
