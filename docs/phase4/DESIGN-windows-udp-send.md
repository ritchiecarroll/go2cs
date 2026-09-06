# The Windows UDP SEND wrapper — sizing `syscall.WSASendto` and its two dead siblings

**Status:** SIZING, no cut. Written to COORD's dispatch of 2026-09-05 (`de79b9852`), which asked for
the two defects named separately with their emission evidence, the remedy for each, an acceptance
stated *before* any run, a blast-radius prediction, and one question answered outright: does the
native writer as it stands today cover every shape those callers pass.

Everything below is read off the tree at master `b91684991`. Where a claim could have been taken
from another record's prose it was re-derived here instead, and the two places where that changed
the answer are marked ⚠.

---

## 1. What the reconciled table pointed at, and what is actually there

The table names one unremedied socket-send wrapper carrying two defects in one call, and a
socket-address helper displaced by all three signals whose own comment forbids what three
still-generated callers do. Both are real. The census over the corpus refines the shape in three
ways, and each refinement changes the increment's size.

**The three callers are `syscall.WSASendto`, `syscall.wsaSendtoInet4` and `syscall.wsaSendtoInet6`**
(`src/core/syscall/windows/syscall_windows.cs`, bodies at 987, 1019 and 1047 — 31, 27 and 27 lines,
zero hoisted string literals in any of them). **Only the first is LIVE.** The other two are dead in
this corpus, by two independent derivations:

- a corpus-wide grep for either name outside its own declaration and the two companion headers
  returns nothing — no call site in production or in test emission;
- Go's own callers of those two are the linkname *pull* in `internal/syscall/windows/net_windows.go`
  (`//go:linkname WSASendtoInet4 syscall.wsaSendtoInet4`), and the corpus answers that pull at the
  DECLARATION site, in `internal/syscall/windows/windows/net_windows_impl.cs`, so `syscall`'s own
  bodies are unreachable from either direction.

That companion's header asserts the same thing. This record does not rest on that sentence; both
derivations above were run here.

**`WSASendto`'s only consumer is `internal/poll.FD.WriteTo`** (`fd_windows.cs:933` and `:945`, the
zero-length and the chunked arm of one function), matching Go's own two call sites exactly.

⚠ **The two defects are four.** The table's pair are the LAYOUT defects, and they are the two a
static census can see. The call is an *overlapped submit*, and the same statement carries two more
of the lifetime kind. All four are visible in the emitted body and all four fall to one remedy, so
they are listed together in §2 rather than split by census provenance.

---

## 2. The four defects, each with its emission evidence

The kernel-facing part of the live body, from `syscall_windows.cs:987`:

```csharp
public static error /*err*/ WSASendto(ΔHandle s, ж<WSABuf> Ꮡbufs, uint32 bufcnt, ж<uint32> Ꮡsent,
                                      uint32 flags, ΔSockaddr to, ж<Overlapped> Ꮡoverlapped,
                                      ж<byte> Ꮡcroutine) {
    @unsafe.Pointer rsa = default!;
    int32 len = default!;
    if (to != default!) {
        (rsa, len, err) = to.sockaddr();
        ...
    }
    var ᴋ3 = Ꮡbufs;
    var ᴋ4 = Ꮡsent;
    var ᴋ5 = (@unsafe.Pointer)rsa;
    var ᴋ6 = Ꮡoverlapped;
    var ᴋ7 = Ꮡcroutine;
    var (r1, _, e1) = Syscall9(procWSASendTo.Addr(), 9, (uintptr)s, (uintptr)ᴋ3, (uintptr)bufcnt,
                              (uintptr)ᴋ4, (uintptr)flags, (uintptr)ᴋ5, (uintptr)len,
                              (uintptr)ᴋ6, (uintptr)ᴋ7);
```

**(1) THE BUFFER DESCRIPTOR — `(uintptr)ᴋ3`.** `WSABuf` converts to `{uint32 Len; ж<byte> Buf}`
(`types_windows.cs:555`), a MANAGED REFERENCE where native `WSABUF` wants a raw `CHAR*`. It carries
no `[GoValueClone]` and could not: that attribute records a fixed-size ARRAY field, and this is a
pointer field, which is why the census's arm (b) rather than arm (a) is what finds it. The address
of a `WSABuf` is not a native `WSABUF` in layout or in size. Same class and same remedy as
`Timezoneinformation`, `win32finddata1` and `RawSockaddrInet4`.

