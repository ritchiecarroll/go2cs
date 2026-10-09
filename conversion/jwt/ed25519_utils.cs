namespace go.github.com.golang_jwt.jwt;

using crypto = crypto_package;
using ed25519 = go.crypto.ed25519_package;
using x509 = go.crypto.x509_package;
using pem = encoding.pem_package;
using errors = errors_package;
using encoding;
using go.crypto;

partial class jwt_package {

public static error ErrNotEdPrivateKey = errors.New("key is not a valid Ed25519 private key"u8);
public static error ErrNotEdPublicKey = errors.New("key is not a valid Ed25519 public key"u8);

public static (cryptoꓸPrivateKey, error) ParseEdPrivateKeyFromPEM(slice<byte> key) {
    error err = default!;
    ж<pem.Block> block = default!;
    {
        (block, _) = pem.Decode(key); if (block == nil) {
            return (default!, ErrKeyMustBePEMEncoded);
        }
    }
    any parsedKey = default!;
    {
        (parsedKey, err) = x509.ParsePKCS8PrivateKey((~block).Bytes); if (err != default!) {
            return (default!, err);
        }
    }
    ed25519.PrivateKey pkey = default!;
    bool ok = default!;
    {
        (pkey, ok) = parsedKey._<ed25519.PrivateKey>(ᐧ); if (!ok) {
            return (default!, ErrNotEdPrivateKey);
        }
    }
    return (pkey, default!);
}

public static (cryptoꓸPublicKey, error) ParseEdPublicKeyFromPEM(slice<byte> key) {
    error err = default!;
    ж<pem.Block> block = default!;
    {
        (block, _) = pem.Decode(key); if (block == nil) {
            return (default!, ErrKeyMustBePEMEncoded);
        }
    }
    any parsedKey = default!;
    {
        (parsedKey, err) = x509.ParsePKIXPublicKey((~block).Bytes); if (err != default!) {
            return (default!, err);
        }
    }
    ed25519.PublicKey pkey = default!;
    bool ok = default!;
    {
        (pkey, ok) = parsedKey._<ed25519.PublicKey>(ᐧ); if (!ok) {
            return (default!, ErrNotEdPublicKey);
        }
    }
    return (pkey, default!);
}

} // end jwt_package
