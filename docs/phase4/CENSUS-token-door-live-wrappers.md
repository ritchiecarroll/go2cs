# CENSUS — the wrappers that hand the token door a tagged token

*C1, 2026-09-13, measured at `a02ac3df3`. Instrument: `src/token-door-census.sh`.*

This began as COORD's sizing of the `syscall TestGetStartupInfo` validation candidate
(`b91d60e4d`, `9da4d9f9a`). The sizing's answer turned out not to be about that test, so the record
is named for what it actually measures.

**One sentence:** `TestGetStartupInfo` is not a validation candidate — it is a **stale bank whose
predicted verdict is now FAIL**, the bank predates the machinery that must refuse it by three
converter-weeks, and the row is **one of seven**.

---

## 1. The carried-in hypothesis was wrong, and it is worth saying why

The sizing arrived with a hypothesis: *the wrapper's error derivation is suspect, since
`GetStartupInfoW` returns `void` and the Go wrapper must synthesise an error.* It does not.

```go
// GOROOT/src/syscall/syscall_windows.go:1456
func GetStartupInfo(startupInfo *StartupInfo) error {
	getStartupInfo(startupInfo)
	return nil                     // https://go.dev/issue/31316
}
```

```csharp
// src/core/syscall/windows/syscall_windows.cs:1554
public static error GetStartupInfo(ж<StartupInfo> ᏑstartupInfo) {
    getStartupInfo(ᏑstartupInfo);
    return default!;
}
```

The emission is faithful. `return nil` is unconditional on both sides, so the test's only assertion —

```go
err := syscall.GetStartupInfo(&si)
if err != nil { t.Fatalf(...) }      // syscall_windows_test.go:221
```

— **cannot fire**. The row's assertion is structurally incapable of failing, whatever the wrapper
does to memory. A PASS on this row certifies `nil == nil`; that is worth knowing before anyone
spends a Windows run banking it.

## 2. The defect is one frame down, at the pointer — the chain, read at the code

Every link below is a file and line in this tree, not a recollection:

| # | Fact | Site |
|---|---|---|
| 1 | `StartupInfo` declares four reference-typed fields — `ж<uint16> _`, `Desktop`, `Title`, `ж<byte> ___` | `types_windows.cs:424-439` |
| 2 | `ж<T>` is `public abstract partial class` — a reference type | `golib/ж.cs:85` |
| 3 | so `IsReferenceOrContainsReferences<StartupInfo>()` is true → `StandardBox` stores in `m_val` and allocates **no `m_slot`** | `golib/ж.StandardBox.cs:54-68` |
| 4 | no slot → `StorageKind` is `PointerStorage.None` | `golib/ж.StandardBox.cs:173` |
| 5 | `operator uintptr` therefore takes the **token arm**, not any address path | `golib/ж.cs:817-822` |
| 6 | the token is `AllocationBase(hash)` = `TagBit \| …` with **bit 63 forced set** — non-canonical on x86-64, impossible as a user-mode address | `golib/ж.cs:489-496` |
| 7 | `syscalln`'s **first statement**, before any dispatch, is `refuseManagedPointerTokens(fn, a)` | `dll_windows.cs:159` |
| 8 | which throws `panic` on any tagged argument, naming the argument index and the remedy | `dll_windows.cs:134-150` |

Both branches of `Ꮡ<T>` reach the same answer: `SlottedStandardBox<T>` derives from `StandardBox<T>`
and only adds a view slot (`ж.Views.cs:93-100`), so step 3 is unchanged.

`getStartupInfo` is **not displaced** by any hand-own — the `[module: GoManualConversion]` hits in
`zsyscall_windows.cs` are all placeholder *comments* naming other functions, and a corpus-wide grep
for the symbol finds only the emission and its `//sys` directive.

**Predicted verdict at this tree:**

```
panic: syscall: argument 0 is a managed pointer token, not an address -- the pointee is
reference-bearing, so passing it to native code would read or write memory that is not the
caller's. Hand-own this wrapper against a blittable mirror (see zsyscall_windows_version_impl.cs)
```

The corpus states the same fact in its own words, one file over, written by whoever built the
process-launch hand-own:

> *"The converted `[GoType]` structs (`types_windows.cs` `StartupInfo` / `_STARTUPINFOEXW`) hold
> their pointer fields as managed `ж<T>` boxes, **so they can neither be sized nor passed**; these
> are the blittable equivalents."* — `exec_windows.cs:320-324`

## 3. The bank predates the door by three converter-weeks

