# DATA — L3: the extension scan scoped by the receiver's home, built and measured

> **State: BUILT and MEASURED (2026-11-07).** COORD approved the design
> ([`DESIGN-extension-receiver-scope.md`](DESIGN-extension-receiver-scope.md)) at 02:12Z with one addition: resolve a
> TypeRef receiver through type forwarders, or say why none can be forwarded. Measured on one linux lane box (4 cores,
> SDK 10.0.403, go1.26.10). Base `4d1874d9c0` (master `df7c09ab7b` with batch 225 merged in, as ordered); red
> `f6932f2d54`; fix `914cce6b70`. Data: [`data/extension-receiver-scope/build/`](data/extension-receiver-scope/build/).
> After batch 225 landed, master `c9cb14e731` was merged in (`3d16ccdc96`) and one guard entry fixed (`a282b67210`).
> The measurements in §4 are at base and fix; the gates in §6 are at the merged tip.

## 1. What was built

- **golib** (`TypeExtensions.ExtensionMethodRegistry.cs`). The walk keeps each host class as a metadata token with
  the HOMES its methods extend. For each method carrying `ExtensionAttribute` it decodes the signature's first
  parameter with no type load: by-refs, modifiers, golib's `ж<X>` and generic instantiations are read through, and
  a `TypeDef` is homed in the assembly itself. Any other receiver is resolved by token and homed where its type loads
  (`ExtensionHome`). A host's methods (`GetMethods`, `GetParameters`) are read the first time a question needs them,
  then kept.
- **The questions.** `GetGoMethodSetCandidates` and `GetExtensionMethods(Type)` read only the hosts of the homes a
  matching receiver can have (`ExtensionHomesOf`). That set is the type's own home, its bases', its interfaces' and
  the core library's. It is wider than the design's "the element's assembly" because `ReceiverElementMatches` also
  admits a receiver assignable from the element. The filters are unchanged, so each answer is the same subset in the
  same order. A box target (`GetExtensionMethods(typeof(ж<X>))`, used by panic printing) matches every box receiver
  of every type, so it still reads the whole scan.
- **Native AOT.** `GoExtensionHostList(HomesListed = true)` marks a list whose entries name their foreign homes:
  `GoExtensionHost(typeof(H), Homes = new[] { typeof(T) })`, one type per assembly, or `AnyHome = true`. A host
  whose homes are not listed, or an assembly from an older generator, is read at scan time.
- **go2cs-gen.** `ExtensionHostListGenerator` and the literal interceptors' host write the homes. The templates'
  hosts extend only their own assembly's types, and GolibTests holds every listing to its receivers.

## 2. COORD's addition: type forwarders

