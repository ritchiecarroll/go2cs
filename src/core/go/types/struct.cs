// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using static @internal.types.errors_package;
using strconv = strconv_package;
using errors = @internal.types.errors_package;
using global::go.go;

partial class types_package {

// ----------------------------------------------------------------------------
// API

// A Struct represents a struct type.
partial struct Struct {
    internal slice<ж<Var>> fields; // fields != nil indicates the struct is set up (possibly with len(fields) == 0)
    internal slice<@string> tags; // field tags; nil if there are no tags
}

// NewStruct returns a new struct with the given fields and corresponding field tags.
// If a field with index i has a tag, tags[i] must be that tag, but len(tags) may be
// only as long as required to hold the tag with the largest index i. Consequently,
// if no field has a tag, tags may be nil.
public static ж<Struct> NewStruct(slice<ж<Var>> fields, slice<@string> tags) {
    objset fset = default!;
    foreach (var (_, f) in fields) {
        if ((~f).name != "_"u8 && fset.insert(new VarжObject(f)) != default!) {
            throw panic("multiple fields with the same name");
        }
    }
    if (len(tags) > len(fields)) {
        throw panic("more tags than fields");
    }
    var s = Ꮡ(new Struct(fields: fields, tags: tags));
    s.markComplete();
    return s;
}

// NumFields returns the number of fields in the struct (including blank and embedded fields).
public static nint NumFields(this ref Struct s) {
    return len(s.fields);
}

// Field returns the i'th field for 0 <= i < NumFields().
public static ж<Var> Field(this ref Struct s, nint i) {
    return s.fields[i];
}

// Tag returns the i'th field tag for 0 <= i < NumFields().
public static @string Tag(this ref Struct s, nint i) {
    if (i < len(s.tags)) {
        return s.tags[i];
    }
    return ""u8;
}

public static ΔType Underlying(this ж<Struct> Ꮡt) {
    return new StructжΔType(Ꮡt);
}

public static @string String(this ж<Struct> Ꮡt) {
    return TypeString(new StructжΔType(Ꮡt), default!);
}

// ----------------------------------------------------------------------------
// Implementation
internal static void markComplete(this ref Struct s) {
    if (s.fields == default!) {
        s.fields = new slice<ж<Var>>(0);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string embeddedFieldTypeCannotˢ = "embedded field type cannot be unsafe.Pointer"u8;
internal static readonly @string embeddedFieldTypeCannotˢ2 = "embedded field type cannot be a pointer"u8;
internal static readonly @string embeddedFieldTypeCannotˢ3 = "embedded field type cannot be a (pointer to a) type parameter"u8;
internal static readonly @string embeddedFieldTypeCannotˢ4 = "embedded field type cannot be a pointer to an interface"u8;

internal static void structType(this ж<Checker> Ꮡcheck, ж<Struct> Ꮡstyp, ж<ast.StructType> Ꮡe) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var styp = ref Ꮡstyp.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    var list = e.Fields;
    if (list == nil) {
        styp.markComplete();
        return;
    }
    // struct fields and tags
    ref var fields = ref heap<slice<ж<Var>>>(out var Ꮡfields);
    ref var tags = ref heap<slice<@string>>(out var Ꮡtags);
    // for double-declaration checks
    ref var fset = ref heap<objset>(out var Ꮡfset);
    // current field typ and tag
    ref var typ = ref heap<ΔType>(out var Ꮡtyp);
    @string tag = default!;
    void add(ж<ast.Ident> ident, bool embedded) {
        if (tag != ""u8 && Ꮡtags.ValueSlot == default!) {
            Ꮡtags.ValueSlot = new slice<@string>(len(Ꮡfields.ValueSlot));
        }
        if (Ꮡtags.ValueSlot != default!) {
            Ꮡtags.ValueSlot = append(Ꮡtags.ValueSlot, tag);
        }
        tokenꓸPos pos = ident.Pos();
        @string name = ident.Value.Name;
        var fld = NewField(pos, Ꮡcheck.Value.pkg, name, Ꮡtyp.ValueSlot, embedded);
        // spec: "Within a struct, non-blank field names must be unique."
        if (name == "_"u8 || Ꮡcheck.declareInSet(Ꮡfset, pos, new VarжObject(fld))) {
            Ꮡfields.ValueSlot = append(Ꮡfields.ValueSlot, fld);
            Ꮡcheck.Value.recordDef(ident, new VarжObject(fld));
        }
    }
    // addInvalid adds an embedded field of invalid type to the struct for
    // fields with errors; this keeps the number of struct fields in sync
    // with the source as long as the fields are _ or have different names
    // (go.dev/issue/25627).
    var addʗ1 = add;
    void addInvalid(ж<ast.Ident> ident) {
        Ꮡtyp.ValueSlot = new BasicжΔType(Typ[Invalid]);
        tag = ""u8;
        addʗ1(ident, true);
    }
    foreach (var (_, f) in (~list).List) {
        typ = Ꮡcheck.varType((~f).Type);
        tag = Ꮡcheck.tag((~f).Tag);
        if (len((~f).Names) > 0){
            // named fields
            foreach (var (_, name) in (~f).Names) {
                add(name, false);
            }
        } else {
            // embedded field
            // spec: "An embedded type must be specified as a type name T or as a
            // pointer to a non-interface type name *T, and T itself may not be a
            // pointer type."
            tokenꓸPos pos = (~f).Type.Pos(); // position of type, for errors
            var name = embeddedFieldIdent((~f).Type);
            if (name == nil) {
                Ꮡcheck.errorf(new ast_Exprᴠpositioner((~f).Type), InvalidSyntaxTree, "embedded field type %s has no name"u8, (~f).Type);
                name = ast.NewIdent("_"u8);
                name.Value.NamePos = pos;
                addInvalid(name);
                continue;
            }
            add(name, true); // struct{p.T} field has position of T
            // Because we have a name, typ must be of the form T or *T, where T is the name
            // of a (named or alias) type, and t (= deref(typ)) must be the type of T.
            // We must delay this check to the end because we don't want to instantiate
            // (via under(t)) a possibly incomplete type.
            // for use in the closure below
            var embeddedTyp = typ;
            var embeddedPos = f.Value.Type;
            var embeddedPosʗ1 = embeddedPos;
            var embeddedTypʗ1 = embeddedTyp;
            check.later(() => {
                var (t, isPtr) = deref(embeddedTypʗ1);
                switch (under(t).type()) {
                case ж<Basic> u: {
                    if (!isValid(t)) {
                        // error was reported before
                        return;
                    }
                    if ((~u).kind == UnsafePointer) {
                        // unsafe.Pointer is treated like a regular pointer
                        Ꮡcheck.error(new ast_Exprᴠpositioner(embeddedPosʗ1), InvalidPtrEmbed, embeddedFieldTypeCannotˢ);
                    }
                    break;
                }
                case ж<Pointer> u: {
                    Ꮡcheck.error(new ast_Exprᴠpositioner(embeddedPosʗ1), InvalidPtrEmbed, embeddedFieldTypeCannotˢ2);
                    break;
                }
                case ж<Interface> u: {
                    if (isTypeParam(t)) {
                        // The error code here is inconsistent with other error codes for
                        // invalid embedding, because this restriction may be relaxed in the
                        // future, and so it did not warrant a new error code.
                        Ꮡcheck.error(new ast_Exprᴠpositioner(embeddedPosʗ1), MisplacedTypeParam, embeddedFieldTypeCannotˢ3);
                        break;
                    }
                    if (isPtr) {
                        Ꮡcheck.error(new ast_Exprᴠpositioner(embeddedPosʗ1), InvalidPtrEmbed, embeddedFieldTypeCannotˢ4);
                    }
                    break;
                }}
            }).describef(new ast_Exprᴠpositioner(embeddedPos), "check embedded type %s"u8, embeddedTyp);
        }
    }
    styp.fields = fields;
    styp.tags = tags;
    styp.markComplete();
}

internal static ж<ast.Ident> embeddedFieldIdent(ast.Expr e) {
    switch (e.type()) {
    case ж<ast.Ident> eΔ1: {
        return eΔ1;
    }
    case ж<ast.StarExpr> eΔ1: {
        {
            var (_, ok) = (~eΔ1).X._<ж<ast.StarExpr>>(ᐧ); if (!ok) {
                // *T is valid, but **T is not
                return embeddedFieldIdent((~eΔ1).X);
            }
        }
        break;
    }
    case ж<ast.SelectorExpr> eΔ1: {
        return (~eΔ1).Sel;
    }
    case ж<ast.IndexExpr> eΔ1: {
        return embeddedFieldIdent((~eΔ1).X);
    }
    case ж<ast.IndexListExpr> eΔ1: {
        return embeddedFieldIdent((~eΔ1).X);
    }}
    return default!; // invalid embedded field
}

internal static bool declareInSet(this ж<Checker> Ꮡcheck, ж<objset> Ꮡoset, tokenꓸPos pos, Object obj) {
    ref var oset = ref Ꮡoset.DerefOrNull();

    {
        var alt = oset.insert(obj); if (alt != default!) {
            var err = Ꮡcheck.newError(DuplicateDecl);
            err.addf(((atPos)pos), "%s redeclared"u8, obj.Name());
            err.addAltDecl(alt);
            err.report();
            return false;
        }
    }
    return true;
}

internal static @string tag(this ж<Checker> Ꮡcheck, ж<ast.BasicLit> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Ꮡt != nil) {
        if (t.Kind == token.STRING) {
            {
                var (val, err) = strconv.Unquote(t.Value); if (err == default!) {
                    return val;
                }
            }
        }
        Ꮡcheck.errorf(new ast_BasicLitжpositioner(Ꮡt), InvalidSyntaxTree, "incorrect tag syntax: %q"u8, t.Value);
    }
    return ""u8;
}

} // end types_package
