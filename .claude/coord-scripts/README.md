# Coordinator instruments (scrubbed copy)

This directory is the coordinator's instrument set for the go2cs fleet: the mailbox post tool, the resume
verifier, the train assembly / rehearsal / landing scripts (train46, train47, train48 with its resolution
slots), the battery and drop scripts, and the save-state scripts under `save-state/`.

It is a **scrubbed copy** of the coordinator machine's working directory, pushed by owner ruling
(2026-09-13 22:15) so the instruments survive the machine. What the scrub did, mechanically:

1. **Username paths are environment-derived.** Every `<drive>:/Users/<account>` spelling (forward,
   backward, doubled backslashes, `/c/`, `/mnt/c/`) became `$HOME` in `.sh`, `$env:USERPROFILE` in `.ps1`
   and `~` in `.py`/`.md`/`.txt`/`.json`. Single-quoted strings do not expand these; a `.py` path spelled
   `~/...` needs `os.path.expanduser` at the call site. This is a record copy first, a runnable one second.
2. **Identifier literals are not on this surface.** The mailbox post tool derives its account and machine
   classes from the environment. The train assembly scripts' census patterns (`CENSUS_DETECT`,
   `CENSUS_HARD`, `CENSUS_KIND_NAME`) take the account name from `basename "$HOME"`. Every local secret or
   scrub literal lives in a local `sec/` directory beside these scripts, which is never copied or pushed.
3. **Records are excluded.** Mailbox drafts (`coord-entry-*`, `coord-merge-*`, `coord-queue-*`), run records
   (`coord-pkg-run-record-*`), `.log`/`.out`/`.stdout`/`.bak*`, `scratch/`, `archive/`, `rederive`/`replay`
   directories and the `coord-t47-*`/`coord-t48-*` leg records stay on the coordinator machine. The
   records of record are `docs/phase4/` on `master` and `claude/coord-handover`.

Selection and transforms are performed by the build script kept with the coordinator's session
scratchpad (`build-instr.py`); the copy is censused for the account name and the fleet's identifier classes
before commit (count 0), and every `.sh`/`.ps1`/`.py` passes its parser.

The train-48 union slots carry only their `files/` side (the side the assembly script re-materializes); the
`base`/`ours`/`theirs`/`conflicted` derivation copies are re-derivable from the seat SHAs and stay local.