Dated at the tree with `git log -S`, not recalled:

| Date | Ref | Event |
|---|---|---|
| **2026-08-25** | converter `e2182a59e` | `syscall` proof banked: `TestGetStartupInfo \| pass \| pass`, **65 matched · 0 disclosed**, Go 1.23.12 `windows/amd64` |
| 2026-09-05 | `b50d08c422` | **Q44** — a reference-bearing box's `(uintptr)` becomes an order token instead of a movable interior address |
| 2026-09-06 | `d6e181fe1a` | the three-way `PointerStorage` split |
| **2026-09-08** | `3e5ead2d19` | **the token door** — the tag at the mint, the refusal at the trampoline |

So the recorded pass was measured *before* the machinery that must now refuse it existed. That pass
is not evidence the row works: before Q44 the operator handed out a real (movable, unpinned)
interior address, the kernel wrote 104 bytes of `STARTUPINFOW` into the box's own storage at the
wrong offsets, nothing faulted, and the vacuous assertion passed anyway. **The door did not break
this row. It made a row that was already wrong say so.**

## 4. ⚠ It is SEVEN, not one

`src/token-door-census.sh` asks one predicate — *does this wrapper pass, as a direct `(uintptr)`
argument to the trampoline, a `ж<T>` whose `T` is reference-bearing?* — over
`syscall/windows`. Reference-bearing is closed **transitively** (a struct carrying a
reference-bearing struct is one too); under-reporting is the wrong direction for a census whose
finding is a count.

```
== reference-bearing [GoType] structs in syscall/windows: 33
== functions parsed in zsyscall_windows.cs: 127 of 127 bodied signatures
== wrappers handing the trampoline a reference-bearing pointer DIRECTLY: 7

   WRAPPER                        POINTEE              STATUS IN zsyscall
   CertEnumCertificatesInStore    CertContext          LIVE -- token reaches the door
   CreateProcess                  StartupInfo          LIVE -- token reaches the door
   CreateProcessAsUser            StartupInfo          LIVE -- token reaches the door
   GetAdaptersInfo                IpAdapterInfo        LIVE -- token reaches the door
   GetIfEntry                     MibIfRow             LIVE -- token reaches the door
   WSASendTo                      WSABuf               LIVE -- token reaches the door
   getStartupInfo                 StartupInfo          LIVE -- token reaches the door
```

**Three of the seven independently reproduce rows the BOARD already carries as open** —
`getStartupInfo` (BOARD line 3225), `GetIfEntry`/`net.Interfaces`, and the `Cert*` family behind the
`crypto/x509` system verifier. That agreement is the census's corroboration; the other four
(`CreateProcess`, `CreateProcessAsUser`, `GetAdaptersInfo`, `WSASendTo`) I have not seen named
before. Note `GetAdaptersInfo` is **not** the closed `net.adapterAddresses` row — that one is
`GetAdaptersAddresses`, remedied in `core/net/windows/interface_windows_impl.cs`.

### The census's own controls, and the two times they earned their place

The controls run on **every invocation against the real corpus**, which is stronger than a
self-test: `StartupInfo` and `Timezoneinformation` must be flagged reference-bearing,
`Timeval`/`Timespec` must not, `getStartupInfo` must appear in the member list, `GetStdHandle` must
not, and the parse must cover **127 of 127** bodied signatures.

