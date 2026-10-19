namespace go;

using keylike = GenericInterfaceCrossPackage.keylike_package;
using GenericInterfaceCrossPackage;

partial class main_package {

partial struct encKey {
    internal slice<byte> b;
}

internal static slice<byte> Bytes(this ref encKey e) {
    return e.b;
}

partial struct decKey {
    internal slice<byte> b;
    internal ж<encKey> ek;
}

internal static slice<byte> Bytes(this ref decKey d) {
    return d.b;
}

internal static ж<encKey> EncapsulationKey(this ref decKey d) {
    return d.ek;
}

internal static (ж<decKey>, error) generateKey() {
    return (Ꮡ(new decKey(b: new byte[]{1, 2}.slice(), ek: Ꮡ(new encKey(b: new byte[]{3, 4}.slice())))), default!);
}

internal static (ж<encKey>, error) newEncapKey(slice<byte> b) {
    return (Ꮡ(new encKey(b: appendꓸꓸꓸ(slice<byte>(default!), b))), default!);
}

internal static void Main() {
    keylike.RoundTrip<keylike.EncapKey, keylike.DecapKey<keylike.EncapKey>>("round trip", widenResult<ж<decKey>, keylike.DecapKey<keylike.EncapKey>>(generateKey, elemᴛ1 => new decKeyжDecapKey(elemᴛ1)), widen<slice<byte>, ж<encKey>, keylike.EncapKey>(newEncapKey, elemᴛ2 => new encKeyжEncapKey(elemᴛ2)));
}

} // end main_package