**(2) THE ADDRESS — `(uintptr)ᴋ5`.** `rsa` is whatever `to.sockaddr()` returned, and the displaced
helper says in its own body what that is (`syscall_windows_impl.cs:151`):

> The returned pointer keeps the Go shape and the Go meaning — the address of `sa.raw`. It is NOT a
> native image, for the layout reason in the file header, which is why every in-package caller that
> actually reaches the kernel builds one with `writeNativeSockaddr` instead of consuming this.

This body consumes it. That is the sentence the dispatch quotes, and this is the caller that
contradicts it.

**(3) THE BYTES-SENT SLOT — `(uintptr)ᴋ4`.** The caller passes `oΔ1.of(operation.Ꮡqty)`, a FIELD
reference into `internal/poll`'s `operation`, which holds managed references (`ж<FD>`, `WSABuf.Buf`,
`slice<WSABuf>`, `ж<RawSockaddrAny>`). `zsyscall_windows_wsa_impl.cs`'s own header states the
consequence: such a box's pinnable storage recurses to a container whose slot is null, so golib
cannot hold the address still, and the kernel writes `lpNumberOfBytesSent` into storage nothing owns.

**(4) THE OVERLAPPED — `(uintptr)ᴋ6`.** The same box kind, and worse: the OVERLAPPED is the
operation's kernel-side IDENTITY, named by address at submit, at `CancelIoEx` and at
`WSAGetOverlappedResult`. A managed address here is both transient and not the record the completion
machinery keys on.

⚠ **At the current head, (1), (3) and (4) are no longer wrong ADDRESSES — they are not addresses.**
COORD's re-rooting of the socket-option defect (`fe2dc351c`) establishes that golib's
pointer-to-scalar operator takes its token arm on a null pinnable storage, which is true of every
field or element reference rooted in a reference-bearing allocation. All three of these boxes are
exactly that shape, so the call hands the kernel three order tokens and one managed image:
deterministic unmapped numbers, `WSAEFAULT`, no fault. The remedy is identical either way and all
four defects predate that change; this is stated because it decides what a *pre-fix* run of the
acceptance guard in §5 will print.

---

## 3. The remedy, and why it is one increment rather than two

**Defect (2) needs no new machinery and no widening.** `WSASendto` lives in package `syscall`, and
so does the private `writeNativeSockaddr(ΔSockaddr sa, byte* buffer)` (`syscall_windows_impl.cs:179`).
A hand-own of `WSASendto` calls it directly. The public `GoWriteNativeSockaddrInet4/6` seam — which
the Linux half and the `internal/syscall/windows` half consume across an assembly boundary — is not
involved, because here there is no boundary to cross.

**Defects (1), (3) and (4) are already solved one function over, in the same file.**
`zsyscall_windows_wsa_impl.cs` carries `operationFor`, `OverlappedOp.Rearm`, `stageBuffers` and
`NativeWSABuf`, and `WSASend` (line 412) is `WSASendto` minus the address:

```csharp
OverlappedOp operation = operationFor(s, Ꮡoverlapped, wsaModeWrite);
NativeOverlapped* native  = operation.Rearm();
NativeWSABuf*    buffers  = stageBuffers(operation, Ꮡbufs, bufcnt);
uint32 sent = 0;
... Syscall9(procWSASend.Addr(), 7, s, buffers, bufcnt, &sent, flags, native, 0, 0);
if (Ꮡsent != nil) { Ꮡsent.Value = sent; }
```

`WSARecvFrom` (line 1024) is the same shape plus an address region carved from the SAME staging
block, with the ordering rule its comment states: the oversized `Staging()` request must run BEFORE
`stageBuffers`, whose smaller request would otherwise reallocate under the carved pointers.

**So the increment is `WSASend`'s body, plus `WSARecvFrom`'s carve, plus `writeNativeSockaddr` into
the carved region, plus a nine-argument `Syscall9` instead of a seven-argument one.** It belongs in
`zsyscall_windows_wsa_impl.cs` rather than `syscall_windows_impl.cs`, because three of its four
defects are the async-submit family that file owns.

