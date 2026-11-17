## What changed, and why

<!-- One or two sentences on the defect or need, then what this change does about it. Link the issue if there is one. -->

## The test that fails without it

<!-- Name the behavioral test (src/tests/Behavioral), converter test or GolibTests/GenTests test that is red before this
     change and green after it. -->

## Corpus footprint

<!-- If the converter changed: which converted standard-library files move, and why each one does. A corpus regeneration
     goes in its own commit, separate from the fix. Write "none" if the converter did not change. -->

## Public surface

<!-- If this alters what a C# consumer can bind to (emitted code, go2cs-gen output, golib's public API, package or
     assembly identity, diagnostic codes, MSBuild properties): which tier of docs/PublicSurface.md it touches. Write
     "none" otherwise. -->

## Checklist

- [ ] `go test ./...` passes from `src/go2cs`
- [ ] `src/tests/Behavioral/check-no-regression.ps1` is clean, or the golden updates are explained above
- [ ] a change to `src/gen` or `src/core/golib` also passes a full behavioral compile
- [ ] every commit carries a `Signed-off-by:` trailer (`git commit -s`)

See [CONTRIBUTING.md](https://github.com/ritchiecarroll/go2cs/blob/master/CONTRIBUTING.md) for what a contribution
carries and why.
