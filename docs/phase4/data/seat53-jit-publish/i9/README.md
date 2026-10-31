# Seat #53 P1 on the i9: the second CPU

13th Gen Intel Core i9-13900K, win-x64, .NET runtime 10.0.14 (SDK 10.0.403); G-LAPTOP ran 10.0.11. The kit is [p1kit](../p1kit/README.md) at `88ac80ddbf`, run unmodified, 2026-10-31. Its validation counts equal G-LAPTOP's: 5,168 accessors marked, 0 left, 764 files differing between A and NI. The record is [section 9 of the design](../../../DESIGN-jit-inlining-tiering-publish.md).

- `p1-jit.tsv`/`.out` and `p1-aot.tsv`/`.out` hold the driver's 15 interleaved rounds, a discarded warm-up first. The arms are in `arms-jit.tsv` and `arms-aot.tsv`; `base` is H and `aa` is H built twice.
- `hashes.txt`: H and HA are byte-identical (the exe and the strconv, internal.strconv and crypto assemblies), so the band is run noise only.
- `s53-run.txt`: every step, with the WHEA count (0 throughout).
- `disasm/`: dumpbin of the Native AOT `Main` and internal `FormatBool`. H calls `FormatBool`, A's loop has no call, and NI inlines `FormatBool` but calls the NoInlining getters. So under ILC the control is half-inlined: both steps are inlining.
- Box note 1: a `Directory.Build.rsp` with `-m:4` did not bound `dotnet build`, which passes its own `-maxcpucount` (23 nodes sampled). So H's JIT build ran uncapped. This affects build time only.
- Box note 2: every other arm built under CPU affinity 0xF (206 of 206 build samples). A quiet-box check read 0 build processes before the timed rounds.
