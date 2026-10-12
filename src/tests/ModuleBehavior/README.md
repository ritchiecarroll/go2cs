# ModuleBehavior — sample programs for behavioral proofs

A third-party module with no Go tests cannot be validated by `go2cs -recurse -tests`, so its nugetgo package ships a
behavioral proof instead (COORD ruling 2026-10-12); a module with Go tests may carry the same rows beside its test
proof, never in its place. Either way it is one row per shipped RID, each written on a host of that RID by
`src/tools/nugetgo/Invoke-NugetgoBehaviorRow.ps1`, usually through the `nugetgo-behavior` stage of the OS matrix
workflow (`docs/CIMatrix.md`).

Each module has one directory, `<module path>@<version>/`, laid out like `src/tests/ModuleDisclosures/`:

| File | Holds |
|:--|:--|
| `main.go`, `go.mod`, `go.sum` | the sample program, run with `go run` and, converted against the package, with dotnet |
| `mappings.txt` | the module's nugetgo mappings row, which names the package ID the conversion references |

A row records both outputs' hashes, and the pack (`nugetgo-pack.ps1 -BehaviorProofDir`) refuses a row whose outputs
differ, whose assembly is not the packed bytes for its RID, or that was recorded on another RID or GOOS.
