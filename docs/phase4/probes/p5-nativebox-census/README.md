# The native-box dereference census (B0): how to run it and how to read it

The instrument for `docs/phase4/DESIGN-interior-pointer-root.md` 4.8 and 10.5. It counts every whole-value read or
write through a `NativeBox<T>` that a later refusal (B2) would close, so that the refusal lands for a class only when
every banked row on every flavour reads 0 for it. The counter is `src/core/golib/NativeBoxDereferenceCensus.cs`; its
one call is in `NativeBox<T>.ValueAddress`.

## What it counts

| Class | A dereference of |
|:--|:--|
| `B2a` | a `T` that holds a reference and is not a Go struct value: a func, a pointer, a string, a slice, an interface, an `unsafe.Pointer`, a map, a channel, and an array value (the CLR form holds its elements by reference). The row's `kind=` says which |
| `B2b` | a Go struct value `T` that holds a reference and that `GoReflect.ClrLayoutMismatch` does not refuse |
| `UNPINNABLE` | any `T`, through a box derived from a managed box whose storage cannot be held still (design 10.1) |

`dereferences` is every dereference, in a class or not. A dereference that is refused (a layout mismatch, an order
token, the nil page) is not counted. A field view and the native word operations are outside the population and are
not counted. Each `B2` row names `T` and its Go kind and says `source=retained`, `source=pinned` or `source=none`. Each `UNPINNABLE` row names the
root box's type.

## Turning it on

Off by default. Two environment variables, read once at start-up by every process that loads golib:

| Variable | Meaning |
|:--|:--|
| `GO2CS_NATIVEBOX_CENSUS` | any non-empty value turns the census on |
| `GO2CS_NATIVEBOX_CENSUS_FILE` | where to write. A `{pid}` in it is replaced by the process id; without one the id is added before the extension. Unset: `nativebox-census-<pid>.txt` in the temporary directory |

A sweep, a `-tests` run and a behavioral run pass their environment to the hosts they start, so setting the two
variables on the command is the whole of it:

```
GO2CS_NATIVEBOX_CENSUS=1 GO2CS_NATIVEBOX_CENSUS_FILE=<dir>/nb-{pid}.txt  <the sweep or the test command>
pwsh -NoProfile -File Read-NativeBoxCensus.ps1 -Path <dir> [-Pattern 'nb-*.txt'] [-Detail]
```

Use an empty `<dir>` per row or per sweep. Helper processes that a package's tests start inherit the variables and
write their own files, which is intended: they are part of the row.

## Reading it

One process writes several blocks into its own file: one at start-up, one at the first dereference, one whenever a
new row appears, one every 250,000 dereferences and one at exit. **Each block is a cumulative snapshot.** The count
of a process is its LAST block, and the count of a run is the sum of those across files. The reader does exactly
that. Summing blocks double-counts, and taking only exit blocks drops a host that died before its exit hook.

- No file at all: the census never armed in that run. That is NOT MEASURED, never 0, and the reader exits 1.
- A file whose last block is the start block: the process armed and dereferenced no native box. A measured 0.
- A file with no parseable block, or with an `NBCENSUS-BROKEN` line: the reader exits 1 and names it.

## Controls

- The counter: `GolibTests/NativeBoxDereferenceCensusControlTests`, run with the variable set. Every class is driven
  on purpose and each assertion is that its row moved by the dereferences made. With the variable unset the driven
  arms report NOT MEASURED and one arm asserts that nothing is counted and no file is started.
- The reader's fold: three synthetic files, one with two cumulative blocks (100 then 250), one whose last block is a
  partial (7), one with a start block only. The correct total is 257; summing every block reads 357. Measured 257.
- The reader's refusal: the same set plus a file with no census block exits 1 naming that file, and an empty
  directory exits 1.

<!-- 2026-11-14, P5, the interior-pointer cut. The shape (a file per process, a start block, snapshots, no routine
     stderr) is Q44RegistryCensus's, for the reasons recorded in that file and in ../c2-census-read/README.md.
     Measured while writing the control: the hand-owned sync.Mutex, which design 10.5 named as B2b's known member,
     is refused by the layout check (its one field is a managed reference at a known Go offset), so it is not in
     the population. -->