A TypeRef receiver is never homed by the resolution scope it names. The walk resolves it through the runtime's
binder (`Module.ResolveMethod`, then the first parameter's type), which follows an `ExportedType` forwarder, and
homes it in the assembly the type actually loads from. In the BoGo closure only the 35 bridge methods take that path.
`AForwardedReceiverIsHomedWhereItsTypeLoads` is the guard. Its fixture host was compiled against `l3fx.facade`; at
run time that assembly forwards the type to `l3fx.moved`. The bridge is homed in `l3fx.moved`, never in
`l3fx.facade`, and a question about the moved type finds the method. If a forwarded receiver were homed in the
facade, the full-scan equality guard would also catch it: the scoped answer for the moved type would lose the method.

## 3. Guards

| guard | red at `f6932f2d54` | fix |
|:--|:--|:--|
| GolibTests `ExtensionReceiverScopeTests` (compiled fixture: package, `-tests` host, unrelated package, forwarder) | 4 of 6 fail | pass |
| GolibTests `ExtensionHostListTests` (every list has `HomesListed`; each listing's homes equal the walk's and the receivers') | 2 of 4 fail | pass |
| GenTests `EachListingNamesTheHomesBeyondItsAssembly` | fails | pass |
| full-scan equality: both consumers, every type of every reachable converted assembly and of the fixture | passes (nothing scoped yet) | pass, 23 s |

**Made to fail once.** The walk was changed to home every resolved receiver under its DECLARING assembly. Six
guards failed, the equality guards among them. The first line names the site: `go.fx.point (X method set): the
whole scan answers [fx_internal_test_package.Generate(point)], the home-scoped scan []`. The registry was then
restored, byte-identical by sha1. `EveryListingNamesTheHomesItsMethodsExtend` did not fail, because no assembly
GolibTests reaches holds a bridge. That gap is why the fixture exists.

## 4. Measurements

**BoGo probe launch** (`-bogo-mode -is-handshaker-supported`, the crypto/tls `-tests` host, tiering off, 32
interleaved pairs after one warm-up pair, outputs identical):

| arm | median | p25 to p75 |
|:--|--:|--:|
| base `4d1874d9c0` | 544.9 ms | 532.5 to 576.8 |
| fix `914cce6b70` | 460.8 ms | 446.4 to 484.0 |
| difference | **−84.1 ms** (paired median −90.0, p25 −112.4, p75 −68.7) | |

**First structural assert** (the assert probe of [`DATA-aot-structural-assert.md`](DATA-aot-structural-assert.md)
§2, rebuilt from its kit in each tree; 24 launches after a warm-up; arms in order base, fix, twice):

| quantity | base r1 / r2 | fix r1 / r2 |
|:--|--:|--:|
| JIT first value assert | 91.5 / 92.4 ms | **69.9 / 68.8 ms** |
| JIT first pointer assert | 8.7 / 9.2 ms | 2.8 / 2.8 ms |
| JIT process wall | 433.3 / 445.5 ms | 409.3 / 412.6 ms |
| Native AOT first value assert | 91.1 / 87.1 ms | **9.4 / 9.2 ms** |
| Native AOT first pointer assert | 0.88 / 0.88 ms | 0.13 / 0.13 ms |
| Native AOT process wall | 257.8 / 250.9 ms | 168.3 / 174.9 ms |
| Native AOT image | 149,732,648 bytes | 149,824,392 bytes (+0.06%) |

The bases are not the ones the design quoted (BoGo 862 to 882 ms, JIT assert 118.7 ms, AOT assert 114 to 120 ms).
Those came from earlier records. Today's bases were measured on this box at the seat's own base, beside the fix.

**The bridge packages' `-tests` rows**, run with the linux re-bank kit (its pinned argv, one row per invocation;
[`bridge-rows/`](data/extension-receiver-scope/build/bridge-rows/)). The container restarted once mid-run, on a newer
kernel, so crypto/tls was read twice in each arm, the second pair after the restart:

| row | base `4d1874d9c0` | fix `914cce6b70` |
|:--|:--|:--|
| sync | — | PASS 48 + 9 (the banked counts) |
| log/slog | — | PASS 228 + 21 (the banked counts) |
| runtime | DIVERGED 11,088 verdicts, 73 disclosed, 1 diverged | the same: 11,088, 73, 1 |
| crypto/tls, run 1 | PASS 1,394 + 1 | **PASS 4,921 + 0** |
| crypto/tls, run 2 | PASS 1,394 + 1 | **PASS 4,921 + 0** |

- **runtime's divergence is the box's.** `TestDecorateMappings` skips under Go and passes under C#, the same at base
  and fix. Go skips because this kernel does not support naming anonymous mappings (`SetVMANameSupported`).
- **crypto/tls moved, and the move is the saving.** Its `TestBogoSuite` launches the host once per BoGo case, and the
  BoGo runner gives the whole matrix a 10-minute child deadline. At the base the C# side hit that deadline in all
  four reads that ran it (600.5 and 600.4 s). The row then counts the suite as its pinned host-limit disclosure
  and withdraws the 3,527 BoGo case rows beneath it. At the fix the matrix finished: in 448.0 s before the restart and
  593.0 s after. All 4,921 rows then matched Go, the case rows included. The margin after the restart is 7 seconds.
  On this box the BoGo matrix passes or fails on the launch cost L3 removes.

## 5. The prediction, scored

| prediction | measured | |
|:--|:--|:--|
| BoGo probe −45 to −90 ms | −84.1 ms (paired −90.0) | **MET** |
| JIT first assert 55 to 80 ms | 69.9 / 68.8 ms | **MET** |
| Native AOT first assert 20 to 50 ms | 9.4 / 9.2 ms | **MISSED, low**: the band left 20 ms or more for work outside the scan, and 9 ms remained |
| `-tests` bridge hosts identical | sync, log/slog and runtime identical; crypto/tls 1,394 + 1 → 4,921 + 0 | **MISSED**: crypto/tls moved, upward (§4) |
| behavioral suite and GolibTests identical | behavioral 3,580 pass, 0 fail; GolibTests 0 fail in Debug and Release after `a282b67210` | **MET**, after one allow-list fix (§6) |
| CNR byte-identical | 961 behavioral packages byte-identical | **MET** |

Falsifiers. The BoGo saving is above 35 ms, and the JIT first assert is below 90 ms. No answer differed from the full
scan on any type the guard asked. **"A bridge package's `-tests` counts move" FIRED on crypto/tls.** It fired
because the C# BoGo matrix now finishes inside the runner's deadline, not because an answer changed: every one of
the 4,921 rows matches Go. The prediction expected the counts to hold and did not consider that the deadline turns
on launch cost. CNR held: no emitted byte changed.

## 6. Gates

At the merged tip, on the same box. Summary: [`gates.txt`](data/extension-receiver-scope/build/gates.txt).

| gate | result |
|:--|:--|
| `go2cs.slnx` members outside Behavioral, Debug | 103 built, 0 failed |
| GenTests | 447 passed, 0 failed |
| GolibTests Debug / Release | 2,410 / 2,435 passed, 0 failed (50 / 25 skipped) |
| `go test ./...` (src/go2cs) | pass |
| CNR | byte-identical across 961 behavioral packages |
| behavioral suite, 14 letter chunks | 3,580 passed, 0 failed, 48 skipped |
| tracked deletions after the battery | 0 |

**What the first pass caught.** `NoUncountedBackingAllocations` failed in both configurations: its allow-list
named the old host merge (`methods.Concat(found).ToArray()`), which the scoped scan replaced with
`ExtensionHostTypes`' array over the walked hosts. It is the same registry metadata, so the entry moved to the new
line with the same reason (`a282b67210`). The guard runs while building had been filtered to the extension tests,
so they never ran it. GenTests' `AnInterfaceCarriesAStaticSourceAssertOverloadInATopLevelClass` also failed once.
The cause was this tree, not the code: the edited files had been written with LF line endings in the working copy,
and that test looks for a CRLF. The repository pins `eol=crlf`, and the committed bytes are the same either way.
After the ten files were checked out again, GenTests passed 447 of 447.

**Two chunks re-run.** Two chunks aborted on the runner's 3-minute inactivity limit. A-B stopped at its first test,
`CheckAdapterNameInterfaceCollision`, twice, each time on a cold core: the battery had purged the core libraries
before the first run, and restoring the line endings rebuilt them before the second. With the core built it passed
250 of 250. I-K stopped at its 30th test, `CheckInterfaceMapKeyPointer`, with 30 passed. Its re-run passed 177 of
177, that test included. The cause of the I-K stop is not established.

— P1 (2026-11-07)
