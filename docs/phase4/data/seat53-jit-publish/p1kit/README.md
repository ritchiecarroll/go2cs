# Seat #53 P1 kit: the accessor's effect, H against A, with the NoInlining control

This kit reproduces seat #53's part 1 on another machine: `FormatBool` and `crypto.Hash.String` called from a hot
loop. The arms are H (the standard library as the converter before #53 emits it), A (as the #53 converter emits it) and
NI (A with every accessor marked NoInlining, the control). Each runs on the steady JIT and under Native AOT. The record
is [DESIGN-jit-inlining-tiering-publish.md](../../../DESIGN-jit-inlining-tiering-publish.md) section 9.

## What it needs

- A clean checkout of the repository whose committed `src/core` equals `1dacbdf4b6`'s (true at `5bf0fcb694`, the
  deferred-corpus window).
- Go 1.26.10, with its GOROOT spelled exactly as `go env GOROOT` prints it. The scripts set `GOTOOLCHAIN=local`.
- The .NET 10 SDK, and for Native AOT the C++ build tools the SDK's linker needs.
- Bash: Git Bash on Windows, or any Linux shell.
- Python 3.8 or later, for `drive.py` (the seat ran it with 3.9).
- About 25 GB free: three emitted roots, and three Native AOT closures.

Every script takes its paths as arguments and refuses to write into a directory that already exists.

## Steps

1. Build both converters, seed one root, and emit the H and A roots. The seed is the checkout's tracked files only, so
   no `bin/`, `obj/` or `Generated/` enters it; the script asserts the seed's `.cs` count against `git ls-files`
   before either emission. `-stdlib` writes into `-go2cspath`'s `core/` and takes no output positional (the
   second-positional rule is for a single-package conversion).

   ```bash
   ./seed-and-emit.sh <checkout> "<goroot>" <work> 1dacbdf4b6 5bf0fcb694
   ```

   About 17 minutes per emission on a 6-core laptop. Each emission must report `Failed: 0` for all three targets.

2. Make the NoInlining root from the A root. It refuses unless every accessor is marked.

   ```bash
   ./make-ni-root.sh <work>/A/root <work>/NI/root
   ```

3. Convert and build `hotphase` once per arm. The H arm is built twice, as HA, for the A/A band. Use the A converter
   for the NI arm.

   ```bash
   ./convert-and-build.sh <work>/H/go2cs.exe "<goroot>" <work>/H/root/src  hotphase <work>/arms H   jit <rid>
   ./convert-and-build.sh <work>/H/go2cs.exe "<goroot>" <work>/H/root/src  hotphase <work>/arms HA  jit <rid>
   ./convert-and-build.sh <work>/A/go2cs.exe "<goroot>" <work>/A/root/src  hotphase <work>/arms A   jit <rid>
   ./convert-and-build.sh <work>/A/go2cs.exe "<goroot>" <work>/NI/root/src hotphase <work>/arms NI  jit <rid>
   ```

   The Native AOT arms are the same four lines with `aot` in place of `jit`, and different names (`Haot`, `HAaot`,
   `Aaot`, `NIaot`). Each AOT publish compiles the program's whole closure: about 16 to 18 minutes on a laptop. Each
   script prints the built executable's path.

4. List the arms for the driver, one tab-separated row per arm: program, arm, executable, environment. Use only the
   paths the scripts printed, with no other lines. `base` and `aa` are the names the driver reads the A/A band from.
   `DOTNET_TC_CallCountingDelayMs=0` puts every JIT arm on the steady JIT; it does nothing under Native AOT.

   ```text
   hotphase	base	<H exe>	DOTNET_TC_CallCountingDelayMs=0
   hotphase	aa	<HA exe>	DOTNET_TC_CallCountingDelayMs=0
   hotphase	A	<A exe>	DOTNET_TC_CallCountingDelayMs=0
   hotphase	NI	<NI exe>	DOTNET_TC_CallCountingDelayMs=0
   ```

5. Run 15 interleaved rounds per runtime, with nothing else running on the machine:

   ```bash
   python -I -X utf8 ../drive.py arms-jit.tsv 15 p1-jit.tsv
   python -I -X utf8 ../drive.py arms-aot.tsv 15 p1-aot.tsv
   ```

   The driver prints each arm's median [min-max] and its per-round ratio to `base`, read against the A/A band.

## What G-LAPTOP read

AMD Ryzen 5 PRO 6650U, win-x64, .NET 10.0.11:

- `FormatBool`: H 2.434 ns, A 0.711 (0.292), NI 2.399 (0.987) on the steady JIT; H 2.365, A 0.604 (0.256) under Native AOT.
- `Hash.String`: A/H 1.106 on the JIT and 1.048 under AOT, both inside the band.

The JIT control there swapped two DLLs into A's build. This kit's NI root replaces that, so the same control also
covers Native AOT, which compiles from IL and cannot take a swapped DLL.
