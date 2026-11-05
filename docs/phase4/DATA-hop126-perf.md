# DATA — the 1.26 hop's performance re-baseline at go.\* 1.26.10.2

> **State: EXECUTED (2026-11-04/05), awaiting S5.** These are point-in-time measurements for one release hop:
> windows 1 and 2 of [`../PLAN-perf-rebaseline.md`](../PLAN-perf-rebaseline.md) §10 and the PERF-REVIEW list they
> produced. The plan holds the protocol. This record holds the readings, so S5 can bank the Performance page from
> them, and a later hop can compare against them without taking them for rules. Sections are **appended and
> superseded, never overwritten.**
>
> **The raw files** are in [`DATA-hop126-perf/`](DATA-hop126-perf/):
> - every armab sample (`w1-*.tsv`, `w2-*.tsv`: program, arm, round, wall_ms, inner_ms, peak_bytes);
> - the runner's own reports (`*-runner-report.txt`);
> - the masked source hashes (`sources-*.tsv`);
> - all four AOT manifests, including the composed one the runner admitted from;
> - both window logs (box census, image hashes, phase times);
> - the verdict tables (`verdict*.tsv`).
>
> The runner's report keeps each arm's median, min..max and n, not each run. Those are the window 1 runner rows
> below. The armab rows keep every run.

## 1. The instrument and the legs

**Host:** the G perf laptop, an AMD Ryzen 5 PRO 6650U running Windows 11. On AC power with the Balanced scheme, and
0 build or toolchain processes at the end of each window. One idle `VBCSCompiler` was left from a pre-window build
at window 1's start.

**Toolchain:** SDK 10.0.400 with runtime 10.0.11. The AOT images were built on the i9 with SDK 10.0.403 and ILC
10.0.14, which is October's ILC. RID `win-x64` on every C# line, and `windows/amd64` (default `GOAMD64`) on every Go
line.

**Runner:** `226cd9a25d`, the pin seat. It landed as batch 159 (master `ce5072e657`) with the same runner bytes plus
docs only. Window 1 ran from the pushed tip by COORD's ruling, while landing waited on signing.

| leg | role | converter (sha256) | packages | Go |
|:--|:--|:--|:--|:--|
| `go1.26.10.2` | the column S5 banks | `7871e564…`, built by the i9 from `33589333b2`; G rebuilt it and got equal bytes | 1.26.10.2 from nuget.org, template-default float | go1.26.10 |
| `go1.24.13.7` | the comparand, re-timed in this session | `999da499…`, October's i9 build | 1.24.13.7, template-default float | go1.24.13 |
| `go1.26.10.1` | diagnostic, window 1 only | `ad6c6cee…`, October's i9 build | 1.26.10.1, **pinned** (`pin=true`) | go1.26.10 |

**Notes on these columns:**
- **The pin.** A converted program floats `GoStdLibVersion` (`1.26.*`). Once 1.26.10.2 had published, the unpinned
  `.1` leg restored `.2`, and the runner refused all 17 of its programs, correctly. COORD ruled the opt-in `pin=true`
  key (batch 159) for that leg only.
- **The `.2` column is the template default:** "what a consumer gets" floats to the newest release, and tonight that
  is 1.26.10.2. **The `.2` column would need `pin=true` on the day a 1.26.10.3 publishes**, and so would 1.24.13.7 if
  a 1.24.13.8 ever did.
