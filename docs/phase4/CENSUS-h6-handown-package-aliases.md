# CENSUS — hand-own package aliases against the 1.24.13 release (H6, release-agnostic half)

**Point-in-time record.** Taken at master `44f858717` (train 45 landed) on 2026-09-08 by lane G, per
COORD's dispatch in the train-45 landing follow-up. Amend with dated blocks; never execute from it.

## What this measures, and what it deliberately does not

For every `[module: GoManualConversion]` hand-own in the corpus, every `using … _package;` alias is
mapped back to a Go import path and classified against the **go1.24.13** GOROOT: does that package
still exist, has it MOVED, or is it ABSENT. It needs **GOROOT and master only** — no build, no .NET,
no converter run — which is why it is the *release-agnostic* half.

It does **not** reach the EMISSION half: whether a given C# namespace still resolves after the hop
depends on what the corpus emits, not on what GOROOT contains. See *Limits* below, which records a
mechanism this census measured to be FALSE rather than leaving it to be assumed.

## Population

| | |
|---|---|
| marked files (line-anchored `^\s*\[module:\s*(go\.)?GoManualConversion\]`) | **145** |
| the same grep UNANCHORED | 225 — 80 false, the documented trap |
| `using … _package;` aliases across them | **94** |

The 145 matches lane R's independently-derived seed figure.

## Verdicts

| verdict | count |
|---|---|
| EXISTS at 1.24.13 | **92** |
| MOVED | **2** |
| ABSENT | **0** |

### The MOVED rows — the whole actionable set

| file | alias target | 1.24.13 |
|---|---|---|
| `runtime/mfinal.cs` | `runtime/internal/sys` | → `internal/runtime/sys` |
| `runtime/runtime2.cs` | `runtime/internal/sys` | → `internal/runtime/sys` |

**These are exactly the two files lane R identified by BUILDING the re-based ladder** (16 root errors,
identical on three flavours). Two instruments from opposite directions — a corpus build, and this
census with no compiler in it — reaching the same two files and the same alias.

**So the rung order after `runtime` is EMPTY**: 143 of the 145 marked files carry no moved or absent
package alias.

Free H3 datum: `runtime/internal/math` also moved (→ `internal/runtime/math`). No hand-own aliases
it, so it is not a rung, but it is a second member of the same relocation.

## Limits — including a mechanism measured FALSE

**The bare `using <ns>;` form is counted but does not appear above.** 403 of them exist across the
145 files; **zero name a directory lost at 1.24**.

⚠ **The tempting explanation for R's second alias is FALSE and this census says so rather than
guessing.** `runtime/internal` survives at 1.24: it loses `math` and `sys` but keeps
`startlinetest` and `wasitest`, **both still in `go list std`, and the corpus converts all four
today**. So the C# namespace does not go empty, and the `CS0246` R observes on `using runtime.@internal;`
is **not** emptiness. Its mechanism belongs to the emission half.

## Instrument, and the four defects found in it

Scripts: `g-h6census.sh`, `g-nstogo.sh`, `g-movedto.sh` (lane G scratchpad). Every defect below was

**Read layer, stated because a census that does not name one invites the mismatch.** Every
content read is a BLOB read — `git show <ref>:<path>` for each file, and `git grep <ref>` for
the marked-file list. No worktree file is opened, so there is no CRLF-versus-LF layer to get
wrong: the numbers here are properties of the commit `44f858717`, not of anybody's checkout.
(Prompted by lane R's `68c3b733f`, where comparing CRLF working files against LF blobs made
three files read as belonging to neither tree.)
found by **reading the rows**; the totals looked clean at each stage.

1. `/MOVED/` matches inside `REMOVED` — totals read "MOVED 32 / REMOVED 0" over rows that plainly
   said REMOVED. Cured by renaming the third verdict **ABSENT**, so the tokens are substring-disjoint;
   not by a smarter grep.
2. The corpus root namespace `go.` was not stripped, so `go.internal.bisect_package` mapped to
   `go/internal/bisect` and read ABSENT — **29 rows** of a bogus 62/32 split.
