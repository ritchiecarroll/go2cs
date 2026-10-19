namespace go;

partial class main_package {

public partial interface Ord_T<T> /*dyn*/ {
    bool Less(T _);
}

partial struct Ord<T>
    where T : Ord_T<T>
{
    internal T v;
}

public static bool Below<T>(this Ord<T> o, T t)
    where T : Ord_T<T>
{
    return o.v.Less(t);
}

partial interface Lesser<T> {
    bool Less(T _);
}

partial struct age /*num:nint*/;

internal static bool Less(this age a, age b) {
    return a < b;
}

partial struct node {
    internal nint k;
}

internal static bool Less(this ref node n, ж<node> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    return n.k < o.k;
}

partial struct wrap<E> {
    internal nint n;
}

internal static bool Less<E>(this wrap<E> w, wrap<E> o) {
    return w.n < o.n;
}

} // end main_package
