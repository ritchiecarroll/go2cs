# TRAIN L: the i9's complement shard

From COORD. DRAFT 2026-10-01, adapted from TRAIN K's brief. The i9 sweeps its share of the banked roster at the
TRAIN L union, one row at a time, in its own clone. **Readings only**: nothing to cut, commit or push. Start when
COORD's GO message names the SHA.

| | |
|---|---|
| Union ref | `claude/coord-trainL-union` |
| Union SHA | `<UNION: 40 hex, filled in by COORD at push>` (the TRAIN L fixup commit). The union is K's master + 11 seat merges, or 12 when your nugetgo-pack 5.1 parse fix was merged before the fixup (COORD ruling R2) |
| Shard list | `.claude/coord-scripts/trainL/tL-i9-shard.txt` on `claude/coord-handover`: **132 rows**. It is byte-identical to K's shard, sha256 `057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d` (LF bytes). **The GO is authoritative**: COORD states the sha256 of the COMMITTED blob and its row count there, and the driver refuses to start without both. |
| Driver | `.claude/coord-scripts/trainL/tL-i9-shard.sh` (same branch; handover tip `<HND: filled in by COORD>`) |

## What the shard is

It is the complement of the i7's list over the **225** banked rows (`docs/ValidatedTestPackages.md` at the union).
- K banked `runtime` and `runtime/pprof`, and both go to the **i7** (93 rows). They run there as direct `-tests` legs
  with their bank readings: runtime takes about 150 min, and GOROOT on the i7 is pinned to C: (NTFS).
- Your 132 rows are unchanged from K, so the rows L reaches on your side are already yours:
  - c1-token-ids: `internal/fmtsort`, `encoding/json`, `sync`
  - g-deadlock-checkdead: `internal/synctest`, `sync`, `context`
  - `os` and `testing` stay on your NTFS, as K ruled.

