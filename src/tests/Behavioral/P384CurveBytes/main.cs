namespace go;

using ecdh = crypto.ecdh_package;
using ecdsa = crypto.ecdsa_package;
using elliptic = crypto.elliptic_package;
using sha512 = crypto.sha512_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using big = math.big_package;
using crypto;
using encoding;
using math;

partial class main_package {

internal static slice<byte> key(@string label) {
    var d = sha512.Sum384(slice<byte>(label));
    d[0] &= (byte)(0x7f);
    return d[..];
}

internal static readonly @string fixedSig = "30660231009f9537dcd1b97340cbc064cc9d1f6ef09f142c1d70472b4f9738d58d9d69c35755ab0baca0ae0849b2b845b39bff4c4b023100cceb089f51f1f4b122ca5d7b7a6779459b661539662c1585d6f15598d7e2f361b36622275e1bbe9f88ddbd87ea710e2d"u8;

internal static void Main() {
    var curve = ecdh.P384();

    for (nint i = 0; i < 4; i++) {
        var (a, errΔ1) = curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-384 ECDH a %d"u8, i)));
        if (errΔ1 != nil) {
            throw panic(errΔ1);
        }
        (var b, errΔ1) = curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-384 ECDH b %d"u8, i)));
        if (errΔ1 != nil) {
            throw panic(errΔ1);
        }
        (var ab, errΔ1) = a.ECDH(b.PublicKey());
        if (errΔ1 != nil) {
            throw panic(errΔ1);
        }
        (var ba, errΔ1) = b.ECDH(a.PublicKey());
        if (errΔ1 != nil) {
            throw panic(errΔ1);
        }
        fmt.Printf("ecdh %d pub %x\n"u8, i, a.PublicKey().Bytes());
        fmt.Printf("ecdh %d shared %x agree %v\n"u8, i, ab, ((sstring)ab) == ((sstring)ba));
    }

    var (priv, err) = ecdsa.ParseRawPrivateKey(elliptic.P384(), key("go2cs P-384 behavioral key 0"));
    if (err != nil) {
        throw panic(err);
    }
    (var pub, err) = priv.ᏑPublicKey.Bytes();
    if (err != nil) {
        throw panic(err);
    }
    fmt.Printf("ecdsa pub %x\n"u8, pub);

    var h = sha512.Sum384(slice<byte>("go2cs P-384 behavioral message"u8));
    var (sig, _) = hex.DecodeString(fixedSig);
    fmt.Println("ecdsa fixed signature verifies:", ecdsa.VerifyASN1(priv.ᏑPublicKey, h[..], sig));

    var tampered = appendꓸꓸꓸ(slice<byte>(default), sig);
    tampered[len(tampered) - 1] ^= (byte)(1);
    fmt.Println("ecdsa tampered signature verifies:", ecdsa.VerifyASN1(priv.ᏑPublicKey, h[..], tampered));

    h[0] ^= (byte)(1);
    fmt.Println("ecdsa other message verifies:", ecdsa.VerifyASN1(priv.ᏑPublicKey, h[..], sig));

    var p = elliptic.P384();
    foreach (var (i, k) in slice<int64>([1, 2, 3, 0x17f, 1099511627776L])) {
        var (x, y) = p.ScalarBaseMult(big.NewInt(k).Bytes());
        var (x2, y2) = p.ScalarMult(x, y, key(fmt.Sprintf("go2cs P-384 scalar %d"u8, i)));
        fmt.Printf("elliptic %d base %x %x\n"u8, i, x.OrTypedNil(), y.OrTypedNil());
        fmt.Printf("elliptic %d mult %x %x on curve %v\n"u8, i, x2.OrTypedNil(), y2.OrTypedNil(), p.IsOnCurve(x2, y2));
    }
}

} // end main_package
