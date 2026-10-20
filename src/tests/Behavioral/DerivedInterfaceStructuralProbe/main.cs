namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct tempErr {
    internal @string msg;
}

internal static @string Error(this ref tempErr e) {
    return e.msg;
}

internal static bool Temporary(this ref tempErr e) {
    return true;
}

partial struct plainErr {
    internal @string msg;
}

internal static @string Error(this ref plainErr e) {
    return e.msg;
}

internal partial interface classify_type /*dyn*/ :
    error
{
    bool Temporary();
}

internal static @string classify(error err) {
    {
        var (te, ok) = err._<classify_type>(ᐧ); if (ok) {
            return fmt.Sprintf("temporary=%v msg=%v"u8, te.Temporary(), te.Error());
        }
    }

    return "not temporary";
}

partial interface stringish {
    @string String();
}

partial interface named :
    stringish
{
    @string Name();
}

partial struct node {
    internal @string id;
}

internal static @string String(this ref node n) {
    return "node:"u8 + n.id;
}

internal static @string Name(this ref node n) {
    return n.id;
}

internal static nint Depth(this ref node n) {
    return 3;
}

partial struct shallow {
    internal @string id;
}

internal static @string String(this ref shallow s) {
    return "shallow:"u8 + s.id;
}

internal static @string Name(this ref shallow s) {
    return s.id;
}

internal partial interface describe_type /*dyn*/ :
    named
{
    nint Depth();
}

internal static @string describe(any v) {
    {
        var (d, ok) = v._<describe_type>(ᐧ); if (ok) {
            return fmt.Sprintf("%v %v %v"u8, d.String(), d.Name(), d.Depth());
        }
    }

    return "not deep";
}

internal static void Main() {
    fmt.Println(classify(new tempErrжerror(Ꮡ(new tempErr(msg: "boom")))));
    fmt.Println(classify(new plainErrжerror(Ꮡ(new plainErr(msg: "plain")))));
    fmt.Println(describe(Ꮡ(new node(id: "n1"))));
    fmt.Println(describe(Ꮡ(new shallow(id: "s1"))));
}

} // end main_package
