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
    var d = sha512.Sum512(slice<byte>(label));
    var raw = appendꓸꓸꓸ(slice<byte>([0x01]), d[..]);
    return append(raw, (byte)0x5a);
}

internal static readonly @string fixedSig = "308187024127f6c8c09b10eee4ae0d773c088dc6b1503ee66d8b5e426c5cc703b53d31cbc2fcda3efc690f13304cb216d3f8adc1df4e5be19cd1a6d4706a93735982553bd8bb02420177558dbd35bde68d14e5505c488ce0450326714df7e6605000e34079a5e19bf261da154433259e5769e646b4b5aa543447658880a30915770eb5868295edf33a74"u8;

internal static void Main() {
    var curve = ecdh.P521();

    for (nint i = 0; i < 4; i++) {
        var (a, errΔ1) = curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-521 ECDH a %d"u8, i)));
        if (errΔ1 != nil) {
            throw panic(errΔ1);
        }
        (var b, errΔ1) = curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-521 ECDH b %d"u8, i)));
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

    var (priv, err) = ecdsa.ParseRawPrivateKey(elliptic.P521(), key("go2cs P-521 behavioral key 0"));
    if (err != nil) {
        throw panic(err);
    }
    (var pub, err) = priv.ᏑPublicKey.Bytes();
    if (err != nil) {
        throw panic(err);
    }
    fmt.Printf("ecdsa pub %x\n"u8, pub);

    var h = sha512.Sum512(slice<byte>("go2cs P-521 behavioral message"u8));
    var (sig, _) = hex.DecodeString(fixedSig);
    fmt.Println("ecdsa fixed signature verifies:", ecdsa.VerifyASN1(priv.ᏑPublicKey, h[..], sig));

    var tampered = appendꓸꓸꓸ(slice<byte>(default), sig);
    tampered[len(tampered) - 1] ^= (byte)(1);
    fmt.Println("ecdsa tampered signature verifies:", ecdsa.VerifyASN1(priv.ᏑPublicKey, h[..], tampered));

    h[0] ^= (byte)(1);
    fmt.Println("ecdsa other message verifies:", ecdsa.VerifyASN1(priv.ᏑPublicKey, h[..], sig));

    var p = elliptic.P521();
    foreach (var (i, k) in slice<int64>([1, 2, 3, 0x1ff, 1099511627776L])) {
        var (x, y) = p.ScalarBaseMult(big.NewInt(k).Bytes());
        var (x2, y2) = p.ScalarMult(x, y, key(fmt.Sprintf("go2cs P-521 scalar %d"u8, i)));
        fmt.Printf("elliptic %d base %x %x\n"u8, i, x.OrTypedNil(), y.OrTypedNil());
        fmt.Printf("elliptic %d mult %x %x on curve %v\n"u8, i, x2.OrTypedNil(), y2.OrTypedNil(), p.IsOnCurve(x2, y2));
    }
}

} // end main_package
