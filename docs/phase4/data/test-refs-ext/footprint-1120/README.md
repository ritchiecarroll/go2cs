# CLASS A footprint, re-measured 2026-11-20 (C1), with the committed harness

`src/go2cs/testRefsFootprint_test.go` (TestRefsFootprintOverCommittedTestProjects) runs the converter's own reference
closure over all 242 committed `src/core/**/*.tests.csproj`, from the committed inputs that step reads (912 files:
242 projects, 428 `package_info.cs`, 242 `package_test_info.cs`, md5-identical across the three trees). Each TSV row
is one project: what the closure adds to the references the committed project already has. Go 1.26.11, GOARCH amd64,
tags `purego,math_big_pure_go`.

| arm | ref | adapter |
|:--|:--|:--|
| `base-*` | master a98c5175be | `declarationClosureImports` (no record seeds yet) |
| `a1-*` | 4dc692e5f7, the seat P3 reviewed | `declarationClosureImportsFrom` + `testRecordInterfaces` |
| `a2-*` | 217437c173, with the fixpoint and generic-record fixes | the same |

Readings (predicted in 20261120T072004Z-C1 before the seat arms ran; all MET):
- base adds nothing to any project on any target.
- a1 minus base: windows `internal/reflectlite` + `iter`, `internal/syscall/windows/registry` + `io/fs`;
  linux and darwin `internal/reflectlite` + `iter`.
- a2 minus a1: nothing on any target.

Reproduce: `GO2CS_REFS_FOOTPRINT_GOOS=<goos> GO2CS_REFS_FOOTPRINT_OUT=<file.tsv> go test -run
'^TestRefsFootprintOverCommittedTestProjects$' .` in `src/go2cs`, with `src/core` metadata checked out. For the base
arm, replace `testRefsFootprintClosure_test.go` with that ref's form of the call. About 10 s per target.
