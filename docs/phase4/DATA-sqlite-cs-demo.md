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

## 5. Amendment, 2026-11-15 15:20 CST: Windows on tree C, the file variant (Run D), W15 corrected, W17, W18

Lane G. Same laptop, toolchains and conventions as section 1. Section 2's table is left as written. The rows this
amendment changes are listed in 5.4.

### 5.1 Run C on Windows

Tree C (`df6f0598ce`), RID `win-x64`, converted at 12:35 and run at 14:22 CST, with `-recurse` and no
`-inline-arrays` flag (W1 is on master). Convert rc=0, no GO2CS error, and no `GoAlignmentCarrier(typeof(array<array<…`
left. The four lines are **byte-identical** to native Go (`cmp`).

- Stand-ins still needed on Windows: W8 1 site (lib's `__ccgo_ts`), W9 (sqlite's Windows mutex), W9b form 1 45,
  form 2 74, W12 398 element doors, W12b 14 reads and 6 whole-array stores.
- Zero sites: W10 and W11 (W11 is Linux-only).
- No stand-in at all: W4, W6 and W7 (on master or in the stack), W13 and W14 (Linux-only), W15 and W15b.
- Run C on Windows is now the Windows run of record. Run A stays in section 1 as the earlier one.

### 5.2 Run D, a probe: the file variant (`demo/file/`, sqlite-cs `4514a03`)

Tree C with today's stand-ins, owner-approved as a probe ("Can try it now just to see what happens", 2026-11-15).
The 17-line native oracle is in section 4.

| | linux-x64 | win-x64 |
|:--|:--|:--|
| when (CST) | 14:11 | 15:10 |
| convert | rc=0 | rc=0 |
| stand-ins | as Run C on Linux, same counts | as Run C on Windows, same counts |
| output | 17 lines, **byte-identical** | 9 lines match (the default journal pass, then `[wal] set journal_mode=wal`); then **W18**, see 5.4 |

Predicted before the run: the default journal pass matches, and the WAL pass meets its first wall at the `-shm`
mapping. On Linux that third prediction **missed**: there is no wall. On Windows a wall does fire on the `-shm`
path, though not for the predicted reason (W18, in 5.4).

To check that the WAL pass really uses the shared mapping, both the converted program and the native one ran
under `strace` on Linux (COORD, 14:15). The evidence is committed:
[`data/sqlite-cs-demo/shm-strace-opens.txt`](data/sqlite-cs-demo/shm-strace-opens.txt) holds the `open` and `mmap`
lines, and [`data/sqlite-cs-demo/shm-strace-locks.txt`](data/sqlite-cs-demo/shm-strace-locks.txt) holds the lock
calls on the `-shm` file, traced one file per thread.

- **Both runs open the same files.** `demo-delete.db`, its `-journal`, `demo-wal.db`, its `-wal` and its `-shm`,
  each with `O_RDWR|O_CREAT|O_LARGEFILE|O_NOFOLLOW|O_CLOEXEC`.
- **Both map `-shm` the same way.** Each maps it as `mmap(NULL, 32768, PROT_READ|PROT_WRITE, MAP_SHARED, fd, 0)`,
  twice.
- **Both take the same locks.** On every WAL lock slot (120 to 128), the `fcntl` calls on `-shm` are identical,
  call for call: 18 distinct lock rows, same counts.
- **Why that settles it.** `strace` cannot see writes to memory. SQLite keeps its WAL index in heap memory only in
  exclusive locking mode, and then it opens no `-shm` at all. Here `-shm` is opened, mapped shared, and locked
  slot for slot as natively.
- **Verdict:** the converted WAL index lives in the shared mapping, as it does natively. There is no behavioral
  difference that `cmp` cannot see, and no new W-row.

### 5.3 W15: fixed on master by C2's carrier fix alone

Section 2 listed W15 under B2. That is wrong. C2's carrier fix `0ed880b101` (merged as `63c2a339c7`, batch 309)
cures it alone. The proof is one Linux run on tree `595f740409`, a scratch tree that is not pushed:

- **The tree:** master `b2214e00e4` (batch 308), plus `0ed880b101`, plus G's stack `d69ddab07b`. It has no B2 and
  no door B.
- **Stand-ins:** the Linux set plus W14, since B2 is absent. W15 and W15b got none.
- **Result:** no alignment carrier is left. `Bitvec` is read on every CREATE TABLE and libc's `in6_addr` with no
  refusal. The four lines are **byte-identical** to native Go.

Run C held both fixes, which is why it could not tell the two apart. Only W14 waits on B2.

### 5.4 W17 and W18, and the ledger as it now stands

W17 is new: at master `0c1d295ea8` (batch 309) the demo's conversion **refuses** (rc=1) on both operating systems,
with two unresolved dynamic types emitted as raw Go in sqlite's `lib`:

- `struct{FnList int32; FaList uintptr}`, from the WAL-index merge sort;
- `struct{FzName uintptr; Fx Tfts5_tokenizer}`, from fts5's built-in tokenizer table.

