# CgoNugetE2E — the sample of the cgo-nuget-e2e stage

The OS matrix workflow's opt-in `cgo-nuget-e2e` stage (cgo against go.* INC 4, `docs/CIMatrix.md`) packs
`github.com/mattn/go-pointer` v0.0.1, a third-party cgo module, with `nugetgo-pack.ps1 -Cgo -LocalClosure` against
the run's own go.* pack. It then runs this sample on every shipped RID, with `go run` and converted against that
package (`.github/cgo-nuget-pack.ps1`, `.github/cgo-nuget-consume.ps1`).

The layout is `src/tests/ModuleBehavior/`'s: `main.go`, `go.mod`, `go.sum` and the module's `mappings.txt` row,
which `src/tools/nugetgo/Invoke-NugetgoBehaviorRow.ps1` reads through `-SampleDir`. It lives here rather than
there because go-pointer is not a nugetgo package. Its pack is a rehearsal that is never pushed (`local-closure:`),
so it has no row in `nugetgo-modules.psd1`.

The program saves a Go value behind a C pointer, restores it, releases it, and prints only booleans and values, never
a pointer, whose value differs from run to run.
