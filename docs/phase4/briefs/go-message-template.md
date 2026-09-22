# GO -- the campaign launch message (COORD -> i9, G, R by SendMessage; the same text as a FLEET inbox file for the record)

Fill CTIP (the campaign tip = the campaign-tip merge STAMP), DIGEST (the plan's `#digest` at CTIP), WRAPPER (the wrapper blob at CTIP; 158ce37f6c at master 3b48e0c8e0), DRIVER (05ec63184b unless moved), PLAN (24ffcff3b9 unless moved). Send ONLY after the ledger's STAMP line for CTIP is pushed.

```
GO (ledger STAMP <time>): the H10 CAMPAIGN TIP is claude/version-go1.24.13 = CTIP. Run your W=4 slices now.
- TOOLS BY BLOB from CTIP (not master, not your DryRun's copies): driver DRIVER, wrapper WRAPPER (NEW since your DryRun -- the one-verdict cross-check fix), plan PLAN with #digest DIGEST (the driver refuses a mismatch). State each `git rev-parse CTIP:<path>` and the sha256 as read.
- TREE: a FRESH linked worktree ON A BRANCH at CTIP (`git worktree add -b <lane>/h10-w4-<mmddHHMM> <path> CTIP`), the converter REBUILT in it under the pin (`go build -o go2cs.exe .` in src/go2cs), `git status --ignored=matching` = 0 before row 1 (the converter build adds go2cs.exe only), >= 25 GB free, the mkdir lock + PID taken.
- PINS per shell, by output: go1.24.13 (pinned bin FIRST on PATH; go env GOROOT = the pin), dotnet 10 (DOTNET_ROOT + its root first on PATH -- the WRAPPER does not export the .NET half; YOUR LAUNCHER does, and your announce states both), GOTOOLCHAIN=local, CGO_ENABLED=0; pwsh 7 (Core).
- RUN: `-Mode rebank -FleetSize 4 -Worker '<exactly as the plan spells your box>' -OnlySlice 1` then `-OnlySlice 2`, `-ExpectTip CTIP`, `-Scratch` OUTSIDE the tree, `-Ledger`/`-TimingOut` in scratch. The wrapper prints `deadline floors : N derived from the sweep` per row -- quote it once; a long row is not a TIMEOUT unless the results tail says the deadline fired (floor 14).
- BANK: one lane ref per shard, `claude/<lane>-h10-rebank-s1` then `-s2`, cut off CTIP, carrying that shard's artifacts ONLY -- the rows' own directories (MINUS sub-package directories: converting a parent mirrors GOROOT testdata into siblings; that is residue, not a banking path) plus their proof pages `docs/validation/current/<row-dotted>.md`; NEVER `docs/validation/index.md`, NEVER the roster header, NEVER a sibling package's testdata. Roster ROW edits for your rows (Tests / Disclosed columns to the measured values) DO ride the ref. Push-then-announce with the TSV verbatim (all columns) and every row's word; a NOVERDICT / CONVERT / BUILD / TIMEOUT row reported by cause in the same message (results tail first). You cut nothing for a red row.
- After s2: `git ls-files | wc -l` unchanged across any cleanup; `grep '^ D'` empty. Say OFFLINE-IDLE in the announce of s2 and STANDBY.
NEXT: s1 now.
```

Per-lane one-liners appended to the shared text:
- i9: `-Worker 'i9-13900K (sweeper)'`; slice 1 = 28 rows / slice 2 = 76; the RESERVED rows are yours by the plan -- never skipped; runtime is in your slice 1 (its host crash is KNOWN -- the row's word by cause).
- G: `-Worker '6650U G (G-LAPTOP)'`, the LINUX arm; slice 1 = 7 / slice 2 = 26; set the four overrides in the launcher (measured load-bearing).
- R: `-Worker '6850U R (R-LAPTOP)'`; slice 1 = 9 / slice 2 = 33; pwsh 7 must be present (owner-hand) before s1.
- i7 (COORD's own sub-agent): `-Worker 'i7-5820K (coordinator)'`; slice 1 = 7 (the rehearsal's rows, re-run at CTIP) / slice 2 = 26.
