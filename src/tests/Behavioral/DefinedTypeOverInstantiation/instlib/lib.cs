namespace go.go;

partial class instlib_package {

partial struct Pair<T> {
    public T A;
    public nint B;
}

public static T First<T>(this Pair<T> p) { return p.A; }

partial struct Q /*Pair<nint>*/;

public static (slice<Q>, ж<Q>) Same() {
    ref var q = ref heap<Q>(new Q(new Pair<nint>(A: 1, B: 2)), out var Ꮡq);
    return (slice<Q>([q, new(new Pair<nint>(A: 3))]), Ꮡq);
}

} // end instlib_package
