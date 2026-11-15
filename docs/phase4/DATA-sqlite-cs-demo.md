# DATA: the sqlite-cs demo, runs of record

Lane G, 2026-11-15. Written at COORD's order (13:00 CST) so the run of record is a durable file. Until today it
existed only in G's direct posts to COORD. This is a record: runs as they were taken, the wall ledger as it stands,
and the commands. Nothing here lands code. The trees named below are SCRATCH merges in G's clone, never pushed; each
is named by the pushed commits it merges, so it can be rebuilt.

The demo is `demo/` in the public sqlite-cs repository (`726aeb8`): it opens an in-memory database through
`database/sql` with `modernc.org/sqlite` v1.62.0, creates a table, inserts three rows and prints a query. Under
go1.26.11, natively, it prints these four lines on both operating systems (`cmp` equal):

```text
sqlite 3.54.1
2 beta 2.25
1 alpha 1.5
3 gamma -3
```

## 1. Runs of record

All three ran on one Windows x64 laptop, G's. "Linux" is WSL2 on that laptop: Ubuntu 22.04.5 LTS, WSL2 kernel
6.18, RID `linux-x64`, .NET SDK 10.0.403. Windows is RID `win-x64`, .NET SDK
10.0.400. Both use go1.26.11. A stand-in is a scratch text rewrite of the converted C#, made only to look past a
wall. Each is named by its W-id in section 2, and each wall's real fix is another seat's.

