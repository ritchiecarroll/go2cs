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

---

## 9. Appendix, 2026-11-05: the cross-box literal-policy check (the owner's "Run i9 check")

Appended after §8. The owner asked for this check before ruling on the literal policy (§8.4.4). The question: G read
hoisted/plain (H/P) 0.894 on Native AOT, while the i9's 10-30 macro read about 1.004. With the same images on both
boxes, is the difference the box or the harness? The owner then ruled **"Keep plain"** with this result in hand:
programs stay plain by default, the standard library stays hybrid, and `-hoist-literals` stays the opt-in.

The raw files are in [`DATA-hop126-perf/xbox/`](DATA-hop126-perf/xbox/):
- `i9-run1/` to `i9-run3/`, the i9's own output folders, copied over the owner-mapped drive. Runs 2 and 3 are
  byte-identical to the bundle's `out-<timestamp>` folders;
- `G-v1/` and `G-v2/`, G's same-script runs (`G-v1/run.log` line 1: the host name is redacted to G; v1's `run.sh` printed it, and later versions do not);
- `protocol/`: each version's `run.sh` and `PROTOCOL` text, `census.ps1`, `PREDICTION.txt`, the v2 and v3 manifests,
  the v3 reader `v3read.sh` and armab's source.

### 9.1 The bundle

G staged the four PerfStringMatch Native AOT images from the §8.4.4 window, byte-identical to that window's log:

| arm | what it is | sha256 (prefix) |
|:--|:--|:--|
| P | the default plain emission, with the released golib | `3a7b4d5e1aee1aa8` |
| H | `-hoist-literals` | `ad83322c646f40d9` |
| C0 | P's program, with golib at master `9d5d67c735` | `729ba8ef34fa459d` |
| C | P's program, with golib at H's cheaper-hit seat `b061a01052` | `5f6c396f238c610c` |