**Your standing exclusion holds.** No crypto/tls row and no TLS-using net row is in the list: `crypto/tls`, `net`,
`net/http`, `net/http/httptest`, `net/http/httputil`, `net/http/cgi`, `net/http/fcgi`, `net/rpc`, `net/smtp` and
`crypto/x509` all stay on the i7. Never add one. The driver now also refuses, by row name, any list row matching
`crypto/tls`, `crypto/x509`, `net` or `net/*` (not only through the list's sha256).

## 1. Setup (K's, with the L names)

1. A **fresh worktree** of your clone at the union, detached, on the volume you used for K's shard:
   `git fetch origin claude/coord-trainL-union` then `git worktree add --detach <path> <UNION SHA>`.
   Assert `git -C <path> rev-parse HEAD` equals the SHA in the GO.
2. A **run folder outside the worktree** (floor 4). Write the driver and the list into it as blob bytes:
   ```
   git fetch origin claude/coord-handover
   git show FETCH_HEAD:.claude/coord-scripts/trainL/tL-i9-shard.sh  > <run>/tL-i9-shard.sh
   git show FETCH_HEAD:.claude/coord-scripts/trainL/tL-i9-shard.txt > <run>/tL-i9-shard.txt
   ```
3. The toolchain is go1.24.13, with `GOROOT` spelled **exactly** as `go env GOROOT` prints it (floor 6).
   - **New in L:** r-m6 excludes stdlib packages from the DefaultGODEBUG stamp by comparing paths against `GOROOT`. A
     forward-slash spelling would break that exclusion as well as the emission.
   - Use a .NET 10 SDK that resolves under `global.json`.
4. Concurrency is the same as your K shard: MSBuild `-m:4`, go `-p 4`. Heavy work is allowed (owner, 2026-09-28).
   WHEA corrected errors are counted per row and are not a stop. Two reboots inside 24 h means you fall back to
   light-only work and tell COORD.
5. Launch OS-detached:
   ```
   W=<worktree, POSIX path> UNION=<SHA> GOROOT_WIN='<go env GOROOT>' EXPECT_LIST_SHA=<from the GO> EXPECT_ROWS=<from the GO> \
     [FIXUP=<fixup SHA from the GO>] [DOTNET_DIR=<SDK dir>] bash <run>/tL-i9-shard.sh
   ```
   While it runs the worktree is FROZEN (floor 4).

## 2. What the driver does

It is K's driver. The L changes:
- **Asserts.** It requires 75648a022b plus the L seats that reach your rows as ancestors: c1-token-ids 667d052869,
  g-deadlock-checkdead 0377dbff39, r-m4 e9009f2945, r-m6 6a079a9675 and c2-publish-binlog-onK df200fa6f8. It also
  requires a `fixup: TRAIN L` commit and 0 csproj still at `<LangVersion>latest</LangVersion>`.
- **`GO2CS_MODULE_ROOT` is unset.** r-m4's converter sets it per host, for module packages only.
- **Deadlock lines.** Every row's stamp counts `all goroutines are asleep` lines (g-deadlock). Expect 0. The sweep
  console prints no host output on a PASS, so the count also reads the row's `full output:` file (on a FAIL) and the
  fresh comparison record's stderr tails (`deadlock-lines=<total> (console=… fullout=… record-stderr=…)`).
- **Named readings** come from each row's FRESH comparison record:
  - `internal/synctest`: `TestDeadlockRoot` and `TestDeadlockChild` must be pass/pass. They run `select{}` inside a
    bubble, and the bubble path in `channel.Wait` is unchanged.
  - `os`: `TestRemoveAllWithExecutedProcess` must be pass/pass (K's re-bank row).
- **Two artifacts at the end**, kept for COORD:
  - `tL-i9-logs/tracked-changes-U0.patch`. COORD runs leg TE-i9 on it (the scripted `tL-i9-te.sh`, the same reader
    as the i7's TE leg), the r-d6 and i9-crosspkg signature check over your rewritten test sources.
  - The **HS** stamp. No rewritten stdlib test host may carry a module-path argument (r-m4), and `GoDefaultGodebug`
    must stay in exactly 3 corpus files (r-m6).
- **Infra reruns, purge and floors** are unchanged from K: an MSB4166-only failure with no `error CS` reruns at most
  twice and is stated. A verdict failure is never rerun by the driver.

**Do not pass these switches:** `-Hop`, `-TestConfig` / `-TestTiered`, `-SkipBuild` and `-IgnoreDiskPreflight`.
- `-PublishBinlog` now exists (c2-publish-binlog-onK is in this union), but it is **not** part of the shard. Its live
  arm is the i7's leg PB.

## 3. What to expect

**132 of 132 at their banked counts, no mover.** The precedents:
- K's shard read clean at K's union.
- Every L seat states footprint 0, or no row verdict moved, for your rows.

What L adds on top:
- **c1-token-ids.** A `ж` token's base is now the allocation's unique id. The ids share one 29-bit counter with map,
  chan and func `Pointer()` identity tokens, so literal `%p`/uintptr text renumbers while verdicts should not. On
  `encoding/json` (532), `internal/fmtsort` (3) and `sync` (46 + 6) expect 0 movers.
  - Rows with alloc asserts (the deferred-alloc class) are the ones to watch: a first token read of a non-box source
    may allocate once.
- **g-deadlock-checkdead.** A forever wait now parks, and the deadlock report follows Go's checkdead rule (exit 2).
  - Watch `internal/synctest` (28), `sync`, `context` (57 + 1).
  - **The residual is now a HANG**, not a 200 ms exit: main blocked on a real channel or sync while every other
    goroutine nil-blocks. A row that times out where it used to pass is the first suspect for that shape: report it.
- **r-m4 and r-m6.** Every test host now calls `TryStageModule`, which declines for stdlib, and initialises the
  godebug default layer. Stdlib hosts carry no stamp. Expect 0 movers and 0 rewritten `go2cs_test_host.cs` beyond
  those K's converter already rewrites.
- **c2-publish-binlog-onK.** A FAIL row's saved output now goes through `Save-RowOutput -OutDir`. Nothing changes
  unless a row fails.
- **csproj among the rewrites: 0.** The fixup's two edits are behavioral projects, not stdlib test projects. Any
  csproj in `tracked-changes.txt` is a finding.

## 4. A row that fails

- Read its saved row output and classify it: count drift, divergence (test names plus the first line of the C# text),
  timeout (see the residual above), or build/infra.
- Compare it with that row's reading in your TRAIN K shard.
- For a verdict failure, run a **CONTROL** after the shard, never beside it: the same sweep command on the same row at
  TRAIN K's master **75648a022b**, in a second fresh worktree. Name a mover by row and test, with
  `control -> union` verdicts.
- Do not fix it. Report it.

## 5. MANDATORY before the bank, after the shard: NGE, the nugetgo uuid end-to-end (you own the seat)

i9-nugetgo-pack's uuid end-to-end was measured on your local, unpushed D4 base, not at any union. **COORD ruling R8:
NGE is MANDATORY before the seat banks.** Run it at the TRAIN L union (the same worktree, after the shard has finished
and been purged; never beside the shard), under **pwsh 7**, and post every number below. It is not a roster gate, but
the seat does not bank without it. The sequence is INFERRED from the scripts' parameters; correct it from your own run
and say what you changed.
1. Build the converter at the union.
2. Run `src/tools/j0-uuid-rehearsal/j0-pack.ps1` to pack the closure to a scratch folder feed
   (push-nuget `-VersionSuffix`, pack-only).
3. Run `go2cs -recurse=nuget` over google/uuid v1.6.0 from your module cache into a scratch root, then `-tests` into
   that recurse layout to produce MODULE.md.
4. Run `src/tools/nugetgo/nugetgo-pack.ps1` with `-ModulePath github.com/google/uuid -GoVersion 1.6.0 -RecurseRoot ...
   -ValidationDir ... -ClosureVersion <closure> -Feed <feed> -OutDir ... -Scratch ... -RepositoryUrl ... -Upstream ...
   -RehearsalSuffix local.N`. Use **pwsh 7** for NGE. (Separately, R2 rules that nugetgo-pack.ps1 MUST also parse under
   Windows PowerShell 5.1, because release-path scripts run there: before your fix it read 10 errors from line 116, a
   BOM-less em dash. Your fix commit on `claude/i9-nugetgo-pack` may be merged into the union before the fixup; the
   i7's leg NGb51 gates it.)
5. Run `j0-consume.ps1 -PackageId nugetgo.github.com.google.uuid -UuidVersion 1.6.0-local.N`.

Expected (the seat's own figures; each one is posted, and a miss is a finding, never smoothed):
- `Validated 54 tests`.
- One `nugetgo.github.com.google.uuid` nupkg with `lib/net10.0/github.com.google.uuid.dll`.
- **18** `go.*` dependencies, each at `[<closure>, 1.25.0)`.
- consume matches `go run` on **28 of 28** lines.
- The global packages folder is unchanged (j0-consume's cache gate: no new `go.*`/`nugetgo.*` id or id/version pair).
- The nuspec description, the release notes and the head of VALIDATION.md each equal the ruled security caveat text
  byte for byte (quote the first line of each in the post).

Never pass `-Release`, never push, and never write output under GOMODCACHE.

## 6. What to post back

Send one message to COORD, at most 40 lines. Subject: `TRAIN L i9 shard at <union sha10>: <pass>/<rows>`.
```
WHAT: the i9's TRAIN L complement shard at <sha10>: <pass>/132 at banked counts, <n> movers.
EVIDENCE:
- tree <sha10> tree <tree10>; list sha <16 hex> rows <n> (= the GO's); go1.24.13 GOROOT <as printed>; sdk <dotnet --version>; -m:4 / -p 4 via <mechanism>
- wall <s>; WHEA total <n> (rows with nonzero: <pkg=n ...>); reboots <n>
- infra reruns: <pkg: attempt lines | none>
- FAIL rows: <pkg: sweep line; the tests; control verdict at 75648a022b; K-shard verdict> (one line each)
- deadlock lines: <0 | pkg=n ...>; NAMED internal/synctest TestDeadlockRoot/Child <v>; os TestRemoveAllWithExecutedProcess <v>
- GENERATED TYPE MISSING: <pkg: once | REPRODUCED | none>
- tracked files rewritten: <n> across <n> rows; csproj among them: <0 | names>; HS <module-path hosts n, GoDefaultGodebug files n>
- TE patch: <run>/tL-i9-logs/tracked-changes-U0.patch (<lines> lines) -- COORD reads it
- NGE (MANDATORY, R8; pwsh <version>, at <union sha10>): Validated <n>/54; nupkg <id, lib path>; deps <n>/18 at <range>; consume <k>/28; GPF unchanged <yes/no>; description/releaseNotes/VALIDATION.md head == ruled caveat <yes/yes/yes | which differs>
- not measured: <what and why>
NEXT: <idle, worktree kept dirty for COORD's ruling on the test sources | the control you are running>
```
Keep `<run>/tL-i9-logs/` and the worktree until COORD says the shard is read.
