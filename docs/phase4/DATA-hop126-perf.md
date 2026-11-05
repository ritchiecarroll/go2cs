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
