namespace go.github.com.golang_jwt.jwt;

using ecdsa = go.crypto.ecdsa_package;
using x509 = go.crypto.x509_package;
using pem = encoding.pem_package;
using errors = errors_package;
using encoding;
using go.crypto;

partial class jwt_package {

public static error ErrNotECPublicKey = errors.New("key is not a valid ECDSA public key"u8);
public static error ErrNotECPrivateKey = errors.New("key is not a valid ECDSA private key"u8);

public static (ж<ecdsa.PrivateKey>, error) ParseECPrivateKeyFromPEM(slice<byte> key) {
    error err = default!;
    ж<pem.Block> block = default!;
    {
        (block, _) = pem.Decode(key); if (block == nil) {
            return (default!, ErrKeyMustBePEMEncoded);
        }
    }
    any parsedKey = default!;
    {
        (parsedKey, err) = x509.ParseECPrivateKey((~block).Bytes); if (err != default!) {
            {
                (parsedKey, err) = x509.ParsePKCS8PrivateKey((~block).Bytes); if (err != default!) {
                    return (default!, err);
                }
            }
        }
    }
    ж<ecdsa.PrivateKey> pkey = default!;
    bool ok = default!;
    {
        (pkey, ok) = parsedKey._<ж<ecdsa.PrivateKey>>(ᐧ); if (!ok) {
            return (default!, ErrNotECPrivateKey);
        }
    }
    return (pkey, default!);
}

public static (ж<ecdsa.PublicKey>, error) ParseECPublicKeyFromPEM(slice<byte> key) {
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
            {
                var (cert, errΔ1) = x509.ParseCertificate((~block).Bytes); if (errΔ1 == default!){
                    parsedKey = cert.Value.PublicKey;
                } else {
                    return (default!, errΔ1);
                }
            }
        }
    }
    ж<ecdsa.PublicKey> pkey = default!;
    bool ok = default!;
    {
        (pkey, ok) = parsedKey._<ж<ecdsa.PublicKey>>(ᐧ); if (!ok) {
            return (default!, ErrNotECPublicKey);
        }
    }
    return (pkey, default!);
}

} // end jwt_package
