// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

// RFC 1423 describes the encryption of PEM blocks. The algorithm used to
// generate a key from the password was derived by looking at the OpenSSL
// implementation.
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using md5 = go.crypto.md5_package;
using hex = encoding.hex_package;
using pem = encoding.pem_package;
using errors = errors_package;
using io = io_package;
using strings = strings_package;
using encoding;
using go.crypto;
using hash = hash_package;

partial class x509_package {

[GoType("num:nint")] partial struct PEMCipher;

// Possible values for the EncryptPEMBlock encryption algorithm.
internal static PEMCipher _ᴛ1ʗ => /* iota */ 0;

public static PEMCipher PEMCipherDES => 1;

public static PEMCipher PEMCipher3DES => 2;

public static PEMCipher PEMCipherAES128 => 3;

public static PEMCipher PEMCipherAES192 => 4;

public static PEMCipher PEMCipherAES256 => 5;

// rfc1423Algo holds a method for enciphering a PEM block.
[GoType] partial struct rfc1423Algo {
    internal PEMCipher cipher;
    internal @string name;
    internal Func<slice<byte>, (cipher.Block, error)> cipherFunc;
    internal nint keySize;
    internal nint blockSize;
}

// rfc1423Algos holds a slice of the possible ways to encrypt a PEM
// block. The ivSize numbers were taken from the OpenSSL source.
internal static ж<slice<rfc1423Algo>> Ꮡrfc1423Algos = new StandardBox<slice<rfc1423Algo>>(new rfc1423Algo[]{new(
    cipher: PEMCipherDES,
    name: "DES-CBC"u8,
    cipherFunc: des.NewCipher,
    keySize: 8,
    blockSize: des.ΔBlockSize
), new(
    cipher: PEMCipher3DES,
    name: "DES-EDE3-CBC"u8,
    cipherFunc: des.NewTripleDESCipher,
    keySize: 24,
    blockSize: des.ΔBlockSize
), new(
    cipher: PEMCipherAES128,
    name: "AES-128-CBC"u8,
    cipherFunc: aes.NewCipher,
    keySize: 16,
    blockSize: aes.BlockSize
), new(
    cipher: PEMCipherAES192,
    name: "AES-192-CBC"u8,
    cipherFunc: aes.NewCipher,
    keySize: 24,
    blockSize: aes.BlockSize
), new(
    cipher: PEMCipherAES256,
    name: "AES-256-CBC"u8,
    cipherFunc: aes.NewCipher,
    keySize: 32,
    blockSize: aes.BlockSize
)
}.slice());
internal static ref slice<rfc1423Algo> rfc1423Algos => ref Ꮡrfc1423Algos.ValueSlot;

// deriveKey uses a key derivation function to stretch the password into a key
// with the number of bits our cipher requires. This algorithm was derived from
// the OpenSSL source.
internal static slice<byte> deriveKey(this rfc1423Algo c, slice<byte> password, slice<byte> salt) {
    var hash = md5.New();
    var @out = new slice<byte>(c.keySize);
    slice<byte> digest = default!;
    for (nint i = 0; i < builtin.len(@out); i += builtin.len(digest)) {
        hash.Reset();
        hash.Write(digest);
        hash.Write(password);
        hash.Write(salt);
        digest = hash.Sum(digest[..0]);
        copy(@out[(int)(i)..], digest);
    }
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dekInfoˢ = "DEK-Info"u8;

// IsEncryptedPEMBlock returns whether the PEM block is password encrypted
// according to RFC 1423.
//
// Deprecated: Legacy PEM encryption as specified in RFC 1423 is insecure by
// design. Since it does not authenticate the ciphertext, it is vulnerable to
// padding oracle attacks that can let an attacker recover the plaintext.
public static bool IsEncryptedPEMBlock(ж<pem.Block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (_, ok) = b.Headers[dekInfoˢ, ꟷ];
    return ok;
}

// IncorrectPasswordError is returned when an incorrect password is detected.
public static error IncorrectPasswordError = errors.New("x509: decryption password incorrect"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509NoDekInfoHeaderInˢ = "x509: no DEK-Info header in block"u8;
internal static readonly @string x509MalformedDekInfoˢ = "x509: malformed DEK-Info header"u8;
internal static readonly @string x509UnknownEncryptionˢ = "x509: unknown encryption mode"u8;
internal static readonly @string x509IncorrectIvSizeˢ = "x509: incorrect IV size"u8;
internal static readonly @string x509EncryptedPemDataIsˢ = "x509: encrypted PEM data is not a multiple of the block size"u8;
internal static readonly @string x509InvalidPaddingˢ = "x509: invalid padding"u8;

// DecryptPEMBlock takes a PEM block encrypted according to RFC 1423 and the
// password used to encrypt it and returns a slice of decrypted DER encoded
// bytes. It inspects the DEK-Info header to determine the algorithm used for
// decryption. If no DEK-Info header is present, an error is returned. If an
// incorrect password is detected an [IncorrectPasswordError] is returned. Because
// of deficiencies in the format, it's not always possible to detect an
// incorrect password. In these cases no error will be returned but the
// decrypted DER bytes will be random noise.
//
// Deprecated: Legacy PEM encryption as specified in RFC 1423 is insecure by
// design. Since it does not authenticate the ciphertext, it is vulnerable to
// padding oracle attacks that can let an attacker recover the plaintext.
public static (slice<byte>, error) DecryptPEMBlock(ж<pem.Block> Ꮡb, slice<byte> password) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (dek, ok) = b.Headers[dekInfoˢ, ꟷ];
    if (!ok) {
        return (default!, errors.New(x509NoDekInfoHeaderInˢ));
    }
    (var mode, var hexIV, ok) = strings.Cut(dek, ","u8);
    if (!ok) {
        return (default!, errors.New(x509MalformedDekInfoˢ));
    }
    var ciph = cipherByName(mode);
    if (ciph == nil) {
        return (default!, errors.New(x509UnknownEncryptionˢ));
    }
    var (iv, err) = hex.DecodeString(hexIV);
    if (err != default!) {
        return (default!, err);
    }
    if (builtin.len(iv) != (~ciph).blockSize) {
        return (default!, errors.New(x509IncorrectIvSizeˢ));
    }
    // Based on the OpenSSL implementation. The salt is the first 8 bytes
    // of the initialization vector.
    var key = (~ciph).deriveKey(password, iv[..8]);
    (var block, err) = (~ciph).cipherFunc(key);
    if (err != default!) {
        return (default!, err);
    }
    if (builtin.len(b.Bytes) % block.BlockSize() != 0) {
        return (default!, errors.New(x509EncryptedPemDataIsˢ));
    }
    var data = new slice<byte>(builtin.len(b.Bytes));
    var dec = cipher.NewCBCDecrypter(block, iv);
    dec.CryptBlocks(data, b.Bytes);
    // Blocks are padded using a scheme where the last n bytes of padding are all
    // equal to n. It can pad from 1 to blocksize bytes inclusive. See RFC 1423.
    // For example:
    //	[x y z 2 2]
    //	[x y 7 7 7 7 7 7 7]
    // If we detect a bad padding, we assume it is an invalid password.
    nint dlen = builtin.len(data);
    if (dlen == 0 || rem(dlen, (~ciph).blockSize) != 0) {
        return (default!, errors.New(x509InvalidPaddingˢ));
    }
    nint last = (nint)data[dlen - 1];
    if (dlen < last) {
        return (default!, IncorrectPasswordError);
    }
    if (last == 0 || last > (~ciph).blockSize) {
        return (default!, IncorrectPasswordError);
    }
    foreach (var (_, val) in data[(int)(dlen - last)..]) {
        if ((nint)val != last) {
            return (default!, IncorrectPasswordError);
        }
    }
    return (data[..(int)(dlen - last)], default!);
}

// EncryptPEMBlock returns a PEM block of the specified type holding the
// given DER encoded data encrypted with the specified algorithm and
// password according to RFC 1423.
//
// Deprecated: Legacy PEM encryption as specified in RFC 1423 is insecure by
// design. Since it does not authenticate the ciphertext, it is vulnerable to
// padding oracle attacks that can let an attacker recover the plaintext.
public static (ж<pem.Block>, error) EncryptPEMBlock(io.Reader rand, @string blockType, slice<byte> data, slice<byte> password, PEMCipher alg) {
    var ciph = cipherByKey(alg);
    if (ciph == nil) {
        return (default!, errors.New(x509UnknownEncryptionˢ));
    }
    var iv = new slice<byte>((~ciph).blockSize);
    {
        var (_, errΔ1) = io.ReadFull(rand, iv); if (errΔ1 != default!) {
            return (default!, errors.New("x509: cannot generate IV: "u8 + errΔ1.Error()));
        }
    }
    // The salt is the first 8 bytes of the initialization vector,
    // matching the key derivation in DecryptPEMBlock.
    var key = (~ciph).deriveKey(password, iv[..8]);
    var (block, err) = (~ciph).cipherFunc(key);
    if (err != default!) {
        return (default!, err);
    }
    var enc = cipher.NewCBCEncrypter(block, iv);
    nint pad = (~ciph).blockSize - builtin.len(data) % (~ciph).blockSize;
    var encrypted = new slice<byte>(builtin.len(data), builtin.len(data) + pad);
    // We could save this copy by encrypting all the whole blocks in
    // the data separately, but it doesn't seem worth the additional
    // code.
    copy(encrypted, data);
    // See RFC 1423, Section 1.1.
    for (nint i = 0; i < pad; i++) {
        encrypted = append(encrypted, (byte)pad);
    }
    enc.CryptBlocks(encrypted, encrypted);
    return (Ꮡ(new pem.Block(
        Type: blockType,
        Headers: new map<@string, @string>{
            ["Proc-Type"u8] = "4,ENCRYPTED"u8,
            ["DEK-Info"u8] = (~ciph).name + ","u8 + hex.EncodeToString(iv)
        },
        Bytes: encrypted
    )), default!);
}

internal static ж<rfc1423Algo> cipherByName(@string name) {
    foreach (var (i, _) in rfc1423Algos) {
        var alg = Ꮡ(rfc1423Algos, i);
        if ((~alg).name == name) {
            return alg;
        }
    }
    return default!;
}

internal static ж<rfc1423Algo> cipherByKey(PEMCipher key) {
    foreach (var (i, _) in rfc1423Algos) {
        var alg = Ꮡ(rfc1423Algos, i);
        if ((~alg).cipher == key) {
            return alg;
        }
    }
    return default!;
}

} // end x509_package