Sketch, to size it rather than to be copied:

```csharp
// ONE staging block carved into two regions; the oversized request FIRST (WSARecvFrom's rule).
nuint bufBytes = (nuint)(bufcnt == 0 ? 1 : bufcnt) * (nuint)sizeof(NativeWSABuf);
byte* block = (byte*)operation.Staging(bufBytes + (nuint)nativeSockaddrLen);
NativeWSABuf* buffers = stageBuffers(operation, Ꮡbufs, bufcnt);
byte* addr = block + bufBytes;
int32 addrlen = 0;
if (to != default!) {
    (addrlen, err) = writeNativeSockaddr(to, addr);
    if (err != default!) { return err; }
}
```

Two contract details the sketch must keep, both already stated by the generated body:

- **`to == nil` is legal** and means "no address" (Go's own `if to != nil` guard). The image is
  simply not written, and the call passes zero for `lpTo` and for `iTolen`.
- **The error mapping is the generated body's, verbatim:** a `socket_error` return with a zero errno
  maps to `EINVAL`, not to success. The `internal/syscall/windows` sibling already carries that
  sentence and the reason for it.

**No completion transcription is owed.** A send writes nothing back into caller memory except
`lpNumberOfBytesSent`, which the synchronous arm copies and the overlapped arm reports through
`WSAGetOverlappedResult`, exactly as `WSASend` does. `SetOperationCompletion` — the piece that makes
`WSARecvFrom` the longer function — is not needed here at all.

### The two dead siblings

They need nothing. Displacing them would cost a registration, a placeholder and a body apiece to
change the behaviour of code with no caller, which the minimal-footprint rule refuses. They are
recorded here as MEASURED DEAD (§1, two derivations) rather than left to be rediscovered, and the
guard against them silently coming back to life is §7.

---

## 4. The question COORD asked: does the native writer cover every shape?

**Yes for the live caller, and the coverage is CLOSED rather than merely sufficient.**

`WSASendto` passes a `ΔSockaddr`, the interface. `writeNativeSockaddr` switches on the box behind it
and has arms for `ж<SockaddrInet4>`, `ж<SockaddrInet6>` and `ж<SockaddrUnix>`, defaulting to
`EAFNOSUPPORT`. The set of types that can arrive is exactly those three and cannot grow:

- Go declares `Sockaddr` with an UNEXPORTED method (`sockaddr() (unsafe.Pointer, int32, error)`,
  `syscall_windows.go:803`, commented *"lowercase; only we can define Sockaddrs"*), so no package
  outside `syscall` can implement it;
- the Windows-tagged sources define exactly three `sockaddr()` methods, on those three types;
- the converted corpus agrees independently: `syscall/windows/package_info.cs` records exactly three
  `[assembly: GoImplement<…, ΔSockaddr>(Pointer = true)]` entries.

So no widening of the writer is needed, and none would be reachable if written.

**The widening that IS needed is elsewhere, and it belongs to the acceptance rather than to the
corpus.** `operationFor` funnels into `GoAsyncIO.GetOrCreateOperationState`, a
`ConcurrentDictionary.GetOrAdd` keyed by the overlapped box, so a NIL overlapped throws rather than
taking a synchronous path. Every corpus caller passes a non-nil overlapped (`internal/poll`'s
`&o.o`), so that shape has never been issued; a guard driving `WSASendto` directly from a plain
`syscall.Socket` must pass nil, because such a socket is associated with no completion port. The
increment therefore carries a **synchronous arm** for `overlapped == nil`.

That arm may use `stackalloc` rather than operation-owned staging, and the reason is worth stating
because ⟨OQ-G⟩ ruled the opposite for the overlapped path. ⟨OQ-G⟩ turns on Winsock's silence about
when it captures `lpTo`, which matters only if the call can return before the send completes. A
synchronous send cannot, so the established rule applies unchanged: *"a LOCAL at the call site,
trivially stable for exactly that long"* (`syscall_windows_impl.cs`). `TransmitFile` already carries
a nil-overlapped guard in this file, so the shape is not novel here.

---

## 5. Acceptance, stated before any run

**No banked row and no behavioral guard reaches `syscall.WSASendto` on Windows today, and Go's own
suite structurally cannot.** This is the finding that decides the increment's shape, and it is
measured rather than assumed:

- `internal/poll.FD.WriteTo` is reached only from `net`'s generic `netFD.writeTo`, whose callers are
  `IPConn.WriteTo*` (`iprawsock_posix.go:98`) and `UnixConn.WriteTo*` (`unixsock_posix.go:140`);
- `UDPConn` never reaches it. Go 1.23's `UDPConn.writeTo` switches on `fd.family` and calls
  `writeToInet4` / `writeToInet6`, which are the hand-owned pair. ⚠ This corrects a reading made
  while sizing: `UdpLoopbackRoundTrip` uses only `WriteTo(b, Addr)` and therefore *looks* like a
  consumer of the generic path. It is not one. The companion header saying that guard died on
  `WSASendtoInet4` is right, and the inference from the guard's own source was wrong;
- `net`'s suite skips both remaining consumers on Windows through `testableNetwork`
  (`platform_test.go:36`): `unix` and `unixgram` return false for `windows` outright, and `ip`,
  `ip4`, `ip6` require `os.Getuid() == 0`, which Windows never satisfies.

So the acceptance must be a new guard, and the dispatch's constraint — that it cannot be "the guard
compiles" — is satisfiable without privilege and without an unsupported address family.

**The proposal is `WsaSendtoRoundTrip`, a Windows-exclusive behavioral guard in
`SockaddrRoundTrip`'s shape** (`[GoPlatformExclusive("windows")]` plus
`[GoTestMatchingConsoleOutput]`; eight such guards exist, and that one already drives raw `syscall`
socket calls and prints host-invariant values).

