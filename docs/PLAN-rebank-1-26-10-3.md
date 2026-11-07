# PLAN — re-banking the roster for go.* 1.26.10.3

> **State: DRAFT for owner and COORD review (G, 2026-11-07), on `claude/g-dot3-prep` from master `596f75e8ff`.** It
> assigns every roster row to a reading host per OS for the 1.26.10.3 re-bank, from the 2026-11-06 windows canary.
> Nothing here dispatches a row. The canary's verdicts and rulings are cited as the mailbox ledger's lines
> (`docs/phase4/LEDGER.md` on `claude/mailbox`, dated 2026-11-06); this plan points at them and does not restate them.
> The procedure is [`GoCorpusMigration.md`](GoCorpusMigration.md)'s re-bank, as run for 1.26.10.2
> ([`PLAN-release-1-26-10-2.md`](PLAN-release-1-26-10-2.md), *The re-bank legs*), whose structure this plan follows.

## 1. What the canary established

The windows roster (238 rows) was read in full at master between `4aebdadc1a` and `53b3eb88d7`, on go1.26.10:

| reader | rows | tip | ledger lines |
|:--|--:|:--|:--|
| the i9, waves 1-4 | 184 | `4aebdadc1a`, `4a74aecde5`, `624f5bafeb` | 05:44, 06:25, 07:19, 09:24 |
| H, the TLS-ban rows plus `net` and `runtime` | 57 | `53b3eb88d7` | 12:26, 13:16 |

