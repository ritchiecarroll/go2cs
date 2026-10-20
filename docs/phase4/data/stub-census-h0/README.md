# Generated-stub census: H0 baseline (go1.24.13, master 1f39d682e6)

The members `PartialStubGenerator` filled with a throwing body (`[GoExternalStub]`), one file per flavour. They
were read from `src/core/<pkg>/Generated/go2cs-gen/go2cs.PartialStubGenerator/*.stub.g.cs` after a clean build of
`src/go2cs-stdlib.slnx`:

    dotnet build src/go2cs-stdlib.slnx -c Release -p:GoTargetOS=<goos> --no-incremental -m:4 -p:UseSharedCompilation=false

Each line is `project<TAB>file name`, with the project relative to `src`, sorted with `LC_ALL=C`. Compare flavours
and releases on `project + namespace.class.member` with the trailing `.<n>` index stripped: that index is a
per-compilation counter, so one added member renumbers every stub after it in the same project.

| flavour | stub files | errors |
|:--|--:|--:|
| windows | 227 | 0 |
| linux | 243 | 0 |
| darwin | 444 | 0 |

Positive control: `internal/cpu` `cpuid` (bodyless at `cpu_x86.cs:14`) appears in all three lists. Not covered:
`core/testing` (it emits no generator files, and its sources hold no bodyless partial), golib, the CPU profiler and
go2cs-gen.

H5 diffs its own per-flavour lists against these, and every member that appears is a named finding. A rename
predicted before H5 (for example `runtime.asmstdcall` becoming `internal/runtime/syscall/windows` `StdCall` and
`asmstdcall`, unreached as at 1.24) is read as predicted, not as new.
