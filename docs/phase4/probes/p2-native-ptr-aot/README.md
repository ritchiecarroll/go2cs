# Probe: the native pointer-array reader under Native AOT

G's hold on the native reader (2026-10-19): `builtin.NativeArrayPointer` built its foreign
pointer-array box by `MakeGenericMethod` over the pointee type E. Native AOT has no code for a
reflection-built instantiation over a VALUE-type E (`ushort` at net's TXT site), so the reader would
fail in a published binary while every JIT test passed. The fix builds the box through E's compiled
type operations (`GoTypeOps`, trim stage 3c-2b): golib's for a builtin, go2cs-gen's for a converted
struct once its pointer is spelled, and a refusal by name for an E with neither.

## The program

`main.go` maps an anonymous page with `syscall.Mmap` (memory the runtime did not allocate) and
lays out two records in net's `DNSTXTData` shape: a count, then the first word of an array of
pointers. It reaches each record from an address that arrives as a number, as DnsQuery's does, and
views the array the way `net/lookup_windows.go` does:
`(*[1 << 10]*uint16)(unsafe.Pointer(&d.StringArray[0]))[:d.StringCount:d.StringCount]`.
The first record's words name UTF-16 strings (a builtin E, `uint16`); the second's name converted
structs (a struct E, `point`). `go vet` reports the two uintptr-to-pointer conversions; they are
the point of the probe.

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

Both arms on the same host, converted and published by their own tree, run once each:

| tree | publish | binary bytes | AOT run |
|:--|:--|--:|:--|
| 5e31bfb239 (reader before the fix) | rc 0, 180 trim/AOT warnings | 84,480,352 | RED, exit 2: `TypeInitializationException` from `NativePointerArray<ж<ushort>>`'s initializer, inner `NotSupportedException: '...open[System.UInt16](...)' is missing native code. MethodInfo.MakeGenericMethod() is not compatible with AOT compilation` |
| the fix (GoTypeOps door) | rc 0, 165 warnings | 99,181,056 | the four lines Go prints, exit 0 |

The fix's binary is 14,700,704 bytes (17.4%) larger: `IGoTypeOps.ForeignPointerArray` compiles a
`NativePointerArrayBox<E>` for every E whose operations the program carries. Reported, not gated,
as the GoTypeOps remarks rule for size.