The bundle also carries armab, the 1.24.13 Go Fib/Sieve control binaries, `boot.sh` and `run.sh`, plus the protocol and
the prediction. G pushed it to the i9 at `C:\go2cs-perf\xbox-literal\` over the owner-mapped drive; the i9 has no
route into G. Its `manifest.sha256` went v1 `f0fda1f7…`, then v2 `1fe9d45a…`, then v3 `d04cb912…`; the images
never changed. Each version was verified over the drive, and `run.sh` re-checks the manifest on the box itself before
timing.

The protocol is G's 04:28 window repeated:
- a box census, a discarded warm control, then the control start;
- the 8 arms, each image twice (A and AA), interleaved with the order alternating;
- the control end, the box census, and the Fib bootstrap.

### 9.2 The protocol versions, each stated BEFORE the run it governed

- **v1** (11 rounds). Valid only if: rc 0; 0 build/toolchain processes at both ends on AC power; each A/A pair within
  1%; and the Fib control's bootstrap 95% interval holds 0.
- **v2** (21 rounds), amended after run1. A `dotnet.exe` whose command line runs `pwsh.dll` is an interactive shell. It
  is admitted if the same PID is listed at both ends and its CPU time grows by less than 1 s. G proved the
  classifier both ways: two dotnet-hosted pwsh processes were admitted, and a live dotnet running armab was counted.
  The 1% A/A rule was kept.
- **v3** (21 rounds), amended after run2, for run3 onward only.
  - Validity: every arm's A/A gap is under 3%; above that, the run is disturbed and void.
  - Reading: the plan's §8 rule. An arm's value is the mean of its A and AA medians. A ratio's floor is the largest of
    its two arms' A/A gaps and IQRs, and the ratio is "moved" only if |ratio − 1| exceeds that floor.
  - The reason: v1/v2's fixed 1% had been calibrated on G, whose AOT A/A gaps are 0.02-0.66%. The i9 showed one arm
    above 1% per run, a different arm each time.

### 9.3 The runs

| run | box | protocol | verdict | reason |
|:--|:--|:--|:--|:--|
| i9 run1 | i9-13900K | v1 | **INVALID**, not read | rule 2: the owner's idle dotnet-hosted pwsh terminal counted as a toolchain process; rule 3: C0 1.57%, C 1.21% |
| i9 run2 | i9-13900K | v2 | **INVALID**, not read | rule 3: P 1.26% (the i9 computed 1.24%, the same gap measured from the other side) |
| i9 run3 | i9-13900K | v3 | **VALID** | rc 0; 0 processes; the pwsh host grew +0.04 s; A/A 0.13-0.27%; Fib −0.28% [−1.80%, +4.54%] |
| G v1 | AMD 6650U | v1 | valid | the bundle's self-check before shipping; A/A 0.09-0.31%; Fib +0.55% [−3.33%, +2.43%] |
| G v2 | AMD 6650U | v2, and valid under v3 too | valid | A/A 0.08-0.66%; Fib −0.67% [−2.49%, +6.35%] |

The i9's Sieve control (reported only) drifted +1.02% [+0.02%, +2.02%] in run3, not holding 0.

### 9.4 The result: both boxes under v3

PerfStringMatch, Native AOT, the same four images. Each arm is the mean of its A and AA medians, in ms:

| box | P | H | C0 | C |
|:--|--:|--:|--:|--:|
| G (AMD Ryzen 5 PRO 6650U), v2 run | 1288.5 | 1171.8 | 1284.5 | 1245.9 |
| i9 (Intel i9-13900K), run3 | 582.1 | 550.0 | 584.9 | 581.9 |

| box | H/P | C/C0 | C0/P |
|:--|:--|:--|:--|
| G | **0.909**, floor 1.2%: moved | **0.970**, floor 1.6%: moved | 0.997, floor 1.6%: within |
| i9 | **0.945**, floor 1.8%: moved | 0.995, floor 1.6%: within | 1.005, floor 1.6%: within |

G's other valid readings of the same images were H/P 0.894 and 0.899, and C/C0 0.958 and 0.959.

**The prediction, scored as worded** (`protocol/PREDICTION.txt`, written before any i9 run). It said H/P on the i9 in
`[0.86, 0.93]`; if H/P were 0.97 or above instead, the cause was the box. **FAILED HIGH:** the reading was 0.945, which
also falls short of the 0.97 box line. So both are true: the images carry the gap, and the box sets its size.

**What the owner ruled with:**
- With the shipping commit's images, hoisting wins under Native AOT on both boxes, by about 9% (G) and 5.5% (i9).
- The 10-30 macro's 1.004 does not reproduce on the i9 itself with these images. That reading belonged to its own
  images at master `893d48976f`.
- H's cheaper cache hit (`b061a01052`) is box-dependent: 3% on G, within the floor on the i9.
- The rest of master's golib drift (C0/P) is within the floor on both boxes.
- The residual price of "programs plain" on a literal-heavy row is about 5-9% under Native AOT, and larger under the
  JIT on G (H/P 0.752, §8.4.4). The i9 timed AOT only.

---

## 10. Appendix, 2026-11-05: the tuning sweep (-hoist-literals, call-counting delay, TieredPGO)

Appended after §9. The owner asked for the tuning page to carry before-and-after tables, one per setting, showing every row a
setting moves beyond its floor in EITHER direction. This section holds the new measurement. The other settings' tables
come from data already recorded:
- TC0: §3 and `w1-runner-report.txt`;
- AOT folding off: window 2's D26 against N26, `verdict.tsv`;
- `GoReadyToRunUserCode` (R2Ru): §5, `w1-r2ru-*.tsv`.

The shell-cache switch (`Go2csNoShellCache`) is not user-facing. It compiles into golib, and the published go.lib is built
with the cache, so it gets no table (§8.4.4 keeps its tradeoff).

The raw files are in [`DATA-hop126-perf/sweep/`](DATA-hop126-perf/sweep/):
- `sb.sh` (the builds) and `sweep-build-log.txt`;
- `sww.sh` (the window), `sww-log.txt` and the raw samples `sww-arms-1.tsv` and `sww-arms-2.tsv`;
- the controls;
- `sweep-verdict.tsv`;
- the reader `sweepread.sh` and its two proof inputs, `rd-ctl.tsv` and `rd-ctl2.tsv`.

**The builds** (not timed). All 17 programs, with Go sources equal to the shipping commit `33589333b2` (checked), each
converted twice by the `.2` converter (`7871e564`), `-recurse=nuget`, against go.\* 1.26.10.2 as a consumer gets it:
- **P:** plain, the default;
- **H:** `-hoist-literals`.

Each conversion got the template JIT build and the template ReadyToRun publish. That is 34 conversions and 68 builds, all
rc 0. Hoisting creates fields in three programs only: StringMatch (7), TlsHandshake (3) and StringView (1). The **D** arms
are copies of P's JIT and R2R folders with ONLY `runtimeconfig.json`'s `CallCountingDelayMs` changed from 0 to 100, .NET's
own default.

**The window** (13:33-14:31):
- per program, 11 rounds, interleaved, every arm twice (A/A);
- the arms: JP and JH (JIT, plain and hoist), RP and RH (R2R, plain and hoist), JD and RD (plain with delay 100) and JG
  (JIT plain under `DOTNET_TieredPGO=0`);
- valid: rc 0, 0 build processes at both ends, and Fib −0.08% [−3.23%, +2.20%] holds 0.

**The reading** is the §8 rule, with the reader proven both ways on §9's data. It reads StringMatch's known AOT pair as
0.894 moved, and one image under two labels as 0.998 within.
- An arm's value is the mean of its A and AA medians: in-program time, except Startup's process wall.
- A ratio's floor is the largest of its two arms' A/A gaps and IQRs. A cell moves only beyond its floor.
- Thirteen arms have A/A gaps of 3% or more, and each widens its cells' floors:
  - Channel: JD 4.48%, JG 6.49%, JH 3.93%, JP 5.10%, RH 5.37%, RP 4.46%;
  - Iface RP 5.24%; IfaceCall JP 10.97%;
  - String: JG 4.95%, JH 3.76%;
  - StringMatch JD 3.20%;
  - StringView: JG 7.95%, JP 11.77%.

**The predictions, written before the window and scored as worded:**
- Hoist moves StringMatch strongly (JIT ≤ 0.80, R2R ≤ 0.85), IfaceShellChurn little, the rest within: **HELD** (JIT 0.752,
  R2R 0.676; every other row within).
- Delay 100 moves Startup and slows Sort, the rest mostly within: **FAILED**. It moves 14 of 17 rows under JIT and 12 of 17
  under R2R.
- TieredPGO=0 slows Sort and the interface-dispatch rows, the rest within: **FAILED**. It slows 13 of 17.

**The headline: most of the `.1` → `.2` JIT gain is the template's `CallCountingDelayMs=0`.** The delay-100 JIT medians
sit on window 1's `.1` JIT values: Fib 213.4 against 213.0, String 734.3 against 735.1, Map 470.9 against 473.3, and Sort
434.8 against 450.0.

**The framing the tuning page must carry:** these programs run for about 0.1 to 2.5 s, so the delay decides how much of
each run executes before tier-1 code. In a long-running service the delay is only a start-up transient. Startup itself is
the one row that is faster with the delay: −3.7% JIT, −7.0% R2R.

**No Native AOT step is needed.** Hoisting moved only StringMatch, whose AOT pair is §9 (0.909 G, 0.945 i9). The delay and
TieredPGO are tiering settings, which Native AOT does not have.

Every table below, in ms (variant against baseline):

### Hoist under JIT (JH/JP)

| program | variant | baseline | ratio | change | floor | reading |
|:--|--:|--:|--:|--:|--:|:--|
| Channel | 80.0 | 80.7 | 0.992 | -0.8% | 19.9% | within |
| Fib | 97.2 | 97.6 | 0.996 | -0.4% | 3.0% | within |
| FieldView | 1732.7 | 1723.7 | 1.005 | +0.5% | 1.7% | within |
| Iface | 377.1 | 374.7 | 1.006 | +0.6% | 6.9% | within |
| IfaceCall | 340.7 | 358.3 | 0.951 | -4.9% | 11.3% | within |
| IfaceShell | 598.0 | 600.4 | 0.996 | -0.4% | 4.1% | within |
| IfaceShellChurn | 1086.6 | 1076.8 | 1.009 | +0.9% | 2.9% | within |
| Map | 253.7 | 253.6 | 1.000 | +0.0% | 4.4% | within |
| MatMul | 219.2 | 213.2 | 1.028 | +2.8% | 4.9% | within |
| RefLower | 510.4 | 511.3 | 0.998 | -0.2% | 2.2% | within |
| Sieve | 136.3 | 139.3 | 0.978 | -2.2% | 5.9% | within |
| Sort | 255.4 | 255.1 | 1.001 | +0.1% | 1.8% | within |
| Startup | 507.4 | 502.0 | 1.011 | +1.1% | 2.9% | within |
| String | 339.0 | 339.0 | 1.000 | +0.0% | 16.7% | within |
| StringMatch | 594.7 | 791.2 | 0.752 | -24.8% | 10.5% | **moved** |
| StringView | 22.4 | 20.9 | 1.070 | +7.0% | 18.0% | within |
| TlsHandshake | 1069.0 | 1071.6 | 0.998 | -0.2% | 2.0% | within |

### Hoist under ReadyToRun (RH/RP)

| program | variant | baseline | ratio | change | floor | reading |
|:--|--:|--:|--:|--:|--:|:--|
| Channel | 81.1 | 82.5 | 0.983 | -1.7% | 31.9% | within |
| Fib | 100.4 | 101.4 | 0.990 | -1.0% | 5.3% | within |
| FieldView | 1914.5 | 1936.5 | 0.989 | -1.1% | 1.5% | within |
| Iface | 371.0 | 380.9 | 0.974 | -2.6% | 10.9% | within |
| IfaceCall | 353.9 | 349.0 | 1.014 | +1.4% | 20.6% | within |
| IfaceShell | 574.5 | 575.0 | 0.999 | -0.1% | 4.9% | within |
| IfaceShellChurn | 1093.9 | 1090.9 | 1.003 | +0.3% | 4.0% | within |
| Map | 255.0 | 248.7 | 1.026 | +2.6% | 5.2% | within |
| MatMul | 216.8 | 215.4 | 1.007 | +0.7% | 2.2% | within |
| RefLower | 537.4 | 536.8 | 1.001 | +0.1% | 2.3% | within |
| Sieve | 134.1 | 134.7 | 0.995 | -0.5% | 2.7% | within |
| Sort | 254.4 | 255.0 | 0.998 | -0.2% | 2.0% | within |
| Startup | 373.5 | 371.5 | 1.005 | +0.5% | 2.8% | within |
| String | 659.5 | 656.5 | 1.005 | +0.5% | 45.5% | within |
| StringMatch | 566.8 | 838.8 | 0.676 | -32.4% | 9.5% | **moved** |
| StringView | 21.8 | 21.7 | 1.006 | +0.6% | 16.9% | within |
| TlsHandshake | 1081.4 | 1078.2 | 1.003 | +0.3% | 2.0% | within |

### Delay 100 under JIT (JD/JP)

| program | variant | baseline | ratio | change | floor | reading |
|:--|--:|--:|--:|--:|--:|:--|
| Channel | 126.1 | 80.7 | 1.563 | +56.3% | 19.9% | **moved** |
| Fib | 213.4 | 97.6 | 2.186 | +118.6% | 3.0% | **moved** |
| FieldView | 1959.8 | 1723.7 | 1.137 | +13.7% | 1.7% | **moved** |
| Iface | 558.8 | 374.7 | 1.491 | +49.1% | 4.5% | **moved** |
| IfaceCall | 435.8 | 358.3 | 1.216 | +21.6% | 11.3% | **moved** |
| IfaceShell | 723.0 | 600.4 | 1.204 | +20.4% | 4.1% | **moved** |
| IfaceShellChurn | 1574.1 | 1076.8 | 1.462 | +46.2% | 2.0% | **moved** |
| Map | 470.9 | 253.6 | 1.857 | +85.7% | 4.3% | **moved** |
| MatMul | 212.9 | 213.2 | 0.999 | -0.1% | 4.9% | within |
| RefLower | 752.4 | 511.3 | 1.472 | +47.2% | 2.1% | **moved** |
| Sieve | 134.8 | 139.3 | 0.967 | -3.3% | 5.5% | within |
| Sort | 434.8 | 255.1 | 1.704 | +70.4% | 2.3% | **moved** |
| Startup | 483.5 | 502.0 | 0.963 | -3.7% | 2.9% | **moved** |
| String | 734.3 | 339.0 | 2.166 | +116.6% | 16.7% | **moved** |
| StringMatch | 1125.8 | 791.2 | 1.423 | +42.3% | 8.4% | **moved** |
| StringView | 18.8 | 20.9 | 0.897 | -10.3% | 18.0% | within |
| TlsHandshake | 2053.3 | 1071.6 | 1.916 | +91.6% | 7.6% | **moved** |

### Delay 100 under ReadyToRun (RD/RP)

| program | variant | baseline | ratio | change | floor | reading |
|:--|--:|--:|--:|--:|--:|:--|
| Channel | 105.2 | 82.5 | 1.275 | +27.5% | 31.9% | within |
| Fib | 213.2 | 101.4 | 2.102 | +110.2% | 5.0% | **moved** |
| FieldView | 2089.3 | 1936.5 | 1.079 | +7.9% | 1.3% | **moved** |
| Iface | 616.7 | 380.9 | 1.619 | +61.9% | 10.9% | **moved** |
| IfaceCall | 506.5 | 349.0 | 1.451 | +45.1% | 14.5% | **moved** |
| IfaceShell | 770.4 | 575.0 | 1.340 | +34.0% | 4.9% | **moved** |
| IfaceShellChurn | 1269.0 | 1090.9 | 1.163 | +16.3% | 4.0% | **moved** |
| Map | 466.1 | 248.7 | 1.874 | +87.4% | 5.2% | **moved** |
| MatMul | 215.5 | 215.4 | 1.001 | +0.1% | 2.9% | within |
| RefLower | 761.2 | 536.8 | 1.418 | +41.8% | 2.3% | **moved** |
| Sieve | 132.7 | 134.7 | 0.986 | -1.4% | 1.7% | within |
| Sort | 437.6 | 255.0 | 1.716 | +71.6% | 3.1% | **moved** |
| Startup | 345.5 | 371.5 | 0.930 | -7.0% | 2.6% | **moved** |
| String | 810.1 | 656.5 | 1.234 | +23.4% | 44.7% | within |
| StringMatch | 1096.0 | 838.8 | 1.307 | +30.7% | 9.5% | **moved** |
| StringView | 18.4 | 21.7 | 0.848 | -15.2% | 16.9% | within |
| TlsHandshake | 1688.7 | 1078.2 | 1.566 | +56.6% | 6.6% | **moved** |

### TieredPGO=0 under JIT (JG/JP)

| program | variant | baseline | ratio | change | floor | reading |
|:--|--:|--:|--:|--:|--:|:--|
| Channel | 95.8 | 80.7 | 1.187 | +18.7% | 19.9% | within |
| Fib | 98.8 | 97.6 | 1.012 | +1.2% | 3.0% | within |
| FieldView | 2917.6 | 1723.7 | 1.693 | +69.3% | 1.7% | **moved** |
| Iface | 528.0 | 374.7 | 1.409 | +40.9% | 5.6% | **moved** |
| IfaceCall | 807.0 | 358.3 | 2.252 | +125.2% | 11.3% | **moved** |
| IfaceShell | 738.2 | 600.4 | 1.230 | +23.0% | 4.1% | **moved** |
| IfaceShellChurn | 1432.7 | 1076.8 | 1.331 | +33.1% | 2.3% | **moved** |
| Map | 264.6 | 253.6 | 1.044 | +4.4% | 4.3% | **moved** |
| MatMul | 393.7 | 213.2 | 1.847 | +84.7% | 4.9% | **moved** |
| RefLower | 645.7 | 511.3 | 1.263 | +26.3% | 1.7% | **moved** |
| Sieve | 140.8 | 139.3 | 1.010 | +1.0% | 7.0% | within |
| Sort | 305.5 | 255.1 | 1.197 | +19.7% | 2.2% | **moved** |
| Startup | 491.6 | 502.0 | 0.979 | -2.1% | 2.9% | within |
| String | 1465.3 | 339.0 | 4.322 | +332.2% | 16.7% | **moved** |
| StringMatch | 1057.9 | 791.2 | 1.337 | +33.7% | 4.0% | **moved** |
| StringView | 29.4 | 20.9 | 1.405 | +40.5% | 18.6% | **moved** |
| TlsHandshake | 1513.4 | 1071.6 | 1.412 | +41.2% | 2.8% | **moved** |

### 10.x  GoReadyToRunUserCode (R2Ru) against the template's ReadyToRun, all 17 programs (a reader run, 2026-11-05)

Inserted at the end of §10 after §11 landed, at COORD's ruling for the Tuning page, which needs the table under `GoReadyToRunUserCode`. NO new measurement: the
reader `r2ruread.sh` runs over window 1's existing R2Ru samples (§5, `w1-r2ru-rest.tsv` and `w1-r2ru-review.tsv`):
- the window's own `.2` R2R binary against the R2Ru publish of the same tree;
- 5 rounds, or 11 for the review rows.

Window 1 took no per-arm A/A for these arms. So each cell's floor is the LARGER of its two arms' IQRs and window 1's JIT
A/A null (2.8%), and it is stated so in every row. A cell moves only beyond its floor, in either direction. Values are
medians in ms: in-program time, except Startup's process wall.

**Result: R2Ru moves 11 of 17 rows, ALL slower, and none faster.** Startup, Sort, RefLower, FieldView, Fib and
TlsHandshake stay within. Seat #53's IL-only default is the faster publish everywhere it differs (§5 already read this
direction).

| program | R2Ru | R2R | ratio | change | floor | reading |
|:--|--:|--:|--:|--:|--:|:--|
| Channel | 117.1 | 79.9 | 1.466 | +46.6% | 37.0% | **moved** |
| Fib | 108.7 | 110.8 | 0.981 | -1.9% | 38.8% | within |
| FieldView | 1909.0 | 1933.6 | 0.987 | -1.3% | 2.8% | within |
| Iface | 660.4 | 376.1 | 1.756 | +75.6% | 4.2% | **moved** |
| IfaceCall | 1458.1 | 339.6 | 4.294 | +329.4% | 2.8% | **moved** |
| IfaceShell | 728.4 | 574.6 | 1.268 | +26.8% | 3.0% | **moved** |
| IfaceShellChurn | 1188.1 | 1071.7 | 1.109 | +10.9% | 2.8% | **moved** |
| Map | 268.9 | 248.9 | 1.080 | +8.0% | 2.8% | **moved** |
| MatMul | 589.4 | 213.8 | 2.757 | +175.7% | 4.6% | **moved** |
| RefLower | 524.7 | 535.0 | 0.981 | -1.9% | 2.8% | within |
| Sieve | 265.2 | 139.5 | 1.901 | +90.1% | 3.1% | **moved** |
| Sort | 255.0 | 254.4 | 1.003 | +0.3% | 3.1% | within |
| Startup | 374.1 | 374.2 | 1.000 | -0.0% | 2.8% | within |
| String | 1995.5 | 653.5 | 3.054 | +205.4% | 3.1% | **moved** |
| StringMatch | 1215.2 | 804.7 | 1.510 | +51.0% | 6.3% | **moved** |
| StringView | 605.2 | 21.8 | 27.792 | +2679.2% | 12.4% | **moved** |
| TlsHandshake | 1079.6 | 1073.7 | 1.006 | +0.6% | 2.8% | within |

Raw: `DATA-hop126-perf/sweep/r2ru-verdict.tsv` and the reader `r2ruread.sh`; samples are §5's `w1-r2ru-*.tsv`.


---

## 11. Appendix, 2026-11-05: item 7, TlsHandshake under Native AOT at about 2.4x its JIT, root-caused

Appended after §10. §6 queued item 7; COORD seated it as a scout: root cause only, and any fix is a separate ruling. The raw
files are in [`DATA-hop126-perf/item7/`](DATA-hop126-perf/item7/). Three large inputs are not committed and are named by
sha256 in `large-files-not-committed.sha256`: the two ETL traces, the symbol-carrying image with its 1.7 GB PDB, and the
118 MB event dump. They are kept on G under `C:\go2cs\scratch\p1262\trace\`.

### 11.1 Step A: the instruction set does NOT explain it under the JIT (window 08:12-08:18)

The plan's hypothesis H1 was the instruction set. The test ran the same `.2` JIT binary under runtime ISA knobs, against
the admitted D26 AOT image, 11 rounds, with A/A on J. Medians, in ms:

| J | J-AA | J-noBMI2 | J-noAVX2 | J-v2a | D26 (AOT) |
|--:|--:|--:|--:|--:|--:|
| 1069.8 | 1075.5 | 1077.6 | 1141.3 | 1133.5 | 2555.4 |

J-v2a is AVX, AVX2, BMI1, BMI2, LZCNT and MOVBE off, with AES and PCLMULQDQ kept.

- **Scored:** J-v2a/J = 1.057, at or below the pre-registered 1.2, so **H1 FAILED** for the JIT and Step B was dropped. The
  disassembly below brings Step B back, for the AOT side only.
- **Changed before measuring, and stated in the OPEN post:** with AES off, the converted Go TLS stack negotiates
  ChaCha20-Poly1305 (suite 4867) instead of AES-128-GCM (4865). Go's own hardware-AES test reads the .NET runtime's view
  of the CPU. The D26 AOT image negotiates 4865, so the AES-off arms (J-noAES 1070.9, J-v2 1127.6) ran as their own
  verify group.
- **Disclosed:** an idle `VBCSCompiler` from the pre-window build was alive at both ends.
- **Window 1's record already split the gap:** TC0 reads 1523.7 ms, so TC0/J = 1.44 is tiering plus PGO, and AOT/TC0 =
  1.66 is AOT-specific.

### 11.2 The profile step: instrument and controls

- **The image.** The drop's own exe and pdb pair no longer existed, so the i9 republished PerfTlsHandshake `.2` the drop's
  way: converter `7871e564`, masked source identity equal, SDK 10.0.403 / ILC 10.0.14, the PDB kept.
  - Image: sha256 `7f2534b7…`, size-equal to D26.
  - PDB: `6fa856d5…`, 1,704,284,160 bytes.
  - A short window shows it represents the timed D26: 2540.7 against 2538.4 ms, ratio 1.001, floor 1.5%, within.
  - The first attempt at that window is **VOID**: its Fib control (+8.72%) did not hold 0, right after the 1.7 GB
    pull. It is kept as `void1-p7b-*` and not read.
- **The trace.** wpr's CPU sampling needs elevation, which the G session does not have, so it was the owner's hand.
  - The script, `trace-tls.cmd` (sha256 `5aa034c2…`), was reviewed twice by COORD.
  - Before anything runs, it hash-checks every file either run loads: the AOT exe and pdb inline, and the JIT folder's
    435 files through `jit-manifest.txt`, whose own hash is inline.
  - Its checks were proven to refuse a planted mismatch, on a JIT file and on the AOT hash, and both were restored.
- **The first owner run failed, closed (19:49).** wpr returned rc −984068079 = `0xC5585011`, the code G reproduced
  unelevated ("Failed to enable the policy to profile system performance"). No trace was written. The rerun at
  19:52-19:54 from an elevated prompt wrote both traces.
  - **Suggested for the next staged script (declined for this one):** an elevation guard after the hash checks,
    `net session >nul 2>&1 || (echo NOT ELEVATED … & exit /b 4)`.
- **The traced runs match the timed runs:** AOT `elapsed_ns` 2,512.7 ms (timed 2,538) and TC0 1,583.7 ms (window 1
  1,523.7).
- **Reading, headless, local symbols only, no network.**
  - `xperf -symbols` on the 1.7 GB PDB was still building its symbol cache after more than an hour, and was stopped.
  - Instead, `xperf -a dumper` exported the raw samples and stacks in 3 s, and a 40-line dbghelp resolver
    (`symres-Program.cs.txt`) named only the sampled addresses. Its first build loaded symbols deferred and resolved
    nothing; non-deferred, all resolve.
  - It reports any address outside its symbol's extent: 7 of 2,160 frame addresses (runtime stubs and thunks: write barriers, an interface-dispatch stub, an unboxing thunk and a generic-lookup helper) are
    excluded.
  - The TC0 trace's JIT frames are named by xperf from the .NET rundown.
  - **Both are cut to the timed region,** the last `elapsed_ns` of each run: the AOT timed region holds 2,500 samples,
    and the TC0 region weighs 1,558.8 ms.

### 11.3 The result: two codegen shapes in the AOT image cover about 87% of AOT/TC0

Timed region, exclusive ms, AOT against TC0. The gap is 941 ms (`aot-timed-excl.txt`, `tc0-timed-perftls.txt`):

| function | AOT | TC0 | AOT − TC0 |
|:--|--:|--:|--:|
| golib `slice<UIntPtr>.get_Item` | 547 | 99 | **+448** |
| `math/bits.Mul` + `Math.BigMul` software fallback | 204 + 169 | ≈ 0 (inlined) | **+373** |
| bigmod `addMulVVW` | 380 | 352 | +28 |
| golib `ElementWindow.TryGet` | 98 | 99 | −1 |
| `unsafe.Slice` | 85 | 70 | +15 |

**Cause 1, golib's slice indexer:**
- **The shape** (`disasm-aot-slice-UIntPtr-get_Item.txt`): in the AOT image every element read is a CALL. The indexer is
  not inlined, and on each call it does:
  - two bounds compares;
  - a LAZY CLASS-CONSTRUCTOR CHECK on `GoZeroSizeFacts<UIntPtr>` (`cmp qword ptr [statics-8],0; jne <init helper>`);
  - a load of `IsZeroSize`;
  - a native-backing check;
  - then the bounds-checked array access.
- **The cause:** `GoZeroSizeFacts<T>.IsZeroSize = GoZeroSizeFacts.Classify(typeof(T))` is a reflective field walk in a
  static initializer.
  - golib's own comment says the gate "folds at JIT time". It does under the JIT, because the class is initialized
    before the hot method compiles.
  - ILC cannot pre-initialize a reflective static constructor. So under Native AOT the gate stays a runtime check, and
    the indexer stays too big to inline.
- **Its age:** present since `cd74435da6` (2026-08-26), so it is an AOT-against-JIT cost, not a `.1` → `.2` regression.
- **Its reach:** every `slice<T>` element read in every Native AOT program, not this program alone.

**Cause 2, the 64-bit multiply:**
- **The shape** (`disasm-aot-math-bits-Mul.txt`): in the AOT image `math/bits.Mul` is a non-inlined call that
  UNCONDITIONALLY calls `Math.BigMul`'s software fallback: no `mulx` or `mul`, and no runtime CPU check.
- **The JIT side:** under the JIT the multiply is inlined, and §11.1 shows it is free even with BMI2 disabled.
- So ILC compiled the software path at its baseline instruction set. §11.1 ruled out the ISA for the JIT only; for AOT,
  `Math.BigMul`'s compile-time path depends on it. Confirmation is Step B (§11.4).

**The rest, about 120 ms (13%):** `unsafe.Slice`/`SpanLength` and the slice constructor/`Window` run a little higher in AOT,
plus allocation and GC helpers (`RhpNewFast`, `memset`, `mark_through_cards`).

**The named suspects, scored:**
- seat #18's NativeStructMarshal path: **killed**. It is not in the timed profile, and all socket syscalls together are
  3.3%.
- Shared-generic and generic-virtual dispatch in the crypto interfaces: **killed for the timed time.** `crypto.Signer.Sign`
  does go through `GoShellBinding.Invoke` into reflection `DynamicInvoke`, but only once per handshake.
- Static-base helpers: **confirmed**, as cause 1's class-constructor check.

**A named AOT start-up cost, outside the timed region:** 516 ms of the AOT warm-up is spent in `builtin.TryTypeAssert`
→ `TypeExtensions.StructurallyImplements` → `GetGoMethodSetCandidates`, decoding reflection metadata (`NativeFormat`) on
first use. All of those samples fall 0.29-0.89 s after process start, before the timer. COORD names it a candidate for
P1's extension-scan family.

### 11.4 Open from this section

- Step B, approved, diagnostic only: one AOT image with BMI2 in `IlcInstructionSet`, to confirm cause 2. The shipping
  default is untouched; any instruction-set or portability change is the owner's call.
- A design note on making `IsZeroSize` fold under ILC (cause 1), with options, footprint, risk and the expected gain.

### 11.5 Amended 2026-11-05: the multiply scan, the cause-1 design note (option A ruled), and a probe trap

Neither probe below was timed. Each is a small program built as a Native AOT image on G (ILC 10.0.11, SDK 10.0.400; the
i9's is 10.0.14) and read with `dumpbin /disasm`. The sources and the disassembly are in [`item7/`](DATA-hop126-perf/item7/).

**Step B's status.** The i9's first v3 build was refused by ILC in 30 s: G's request spelled the level `x86-x64-v3`, and
this ILC accepts `x86-64-v3`. The rerun with that one token corrected is building, TlsHandshake first. On COORD's ask,
the other 16 programs follow at v3, so the owner sees what the level buys across the suite and not only here.

**The multiply scan: no C# spelling reaches a hardware multiply at the AOT baseline** (`mulprobe-*`). Reflection over
.NET 10.0.11 shows no widening multiply on `X86Base` or `X86Base.X64`, which carry `DivRem` only. The one explicit
64x64->128 intrinsic is `Bmi2.X64.MultiplyNoFlags`, which needs BMI2. Four spellings, each a non-inlined method in one
image at the default instruction set:

| spelling | default instruction set | `x86-64-v3` (positive control) |
|:--|:--|:--|
| `Math.BigMul(x, y, out lo)`, today's `bits_impl.cs` | call to `BigMul`'s software fallback | `mulx` |
| `Math.BigMul(x, y)` returning `UInt128` | the same call | `mulx` |
| `(UInt128)x * y` | the same call | `mulx` |
| the same, split to hi/lo | the same call, plus a call for `UInt128 >>` | `mulx`, still with the `>>` call |

All four print the same product. ILC 10.0.11 refuses a bare `bmi2`, so `x86-64-v3` is the narrowest named level that
includes BMI2. Cause 2 is therefore an instruction-set decision for the owner, and Step B's timing prices it; there is no
corpus spelling to change.

**The cause-1 design note, and COORD's ruling.** `zsprobe-*` is a miniature of `slice<T>`'s indexer, with
`GoZeroSizeFacts<T>`'s reflective initializer copied. It is built with the template's `TrimMode=partial`, and every
variant prints the JIT's answers. Each gate spelling's indexer body, by element type:

| gate | `nuint` | struct, no references | struct holding a reference | empty struct |
|:--|:--|:--|:--|:--|
| today: `static readonly IsZeroSize` | class-constructor check | class-constructor check | class-constructor check | class-constructor check |
| A: a pre-gate, then the cached value | folded | class-constructor check | folded | class-constructor check |
| B: an emitted marker interface, as a type test | folded | folded | folded | folded, to the zero path |

"Folded" means no class-constructor check, no static read and no zero-size branch: 31 instructions against today's 54.
Pre-gate A is `!typeof(T).IsValueType || typeof(T).IsPrimitive || typeof(T).IsEnum ||
RuntimeHelpers.IsReferenceOrContainsReferences<T>()`; when it holds, the answer is false and the cached classification is
never read.

- **A** is golib only, one file: `IsZeroSize` becomes an inlined property, and the reflective field moves to a nested
  holder. Its risk for genuinely zero-size types is none by construction. Every term is a case `Classify` itself answers
  false: its own first line, and a reference field always has size. It covers TlsHandshake's `nuint`, so up to the
  measured +448 ms (about 18% of the 2,534 ms AOT median). It also covers the `bool`, `float64` and `int` slices of
  Sieve, MatMul and Sort, which run 2.2-3.4x their JIT under AOT; their share is unprofiled.
- **B** is generator output or visible emission, plus hand marks. It is a second copy of the rule, computed at compile
  time, and a generic struct whose zero-size-ness depends on a type argument cannot carry a static marker. A missed
  marker silently changes Go behavior: `make([]T, MaxInt)` panics, and `&s[i]` stops being one address.
- **Preinitialization** is not an option: ILC's preinitializer does not run reflection.

**RULED (COORD): A.** R implements it, with a guard that the pre-gate holding implies `Classify` is false and a
disassembly proof. G prices it: AOT images of TlsHandshake, Sieve, MatMul and Sort with that golib, timed against D26. B
stays a later item, only if a profile shows slices of reference-free structs hot.

**A probe trap, kept because it is the hazard itself** (`zsprobe-trap-outputs.txt`). G's first probe build used the AOT
default, full trim. Its copied `Classify` called a bare `GetFields`, which saw NO fields on a 16-byte struct and so
classified it as zero-size. Every element then aliased one slot: the loops summed 56 where the JIT summed 28.
golib guards exactly this: `GoFieldMetadata.InstanceFields` refuses a value type wider than one byte that reports no
fields, and the template's `TrimMode=partial` keeps converted assemblies whole. Built partial, the probe agrees with the
JIT. Any classification that leans on reflection metadata under AOT needs both of those.

### 11.6 Amended 2026-11-06: Step B priced, the x86-64-v3 suite picture, and what a v3 baseline refuses

Raw files are in [`item7/stepb/`](DATA-hop126-perf/item7/stepb/). Every timing line is `win-x64` on G (Ryzen 5 PRO 6650U,
runtime 10.0.11). The images were built by the i9 (SDK 10.0.403, ILC 10.0.14) with the drop's recipe; a v3 image adds
`-p:IlcInstructionSet=x86-64-v3` and nothing else. Each v3 image's ILC response file carries exactly one instruction-set
line, `--instruction-set:x86-64-v3`, and the default-instruction-set (D26) response files carry none. Every image was
sha256-checked against the i9's post and its folder manifest before any timing.

**Step B, TlsHandshake (window 21:54-21:57 on 11-05, valid).** D26 2544.8 ms against v3 2288.6 ms: **0.899**, -256 ms,
floor 1.37%, MOVED. The Fib control drifted -4.49%, and its 95% interval [-22.12%, +1.00%] holds 0. The prediction of about
0.85 (all of the 373 ms) recovered about 69% of that.

The v3 image's disassembly was read by symbol extent through a name->address lookup (`symfind`, dbghelp `SymFromName`). As
a control, on the baseline image it returns exactly the addresses §11.3 recorded.
- `math/bits.Mul` is 12 instructions around `mulx`, and `Math.BigMul`'s software fallback is absent from the image.
- `slice<UIntPtr>.get_Item` is unchanged: the same `GoZeroSizeFacts<UIntPtr>` class-constructor check (cause 1).
- The hot caller `bigmod.addMulVVW` has the same shape at both levels: 4 calls to `get_Item` and one NON-INLINED call to
  `bits.Mul` per iteration, whose `(hi, lo)` comes back through a stack buffer. That fits the ~117 ms v3 did not recover.
  `bits_impl.cs`'s `Mul` carries no inlining attribute on the strength of a JIT-only measurement. Inlining it went to R as
  a separable second commit on the zero-size seat; it is priced below once built.

**The v3 suite, first window (00:25-00:42 on 11-06): INVALID as worded.** The validity rule stated before the run was
"every arm's A/A under 3%", and 3 of 68 arms broke it, all of them D26 arms: Channel 11.77%, Map 8.94%, StringView 7.94%.
Channel and Map are wide in every arm (largest IQRs 21.7% and 20.9%); StringView's D26-AA had a slow tail. The Fib control
held (-2.60%, [-3.10%, +1.63%]); Sieve, reported only, drifted -8.72% with an interval excluding 0. The predictions,
stated in the OPEN post and **scored as worded, 1 of 4 HIT**:

| prediction | result |
|:--|:--|
| TlsHandshake repeats 0.899 within its floor | HIT (0.902) |
| at least 2 other programs move faster | FAILED: none |
| no program moves slower | FAILED: StringMatch and IfaceShellChurn |
| suite geomean 0.95-0.99 | FAILED: about 1.000 |

The window's provisional table, V3/D26, recorded as PROVISIONAL:

| program | ratio | change | floor | reading |
|:--|--:|--:|--:|:--|
| Channel | 1.032 | +3.2% | 21.7% | within (disturbed) |
| Fib | 1.000 | +0.0% | 2.3% | within |
| FieldView | 0.996 | -0.4% | 1.5% | within |
| Iface | 1.001 | +0.1% | 1.9% | within |
| IfaceCall | 0.978 | -2.2% | 7.7% | within |
| IfaceShell | 0.957 | -4.3% | 6.1% | within |
| IfaceShellChurn | 1.035 | +3.5% | 1.7% | moved |
| Map | 1.016 | +1.6% | 20.9% | within (disturbed) |
| MatMul | 1.006 | +0.6% | 1.6% | within |
| RefLower | 0.987 | -1.3% | 1.7% | within |
| Sieve | 1.052 | +5.2% | 18.2% | within |
| Sort | 0.994 | -0.6% | 1.4% | within |
| Startup | 1.008 | +0.8% | 5.4% | within |
| String | 1.029 | +2.9% | 6.1% | within |
| StringMatch | 1.053 | +5.3% | 1.3% | moved |
| StringView | 0.961 | -3.9% | 14.5% | within (disturbed) |
| TlsHandshake | 0.902 | -9.8% | 1.1% | moved |

**The rerun (00:44-00:51 on 11-06), on COORD's ruling made before it ran.** Five programs at 21 rounds per arm, with
everything else unchanged: the three disturbed rows, plus StringMatch and IfaceShellChurn as REPLICATION of the two
unexpected slower rows. Validity was split so that no amendment could reach the decisive rows:
- StringMatch and IfaceShellChurn kept the original A/A < 3%.
- Channel, Map and StringView could use "A/A < 3% or under the larger IQR of the pair". Those rows read noise-bound, with
  no claim unless MOVED.

All five passed. StringMatch (0.09/0.06%), IfaceShellChurn (0.24/0.05%), Map (1.11/0.52%) and StringView (0.04/1.47%)
passed the original rule. Channel (6.68/8.08% against IQRs of 15.53/12.85%) passed only the amended one. The Fib control
held (-2.26%, [-3.91%, +3.46%]). The 12 untouched rows stand from the first window on PER-ROW validity: each passed its own
A/A, and the control held.

**The final v3 picture, 17 programs.** V3/D26, ms medians (in-program time; Startup is process wall). The rows for
Channel, Map, StringView, StringMatch and IfaceShellChurn come from the rerun:

| program | v3 | D26 | ratio | change | floor | reading |
|:--|--:|--:|--:|--:|--:|:--|
| TlsHandshake | 2295.0 | 2543.2 | 0.902 | -9.8% | 1.1% | **moved faster** |
| StringMatch | 1352.6 | 1292.1 | 1.047 | +4.7% | 1.0% | **moved slower** (replicates 1.053) |
| IfaceShellChurn | 1752.1 | 1700.3 | 1.030 | +3.0% | 1.5% | **moved slower** (replicates 1.035) |
| IfaceShell | 1221.4 | 1276.2 | 0.957 | -4.3% | 6.1% | within |
| IfaceCall | 543.5 | 555.7 | 0.978 | -2.2% | 7.7% | within |
| RefLower | 647.7 | 656.2 | 0.987 | -1.3% | 1.7% | within |
| Sort | 548.1 | 551.4 | 0.994 | -0.6% | 1.4% | within |
| FieldView | 2361.6 | 2372.1 | 0.996 | -0.4% | 1.5% | within |
| Fib | 179.9 | 179.9 | 1.000 | +0.0% | 2.3% | within |
| Iface | 577.8 | 577.4 | 1.001 | +0.1% | 1.9% | within |
| Map | 294.5 | 293.7 | 1.003 | +0.3% | 4.8% | within |
| MatMul | 743.7 | 739.1 | 1.006 | +0.6% | 1.6% | within |
| StringView | 19.2 | 19.0 | 1.008 | +0.8% | 10.5% | within |
| Startup | 171.0 | 169.6 | 1.008 | +0.8% | 5.4% | within |
| Channel | 113.3 | 112.0 | 1.011 | +1.1% | 15.5% | within; noise-bound, no claim |
| String | 1443.7 | 1402.3 | 1.029 | +2.9% | 6.1% | within |
| Sieve | 321.2 | 305.4 | 1.052 | +5.2% | 18.2% | within |

Suite geomean of the 17 ratios is **1.000**, and **1.006** without TlsHandshake. On this box, x86-64-v3 buys
TlsHandshake's multiply and nothing else. It costs StringMatch about 5% and IfaceShellChurn about 3%.

**StringMatch's mechanism: `vzeroupper` on every exit of the byte comparer that string equality reaches.** The i9 built a
matched pair, each with its own native PDB:
- default `f99efb3c…` (size-equal to D26) and v3 `f1058d1c…` (the timed v3 image itself);
- the same converted tree (masked identity `bd2da796`), and ILC response files that differ in exactly the one
  instruction-set line.

Hot functions were compared by symbol extent (`sm-hot-sizes.txt`, `sm-cmp.sh`); the program compares strings of 10 bytes
or fewer.
- `classify`, golib's `@string` equality, `HasPrefix` and the `StringLiteralCache` methods are the same length at both
  levels.
- `run` and `kindName` are re-encoded to VEX instruction for instruction: 321 against 320 and 151 against 150
  instructions, the same calls, and no `vzeroupper`.
- `System.SpanHelpers.SequenceEqual(ref byte, ref byte, nuint)` grows from 72 to 98 instructions (240 to 336 bytes).
  - At v3 it adds a 32-byte `ymm` loop, and the compiler places `vzeroupper` before every one of its three returns. That
    includes the scalar return that lengths under 16 bytes take (`dis-sm-v3-SequenceEqual.txt`, `0x14008B6AC`).
  - Every short compare also gains one more length test (`cmp r8,20h`).
  - The default body has neither (`dis-sm-default-SequenceEqual.txt`).

Among the functions compared, the executed-path difference is extra work in every equal-length string compare (the map's hashing and lookup helpers were not in the compared set):
- one compare and a `vzeroupper`, per call;
- in a function that never touches a `ymm` register on that path.

This is consistent in direction with the replicated +4.7%. Its cost per call on this Zen 3+ part is not measured here.
Seen from the corpus, it is the same trade the instruction set makes everywhere: wider vector paths in shared .NET helpers,
paid for by short inputs.

**What a v3 baseline refuses (portability).** From Microsoft's own sources, at the `release/10.0` branch of dotnet/runtime:
- [The Native AOT optimizing guide](https://github.com/dotnet/runtime/blob/release/10.0/src/coreclr/nativeaot/docs/optimizing.md)
  says the default "targets the minimum instruction set supported by the target OS and architecture", and that with
  `IlcInstructionSet` "the native binary will require the instruction sets to be supported by the hardware in order to run".
- ILC's group table (`CorInfoInstructionSet.cs`) defines x86-64-v3 as `x86-64-v2 avx2`. Its `avx2` group is AVX2, BMI1,
  BMI2, F16C, FMA and LZCNT; v2 adds the SSE3-SSE4.2 and POPCNT group. `ilc --help` (10.0.11) lists x86-64-v3 among its
  predefined groups.
- At start-up the Native AOT runtime's `DetectCPUFeatures` (`nativeaot/Runtime/startup.cpp`) compares the CPU against the
  image's required features. When any is missing, it prints "The required instruction sets are not supported by the current
  CPU." and calls `RhFailFast()`. The program does not run at all, and there is no fallback path.
- Microsoft's documents name no processor models. By the vendors' feature tables (not Microsoft's), the CPUs that lack AVX2,
  BMI2 or FMA include:
  - Intel cores before Haswell (2013): Sandy Bridge and Ivy Bridge;
  - Intel's low-power Atom-class cores through Tremont, including the Celeron and Pentium Silver N-series;
  - Pentium Gold and Celeron parts of the Skylake through Comet Lake generations, which ship with AVX disabled;
  - AMD before Excavator (2015), and AMD's Jaguar and Puma low-power cores;
  - virtual machines whose hypervisor masks AVX, and x64 emulation layers that do not expose AVX2.

**Open from this section: the pricing window**, as COORD amended it on 2026-11-06 before any image was built.
- The i9 found that the drop recipe restores golib and `go.math.bits` from nuget.org's 1.26.10.2 packages, so A and AI built
  that way would carry neither change.
- So the arms are source-reference builds (`-go2cspath` at each arm's worktree) with a base arm:
  - B0 at `f0ed6c71f6` (TlsHandshake, Sieve, MatMul, Sort);
  - A at `2393238b8a`, R's zero-size pre-gate (the same four);
  - AI at `32a0372d23`, A plus the `Mul` inline (TlsHandshake).
- The readings are A/B0 and AI/A. D26 is not an arm, and AIv3 is skipped (a TlsHandshake publish takes about 1,535 s).
- The disassembly gates come before any timing.
- The predictions stated 2026-11-05 carry over to the new base: A against base 0.80-0.90 for TlsHandshake; at least 2 of
  Sieve, MatMul and Sort moved faster; AI/A 0.88-0.96.

### 11.7 Amended 2026-11-06: the two portable fixes, priced at the default instruction set

Raw files are in [`item7/pricing/`](DATA-hop126-perf/item7/pricing/). The design was approved before any image was built,
and its arms were amended on 2026-11-06 (§11.6, *Open from this section*): source-reference builds by the i9 at the
default instruction set, each with its native PDB.

| arm | tree | change | programs |
|:--|:--|:--|:--|
| B0 | `f0ed6c71f6` | base | TlsHandshake, Sieve, MatMul, Sort |
| A | `2393238b8a` | R's zero-size pre-gate (golib `GoZeroSize.cs` only) | the same four |
| AI | `32a0372d23` | A plus the `bits.Mul` inline (`math/bits` `bits_impl.cs` only) | TlsHandshake |

The i9 confirmed the following for every image:
- the converted program sources are byte-identical across arms (paths masked);
- each image's golib and `math/bits` tree hashes are named in its manifest;
- no response file names an instruction set;
- every `go.*` project resolves from that arm's own `src/core` (0 packages);
- SDK 10.0.403, ILC 10.0.14.

Every file was sha256-checked against the i9's post and its folder manifest before use. All timing is `win-x64` on G.

**The disassembly gates, read before any timing** (`pgates.sh`, by symbol extent on TlsHandshake): GATES PASS.
- B0's `slice<UIntPtr>.get_Item` carries the `GoZeroSizeFacts` class-constructor check, as the control. A's and AI's do
  not: 31 instructions against 51, the shape the §11.5 probe predicted.
- B0's and A's `bigmod.addMulVVW` call `math/bits.Mul`. AI's does not. It calls `Math.BigMul`'s software fallback directly
  and holds no `mulx`, as a default-instruction-set image should.
- **A gate hazard, recorded for any future ILC symbol gate: identical-body folding.** In A, `slice<UIntPtr>.get_Item`
  became a one-instruction `jmp` to `slice<Double>.get_Item`: with the gate gone, the two 8-byte-element indexers are the
  same machine code, and ILC keeps one. A gate that reads only the named symbol's extent passes VACUOUSLY on such a stub,
  as the first read here did. `pgates.sh` now follows a one-instruction `jmp` to its target and says so, so the gate
  reads the body the caller actually runs.
- Visible but not gated: in A, `get_Item` is smaller but still a CALL from `addMulVVW`, four per iteration. It is not
  inlined.

**The window (04:11-04:18): INVALID as worded.** Exactly one arm broke "every arm's A/A under 3%": Sieve's B0, at 3.86%.
Every other arm's A/A was 0.05-0.55%. The Fib control held (+2.18%, [-2.29%, +3.85%]), and the census found 0 build
processes and 0 pwsh hosts at both ends. Per the §11.6 precedent, the other rows stand on per-row validity. The Sieve row
was re-measured alone (below), and the window's 0.834 for Sieve is not read.

**The Sieve-only rerun (04:19-04:20, valid)**, as COORD approved it before it ran:
- B0 and A at 21 rounds, under the original rule with no amendment.
- A/A: B0 0.59%, A 0.18%. Fib control -0.30%, [-4.33%, +3.98%].
- The Go Sieve control, which is reported only in every window, stepped +22.57% from start to end (interval excluding 0),
  while Fib did not. All 11 end samples (60-68 ms) sit above all 11 start samples (44-56 ms), so it is a step, not a drift.
  It is **not explained**: the census shows nothing.
  - The stated hypothesis, unverified: a memory-subsystem state change. Go's Sieve is memory-bound and Fib is compute-bound.
  - Interleaving the arms in alternating order cancels a slow drift, but not a step inside one arm's half. The per-round
    table of the four arms shows no level change from round 0 to round 20 (`psieve-arms.tsv`), so no step reached the arms.

**The result.** ms medians:

| reading | program | new | base | ratio | change | floor | |
|:--|:--|--:|--:|--:|--:|--:|:--|
| A/B0 | MatMul | 605.0 | 735.0 | 0.823 | -17.7% | 1.5% | **moved faster** |
| A/B0 | Sieve (rerun) | 277.5 | 320.9 | 0.865 | -13.5% | 6.7% | **moved faster** |
| A/B0 | Sort | 481.9 | 556.1 | 0.867 | -13.3% | 1.4% | **moved faster** |
| A/B0 | TlsHandshake | 2347.3 | 2543.3 | 0.923 | -7.7% | 0.7% | **moved faster** |
| AI/A | TlsHandshake | 2175.8 | 2347.3 | 0.927 | -7.3% | 0.9% | **moved faster** |
| AI/B0, both fixes | TlsHandshake | 2175.8 | 2543.3 | 0.856 | -14.4% | 0.9% | **moved faster** |

**The predictions, stated 2026-11-05 and scored as worded: 2 of 3 HIT.**
- A/B0 TlsHandshake 0.80-0.90: **FAILED** (0.923). The prediction priced all of `get_Item`'s 448 ms. A recovers 196 ms,
  because the indexer lost its class-constructor check but is still a call, four per iteration.
- At least 2 of Sieve, MatMul and Sort move faster: **HIT**, all three.
- AI/A TlsHandshake 0.88-0.96: **HIT** (0.927, -172 ms).

**Beside §11.6, for the instruction-set decision.**
- **The two portable fixes:** at the DEFAULT instruction set, they take TlsHandshake to 0.856 (-368 ms). Option A alone
  speeds the slice-heavy programs by 13-18%, and it costs no CPU support.
- **x86-64-v3 alone:** TlsHandshake 0.899 (-256 ms), the slice-heavy programs within noise, StringMatch +4.7% and
  IfaceShellChurn +3.0%, a suite geomean of 1.000, and an image that refuses to start on CPUs without AVX2, BMI1, BMI2,
  F16C, FMA or LZCNT.
- **Not measured:** v3 on top of both fixes (AIv3 was skipped). What v3 could still add on TlsHandshake is at most the
  software fallback's share (169 ms exclusive in §11.3).
- **Open:** the remaining `get_Item` call is the next lever, and R holds that scout (an AOT indexer inline).

### 11.8 Amended 2026-11-06: the ReadyToRun composite lever in single-file publishes, and what the 3 s really is

Raw files are in [`item7/r2r-lever/`](DATA-hop126-perf/item7/r2r-lever/). The question, from the i9's discriminator: does
crossgen2's `--composite`, the SDK default for a self-contained single-file ReadyToRun publish, cost converted programs?
And should single-file converted templates set `PublishReadyToRunComposite=false`?

The i9 built every image at master `52d69e8968` by the drop recipe:
- `-recurse=nuget` against the published 1.26.10.2 `go.*` packages; the masked source identities equal the drop's.
- `dotnet publish -c Release -r win-x64 --self-contained -p:PublishReadyToRun=true -p:PublishSingleFile=true`, at the
  default instruction set. SDK 10.0.403, crossgen2 and runtime pack 10.0.14.
- Each publish folder was pulled whole and every file checked against its own manifest.

The converted csproj (and `csproj-template.xml`) sets `EnableCompressionInSingleFile=true`. It keeps the program's own
assembly out of ReadyToRun, the same in every arm.

| arm | packaging | crossgen2 | exe | publish |
|:--|:--|:--|--:|--:|
| C | composite, compressed (what a plain single-file publish gets today) | one `--composite` invocation, a 1,649,922,048-byte image | 571 MB | ~765 s |
| N | per-assembly, compressed (`PublishReadyToRunComposite=false`, what the shipped single-file profiles set) | one invocation per assembly (82-83) | 33 MB | 30 s |
| CU | composite, UNcompressed (C plus `EnableCompressionInSingleFile=false`), PerfStartup only | as C | 1,696 MB | 699 s |

**Window 1, C against N (05:02-05:07, valid).** A/A 0.08-2.27%; the Fib control held (-0.55%, [-3.74%, +1.70%]).

| program | N | C | N/C | floor | |
|:--|--:|--:|--:|--:|:--|
| Startup, process wall | 473.2 | 3492.3 | 0.136 | 4.5% | **N faster by 3.0 s** |
| StringMatch, in-program | 779.8 | 1074.4 | 0.726 | 10.2% | **N faster** |

StringMatch's process wall, which is not the reading, was 1272.7 against 4643.0 ms.

The predictions, scored as worded:
- Startup N/C below 1: **HIT**, far beyond its stated size.
- StringMatch within its floor: **FAILED**.

**The mechanism, step by step.**
- *Not JIT time.* `DOTNET_JitDisasmSummary`, one run per image outside the timing (`jit-*-summary.txt`): C JITs about 5x
  FEWER methods than N (Startup 510 against 2626; StringMatch 911 against 2684). The composite precompiles far more of the
  framework and its generic instantiations.
- *`bits.Mul` cannot be tested here.* Neither program calls `math/bits.Mul`, `Mul64` or `Math.BigMul`, so neither arm's
  list names them. TlsHandshake would be the program for the i9's discriminator.
- *Compression, by size.* C's 571,067,714-byte exe bundles a 1,649,922,048-byte composite image, so it is compressed, at
  about 2.9:1. CU's uncompressed exe is 1,695,755,227 bytes.
- *Compression is the whole 3 s.* The discriminating window (05:23-05:26, valid: A/A 0.05-0.69%, Fib control +0.88%,
  [-2.11%, +5.61%]), PerfStartup process wall:

| arm | wall (ms) | against C | against N |
|:--|--:|--:|--:|
| C, composite compressed | 3480.3 | | 7.40x |
| N, per-assembly compressed | 470.4 | 0.135 (replicates 0.136) | |
| CU, composite uncompressed | 319.7 | **0.092** (-3.16 s) | **0.680** (-150.7 ms) |

Single-file compression of the one 1.65 GB composite image is the entire ~3 s: every start decompresses it. With
compression off, the composite starts 32% faster than the per-assembly profile. "Composite wins start-up" holds once it is
not decompressed. The prediction stated before this window scored 1 of 2:
- CU/C below 0.5: **HIT**.
- CU/N in 0.9-1.3: **FAILED**, because CU landed below the range.

The fit with P1's BoGo host (composite, uncompressed, about 1.2 s) is consistent.

**StringMatch's loop under composite, a hypothesis, unverified.** With fewer methods JIT'd, more of C's hot framework code
stays as precompiled ReadyToRun code instead of being re-JIT'd at Tier1 with dynamic PGO (N's StringMatch lists 61
"Tier1 with Dynamic PGO" methods). CU's in-program time was not measured, so whether the loop penalty survives without
compression is open.

**The 2x2 as far as measured, and the trade for the template ruling** (Startup wall):

| | compressed | uncompressed |
|:--|:--|:--|
| composite | 3480 ms, 571 MB | 320 ms, 1,696 MB |
| per-assembly | 470 ms, 33 MB | NOT MEASURED (ruled below) |

- The one combination that is never right is composite WITH compression. Today it is what a plain
  `dotnet publish -p:PublishSingleFile=true` produces under `csproj-template.xml`, which sets compression and leaves the
  SDK's composite default.
- The shipped single-file profiles already avoid it with `PublishReadyToRunComposite=false` (seat #14, pinned by
  `TestSingleFileProfilesCompileReadyToRunPerAssembly`).
- CU buys about 150 ms of start-up at a 1.7 GB executable and a ~700 s publish. Its in-program cost is unmeasured.
- **RULED (COORD, 2026-11-06): no further arms.** Per-assembly uncompressed and CU's in-program time would not change the
  template decision. CU's 1.7 GB executable and ~700 s publish rule it out as a default whatever its loop does, and N is
  already the shipped profiles' shape.
- **The fix is seated with C2:** `csproj-template.xml` sets `PublishReadyToRunComposite=false` for an Exe unless the
  project sets it, so a plain single-file publish gets N, not C.

### 11.9 Amended 2026-11-06: R's AOT indexer inline, priced, and a Debug standard library behind source-reference builds

Raw files are in [`item7/indexer/`](DATA-hop126-perf/item7/indexer/).

**The change and the arms.** R's change is `AggressiveInlining` on `slice<T>`'s `int`, `nint` and `ulong` getters. ILC's inline
budget counted the zero-size gate's IL before folding it, so after §11.7's option A the getter was still a call.
- P is `53b3eb88d7` (base, which carries batch 209: option A plus the `Mul` inline), and D is R's `da3f68520d`.
- Among the paths that reach a binary (`src/core`, `src/go2cs`), P and D differ only in `src/core/golib/slice.cs`; R's other
  two files are docs.
- Every arm is source-built, so all arms share one build shape:
  - AOT: TlsHandshake, Sieve and MatMul, built by the i9 by source reference with the csproj named, and the native PDB
    kept.
  - JIT: the same three programs, built by G with `dotnet build`.
  - R2R: Startup, built by G with `dotnet publish`.
- Every file was sha256-checked against its builder's post and manifest before use. All timing is `win-x64` on G.

**The gates, read before any timing** (`igates.sh`, by symbol extent): GATES PASS.
- P's `main.sieve`, `main.matmul` and `bigmod.addMulVVW` make indexer calls (the control). D's make none; their remaining
  calls are cold: the throw helpers, the bounds panic and `NativeElementRef`.
- The gate counts a call as an indexer call by name, or as a one-instruction `jmp` stub to one (§11.7's folding hazard, read
  from the caller's side). Folding appeared again: P's `matmul` reaches an indexer under a folded name,
  `slice<StackRecord>.get_Item`, and the name check counted it.
- Proven both ways first, on the baseline TlsHandshake image in both slots: P passed, and D failed as it must.

**Window 1 (18:44-19:13): VALID as worded.**
- Every arm's A/A is under 3% (worst 2.19%), and the Fib control held start to end (-2.34%, [-6.16%, +1.30%]).
- Disclosed: Fib from start to middle -5.13% ([-6.57%, -0.49%]), and the Go Sieve control, reported only, stepped +90.6%.

| column | program | D | P | D/P | floor | |
|:--|:--|--:|--:|--:|--:|:--|
| AOT | MatMul | 215.7 | 601.1 | 0.359 | 1.5% | **moved faster** |
| AOT | Sieve | 152.3 | 296.2 | 0.514 | 12.2% | **moved faster** |
| AOT | TlsHandshake | 1640.4 | 2185.1 | 0.751 | 1.0% | **moved faster** |
| JIT | MatMul | 216.1 | 217.7 | 0.993 | 3.7% | within; superseded by window 2 |
| JIT | Sieve | 156.9 | 159.8 | 0.982 | 11.9% | within; superseded by window 2 |
| JIT | TlsHandshake | *(Debug stdlib)* | *(Debug stdlib)* | 1.005 | 2.3% | **SUPERSEDED**: the build was not a JIT build (below) |
| R2R fd1 | Startup (wall) | 382.8 | 382.3 | 1.001 | 2.3% | within; superseded by window 2 |

The window-1 JIT TlsHandshake medians, about 23 seconds, are deliberately not printed. They measure a Debug standard library
and are not a JIT number.

**The predictions, stated in the OPEN post and scored as worded: 4 of 5 HIT.**
- AOT TlsHandshake in 0.88-0.95: **FAILED**, landing below the band at 0.751 (-545 ms). R's 0.86-0.93 misses too.
- AOT Sieve 0.45-0.65 and AOT MatMul 0.30-0.50: **HIT**, within 0.01 of R's R-LAPTOP reads (0.507 and 0.361).
- JIT within its floor, and R2R Startup within its floor: **HIT** as worded. The JIT TlsHandshake row is superseded below.

**The anomaly and its root cause: a Debug standard library under source-reference builds.**
- *The symptom.* The window-1 JIT TlsHandshake read about 21x the release record's ~1,070 ms, the same on both arms.
- *The reading.* G's metadata reader (`asmflags`) reads each assembly's `DebuggableAttribute`. In that closure, 160 of 163
  assemblies were Debug-compiled (`0x107`, optimizations disabled). Only the program, golib and go2cs-gen were optimized.
  The Sieve and MatMul closures had 64 of 67 Debug.
- *The cause.* The converter writes a per-project `.slnx` beside each converted csproj, listing only the program, golib and
  go2cs-gen. A bare `dotnet build -c Release` in that folder builds the solution. MSBuild drops the parent Configuration on
  references to projects outside the solution, so every other converted standard-library package builds in its default,
  Debug. The behavior is verified; that MSBuild's `ShouldUnsetParentConfigurationAndPlatform` is the mechanism is G's
  reading.
- *The experiment.* One converted Sieve, three builds (`diag-debug-stdlib.txt`):
  - bare `dotnet build -c Release`: 64 of 67 Debug;
  - `dotnet build PerfSieve.slnx`: 64 of 67 Debug;
  - `dotnet build PerfSieve.csproj`: 0 of 67 Debug.
- *The fix-check.* P's TlsHandshake built via the csproj has 0 of 163 Debug and runs 1,071, 1,071 and 1,098 ms in-program
  (three diagnostic runs, not a window), which matches the release record. The Debug standard library is the whole 21x.
- *A CONSUMER finding, routed as H's seat.* A user who converts with plain `-recurse` (local core references) and runs the
  natural `dotnet build -c Release` in the project folder ships a Debug standard library. `-recurse=nuget` is unaffected,
  which is why release measurement never saw it.
- *The same root cause, corrected.* The R2R template publish's IL1034 ("Root assembly 'golib' does not have entry point"),
  first attributed to `golib.csproj`'s unconditional `PublishTrimmed`, comes from the same solution. A bare
  `dotnet publish` publishes every project in the `.slnx`, golib included, as a project of its own. Named explicitly, golib
  is only a reference, and the template publish succeeds. The guard on `golib.csproj` stays correct and harmless, but only
  fixing the generated solution addresses both symptoms. H's proof covers both, each red first.
- *The AOT arms are clean.* The i9 published every AOT image with the csproj named (`dotnet publish "<Prog>.csproj"`). On
  the files each ILC response file names, 322 of 322 indexer assemblies read optimized, as did 875 of 875 for §11.7's
  B0/A/AI. The i9's reader was controlled both ways. So window 1's AOT readings and §11.7's stand as recorded.

**Window 2 (20:16-20:21), the JIT and R2R columns rebuilt via the csproj: VALID.**
- An asmflags gate first: 0 Debug assemblies in all 10 closures (`asmgate.txt`).
- The R2R arms in two named shapes:
  - fd1 (framework-dependent, untrimmed), for continuity with window 1;
  - the TEMPLATE shape, self-contained and trimmed, which is what a user ships and which builds once the csproj is named.
- Every arm's A/A is under 3% (worst 2.35%). The Fib control held (+0.77%, [-3.37%, +4.85%]). The Go Sieve control, reported
  only, read -4.82%.

| column | program | D | P | D/P | floor | |
|:--|:--|--:|--:|--:|--:|:--|
| JIT | TlsHandshake | 1107.9 | 1099.4 | 1.008 | 2.3% | within |
| JIT | Sieve | 159.9 | 158.6 | 1.008 | 18.2% | within |
| JIT | MatMul | 220.9 | 221.4 | 0.998 | 12.5% | within |
| R2R fd1 | Startup (wall) | 386.7 | 386.3 | 1.001 | 2.9% | within |
| R2R template | Startup (wall) | 388.9 | 393.3 | 0.989 | 3.7% | within |

The predictions, stated in its OPEN post and scored as worded: **3 of 3 HIT**.
- JIT TlsHandshake back within 5% of ~1,070 ms (1,099.4, +2.7%).
- JIT D/P within its floor for all three, now with optimized callers.
- R2R Startup within its floor in both shapes.

**The result.** R's indexer inline moves AOT only:
- MatMul 0.359, Sieve 0.514 and TlsHandshake 0.751. With it, AOT runs Sieve and MatMul at their JIT's speed.
- JIT and R2R start-up stay flat.
- TlsHandshake under AOT now reads 1,640 ms, about 1.5x its JIT (1,099 ms in window 2), against §11's 2.4x.
