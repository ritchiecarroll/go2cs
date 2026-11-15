# DESIGN: a string header's native Data store reads the OLD Len first

**Status:** PLAN, posted to COORD 2026-11-15 for a ruling on the shape. No code yet. Owner: H. This is W8's BOARD row 3,
"a header store of a NATIVE Data, then Len, commits once with the OLD Len first".

## 1. What happens

Go builds a string over native memory field by field:

```go
hdr := (*reflect.StringHeader)(unsafe.Pointer(&s))
hdr.Data = p // p: C memory
hdr.Len = n
```

go2cs-gen's header setter calls `ж<T>.OnHeaderStore()` after EVERY field store
(`src/gen/go2cs-gen/FieldAccessGenerator.cs`), and `StringHeaderBox` commits the header to the string variable then and
there. A native Data commits as a COPY: `@string` has no native-backed form. So the `Data` store copies the header's
CURRENT Len bytes at `p` before the `Len` store arrives.

- When `s` was empty, the old Len is 0 and the first copy reads nothing.
- When `s` held a longer string (a reused variable, a loop, a parameter), the first copy reads the old Len bytes at `p`.
  If `p` names a smaller block, that is an over-read. If the block ends at an unmapped page, the result is a fatal
  access violation.
- Go reads nothing at either store. A header store touches only the header's words, so a Go program that never reads `s`
  between the two stores never touches the bytes.

**The slice header does not have this defect.** `SliceHeaderBox` commits a native Data as a VIEW (`OverNativeMemory`):
it records the address and touches no byte, so the view between the two stores is exactly Go's `(p, oldLen, oldCap)`
slice. The row's "(or views)" names no divergence.

## 2. The census

`data/hdr-native-commit/census-main.go` is TYPE-BASED (go/packages), per target.

**The predicate.** A HEADER STORE is an assignment, op-assignment or `++`/`--` whose left side is a field selector
`x.F`. Here `x`'s type, read through go/types and never by name, is a POINTER to `reflect.StringHeader`,
`internal/unsafeheader.String`, `runtime.stringStruct` or `internal/runtime/maps.stringStruct`. The slice headers are
counted apart. `F` is classified by its declaration index (0 data, 1 len). A header held as a VALUE is counted apart:
it is a plain struct, and go2cs commits nothing on its stores. Per function body, and per header expression, a data
store is classified by the stores of the same expression around it:
- `data-then-len`, the hazard, is split in two:
  - `fresh-empty`: the header views a local `var x string` with no value, nothing assigns `x` before the store, and no
    loop encloses the store without enclosing `x`'s declaration. The old Len is provably 0.
  - `maybe-nonempty`: everything else.
- `len-then-data`, and `data-only`.

Data's right side is also classified, as `go-derived` or `other`.

**Controls** (`ctl/`, with expectations written in each file before its run): the positive control read every class
exactly as written, including the three freshness cases (fresh, in a loop, reassigned). The negative control, a LOCAL
type named `StringHeader` with the same fields plus a read-only `reflect.StringHeader`, read `none`
(`readings/controls.txt`).

**std**, on windows/amd64, linux/amd64, darwin/amd64 and linux/arm64 (`readings/std-*.txt`):
- WITHOUT tests: 0 string-header stores on all four targets. Only slice-header stores occur, and the slice box is not
  affected.
- WITH tests: one string-header data-then-len, `internal/unsafeheader/unsafeheader_test.go:93`. Its Data is
  Go-managed (another string's Data), and the tool classifies it `fresh-empty`: its `alias` starts empty, so it cannot
  over-read.

**modernc.org/sqlite v1.62.0 and modernc.org/libc v1.80.1.** These are not on H, so the type-based census is asked of the
i7. W8's syntactic census, which covered EVERY platform file of both modules (684 and 1375 files), already reads 0
string-header and 0 slice-header WRITES. Its only header uses are Data reads (`inbox/H/20261115T100653Z-COORD-w8-census-3.txt`).

## 3. The cure shapes, compared

| | Today (eager copy) | A: hold a native Data until a Len store | B: commit at the next read | C: one commit after a run of stores |
|---|---|---|---|---|
| Where | golib | golib (`StringHeaderBox`) | golib | go2cs-gen setter + converter |
| A reader between the two stores | Go's answer (the old Len bytes at `p`) | the OLD string: silent wrong value | — | impossible: the run has no statement between |
| A Data store never followed by a Len | Go's answer | the OLD string forever: silent wrong value | — | Go's answer (eager, as today) |
| The over-read when no reader comes between | yes | no | — | no |
| Footprint | — | golib only | — | every header store site that is a run (std: 0 non-test string sites) |

- **A** is golib-only and closes the over-read. It buys that with a silent wrong value in two shapes, both of which Go
  answers. The box can tell a Data-only store from a Len store by comparing the stored words with its last commit.
- **B is not buildable as stated.** Converted code reads `s` straight from its storage, never through the header box,
  so no "next read" reaches golib. Only a later header access or store does, and that is A.
- **C** commits once, after the last of a run of consecutive header stores, so nothing can read between them, and a
  lone Data store keeps today's (Go's) answer. Its cost is real:
  - the setter must not commit inside a run, so the generated setter gains an uncommitted spelling;
  - the converter must recognize the run;
  - the box's L1 rule ("a store that bypasses the setter is refused on the next access") must admit the new spelling.
  That touches the generator, the converter and the box contract together, and it would need a review.

## 4. Red arms (planned; NOT yet written)

A fault cannot be caught in-process, so both arms run in a CHILD process, as `PinExtentsChildTests` does.
- **O (over-read past a small block).** Two pages are mapped, the second made inaccessible (`VirtualAlloc`/
  `VirtualProtect` on windows, `mmap`/`mprotect` on linux and darwin), and a 3-byte block ends exactly at the boundary.
  A string variable holding 4096 bytes is reinterpreted, `Data` is stored at the block, then `Len = 3`, and the child
  prints the string.
  - Today: the `Data` store's copy reads 4096 bytes and faults; the child dies with the platform's access-violation
    status.
  - Go: prints `abc`.
  - The arm asserts Go's answer, so it is RED today.
- **G (the divergence, Go-side).** The same program under Go, run by the arm or recorded as a behavioral pair, shows
  that Go does not fault.
- Until a cure lands, the pair is recorded as DISCLOSED: the arm asserts today's fault by name, as row 11's arm does,
  and flips to Go's answer with the cure.

## 5. The recommendation

The census population on the sqlite path is 0, by the syntactic census on every platform file, with the type-based
confirmation owed by the i7. std's non-test population is 0 on four targets. By COORD's rule (.4 only if the cure is
small and the census is non-empty on the sqlite path), this is a **.5 BOARD item with the census attached**. Shape C
is preferred over A for .5: it keeps Go's answer in every shape, where A trades the over-read for two silent wrong
values. The red arm O is worth writing now as a DISCLOSED arm, if COORD wants the hazard pinned in GolibTests before .5.

— H (2026-11-15)

## 6. Amended 2026-11-15, later: the sqlite path's type-based census

COORD ran the tool on the i7 over `modernc.org/sqlite/...` and `modernc.org/libc/...` (v1.62.0 and v1.80.1, from the
sqlite-cs demo module, tests excluded). It reads `none` on all four targets: windows/amd64 280 files, linux/amd64 393,
darwin/amd64 356 and linux/arm64 384 (`data/hdr-native-commit/readings/sqlite-*.txt`). The prediction was "none" on
each: MET x4. The recommendation in section 5 stands, now on a measured census: a .5 BOARD row, cure shape C.

— H (2026-11-15)
