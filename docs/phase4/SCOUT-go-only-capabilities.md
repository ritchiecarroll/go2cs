# Go capabilities the .NET 10 BCL lacks (scout for the release announcement)

> **State: SCOUT, 2026-10-26.** Read-only research for the owner-asked "Go capabilities .NET doesn't have"
> question, feeding the wider 1.26.10.1 announcement. Not a visitor document and not for master's visitor docs.
> Nothing in the converter, golib or corpus was changed.
>
> **Method.** The .NET side was read on learn.microsoft.com (API reference pages, the cross-platform
> cryptography support tables, and the "What's new in .NET 10" and ".NET 11" library pages). The Go side was read
> in Go 1.26.10's own source. Counts are each package's banked row in
> [`../ValidatedTestPackages.md`](../ValidatedTestPackages.md) at master `b6ccfa80a5` (matched verdicts, then
> disclosed): a banked row means Go's own tests for that package pass converted.
>
> **Two limits on the evidence.** "No .NET type" rests on the namespace listing or a learn.microsoft.com search
> finding none, which is evidence of absence, not proof. Several API pages now render the .NET 11 prerelease
> view, so a type listed on a namespace page is not by itself proof that it ships in .NET 10; where that matters
> the row says so.

## Tiers

- **A**: absent from the .NET 10 BCL (third-party only, or nothing).
- **B**: present in .NET 10 but platform-dependent (an OS build or OpenSSL version gates it, `IsSupported` is
  false elsewhere), where the converted Go is managed code that runs the same on every OS.
- **C**: present in both, with a difference a developer may want.
- **Not a gap**: .NET 10 has a close cross-platform equivalent.
- **UNVERIFIED**: not settled; do not claim.

## Tier counts

| Tier | Rows |
|---|---|
| A | 21 |
| B | 5 |
| C | 9 |
| Not a gap | 10 |
| UNVERIFIED | 3 |

## Cryptography

