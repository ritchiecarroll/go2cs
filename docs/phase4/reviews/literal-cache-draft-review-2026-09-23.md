# COORD review of C2's string-literal cache draft (951c916403)

Read-only adversarial review, 2026-09-23: four lenses (citations and census, probe soundness, parity hazards, alternatives), each contested finding re-checked by an independent skeptic, then synthesised. Evidence is cited as path:line@sha; finding ids (L1-..L4-..) index the per-lens records.

# PART 1: Review of C2's string-literal cache draft

**Bottom line.** For the rows it names, the recommendation holds. At every §8 member row the cache removes the same counted copy that hoisting would remove. The code stays exactly as Go wrote it, and one golib revert rolls it back. But the draft oversells the cache in three ways, and any one of them could change your ruling.

1. **Coverage.** The draft says the 16-byte limit "covers every degenerate-slug literal, the verb formats". ("Degenerate-slug" means a literal too short or symbolic to get a readable field name.) It does not cover them all. Only about one format-position literal in six is 16 bytes or less. Long hex and non-ASCII literals are degenerate but long. If arms A and B are retired, all of those keep copying on every call.
2. **The recommended design was never measured.** Nobody built the recommended design (two entries per bucket, 16-byte limit). Every cache number comes from a stand-in with one entry per bucket and a 32-byte limit. Extrapolating the probe's cost per byte, a hit on a 16-byte literal may cost more than today's copy (about 14 ns against 10 ns).
3. **Determinism.** The cache is shared across the whole process, but go2cs counts allocations per thread. Another thread, or a short string that isn't a literal, can evict a literal while a test is measuring it. go2cs's AllocsPerRun (the port of Go's allocations-per-call test helper) rounds any nonzero result up to 1, so one stray miss fails a row that expects 0. The risk is low and only affects future rows. Still, a hoisted field can never miss, and the draft relies on determinism to reject address-keyed caches.

**In the cache's favour.** At slog's `...any` rows, arm A would not pre-box the keys either (store them already boxed), because the package uses them as plain strings elsewhere. So hoisting gains less there than the probe shows. The cache also reaches literals hoisting cannot, such as lambdas in package-level tables like fmt's TestCountMallocs.

**An alternative the draft did not size.** The generator could register every literal when the module loads, into an exact table that holds literals only and is keyed by each literal's fixed address in the assembly. That keeps the zero visible change and removes both the eviction risk and the length limit. It depends on an unverified compiler behaviour and needs a probe first.

**Unmeasured:** Windows, NativeAOT, tiered JIT, ARM64, how often cross-thread evictions happen, and the Performance suite. In that suite hoisting plausibly gains about 3x what the cache does.

# PART 2: Fix list for C2 before C1 records the ruling

1. **Correct the coverage claim.** DESIGN:627-628@951c916403 is false.
   - The 16-byte gate covers about 14-17% of format-position literals (census over 47e088d3d7; §4.2's own record says 372 of 2,985 are verb-only).
   - Degenerate literals longer than 16 B exist, e.g. crypto/cipher/gcm_test.cs:562@47e088d3d7, a 32-byte hex literal passed to a @string parameter.
   - State the leftover set and give the owner three options: a hybrid, a raised gate, or accepting the gap. (L3-1, L4-4, L4-19)
2. **Label the recommended form as unmeasured, or measure it.** Caches.cs:15,:20@951c916403 is one entry per bucket with MaxLen 32. Add 2-way rows: hit in way 0, hit in way 1, miss, three literals thrashing one set, and the 8-thread stress. Add hit rows at 8, 12 and 16 B, and V2's own over-gate row. (L2-1, L1-23, L2-11, L4-11, L2-4)
3. **Specify the 2-way replacement policy.** Use a single store with no write on the hit path, e.g. fill way 0 if it is empty, otherwise overwrite way 1. A FIFO shift needs two stores, and LRU writes on every hit. (L2-2, L3-3)
4. **Measure the hit cost near the gate.** The hash is byte-serial FNV-1a (Caches.cs:28). Extrapolated, a hit at 16 B costs about 14 ns against about 10 ns for the copy. Either measure it, or lower the gate to 8 B (which still covers every §8 A/B member), or hash a word at a time. (L2-4)
5. **Replace the gate-cost evidence.** The "16.4 -> 18.6 ns, inside this VM's spread" figure comes from V6 (an 8-byte gate, Caches.cs:118). Its +13.7% is larger than every repeat spread in output-linux.txt, which top out around 5%. (L1-24, L2-3, L3-5)
6. **Rewrite the determinism paragraph, 8.1.3(4) (DESIGN:585-592).**
   - A miss inserts any span under the gate (Caches.cs:39-41).
   - The table is process-global, while the counter is per-thread (AllocationCounter.cs:72-78@47e088d3d7).
   - go2cs's AllocsPerRun floors a nonzero result at 1 (testing.cs:743,:755), while Go divides with no floor (allocs.go:44).
   - The literal set includes callees, e.g. slog handler.cs:389 attrSep.
   - State that a zero-reading retirement that depends on the cache is statistical, and add a cross-thread eviction probe. (L4-6, L3-3, L3-4, L2-7, L1-27)
7. **Add the count-to-bytes cliff.** When `counted` reaches 0 but bytes are still allocated, AllocsPerRun reports bytes per run, so the reading goes up (testing.cs:750-755@47e088d3d7).
   - For each predicted row, name the objects that stay counted after the cache.
   - Check fmt's Fprintf(buf,"%x") leg (fmt_test.cs:1603-1607; disclosures.json:9-10). fmt's planned non-copying slice view plus the cache would make that leg report bytes and fail. (L4-7, L3-12, L1-03)
8. **State the footprint.** The cache reaches literals the arms never touch:
   - package-level lambdas and composites (fmt_test.cs:1566-1625);
   - callee literals (slog handler.cs:385-389, which feeds TestTextHandlerAlloc).
   
   The reconvert hunk gate (DESIGN:516) reads zero for a golib or generator change. So "before it lands" (:636-642) needs, on every OS lane, the full banked operational sweep, a re-read of every AllocsPerRun row and a re-read of the disclosures, as §4.6 required for Tier C. (L4-9, L3-2)
9. **Add the Performance suite and platform legs to the gate list.**
   - PerfStringMatch.cs:77@47e088d3d7 evaluates `"// "u8` about 16M times per run. Estimate (inference): today ~156 ms, cache ~117 ms, hoisting ~34 ms.
   - Add Windows, NativeAOT (Performance/Directory.Build.targets:11), tiered JIT and ARM64. (L4-10, L2-17)
10. **Size the literal-only registration table in 8.1.3(7).**
    - Precedent: the generator's `[ModuleInitializer]` registration at AdapterImplTemplate.cs:108-113@47e088d3d7.
    - The table is exact, never evicts and needs no length gate.
    - It depends on Roslyn deduplicating identical u8 data within one module, which needs a probe.
    - Costs: about 107K u8 occurrences corpus-wide, and NativeAOT module initialization. (L4-2)
11. **Correct the rationale for rejecting the pointer-backed @string (DESIGN:613-617).** `Bytes` already branches on a null backing (string.cs:66). "Adds a field" still stands (the struct grows from 16 to 24 B), and the conclusion stands. (L4-3)
12. **Restate the any-slot comparison.**
    - Arm A would not pre-box n, s or d: `preBoxed` requires valueUses == 0 (hoistedLiteralOperations.go:720), and the package has value uses at text_handler_test.cs:194, value_test.cs:269 and handler_test.cs:560.
    - The probe's 72 B is a `params object[]` (Program.cs:21@951c916403). Real code uses `Span<any>` (logger.cs:241-242). (L4-8, L1-03)
13. **Complete the hazard list in 8.1.3(6).**
    - A non-literal with repeated content gets undercounted, which is a false pass (AllocationCounter.cs:64-69).
    - Two evaluations of one literal can give different unsafe.StringData results after an eviction (L3-10).
    - Public writable views (string.cs:251-281) widen the blast radius; consider making ToSpan read-only (L2-18, L3-11).
    - Retained non-literal inserts collide with the rationale in unsafe.cs:1088-1099 (L4-12). (L2-8)
14. **Add the exceptions to "two entries only".**
    - UTF-16 tuple returns bind `operator @string(string)` (string.cs:450), e.g. path/filepath/windows/path.cs:193 and runtime/symtab.cs:959, 16 sites in all.
    - sstring's escape to @string goes through the copying constructor (sstring.cs:177-180). (L4-5, L2-21)
15. **Guard the callers, not only the wiring.** The operator is a public implicit conversion. Pin non-literal spans to the copying constructor (DESIGN:640), using maphash_test.cs:271 and :366 as witnesses. Warn that DESIGN-string-byte-window stage 4 must not build its one-rune string with `(@string)span`. (L3-8, L4-18)
16. **Fix the probe's wording.**
    - "0 mismatches in 32,000,000" is 16M ContentDM plus 16M address-keyed results (Program.cs:142-150). It was x64 only, and ContentDM cannot fail by construction.
    - "2 counted per call" is per two conversions (Program.cs:108). Each conversion takes about 21.4 ns, about 2.2x today's time at the same count (output-linux.txt:34). (L1-23, L2-14, L3-5, L3-6)
17. **Correct these figures.**
    - Retention is about 128-160 KiB of arrays plus a 32 KiB table, not "≈64 KiB" (DESIGN:612).
    - Name the "15 distinct literals": 12 go through the operator per call (n s d a b c e f %s Atoi ParseInt ParseUint), plus "", hello and two.
    - Specify the hash exactly: FNV-1a, then XOR with the length, masked to 4095 (Caches.cs:23-30).
    - As a share of the removable cost, the cache recovers 32% (A) and 28% (B). (L2-19, L3-14, L2-13, L3-4, L1-26, L1-25, L2-5)
18. **Fix "22 new @string( sites use :92".** Only three bind the :92 span constructor: string.cs:151, sstring.cs:179 and string.cs:462 (the operator itself). The IArray<byte> constructor at string.cs:162 also chains to it. Separately, the format-exclusion test is at hoistedLiteralOperations.go:595, not :590-592. (L1-05, L1-07)
19. **Correct the -annotations census in 8.1.5.**
    - The denominator is 3,912, not 3,925.
    - package_init prose is 375 lines, not 97.
    - Metadata anchors are 1,890 lines, not 333.
    - package_info prose is about 21.8K lines, not about 15K.
    - The emitter list is missing testConversion.go:1604, cgoDynamicImports.go:217 and refVerdictPublication.go:69.
    - The default flip moves about 29K lines, not about 21K. (L1-12, L1-17, L1-18, L1-19)
20. **List the -annotations churn outside src/core.**
    - 399 .cs.target goldens (775 lines) and 15 Performance .cs files.
    - Docs: docs/README.md:334, ConversionStrategies.md:851 and ConversionStrategies-Reference.md:6968.
    - The hand-owned runtime/mranges_impl.cs, which a flip will not touch.
    - Drift in disclosure records that cite C# path:line.
    - The flag must also be passed to the -tests seed and to check-no-regression. (L1-20, L4-17)
21. **The prose-reader check already fails.** packageInfoWriter.go:236-242 copies existing lines through verbatim, and migrateProseBlock finds a block by its first prose line (:36, :117-150). So changing the emitters alone leaves the committed prose in place; the seat needs a strip pass. Once stripped, prose cannot be turned back on in files already written. (L1-11)
22. **Extend the never-governed list.**
    - Add the 225 go2cs_test_host.cs "DO NOT EDIT" headers (testConversion.go:4275).
    - Name both funcPlaceholderLead readers: platformHandOwn.go:384 and manualConversionDestination_test.go.
    - Classify the `/* expr */` const-echo family (convBinaryExpr.go:135, :961; convCallExpr.go:6170; about 1.8-2.1K lines). (L1-16, L1-09, L4-16)

**Refuted:** no finding was refuted outright. Verification did refute these sub-claims:
- L3-1's example: handshake_test.cs:96 is a println, a universe builtin that never hoists (hoistedLiteralOperations.go:557). gcm_test.cs:562 replaces it.
- L2-13's "15 cannot be reproduced".
- L1-12's "96 GoManualConversion hand-owns": only 4 carry the attribute.
- L1-11's ":963 re-inserts prose on each rebuild": it inserts only when the section is created.
- L2-6's "pre-boxed hoisting can reach 0 bytes".
- L2-2's FIFO-shift remedy, which itself needs two stores.
- L4-3's "no lookup" benefit.
- L3-8's two witnesses: the strings plan uses tmpstring, and maphash never reaches the span operator.

**Open:** no finding came back uncertain. These inferences are still unverified:
- that Roslyn deduplicates identical u8 data within one module (L4-2);
- whether C# 14 first-class spans let a Span<byte> bind the operator implicitly (L3-8);
- that a hit at 16 B costs at least as much as the copy (L2-4, extrapolated);
- the PerfStringMatch millisecond estimates, which apply one machine's per-site costs to another's baseline;
- how often cross-thread evictions actually happen;
- whether putting the cache inside the operator stops it from inlining (L1-24, L2-11).
