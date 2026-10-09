namespace go.github.com.golang_jwt.jwt.v5;

using crypto = crypto_package;
using rsa = go.crypto.rsa_package;
using os = os_package;
using jwt = go.github.com.golang_jwt.jwt.jwt_package;
using ecdsa = go.crypto.ecdsa_package;
using go.crypto;
using go.github.com.golang_jwt.jwt;

partial class test_package {

public static ж<rsa.PrivateKey> LoadRSAPrivateKeyFromDisk(@string location) {
    var (keyData, e) = os.ReadFile(location);
    if (e != default!) {
        throw panic(e.Error());
    }
    (var key, e) = jwt.ParseRSAPrivateKeyFromPEM(keyData);
    if (e != default!) {
        throw panic(e.Error());
    }
    return key;
}

public static ж<rsa.PublicKey> LoadRSAPublicKeyFromDisk(@string location) {
    var (keyData, e) = os.ReadFile(location);
    if (e != default!) {
        throw panic(e.Error());
    }
    (var key, e) = jwt.ParseRSAPublicKeyFromPEM(keyData);
    if (e != default!) {
        throw panic(e.Error());
    }
    return key;
}

public static @string MakeSampleToken(jwt.Claims c, jwt.SigningMethod method, any key) {
    var token = jwt.NewWithClaims(method, c);
    var (s, e) = token.SignedString(key);
    if (e != default!) {
        throw panic(e.Error());
    }
    return s;
}

public static cryptoꓸPrivateKey LoadECPrivateKeyFromDisk(@string location) {
    var (keyData, e) = os.ReadFile(location);
    if (e != default!) {
        throw panic(e.Error());
    }
    (var key, e) = jwt.ParseECPrivateKeyFromPEM(keyData);
    if (e != default!) {
        throw panic(e.Error());
    }
    return key.OrTypedNil();
}

public static cryptoꓸPublicKey LoadECPublicKeyFromDisk(@string location) {
    var (keyData, e) = os.ReadFile(location);
    if (e != default!) {
        throw panic(e.Error());
    }
    (var key, e) = jwt.ParseECPublicKeyFromPEM(keyData);
    if (e != default!) {
        throw panic(e.Error());
    }
    return key.OrTypedNil();
}

} // end test_package