Evidence keys: **XP** = [Cross-platform cryptography in .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography);
**NS** = [System.Security.Cryptography namespace](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography?view=net-10.0);
**N10** = [What's new in .NET 10 libraries](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/libraries);
**N11** = [What's new in .NET 11 libraries](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/libraries).

| Go package | What it provides | .NET 10 status | Tier | Evidence | Roster row |
|---|---|---|---|---|---|
| `crypto/ed25519` | Ed25519 signatures, with the Ed25519ph and Ed25519ctx variants | No Ed25519 or EdDSA type; none in the namespace listing, which already shows .NET 11 prerelease types | A | NS; XP (Ed25519 appears only inside composite ML-DSA rows); [Q&A on Ed25519/X25519 support](https://learn.microsoft.com/en-us/answers/questions/1696656/when-will-microsoft-support-ed25519-x25519-and-sha) | 9 + 1 |
| `crypto/hpke` | RFC 9180 Hybrid Public Key Encryption: DHKEM over X25519 and the NIST curves, ML-KEM-768/1024, and the hybrid KEMs MLKEM768X25519, MLKEM768P256, MLKEM1024P384; HKDF or SHAKE KDFs; AES-GCM or ChaCha20-Poly1305 | No HPKE type in .NET 10. N11 announces experimental HPKE APIs for .NET 11 (type names and platform notes not read: UNVERIFIED) | A | NS; N10; N11 | 231 |
| `crypto/ecdh` (X25519) | X25519 key agreement | No X25519 type in .NET 10. `X25519DiffieHellman` (CNG and OpenSSL backed) arrives with .NET 11 | A (for .NET 10) | XP; N11; [X25519DiffieHellmanOpenSsl](https://learn.microsoft.com/zh-cn/dotnet/api/system.security.cryptography.x25519diffiehellmanopenssl?view=net-11.0) | 47 |
| `crypto/mlkem` | ML-KEM-768 and ML-KEM-1024 (FIPS 203) | `MLKem` exists; supported only on "Windows 11 Insiders (Latest)" and Linux with OpenSSL 3.5.0+; not macOS, iOS, Android or browser. Some members are `[Experimental]` | B | XP; N10; [MLKem](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.mlkem?view=net-10.0) | 8 |
| `crypto/sha3` | SHA3-224/256/384/512, SHAKE128/256, cSHAKE128/256 | `SHA3_256/384/512`, `Shake128/256` need Windows 11 build 25324+ or OpenSSL 1.1.1+; not macOS, iOS, Android or browser. No SHA3-224 and no cSHAKE type | B (A for SHA3-224 and cSHAKE) | XP; NS | 54 + 5 |
| `vendor/golang.org/x/crypto/chacha20poly1305` | ChaCha20-Poly1305 and XChaCha20-Poly1305 (24-byte nonce) | `ChaCha20Poly1305` needs Windows 10 build 20142+, OpenSSL 1.1.0+ or Android API 28+; not browser. No XChaCha type | B (A for XChaCha) | XP; NS | no row (its tests are not banked); published as `go.vendor.golang.org.x.crypto.chacha20poly1305` |
| `crypto/cipher` (CTR) | AES-CTR as a stream | `CipherMode` has no CTR; a Microsoft Q&A answer says the built-in AES class does not implement CTR | A | [CipherMode](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.ciphermode?view=net-8.0); [Q&A on AES-CTR](https://learn.microsoft.com/en-us/answers/questions/385674/does-net-framework-support-aes-256-ctr-mode) | 27289 |
| `crypto/rc4` | RC4 (legacy, for interop only) | No type | A | NS; XP | 75 |
| `crypto/dsa` | DSA (deprecated in Go) | Key creation fails on macOS; no iOS support | B | XP | 4 |
| `crypto/tls` | Managed TLS 1.2/1.3 client and server; Go 1.26 offers X25519MLKEM768, SecP256r1MLKEM768 and SecP384r1MLKEM1024 ahead of the classical groups by default | `SslStream` uses the OS TLS stack. Schannel's ML-KEM groups are Insider-only and off by default; a Microsoft moderator answer (2026-02-12) says Schannel does not yet use hybrid-PQC key exchange. Linux with OpenSSL 3.5: UNVERIFIED | C | N10; [TLS supported groups, Windows 11 24H2+](https://learn.microsoft.com/en-us/windows/win32/secauthn/tls-supported-groups-in-windows-11-24h2-and-later); [Q&A](https://learn.microsoft.com/en-sg/answers/questions/5769793/we-are-using-c-net-server-and-client-with-tls-1-3) | 1393 + 2; the BoringSSL suite is withdrawn as a host limit, so do not claim it |
| `crypto/cipher` (GCM) | AES-GCM with a custom nonce size and a random-nonce mode | `AesGcm` works except in the browser; the tag is 128-bit only on Apple; 12-byte nonces only | C | XP; [AesGcm.NonceByteSizes](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm.noncebytesizes?view=net-9.0) | 27289 |
| `crypto/x509` | Certificates, CSRs and CRLs, including Ed25519 keys | `X509Certificate2` exists; no Ed25519 keys (see above). Not compared in depth | C | NS | 540 |
| `crypto/pbkdf2` | PBKDF2 over any hash | `Rfc2898DeriveBytes.Pbkdf2` accepts SHA-1/256/384/512 only | Not a gap | [Rfc2898DeriveBytes.Pbkdf2](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rfc2898derivebytes.pbkdf2?view=net-10.0) | 5 |
| `crypto/hkdf` | RFC 5869 HKDF | `HKDF` class (not in the browser) | Not a gap | [HKDF](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hkdf?view=net-10.0) | 3 |
| `crypto/ecdsa` | ECDSA over the NIST curves | Supported on every OS | Not a gap | XP | 125 |
| `crypto/ecdh` (NIST curves) | ECDH over P-256/384/521 | `ECDiffieHellman`, every OS | Not a gap | XP | 47 |
| `crypto/cipher` (CBC, CFB) | Block-cipher modes | AES-CBC and AES-CFB8/128 on every OS | Not a gap | XP | 27289 |
| `crypto/cipher` (OFB) | OFB as a stream | `CipherMode.OFB` exists in the enum; XP has no OFB row | UNVERIFIED | CipherMode; XP | 27289 |

ML-DSA is not claimed: Go 1.26 has it only as `crypto/internal/fips140/mldsa`, an internal package, while .NET 10
ships `MLDsa`, `SlhDsa` and `CompositeMLDsa` (experimental and platform-dependent).

## Everything else

| Go package | What it provides | .NET 10 status | Tier | Evidence | Roster row |
|---|---|---|---|---|---|
| `image/png`, `image/jpeg`, `image/gif`, `image/draw` | Managed decode and encode (animated GIF via `EncodeAll`); Porter-Duff compositing | No image codec in the BCL. System.Drawing.Common is Windows-only since .NET 6 and Microsoft points to third-party libraries; WPF imaging is Windows desktop only | B (A for a cross-platform BCL codec) | [System.Drawing.Common only supported on Windows](https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only) | png 28, jpeg 19, gif 28, draw 9 |
| `math/big` (`Float`, `Rat`) | Arbitrary-precision binary floating point with settable precision and six rounding modes; exact rationals | Only `BigInteger`; "Numerics in .NET" lists no arbitrary-precision float or rational | A | [Numerics in .NET](https://learn.microsoft.com/en-us/dotnet/standard/numerics); [BigInteger](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.biginteger?view=net-10.0) | 248 + 3 |
| `go/parser`, `go/ast`, `go/types`, `go/format`, `go/printer`, `go/doc` | Go's parser, type checker, gofmt and doc extraction, callable from .NET | Nothing comparable (Roslyn covers C# and Visual Basic) | A | none needed: the absence is of a Go toolchain | parser 183, types 597, ast 36, format 4, printer 45, doc 90 |
| `text/template` | A data-driven template engine that parses templates at run time | T4 runtime templates are preprocessed at design time in Visual Studio; no runtime engine in the BCL found | A | [Run-time text generation with T4](https://learn.microsoft.com/en-us/visualstudio/modeling/run-time-text-generation-with-t4-text-templates?view=visualstudio) | 53 |
| `html/template` | Contextual auto-escaping across HTML, JavaScript, CSS and URL contexts | No BCL equivalent found (Razor is ASP.NET Core, not the BCL, and was not compared) | A | same T4 page | 265 |
| `debug/elf`, `debug/macho`, `debug/dwarf` | ELF, Mach-O and DWARF readers | `System.Reflection.PortableExecutable` reads PE/COFF only | A | [System.Reflection.PortableExecutable](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.portableexecutable?view=net-11.0-pp) | elf 32, macho 7, dwarf 41 |
| `compress/bzip2` | bzip2 decompression (Go's package is decode-only) | System.IO.Compression has Brotli, Deflate, GZip, ZLib and Zip; no bzip2 | A | [System.IO.Compression](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression?view=net-10.0) | 4 |
| `compress/lzw` | LZW reader and writer, both bit orders | No LZW type | A | same | 17 |
| `index/suffixarray` | Suffix array with substring and regexp lookup | No type found | A | search only | 12 |
| `mime/quotedprintable` | Quoted-printable reader and writer | `System.Net.Mime` has only the `TransferEncoding.QuotedPrintable` enum value | A | [System.Net.Mime](https://learn.microsoft.com/en-us/dotnet/api/system.net.mime?view=net-10.0) | 5 |
| `encoding/base32` | RFC 4648 Base32 (standard and hex alphabets) | No public Base32 API found | A | search only | 26 |
| `encoding/ascii85` | Ascii85 | No type found | A | search only | 9 |
| `hash/fnv` | FNV-1 and FNV-1a at 32, 64 and 128 bits | Not in System.IO.Hashing's listing (Adler32, Crc32/64, XxHash) | A | [System.IO.Hashing](https://learn.microsoft.com/en-us/dotnet/api/system.io.hashing?view=net-10.0-pp) | 62 |
| `encoding/gob` | Go's self-describing binary serialization | No equivalent; useful for exchanging data with Go programs | A | none | 106 |
| `text/scanner` | A Go-syntax tokenizer | No general tokenizer in the BCL (not searched specifically) | A (minor) | none | 18 |
| `testing/quick` | Property-based testing (frozen in Go) | Third-party only (FsCheck, CsCheck) | A (minor) | none | 8 |
| `container/ring` | A circular list | `LinkedList<T>` is not circular | A (trivial) | none | 8 |
| `hash/crc64` | ISO and ECMA tables, custom tables | `System.IO.Hashing.Crc64` (an out-of-band NuGet package) is ECMA-182 only and states it is not compatible with ISO 3309; a custom-polynomial parameter set appears only in the .NET 11 prerelease docs | C | [Crc64](https://learn.microsoft.com/en-us/dotnet/api/system.io.hashing.crc64?view=net-10.0-pp) | 12 |
| `hash/crc32` | IEEE, Castagnoli and Koopman tables | `System.IO.Hashing.Crc32` is IEEE; `BitOperations.Crc32C` is a step function only; no Koopman | C (weak) | [Crc32](https://learn.microsoft.com/en-us/dotnet/api/system.io.hashing.crc32?view=net-10.0-pp); [BitOperations.Crc32C](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.bitoperations.crc32c?view=net-10.0) | 17 |
| `regexp` | RE2 syntax and semantics, linear time in the input | `RegexOptions.NonBacktracking` (.NET 7+) also guarantees linear time, without backreferences, lookarounds or atomic groups (which Go lacks too). The difference is exact RE2/Go syntax and match semantics, not a missing capability | C | [Regular expression options](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options) | 45 |
| `net/mail` | RFC 5322 message, header, date and address-list parsing | `MailAddress` parses single addresses and documents groups as unsupported; no message parser found | C | [MailAddress](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.mailaddress?view=net-10.0) | 11 |
| `net/smtp` | A small SMTP client | Microsoft does not recommend `SmtpClient` for new development; Go's `net/smtp` is frozen too | C (weak) | [SmtpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient?view=net-10.0) | 20 |
| `net/netip` | Small comparable address, address-port and prefix value types | `IPAddress` plus `IPNetwork` (.NET 8+): a different shape, not a missing capability | C (weak) | [IPNetwork](https://learn.microsoft.com/en-us/dotnet/api/system.net.ipnetwork?view=net-8.0) | 211 + 57 |
| `archive/tar` | tar | `System.Formats.Tar` (.NET 7+) | Not a gap | [TarWriter](https://learn.microsoft.com/en-us/dotnet/api/system.formats.tar.tarwriter?view=net-10.0) | 98 |
| `archive/zip` | zip | `ZipArchive`, `ZipFile` | Not a gap | System.IO.Compression | 101 |
| `container/heap` | Heap algorithms | `PriorityQueue<TElement,TPriority>` | Not a gap | [PriorityQueue](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.priorityqueue-2?view=net-10.0) | 7 |
| `debug/pe` | PE reader | `System.Reflection.PortableExecutable` | Not a gap | as above | 10 |
| `math/big` (`Int`) | Arbitrary-precision integers | `BigInteger` | Not a gap | as above | 248 + 3 |
| `hash/adler32` | Adler-32 | `Adler32` appears in System.IO.Hashing only on .NET 11 prerelease pages | UNVERIFIED | [Adler32 (11.0 prerelease)](https://learn.microsoft.com/zh-cn/dotnet/api/system.io.hashing.adler32.-ctor?view=net-11.0-pp) | 9 |
| `mime/multipart` | Multipart reader and writer | `MultipartFormDataContent` writes multipart; a reader was not checked | UNVERIFIED | not checked | 53 |

## Top five

1. **Ed25519 (`crypto/ed25519`, A).** .NET 10 has no Ed25519 or EdDSA type, and the namespace listing that already
   shows .NET 11 prerelease types has none either. 9 tests pass converted, 1 disclosed.
2. **HPKE (`crypto/hpke`, A).** RFC 9180 with post-quantum hybrid KEMs, absent from .NET 10; .NET 11 announces an
   experimental HPKE. 231 tests pass converted.
3. **ML-KEM and SHA-3 on every OS (`crypto/mlkem`, `crypto/sha3`, B).** .NET 10's `MLKem` needs Windows 11
   Insiders or OpenSSL 3.5+, and its SHA-3 and SHAKE need a recent Windows 11 or OpenSSL; neither runs on macOS,
   iOS, Android or in the browser. SHA3-224 and cSHAKE are absent outright. The converted Go is managed code.
4. **Cross-platform image codecs (`image/png`, `image/jpeg`, `image/gif`, B/A).** The BCL has none, and
   System.Drawing.Common is Windows-only.
5. **Arbitrary-precision `Float` and `Rat` (`math/big`, A).** .NET has `BigInteger` only.

Honorable mentions: Go's parser and type checker (`go/types`, 597 tests); `html/template`'s contextual
auto-escaping; ELF, Mach-O and DWARF readers; X25519 and XChaCha20-Poly1305 for .NET 10 (X25519 arrives in .NET 11).

## Do not overclaim

- `regexp`: .NET 7+ `NonBacktracking` is also linear-time. Claim RE2 compatibility, not "the only linear-time
  engine".
- `crypto/tls`: the banked row is 1,393 + 2, and the BoringSSL suite is withdrawn as a host limit; do not claim the
  full TLS suite passes.
- X25519: say that .NET 11 adds it.
- HKDF, PBKDF2, ECDSA, NIST-curve ECDH, AES-CBC/CFB, AES-GCM outside the browser, tar, zip, a priority queue,
  a PE reader and `BigInteger`-style integers are **not** gaps.

## Draft announcement paragraph

> go2cs 1.26.10.1 brings Go's standard library to .NET as managed code that behaves the same on Windows, Linux and
> macOS. That includes cryptography the .NET 10 base library does not have: Ed25519 signatures and RFC 9180 Hybrid
> Public Key Encryption with post-quantum hybrid KEMs. It also includes ML-KEM, SHA-3 and SHAKE that do not depend
> on a particular Windows build or OpenSSL version and run on macOS too. Beyond cryptography, you get PNG, JPEG and
> GIF encoding and decoding without System.Drawing, arbitrary-precision `big.Float` and `big.Rat`, and Go's own
> parser and type checker. Each of these packages is validated against Go's own tests after conversion.
