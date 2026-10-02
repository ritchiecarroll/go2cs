# Validation proofs

One page per validated package — the full per-test differential behind its row in
[Validated Test Packages](../ValidatedTestPackages.md): every eligible `Test` function, `go test`'s
verdict and go2cs's, side by side. The converter writes these pages itself, from the same
comparison record that decides whether a package validates at all.

Pages under `current/` are living proof — regenerated only when a package's verdicts change.
Versioned sibling directories are frozen publication snapshots: written once at release and never
rewritten, so the proof link for a published package stays the proof as of that binary.

## Frozen snapshots

One directory per published release. The page counts below are exact and cannot go stale: a frozen
directory is written once and never rewritten, so each number is a permanent statement of how many
proof pages that build shipped with. From 1.24.13.1 on, a snapshot also freezes the pages of excluded
packages and of import paths a Go release retired, so its page count can exceed the validated count on
the roster page beside it (1.24.13.1: 232 pages, 218 validated).

| Release | Snapshot | Proof pages | Roster as it stood |
|:--|:--|--:|:--|
| 1.23.1.2 | [`1.23.1.2/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.23.1.2) | 72 | — |
| 1.23.1.3 | [`1.23.1.3/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.23.1.3) | 73 | — |
| 1.23.1.4 | [`1.23.1.4/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.23.1.4) | 110 | — |
| 1.23.1.5 | [`1.23.1.5/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.23.1.5) | 110 | — |
| 1.23.1.6 | [`1.23.1.6/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.23.1.6) | 126 | — |
| 1.23.1.7 | [`1.23.1.7/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.23.1.7) | 162 | — |
| 1.23.12.1 | [`1.23.12.1/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.23.12.1) | 172 | — |
| 1.23.12.2 | [`1.23.12.2/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.23.12.2) | 189 | — |
| 1.23.12.3 | [`1.23.12.3/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.23.12.3) | 204 | [`ValidatedTestPackages.md`](1.23.12.3/ValidatedTestPackages.md) |
| 1.24.13.1 | [`1.24.13.1/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.24.13.1) | 232 | [`ValidatedTestPackages.md`](1.24.13.1/ValidatedTestPackages.md) |
| 1.24.13.2 | [`1.24.13.2/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.24.13.2) | 232 | [`ValidatedTestPackages.md`](1.24.13.2/ValidatedTestPackages.md) |
| 1.24.13.3 | [`1.24.13.3/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.24.13.3) | 239 | [`ValidatedTestPackages.md`](1.24.13.3/ValidatedTestPackages.md) |

