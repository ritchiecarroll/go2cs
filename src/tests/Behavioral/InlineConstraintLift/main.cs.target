namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct namer {
    internal @string s;
}

internal static @string Name(this namer n) {
    return n.s;
}

partial struct tagged {
    internal @string s;
    internal nint tag;
}

internal static @string Name(this tagged t) {
    return t.s;
}

internal static nint Tag(this tagged t) {
    return t.tag;
}

public partial interface Show_T /*dyn*/ {
    @string Name();
}

public static @string Show<T>(T t)
    where T : Show_T
{
    return "<"u8 + t.Name() + ">"u8;
}

internal static @string show<T>(T t)
    where T : Show_T
{
    return "["u8 + t.Name() + "]"u8;
}

public partial interface Label_T /*dyn*/ {
    @string Name();
    nint Tag();
}

public static @string Label<T>(T t)
    where T : Label_T
{
    return fmt.Sprintf("%s#%d"u8, t.Name(), t.Tag());
}

public partial interface Box_T /*dyn*/ {
    @string Name();
}

partial struct Box<T>
    where T : Box_T
{
    internal slice<T> items;
}

public static void Add<T>(this ref Box<T> b, T t)
    where T : Box_T
{
    b.items = append(b.items, t);
}

public static @string Names<T>(this ref Box<T> b)
    where T : Box_T
{
    @string @out = "";
    foreach (var (_, it) in b.items) {
        @out += it.Name() + ";"u8;
    }
    return @out;
}

partial interface pair_T /*dyn*/ {
    @string Name();
}

partial struct pair<T>
    where T : pair_T
{
    internal T a, b;
}

internal static @string joined<T>(this pair<T> p)
    where T : pair_T
{
    return p.a.Name() + "+"u8 + p.b.Name();
}

partial interface nameI {
    @string Name();
}

partial struct slot<T>
    where T : nameI
{
    internal T v;
}

internal static @string get<T>(this slot<T> s)
    where T : nameI
{
    return s.v.Name();
}

public partial interface Both_C /*dyn*/ {
    nint Tag();
}

public static @string Both<A, B, C>(A a, B b, C c)
    where A : Box_T
    where B : Box_T
    where C : Both_C
{
    return a.Name() + "/"u8 + b.Name() + "/"u8 + fmt.Sprint(c.Tag());
}

public partial interface Min_T<T> /*dyn*/ {
    bool Less(T _);
}

public static T Min<T>(T a, T b)
    where T : Min_T<T>
{
    if (b.Less(a)) {
        return b;
    }
    return a;
}

internal static void Main() {
    fmt.Println(Show(new namer("x")));
    fmt.Println(show(new namer("y")));
    fmt.Println(Label(new tagged("t", 7)));

    var b = Ꮡ(new Box<namer>(nil));
    b.Add(new namer("a"));
    b.Add(new namer("b"));
    fmt.Println(b.Names());

    fmt.Println(new pair<tagged>(new tagged("p", 1), new tagged("q", 2)).joined());
    fmt.Println(new slot<tagged>(new tagged("s", 9)).get());
    fmt.Println(Both(new namer("n"), new tagged("m", 0), new tagged("k", 3)));

    fmt.Println(Min(((age)5), ((age)3)));
    fmt.Println(new Ord<age>(v: 3).Below(5), new Ord<age>(v: 3).Below(2));
    fmt.Println(new Ord<nodeжOrd_T>(v: Ꮡ(new node(1))).Below(Ꮡ(new node(0))), new Ord<nodeжOrd_T>(v: Ꮡ(new node(1))).Below(Ꮡ(new node(4))));
    fmt.Println(new Ord<wrap<@string>>(v: new wrap<@string>(2)).Below(new wrap<@string>(5)), new Ord<wrap<nint>>(v: new wrap<nint>(7)).Below(new wrap<nint>(3)));

    Lesser<wrap<@string>> l = new wrap<@string>(1);
    fmt.Println(l.Less(new wrap<@string>(2)), l.Less(new wrap<@string>(0)));
}

} // end main_package