- **The converter note:** after batch 153, `src/go2cs` at `33589333b2` differs from the release record `1ca308d85f` by
  `nugetFeed.go` (0c0b58bae1, `-nuget-map-feed`'s rehearsal order). The change is inert for these programs:
  - the i9 measured that a record-built converter (`2ce5572f…`) gives all 17 masked source hashes equal;
  - G's pass 2 converted the `.2` leg with `7871e564` and compared it with pass 1's record-built conversion: all 17
    masked source hashes are equal, and all 17 are distinct.
- **Packages:** a fresh `NUGET_PACKAGES` for this session. go.lib 1.26.10.2's `.nupkg.sha512` is
  `+kisZdiZLm1JcZps3MqU5AxxL5ZXmXWrAKFRvXqVE1hrxJM3HOEgB78cVBjWJEcLf22cnGdwqIJVpSTikmMJVQ==`. G's restore, the i9's
  restore and nuget.org's catalog `packageHash` agree.
- **ReadyToRun shape per leg**, read from the published files: `.2` publishes its own code IL-only, with golib
  ReadyToRun (seat #53). `1.24.13.7` and `.1` publish their own code ReadyToRun.

**AOT images.** These were admitted before window 2:
- The i9 built 36 images: 17 template-default, 2 second publishes and 17 with folding off, as 39 files totalling
  7,145,127,417 bytes, with `manifest.sha256` = `821ef315…`.
- G pulled them itself, through the owner-mapped drive to the i9's admin share; the i9 had no route into G.
  `sha256sum -c` read 38/38 OK.
- One composed manifest of 54 lines (`328f85de…`), in the runner's 7 columns: the i9's 36 lines, plus eighteen lines of
  October's 1.24.13.7 leg, re-keyed to the masked hash. In the re-key, each line's unmasked hash with October's path rewrite
  reproduced the original, and the unrewritten hash differed.
- The runner admitted every image by sha256, size and masked source hash: `.2` 17/17 and `1.24.13.7` 16/16, with
  `[Verify]` ok. Its one refusal is the ruled n/a: **1.24.13.7 TlsHandshake has no AOT image**. October's publish
  failed with LNK1183, a 6.59 GB COFF object past 4 GiB, at a 51 GB peak. So the 1.24.13.7 leg could not link TLS at
  all, and the `.2` TLS AOT below is its first reading.
- AOT output is size-deterministic, not byte-deterministic: the second publishes match in size but not in sha256.

## 2. The windows: validity

| | window 1 (JIT, R2R, TC0, Fd1, R2Ru, PGO0) | window 2 (Native AOT) |
|:--|:--|:--|
| time | 2026-11-04 20:56 – 22:47 | 2026-11-05 00:37 – 00:52 |
| relays during the window | none: `C:\go2cs-perf` newest file and count (67) unchanged | none: unchanged (161) |
| control, Fib (the void test) | +1.13 %, bootstrap 95 % [−0.34 %, +8.86 %]: **holds 0, not void** | +4.62 %, [−3.95 %, +6.94 %]: **holds 0, not void** |
| control, Sieve (reported only) | +12.0 % [+4.5 %, +24.6 %] | +13.2 % [+6.6 %, +33.8 %] |
| A/A null, AOT (start / end) | 0.13 % / 0.23 % | 0.34 % / 0.10 % |
| A/A null, JIT (start / end) | 1.7 % / 2.8 % | 1.8 % / 0.6 % |

Notes:
- The JIT null is wider than October's 0.3 %. Window 1's verdicts use 2.8 % as the JIT/R2R/TC0/Fd1 floor; window 2's
  use 0.34 %.
- Sieve's control drifts upward in both windows. It is reported only, as ruled; Fib carries the test.
- Repetitions: 5 runs (11 for Startup, Sort, RefLower, Iface, IfaceCall, Channel, and FieldView's AOT), one discarded
  warmup, legs interleaved.

## 3. The table (the 1.26.10.2 column, with the same session's 1.24.13.7 comparand)

Milliseconds, median: in-program time, except Startup, which is process wall time. In brackets, the ratio to Go. The
JIT and ReadyToRun ratios are against window 1's Go and the AOT ratio against window 2's Go, both from the same
binaries.

| Benchmark | Go | C# JIT | C# ReadyToRun | C# Native AOT | 1.24.13.7 JIT | 1.24.13.7 ReadyToRun | 1.24.13.7 AOT (fold off) |
|---|---:|---:|---:|---:|---:|---:|---:|
| Startup | 26.1 | 506.4 (19.40x) | 371.4 (14.23x) | 158.8 (6.40x) | 607.8 | 333.6 | 580.6 |
| Fib | 115.1 | 96.9 (0.84x) | 101.3 (0.88x) | 177.5 (1.52x) | 181.8 | 179.6 | 178.9 |
| Sieve | 62.8 | 134.0 (2.13x) | 135.7 (2.16x) | 295.0 (5.13x) | 135.2 | 343.4 | 320.6 |
| MatMul | 123.0 | 217.0 (1.76x) | 215.7 (1.75x) | 726.8 (6.17x) | 214.3 | 634.5 | 732.1 |
| String | 103.8 | 340.1 (3.28x) | 655.7 (6.32x) | 1396.5 (13.24x) | 732.8 | 1621.8 | 1394.2 |
| StringView | 9.6 | 21.9 (2.28x) | 21.7 (2.26x) | 19.2 (1.85x) | 18.9 | 645.6 | 24.1 |
| StringMatch | 174.7 | 776.2 (4.44x) | 789.2 (4.52x) | 1274.4 (7.33x) | 896.0 | 1469.5 | 1154.8 |
| Map | 661.7 | 251.0 (0.38x) | 252.0 (0.38x) | 285.6 (0.44x) | 455.7 | 659.5 | 307.0 |
| Sort | 140.1 | 252.5 (1.80x) | 252.4 (1.80x) | 543.6 (4.00x) | 429.1 | 361.7 | 542.6 |
| Channel | 39.8 | 78.5 (1.97x) | 83.7 (2.10x) | 103.6 (2.58x) | 108.7 | 162.9 | 116.9 |
| IfaceCall | 184.4 | 336.2 (1.82x) | 345.2 (1.87x) | 559.6 (3.12x) | 388.2 | 1510.7 | 520.1 |
| Iface | 90.4 | 375.3 (4.15x) | 376.6 (4.17x) | 572.8 (6.80x) | 533.8 | 844.8 | 564.3 |
| IfaceShell | 22.3 | 602.7 (27.03x) | 581.9 (26.09x) | 1177.5 (52.75x) | 936.3 | 991.6 | 1505.0 |
| IfaceShellChurn | 36.9 | 1083.4 (29.36x) | 1073.1 (29.08x) | 1712.0 (46.93x) | 1531.0 | 1229.9 | 1533.3 |
| FieldView | 41.7 | 1725.3 (41.37x) | 1911.4 (45.84x) | 2347.1 (57.76x) | 9449.3 | 6341.7 | 6055.6 |
| RefLower | 240.6 | 510.2 (2.12x) | 536.3 (2.23x) | 644.5 (2.69x) | 636.8 | 660.0 | 1875.5 |
| TlsHandshake | 128.2 | 1057.9 (8.25x) | 1078.7 (8.41x) | 2534.2 (20.41x) | 2405.6 | 1680.2 | n/a (LNK1183) |

The 1.24.13.7 AOT column was published with `IlcFoldIdenticalMethodBodies=false`, the documented workaround that
let that leg publish at all. The 1.26.10.2 AOT column is the template-default publish. The like-for-like AOTnf
column is in `verdict.tsv` as `Aot-N26`.

## 4. Section 8's rule, applied mechanically

Each `.2 / 1.24.13.7` median ratio moves only when |ratio − 1| exceeds the larger of the column's A/A null and the
row's own spread. The row's spread is the larger, across the two legs, of (max − min) / median.
`verdict.tsv` lists all 117 cells, and `verdict-dot1.tsv` the `.1 → .2` step. Counts against 1.24.13.7:

| column | improvement | within the floor | regression |
|:--|--:|--:|--:|
| JIT | 12 | 4 | 1 (StringView) |
| ReadyToRun | 15 | 1 | 1 (Startup) |
| Native AOT, template default | 7 | 7 | 2 (IfaceShellChurn, StringMatch) |
| Native AOT, both folding off | 7 | 7 | 2 (the same two) |
| TC0 (diagnostic) | 4 | 11 | 2 (IfaceShellChurn, StringMatch) |
| Fd1 (diagnostic) | 16 | 0 | 1 (Startup) |

## 5. Diagnostic arms (DATA only, never the page)

- **R2Ru**: `.2` published with `GoReadyToRunUserCode=true`, timed against the window's own `.2` R2R.
  - It is equal or slower on every row: StringView 605 vs 21.8 ms (about 28x), IfaceCall 1458 vs 340, String 1995 vs
    653, MatMul 589 vs 214, Sieve 265 vs 140.
  - It is equal on Startup (374 vs 374 ms wall), Sort, RefLower, FieldView and TLS.
  - So seat #53's IL-only default is the faster publish, and own-code R2R is not where Startup's cost is.
- **PGO0 (Sort, JIT, `DOTNET_TieredPGO=0`)**: `.2` goes from 254.0 to 303.8 ms (+19.6 %); 1.24.13.7 from 423.9 to
  471.7 (+11.3 %). Tiered PGO is worth about 20 % on Sort at `.2`.
- **The `.1 → .2` step** (pinned `.1`):
  - JIT, R2R and Fd1 improved broadly: Sort 0.56, IfaceCall 0.77 JIT and 0.23 R2R.
  - Regressions under the rule: Startup R2R +13.6 %, Fd1 +15.0 % and TC0 +7.2 %, and IfaceShell TC0 +7.2 %.

## 6. The PERF-REVIEW list

1. **The RefLower/Iface layout lottery: not present at `.2`.** Each row's second publish is its own A/A:
   - RefLower: `.2` 644.5 / 645.1 ms (0.1 %), 1.24.13.7 1875.5 / 1877.2 (0.1 %);
   - Iface: `.2` 572.8 / 571.6 (0.2 %), 1.24.13.7 564.3 / 556.2 (1.5 %).

   RefLower's AOT gain (0.34x) is real. Iface's AOT ratio (1.015) is within its own pair spread.
2. **Sort's tiering.** JIT goes from 429.1 (1.24.13.7) and 450.0 (`.1`) to 252.5 ms. TC0 is flat across all three
   legs (299 to 301), and PGO0 costs `.2` 19.6 %. So the gain is tiering reaching PGO'd tier-1 code sooner, not
   better full-opt code. That fits `.2`'s template call-counting delay of 0 (the `.1 → .2` step). R2R equals JIT at
   `.2` (252.4), because own code is IL-only.
3. **FieldView's AOT: resolved.** October read the `.1` AOT at about 10x slower (45.7 to 53.5 s). At `.2` it is
   2347 ms, against 1.24.13.7's 6055 (0.39x; folding off reads the same). AOT is still 1.36x the `.2` JIT and 57.8x
   Go.
4. **NEW: IfaceShellChurn and StringMatch regress in the full-opt arms only.**
   - AOT: +11.7 % and +10.4% (folding off: +11.2 % and +10.6 %).
   - TC0: +11.5 % and +21.2 %.
   - JIT, R2R and Fd1 all improve (0.71/0.87 and 0.87/0.54).
   - The same direction in AOT and TC0, and in neither tiered arm, points at full-opt code quality from a converter
     or golib change since 1.24.13.7, which tiering and PGO hide. A seat would bisect it on TC0, the cheap arm.
5. **NEW: Startup's ReadyToRun and Fd1 regress, +11.3 % and +13.5 % against 1.24.13.7** (+13.6 % and +15.0 % against
   `.1`).
   - This is not the IL-only own code: R2Ru's Startup is identical.
   - TC0 Startup also rose 7.2 % from `.1`, so part of it is startup work rather than tiering.
   - Candidates for a probe: the call-counting delay of 0, through the `DOTNET_TC_CallCountingDelayMs` override on the
     R2R binary; and the `.1 → .2` startup changes.
   - Startup's JIT (506 against 608) and AOT (159 against 581) both improved.
6. **NEW, marginal: StringView JIT +15.9 %** (21.9 against 18.9 ms), just over its 15.1 % own spread. Its R2R
   (0.03x) and AOT (0.80x) improved. Re-read it alone before it counts.
7. **NEW: TlsHandshake's first AOT reading is 2534 ms, 2.4x its own `.2` JIT** (1058) and 20.4x Go. Folding off reads
   the same (2537). This is not a regression, since 1.24.13.7 could not link it, but it is the only row where AOT
   trails JIT by more than 2x.
8. **Noted: the JIT A/A null is 1.7 to 2.8 %, against October's 0.3 %.** The fixed `.2` JIT Fib binary is noisier;
   tiered start with a call-counting delay of 0 is one candidate, unmeasured. It sets the window 1 floor above.
9. **Noted: Sieve's Go control drifts +12 to +13 % within each window** (reported only; Fib holds). It is worth a
   thermal look before the next hop.

## 7. What S5 needs from here

- The page's 1.26 column: §3's `.2` columns. Window 1's runner report has the full per-arm medians.
- The legend:
  - the `.2` R2R shape (own code IL-only, golib R2R);
  - the template-default float to 1.26.10.2, and the pin rule for a later release;
  - the AOT note: 1.24.13.7 used folding off, and its TLS could not link.
- History: the 1.24.13.7 columns above, with their folding-off note.
- The fairness check, at the bank tip, against this record's runner `226cd9a25d` / `ce5072e657`.

---

## 8. Appendix, 2026-11-05: the PERF-REVIEW probes (items 4 to 7)

Appended after §6's list. COORD ordered the probes 6 → 5 → 4 and ruled on each. Each probe had its own window, with
the same rules as windows 1 and 2:
- OPEN and CLOSE posted, and relays held;
- the Fib control's bootstrap interval holds 0 in every one;
- the drop folder's newest file and count were unchanged within each window.

Every prediction was written before its window and is scored here as worded. All the raw files are in
[`DATA-hop126-perf/`](DATA-hop126-perf/) under the `p6-`, `p5-`, `pi-`, `px-`, `pb1-`, `pb2-` and `pw-` prefixes, with
the `*-prediction*.txt` notes, the logs, `bisect-build-log.txt` and `rangeB-commits.txt`. Timing is armab, October's
foldab plus a per-arm environment column. Its environment column was shown to take effect by a bogus `DOTNET_ROOT` arm
that failed Verify.

### 8.1 Correction to §6 item 7

§6 item 7 says TlsHandshake is "the only row where AOT trails JIT by more than 2x". **That is wrong.** By §3,
Native AOT trails the `.2` JIT by more than 2x on five rows: String 4.11x, MatMul 3.35x, TlsHandshake 2.40x, Sieve
2.20x and Sort 2.15x. TlsHandshake's reading stands as its first AOT reading. It is not the only such row. COORD's S5
drafter caught it. The page does not repeat the claim.

### 8.2 Item 6, StringView JIT: CLOSED as noise (window 01:07-01:09)

StringView JIT medians over 21 rounds, in ms:

| `.2` | its A/A twin | `.1` | 1.24.13.7 | `.2` with call-counting delay 100 |
|--:|--:|--:|--:|--:|
| 19.93 | 20.77 | 18.90 | 19.08 | 18.85 |

- `.2` / 1.24.13.7 = 1.044, against the row's own spread of 18.9%: **within**.
- Window 1's +15.9% was a noisy `.2` reading (21.9 ms then, 19.9 now).
- Prediction P1 (`[1.08, 1.20]`, real) **failed low**, which triggered its own pre-stated rule: item 6 closes.
- P2 (1.054) and P3 (0.946) sat just outside ±5%, and P4 (A/A within 3%) failed at 4.2%. All four are inside the
  A/A spread.

### 8.3 Item 5, the Startup ReadyToRun/Fd1 step: RULED a tradeoff plus creep (window 01:10-01:13)

PerfStartup process wall, medians over 21 rounds (IQR 1-2.6%), in ms. The "delay 100" arm is a copy of the `.2`
binary with only its `runtimeconfig.json` changed (CallCountingDelayMs 0 → 100); every other file is identical.

| arm | `.2` | `.2` A/A | `.2` delay 100 | `.1` | 1.24.13.7 |
|:--|--:|--:|--:|--:|--:|
| ReadyToRun | 371.6 | 372.8 | 342.9 | 328.5 | 333.4 |
| Fd1 | 367.9 | | 341.8 | 327.3 | 334.0 |
| TC0 (the JIT binary) | 795.9 | | | 744.7 | |

**Scoring:** every prediction held. P1 R2R delay 100 / default = 0.923, in `[0.88, 0.96]`. P2 delay 100 / `.1` =
1.044, in `[1.00, 1.08]`. P3 Fd1 read 0.929 and 1.044. P4 A/A 0.3%.

The prediction note itself was not written to disk: a chained command stopped at a line-ending-only `diff` before
writing it. The same prediction, worded identically, was posted in the window's OPEN at 01:10:38, before the window
started at 01:10:55. That post is kept as `p5-prediction-open-post.txt`.

Of the `.1` → `.2` R2R step (+43 ms, +13.1%):
- **About 29 ms, the larger part, is the `.2` template's call-counting delay of 0.** COORD ruled it a tradeoff, not a
  defect: the same setting is Sort's 44% JIT gain (§6 item 2). The S5 legend states about 30 ms, and docs/Tuning.md
  says that `GoCallCountingDelayMs` restores the old start.
- **About 14 ms (+4.4%) is startup work that grew from `.1` to `.2`.** It is just as visible under TC0 (+6.9%), and it
  joined item 4 (§8.4.5).
- Against 1.24.13.7, `.2` with delay 100 is +2.9%.

### 8.4 Item 4, the full-optimization regressions: two causes, both named

**The instrument.** Every commit is built in-tree in one detached bisect worktree with that commit's own runner
(`--phase transpile,build --no-aot`), against that commit's `src/core` by project reference. The program's Go source is
held equal to `1ca308d85f`'s:
- IfaceShellChurn is overlaid where a commit lacks it;
- an equality check, proven both ways on a CRLF checkout, refuses any difference.

Each binary is timed under `DOTNET_TieredCompilation=0`. One caution for anyone re-reading the logs:
`bisect-build-log.txt`'s "exe sha256" values hash the native apphost and identify nothing. The comparisons here use the
program and golib `.dll`s.

**The stop rule passed (window 01:46-01:51).** In-tree, the `.7` → `.1` step reproduces the packaged one: IfaceShellChurn
+10.6% (packaged +11.8%) and StringMatch +13.8% (packaged +11.7%). TC0 medians, in ms:

| program | `.7` b0ef977fda | pre-hop ae0c4c6e43 | hop 458b159eea | `.1` 5100d1b16c | `.2` 1ca308d85f |
|:--|--:|--:|--:|--:|--:|
| IfaceShellChurn | 1559.4 | 1561.2 | 1725.9 | 1724.0 | 1704.1 |
| StringMatch | 960.5 | 960.8 | 1090.2 | 1092.7 | 1160.6 |
| Startup (wall) | | | | 708.8 | 760.2 |

The 58 pre-hop commits are flat, and **the whole step sits on the Go 1.26 hop merge `458b159eea`**, as predicted.
COORD steered away from a blind bisect inside the hop branch's 1,615 commits. Instead: an axis split, an emission diff,
then the methods the JIT compiles.

**8.4.1 The axis split (window 02:03-02:08).** Each cross arm takes the program emission from one side of the hop and
golib, the generators and `src/core` from the other. Both built: their program `.dll`s are distinct, and each golib
matches its own side. TC0, in ms:

| program | pre-hop | hop | hop library + pre-hop emission | pre-hop library + hop emission |
|:--|--:|--:|--:|--:|
| IfaceShellChurn | 1564.1 | 1711.9 | 1747.0 | 1599.8 |
| StringMatch | 959.1 | 1081.6 | 963.9 | 2076.8 |

**8.4.2 StringMatch: the literal emission.**
- **The cause:** on the hop's own library, the pre-hop emission runs 964 ms and the hop emission 1082 ms (+12.3%),
  which is the whole step. My prediction (the library axis) failed here.
- **The shape:** the hop converter emits Go string literals as plain C# `"…"` literals, converted at each use through
  golib's `implicit operator @string(string)` → `StringLiteralCache.Get` (`8fbc2e5477`, 2026-10-19, the file's only
  commit). The pre-hop converter emitted `"…"u8` literals and hoisted repeated ones into `static readonly @string`
  fields.
- **The cost:** 20M iterations, so about 5.9 ns per iteration, or roughly 0.9 ns per literal use under full opt.
- **The disassembly** (`dis-stringmatch-*.txt`, kindName under TC0; Tier1 has the same shape):
  - the hit inlines;
  - it costs three dependent loads and a literal-address spill;
  - the `Slow` call makes kindName a non-leaf, with a 136-byte frame and GC-temp zeroing on every call;
  - the hoisted emission compiles kindName to a frameless leaf.
- **The follow-up:** that design note became H's seat `claude/h-literal-cache-hit` (`b061a01052`), a golib-only change
  with no emission change, timed in §8.4.4.

**8.4.3 IfaceShellChurn: the library, golib's shell-instance cache.**
- **How it was found:** the generated-code diff (`EmitCompilerGeneratedFiles`, both trees) shows nothing per-assert.
  The diff of the methods the JIT compiles in a TC0 run (`jit-methods-churn-*.txt`) shows the hop build reaching new
  assert machinery: `AdapterBinder.CacheShellInstances` around `BuildObjectShellFactory`/`BuildGenericShellFactory`,
  and `AdapterRegistry.TryGetAdapterFactory`.
- **The cache:** DESIGN-iface-shell-caching P3, from `f9e586d5f0` (2026-10-12), reworked 2026-10-21 to a last-shell
  `WeakReference` slot.
- **The miss:** IfaceShellChurn asserts a fresh value every time, so every assert misses. A miss is a `TryGetTarget`,
  a factory build, a new `LastShell` pair and a weak-handle write.

**8.4.4 The confirm and the policy pair (window 04:28-04:41).** Means of each arm's A/A medians; the A/A pairs agree
within 0.1-3.4%.

The shell cache, TC0, in ms. The "no cache" arms are built with golib's own `Go2csNoShellCache` switch (each no-cache
`golib.dll` differs from its cached twin):

