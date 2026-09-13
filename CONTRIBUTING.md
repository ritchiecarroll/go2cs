# Contributing to go2cs

Thank you for considering a contribution. This file states the two things every
contribution to this repository carries, and why. The licensing background is in
[LICENSING.md](LICENSING.md).

## 1. Developer Certificate of Origin

Every commit in a pull request carries a `Signed-off-by:` trailer with your real
name and an address you can be reached at:

```text
Signed-off-by: Your Name <you@example.com>
```

`git commit -s` adds it. By signing off you certify the
[Developer Certificate of Origin 1.1](https://developercertificate.org/): that you
wrote the change or have the right to submit it under the file's license, and that
you understand the contribution is public and will be redistributed. The `DCO`
workflow refuses a pull request whose commits lack the trailer.

Which license a file is under follows its header and the component matrix in
LICENSING.md: MIT for the runtime, shared symbols, generators, tests, tour and
documentation; BSD-3-Clause for everything under `src/core` except `golib/` and
`go2cs/`; AGPL-3.0-only with the output exception for the converter under
`src/go2cs`. A new file takes the header its component uses; upstream-derived
files keep their upstream header.

## 2. Relicensing grant for converter contributions

The converter (`src/go2cs`) is offered to the public under AGPL-3.0-only and to
companies that cannot accept the AGPL under a separate commercial license from the
copyright holder listed in [AUTHORS](AUTHORS). That second channel is what funds a
one-maintainer project, and it only works if the copyright holder can relicense
the whole converter.

So a contribution to any file under `src/go2cs` is accepted only with this grant:

> I grant the copyright holder listed in AUTHORS, and that holder's successors and
> assigns, a perpetual, irrevocable, worldwide, non-exclusive, royalty-free and
> transferable right to sublicense my contribution to the go2cs converter, in whole
> or in part, under any license terms the holder chooses, including proprietary
> commercial terms. I keep the copyright in my contribution and every right to use
> it elsewhere. The public AGPL grant on the converter is not affected.

You make the grant once, explicitly. On your first pull request that touches
`src/go2cs` the `CLA Assistant` workflow asks for it; you accept by posting this
sentence, exactly, as a comment on the pull request:

```text
I agree to the relicensing grant in CONTRIBUTING.md, section 2.
```

The workflow records the acceptance in `.github/cla-signatures.json` on `master`
and reports a status check, so a converter pull request from a contributor who has
not accepted cannot merge. The acceptance covers every later contribution you make
to the converter, the workflow recognizes you without asking again, and the
sign-off on each converter commit reaffirms it. Commenting `recheck` re-runs the
check.

If you cannot make that grant, say so in the pull request: the change can still
land in an MIT or BSD component when it belongs there, or stay out of the
converter. Contributions to the MIT and BSD components need only the sign-off.

Contributions made with AI assistance are welcome and are treated as yours: you
direct the tool, you review the result, you sign off. Name the tool in the commit
body if you like; do not list it as an author.

## 3. What a change owes before it is proposed

The repository's engineering discipline is in [CLAUDE.md](CLAUDE.md) and
[docs/Architecture.md](docs/Architecture.md): a converter change comes with a
behavioral test that fails without it, a clean `check-no-regression.ps1` run or an
explained set of golden updates, and a passing converter test suite
(`go test ./...` from `src/go2cs`). A change to `src/gen` or `src/core/golib` also
owes a full behavioral compile. Corpus regenerations are separate from converter
fixes so a fix is reviewable on its own.