| | Run A | Run B | Run C |
|:--|:--|:--|:--|
| when (CST) | 2026-11-15 01:58 | 2026-11-15 00:47 | 2026-11-15 12:34 |
| OS, RID | Windows, `win-x64` | Linux (WSL2), `linux-x64` | Linux (WSL2), `linux-x64` |
| go2cs tree | `b64be5b0a6` | `e489b498d7` | `df6f0598ce` |
| tree is | master `c2919b8b16` (batch 305) + W2 `d078a4ea21` + W5a `6b27144002` + P2's seat 3 `e2784f7e02` | master `091935257c` (batch 307) + L1 `8e8c6dd0e8` + W7 `ff06d10513` | B2 `4c8ce23f81` (C2, on master `b2214e00e4`, batch 308) + G's 310 stack `d69ddab07b` (L1, W7, W13) |
| convert flags | `-recurse -inline-arrays "-golang.org/x/sys/windows"` (W1's stand-in) | `-recurse` | `-recurse` |
| stand-ins | W4, W6, W7, W8, W9, W9b forms 1 and 2, W10, W12, W12b, W15b | W8, W9 linux, W9b forms 1 and 2, W10, W11 (both halves), W12, W12b, W13, W14, W15, W15b | W8, W9 linux, W9b forms 1 and 2, W11, W12, W12b |
| converted output | the four lines above | the four lines above | the four lines above |
| `cmp` with native | **byte-identical** | **byte-identical** | **byte-identical** |

- Run C is the B2 gate reading: W14 and W15 ran with **no** stand-in, and W10's stand-in found **0** sites, because
  door A had reached master. Its stand-in counts were W8 2 (lib and libc), W9 linux 5 functions, W9b form 1 50,
  form 2 76, W11 one record and one adapter, W12 813 element doors, W12b 54 reads and 68 whole-array stores.
- Run A is the only Windows run of record, and its tree is older (batch 305 plus three seats). A Windows run on
  Run C's tree was started at 12:35 CST and had not finished when this was written. An amendment records it.
- Run A's W9 stand-in is sqlite's own Windows mutex (`lib/mutex.go`). On Linux, sqlite uses pthread mutexes in libc
  instead, so W9 there is libc's `pthreadMutex` (`W9 linux`).
- None of the three trees is pushed. Each is rebuilt by merging the pushed commits named in the table. Run C has one
  conflict, in `docs/ConversionStrategies.md`, taken from B2.

## 2. The wall ledger

One line per wall: what it is, then where it stands at master `0c1d295ea8` (batch 309). "Stand-in" means the runs
above looked past it with a scratch rewrite. Sizes and owners are COORD's rulings.

| wall | what it is | status |
|:--|:--|:--|
| W1 | `golang.org/x/sys/windows` refused (GO2CS2010): inline-array mixed pairs | on master (batch 306, C2) |
| W2 | sqlite `lib` aliases emitted as raw Go: a 64 KB line reader, and a forward alias lift | on master (batch 306, G) |
| W3 = S8c | `mathutil` CS1929: a direct-box method on a named result | on master (batch 307, P2's seat 3) |
| W4 = S10 | libc's `gotime.*` keeps Go member names through a renamed import | on master (batch 307, P2) |
| W5a | an element address names an imported anonymous struct's lift | on master (batch 307, G) |
| W5b | libc CS0021 through `(*RawMem)(p)[:]` | compile closed by P5's door (batch 305); run time is doors A and B |
| W6 | a struct-level `[GoInlineLayout]` stamp without `using go.golib;` | on master (batch 306, C2) |
| W7 | an alias record keeps an unresolved dynamic-type marker (forward alias chain) | seat in 310 (G, `claude/g-sqlite-w7-alias-record-markers`) |
| W8 | `__ccgo_ts`: a `StringHeader` reinterpret needs a lifetime-stable base | stand-in; H's seat, in gate |
| W9 | sqlite's Windows `mutex` struct in C memory holds a managed `sync.Mutex` | stand-in; P2 (ruled: a Go-layout `sync.Mutex`) |
| W9 linux | libc's `pthreadMutex` in C memory, the same class, on the Linux main path | stand-in; P2 (310 stack) |
| W9b form 1 | `FromPinnedBox(Ꮡ((~To<T>(p))).of(T.ᏑF))`: a write through `&(*T)(p).F` boxes a copy | stand-in; P2 (310 stack) |
| W9b form 2 (W16) | `Ꮡ((To<T>(p)).Value.F).Reinterpret<…>()`: the same, a copy of the field | stand-in; P2 (310 stack) |
| W9b form 3 | `&*(*T)(p)`: 23 Linux libc float sites, 2 of them writes | not on the demo's path; P2 |
| W10 = S3 | `Xmemcpy` copies into a temporary span view | on master (door A, P5, batch 308); 0 sites at Run C |
| W11 | `syscall.ΔSignal` spelled by its Go name in a GoImplement record and its adapter | stand-in; P2 (310 stack) |
| W12 | an index through a conversion to an unnamed array over C memory | stand-in at Run C; P5's door B and F1 part C are in batch 309, re-count owed |
| W12b | door B's struct-element reads and whole-array stores | stand-in at Run C; same as W12 |
| W13 | a collision-renamed package var's box named by its raw name (`Ꮡtid`) | seat in 310 (G, `claude/g-sqlite-w13-collision-renamed-receiver`) |
| W14 | libc's `TBF_ctx` (4,168 bytes) kept out of inline arrays; its reinterpret reads null fields | B2 (`claude/c2-b2-value-decode` `4c8ce23f81`); met without stand-in at Run C |
| W15 | `[0][N]T` alignment carriers drop N (`Bitvec` on every CREATE TABLE; libc's `in6_addr`) | B2, same; met without stand-in at Run C |
| L1 | a function-local anonymous-struct lift unnamed in a Reinterpret and in multi-name vars | on master (batch 309, G) |

Door C (`goWideString`) was not reached in any run.

## 3. Commands

Paths are written with `<…>` placeholders. `<go2cs>` is a go2cs checkout at the tree named, and `<demo>` is
sqlite-cs's `demo/`. Converting a single package or module writes beside its input unless an output directory is
given, so always pass `<out>`.

Convert (both OSes; on Linux the same command in WSL):

```text
go -C <go2cs>/src/go2cs build -o <bin>/go2cs .
cd <demo>
<bin>/go2cs -recurse -go2cspath <go2cs>/src <demo> <out>
```

Build:

```text
Windows:  dotnet build <out>/src/sqlitecs/demo/sqlitecs.demo.csproj -c Release
Linux:    dotnet build <out>/src/sqlitecs/demo/sqlitecs.demo.csproj -c Release -p:GoTargetOS=linux
```

Run, then compare with native:

```text
Windows:  <out>/…/bin/…/sqlitecs.demo.exe > converted.txt
Linux:    dotnet <out>/.artifacts/bin/…/Release/net10.0/demo.dll > converted.txt
          (cd <demo> && go run .) > native.txt
          cmp converted.txt native.txt
```

On Linux, `-p:GoTargetOS=linux` is required: without it the Windows flavour of the standard library links. The
stand-ins of section 1 are applied to `<out>` between convert and build. They are diagnosis only, and none is part
of any reproduce step that claims the gate.

## 4. The file-backed variant (owner-approved, 2026-11-15)

Planned at `demo/file/` in sqlite-cs, as its own module `sqlitecs/demo/file`. Go's `./...` excludes a nested
module, so `demo/`'s closure, and its four-line oracle above, are unchanged. For SQLite's default journal mode and
then WAL, it creates a database file in a fresh temporary directory, creates a table and an index, inserts rows and
closes the file. It then reopens the file, checks it, queries it, closes it and removes it. It prints no path and no
time. Native output under go1.26.11, `cmp` equal on Windows and Linux:

```text
sqlite 3.54.1
[delete] created: file present=true, nonempty=true
[delete] reopened: journal_mode=delete integrity=ok pages=3 rows=3
[delete] 2 beta 2.25
[delete] 1 alpha 1.5
[delete] 3 gamma -3
[delete] first name from b: beta
[delete] removed: true
[wal] set journal_mode=wal
[wal] created: file present=true, nonempty=true
[wal] reopened: journal_mode=wal integrity=ok pages=3 rows=3
[wal] 2 beta 2.25
[wal] 1 alpha 1.5
[wal] 3 gamma -3
[wal] first name from b: beta
[wal] removed: true
```

Its Go source goes to sqlite-cs when ready. Its converted output, and any build of it, stays off that repository
until 1.26.11.1 publishes (owner, 2026-11-15). It is converted in the post-310 no-stand-in re-measure on both
operating systems. A refusal on the WAL pass (shared memory, `mmap`) is recorded as an S-row, not dropped.