| program | pre-hop | hop | hop, no cache | `.2` | `.2`, no cache |
|:--|--:|--:|--:|--:|--:|
| IfaceShellChurn | 1570.5 | 1746.4 | 1632.9 | 1730.1 | 1646.6 |
| IfaceShell | | 821.4 | 1082.7 | 829.5 | 1075.1 |

- **RULED: KEPT, a designed tradeoff.** The cache costs IfaceShellChurn +7.0% (hop) and +5.1% (`.2`), and saves
  IfaceShell 24% and 23%. It is within `run-shell-churn-bound.ps1`'s own 1.15x budget.
- **My prediction (no-cache Churn ≈ pre-hop, within 2%) FAILED.** No-cache Churn is still +4.0% over pre-hop, so the
  cache is about two-thirds of the hop step (114 of 176 ms).
- **RULED: RECORDED, no hop-branch bisect.** The remaining +4% is unlocalized hop-library drift.

**The owner's literal-policy pair.** PerfStringMatch at the SHIPPING commit, converted by the `.2` converter (`7871e564`)
against go.\* 1.26.10.2 as a consumer gets it, in ms:
- **P:** the default plain emission, with the released golib.
- **H:** `-hoist-literals`, the hybrid (7 fields hoisted).
- **C0:** P's program with golib built at master `9d5d67c735`.
- **C:** P's program with golib at H's seat `b061a01052`.

