# Reading the Q44 census: the fold, and why the obvious one is wrong

`Read-Q44Census.ps1` folds a row's census files into a row total. It exists because the output
did not say how to fold it, and a reader who does not know gets a **plausible wrong number**
rather than an error.

## The defect this closes, and whose it was

The partial flush (added 2026-09-08) makes one process write **several** blocks into its file:
one at the first conversion, one every 250,000 after, and a final block at exit. Those blocks are
**cumulative snapshots of one running total, not increments.**

i9 read a row **1.96x high** by summing every block (mailbox `c62ca28686`) — by a method that had
been *correct* until the flush existed, since before it no file ever carried two blocks. Nothing in
the output said the shape had changed underneath it. **That is an instrument defect, not a reader
error**, and both halves are fixed here:

- every emitted block now carries a `Q44CENSUS-FOLD` line stating the rule, guarded by an arm in
  `Q44RegistryCensusControlTests` so it cannot be dropped silently;
- this script does the fold, so nobody has to re-derive it.

## The three folds

| fold | verdict |
|:--|:--|
| sum every block | **wrong** — double-counts; the 1.96x reading |
| take the FINAL block per file | **wrong** — silently drops any process killed before it finished, which is *the case the flush exists for*. Two of seven files on the measured row carried a partial and no final. |
| **LAST block per file, then sum across FILES** | correct for both, and what this script does |

## Refusals

A file with **no parseable block** is refused (exit 1) rather than counted as zero, and an empty
match set is refused rather than reported as a zero row. An unrun census is not a measured
near-zero — it is an unrun census wearing a result's clothes. That is the same falsifier the
`reflect` row is recorded under (`NO USABLE CENSUS`, not `conversions=1`).

A `SKEW` line is printed, not hidden, when the arms do not sum to the conversions: that is expected
whenever a file's last block is a PARTIAL (threads counted a conversion and had not yet counted an
arm when the snapshot was taken), and it is the reader's cue that the row is live rather than final.

## Usage

```
pwsh -NoProfile -File Read-Q44Census.ps1 -Path <dir> [-Pattern '*q44*census*.txt'] [-Detail]
```

## Controls (run before the script was believed)

- **the fold**: two synthetic files — one carrying two cumulative blocks (100 then 250), one
  carrying a partial only (7). Correct total **257**; the naive sum would read **357**. Measured 257.
- **the refusal**: the same set plus a file with no census block — exit **1**, naming that file.

Written for both PowerShell editions: core cmdlets only, no `Add-Type`, no
`System.Web.Extensions` (the Desktop-only dependency that silently disabled three sweep arms on
every Linux host). Exercised here under pwsh 7; pure ASCII, so the 5.1 BOM trap does not apply.
