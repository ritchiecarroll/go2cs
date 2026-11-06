# DATA — the Native AOT start-up cost of a structural type assert

> **State: MEASURED (2026-11-06), one linux lane box (4 cores, ILC 10.0.14), branch `claude/p1-aot-assert` cut at
> the extension-scan tip `93fc19e371`.** golib only. One lever lands with this record: `9133bba5a0` (red) and
> `882faace3a`. The others are named in §5 with their measured share. The prediction was posted before the first
> run (mailbox `5b4defa4a`) and is scored in §6. Raw readings, the probe and the scratch instrument are in
> [`data/aot-structural-assert/`](data/aot-structural-assert/).

## 1. Why this was measured

G's item-7 trace ([`DATA-hop126-perf.md`](DATA-hop126-perf.md) §11.3) found 516 ms of PerfTlsHandshake's Native AOT
warm-up in `builtin.TryTypeAssert` → `StructurallyImplements` → `GetGoMethodSetCandidates`, decoding reflection
metadata on first use. That program's ILC needs about 23 GB, so this seat measures a small program with the same
path.

## 2. The probe and the method

- **The probe** is a scratch copy of `PerfIfaceShell`: 62 go2cs assemblies (golib, runtime, reflect, os, syscall,
  time, fmt and their imports). It asserts to an anonymous interface, so no compile-time adapter exists and both
  shell tiers run. Only `Main` was edited: it times, in-process with `Stopwatch`, the first assert of a value
  (`counter`, the object shell), then the first assert of a pointer (`&box`, the generic shell), a second round of
  both, and 1,000 loop iterations. The project was deleted after the seat.
- **Builds.** The JIT build is the converted project's own Release build. The AOT build is the Performance suite's
  `-p:PerfAot=true` publish (partial trim, about 14.5 minutes and a 150 MB image here).
- **Launches.** 1 warm-up and 24 timed launches per arm. The base was measured twice, an hour apart, the second time
  alongside the lever.
- **The split.** A scratch golib build with phase timers (`kit/marks.patch.py`, never committed), 6 launches plus 1
  warm-up per runtime. The timers cost nothing measurable: the instrumented AOT first assert read 268.8 ms against
  268.0 ms.

## 3. First-assert cost, AOT against JIT

Medians, p25 to p75 in brackets:

| arm | first value assert | first pointer assert | second round | process wall |
|:--|--:|--:|--:|--:|
| AOT, base | 268.0 ms (264.5 to 272.2) | 1.05 ms | 2.5 µs | 469 ms |
| AOT, base again | 265.5 ms (263.8 to 267.6) | 1.06 ms | 2.6 µs | 464 ms |
| AOT, lever `882faace3a` | **254.7 ms** (251.7 to 270.7) | 1.11 ms | 2.7 µs | 468 ms |
| JIT, base | 121.4 ms (118.9 to 122.8) | 11.65 ms | 65 µs | 574 ms |
| JIT, base again | 120.4 ms (119.7 to 126.7) | 11.59 ms | 35 µs | 573 ms |
| JIT, lever | **116.5 ms** (115.4 to 118.9) | 11.37 ms | 67 µs | 569 ms |

- The first assert costs **2.2 times as much under AOT** as under the JIT. The first value assert carries almost all
  of it: the pointer assert that follows reuses the scan.
- Under AOT the first assert is 57% of the process wall. The other 200 ms is before `Main`: `Main`'s own work
  besides the asserts is under 1 ms. That part is not this seat's.
- Every launch printed the asserts as matched, with the checksum Go prints.

## 4. Where the AOT first assert goes

Phase timers, medians of 6 launches (`phase-timers.tsv`). Each figure is the time spent across the first assert's
three method-set keys: the value key, the pointer key, and the binder's own pointer-set key for `counter`.