Only `golib.dll` is swapped: IL-only in every arm (checked), version 1.26.10.2. Each AOT arm is published from a
private package cache. C against C0 isolates H's change; C0 against P is master's other golib drift.

| | P | H | C0 | C | H/P | C/C0 | C0/P |
|:--|--:|--:|--:|--:|--:|--:|--:|
| Native AOT | 1315.6 | 1176.6 | 1295.8 | 1241.3 | 0.894 | 0.958 | 0.985 |
| TC0 | 1224.4 | 972.2 | 1246.6 | 1169.3 | 0.794 | 0.938 | 1.018 |
| default JIT | 799.8 | 601.4 | 801.3 | 728.1 | 0.752 | 0.909 | 1.002 |

- **My prediction (H/P in `[0.90, 0.96]`) FAILED LOW** in all three columns. Hoisting buys 11% on AOT, 21% on TC0 and
  25% under default JIT.
- **H's own TC0 band (0.94-0.96) held at its edge (0.938).**
- **The residual** of plain plus the cheaper cache, against hoisting, is about 6% on AOT, 20% on TC0 and 21% on JIT for
  this row.
- **COORD's question, why the 10-30 record read 0.991:** that figure was DESIGN-literal-hoisting-survival.md §2's
  pre-#51 "full plain emission" row. Its own record superseded it; after #51 the honest baseline is +6.6%, and the i9's
  §9.2 macro read 1.13 on the JIT and 1.004 on AOT, an AOT asymmetry that record leaves unexplained. The cache path did
  not change after 10-30. The policy goes to the owner with this table.

