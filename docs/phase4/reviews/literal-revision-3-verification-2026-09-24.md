# COORD verification of C2's string-literal revision 3 (d382b60677)

Read-only, 2026-09-24: two lenses (the start-up A/B; fix completeness and the recommendation), each contested finding re-checked by a skeptic. Evidence cites path:line@sha.

## Literal design rev 3 (d382b60677): note for the owner

**The decision stands.** Take the table, then G8/G10, then arm C, and leave the visible campaigns for later. Five findings survived their skeptic and change the ruling text:

1. **Tier 2 is not golib-only** (R2). Its order precondition adds a hook to every converter-emitted `package_info.cs`: 397 in src/core and 736 in the behavioral tests (DESIGN:1364-1365). That means a corpus re-baseline. The code a Go reader sees is unchanged.
2. **"Parity at every `@string` site" is false** (R1). About 28 production literals in tuple returns and tuple assignments are emitted as UTF-16, and the table cannot serve them (visitReturnStmt.go:349-354). Arm A or a UTF-16 path is still needed there. Tier 2 parity has also never been read on an AllocsPerRun row (R9).
3. **G10 needs read-only views first** (R4). Otherwise one `ToSpan` write corrupts every one-byte string in the process.
4. **The start-up cost falls on the everyday regimes** (L1-08). `dotnet run`, `dotnet build` and the test host never get R2R.
   - MEASURED on linux: hello world +41 ms tiered (375 to 416 ms exec-to-exit), and +38 to 58 ms at TC=0, where the min and the median disagree.
   - `dotnet run` and Windows were never measured.
   - Under R2R the cost is about 1-5 ms, not "2 ms or less". NativeAOT was never an arm.
5. **G12 reaches at most 36 sites plus some of the 158 no-verdict ones, not 216** (R6). Rank it lower.

**Start-up mitigations worth sizing (none measured yet):**
- **Hybrid lazy registration:** a miss runs that module's eager registration once. It reaches `dotnet run` and the test host. It still carries a per-module AllocsPerRun byte-reading hazard (L1-10, overstated). One probe settles it: image-range discovery under single-file.
- **Partial R2R of only the test host's registration initializers** (INFERENCE, L1-11).
- **A per-module blob** always misses under the address key. A reflection-over-RVA form has not been probed (L1-12).

## Fixes for C2 (all citations @d382b60677 unless noted)

1. **Restore the no-R2R scope** (L1-08). Say that `dotnet run`, `dotnet build` and the test host never get ReadyToRun, and scope ":1318 the one that removes the cost".
   - PublishReadyToRun is set only for a non-Library publish (csproj-template.xml:50-55) and is absent from test-csproj-template.xml:93-97.
   - COORD said this in 545eacef7f:19.
2. **Fix the R2R and AOT figures** (L1-07, R3).
   - Change ":1299 removes it" and ":1539-1540 2 ms or less" to "about 1-5 ms".
   - Evidence: min B-A is +4.9/+4.0, Boff-A is -3.0/-6.4, and net/http's Register alone takes 3.14 ms (results-linux.txt:7,10,13-14).
   - Drop "NativeAOT: measured above" (:1318). NativeAOT was not an arm (time.py:10).
3. **Record the spread at TC=0** (L1-03, R3).
   - Median and min B-A differ by about 20 ms at TC=0 only (results-linux.txt:6,9).
   - Record a spread measure and a min for Boff (time.py:18-20). Quote hello at TC=0 as 38-58 ms.
   - Withdraw ":1295 not the table" for TC=0: B-Boff is +19.2/+22.1 ms against Register's 1.94/6.91 ms.
   - Do not blame the inlined Lookup for that gap. B and Boff run the same dll (time.py:7-8).
4. **Per-literal cost and the Boff control** (L1-04, L1-05).
   - Publish jitMs, jitMethods and jitILBytes for A, B and Boff. LiteralTable.cs:31 already prints them, but nothing records them.
   - Call the 9-15 µs (:1297) a ratio, not a slope: tiered Boff-A rose x1.22 for x2.04 the literals.
   - Disclose that Boff also carries the inlined Lookup and the 4,096-slot static constructor (LiteralTable.cs:21,109-110).