3. A naive `.`→`/` breaks a vendored domain: `…vendor.golang.org.x.sys.cpu_package` became
   `vendor/golang/org/x/sys/cpu`, a false ABSENT for a package present at BOTH releases. Cured with a
   resolver (naive path, then re-join each adjacent pair with a dot, take the first that exists).
4. The MOVED resolver first answered `cmd/internal/sys` — a real package and the wrong one — by taking
   the first same-basename directory. It now ranks candidates on shared path components, excludes
   toolchain trees, and prints `AMBIGUOUS{…}` rather than picking one.

**Controls**: the mapper reports `runtime/internal/sys` → `internal/runtime/sys` (R's own member), and
`crypto/internal/alias` → `crypto/internal/fips140/alias` (matching `claude/g-weak-rekey`'s subject).
Re-running the whole census reproduces 145 / 94 / 92-2-0 exactly.

## Full table

```
FILE                                                     ALIAS-TARGET                         VERDICT
crypto/internal/boring/bcache/cache.cs                   sync/atomic                          EXISTS
crypto/subtle/xor_generic.cs                             runtime                              EXISTS
debug/pe/symbol_impl.cs                                  internal/saferio                     EXISTS
debug/pe/symbol_impl.cs                                  encoding/binary                      EXISTS
debug/pe/symbol_impl.cs                                  errors                               EXISTS
debug/pe/symbol_impl.cs                                  fmt                                  EXISTS
debug/pe/symbol_impl.cs                                  io                                   EXISTS
internal/concurrent/hashtriemap_whitebox.cs              sync/atomic                          EXISTS
internal/godebug/godebug.cs                              internal/bisect                      EXISTS
internal/godebug/godebug.cs                              internal/godebugs                    EXISTS
internal/syscall/unix/darwin/net_darwin_impl.cs          internal/abi                         EXISTS
internal/syscall/unix/darwin/net_darwin_impl.cs          syscall                              EXISTS
internal/syscall/unix/darwin/user_darwin_impl.cs         internal/abi                         EXISTS
internal/syscall/unix/darwin/user_darwin_impl.cs         syscall                              EXISTS
internal/syscall/unix/linux/net_linux_impl.cs            syscall                              EXISTS
internal/syscall/windows/exec_windows_test.cs            fmt                                  EXISTS
internal/syscall/windows/exec_windows_test.cs            os/exec                              EXISTS
internal/syscall/windows/exec_windows_test.cs            io                                   EXISTS
internal/syscall/windows/exec_windows_test.cs            os                                   EXISTS
internal/syscall/windows/exec_windows_test.cs            syscall                              EXISTS
internal/syscall/windows/exec_windows_test.cs            testing                              EXISTS
internal/syscall/windows/registry/registry_test.cs       bytes                                EXISTS
internal/syscall/windows/registry/registry_test.cs       crypto/rand                          EXISTS
internal/syscall/windows/registry/registry_test.cs       internal/syscall/windows/registry    EXISTS
internal/syscall/windows/registry/registry_test.cs       os                                   EXISTS
internal/syscall/windows/registry/registry_test.cs       syscall                              EXISTS
internal/syscall/windows/registry/registry_test.cs       testing                              EXISTS
internal/syscall/windows/registry/windows/value.cs       errors                               EXISTS
internal/syscall/windows/registry/windows/value.cs       syscall                              EXISTS
internal/syscall/windows/registry/windows/value.cs       unicode/utf16                        EXISTS
internal/syscall/windows/windows/net_windows_impl.cs     syscall                              EXISTS
internal/syscall/windows/windows/syscall_windows_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_module_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_privilege_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_ptrout_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_version_impl.cs syscall                              EXISTS
internal/syscall/windows/windows/zsyscall_windows_wsa_impl.cs syscall                              EXISTS
net/windows/interface_windows_impl.cs                    internal/syscall/windows             EXISTS
net/windows/interface_windows_impl.cs                    os                                   EXISTS
net/windows/interface_windows_impl.cs                    syscall                              EXISTS
net/windows/lookup_windows.cs                            internal/syscall/windows             EXISTS
net/windows/lookup_windows.cs                            context                              EXISTS
net/windows/lookup_windows.cs                            os                                   EXISTS
net/windows/lookup_windows.cs                            syscall                              EXISTS
net/windows/lookup_windows.cs                            time                                 EXISTS
os/darwin/dir_darwin_impl.cs                             internal/poll                        EXISTS
os/darwin/dir_darwin_impl.cs                             io/fs                                EXISTS
os/darwin/dir_darwin_impl.cs                             sync/atomic                          EXISTS
os/darwin/dir_darwin_impl.cs                             syscall                              EXISTS
os/linux/wait_waitid.cs                                  syscall                              EXISTS
os/user/windows/lookup_windows_impl.cs                   internal/syscall/windows             EXISTS
os/user/windows/lookup_windows_impl.cs                   fmt                                  EXISTS
os/user/windows/lookup_windows_impl.cs                   syscall                              EXISTS
os/windows/dir_windows_impl.cs                           internal/syscall/windows             EXISTS
os/windows/dir_windows_impl.cs                           io/fs                                EXISTS
os/windows/dir_windows_impl.cs                           syscall                              EXISTS
os/windows/file_windows_impl.cs                          internal/syscall/windows             EXISTS
os/windows/file_windows_impl.cs                          syscall                              EXISTS
reflect/makefunc_impl.cs                                 internal/abi                         EXISTS
reflect/value_impl.cs                                    internal/abi                         EXISTS
reflect/value_impl.cs                                    strconv                              EXISTS
runtime/darwin/signal_posix_darwin_impl.cs               internal/runtime/atomic              EXISTS
runtime/debug/stubs_impl.cs                              time                                 EXISTS
runtime/linux/signal_posix_impl.cs                       internal/runtime/atomic              EXISTS
runtime/mem_persistent_impl.cs                           internal/goarch                      EXISTS
runtime/mem_persistent_impl.cs                           internal/runtime/atomic              EXISTS
runtime/mfinal.cs                                        internal/abi                         EXISTS
runtime/mfinal.cs                                        internal/goarch                      EXISTS
runtime/mfinal.cs                                        internal/runtime/atomic              EXISTS
runtime/mfinal.cs                                        runtime/internal/sys                 MOVED -> internal/runtime/sys
runtime/mranges_impl.cs                                  internal/goarch                      EXISTS
runtime/pprof/pprof_impl.cs                              internal/profilerecord               EXISTS
runtime/pprof/symtab_impl.cs                             runtime                              EXISTS
runtime/runtime2.cs                                      internal/abi                         EXISTS
runtime/runtime2.cs                                      internal/chacha8rand                 EXISTS
runtime/runtime2.cs                                      internal/goarch                      EXISTS
runtime/runtime2.cs                                      internal/runtime/atomic              EXISTS
runtime/runtime2.cs                                      runtime/internal/sys                 MOVED -> internal/runtime/sys
sync/once.cs                                             sync/atomic                          EXISTS
sync/poolqueue.cs                                        sync/atomic                          EXISTS
syscall/darwin/exec_libc2_impl.cs                        errors                               EXISTS
syscall/darwin/sockaddr_darwin_impl.cs                   internal/abi                         EXISTS
syscall/linux/exec_unix.cs                               internal/bytealg                     EXISTS
syscall/linux/exec_unix.cs                               errors                               EXISTS
syscall/windows/dll_windows.cs                           internal/syscall/windows/sysdll      EXISTS
syscall/windows/exec_windows.cs                          internal/bytealg                     EXISTS
syscall/windows/exec_windows.cs                          unicode/utf16                        EXISTS
syscall/windows/syscall_windows_impl.cs                  errors                               EXISTS
unique/clone.cs                                          internal/abi                         EXISTS
unique/clone.cs                                          internal/stringslite                 EXISTS
vendor/golang.org/x/crypto/sha3/xor.cs                   encoding/binary                      EXISTS
vendor/golang.org/x/crypto/sha3/xor.cs                   vendor/golang.org/x/sys/cpu          EXISTS
vendor/golang.org/x/net/route/darwin/sys_impl.cs         syscall                              EXISTS
```