- Three rows were read by both: `crypto`, `runtime` and `net`. So 184 + 57 − 3 = 238.
- **Every row passed at its banked windows count. No roster count moved** (13:16).
- The committed `-tests` corpus drifts. Every drifted file attributes to batches 204, 189, 166 or 211; there are none
  unattributed (05:56; the i9's `logs\attr204` and `logs\attrw2`-`attrw4`). These files churn at the .3 `-tests`
  regeneration by design, and none changes a verdict.

## 2. Windows

| host | rows | projected | basis |
|:--|--:|--:|:--|
| **the i9** | 181: every row it read except `runtime`, `net` and `os`, with `crypto` included | **1.69 h** (6,095 s) | the sum of the i9's per-row walls ([`i9-canary-walls.tsv`](phase4/data/rebank-1.26.10.3/i9-canary-walls.tsv)), less `os` (180 s), which counts four first-row converter builds (one per wave) where the bank has one |
| **H** | 56: the 54 TLS-ban rows (`os/exec` among them: its suite calls StartTLS), `runtime` and `os` | **at least 2.6 h** (9,351 s) | H's 57-row total of 11,087 s, less `net` (1,599 s) and `crypto` (317 s), plus `os` at the i9's 180 s. `crypto` carried that run's first converter build, so removing it and adding the build back nets to about zero. `runtime`'s 3,633 s was read under a 60-minute deadline, so at 90 minutes it is a floor, not an estimate |
| **a GitHub-hosted windows runner** | `net` | about 0.4 h (1,513 to 1,599 s on the two LAN boxes) | ruling (1), 13:16 |

**The rules each host reads under:**
- *The i9* keeps its `GOMAXPROCS=4` cap and does not read `runtime`: the cap disables the update that
  `TestGOMAXPROCSUpdate` checks (07:19). It does not read the TLS-ban rows, under the owner's standing ban.
- `crypto` (top-level) is outside the i9's `crypto/*` ban predicate, and both hosts read it 8 = 8 (13:16), so it is
  the i9's.
- *H* reads `runtime` with a **90-minute test deadline** (ruling (2), 13:16): its read took 3,633 s under 60 minutes,
  the tightest margin on the roster.
- *H* reads `os`, and first confirms that `C:\pagefile.sys` exists (COORD ruling, 2026-11-07; §6).
- *`net`* banks from the os-matrix `sweep-shard` windows job against that runner's own Go oracle (ruling (1), 13:16).
  The LAN resolver answers SERVFAIL for `invalid.invalid` on both LAN boxes, and no LAN box banks `net`. There is no
  network change.
- **H's per-row walls were not kept**; H's scratch was reclaimed after its DONE. Its shard is one serial list, so its
  total sizes it. H posted the walls of its ten longest rows, and the other 47 sum to 2,933 s (a mean of about 62 s).

**The windows critical path is H, at least 2.6 h** (`runtime`'s wall is a floor). The i9 and the hosted runner finish inside it.

## 3. Linux

The canary read windows only. The linux leg is the per-package pipeline, never the sweep, with each row re-read at the
re-bank tip. Its deal and walls are 1.26.10.2's
([`data/rebank-1.26.10.2/legs.tsv`](phase4/data/rebank-1.26.10.2/legs.tsv)), go1.26.10 measurements, because no .3
linux wall exists yet:

| host | shards | rows | projected (1.26.10.2's walls) | holds |
|:--|:--|--:|--:|:--|
| **R-WSL** | RWSL-1, RWSL-2 | 143 | 4.02 h | the reserved set (every `$longTimeouts` key, `runtime` and `internal/poll` included) and the rows heavy on disk or memory (`archive/zip`'s 4 GiB streams, the TLS and HTTP rows, `index/suffixarray`) |
| **P1** (cloud container) | P1-1, P1-2 | 46 | 4.94 h | `testing` last in P1-2 |
| **P2** (cloud container) | P2-1, P2-2 | 47 | 5.03 h | `crypto/x509` in P2-1 |

- **The backup lane is G's WSL**, qualified for 1.26.10.2 and kept: go1.26.10 and dotnet 10.0.401 under its home,
  TMPDIR=/tmp, act `all`. R-WSL is primary (COORD ruling, 2026-11-07). It never runs beside a windows shard on G,
  because both share G's system drive.
- The linux critical path is about 5.0 h from dispatch, plus about 15 minutes per shard for clone and build, as at .2.

## 4. Darwin

| host | dispatches | rows | projected | rules |
|:--|:--|--:|--:|:--|
| **C1**, the os-matrix CI shards (osx-x64 and osx-arm64) | C1-1 … C1-15 | 236 | 11.5 h of the slower RID (1.26.10.2's walls), about 4 to 4.5 h of wall time at five mac jobs at once | dispatch `runtime` (C1-13) first |

- **Arch-strict:** the `-tests` oracle runs at the RID's own GOARCH (batch 208), so osx-arm64 is read against an arm64
  Go oracle, not a Rosetta artifact.
- A darwin row whose two RIDs differ **banks the arch-strict reading**, never a sum or the kinder of the two (C1 AES
  ruling (1), ledger 2026-11-06 06:31); `crypto/subtle`'s darwin row is the named instance.
- Batch 216 moves `crypto/tls`'s osx-arm64 cipher preference to AES-GCM, as Go's; its darwin row reads at the bank tip.

## 5. Pages that re-render at the bank (stale renders)

These pages carry text from before a landed record change. The re-bank re-renders them, and no count moves:

| page | what is stale | ledger |
|:--|:--|:--|
| `testing` | `TestTBHelper`'s reason, rendered 2026-11-04 and rewritten by batch 191 | 07:19 |
| `internal/cpu` | rendered before `b769aa3bc2` | 09:24 |
| `internal/runtime/gc/scan` | rendered before `b769aa3bc2` | 09:24 |
| `crypto/sha1` | its darwin withdrawn section | 12:26 |

## 6. Rows whose reading depends on the host (host truth)

| row | what the host changes | where it banks |
|:--|:--|:--|
| `os` | `TestStatPagefile` skips where `C:\pagefile.sys` does not exist; the i9's pagefile is on another drive, so the i9 reads skip|skip where the page has pass|pass (05:44) | H, which first confirms `C:\pagefile.sys` exists: if it does, `os` banks at its banked pass|pass; if not, the bank records skip|skip as host truth with the reason (COORD ruling, 2026-11-07) |
| `archive/tar` | `TestFileInfoHeaderSymlink` passes where Windows Developer Mode is on (the i9 since 2026-10-26), and skips without it | the i9; the skip-to-pass line is host truth (06:25) |
| `net` | the LAN resolver answers SERVFAIL for an invalid name, so `TestLookupNoSuchHost` fails on every LAN box | the GitHub-hosted windows runner (§2) |

## 7. Rulings (COORD, 2026-11-07)

- `os` banks on H, which first confirms that `C:\pagefile.sys` exists (§2, §6).
- The linux hosts: R-WSL is primary, as at 1.26.10.2, with G's WSL as the backup lane (§3).
