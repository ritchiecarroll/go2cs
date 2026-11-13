# Review evidence: C2's I0 part D (claude/c2-inline-arrays-i0 3c04e59523), C1, 2026-11-13

Read-only censuses over Go source; nothing was converted or built with go2cs. Go 1.26.10, modernc.org/sqlite v1.62.0,
modernc.org/libc v1.80.1, CGO_ENABLED=0. `$GOMODCACHE`, `$GOROOT` and `$SCRATCH` replace local paths. Each tool is a
`main.go` over golang.org/x/tools/go/packages; run it from a module that requires the packages it loads.

| tool | counts | outputs |
|:--|:--|:--|
| `i0d-main.go` | (a) locals and params over 1 KiB of an array-holding struct declared in `DECL` (upper bound for the 7.7.4 by-value cap); (b) uses, outside `DECL` or in its `_test.go` files, of an array field declared in `DECL`, by face | `i0d-lib-*`, `i0d-libc-*`; control `i0d-control-syscall-net-os` |
| `i0e-main.go` | a whole array field boxed into an interface argument; an address, slice or pointer-receiver method BELOW an element (`&x.f[i].g`) | `i0e-*`; controls `i0e-control-std`, `i0e-control-GoImageCopyThroughView` |
| `i0f-main.go` | a keyed array literal initializing an array field; `len`/`cap`/`range` of an array field whose operand calls or receives | `i0f-*`; control `i0f-control-std` |

Every count is over ALL array-holding structs, not the admission's set, so each is an upper bound on what part D admits.
