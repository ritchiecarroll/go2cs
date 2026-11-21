# Global ᴍ cost test: performance A/B kit (scratch, never for master)

This kit is for G's box. It times master against the glyph scratch branch on one machine, interleaved, as asked in COORD 20261120T202904Z item 3.

## Before you run it
1. Make three checkouts:
   - `master`: master at the base SHA. The cost test's base is f604d2ae89.
   - `master2`: a second checkout of the same SHA, for the A/A pass.
   - `glyph`: claude/c1-scratch-glyph-global.
2. Unset `GO2CS_GLYPH_METHODS`. The kit measures the converter's default, which applies the transform to libraries only.
3. Hold the SDK to the 10.0 band for the shell. `global.json` rolls forward to the newest major installed, and the kit refuses anything that is not 10.x.

## Run
A/A first, then A/B. Use the same box, the same power plan, and no other load:

    pwsh ./Invoke-GlyphAB.ps1 -Base C:\g2cs\master -Glyph C:\g2cs\master2 -Out C:\g2cs\ab-AA
    pwsh ./Invoke-GlyphAB.ps1 -Base C:\g2cs\master -Glyph C:\g2cs\glyph   -Out C:\g2cs\ab-AB

## Legs
| Leg | What | Arms timed by |
|---|---|---|
| Headline | Each checkout's own `run-performance.ps1`: JIT and Native AOT, Release. Round 1 builds; rounds 2 to N re-measure. Arm order alternates by round. | the runner's own medians, in `headline-r<round>-<arm>.md` |
| Debug | Sort, StringMatch and TlsHandshake built `-c Debug`, with the go.* references Debug too. This is the uninlined-forwarder worst case. | in-program `elapsed_ns` and wall time, 11 runs per arm, alternating |
| R2R | The converted project's own ReadyToRun publish. The stdlib is not composite and sits in its own version bubbles, so cross-assembly forwarders stay calls until tier 1. | wall time (start-up) and `elapsed_ns`, 11 runs per arm, alternating |

## Output
- `run.txt` records the SHAs, the host, the SDK and the legs.
- `transpile-compare.txt` should read `same` for every benchmark file. The programs are package main, which the default leaves unchanged.
- Each leg writes `<leg>.csv` (every run) and `<leg>-summary.csv/.txt` (medians and Δ%).
- Read every A/B Δ against the A/A Δ of the same leg and benchmark.

## Refusals
Every refusal names what it found. The kit refuses when:
- the two arms are the same directory;
- the `-Out` directory exists;
- `GO2CS_GLYPH_METHODS` is set;
- the SDK is not 10.x;
- a tracked file outside `src/tests/Performance` is modified;
- a program's output differs between runs or between arms.

The kit was written on a Linux box without .NET or PowerShell. It has NOT been run. The first run on the box is its rehearsal.