**8.4.5 Range B, `.1` → `.2`: no single cause (windows 02:24-02:28 and 04:52-04:54).** The quartiles of the 176
first-parent commits are lines 44/88/132 of `rangeB-commits.txt`. TC0 medians, in ms:

| | `.1` | Q1 a93662afc6 | Q2 801497a338 | Q3 de5696c935 | `.2` |
|:--|--:|--:|--:|--:|--:|
| Startup (wall, IQR 0.7-1.3%) | 713.6 | 725.2 | 722.8 | 740.2 | 762.0 |
| StringMatch (IQR 1.8-5.8%) | 1096.5 | 1134.2 | 1116.1 | 1121.8 | 1156.3 |

The one ruled further cut, inside Q3 → `.2` (IQR 2.1-5.1%, a noisier window), in ms:

| | Q3 | 64d145b2c4 | 09d205c94f | 263576e2de | `.2` |
|:--|--:|--:|--:|--:|--:|
| Startup (wall) | 760.7 | 755.3 | 789.9 | 779.0 | 780.0 |

- **RULED, RECORDED:** Startup's `.1` → `.2` startup-work part is **creep with no single cause**.
- The largest move sits between `64d145b2c4` and `09d205c94f` (lines 144-154, batches 125-134), at about one IQR.
- StringMatch's `.1` → `.2` "second step" is not monotone. It is **recorded as noise around +3-5%**.

**The parity question (window 02:24-02:28).** On the hop tree, in-tree, under DEFAULT tiering: StringMatch plain is
1047.4 ms and the hoisted emission 923.2 ms, +13.4%. The step is not full-opt-only, which the shipping-commit pair
above confirmed.

### 8.5 Item 7

TlsHandshake's AOT at 2.4x its JIT: QUEUED behind item 4 (COORD), with no seat yet. §8.1 corrects how §6 stated it.