A snapshot froze the per-package proofs and not the roster PAGE around them until 1.23.12.3, so for
every release above it the campaign's own "how things stood" view lives only in the signed git tag.
`git show nuget-<version>` — `nuget-1.23.12.3`, `nuget-1.23.1.7` — reaches the exact source tree a
release was built from, roster included. From 1.23.12.3 on the snapshot carries that page itself, and
[`src/push-nuget.ps1`](https://github.com/ritchiecarroll/go2cs/blob/master/src/push-nuget.ps1) writes
it beside the proofs on every release.

| Package | Proof | Converted package |
|:--|:--|:--|
| `archive/tar` | [`archive.tar.md`](current/archive.tar.md) | [`src/core/archive/tar`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/archive/tar) |
| `archive/zip` | [`archive.zip.md`](current/archive.zip.md) | [`src/core/archive/zip`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/archive/zip) |
| `bufio` | [`bufio.md`](current/bufio.md) | [`src/core/bufio`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/bufio) |
| `bytes` | [`bytes.md`](current/bytes.md) | [`src/core/bytes`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/bytes) |
| `cmp` | [`cmp.md`](current/cmp.md) | [`src/core/cmp`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/cmp) |
| `compress/bzip2` | [`compress.bzip2.md`](current/compress.bzip2.md) | [`src/core/compress/bzip2`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/bzip2) |
| `compress/flate` | [`compress.flate.md`](current/compress.flate.md) | [`src/core/compress/flate`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/flate) |
| `compress/gzip` | [`compress.gzip.md`](current/compress.gzip.md) | [`src/core/compress/gzip`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/gzip) |
| `compress/lzw` | [`compress.lzw.md`](current/compress.lzw.md) | [`src/core/compress/lzw`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/lzw) |
| `compress/zlib` | [`compress.zlib.md`](current/compress.zlib.md) | [`src/core/compress/zlib`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/zlib) |
| `container/heap` | [`container.heap.md`](current/container.heap.md) | [`src/core/container/heap`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/container/heap) |
| `container/list` | [`container.list.md`](current/container.list.md) | [`src/core/container/list`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/container/list) |
| `container/ring` | [`container.ring.md`](current/container.ring.md) | [`src/core/container/ring`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/container/ring) |
| `context` | [`context.md`](current/context.md) | [`src/core/context`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/context) |
| `crypto` | [`crypto.md`](current/crypto.md) | [`src/core/crypto`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto) |
| `crypto/aes` | [`crypto.aes.md`](current/crypto.aes.md) | [`src/core/crypto/aes`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/aes) |
| `crypto/cipher` | [`crypto.cipher.md`](current/crypto.cipher.md) | [`src/core/crypto/cipher`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/cipher) |
| `crypto/des` | [`crypto.des.md`](current/crypto.des.md) | [`src/core/crypto/des`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/des) |
| `crypto/dsa` | [`crypto.dsa.md`](current/crypto.dsa.md) | [`src/core/crypto/dsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/dsa) |
| `crypto/ecdh` | [`crypto.ecdh.md`](current/crypto.ecdh.md) | [`src/core/crypto/ecdh`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/ecdh) |
| `crypto/ecdsa` | [`crypto.ecdsa.md`](current/crypto.ecdsa.md) | [`src/core/crypto/ecdsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/ecdsa) |
| `crypto/ed25519` | [`crypto.ed25519.md`](current/crypto.ed25519.md) | [`src/core/crypto/ed25519`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/ed25519) |
| `crypto/elliptic` | [`crypto.elliptic.md`](current/crypto.elliptic.md) | [`src/core/crypto/elliptic`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/elliptic) |
| `crypto/hkdf` | [`crypto.hkdf.md`](current/crypto.hkdf.md) | [`src/core/crypto/hkdf`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/hkdf) |
| `crypto/hmac` | [`crypto.hmac.md`](current/crypto.hmac.md) | [`src/core/crypto/hmac`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/hmac) |
| `crypto/internal/boring` | [`crypto.internal.boring.md`](current/crypto.internal.boring.md) | [`src/core/crypto/internal/boring`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/boring) |
| `crypto/internal/boring/bcache` | [`crypto.internal.boring.bcache.md`](current/crypto.internal.boring.bcache.md) | [`src/core/crypto/internal/boring/bcache`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/boring/bcache) |
| `crypto/internal/fips140/aes` | [`crypto.internal.fips140.aes.md`](current/crypto.internal.fips140.aes.md) | [`src/core/crypto/internal/fips140/aes`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/aes) |
| `crypto/internal/fips140/bigmod` | [`crypto.internal.fips140.bigmod.md`](current/crypto.internal.fips140.bigmod.md) | [`src/core/crypto/internal/fips140/bigmod`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/bigmod) |
| `crypto/internal/fips140/ecdh` | [`crypto.internal.fips140.ecdh.md`](current/crypto.internal.fips140.ecdh.md) | [`src/core/crypto/internal/fips140/ecdh`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/ecdh) |
| `crypto/internal/fips140/ecdsa` | [`crypto.internal.fips140.ecdsa.md`](current/crypto.internal.fips140.ecdsa.md) | [`src/core/crypto/internal/fips140/ecdsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/ecdsa) |
| `crypto/internal/fips140/edwards25519` | [`crypto.internal.fips140.edwards25519.md`](current/crypto.internal.fips140.edwards25519.md) | [`src/core/crypto/internal/fips140/edwards25519`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/edwards25519) |
| `crypto/internal/fips140/edwards25519/field` | [`crypto.internal.fips140.edwards25519.field.md`](current/crypto.internal.fips140.edwards25519.field.md) | [`src/core/crypto/internal/fips140/edwards25519/field`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/edwards25519/field) |
| `crypto/internal/fips140/mlkem` | [`crypto.internal.fips140.mlkem.md`](current/crypto.internal.fips140.mlkem.md) | [`src/core/crypto/internal/fips140/mlkem`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/mlkem) |
| `crypto/internal/fips140/nistec` | [`crypto.internal.fips140.nistec.md`](current/crypto.internal.fips140.nistec.md) | [`src/core/crypto/internal/fips140/nistec`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/nistec) |
| `crypto/internal/fips140/rsa` | [`crypto.internal.fips140.rsa.md`](current/crypto.internal.fips140.rsa.md) | [`src/core/crypto/internal/fips140/rsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/rsa) |
| `crypto/internal/fips140test` | [`crypto.internal.fips140test.md`](current/crypto.internal.fips140test.md) | [`src/core/crypto/internal/fips140test`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140test) |
| `crypto/internal/hpke` | [`crypto.internal.hpke.md`](current/crypto.internal.hpke.md) | [`src/core/crypto/internal/hpke`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/hpke) |
| `crypto/internal/sysrand` | [`crypto.internal.sysrand.md`](current/crypto.internal.sysrand.md) | [`src/core/crypto/internal/sysrand`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/sysrand) |
| `crypto/md5` | [`crypto.md5.md`](current/crypto.md5.md) | [`src/core/crypto/md5`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/md5) |
| `crypto/mlkem` | [`crypto.mlkem.md`](current/crypto.mlkem.md) | [`src/core/crypto/mlkem`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/mlkem) |
| `crypto/pbkdf2` | [`crypto.pbkdf2.md`](current/crypto.pbkdf2.md) | [`src/core/crypto/pbkdf2`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/pbkdf2) |
| `crypto/rand` | [`crypto.rand.md`](current/crypto.rand.md) | [`src/core/crypto/rand`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/rand) |
| `crypto/rc4` | [`crypto.rc4.md`](current/crypto.rc4.md) | [`src/core/crypto/rc4`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/rc4) |
| `crypto/rsa` | [`crypto.rsa.md`](current/crypto.rsa.md) | [`src/core/crypto/rsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/rsa) |
| `crypto/sha1` | [`crypto.sha1.md`](current/crypto.sha1.md) | [`src/core/crypto/sha1`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/sha1) |
| `crypto/sha256` | [`crypto.sha256.md`](current/crypto.sha256.md) | [`src/core/crypto/sha256`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/sha256) |
| `crypto/sha3` | [`crypto.sha3.md`](current/crypto.sha3.md) | [`src/core/crypto/sha3`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/sha3) |
| `crypto/sha512` | [`crypto.sha512.md`](current/crypto.sha512.md) | [`src/core/crypto/sha512`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/sha512) |
| `crypto/subtle` | [`crypto.subtle.md`](current/crypto.subtle.md) | [`src/core/crypto/subtle`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/subtle) |
| `crypto/tls` | [`crypto.tls.md`](current/crypto.tls.md) | [`src/core/crypto/tls`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/tls) |
| `crypto/x509` | [`crypto.x509.md`](current/crypto.x509.md) | [`src/core/crypto/x509`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/x509) |
| `database/sql` | [`database.sql.md`](current/database.sql.md) | [`src/core/database/sql`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/database/sql) |
| `database/sql/driver` | [`database.sql.driver.md`](current/database.sql.driver.md) | [`src/core/database/sql/driver`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/database/sql/driver) |
| `debug/buildinfo` | [`debug.buildinfo.md`](current/debug.buildinfo.md) | [`src/core/debug/buildinfo`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/buildinfo) |
| `debug/dwarf` | [`debug.dwarf.md`](current/debug.dwarf.md) | [`src/core/debug/dwarf`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/dwarf) |
| `debug/elf` | [`debug.elf.md`](current/debug.elf.md) | [`src/core/debug/elf`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/elf) |
| `debug/gosym` | [`debug.gosym.md`](current/debug.gosym.md) | [`src/core/debug/gosym`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/gosym) |
| `debug/macho` | [`debug.macho.md`](current/debug.macho.md) | [`src/core/debug/macho`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/macho) |
| `debug/pe` | [`debug.pe.md`](current/debug.pe.md) | [`src/core/debug/pe`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/pe) |
| `debug/plan9obj` | [`debug.plan9obj.md`](current/debug.plan9obj.md) | [`src/core/debug/plan9obj`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/plan9obj) |
| `embed/internal/embedtest` | [`embed.internal.embedtest.md`](current/embed.internal.embedtest.md) | [`src/core/embed/internal/embedtest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/embed/internal/embedtest) |
| `encoding/ascii85` | [`encoding.ascii85.md`](current/encoding.ascii85.md) | [`src/core/encoding/ascii85`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/ascii85) |
| `encoding/asn1` | [`encoding.asn1.md`](current/encoding.asn1.md) | [`src/core/encoding/asn1`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/asn1) |
| `encoding/base32` | [`encoding.base32.md`](current/encoding.base32.md) | [`src/core/encoding/base32`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/base32) |
| `encoding/base64` | [`encoding.base64.md`](current/encoding.base64.md) | [`src/core/encoding/base64`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/base64) |
| `encoding/binary` | [`encoding.binary.md`](current/encoding.binary.md) | [`src/core/encoding/binary`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/binary) |
| `encoding/csv` | [`encoding.csv.md`](current/encoding.csv.md) | [`src/core/encoding/csv`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/csv) |
| `encoding/gob` | [`encoding.gob.md`](current/encoding.gob.md) | [`src/core/encoding/gob`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/gob) |
| `encoding/hex` | [`encoding.hex.md`](current/encoding.hex.md) | [`src/core/encoding/hex`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/hex) |
| `encoding/json` | [`encoding.json.md`](current/encoding.json.md) | [`src/core/encoding/json`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/json) |
| `encoding/xml` | [`encoding.xml.md`](current/encoding.xml.md) | [`src/core/encoding/xml`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/xml) |
| `encoding/pem` | [`encoding.pem.md`](current/encoding.pem.md) | [`src/core/encoding/pem`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/pem) |
| `errors` | [`errors.md`](current/errors.md) | [`src/core/errors`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/errors) |
| `expvar` | [`expvar.md`](current/expvar.md) | [`src/core/expvar`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/expvar) |
| `flag` | [`flag.md`](current/flag.md) | [`src/core/flag`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/flag) |
| `fmt` | [`fmt.md`](current/fmt.md) | [`src/core/fmt`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/fmt) |
| `go/ast` | [`go.ast.md`](current/go.ast.md) | [`src/core/go/ast`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/ast) |
| `go/ast/internal/tests` | [`go.ast.internal.tests.md`](current/go.ast.internal.tests.md) | [`src/core/go/ast/internal/tests`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/ast/internal/tests) |
| `go/build` | [`go.build.md`](current/go.build.md) | [`src/core/go/build`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/build) |
| `go/build/constraint` | [`go.build.constraint.md`](current/go.build.constraint.md) | [`src/core/go/build/constraint`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/build/constraint) |
| `go/constant` | [`go.constant.md`](current/go.constant.md) | [`src/core/go/constant`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/constant) |
| `go/doc` | [`go.doc.md`](current/go.doc.md) | [`src/core/go/doc`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/doc) |
| `go/doc/comment` | [`go.doc.comment.md`](current/go.doc.comment.md) | [`src/core/go/doc/comment`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/doc/comment) |
| `go/format` | [`go.format.md`](current/go.format.md) | [`src/core/go/format`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/format) |
| `go/importer` | [`go.importer.md`](current/go.importer.md) | [`src/core/go/importer`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/importer) |
| `go/internal/gccgoimporter` | [`go.internal.gccgoimporter.md`](current/go.internal.gccgoimporter.md) | [`src/core/go/internal/gccgoimporter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/internal/gccgoimporter) |
| `go/internal/gcimporter` | [`go.internal.gcimporter.md`](current/go.internal.gcimporter.md) | [`src/core/go/internal/gcimporter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/internal/gcimporter) |
| `go/internal/srcimporter` | [`go.internal.srcimporter.md`](current/go.internal.srcimporter.md) | [`src/core/go/internal/srcimporter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/internal/srcimporter) |
| `go/parser` | [`go.parser.md`](current/go.parser.md) | [`src/core/go/parser`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/parser) |
| `go/printer` | [`go.printer.md`](current/go.printer.md) | [`src/core/go/printer`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/printer) |
| `go/scanner` | [`go.scanner.md`](current/go.scanner.md) | [`src/core/go/scanner`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/scanner) |
| `go/token` | [`go.token.md`](current/go.token.md) | [`src/core/go/token`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/token) |
| `go/types` | [`go.types.md`](current/go.types.md) | [`src/core/go/types`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/types) |
| `go/version` | [`go.version.md`](current/go.version.md) | [`src/core/go/version`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/version) |
| `hash` | [`hash.md`](current/hash.md) | [`src/core/hash`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash) |
| `hash/adler32` | [`hash.adler32.md`](current/hash.adler32.md) | [`src/core/hash/adler32`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/adler32) |
| `hash/crc32` | [`hash.crc32.md`](current/hash.crc32.md) | [`src/core/hash/crc32`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/crc32) |
| `hash/crc64` | [`hash.crc64.md`](current/hash.crc64.md) | [`src/core/hash/crc64`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/crc64) |
| `hash/fnv` | [`hash.fnv.md`](current/hash.fnv.md) | [`src/core/hash/fnv`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/fnv) |
| `hash/maphash` | [`hash.maphash.md`](current/hash.maphash.md) | [`src/core/hash/maphash`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/maphash) |
| `html` | [`html.md`](current/html.md) | [`src/core/html`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/html) |
| `html/template` | [`html.template.md`](current/html.template.md) | [`src/core/html/template`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/html/template) |
| `image` | [`image.md`](current/image.md) | [`src/core/image`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image) |
| `image/color` | [`image.color.md`](current/image.color.md) | [`src/core/image/color`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/color) |
| `image/draw` | [`image.draw.md`](current/image.draw.md) | [`src/core/image/draw`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/draw) |
| `image/gif` | [`image.gif.md`](current/image.gif.md) | [`src/core/image/gif`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/gif) |
| `image/jpeg` | [`image.jpeg.md`](current/image.jpeg.md) | [`src/core/image/jpeg`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/jpeg) |
| `image/png` | [`image.png.md`](current/image.png.md) | [`src/core/image/png`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/png) |
| `index/suffixarray` | [`index.suffixarray.md`](current/index.suffixarray.md) | [`src/core/index/suffixarray`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/index/suffixarray) |
| `internal/abi` | [`internal.abi.md`](current/internal.abi.md) | [`src/core/internal/abi`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/abi) |
| `internal/buildcfg` | [`internal.buildcfg.md`](current/internal.buildcfg.md) | [`src/core/internal/buildcfg`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/buildcfg) |
| `internal/chacha8rand` | [`internal.chacha8rand.md`](current/internal.chacha8rand.md) | [`src/core/internal/chacha8rand`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/chacha8rand) |
| `internal/coverage/cfile` | [`internal.coverage.cfile.md`](current/internal.coverage.cfile.md) | [`src/core/internal/coverage/cfile`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/cfile) |
| `internal/coverage/cformat` | [`internal.coverage.cformat.md`](current/internal.coverage.cformat.md) | [`src/core/internal/coverage/cformat`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/cformat) |
| `internal/coverage/cmerge` | [`internal.coverage.cmerge.md`](current/internal.coverage.cmerge.md) | [`src/core/internal/coverage/cmerge`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/cmerge) |
| `internal/coverage/pods` | [`internal.coverage.pods.md`](current/internal.coverage.pods.md) | [`src/core/internal/coverage/pods`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/pods) |
| `internal/coverage/slicereader` | [`internal.coverage.slicereader.md`](current/internal.coverage.slicereader.md) | [`src/core/internal/coverage/slicereader`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/slicereader) |
| `internal/coverage/slicewriter` | [`internal.coverage.slicewriter.md`](current/internal.coverage.slicewriter.md) | [`src/core/internal/coverage/slicewriter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/slicewriter) |
| `internal/coverage/test` | [`internal.coverage.test.md`](current/internal.coverage.test.md) | [`src/core/internal/coverage/test`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/test) |
| `internal/cpu` | [`internal.cpu.md`](current/internal.cpu.md) | [`src/core/internal/cpu`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/cpu) |
| `internal/dag` | [`internal.dag.md`](current/internal.dag.md) | [`src/core/internal/dag`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/dag) |
| `internal/diff` | [`internal.diff.md`](current/internal.diff.md) | [`src/core/internal/diff`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/diff) |
| `internal/fmtsort` | [`internal.fmtsort.md`](current/internal.fmtsort.md) | [`src/core/internal/fmtsort`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/fmtsort) |
| `internal/fuzz` | [`internal.fuzz.md`](current/internal.fuzz.md) | [`src/core/internal/fuzz`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/fuzz) |
| `internal/godebug` | [`internal.godebug.md`](current/internal.godebug.md) | [`src/core/internal/godebug`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/godebug) |
| `internal/godebugs` | [`internal.godebugs.md`](current/internal.godebugs.md) | [`src/core/internal/godebugs`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/godebugs) |
| `internal/gover` | [`internal.gover.md`](current/internal.gover.md) | [`src/core/internal/gover`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/gover) |
| `internal/itoa` | [`internal.itoa.md`](current/internal.itoa.md) | [`src/core/internal/itoa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/itoa) |
| `internal/pkgbits` | [`internal.pkgbits.md`](current/internal.pkgbits.md) | [`src/core/internal/pkgbits`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/pkgbits) |
| `internal/platform` | [`internal.platform.md`](current/internal.platform.md) | [`src/core/internal/platform`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/platform) |
| `internal/poll` | [`internal.poll.md`](current/internal.poll.md) | [`src/core/internal/poll`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/poll) |
| `internal/profile` | [`internal.profile.md`](current/internal.profile.md) | [`src/core/internal/profile`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/profile) |
| `internal/reflectlite` | [`internal.reflectlite.md`](current/internal.reflectlite.md) | [`src/core/internal/reflectlite`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/reflectlite) |
| `internal/runtime/atomic` | [`internal.runtime.atomic.md`](current/internal.runtime.atomic.md) | [`src/core/internal/runtime/atomic`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/runtime/atomic) |
| `internal/runtime/maps` | [`internal.runtime.maps.md`](current/internal.runtime.maps.md) | [`src/core/internal/runtime/maps`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/runtime/maps) |
| `internal/runtime/math` | [`internal.runtime.math.md`](current/internal.runtime.math.md) | [`src/core/internal/runtime/math`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/runtime/math) |
| `internal/runtime/sys` | [`internal.runtime.sys.md`](current/internal.runtime.sys.md) | [`src/core/internal/runtime/sys`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/runtime/sys) |
| `internal/saferio` | [`internal.saferio.md`](current/internal.saferio.md) | [`src/core/internal/saferio`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/saferio) |
| `internal/singleflight` | [`internal.singleflight.md`](current/internal.singleflight.md) | [`src/core/internal/singleflight`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/singleflight) |
| `internal/sync` | [`internal.sync.md`](current/internal.sync.md) | [`src/core/internal/sync`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/sync) |
| `internal/synctest` | [`internal.synctest.md`](current/internal.synctest.md) | [`src/core/internal/synctest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/synctest) |
| `internal/syscall/windows` | [`internal.syscall.windows.md`](current/internal.syscall.windows.md) | [`src/core/internal/syscall/windows`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/syscall/windows) |
| `internal/syscall/windows/registry` | [`internal.syscall.windows.registry.md`](current/internal.syscall.windows.registry.md) | [`src/core/internal/syscall/windows/registry`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/syscall/windows/registry) |
| `internal/sysinfo` | [`internal.sysinfo.md`](current/internal.sysinfo.md) | [`src/core/internal/sysinfo`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/sysinfo) |
| `internal/testenv` | [`internal.testenv.md`](current/internal.testenv.md) | [`src/core/internal/testenv`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/testenv) |
| `internal/trace` | [`internal.trace.md`](current/internal.trace.md) | [`src/core/internal/trace`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/trace) |
| `internal/trace/internal/oldtrace` | [`internal.trace.internal.oldtrace.md`](current/internal.trace.internal.oldtrace.md) | [`src/core/internal/trace/internal/oldtrace`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/trace/internal/oldtrace) |
| `internal/types/errors` | [`internal.types.errors.md`](current/internal.types.errors.md) | [`src/core/internal/types/errors`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/types/errors) |
| `internal/xcoff` | [`internal.xcoff.md`](current/internal.xcoff.md) | [`src/core/internal/xcoff`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/xcoff) |
| `internal/zstd` | [`internal.zstd.md`](current/internal.zstd.md) | [`src/core/internal/zstd`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/zstd) |
| `io` | [`io.md`](current/io.md) | [`src/core/io`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/io) |
| `io/fs` | [`io.fs.md`](current/io.fs.md) | [`src/core/io/fs`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/io/fs) |
| `io/ioutil` | [`io.ioutil.md`](current/io.ioutil.md) | [`src/core/io/ioutil`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/io/ioutil) |
| `iter` | [`iter.md`](current/iter.md) | [`src/core/iter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/iter) |
| `log` | [`log.md`](current/log.md) | [`src/core/log`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/log) |
| `log/slog` | [`log.slog.md`](current/log.slog.md) | [`src/core/log/slog`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/log/slog) |
| `log/slog/internal/benchmarks` | [`log.slog.internal.benchmarks.md`](current/log.slog.internal.benchmarks.md) | [`src/core/log/slog/internal/benchmarks`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/log/slog/internal/benchmarks) |
| `log/slog/internal/buffer` | [`log.slog.internal.buffer.md`](current/log.slog.internal.buffer.md) | [`src/core/log/slog/internal/buffer`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/log/slog/internal/buffer) |
| `maps` | [`maps.md`](current/maps.md) | [`src/core/maps`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/maps) |
| `math` | [`math.md`](current/math.md) | [`src/core/math`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math) |
| `math/big` | [`math.big.md`](current/math.big.md) | [`src/core/math/big`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/big) |
| `math/bits` | [`math.bits.md`](current/math.bits.md) | [`src/core/math/bits`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/bits) |
| `math/cmplx` | [`math.cmplx.md`](current/math.cmplx.md) | [`src/core/math/cmplx`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/cmplx) |
| `math/rand` | [`math.rand.md`](current/math.rand.md) | [`src/core/math/rand`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/rand) |
| `math/rand/v2` | [`math.rand.v2.md`](current/math.rand.v2.md) | [`src/core/math/rand/v2`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/rand/v2) |
| `mime` | [`mime.md`](current/mime.md) | [`src/core/mime`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/mime) |
| `mime/multipart` | [`mime.multipart.md`](current/mime.multipart.md) | [`src/core/mime/multipart`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/mime/multipart) |
| `mime/quotedprintable` | [`mime.quotedprintable.md`](current/mime.quotedprintable.md) | [`src/core/mime/quotedprintable`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/mime/quotedprintable) |
| `net` | [`net.md`](current/net.md) | [`src/core/net`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net) |
| `net/http` | [`net.http.md`](current/net.http.md) | [`src/core/net/http`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http) |
| `net/http/cgi` | [`net.http.cgi.md`](current/net.http.cgi.md) | [`src/core/net/http/cgi`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/cgi) |
| `net/http/cookiejar` | [`net.http.cookiejar.md`](current/net.http.cookiejar.md) | [`src/core/net/http/cookiejar`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/cookiejar) |
| `net/http/fcgi` | [`net.http.fcgi.md`](current/net.http.fcgi.md) | [`src/core/net/http/fcgi`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/fcgi) |
| `net/http/httptest` | [`net.http.httptest.md`](current/net.http.httptest.md) | [`src/core/net/http/httptest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/httptest) |
| `net/http/httptrace` | [`net.http.httptrace.md`](current/net.http.httptrace.md) | [`src/core/net/http/httptrace`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/httptrace) |
| `net/http/httputil` | [`net.http.httputil.md`](current/net.http.httputil.md) | [`src/core/net/http/httputil`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/httputil) |
| `net/http/internal` | [`net.http.internal.md`](current/net.http.internal.md) | [`src/core/net/http/internal`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/internal) |
| `net/http/internal/ascii` | [`net.http.internal.ascii.md`](current/net.http.internal.ascii.md) | [`src/core/net/http/internal/ascii`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/internal/ascii) |
| `net/http/pprof` | [`net.http.pprof.md`](current/net.http.pprof.md) | [`src/core/net/http/pprof`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/pprof) |
| `net/mail` | [`net.mail.md`](current/net.mail.md) | [`src/core/net/mail`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/mail) |
| `net/netip` | [`net.netip.md`](current/net.netip.md) | [`src/core/net/netip`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/netip) |
| `net/rpc` | [`net.rpc.md`](current/net.rpc.md) | [`src/core/net/rpc`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/rpc) |
| `net/rpc/jsonrpc` | [`net.rpc.jsonrpc.md`](current/net.rpc.jsonrpc.md) | [`src/core/net/rpc/jsonrpc`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/rpc/jsonrpc) |
| `net/smtp` | [`net.smtp.md`](current/net.smtp.md) | [`src/core/net/smtp`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/smtp) |
| `net/textproto` | [`net.textproto.md`](current/net.textproto.md) | [`src/core/net/textproto`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/textproto) |
| `net/url` | [`net.url.md`](current/net.url.md) | [`src/core/net/url`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/url) |
| `os` | [`os.md`](current/os.md) | [`src/core/os`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os) |
| `os/exec` | [`os.exec.md`](current/os.exec.md) | [`src/core/os/exec`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os/exec) |
| `os/exec/internal/fdtest` | [`os.exec.internal.fdtest.md`](current/os.exec.internal.fdtest.md) | [`src/core/os/exec/internal/fdtest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os/exec/internal/fdtest) |
| `os/signal` | [`os.signal.md`](current/os.signal.md) | [`src/core/os/signal`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os/signal) |
| `os/user` | [`os.user.md`](current/os.user.md) | [`src/core/os/user`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os/user) |
| `path` | [`path.md`](current/path.md) | [`src/core/path`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/path) |
| `path/filepath` | [`path.filepath.md`](current/path.filepath.md) | [`src/core/path/filepath`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/path/filepath) |
| `plugin` | [`plugin.md`](current/plugin.md) | [`src/core/plugin`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/plugin) |
| `reflect` | [`reflect.md`](current/reflect.md) | [`src/core/reflect`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/reflect) |
| `regexp` | [`regexp.md`](current/regexp.md) | [`src/core/regexp`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/regexp) |
| `regexp/syntax` | [`regexp.syntax.md`](current/regexp.syntax.md) | [`src/core/regexp/syntax`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/regexp/syntax) |
| `runtime` | [`runtime.md`](current/runtime.md) | [`src/core/runtime`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/runtime) |
| `runtime/debug` | [`runtime.debug.md`](current/runtime.debug.md) | [`src/core/runtime/debug`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/runtime/debug) |
| `runtime/metrics` | [`runtime.metrics.md`](current/runtime.metrics.md) | [`src/core/runtime/metrics`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/runtime/metrics) |
| `runtime/pprof` | [`runtime.pprof.md`](current/runtime.pprof.md) | [`src/core/runtime/pprof`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/runtime/pprof) |
| `runtime/trace` | [`runtime.trace.md`](current/runtime.trace.md) | [`src/core/runtime/trace`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/runtime/trace) |
| `slices` | [`slices.md`](current/slices.md) | [`src/core/slices`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/slices) |
| `sort` | [`sort.md`](current/sort.md) | [`src/core/sort`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/sort) |
| `strconv` | [`strconv.md`](current/strconv.md) | [`src/core/strconv`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/strconv) |
| `strings` | [`strings.md`](current/strings.md) | [`src/core/strings`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/strings) |
| `sync` | [`sync.md`](current/sync.md) | [`src/core/sync`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/sync) |
| `sync/atomic` | [`sync.atomic.md`](current/sync.atomic.md) | [`src/core/sync/atomic`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/sync/atomic) |
| `syscall` | [`syscall.md`](current/syscall.md) | [`src/core/syscall`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/syscall) |
| `testing` | [`testing.md`](current/testing.md) | [`src/core/testing`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing) |
| `testing/fstest` | [`testing.fstest.md`](current/testing.fstest.md) | [`src/core/testing/fstest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing/fstest) |
| `testing/iotest` | [`testing.iotest.md`](current/testing.iotest.md) | [`src/core/testing/iotest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing/iotest) |
| `testing/quick` | [`testing.quick.md`](current/testing.quick.md) | [`src/core/testing/quick`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing/quick) |
| `testing/slogtest` | [`testing.slogtest.md`](current/testing.slogtest.md) | [`src/core/testing/slogtest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing/slogtest) |
| `text/scanner` | [`text.scanner.md`](current/text.scanner.md) | [`src/core/text/scanner`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/text/scanner) |
| `text/tabwriter` | [`text.tabwriter.md`](current/text.tabwriter.md) | [`src/core/text/tabwriter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/text/tabwriter) |
| `text/template` | [`text.template.md`](current/text.template.md) | [`src/core/text/template`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/text/template) |
| `text/template/parse` | [`text.template.parse.md`](current/text.template.parse.md) | [`src/core/text/template/parse`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/text/template/parse) |
| `time` | [`time.md`](current/time.md) | [`src/core/time`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/time) |
| `unicode` | [`unicode.md`](current/unicode.md) | [`src/core/unicode`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode) |
| `unicode/utf16` | [`unicode.utf16.md`](current/unicode.utf16.md) | [`src/core/unicode/utf16`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode/utf16) |
| `unicode/utf8` | [`unicode.utf8.md`](current/unicode.utf8.md) | [`src/core/unicode/utf8`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode/utf8) |
| `unique` | [`unique.md`](current/unique.md) | [`src/core/unique`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unique) |
| `weak` | [`weak.md`](current/weak.md) | [`src/core/weak`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/weak) |
