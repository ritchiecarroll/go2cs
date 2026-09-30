# Microsoft's .NET 10.0.12 on a linux container that only reaches nuget.org

**Why.** The owner's ruling of 2026-09-30 (ledger 019626a3f3) puts the fleet on Microsoft's build of .NET.

Canonical's Ubuntu build of 10.0.12 (`/usr/lib/dotnet`, RID `ubuntu.24.04-x64`, whose `libcoreclr.so` links the
system libunwind) faults under on-stack replacement: a fatal `0x80131506`, or a SILENT exit with rc 0 and
truncated stdout. Microsoft's build of the same commit does not (`claude/c2-osr-repro`, amendment of 2026-09-30).

**The egress.** From a cloud container, `dot.net`, `builds.dotnet.microsoft.com` and `dotnetcli.azureedge.net`
answer 403 at the proxy, so `dotnet-install.sh` cannot run. `api.nuget.org` is reachable.

nuget.org carries Microsoft's runtime pack, `microsoft.netcore.app.runtime.linux-x64` 10.0.12: libcoreclr,
libclrjit, libhostpolicy, libhostfxr and the managed libraries. It does NOT carry the `dotnet` muxer for 10.x.
The newest `microsoft.netcore.dotnethost` there is 8.0.31.

## The recipe

```
eval "$(bash docs/phase4/recipes/microsoft-dotnet/use-ms-dotnet.sh)"
dotnet build-server shutdown
```

`use-ms-dotnet.sh` builds `~/.dotnet-ms` once:
1. It downloads the pack from nuget.org.
2. It checks the pack with `dotnet nuget verify` (Microsoft author signature plus the nuget.org repository
   signature).
3. It copies `/usr/lib/dotnet`.
4. It replaces `shared/Microsoft.NETCore.App/10.0.12` wholesale with the pack's files, and
   `host/fxr/10.0.12/libhostfxr.so` with the pack's.

It prints `DOTNET_ROOT` and `PATH` for the current shell. Put the `eval` line in the shell profile for readings.

**What stays the distribution's.** The `dotnet` muxer binary (a native launcher that holds no runtime), the SDK's
managed files, `packs/` (reference packs and the apphost template) and the templates. Every one of them now
RUNS on Microsoft's runtime.

**How each consumer reaches the runtime.**
- `dotnet build`, `dotnet test`, and GolibTests' testhost resolve through the muxer first on `PATH`, which is
  `~/.dotnet-ms/dotnet`, whose root is its own directory.
- Apphosts resolve through `DOTNET_ROOT`: `runtime.tests` and every `-tests` host, the behavioral programs,
  anything built framework-dependent. Without `DOTNET_ROOT` an apphost falls back to
  `/etc/dotnet/install_location`, which is Canonical's. So the export is REQUIRED, not optional.
- `dotnet build-server shutdown` matters. A build server started earlier (an MSBuild node, VBCSCompiler) keeps
  running on the runtime it started with, and it is not a child of your command, so the verifier below cannot
  see it. Shut them down once after switching.

## The verification every lane posts

```
bash docs/phase4/recipes/microsoft-dotnet/verify-ms-dotnet.sh <the reading's command>
```

The verifier runs the command under `strace -f` and collects every `libcoreclr.so` that any process opened
successfully. It PASSes only if every copy has the sha256 of the Microsoft pack's `libcoreclr.so`
(`df5cfe7ba5abf793...`), which it recorded at build time, AND links no libunwind. It also FAILs when nothing
loaded a runtime, since nothing was proven then. It returns the command's own rc if the command failed,
otherwise 0 for PASS and 9 for FAIL.

Post four things:
- the path it prints;
- `ldd <that libcoreclr.so> | grep unwind`, which must be empty;
- its sha256, which must equal the pack's (`df5cfe7b...`). `use-ms-dotnet.sh` records it in
  `~/.dotnet-ms/.ms-runtime-10.0.12`; a box where a pinned publish already restored the pack also has it at
  `~/.nuget/packages/microsoft.netcore.app.runtime.linux-x64/10.0.12/runtimes/linux-x64/native/libcoreclr.so`;
- one GolibTests run, green. Run it once under the verifier to prove the runtime, and read the verdicts from a
  PLAIN run: `strace` perturbs CPU-time accounting, and `AThreadsSamplesAddUpToTheCpuTimeItUsed` failed under it
  on C2's box while passing plainly on both runtimes. Build the linux flavour
  (`CGO_ENABLED=0 GoTargetOS=linux dotnet build -p:GoTargetOS=linux`). A default build compiles golib's windows
  runtime files, whose `kernel32` calls fail on linux for reasons unrelated to the runtime build.

The verifier costs `strace` overhead (with `--seccomp-bpf`, only `openat` stops a process). Use it for the switch's
proof and for spot checks, never inside timed or CPU-accounting runs.

**From the switch on, every linux reading's post names its runtime build** (Microsoft or Canonical), and no
.NET reading is gated on its exit code alone (the fleet rule of 2026-09-30).

## Measured on C2's box (2026-09-30)

Tree: master `f819887fa3`. `use-ms-dotnet.sh` ran in 23 s (the pack is 69 MB; `~/.dotnet-ms` copies the
613 MB `/usr/lib/dotnet`).

| Check | Runtime env | Verifier | Verdicts |
|---|---|---|---|
| `dotnet --version` | Microsoft root | PASS, 1 path | n/a |
| `dotnet --version` (NEGATIVE control) | default (Canonical) | **FAIL**, rc 9, names `/usr/lib/dotnet/.../libcoreclr.so`, links libunwind 2 | n/a |
| an apphost, `PATH` only, no `DOTNET_ROOT` (NEGATIVE control) | falls back to `/etc/dotnet/install_location` | **FAIL**, rc 9, Canonical | n/a |
| the same apphost with `DOTNET_ROOT` | Microsoft root | PASS | n/a |
| GolibTests, Release TC0, linux flavour, plain | Canonical | not run under it | 1441 / 2 / 16 |
| GolibTests, the same, plain | Microsoft root | not run under it | 1441 / 2 / 16, the SAME two failures (this box's known pair) |
| GolibTests under the verifier | Microsoft root | PASS, 1 path | 1440 / 3 / 16: the extra failure is `AThreadsSamplesAddUpToTheCpuTimeItUsed`, strace's |
| `-tests -test-action all` on `unicode/utf8` (Release) | Microsoft root | PASS, 3 paths, including the self-contained host's bundled `bin/tests/.../ubuntu.24.04-x64/libcoreclr.so`, copied from the root | 14 validated + 1 disclosed = the roster's row |
| behavioral `OutputComparisonTests`, 4 projects incl. `NoinlineDirectiveFrame` | Microsoft root | PASS | 4 / 0 |

The `-tests` pipeline's self-contained host BUNDLES the runtime of the root it was built from. A reading built
before the switch carries Canonical's `libcoreclr.so` in its own `bin/` whatever `DOTNET_ROOT` says, so rebuild
after switching; the verifier names that path if one slips through.
