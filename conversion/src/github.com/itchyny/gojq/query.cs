namespace go.github.com.itchyny;

using context = context_package;
using strings = strings_package;

partial class gojq_package {

public static (ж<Query>, error) Parse(@string src) {
    var l = newLexer(src);
    if (yyParse(new lexerжyyLexer(l)) > 0) {
        return (default, l.err);
    }
    return (l.result, default);
}

partial struct Query {
    public ж<ConstObject> Meta;
    public slice<ж<Import>> Imports;
    public slice<ж<FuncDef>> FuncDefs;
    public ж<Term> Term;
    public ж<Query> Left;
    public ж<Query> Right;
    public slice<ж<Pattern>> Patterns;
    public Operator Op;
}

public static Iter Run(this ж<Query> Ꮡe, any v) {
    return Ꮡe.RunWithContext(context.Background(), v);
}

public static Iter RunWithContext(this ж<Query> Ꮡe, context.Context ctx, any v) {
    var (code, err) = Compile(Ꮡe);
    if (err != nil) {
        return NewIter(err);
    }
    return code.RunWithContext(ctx, v);
}

public static @string String(this ref Query e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Query e, ж<strings.Builder> s) {
    if (e.Meta != nil) {
        s.WriteString("module ");
        e.Meta.writeTo(s);
        s.WriteString(";\n");
    }
    foreach (var (_, im) in range(e.Imports)) {
        im.writeTo(s);
    }
    foreach (var (_, fd) in range(e.FuncDefs)) {
        fd.writeTo(s);
        s.WriteByte((byte)' ');
    }
    if (e.Term != nil) {
        e.Term.writeTo(s);
    } else if (e.Right != nil) {
        e.Left.writeTo(s);
        if (e.Op != OpComma) {
            s.WriteByte((byte)' ');
        }
        foreach (var (i, p) in range(e.Patterns)) {
            if (i == 0) {
                s.WriteString("as ");
            } else {
                s.WriteString("?// ");
            }
            p.writeTo(s);
            s.WriteByte((byte)' ');
        }
        s.WriteString(e.Op.String());
        s.WriteByte((byte)' ');
        e.Right.writeTo(s);
    }
}

internal static any toIndexKey(this ref Query e) {
    if (e.Term == nil) {
        return default;
    }
    return e.Term.toIndexKey();
}

internal static slice<any> toIndices(this ref Query e, slice<any> xs) {
    if (e.Term == nil) {
        return default;
    }
    return e.Term.toIndices(xs);
}

partial struct Import {
    public @string ImportPath;
    public @string ImportAlias;
    public @string IncludePath;
    public ж<ConstObject> Meta;
}

public static @string String(this ref Import e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Import e, ж<strings.Builder> s) {
    if (e.ImportPath != ""u8) {
        s.WriteString("import ");
        jsonEncodeString(s, e.ImportPath);
        s.WriteString(" as ");
        s.WriteString(e.ImportAlias);
    } else {
        s.WriteString("include ");
        jsonEncodeString(s, e.IncludePath);
    }
    if (e.Meta != nil) {
        s.WriteByte((byte)' ');
        e.Meta.writeTo(s);
    }
    s.WriteString(";\n");
}

partial struct FuncDef {
    public @string Name;
    public slice<@string> Args;
    public ж<Query> Body;
}

public static @string String(this ref FuncDef e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref FuncDef e, ж<strings.Builder> s) {
    s.WriteString("def ");
    s.WriteString(e.Name);
    if (len(e.Args) > 0) {
        s.WriteByte((byte)'(');
        foreach (var (i, eΔ1) in range(e.Args)) {
            if (i > 0) {
                s.WriteString("; ");
            }
            s.WriteString(eΔ1);
        }
        s.WriteByte((byte)')');
    }
    s.WriteString(": ");
    e.Body.writeTo(s);
    s.WriteByte((byte)';');
}

partial struct Term {
    public TermType Type;
    public ж<Index> Index;
    public ж<Func> Func;
    public ж<Object> Object;
    public ж<Array> Array;
    public @string Number;
    public ж<Unary> Unary;
    public @string Format;
    public ж<ΔString> Str;
    public ж<If> If;
    public ж<Try> Try;
    public ж<Reduce> Reduce;
    public ж<Foreach> Foreach;
    public ж<Label> Label;
    public @string Break;
    public ж<Query> Query;
    public slice<ж<Suffix>> SuffixList;
}

public static @string String(this ref Term e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Term e, ж<strings.Builder> s) {
    switch (e.Type) {
    case var ᴛ when ᴛ == TermTypeIdentity: {
        s.WriteByte((byte)'.');
        break;
    }
    case var ᴛ when ᴛ == TermTypeRecurse: {
        s.WriteString("..");
        break;
    }
    case var ᴛ when ᴛ == TermTypeNull: {
        s.WriteString("null");
        break;
    }
    case var ᴛ when ᴛ == TermTypeTrue: {
        s.WriteString("true");
        break;
    }
    case var ᴛ when ᴛ == TermTypeFalse: {
        s.WriteString("false");
        break;
    }
    case var ᴛ when ᴛ == TermTypeIndex: {
        e.Index.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeFunc: {
        e.Func.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeObject: {
        e.Object.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeArray: {
        e.Array.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeNumber: {
        s.WriteString(e.Number);
        break;
    }
    case var ᴛ when ᴛ == TermTypeUnary: {
        e.Unary.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeFormat: {
        s.WriteString(e.Format);
        if (e.Str != nil) {
            s.WriteByte((byte)' ');
            e.Str.writeTo(s);
        }
        break;
    }
    case var ᴛ when ᴛ == TermTypeString: {
        e.Str.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeIf: {
        e.If.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeTry: {
        e.Try.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeReduce: {
        e.Reduce.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeForeach: {
        e.Foreach.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeLabel: {
        e.Label.writeTo(s);
        break;
    }
    case var ᴛ when ᴛ == TermTypeBreak: {
        s.WriteString("break ");
        s.WriteString(e.Break);
        break;
    }
    case var ᴛ when ᴛ == TermTypeQuery: {
        s.WriteByte((byte)'(');
        e.Query.writeTo(s);
        s.WriteByte((byte)')');
        break;
    }}

    foreach (var (_, eΔ1) in range(e.SuffixList)) {
        eΔ1.writeTo(s);
    }
}

internal static any toIndexKey(this ref Term e) {
    switch (e.Type) {
    case var ᴛ when ᴛ == TermTypeNumber: {
        return toNumber(e.Number);
    }
    case var ᴛ when ᴛ == TermTypeUnary: {
        return e.Unary.toNumber();
    }
    case var ᴛ when ᴛ == TermTypeString: {
        if (e.Str.Queries == nil) {
            return e.Str.Str;
        }
        return default;
    }
    default: {
        return default;
    }}

}

internal static slice<any> toIndices(this ref Term e, slice<any> xs) {
    switch (e.Type) {
    case var ᴛ when ᴛ == TermTypeIndex: {
        xs = e.Index.toIndices(xs); if (xs == nil) {
            return default;
        }
        break;
    }
    case var ᴛ when ᴛ == TermTypeQuery: {
        xs = e.Query.toIndices(xs); if (xs == nil) {
            return default;
        }
        break;
    }
    default: {
        return default;
    }}

    foreach (var (_, s) in range(e.SuffixList)) {
        xs = s.toIndices(xs); if (xs == nil) {
            return default;
        }
    }
    return xs;
}

internal static any toNumber(this ref Term e) {
    if (e.Type == TermTypeNumber) {
        return toNumber(e.Number);
    }
    return default;
}

partial struct Unary {
    public Operator Op;
    public ж<Term> Term;
}

public static @string String(this ref Unary e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Unary e, ж<strings.Builder> s) {
    s.WriteString(e.Op.String());
    e.Term.writeTo(s);
}

internal static any toNumber(this ref Unary e) {
    var v = e.Term.toNumber();
    if (v != nil && e.Op == OpSub) {
        v = funcOpNegate(v);
    }
    return v;
}

partial struct Pattern {
    public @string Name;
    public slice<ж<Pattern>> Array;
    public slice<ж<PatternObject>> Object;
}

public static @string String(this ref Pattern e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Pattern e, ж<strings.Builder> s) {
    if (e.Name != ""u8) {
        s.WriteString(e.Name);
    } else if (len(e.Array) > 0) {
        s.WriteByte((byte)'[');
        foreach (var (i, eΔ1) in range(e.Array)) {
            if (i > 0) {
                s.WriteString(", ");
            }
            eΔ1.writeTo(s);
        }
        s.WriteByte((byte)']');
    } else if (len(e.Object) > 0) {
        s.WriteByte((byte)'{');
        foreach (var (i, eΔ2) in range(e.Object)) {
            if (i > 0) {
                s.WriteString(", ");
            }
            eΔ2.writeTo(s);
        }
        s.WriteByte((byte)'}');
    }
}

partial struct PatternObject {
    public @string Key;
    public ж<ΔString> KeyString;
    public ж<Query> KeyQuery;
    public ж<Pattern> Val;
}

public static @string String(this ref PatternObject e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref PatternObject e, ж<strings.Builder> s) {
    if (e.Key != ""u8) {
        s.WriteString(e.Key);
    } else if (e.KeyString != nil) {
        e.KeyString.writeTo(s);
    } else if (e.KeyQuery != nil) {
        s.WriteByte((byte)'(');
        e.KeyQuery.writeTo(s);
        s.WriteByte((byte)')');
    }
    if (e.Val != nil) {
        s.WriteString(": ");
        e.Val.writeTo(s);
    }
}

partial struct Index {
    public @string Name;
    public ж<ΔString> Str;
    public ж<Query> Start;
    public ж<Query> End;
    public bool IsSlice;
}

public static @string String(this ref Index e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Index e, ж<strings.Builder> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    nint l = s.Len(); if (l > 0) {
        var c = s.String()[l - 1]; if (c == '.' || '0' <= c && c <= '9') {
            Ꮡs.WriteByte((byte)' ');
        }
    }
    Ꮡs.WriteByte((byte)'.');
    e.writeSuffixTo(Ꮡs);
}

internal static void writeSuffixTo(this ref Index e, ж<strings.Builder> s) {
    if (e.Name != ""u8) {
        s.WriteString(e.Name);
    } else if (e.Str != nil) {
        e.Str.writeTo(s);
    } else {
        s.WriteByte((byte)'[');
        if (e.IsSlice) {
            if (e.Start != nil) {
                e.Start.writeTo(s);
            }
            s.WriteByte((byte)':');
            if (e.End != nil) {
                e.End.writeTo(s);
            }
        } else {
            e.Start.writeTo(s);
        }
        s.WriteByte((byte)']');
    }
}

internal static any toIndexKey(this ref Index e) {
    if (e.Name != ""u8) {
        return e.Name;
    } else if (e.Str != nil) {
        if (e.Str.Queries == nil) {
            return e.Str.Str;
        }
    } else if (!e.IsSlice) {
        return e.Start.toIndexKey();
    } else {
        any start = default;
        any end = default;
        var ok = true;
        if (e.Start != nil) {
            start = e.Start.toIndexKey();
            ok = start != nil;
        }
        if (e.End != nil && ok) {
            end = e.End.toIndexKey();
            ok = end != nil;
        }
        if (ok) {
            return new map<@string, any>{["start"] = start, ["end"] = end};
        }
    }
    return default;
}

internal static slice<any> toIndices(this ref Index e, slice<any> xs) {
    var k = e.toIndexKey(); if (k != nil) {
        return builtin.append(xs, k);
    }
    return default;
}

partial struct Func {
    public @string Name;
    public slice<ж<Query>> Args;
}

public static @string String(this ref Func e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Func e, ж<strings.Builder> s) {
    s.WriteString(e.Name);
    if (len(e.Args) > 0) {
        s.WriteByte((byte)'(');
        foreach (var (i, eΔ1) in range(e.Args)) {
            if (i > 0) {
                s.WriteString("; ");
            }
            eΔ1.writeTo(s);
        }
        s.WriteByte((byte)')');
    }
}

partial struct ΔString {
    public @string Str;
    public slice<ж<Query>> Queries;
}

public static @string String(this ref ΔString e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref ΔString e, ж<strings.Builder> s) {
    if (e.Queries == nil) {
        jsonEncodeString(s, e.Str);
        return;
    }
    s.WriteByte((byte)'"');
    foreach (var (_, eΔ1) in range(e.Queries)) {
        if (eΔ1.Term.Str == nil) {
            s.WriteString(@"\");
            eΔ1.writeTo(s);
        } else {
            @string es = eΔ1.String();
            s.WriteString(es.slice(1, len(es) - 1));
        }
    }
    s.WriteByte((byte)'"');
}

partial struct Object {
    public slice<ж<ObjectKeyVal>> KeyVals;
}

public static @string String(this ref Object e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Object e, ж<strings.Builder> s) {
    if (len(e.KeyVals) == 0) {
        s.WriteString("{}");
        return;
    }
    s.WriteString("{ ");
    foreach (var (i, kv) in range(e.KeyVals)) {
        if (i > 0) {
            s.WriteString(", ");
        }
        kv.writeTo(s);
    }
    s.WriteString(" }");
}

partial struct ObjectKeyVal {
    public @string Key;
    public ж<ΔString> KeyString;
    public ж<Query> KeyQuery;
    public ж<Query> Val;
}

public static @string String(this ref ObjectKeyVal e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref ObjectKeyVal e, ж<strings.Builder> s) {
    if (e.Key != ""u8) {
        s.WriteString(e.Key);
    } else if (e.KeyString != nil) {
        e.KeyString.writeTo(s);
    } else if (e.KeyQuery != nil) {
        s.WriteByte((byte)'(');
        e.KeyQuery.writeTo(s);
        s.WriteByte((byte)')');
    }
    if (e.Val != nil) {
        s.WriteString(": ");
        e.Val.writeTo(s);
    }
}

partial struct Array {
    public ж<Query> Query;
}

public static @string String(this ref Array e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Array e, ж<strings.Builder> s) {
    s.WriteByte((byte)'[');
    if (e.Query != nil) {
        e.Query.writeTo(s);
    }
    s.WriteByte((byte)']');
}

partial struct Suffix {
    public ж<Index> Index;
    public bool Iter;
    public bool Optional;
}

public static @string String(this ref Suffix e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Suffix e, ж<strings.Builder> s) {
    if (e.Index != nil) {
        if (e.Index.Name != ""u8 || e.Index.Str != nil) {
            e.Index.writeTo(s);
        } else {
            e.Index.writeSuffixTo(s);
        }
    } else if (e.Iter) {
        s.WriteString("[]");
    } else if (e.Optional) {
        s.WriteByte((byte)'?');
    }
}

internal static ж<Term> toTerm(this ref Suffix e) {
    if (e.Index != nil) {
        return Ꮡ(new Term(Type: TermTypeIndex, Index: e.Index));
    } else if (e.Iter) {
        return Ꮡ(new Term(Type: TermTypeIdentity, SuffixList: [Ꮡ(new Suffix(Iter: true))]));
    } else {
        return default;
    }
}

internal static slice<any> toIndices(this ref Suffix e, slice<any> xs) {
    if (e.Index == nil) {
        return default;
    }
    return e.Index.toIndices(xs);
}

partial struct If {
    public ж<Query> Cond;
    public ж<Query> Then;
    public slice<ж<IfElif>> Elif;
    public ж<Query> Else;
}

public static @string String(this ref If e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref If e, ж<strings.Builder> s) {
    s.WriteString("if ");
    e.Cond.writeTo(s);
    s.WriteString(" then ");
    e.Then.writeTo(s);
    foreach (var (_, eΔ1) in range(e.Elif)) {
        s.WriteByte((byte)' ');
        eΔ1.writeTo(s);
    }
    if (e.Else != nil) {
        s.WriteString(" else ");
        e.Else.writeTo(s);
    }
    s.WriteString(" end");
}

partial struct IfElif {
    public ж<Query> Cond;
    public ж<Query> Then;
}

public static @string String(this ref IfElif e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref IfElif e, ж<strings.Builder> s) {
    s.WriteString("elif ");
    e.Cond.writeTo(s);
    s.WriteString(" then ");
    e.Then.writeTo(s);
}

partial struct Try {
    public ж<Query> Body;
    public ж<Query> Catch;
}

public static @string String(this ref Try e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Try e, ж<strings.Builder> s) {
    s.WriteString("try ");
    e.Body.writeTo(s);
    if (e.Catch != nil) {
        s.WriteString(" catch ");
        e.Catch.writeTo(s);
    }
}

partial struct Reduce {
    public ж<Query> Query;
    public ж<Pattern> Pattern;
    public ж<Query> Start;
    public ж<Query> Update;
}

public static @string String(this ref Reduce e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Reduce e, ж<strings.Builder> s) {
    s.WriteString("reduce ");
    e.Query.writeTo(s);
    s.WriteString(" as ");
    e.Pattern.writeTo(s);
    s.WriteString(" (");
    e.Start.writeTo(s);
    s.WriteString("; ");
    e.Update.writeTo(s);
    s.WriteByte((byte)')');
}

partial struct Foreach {
    public ж<Query> Query;
    public ж<Pattern> Pattern;
    public ж<Query> Start;
    public ж<Query> Update;
    public ж<Query> Extract;
}

public static @string String(this ref Foreach e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Foreach e, ж<strings.Builder> s) {
    s.WriteString("foreach ");
    e.Query.writeTo(s);
    s.WriteString(" as ");
    e.Pattern.writeTo(s);
    s.WriteString(" (");
    e.Start.writeTo(s);
    s.WriteString("; ");
    e.Update.writeTo(s);
    if (e.Extract != nil) {
        s.WriteString("; ");
        e.Extract.writeTo(s);
    }
    s.WriteByte((byte)')');
}

partial struct Label {
    public @string Ident;
    public ж<Query> Body;
}

public static @string String(this ref Label e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref Label e, ж<strings.Builder> s) {
    s.WriteString("label ");
    s.WriteString(e.Ident);
    s.WriteString(" | ");
    e.Body.writeTo(s);
}

partial struct ConstTerm {
    public ж<ConstObject> Object;
    public ж<ConstArray> Array;
    public @string Number;
    public @string Str;
    public bool Null;
    public bool True;
    public bool False;
}

public static @string String(this ref ConstTerm e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref ConstTerm e, ж<strings.Builder> s) {
    if (e.Object != nil) {
        e.Object.writeTo(s);
    } else if (e.Array != nil) {
        e.Array.writeTo(s);
    } else if (e.Number != ""u8) {
        s.WriteString(e.Number);
    } else if (e.Null) {
        s.WriteString("null");
    } else if (e.True) {
        s.WriteString("true");
    } else if (e.False) {
        s.WriteString("false");
    } else {
        jsonEncodeString(s, e.Str);
    }
}

internal static any toValue(this ref ConstTerm e) {
    if (e.Object != nil) {
        return e.Object.ToValue();
    } else if (e.Array != nil) {
        return e.Array.toValue();
    } else if (e.Number != ""u8) {
        return toNumber(e.Number);
    } else if (e.Null) {
        return default;
    } else if (e.True) {
        return true;
    } else if (e.False) {
        return false;
    } else {
        return e.Str;
    }
}

internal static (@string, bool) toString(this ref ConstTerm e) {
    if (e.Object != nil || e.Array != nil || e.Number != ""u8 || e.Null || e.True || e.False) {
        return ("", false);
    }
    return (e.Str, true);
}

partial struct ConstObject {
    public slice<ж<ConstObjectKeyVal>> KeyVals;
}

public static @string String(this ref ConstObject e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref ConstObject e, ж<strings.Builder> s) {
    if (len(e.KeyVals) == 0) {
        s.WriteString("{}");
        return;
    }
    s.WriteString("{ ");
    foreach (var (i, kv) in range(e.KeyVals)) {
        if (i > 0) {
            s.WriteString(", ");
        }
        kv.writeTo(s);
    }
    s.WriteString(" }");
}

public static map<@string, any> ToValue(this ж<ConstObject> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    if (Ꮡe == nil) {
        return default;
    }
    var v = new map<@string, any>(len(e.KeyVals));
    foreach (var (_, eΔ1) in range(e.KeyVals)) {
        @string key = eΔ1.Key;
        if (key == ""u8) {
            key = eΔ1.KeyString;
        }
        v[key] = eΔ1.Val.toValue();
    }
    return v;
}

partial struct ConstObjectKeyVal {
    public @string Key;
    public @string KeyString;
    public ж<ConstTerm> Val;
}

public static @string String(this ref ConstObjectKeyVal e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref ConstObjectKeyVal e, ж<strings.Builder> s) {
    if (e.Key != ""u8) {
        s.WriteString(e.Key);
    } else {
        jsonEncodeString(s, e.KeyString);
    }
    s.WriteString(": ");
    e.Val.writeTo(s);
}

partial struct ConstArray {
    public slice<ж<ConstTerm>> Elems;
}

public static @string String(this ref ConstArray e) {
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    e.writeTo(Ꮡs);
    return s.String();
}

internal static void writeTo(this ref ConstArray e, ж<strings.Builder> s) {
    s.WriteByte((byte)'[');
    foreach (var (i, eΔ1) in range(e.Elems)) {
        if (i > 0) {
            s.WriteString(", ");
        }
        eΔ1.writeTo(s);
    }
    s.WriteByte((byte)']');
}

internal static slice<any> toValue(this ref ConstArray e) {
    var v = new slice<any>(len(e.Elems));
    foreach (var (i, eΔ1) in range(e.Elems)) {
        v[i] = eΔ1.toValue();
    }
    return v;
}

} // end gojq_package
