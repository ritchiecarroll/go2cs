# Probe: reflect Call, CallSlice and MakeFunc of value-type variadic funcs under Native AOT

P1's variadic trampoline registry (golib's `GoReflect.VariadicRegistry.cs`, d4f52bbbfe) closes reflect's call
and make trampolines of a variadic func through COMPILED code: go2cs-gen registers every variadic signature that
can reach reflect, so Native AOT has the value-type instantiation it cannot build by `MakeGenericMethod`. Its own
`varfunc` probe was never committed, so this one was written for the lazy-registries seat
(claude/g-lazy-registries), which moved the escape registry's variadic registrations from the module initializer
to a queue run on the registry's first miss. Keep it for the next change to either.

## The program

`main.go` converts four variadic funcs to `any`, which is what makes the escape registry name their signatures,
and drives each through reflect:

- `Call` of `func(string, ...string) string` (the `@string` family);
- `CallSlice` of the same func;
- `Call` of `func(int, ...point) int`, a converted struct as the tail element;
- `Call` of `func(string, ...uint16)`, an Action family with a builtin value-type tail;
- `MakeFunc` of `func(string, ...string) string`, set through `reflect.ValueOf(&mk).Elem().Set` and called.

## Running it

Convert with the tree's converter into a scratch directory (the output directory as the second positional), add a
`Directory.Build.targets` there with the AOT publish properties of `src/tests/Performance/Directory.Build.targets`
(`PublishAot`, `PublishTrimmed`, `TrimMode=partial`, `SelfContained`,
`RuntimeIdentifier=$(NETCoreSdkRuntimeIdentifier)`, `PublishReadyToRun=false`), then
`dotnet publish -c Release -p:GoTargetOS=linux -p:go2csPath=<tree>/src/` and run the binary. Nothing in it is
Linux-specific; the readings below are linux-x64.

## Readings

Go 1.26.9 prints:

    call a-b
    callslice c+d
    struct tail 15
    count u16 2
    makefunc n2

| tree | publish | bytes | run |
|---|---|---|---|
| d410fb78bc (batch-16 tip: P1's registry, registrations run by the module initializer) | rc 0, 0 errors | 135,591,496 | exit 0, Go-equal |
| 2cd5dcd2f1 (lazy registries: the same registrations queued, run on the first miss) | rc 0, 0 errors | 135,554,344 | exit 0, Go-equal |

Linux, Microsoft .NET 10.0.401 SDK, each tree converted with the converter built from that tree. P1's committed
`promoted` probe (docs/phase4/probes/p1-reflect-method-arms/promoted) read Go-equal at both trees too:
135,584,216 and 135,547,112 bytes.

A falsifier this probe has NOT been run against: a tree with neither the registry nor the escape-registry
registrations. P1's BOARD row (2026-10-21) records that shape as exit 2, `NotSupportedException` for missing native
code in `callVariadicFunc1`; this probe was written after that fix and did not re-measure it.