5. **Reproducibility and labels** (L1-06, L1-15, L1-02).
   - Record the build configuration of the JIT legs (README.md:8).
   - The "test host's regime" label is accurate on JIT mode only. The host is a Release, single-file publish (testConversion.go:6532-6556).
   - Commit the driver, the string.cs:460 operator patch and report.tsv.
   - Disclose that genreg.py:14 skips `@"…"u8` literals (about 17 lines, under 1%).
   - Change :1263 to "every initializing import" (visitImportSpec.go:429).
6. **Record the three mitigations with their hazards** (L1-10, L1-11, L1-12).
   - Hybrid lazy registration: the in-window byte-reading hazard (testing.cs:743-755) and the range test it adds to every miss.
   - R2R for the test host: it changes the codegen of record (testConversion.go:6543-6553).
   - Per-module blob: explain why it misses under the address key (:979-980, :1036-1037).
7. **Carve out the UTF-16 sites** (R1, R9).
   - Literals in tuple returns and tuple assignments (visitReturnStmt.go:349-354, visitAssignStmt.go:1700-1703; e.g. runtime/symtab.cs:959) go through the counted `operator @string(string)` (string.cs:450).
   - Amend :1538 and :1552-1553, and restore revision 2's list (:887-889).
   - Mark parity as a prediction until AllocsPerRun is read on a Tier 2 build.
8. **Tier 2 footprint** (R2). The order hook (:1364-1365) edits every `package_info.cs`. Amend :1538 "no visible delta" and the zero-hunk premise at :877.
9. **G10 and G8** (R4, R5).
   - G10: add read-only views (string.cs:251-281) to its preconditions (:1466, :1549-1550).
   - G10: limit it to `string([]byte)` of length 1 (go1.24.13 runtime/string.go:144-150). `string(byte)` goes through intstring, which allocates when escaping (:291-298).
   - G8: state that exactness on literal operands needs Tier 2, and on sstring operands needs V-fix 11 (sstring.cs:453-458).
   - G8: fix ":1460-1461 always immutable". AliasOf and TransientAliasOf alias mutable storage (string.cs:121-154).
10. **G12 and G9** (R6, R7).
    - G12: in go1.24.13 a non-escaping `[]byte(const)` is a stack copy (walk/convert.go:277-292).
    - G12: `-m` does print "zero-copy" (escape.go:338-345), so :1487-1488 is false. The census regex at main.go:110 misses that line.
    - Re-rank G12 at :1557. Its reach is at most 36 plus some of 158 (census-windows.txt:66-68), not 216.
    - G9: the counted gain on chains with an sstring operand needs V-fix 11. The design's own example, time/time.cs:346, already counts 1, the same as Go.
11. **§8.2R evidence** (V13, R10, R12).
    - G2/G3/G4 and O1's 111 (:1445-1447, :1500) are not produced by the committed c2-escape-join/main.go (:242-431). That contradicts README.md:4, which says they can be regenerated.
    - Reconcile the no-verdict counts: 47/3,248 in the design against 20/3,107 in the committed census (census-windows.txt:12,17-18).
    - :1437 "under 1% in every row" is false. R5 noescape is 36/34/28 and J leak-heap is 3,090/2,849/2,848.
    - :1439 "the complete list" omits countrunes, `len(string(b))`, the 32-element stack buffers, concatbytes, and zero-copy for non-literal strings.
12. **Figures and pointers** (R13, L1-16, R11, V19, V2, L1-13).
    - :1346 "369K-713K" should read 369K-389K (output-round3-linux.txt:62-63).
    - Reconcile :1350's 3,094 with the probe's 3,092/3,095 and COORD's 3,419.
    - :1545 compares across runs, measured an inlined lookup, and ran with tiering off. Add the tiered miss tax (+7.56/+7.76 ns, :1414), a tiered run of the growable table, and the census of non-literal callers (:640-641).
    - Put supersession pointers in place at :1009-1021, :1048, :1200-1203 and :1221, and fix the :531 note.
    - Label the net/http test process "likely larger than +102 ms" (3,805 test literals, :1005).
