# Darwin leg at f75e0c53ec: dry run, stopped by ruling

The 1.26.10.2 darwin re-bank was dispatched at the TIP `f75e0c53ec` on 2026-10-27 and **stopped by the owner's
ruling** ("go for best known .2 possible"; COORD 2026-10-27): .2 widens first, with seats #18 and #19 and possibly
more, and the leg is re-dispatched at a new TIP under the same plan section. **Nothing in this directory is evidence of
record for 1.26.10.2.** It is a dry-run record of what the stopped TIP read.

| Dispatch | Run | Outcome |
|:--|:--|:--|
| C1-15 `net` | 40596411734 | completed on both RIDs; `C1-15.tsv` |
| C1-13 `runtime` | 40596382419 | osx-arm64 completed (failure); osx-x64 cancelled |
| C1-3, C1-11, C1-1, C1-2 | 40596444823, 40596478827, 40596512050, 40602978789 | cancelled |
| the other nine | — | never dispatched |
