# go2cs fork of github.com/zeebo/xxh3 v1.1.0: no `unsafe`

This directory is `github.com/zeebo/xxh3` at v1.1.0 with every use of `unsafe` removed. It exists so that a module
that depends on xxh3 can be converted by go2cs today, which cannot yet convert xxh3's string- and slice-header views
over managed memory (C45; the planned general fix is a post-1.26.11.1 train, design record
`docs/phase4/DESIGN-unsafe-header-views.md`). It computes the same hashes, bit for bit.

Use it from a Go module with a `replace`:

```
replace github.com/zeebo/xxh3 => <path to this directory>
```

The module path, the package name and the whole exported API are unchanged.

## What changed, and nothing else

| File | Change |
|:--|:--|
| `utils.go` | `ptr` is no longer `unsafe.Pointer`. It is `struct{ b []byte; o uintptr; def bool }`: a byte slice, an offset into it, and a flag marking the package's default key. `add`/`sub` step it as the original's `ptr(uintptr(p)+n)` did. `readU8/16/32/64` and `writeU64` read and write the same little-endian bytes through `encoding/binary`, bounds-checked. `bytesOf`/`stringOf` build the original's `str{p, l}` header. |
| `hash64.go`, `hash128.go`, `hash64_seed.go`, `hash128_seed.go` | `*(*str)(ptr(&b))` -> `bytesOf(b)`, `*(*str)(ptr(&s))` -> `stringOf(s)`. The seeded secret `ptr(&[secretSize]byte{})` -> `ptr{b: make([]byte, secretSize)}`. |
| `consts.go` | `key` is `ptr{def: true, b: []u8{...}}`, the same 192 bytes. |
| `accum_generic.go` | Pointer steps use `add`/`sub`. `secret != key`, a pointer-identity test, is `!secret.def`. |
| `hasher.go` | The header and data-word reads of `buf` become `string(buf)` and `ptr{b: []byte(buf[:_block])}`. The second converts only the block being accumulated, so a long first write is not copied quadratically. Views of the Hasher's own buffer `ptr(&h.buf)` and `ptr(&h.buf[0])` become `ptr{b: h.buf[:]}`. A nil key test or assignment uses the zero `ptr`. |
| `accum_stubs_other.go` | Now the only stub file, on every platform: the vector paths need assembly, so the CPU flags are false and the scalar path always runs. |
| removed | `accum_stubs_amd64.go`, `accum_stubs_arm64.go`, the four `accum_vector_*.s` files, `_compat.c`, `Makefile`, and the `klauspost/cpuid` dependency in `go.mod`. |

The algorithm text, every constant and the length branches are untouched.

## The proof

`identity/check.sh` runs one program twice under real Go, once against the original v1.1.0 (`identity/original`)
and once against this fork (`identity/fork`, which replaces the module with `../..`). It exits 0 only when the two
outputs are byte-identical. The program prints one line per result:

- `Hash`, `HashString`, `Hash128`, `HashString128` at every length from 0 to 2600: the 0, 1-3, 4-8, 9-16, 17-128,
  129-240 and >240 branches and the first block boundaries. Also at 4095, 4096, 4097, 10000, 65543 and 1 MiB + 3.
- `HashSeed`, `HashStringSeed`, `Hash128Seed` and `HashString128Seed` at the same lengths, for five seeds (0, 1,
  0xdeadbeef, 0x9E3779B97F4A7C15, all-ones).
- The streaming `Hasher` and `Hasher128` (`NewSeed`/`NewSeed128`) over 18 lengths x 9 chunk patterns x the five
  seeds, alternating `Write` and `WriteString`. Each reads `Sum64`, `Sum128` and `Sum` with an append. Also a reused
  hasher (`Reset`, `ResetSeed`) and a zero-value `Hasher`/`Hasher128` with `Size`/`BlockSize`.

Measured 2026-11-11 with Go 1.26.10 on linux/amd64, on a CPU with AVX2 and AVX-512, so the original took its vector
paths for long inputs:

- Both outputs are 16,632 lines with sha256 prefix e1dff98822109fc7: IDENTICAL.
- Negative control: one key constant in the fork's 129-240 branch changed made 150 lines differ. Restored byte for
  byte, the outputs were identical again.
- xxh3's own tests, including its 4,368-line compatibility table against the C reference, pass on the fork.
- Converted by go2cs (`-recurse`, linux, Release), the fork's identity program printed output byte-identical to the
  original's Go output (16,632 lines).
- Not run: the original's NEON path on arm64.

## Cost

Hashing a `string` copies it once into a byte slice, as `stringOf` must without a header view. The scalar path
replaces the original's SIMD paths, so long inputs hash more slowly than with the original under Go. The results do
not change.

## License

xxh3's BSD 2-Clause license (`LICENSE`) is kept and still governs these files. The changes above were made by the
go2cs authors in 2026.