| part | AOT | JIT | what it is |
|:--|--:|--:|:--|
| **golib's extension scan** (`GetExtensionMethods`), all of it | **250.1 ms** | 78.1 ms | runs once, on the first key |
| — `Assembly.GetTypes()` on 56 assemblies | 75.3 | — | the AOT path: there is no raw metadata, so the scan's TypeDef walk falls back |
| — the shape test on each of the 6,514 types returned | 64.5 | 0.2 | the first property read of each type; under the JIT only 79 classes are read |
| — `GetParameters()` for each of 4,384 methods' receiver | 71.6 | 48.5 | |
| — `IsDefined(ExtensionAttribute)` on 17,528 static methods | 25.7 | 5.8 | every package-level function is a static method of its package class |
| — `GetMethods` on the 155 classes of the right shape | 9.6 | 3.0 | |
| — the TypeDef walk | — | 10.7 | the JIT path |
| the per-key filter: the pointer-receiver rule (attribute reads on value keys) | 14.2 | 4.9 | evaluated 13,152 times at the base |
| the per-key filter: receiver element and match | 3.8 | 5.4 | |
| `AdapterBinder.TryCreate` | 2.7 | 12.0 | |

- Under AOT, **listing and touching every type costs 140 ms**: `GetTypes` plus the first read of each type. That is
  56% of the scan. The receivers' `GetParameters` cost 72 ms (29%), and the attribute reads on package functions 26
  ms (10%).
- G's 516 ms over the crypto/tls closure (162 assemblies) is consistent with this scaling. That is inferred, not
  measured there.

## 5. The levers

| lever | AOT saving on the probe | where | status |
|:--|:--|:--|:--|
| L1. match the receiver before the pointer-receiver rule's attribute reads | **−11 ms** measured (268/266 to 255); JIT −4 ms | golib, behavior-neutral (two pure filters reordered) | **landed here** |
| L2. a build-time list of each assembly's extension-host classes, read under AOT instead of `GetTypes()` | up to 140 ms (the 56% above) | go2cs-gen emits an assembly attribute per converted assembly; golib reads it | not built: a gen change in every converted assembly |
| L3. carry each method's receiver type in that same list, or read only the asked type's own assembly | up to 72 ms AOT, 48 ms JIT | gen (the list) or golib (the scope) | not built: see below |
| L4. skip `IsDefined` on package functions, by listing the extension methods themselves in L2 | up to 26 ms | gen | not built |

- **L1's saving scales with value keys.** The probe asks three keys. A program that asserts many value types to
  interfaces, crypto/tls among them, pays the 13,152-read sweep for each value key at the base, and now pays only its
  own type's reads.
- **L3's golib-only form is a behavior question, not a free win.** Go declares a type's methods in the type's own
  package, so a key for `X` could read only `X`'s assembly. But a `-tests` whitebox bridge declares methods on a
  production package's types from the test host assembly. The generator's promoted wrappers and generic
  definitions would also need stating. It needs its own design and its own red tests.
- **L2 is the AOT lever with the largest share.** NativeAOT exposes no metadata reader, so `GetTypes()` is golib's
  only way to enumerate types there. It is the build-time list that the extension-scan design (§9 of
  [`DATA-bogo-startup.md`](DATA-bogo-startup.md)) set aside for the JIT. Under AOT there is no metadata-table
  alternative.

## 6. The prediction, scored

| predicted | measured | score |
|:--|:--|:--|
| AOT first value assert 150 to 350 ms | 268 ms | MET |
| JIT first value assert 20 to 80 ms | 121 ms | MISSED, over the band |
| AOT at least 3 times the JIT | 2.2 times | MISSED |
| first pointer assert: AOT 10 to 60 ms, JIT 2 to 15 ms | AOT 1.05 ms, JIT 11.6 ms | MISSED under AOT (under the band), MET under the JIT |
| second round under 50 µs on both | AOT 2.5 µs, JIT 35 to 67 µs | MET under AOT, MISSED under the JIT |
| raw metadata answers false under AOT, no throw, asserts match Go | `GetTypes` taken on 56 assemblies, every launch matched | MET |
| the scan 50 to 70% of the AOT first assert | 93% | MISSED |
| the per-key filter 20 to 40%, mostly attribute reads | 7% | MISSED |
| the shell binder at most 10% | 1% | MET |
| L1 saves 20 to 80 ms per value key | 11 ms | MISSED; its falsifier (under 10 ms) did not fire |

The misses share one cause. I sized the per-key filter from the code's shape and did not know two things: that
under AOT the first read of each type `GetTypes()` returns costs about as much as `GetTypes()` itself, and that
`GetParameters` is the scan's second-largest part on both runtimes. The JIT misses come from JIT-compiling the
reflection path, which I did not price.

— P1 (2026-11-06)
