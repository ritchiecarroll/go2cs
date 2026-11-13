# ts-proof-kit

The kit that measures and stages typescript-go-cs. This branch is a scratch branch and never merges to master.

It holds only what go2cs wrote: the comparison scripts, `layout.py`, the README and PROOF templates, `run-gate.sh`,
the sample `.ts` inputs and projects, and the Go sources of the four demos and the corpus extractor. It holds no
TypeScript source and no converted C#: `run-gate.sh` takes TypeScript from a checkout at ba29a562991c and converts
it on the machine that runs the gate.

    GO2CS=<go2cs clone> TS=<TypeScript checkout> W=<work root> DENY=<local name list> \
        bash run-gate.sh <go2cs ref> nuget <GoStdLibVersion>

`layout.py fill` never writes the repository's README.md. It writes the fill values, and with `--owner-readme` a
patch on the existing README, into `<staging dir>.readme/`.
