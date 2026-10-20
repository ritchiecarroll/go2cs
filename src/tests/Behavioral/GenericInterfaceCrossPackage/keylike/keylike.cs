namespace go.GenericInterfaceCrossPackage;

using fmt = fmt_package;

partial class keylike_package {

partial interface EncapKey {
    slice<byte> Bytes();
}

partial interface DecapKey<E> 
    where E : EncapKey{
    slice<byte> Bytes();
    E EncapsulationKey();
}

public static void RoundTrip<E, D>(@string name, Func<(D, error)> generate, Func<slice<byte>, (E, error)> newEncap)
    where E : EncapKey
    where D : DecapKey<E>
{
    var (dk, err) = generate();
    if (err != default!) {
        fmt.Println(name, "generate:", err);
        return;
    }
    var ek = dk.EncapsulationKey();
    (var ek1, err) = newEncap(ek.Bytes());
    if (err != default!) {
        fmt.Println(name, "newEncap:", err);
        return;
    }
    fmt.Printf("%s: dk=%v ek=%v ek1=%v\n"u8, name, dk.Bytes(), ek.Bytes(), ek1.Bytes());
}

} // end keylike_package
