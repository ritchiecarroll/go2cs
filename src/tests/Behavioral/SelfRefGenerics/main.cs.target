namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Adder<A> 
    where A : Adder<A>{
    A Add(A _);
}

partial struct vec {
    internal nint x, y;
}

internal static vec Add(this vec v, vec o) {
    return new vec(v.x + o.x, v.y + o.y);
}

partial struct num /*num:nint*/;

internal static num Add(this num n, num o) {
    return n + o;
}

public static A Sum<A>(params Span<A> xsʗp)
    where A : Adder<A>
{
    var xs = xsʗp.sslice();

    A total = GoZero<A>();
    foreach (var (_, x) in xs) {
        total = total.Add(x);
    }
    return total;
}

partial interface Ordered<T> 
    where T : Ordered<T>{
    bool Less(T _);
}

partial struct age /*num:nint*/;

internal static bool Less(this age a, age b) {
    return a < b;
}

public static T Min<T>(T a, T b)
    where T : Ordered<T>
{
    if (b.Less(a)) {
        return b;
    }
    return a;
}

partial struct MinSet<T>
    where T : Ordered<T>
{
    internal slice<T> items;
}

public static void Put<T>(this ref MinSet<T> s, T x)
    where T : Ordered<T>
{
    s.items = append(s.items, x);
}

public static T Least<T>(this ref MinSet<T> s)
    where T : Ordered<T>
{
    var m = s.items[0];
    foreach (var (_, x) in s.items[1..]) {
        m = Min(m, x);
    }
    return m;
}

partial interface GraphNode<N> 
    where N : GraphNode<N>{
    N Next();
    ж<Graph<N>> Owner();
    @string Label();
}

partial struct Graph<N>
    where N : GraphNode<N>
{
    internal N head;
    internal nint size;
}

public static @string Labels<N>(this ж<Graph<N>> Ꮡg)
    where N : GraphNode<N>
{
    ref var g = ref Ꮡg.DerefOrNull();

    @string @out = "";
    var x = g.head;
    foreach (var _ᴛ1 in range(g.size)) {
        if (x.Owner() != Ꮡg) {
            @out += "?"u8;
        }
        @out += x.Label();
        x = x.Next();
    }
    return @out;
}

partial struct gnode {
    internal ж<Graph<gnodeжGraphNode>> g;
    internal ж<gnode> next;
    internal @string name;
}

internal static ж<gnode> Next(this ref gnode n) {
    return n.next;
}

internal static ж<Graph<gnodeжGraphNode>> Owner(this ref gnode n) {
    return n.g;
}

internal static @string Label(this ref gnode n) {
    return n.name;
}

internal static void Main() {
    fmt.Println(Sum(new vec(1, 2), new vec(3, 4), new vec(5, 6)));
    fmt.Println(Sum<num>(1, 2, 3, 4));
    fmt.Println(Min((age)7, (age)3));

    MinSet<age> ms = default;
    ms.Put(5);
    ms.Put(2);
    ms.Put(8);
    fmt.Println(ms.Least());

    var g = Ꮡ(new Graph<gnodeжGraphNode>(size: 4));
    var a = Ꮡ(new gnode(g: g, name: "a"));
    var b = Ꮡ(new gnode(g: g, name: "b"));
    var c = Ꮡ(new gnode(name: "c"));
    (a.next, b.next, c.next) = (b, c, a);
    g.head = a;
    fmt.Println(g.Labels());
}

} // end main_package
