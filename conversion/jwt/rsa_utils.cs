namespace go.github.com.golang_jwt.jwt;

using rsa = go.crypto.rsa_package;
using x509 = go.crypto.x509_package;
using pem = encoding.pem_package;
using errors = errors_package;
using encoding;
using go.crypto;

partial class jwt_package {

public static error ErrKeyMustBePEMEncoded = errors.New("invalid key: Key must be a PEM encoded PKCS1 or PKCS8 key"u8);
public static error ErrNotRSAPrivateKey = errors.New("key is not a valid RSA private key"u8);
public static error ErrNotRSAPublicKey = errors.New("key is not a valid RSA public key"u8);

public static (ж<rsa.PrivateKey>, error) ParseRSAPrivateKeyFromPEM(slice<byte> key) {
    error err = default!;
    ж<pem.Block> block = default!;
    {
        (block, _) = pem.Decode(key); if (block == nil) {
            return (default!, ErrKeyMustBePEMEncoded);
        }
    }
    any parsedKey = default!;
    {
        (parsedKey, err) = x509.ParsePKCS1PrivateKey((~block).Bytes); if (err != default!) {
            {
                (parsedKey, err) = x509.ParsePKCS8PrivateKey((~block).Bytes); if (err != default!) {
                    return (default!, err);
                }
            }
        }
    }
    ж<rsa.PrivateKey> pkey = default!;
    bool ok = default!;
    {
        (pkey, ok) = parsedKey._<ж<rsa.PrivateKey>>(ᐧ); if (!ok) {
            return (default!, ErrNotRSAPrivateKey);
        }
    }
    return (pkey, default!);
}

public static (ж<rsa.PrivateKey>, error) ParseRSAPrivateKeyFromPEMWithPassword(slice<byte> key, @string password) {
    error err = default!;
    ж<pem.Block> block = default!;
    {
        (block, _) = pem.Decode(key); if (block == nil) {
            return (default!, ErrKeyMustBePEMEncoded);
        }
    }
    any parsedKey = default!;
    slice<byte> blockDecrypted = default!;
    {
        (blockDecrypted, err) = x509.DecryptPEMBlock(block, slice<byte>(password)); if (err != default!) {
            return (default!, err);
        }
    }
    {
        (parsedKey, err) = x509.ParsePKCS1PrivateKey(blockDecrypted); if (err != default!) {
            {
                (parsedKey, err) = x509.ParsePKCS8PrivateKey(blockDecrypted); if (err != default!) {
                    return (default!, err);
                }
            }
        }
    }
    ж<rsa.PrivateKey> pkey = default!;
    bool ok = default!;
    {
        (pkey, ok) = parsedKey._<ж<rsa.PrivateKey>>(ᐧ); if (!ok) {
            return (default!, ErrNotRSAPrivateKey);
        }
    }
    return (pkey, default!);
}

public static (ж<rsa.PublicKey>, error) ParseRSAPublicKeyFromPEM(slice<byte> key) {
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
                    {
                        (parsedKey, errΔ1) = x509.ParsePKCS1PublicKey((~block).Bytes); if (errΔ1 != default!) {
                            return (default!, errΔ1);
                        }
                    }
                }
            }
        }
    }
    ж<rsa.PublicKey> pkey = default!;
    bool ok = default!;
    {
        (pkey, ok) = parsedKey._<ж<rsa.PublicKey>>(ᐧ); if (!ok) {
            return (default!, ErrNotRSAPublicKey);
        }
    }
    return (pkey, default!);
}

} // end jwt_package
