# Data for DATA-startup-method-sets-a1.md (B3 arm A1, receiver keys)

Measured 2026-11-18 by P5 on one cloud linux/amd64 VM (4 vCPU, .NET SDK 10.0.400, go1.26.11), Release builds under
the JIT, `GoTargetOS=linux`, at master + batch 311 (`f36b6b0d48`) unless a file says otherwise. Programs converted
with the converter built at that commit. Nothing here ships.

| file | what it is |
|:--|:--|
| `step1/gen.py` | writes the program `BigAst` (run as `gen.py 219 260 main.go`): 219 structs embedding a base with 260 pointer-receiver methods, and one `String` method fmt prints. `wide` is `../startup-method-sets/programs/wide.go` |
| `step1/measurement-golib.patch` | step 1's measurement golib: the A1 prototype behind `GO2CS_B3_KEYS`, the box key behind `GO2CS_B3_BOXKEYS`, the guard's red arm behind `GO2CS_B3_KEYS_BREAK`, and the probe hooks. Needs `cut/B3Probe.cs.txt` as `src/core/golib/B3Probe.cs`. **Never merged** |
| `step1/{wide,big}-*.txt` | step 1's probe output, one run per arm: `off` (master + 311), `keys` (prototype), `boxkeys` (prototype + box key) |
| `step1/wall.py`, `step1/wall-*.json` | step 1's interleaved wall arms, one binary with environment switches |
| `cut/probe.py`, `cut/B3Probe.cs.txt` | the minimal probe applied to a golib tree (`probe.py TREE B3Probe.cs`): host classes read, methods read, `CollectGoMethodSetCandidates` time. Applied to master + 311 for `base` and to master + 311 + the cut's golib diff for `cut`. **Never merged** |
| `cut/probe-{base,cut}-{BigAst,wide}.txt` | one probe run each, `GO2CS_B3_PROBE=2` (with the keys) |
| `cut/wall2.py`, `cut/wall-*.json` | interleaved wall arms over two binaries (`base`, `base-AA` = the same binary, `cut`), probe off, rotating order, paired against `base`; `wall-wide-2.json` is a second sample |
