namespace go.ItabLateRegistration;

using lib = go.ItabLateRegistration.lib_package;
using go.ItabLateRegistration;

partial class latelib_package {

public static (@string, nint) Prime() {
    ref var c = ref heap<lib.Circle>(out var Ꮡc);
    c = new lib.Circle(R: 7);
    ref var q = ref heap<lib.Square>(out var Ꮡq);
    q = new lib.Square(S: 8);

    lib.Named n = new lib.CircleжNamed(Ꮡc);
    lib.Sized s = new lib.SquareжSized(Ꮡq);

    return (n.Name(), s.Size());
}

} // end latelib_package
