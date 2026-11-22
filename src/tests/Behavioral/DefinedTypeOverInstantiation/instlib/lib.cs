global using Alias = go.go.instlib_package.Pair<go.@string>;
global using Alias2 = go.go.instlib_package.Pair<go.@string>;

namespace go.go;

using static instlib_package;

partial class instlib_package {

partial struct Pair<T> {
    public T A;
    public nint B;
}

} partial class instlibᴛPair {

public static T First<T>(this Pair<T> p) { return p.A; }

} partial class instlib_package {

partial struct Q /*Pair<nint>*/;

public static (slice<Q>, ж<Q>) Same() {
    ref var q = ref heap<Q>(new Q(new Pair<nint>(A: 1, B: 2)), out var Ꮡq);
    return (slice<Q>([q, new(new Pair<nint>(A: 3))]), Ꮡq);
}

} // end instlib_package