Both are function-local anonymous structs, and ccgo declares each only through a blank placeholder
(`var _ /* aSub at bp+0 */ [13]struct{…}`). A blank variable gets no lift. Door B (P5, batch 309) now names the
element of `(*[13]struct{…})(p)` from its type, through `@unsafe.Pointer.ElementRef<E>(…)`, instead of rendering
the conversion as before. Rendering is what made the lift, so now nothing does.

W17 is the same class as L1: a type is named in place of the visit that would lift it. It is routed to P5 and
seats in batch 310. It is loud: the converter refuses rather than emitting code that runs wrong. Runs B, C and D
were taken on trees from before door B, and they stand.

W18 is new too, found by Run D on Windows. The first `-shm` map truncates the `-shm` file through sqlite's syscall
table, and the run panics:

```text
panic: go2cs: func cookie 0xc000800000002cc0 holds a Func`6, which cannot be read as Func`6 [recovered]
	panic: go2cs: func cookie 0xc000800000002cc0 holds a Func`6, which cannot be read as Func`6

goroutine 1 [running]:
modernc.org/sqlite/lib._winHandleSeek()
	modernc.org/sqlite@v1.62.0/lib/sqlite_g_00000000000c0000.go:117332
modernc.org/sqlite/lib._winHandleTruncate()
	modernc.org/sqlite@v1.62.0/lib/sqlite_g_00000000000c0000.go:117369
modernc.org/sqlite/lib._winLockSharedMemory()
	modernc.org/sqlite@v1.62.0/lib/sqlite_g_00000000000c0000.go:117556
modernc.org/sqlite/lib._winShmMap()
	modernc.org/sqlite@v1.62.0/lib/sqlite_windows.go:126240
modernc.org/sqlite/lib._sqlite3OsShmMap()
	modernc.org/sqlite@v1.62.0/lib/sqlite.go:34354
modernc.org/sqlite/lib._walIndexPageRealloc()
	modernc.org/sqlite@v1.62.0/lib/sqlite_windows.go:118718
modernc.org/sqlite/lib._walIndexPage()
	modernc.org/sqlite@v1.62.0/lib/sqlite_g_000000000007fd6b.go:13459
modernc.org/sqlite/lib._walIndexReadHdr()
	modernc.org/sqlite@v1.62.0/lib/sqlite_g_00000000000c0000.go:116213
```

- **The site.** lib reads `_aSyscall[37]` as `func(*libc.TLS, THANDLE, TLARGE_INTEGER, TPLARGE_INTEGER, TDWORD)
  TBOOL`. The word was minted from libc's `XSetFilePointerEx`, whose parameter is libc's `largeInteger`.
- **Why S23 did not cover it.** This is sqlite row S23's site. S23's fix (P1, `507c336abf`, batch 302) is in tree
  C, yet lib still lifts its own `TLARGE_INTEGERᴛ1` instead of reusing libc's lift.
- **The root.** The reuse keys on `st.String()`, which keeps the alias names of the field types. lib spells the
  struct `struct{FLowPart …lib.TDWORD; FHighPart …lib.TLONG}`, and libc spells it
  `struct{FLowPart uint32; FHighPart int32}`. `TDWORD = uint32` and `TLONG = int32` are aliases, so Go calls the
  two identical, but the string match misses.
- **Status.** Routed to P1, size S. It is Windows-only: Linux's WAL pass is clean.
- **Diagnostics.** The design's P1.2 promised that this refusal would name the lifted type. It names neither type.

| wall | correction to section 2 |
|:--|:--|
| W10 | retired: door A (batch 308) closed it; 0 sites in Runs C and D |
| W12, W12b | at batch 309 + G's stack, without B2, the stand-ins found 0 and 18 + 1 sites (door B and F1 part C); not run, blocked by W17 |
| W15 | on master (batch 309, C2 `0ed880b101`); needs no B2 |
| W17 | **new**: master batch 309 refuses the conversion; P5, batch 310 |
| W18 | **new**: the file variant's WAL pass on Windows (S23's site; its reuse misses alias-spelled fields); P1 |

### 5.5 The convert-only check COORD adds to the batch battery

COORD is adding a convert-only check to the batch battery, so that a converter batch cannot break this conversion
silently again. Its recipe:

- **Source:** sqlite-cs `demo/` (`modernc.org/sqlite` v1.62.0, `modernc.org/libc` v1.80.1, `go.sum` checked in).
- **Command:** `go2cs -recurse -go2cspath <tree>/src <demo> <out>`, once per target, `windows/amd64` and
  `linux/amd64`.
- **Pass:** rc=0, no "unresolved dynamic type(s) were emitted as raw Go" line in the log, and no `«DYNTYPE` text in
  any emitted `.cs`.
- **Time:** about three minutes per target on G's laptop.
- **One host covers both targets.** `-platforms linux/amd64` with `-recurse` from a Windows host gives
  `.cs` output byte-identical (CR-stripped) to a native Linux conversion of the same tree. Only each `.csproj`'s
  `GoInputDigest` differs, because each host hashes its own checkout of the inputs.
- **It is proven to fire.** Master `0c1d295ea8` reads red on it today (W17).
