namespace go;

using ecdh = crypto.ecdh_package;
using ecdsa = crypto.ecdsa_package;
using elliptic = crypto.elliptic_package;
using sha256 = crypto.sha256_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using big = math.big_package;
using crypto;
using encoding;
using math;

partial class main_package {

internal static slice<byte> key(@string label) {
    var d = sha256.Sum256(slice<byte>(label));
    d[0] &= (byte)(0x7f);
    return d[..];
}

internal static readonly @string fixedSig = "3045022039ecb7479d32b4b4bdc7d3848a53a59a60a16ebbca393045a3c3424c9a6a494a022100d7c2d0d24c24092e004677a0426a29fba901d520cf22dd5abf43e04de892fefd"u8;

internal static void Main() {
    var curve = ecdh.P256();

    for (nint i = 0; i < 4; i++) {
        var (a, errΔ1) = curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-256 ECDH a %d"u8, i)));
        if (errΔ1 != nil) {
            throw panic(errΔ1);
        }
        (var b, errΔ1) = curve.NewPrivateKey(key(fmt.Sprintf("go2cs P-256 ECDH b %d"u8, i)));
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

    var (priv, err) = ecdsa.ParseRawPrivateKey(elliptic.P256(), key("go2cs P-256 behavioral key 0"));
    if (err != nil) {
        throw panic(err);
    }
    (var pub, err) = priv.ᏑPublicKey.Bytes();
    if (err != nil) {
        throw panic(err);
    }
    fmt.Printf("ecdsa pub %x\n"u8, pub);

    var h = sha256.Sum256(slice<byte>("go2cs P-256 behavioral message"u8));
    var (sig, _) = hex.DecodeString(fixedSig);
    fmt.Println("ecdsa fixed signature verifies:", ecdsa.VerifyASN1(priv.ᏑPublicKey, h[..], sig));

    var tampered = appendꓸꓸꓸ(slice<byte>(default), sig);
    tampered[len(tampered) - 1] ^= (byte)(1);
    fmt.Println("ecdsa tampered signature verifies:", ecdsa.VerifyASN1(priv.ᏑPublicKey, h[..], tampered));

    h[0] ^= (byte)(1);
    fmt.Println("ecdsa other message verifies:", ecdsa.VerifyASN1(priv.ᏑPublicKey, h[..], sig));

    var p = elliptic.P256();
    foreach (var (i, k) in slice<int64>([1, 2, 3, 0xff, 1099511627776L])) {
        var (x, y) = p.ScalarBaseMult(big.NewInt(k).Bytes());
        var (x2, y2) = p.ScalarMult(x, y, key(fmt.Sprintf("go2cs P-256 scalar %d"u8, i)));
        fmt.Printf("elliptic %d base %x %x\n"u8, i, x.OrTypedNil(), y.OrTypedNil());
        fmt.Printf("elliptic %d mult %x %x on curve %v\n"u8, i, x2.OrTypedNil(), y2.OrTypedNil(), p.IsOnCurve(x2, y2));
    }
}

} // end main_package
