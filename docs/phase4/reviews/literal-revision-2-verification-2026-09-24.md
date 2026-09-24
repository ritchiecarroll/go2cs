# COORD verification of C2's string-literal revision 2 (ea2f481b4c)

Read-only adversarial verification, 2026-09-24: three lenses (the registration table, the sstring-first model, the recommendation against the owner's axis), each contested finding re-checked by an independent skeptic, then synthesised. Evidence cites path:line@sha.

## Part 1: for the owner

**Bottom line.** Take Tier 2 (the literal registration table) and Tier 3 (hoisting where it is already ruled). Do not take Tier 1 (sstring first) as written.

- **Tier 2 is measured and sound.** A hit costs 3.7–4.6 ns with zero counted allocations. A miss, including a lost Roslyn deduplication, falls back to today's copy and never returns a wrong string. Converted code does not change.
- **Tier 1's first step is already done.** Since 2026-08-12 the converter emits a zero-copy `tmpstring(b)` for `m[string(b)]` reads, on 50 production lines. The "full gap" headline is false.
- **O1 (sstring parameters) has a false safety basis.** C2 says every breaker is a compile error. Some breakers are silent counted copies instead, and golib's run-time binder would silently answer false to `w.(io.StringWriter)`.
- **O1 buys little after Tier 2.** It saves about 4 ns per literal and puts a second string type into public signatures.

**What could change the ruling**

1. **Start-up cost.** Import hooks initialise the whole import closure eagerly, so a program pays for every imported module, not "47 ms at the largest module".
   - A hello world that imports fmt registers about 3.4K literals, about 1.7K of them from runtime. One lens counts fewer.
   - Scaling linearly from one probe method (INFERENCE), that is roughly 20–60 ms under JIT, against a published 279 ms start-up. Under NativeAOT it is about 1 ms.
   - `dotnet run` and the test host get no ReadyToRun.
2. **The Roslyn deduplication dependency.** The C# spec does not promise it. Losing it is harmless (counts go back to today's), but the probe covered only one file. Generator trees, ReadyToRun and Windows are untested.

**More sstring, closer to Go.** These exact Go rules are missing from the design:
- `string(r)`: Go uses a stack buffer.
- One-byte strings.
- Concatenation with one non-empty operand.
- A concatenation of several operands done as one allocation.
- Read-only `[]byte("lit")`.

**Still unmeasured**
- A start-up A/B on an fmt hello world and on the net/http test process, under tiered JIT, tiering off (TC=0) and ReadyToRun.
- O1's reach after filtering out func values, binder dispatch and reflection.
- AllocsPerRun on a Tier 2 build.
- The -m join, which is not committed.

## Part 2: fix list for C2

1. **Drop O2 as the first step of Tier 1, and rewrite the G1 row and relay line 15.** `mapReadTmpStringKey` already emits a zero-copy `tmpstring(b)` (src/go2cs/convIndexExpr.go:388-437@47e088d3d7, which calls builtin.cs:3009-3012 and then string.cs:121). It covers 50 production lines in 14 files and is pinned by AllocationCounterTests.cs:120. Size only what is left: named string keys, named-byte elements and composite keys. [S4/R1, high]
2. **Cost start-up as the sum over the import closure, not the largest module.**
   - MEASURED: the import hooks call RunModuleConstructor (fmt/package_info.cs:113-122, builtin.cs:239-242@47e088d3d7). About 99% of the closure's literals are forced this way (packageInitFacts.go:17-80).
   - Replace the design's :1009-1021 and :1221 and relay line 14. [T-03/R5]
3. **Measure start-up before the ruling.**
   - Targets: an fmt hello world and the net/http test process.
   - Regimes: tiered JIT; TC=0, the test host's regime (testConversion.go:6554-6557, which has no ReadyToRun); and publish-time ReadyToRun (csproj-template.xml:50-55). [T-03/T-04/T-05]
4. **Fix the "work alone (second call) 0.15-0.24 ms" label.** That figure is the early return for already-registered literals (Caches2.cs:130@ea2f481b4c). Quote NativeAOT's first call, 1.18-1.48 ms (output-round2-linux.txt:175-178), as the cost without the JIT. [T-04]
5. **Size the start-up mitigations honestly.**
   - The @string-reaching filter cuts at most about 3.5%: only 121 of 3,419 closure literals are used solely in comparisons.
   - The lazy per-module-range alternative has costs: a path first taken inside the AllocsPerRun window counts 1 (testing.cs:725-755), and image-range discovery under single-file and AOT builds is unmeasured. [T-05]
6. **Rewrite O1's safety basis (DESIGN:1118-1126).**
   - Silent counted copies: an @string field store, an explicit `F<@string>`, `copy()` (builtin.cs:985/1085/1109), local `[]string` elements, and passing onward to `...string`.
   - Silent run-time miss: the binder demands exact parameter-type identity (TypeExtensions.GoMethodSets.cs:515-545) and fails soft (AdapterBinder.cs:70-76).
   - Compile errors: func values (text/template/funcs.cs:40).
   - The generator's static adapters do bridge @string to sstring (InterfaceImplTemplate.cs:294). The filter is therefore "reachable by neither binder fallback nor reflection", not "satisfies no interface". [S8/R2]
7. **Re-rank O1 as a performance campaign that follows Tier 2.**
   - For literals it adds about 4 ns and no count gain. Its count gain comes only from `string(b)` arguments, which need the proof in item 10.
   - Pilot it on unexported fmt helpers (doPrintf, parsenum, argNumber, writeString), or a ReadOnlySpan<byte> twin overload.
   - golib's sstring has no `ꓸꓸꓸ`, enumerator, copy or append (fmt/print.cs:125-127). [S10/R3]
8. **Route sstring→@string (sstring.cs:177-180@47e088d3d7) through the table.** Otherwise O1 undoes Tier 2's counts for literals that pass through a view. §8.3 :1212's "everything a view cannot reach" is currently false. [T-13/R3]
9. **Route InheritedTypeTemplate.cs:390 through `(@string)value` and list it among the wired entries.** It calls the copying constructor today. A live bypass is reflect/all_test.cs:6683 `Tag: "s"u8`. [R6/T-08]
10. **Add a no-mutation proof across callees before viewing `string(b)` call arguments (O1 and O4).**
    - `objectIsWritten` works only within one function (escapeAnalysisOperations.go:1479-1530@47e088d3d7) and is alias-blind (INFERENCE).
    - Go's G4 is a stack copy of at most 32 B (walk/convert.go:229-243).
    - Watch these exact-count rows: bufio_test.go:603, io/multi_test.go:115, gob/timing_test.go:132 and slices_test.go:956. [S12/R2]
11. **Before widening sstring, route its allocations through AllocationCounter.**
    - Raw allocations: sstring.cs:76, 111, 204, 214, 445, 450 and 453-459. For example, encoding/json/encode.cs:1279 makes 2 allocations and counts 1.
    - The `NoUncountedBackingAllocations` guard cited at AllocationCounter.cs:158 does not exist. [S11]
12. **Add the Go no-copy rules that §8.2.1 omits, then size them.**
    - `string(r)` in a non-escaping position uses a 4-byte stack buffer (convert.go:260-266); golib allocates (string.cs:502-505).
    - One-byte strings come from `staticuint64s` (runtime/string.go:144-150). Zero bytes are already free.
    - A concatenation with one non-empty operand returns that operand without allocating (string.go:46-51 vs string.cs:718-755).
    - A multi-operand concatenation is emitted as a chain of `+` (time/time.cs:346), which costs one allocation per `+` where Go uses one.
    - Zero-copy read-only `[]byte("lit")` (escape.go:336-346): about 234 production lines. [S3/R4]
13. **Commit the -m join script and a digest of its 136K-line output.** Disclose the exclusions (variadic, named-type, generic and hand-owned callees) and cover each GOOS, not only windows (DESIGN:1098). [S7]
14. **Replace the stale CS0034 rationale (sstring.cs:182-188).** The exact comparison operators landed later the same day, in c5398fcb64. Add negative tests to step (i):
    - `object o = sstr` must stay an error.
    - Span<byte> must not bind an sstring parameter.
    - Check for CS0121 in every new overload set. An sstring map indexer would expose about 3,548 map-literal lines. [S9/S5]
15. **Fix the initializer order.**
    - Generator initializers run after package_info.cs's hooks and after the package's own `init()` (os.csproj:147-151, fmt.csproj:140-153).
    - Emit a first-position hook in package_info.cs that calls a generated partial method.
    - Gate the order under JIT too, not only NativeAOT (:1046). Until then, withdraw ":909 one literal's pointer never changes". [T-06/R12]
16. **Harden the deduplication dependency.**
    - Pin a generator-tree literal against a user-tree literal in a different file.
    - Add a per-module sentinel self-check.
    - Record the build configuration of c2-rva-dedup.
    - Add ReadyToRun and Windows legs. [T-02/R11]
17. **Guard collectible load contexts.** Skip registration when `IsCollectible` is set, or purge the module's keys on `Unloading`. After an unload, a stale (address, length) key can return a wrong string. [T-11/R11]
18. **Reconcile :909 ("none of the four") with :1038.** Public `Slice()`/`ToSpan()` (string.cs:251-281) are writable views over a shared backing. Make them read-only as a precondition, or state the hazard. [T-11/R12]
19. **Restore and add gates.**
    - Restore the census of the operator's non-literal callers (:640-641).
    - Add tiered JIT to 8.1R.5 (review fix 9). The tiered miss is +50-56% (output-round2-linux.txt:83-88).
    - Add a V10 concurrent stress run, an ARM64 leg, and an operator-inlining check. [R10/T-09]
20. **Specify how the table grows and give an honest memory figure.**
    - The probe's table is a fixed 1<<16 slots and spins forever when full (Caches2.cs:106).
    - The memory bound must include the 24 B per-array overhead (about 150 KiB for hello world) and the size of the index (:1028). [T-09/T-10]
21. **Write the predictions as "≤" rather than "=".** State that at rollout Tier 2 can flip a passing row to a byte reading (testing.cs:743-755). [R7]
22. **Correct these figures.**
    - §8.2.4's 86/78 ns should be 62.94/63.36 ns (output-linux.txt:23,25).
    - The JIT ratio reaches 3.79×.
    - NativeAOT recovery at 8 B is 53%.
    - The tiered miss is +7.76 ns.
    - There are 4 sstring locals, not 3.
    - The G1 condition is wrong: `mapKeyReplaceStrConv` (order.go:321-363) has no nonempty-literal rule, and the read-only gate is at :1209. [R8/R9/R14/S2]
23. **Optional: intern the eager values by content at registration.** This gives identity across modules (196-201 duplicate contents in the fmt closure) for about 0.3 ms (INFERENCE). [T-07]

**Open (uncertain)**
- **The closure literal count.** Lens T and its verifier found 3,419-3,440 with runtime at 1,683; lens R found 2,077 and its verifier 1,340-1,610. My own spot-check found 1,779 for runtime; its regex was loose and the pathspec also caught runtime's sub-packages. It is consistent with the higher count but does not settle it.
- **How the binder fails.** Whether the silent miss shows up as a false type assertion or as an exception has not been run.
- **Why the JIT regime matched.** The MinOpts explanation for the equal tiered and tiering-off timings is INFERENCE.
- **The tiered miss.** Whether dynamic PGO explains the tiered +7.6 ns is INFERENCE.
- **Occupancy.** Probe length and miss cost at realistic table occupancy are unmeasured.

**Refuted:**
- R5's 4,594-literal test module (test csprojs reference production rather than recompiling it).
- S8's claims that interface satisfaction is a compile error, and its GoReflect.MethodSets.cs:418 citation.
- T-08's claim that InheritedTypeTemplate.cs:390 calls the operator.
- S3(c)'s zero-byte gap.
- R4(2)'s concatenation-operand gap (sstring.cs:412-448 already has the operators).
- T-07's reading that the design claims identity across modules.
