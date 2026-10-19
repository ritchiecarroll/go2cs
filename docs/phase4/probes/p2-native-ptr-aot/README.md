# Probe: the native pointer-array reader under Native AOT

G's hold on the native reader (2026-10-19): `builtin.NativeArrayPointer` built its foreign
pointer-array box by `MakeGenericMethod` over the pointee type E. Native AOT has no code for a
reflection-built instantiation over a VALUE-type E (`ushort` at net's TXT site), so the reader would
fail in a published binary while every JIT test passed. The first fix built the box through E's
compiled type operations (`GoTypeOps`), which put the box into every type the program carries
(+17.4% binary). G ruled it NARROWER: golib's `NativeArrayOpeners` keeps one opener per element,
registered by go2cs-gen's `NativeArrayOpenerGenerator` in the module initializer of every package
that SPELLS the element at a door (AliasPointer, unsafe.Slice over a `**E`, the array-pointer doors,
an `[N]X` or `[N]*E` struct field). An element no compilation spells refuses by name, as a Go panic,
on the JIT and under Native AOT alike. The window over a native struct's `[N]X` field, which also
closed a generic method by reflection, opens through the same registry.

## The program

`main.go` maps an anonymous page with `syscall.Mmap` (memory the runtime did not allocate) and
lays out two records in net's `DNSTXTData` shape: a count, then the first word of an array of
pointers. It reaches each record from an address that arrives as a number, as DnsQuery's does, and
views the array the way `net/lookup_windows.go` does:
`(*[1 << 10]*uint16)(unsafe.Pointer(&d.StringArray[0]))[:d.StringCount:d.StringCount]`.
The first record's words name UTF-16 strings (a builtin E, `uint16`); the second's name converted
structs (a struct E, `point`). `go vet` reports the uintptr-to-pointer conversions; they are the
point of the probe.

Four arms follow. `unsafe.Slice` over a `**point` that arrives as a number reads the second record's
words, the shape of crypto/x509's windows chain reads (COORD's ruling: in this seat). `reflect.NewAt`
builds `*[2]*point` over the same words, whose reader the program compiled, and `*[2]*other`, a type
of the same layout the program never spells as a pointer array: the converted program refuses it by
name where Go reads the words, which is the ruled divergence. A third record holds a `[4]uint16`
VALUE field, read element by element through the window over a native struct's array field.

Two shapes were taken out of the arms because they fail before the reader, on their own: reading a
pointer FIELD of a struct in native memory (`cc.Chains`) and ranging over an array field of one
(`range nr.Name`) both read the whole struct, whose CLR layout is not Go's, and refuse by name. They
are reported as findings, not tested here.

A first spelling viewed `&b[0]` (an element of a Go slice) directly. The converter refuses that
statically, by name ("a slice view (*[N]*uint16)(p)[...] over a managed element has no managed
aliasing pair"), so it never reached the reader; the TXT site's spelling does.

## Running it

Convert with the tree's converter into a scratch directory (the output directory as the second
positional), add a `Directory.Build.targets` there carrying the AOT publish properties of
`src/tests/Performance/Directory.Build.targets` (`PublishAot`, `PublishTrimmed`, `TrimMode=partial`,
`SelfContained`, `RuntimeIdentifier=$(NETCoreSdkRuntimeIdentifier)`, `PublishReadyToRun=false`),
then `dotnet publish -c Release -p:GoTargetOS=linux -p:go2csPath=<tree>/src/` and run the binary.

## Readings

Linux, uid 0, Microsoft .NET 10.0.12 / SDK 10.0.112, go1.26.9. Go prints:

    0 hello
    1 world
    point 0 3 4
    point 1 5 12
    slice point 0 3 4
    slice point 1 5 12
    reflect main.point 0 3 4
    reflect main.point 1 5 12
    reflect main.other 0 3 4
    reflect main.other 1 5 12
    name 3 103 111 112 0

| tree | program | publish | bytes | run |
|---|---|---|---|---|
| fc87c9094d (before the narrowing: GoTypeOps builds every box) | this one | rc 0, 179 warnings | 99,335,120 | exit 2: lines 1-4 Go-equal; `slice` refuses (the element contains managed references); both `reflect` lines panic on an invalid `Value`; `name` dies of `MakeGenericMethod` (`NativeArrayField.open<ushort>` has no native code) |
| 61bbc51aa4 (narrowed) | this one | rc 0, 178 warnings | 84,508,144 (-14.9%) | exit 0: Go-equal on every line but `reflect main.other`, one refusal naming `*[2]*main.other` (the ruled divergence) |
| 61bbc51aa4 (narrowed) | the first program (886f5e3896) | rc 0, 178 warnings | 84,437,424 (-0.05% against 84,480,352 at 5e31bfb239) | exit 0, Go-equal |

The narrowed publish generated 97 `RegisterArray` and 14 `RegisterPointerArray` calls over the probe's closure, in 27
files.

The first program (the two records only) read at 5e31bfb239, before any fix: publish rc 0, 84,480,352
bytes, RED exit 2 (`NotSupportedException` from `NativePointerArray<ж<ushort>>`'s initializer:
`MakeGenericMethod() is not compatible with AOT compilation`). The GoTypeOps fix read Go-equal at
99,181,056 bytes (+17.4%).
