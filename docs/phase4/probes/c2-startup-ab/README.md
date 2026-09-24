# c2-startup-ab -- what the literal registration table costs at start-up (point-in-time record)

Input to [`DESIGN-string-literal-allocation.md`](../../DESIGN-string-literal-allocation.md) §8.1R2.1
(revision 3) and §8.1R3 (revision 4). Local, unofficial, not a gate. Results:
[`results-linux.txt`](results-linux.txt) (revision 3) and [`results-rev4-linux.txt`](results-rev4-linux.txt)
(revision 4).

**Programs.** `hello.go.txt` (fmt) and `nethttp.go.txt` (net/http), each converted by the tree's own
converter into a scratch copy of `src` (`git archive HEAD src`), then built `-c Release` with
`-p:GoTargetOS=linux` (the Windows flavour cannot start on linux) and run as `dotnet <app>.dll`.

**Arms** (`driver.sh <scratch> <converter> <arm> <program> [single]`):
- **A:** today's tree, plus `LiteralTable.cs` in golib, unused (the operator is not wired and nothing
  registers).
- **B (eager):** `operator-patch.py` wires the span operator (string.cs:460) to `LiteralTable.Lookup`, and
  `genreg.py eager` gives every closure module a `[ModuleInitializer]` registering that module's distinct
  regular `"…"u8` literals. Verbatim `@"…"u8` literals are skipped (16 distinct in the hello closure).
- **Boff:** B run with `GO2CS_LITTABLE_OFF=1`: the initializers run, `Register` returns at once. Boff
  still carries the inlined lookup at every conversion and the table's static constructor.
- **L (hybrid lazy):** `genreg.py lazy`: each module registers only its image range, found from the
  address of one of its own literals (the anchor scan), and a registrar; a miss inside the range runs the
  registrar once.
- **Lh:** L with `GO2CS_LITTABLE_HELPER=1` (the registrar runs on a helper thread). It deadlocks once the
  order is fixed; not timed.
- **Bz, Lz:** B and L as revision 3 built them, with the registration compiled AFTER package_info.cs.

**Registration order (revision 4).** The registration file must compile BEFORE package_info.cs, whose
import hooks run the package's static constructor and so its init-time literal conversions. `genreg.py`
writes `regprobe.g.inc` and `driver.sh` adds it as a Compile item in the scratch
`src/Directory.Build.props`. Revision 3's arms (`zz_regprobe.g.cs`, compiled with the `*.cs` glob) ran it
after the hooks; `GO2CS_REGPROBE_FILE=zz_regprobe.g.cs` reproduces that.

**Reports** (stderr at exit): `GO2CS_LITTABLE_REPORT=1` prints registrations, hits, misses, lazy-module
counts, registrar bytes on the calling thread, `Register`'s time and the JIT metrics (`jitMs` is summed
across threads, not wall time). `GO2CS_LITTABLE_REPORT_MODULES=1` adds one line per lazy module.
`GO2CS_LITTABLE_MISSLOG=1` logs each miss and at exit classifies it against the module images and the
registered ranges, runs every remaining registrar, looks it up again, and prints the bytes of any that
still miss. It changes what `REPORT_MODULES` shows (every registrar has then run).

**Regimes:** tiered JIT; TC=0 (`DOTNET_TieredCompilation=0`); revision 3's template publish
(`PublishReadyToRun`, `PublishTrimmed`); revision 4's `single` publish, the test host's shape
(test-csproj-template.xml: self-contained, single-file, no ReadyToRun, no trimming).

**Timing:** `time.py <scratch> <program> <N> <out> [arms]`: wall time from exec to exit, N runs per arm
after 3 warm-ups, arms interleaved in alternating order; median, p25-p75, min, and deltas against A.

**The net/http TEST process is not measured here:** its test project does not build for linux at this
tree (see §8.1R2.1). `nethttp.go.txt` carries net/http's production closure instead.
