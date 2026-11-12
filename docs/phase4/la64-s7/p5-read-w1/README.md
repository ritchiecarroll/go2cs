# S7 wave 1, P5's independent read (2026-11-12)

Lane P5's co-read of wave 1 at master `06610aaa65`, made from the run artifacts with `p5read.py` (not P2's collector),
one TSV per dispatch. A reading record, not evidence for `annotate_linux.py`: its columns are P5's own.

- `verdicts` / `cs_verdicts`: the comparison record's Go and C# verdict counts; `matched`: names whose two verdicts are equal;
  `disclosed`: the length of the record's disclosure list (host-fatal entries included); `new`: errors of the Go/C# mismatch
  shape; `word` and `note`: the sweep log's per-row line; `absent`: Go verdict names the OTHER leg has and this one lacks;
  `diff_names`: names whose two verdicts differ (first 12); `wall_s`: the sweep's row wall.
- Runs, each head read equal to the tip: A-13 43151871852, A-7 43151877722, A-8 43151882036, A-9 43151886546,
  A-6 43151891055, A-12 43151896336, A-2 43151902079.
- One-row re-reads at master + the timeout option + the zstd linux pin (`a31cb64783`), each head read equal to that tip:
  `W1-A7R.tsv` (internal/poll, timeout 30m, run 43161387949) and `W1-Z01.tsv` to `W1-Z10.tsv` (internal/zstd, the ten
  reads: 43161391741, 43161395278, 43161398705, 43161402038, 43161405088, 43161416746, 43161420090, 43161423429,
  43161426535, 43161429556).