- **Receiver:** an ordinary `net.ListenPacket("udp4", "127.0.0.1:0")`. Its read path is the
  hand-owned `WSARecvFrom`, already proven by `UdpLoopbackRoundTrip`, so the receiver is not what is
  under test.
- **Sender:** a raw `syscall.Socket(AF_INET, SOCK_DGRAM, 0)`, bound with `syscall.Bind`, its own
  address read back with `syscall.Getsockname` — all three hand-owned already and proven.
- **The call under test:** `syscall.WSASendto(s, &buf, 1, &sent, 0, &SockaddrInet4{…}, nil, nil)`
  with the payload in a `syscall.WSABuf`.
- **What is asserted, none of it merely the absence of a fault:** the returned error is nil; `sent`
  equals the payload length (defect 3 — the kernel's writeback landed where the caller reads); the
  receiver's `ReadFrom` returns those exact bytes (defects 1 and 4 — the descriptor and the submit);
  and the peer address it reports equals the raw socket's own `Getsockname` answer field for field
  (defect 2 — the address image). Ports appear only as a relationship between two of them, never as
  numbers, so the output is host-invariant.

**Red control:** the guard must FAIL on master before the fix. Per §2's ⚠, the expected pre-fix
signature at the current head is a `WSAEFAULT` return with `sent == 0` rather than a crash, which is
why the guard asserts the nil error and the byte count instead of relying on a fault. That is the
same reasoning `SockaddrRoundTrip`'s own header gives for its shape.

`syscall.Recvfrom` is NOT available as the receive side: on Windows it is a stub returning
`EWINDOWS` (`syscall_windows.cs:1186`), which is Go's own behaviour and not a conversion gap.

---

## 6. Blast radius — the two-seeded diff prediction

Registration: one entry, `"WSASendto": goosWindows`, beside `Bind`, `Connect`, `Getsockname` and
`Getpeername` in `manualConversionFuncs["syscall"]` — the same key shape (a bare package-level name)
and the same GOOS scoping those four use.

Predicted footprint, as a path set and line kinds:

| target | files | prediction |
|---|---|---|
| windows | 2 | `syscall/windows/syscall_windows.cs` −31 / +1 (the body at 987–1017 collapses to one placeholder line); `syscall/windows/package_info.cs` +1 / −1, the `syscall_windows.cs` `GoPositionMap` line re-encoded |
| linux | 0 | the entry is `goosWindows` |
| darwin | 0 | the entry is `goosWindows` |

The map line is re-encoded rather than retired because `syscall_windows.cs` keeps plenty of other
mapped content; this is a removal, so its map line's ABSENCE would be the falsifier, not its
presence. No `[assembly: GoImplement…]` record is at risk: the displaced body performs no
concrete-to-interface conversion — it consumes a `ΔSockaddr`, it does not produce one — which is
what made `RawSockaddrAny.Sockaddr` a different case at L10. No hoisted string literals exist in the
body, so nothing dangles. The guard project adds its own four `Check…` lines and one golden.

**Falsifier:** any hunk outside those two paths, any `GoPositionMap` line for a file other than
`syscall_windows.cs`, or any import-hook line in the delta.

---

## 7. What this increment does NOT do, and the guard that keeps it honest

The two dead siblings stay generated and stay dead. That is a claim about today's corpus, and what
would silently falsify it is a converter change emitting a forwarding property for the linkname
pull — at which point `wsaSendtoInet4` and `wsaSendtoInet6` become live with all four defects intact
and nothing pointing at them. The cheap guard is a census assertion in the converter's own
`go test ./...` tier, in the seam-ledger shape this repo already uses: those two names have no call
site in the emitted corpus. Its whole value is the day it goes red.

---

## 8. Size

**One increment.** One registration, one hand-owned body of roughly `WSASend`'s length plus
`WSARecvFrom`'s carve, one synchronous arm, one behavioral guard, one census assertion. It needs no
new golib primitive, no new public seam, no widening of the native writer, and no change to
`internal/poll` or to `net`.

The reason it is not two is §3: three of the four defects fall to machinery that already exists in
the file the body will live in, and the fourth to a private function in the same package. The part
that is genuinely new is the nil-overlapped synchronous arm, and it exists only because the
acceptance guard needs a shape the corpus never issues — a fair price for having an acceptance at
all, given §5.

---

## 9. MEASURED at the cut (2026-09-06) — the sizing scored against what the run said

Appended, not rewritten: §6's prediction stands above exactly as it was posted, and this block scores
it. Everything here is a reading, and the two places the sizing was WRONG are marked ✗.

### The two-seeded three-target A/B

Both roots seeded from ONE frozen snapshot (`git archive` at `bb020ef35`) before either arm ran;
each arm a full `-stdlib -comments` emission over `windows/amd64,linux/amd64,darwin/amd64`; the base
arm on a converter built from the same commit with only the registration absent. Base wall 1,217 s,
cut wall 986 s. Write-evidence was a CONTENT test, not a timestamp: a `[module: GoManualConversion]`
file inside the package this arc touches (`syscall/windows/syscall_windows_impl.cs`) hashes equal
across both seeds and is unchanged by either emission.

**The footprint is TWO files, both under `syscall/windows`, zero on linux and zero on darwin — the
path set exactly as predicted.**

| target | files |
|---|---|
| windows | `syscall/windows/syscall_windows.cs`, `syscall/windows/package_info.cs` |
| linux | none |
| darwin | none |

### ✗ The line counts were wrong, and the reason is a mechanism the sizing did not model

§6 predicted `syscall_windows.cs` at **−31 / +1**. Measured: **−46 / +16** (`git diff --numstat`;
a `grep -cE '^[-+][^-+]'` reads 44/15 because it drops removed BLANK lines, which is why the numstat
is the figure of record). The body-to-placeholder part is exactly what was predicted. The rest is a
**RENUMBERING**: the converter's `ᴋ` temporaries are numbered sequentially per FILE, so removing
`WSASendto`'s five (`ᴋ3`–`ᴋ7`) shifts every later one in the file — `wsaSendtoInet4`'s `ᴋ8`–`ᴋ12`
become `ᴋ3`–`ᴋ7`, `connectEx`'s `ᴋ18`–`ᴋ21` become `ᴋ13`–`ᴋ16`, and so on to the end. Classified:
9 of the 15 added and 20 of the 44 removed lines are pure `ᴋ` renumbering.

**The rule that falls out: a displacement's footprint in a file that carries `ᴋ` temporaries is not
bounded by the displaced body.** It reaches every later temporary in that file, and a sizing that
predicts only the body will under-predict by that tail.

### ✗ The position-map line MOVES but is NOT APPLIED, and the sizing should have said so

§6 predicted `package_info.cs` at +1/−1 with the `syscall_windows.cs` `GoPositionMap` line
re-encoded. The emission delta is exactly that — one line, one kind, as predicted. **It was not
applied, and applying it would have been wrong.** The 3-way merge CONFLICTED, which is the
instrument refusing correctly, and the reason is the standing rule: an unbanked relocation sits
between the committed tree and a fresh emission, so the emission's value describes NEITHER. Measured
here: the committed map line already differs from the BASE emission's, and the committed
`syscall_windows.cs` is 1,559 lines against the base emission's 1,565 and the cut's 1,535. The
deliberate regen levels the map and the relocation together; a converter train does not.

**So the APPLIED footprint is ONE file.** The sizing said two, and the second is a line the ruling
forbids a train to touch.

### The application's proof pair

- **applied delta == emission delta**, per file: +15/−44 on both sides under one CR-normalised
  instrument, `git diff --numstat` reading 16/46 for the same change.
- **the residual against the emission is the IDENTICAL SET before and after**: 28 content lines,
  compared as sorted sets, byte-equal. So the application carried nothing the change does not own.
- **line KINDS**: 0 `GoPositionMap` lines and 0 import-hook lines in the applied delta.

That residual is standing corpus drift and is named rather than absorbed: four forced-init hooks
(`initᴛᴛimportꓸerrors`, `…internalꓸoserror`, `…runtime`, `…sync`) that the committed file carries and
a fresh `-stdlib` emission does not, plus two `case {}` parenthesisation lines from a converter
change that landed without its regen.

### Measured in passing, and not this cut's to fix

A fresh three-target emission classifies SEVEN packages' per-GOOS trios as SHARED and writes them
flat: `crypto/rand/util.cs`, `internal/poll/fd_poll_runtime.cs`, `net/dnsclient.cs`, `os/exec.cs`,
`os/user/user.cs`, `path/filepath/symlink.cs`, `time/zoneinfo_read.cs`. The arithmetic closes
exactly — 3,728 seeded − 21 per-GOOS removed + 7 flat added = 3,714 emitted — it is identical in
both arms so it cancels in the diff, and it belongs to a deliberate regen.

### The acceptance, both directions

`WsaSendtoRoundTrip` passes all four phases (Transpile, Compile, Target, Output), the Output phase
being a byte comparison of the converted program's stdout against `go run`: ten lines, all `true`.

**The red control fires where it must and nowhere else.** Neutering the mechanism — the synchronous
arm patched to hand the kernel the generated body's own arguments, the managed `WSABuf` by address
and the managed image `sockaddr()` returns — leaves Transpile, Compile and Target GREEN and turns
**Output** red with `stdout mismatch C# vs Go`. The converted side under that control prints:

```
roundtrip: send reported no error: false
roundtrip: byte count equals payload: false
roundtrip: readfrom failed
zerolen: send reported no error: false
...
```

exit code 0, no fault — **the refused-call signature §5 predicted, and the reason a fault-asserting
control would have asserted something false.** The restore is byte-identical to the pre-control file
by hash.

One durability note on that control: it was measured on a tree that does not yet carry the
pointer-token storage repair. Once that lands, three of the four arguments go back to being wrong
ADDRESSES rather than unmapped tokens, and the pre-fix failure mode may change shape. The guard
asserts VALUES — a byte count, arriving bytes, an address compared field for field — so it goes red
either way, which is the property that made value-assertions the right choice rather than a stylistic
one.

### Gates at the cut

Converter suite `go test ./...` ok, 196 s, at the tip — including
`TestManualConversionRegistrationsDisplaceSomething`, which was RED between the registration and the
footprint landing and is the guard that makes "registration and footprint are ONE commit" mechanical.
`check-solution-integrity.ps1`: 0 cycles on windows, linux and darwin; 721 behavioral projects
registered; path casing clean. The four `*Tests.cs` classes moved 3/3/3/3, the symmetric shape one
new project makes.
