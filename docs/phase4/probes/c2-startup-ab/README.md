# c2-startup-ab -- what the literal registration table costs at start-up (point-in-time record)

Input to [`DESIGN-string-literal-allocation.md`](../../DESIGN-string-literal-allocation.md) §8.1R2.1
(COORD's verification, fixes 2, 3 and 5). Local, unofficial, not a gate. Results:
[`results-linux.txt`](results-linux.txt).

**Programs.** `hello.go.txt` (fmt) and `nethttp.go.txt` (net/http), each converted by the tree's own
converter into a scratch copy of `src`, then built with `-p:GoTargetOS=linux` (the Windows flavour cannot
start on linux).

**Arms:**
- **A:** today's scratch tree.
- **B:** `LiteralTable.cs` copied into the scratch `src/core/golib`. The span operator
  (string.cs:460) returns `new @string(literal)` when `LiteralTable.Lookup` finds the span, and
  `genreg.py` writes one `[ModuleInitializer]` per closure module registering that module's distinct
  regular `"…"u8` literals.
- **Boff:** B with `GO2CS_LITTABLE_OFF=1` (the initializers run, `Register` returns at once).

`GO2CS_LITTABLE_REPORT=1` prints the registration count and the time spent in `Register` at exit.

**Regimes:**
- tiered JIT (`dotnet <app>.dll`);
- TC=0 (`DOTNET_TieredCompilation=0`, the test host's regime);
- the template's own publish (`dotnet publish -c Release -r linux-x64`, which sets `PublishReadyToRun`
  and `PublishTrimmed`). The images were checked for a ManagedNativeHeader.

**Timing:** `time.py`, wall time from exec to exit, 31 runs per arm after 3 warm-ups, arms interleaved
in alternating order.

**The net/http TEST process is not measured here:** its test project does not build for linux at this
tree (see the design block). `nethttp.go.txt` carries net/http's production closure instead.
