namespace go.ItabLateRegistration;

using lib = go.ItabLateRegistration.lib_package;
using go.ItabLateRegistration;

partial class latelib_package {

public static (@string, nint) Prime() {
    ref var c = ref heap<lib.Circle>(new lib.Circle(R: 7), out var Ꮡc);
    ref var q = ref heap<lib.Square>(new lib.Square(S: 8), out var Ꮡq);

    lib.Named n = new lib.CircleжNamed(Ꮡc);
    lib.Sized s = new lib.SquareжSized(Ꮡq);

    return (n.Name(), s.Size());
}

} // end latelib_package