Deliberate negative control (floor #13): regress `StartupInfo`'s four pointer fields to `uintptr` in
a scratch copy → structs 33 → 32, `CONTROL FAILED: StartupInfo not flagged`, **rc=1**; real corpus
**rc=0**. Made to fail on purpose, then restored.

Two defects the controls caught before this record existed, both this session's recurring classes:

- **CRLF.** The first cut anchored field detection on `/;$/`, which cannot match a line ending
  `;\r`, and the census confidently reported **zero** reference-bearing structs. Only the
  `StartupInfo` control separated that from a real finding. Third tool today to meet this trap.
- **An invented threshold.** The denominator gate asserted `parsed > 200` — a number with nothing
  behind it — and refused a parse that was in fact complete. The population is 127 bodied
  signatures (the other 162 `static` lines are `LazyDLL`/`LazyProc` **fields**, measured not
  assumed), so the gate now compares against an independently counted total.
- **A tuple-return parse.** The first cut named a member `CertContext`, cutting the signature at the
  *return tuple*'s paren. The real function is `CertEnumCertificatesInStore`. A census that
  misnames can also miss, so the parse now anchors on the last `(...)` before `{`.

## 5. ⚠ The reason this is worth recording BEFORE the hop

At H5 the roster is re-measured wholesale. If these seven turn red then, the available reading is
*"the hop broke `syscall`"* — and they have been red since 2026-09-08, four days before the hop.
This is the misattribution hazard the golib-gen rules already name: *the cost is not the wall, it is
that the wall bills the wrong commit.* Recording the prediction now, dated and falsifiable, is what
makes the H5 reading interpretable either way:

- the seven fail at H5 → **predicted**, attributable to `3e5ead2d19`, not to the hop;
- fewer than seven fail → **this census is wrong** and its predicate needs re-deriving;
- `TestGetStartupInfo` passes at H5 → the chain in §2 has a link I misread, and §2 names every link
  by file and line so the disagreement is locatable.

## 5a. ⚠ AMENDMENT, 2026-09-13, same day — REACHABILITY, the axis §4 did not measure

§4's table column reads `LIVE -- token reaches the door`. **That wording is wrong and this block
corrects it.** What the census measured is that a wrapper is **UNDISPLACED** — no hand-own stands in
front of it — so *if it is called*, the token reaches the door. Whether anything calls it is a
**separate axis, and §4 did not measure it.** The BOARD makes exactly this distinction elsewhere in
its own words (`Process32First`/`Next` are recorded as "reached-and-working rather than latent"), and
I reproduced the class while quoting it.

Measured now, over `src/core` (production *and* converted tests), predicate controlled against
wrappers that are certainly called — `CloseHandle` 18, `WriteFile` 91, `CreateFile` 13,
`GetStdHandle` 1 — so a zero is a reading and not a blind grep:

| Wrapper | in-corpus callers | disposition |
|---|--:|---|
| `getStartupInfo` | **1** | **REACHED** — `syscall_windows_test.cs:247` → `GetStartupInfo` → `syscall_windows.cs:1555` → `Syscall` → the door |
| `CertEnumCertificatesInStore` | 0 | latent public-API surface |
| `CreateProcess` | 0 | latent — and *demonstrably* bypassed: `exec_windows.cs:376` declares its own `[LibraryImport] win32CreateProcess` over the blittable `NativeStartupInfoW`, so the corpus's own `StartProcess` never enters the zsyscall wrapper |
| `CreateProcessAsUser` | 0 | latent — same bypass, `exec_windows.cs:379` |
| `GetAdaptersInfo` | 0 | latent public-API surface |
| `GetIfEntry` | 0 | latent public-API surface |
| `WSASendTo` | 0 | latent public-API surface |

**What this changes.** §5's prediction — *"the seven fail at H5"* — **is wrong**. Only the reached one
can turn red; the other six stay exactly as silent at H5 as they are today, because nothing in the
corpus calls them. The corrected prediction is:

- **`TestGetStartupInfo` flips pass → fail at the next `syscall` run.** Unchanged, and it is the
  whole of the falsifiable claim.
- **The other six are latent, not live** — public API a user program or an out-of-corpus test can
  reach, and the reason the roster is otherwise green is simply that nothing calls them.

**What still stands, unchanged:** the eight-link chain in §2 (a statement about the wrapper, not about
its callers); the dated timeline in §3; the class membership of all seven in §4; and the
misattribution point in §5 — which shrinks from seven rows to one, and survives, because one
mis-billed row is still mis-billed.

**Why it happened, since the shape is more useful than the slip:** §4's predicate was sound and its
controls passed; the defect was that I let a *column heading* assert something the predicate never
tested, then read my own table back as though it had. An instrument's OUTPUT WORDING is part of the
instrument — the same lesson G recorded today about an instrument's input, one field over.

## 6. What is NOT claimed

**None of this has been run.** There is no Windows box in this container, the whole class is
`windows/amd64`, and every statement above is a reading of source plus a reading of the banked
proof's own date. The §2 chain is a derivation; the §4 table is a census over emitted text. Neither
is a measurement of behaviour, and the predicted panic string has never been observed. Whoever holds
a Windows box can settle §2 with one `go test -run TestGetStartupInfo` against the converted host.

The remedy is **not** designed here either. The door's panic text names its shape (a blittable
mirror), and a mirror for this exact struct already exists as `NativeStartupInfoW` in
`exec_windows.cs` — but that one is `private` to the process-launch hand-own, and whether the seven
share one mirror, or each takes its own `_impl.cs` displacement, is a sizing that should follow a
measurement rather than precede it.
