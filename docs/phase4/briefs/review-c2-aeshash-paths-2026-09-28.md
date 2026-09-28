# Review: C2's C4 aeshash (32f269a33c) and C5 source paths (ff6b555e00), 2026-09-28

> Record class: point-in-time review record. Workflow: 15 agents (finders per dimension, each surviving claim put to two refuting skeptics, and ground truth from go1.24.13 probes).
> Seats as pushed: stacked at 7ae74437c2 on claude/bold-thompson-e4gztv, each one commit on master ec23ad9f6b.
> COORD's ruling is on the ledger (2026-09-28). The findings below are the workflow's, verbatim, with paths scrubbed.

## Verdict

- **C4 AESHASH: ACCEPTED ON AMENDMENT (in-seat).** The port is exact: every size class matches aeshashbody, 276 committed rows and a 23,968-row differential give zero diffs. Owed in-seat:
  1. GODEBUG cpu.* is applied only off Windows (Go's getGodebugEarly list). Keep registerX86Options unconditional.
  2. A dispatcher-level guard arm for memhash, memhash32, memhash64 and strhash under AES, with memhash32 != memhash(.,.,4) under AES.
  3. Guard arms for the side change: an empty or non-cpu GODEBUG leaves every flag alone; cpu.aes=off clears HasAES off Windows.
  4. Stale comments corrected.
  5. internal/cpu's two linux disclosure reasons amended to name DebugOptions/cpuinit as the remaining gate.
  6. Its own commit: DebugOptions = true off Windows beside processOptions, exactly as Go's cpuinit, with a linux internal/cpu measurement. The prediction is 8 matched and both disclosures retired.
  The internal/cpu row is re-read at the candidate tip.
- **C5 SOURCE PATHS: RE-CUT (blocker).** Rooting against the ambient GOROOT brings back 4 TestStack assertion failures in the banked runtime/debug row. The existing host-identity disclosure absorbs them silently. It also mis-roots frames into any other Go install the environment names. The re-cut:
  1. Root only against a modelled link-time root, defaultGOROOT, handed to the host by the -tests pipeline through its own channel (e.g. GO2CS_DEFAULT_GOROOT, copied once by a module initializer). Never root against the ambient GOROOT.
  2. Otherwise keep the recorded (trimpath-shaped) form.
  3. Give DESIGN-position-map.md a dated amendment for the rule the seat reverses.
  4. Gate on the runtime/debug row, with TestStack's failure output again exactly its one fifth-frame line.
- **FINDING (gate class):** a test-level disclosure can absorb NEW assertion failures inside the same test while the reason names only the old one. That is a disclosure-completeness hole for the validation-bank tooling, recorded on the ledger.

## Findings (verbatim)

=== KEY aeshash-correctness
-- SURVIVING [concern] refutes=0: The GolibTests table arms guard only the pure bodies under a fixed schedule. Nothing in GolibTests guards the four AES dispatchers. In particular, the 'strhash' arm never calls strhash or strhashContent: it is a second GoAeshash-over-bytes arm.
   evidence: AesStrhashIsAeshashOverTheContent calls GoAeshash(content, seed, schedule) directly. The code that ships, memhash/memhash32/memhash64/strhash in hash_impl.cs (useAeshash, then GoAeshash*(referentBytes(...)/MemoryMarshal.Read<uint|ulong>/strhashContent, h.Value, aesSchedule)), has no guard. AlginitSelectsAesOnGosCondition checks only the flag. RuntimeHashFamilyTests still calls the public fallback bodies (GoMemhash*Pointer), so it cannot see the AES wiring. Mistakes such as strhash hashing the header instead of the content, memhash32 reading 8 bytes, or passing the wrong schedule would pass GolibTests and surface only on the runtime -tests row.
   fix: Add a dispatcher-level arm (GolibTests already reaches internals, or add a public wrapper). On an AES host, check that memhash(p,h,n) == GoAeshash(bytes,h,aeskeysched) and strhash(&s,h) == GoAeshash(s content,h,aeskeysched), and that memhash32/64 over a boxed scalar equal GoAeshash32/64. Also check that memhash32(p,h) != memhash(p,h,4) under AES, which is Go's reason for skipping TestMemHash32Equality.
-- REFUTED: The commit's negative control is real, but it is not committed, and 'fails all three table arms' holds only when the perturbed byte is in the first schedule block.
-- INFO: Every size class of GoAeshash (0, 1-15, 16, 17-32, 33-64, 65-128 and the 129+ loop) matches aeshashbody instruction for instruction. The seed mixing, the AESENC operand order, the memhash32/64 lane placement and the endianness are all correct.
-- INFO: The table arm really does come from Go, and both sides use the same fixed schedule. An independent Go 1.24.13 probe reproduces all 276 committed rows exactly (192 mem, 32 m32, 32 m64, 20 str).
-- INFO: A wider differential beyond the committed table also agrees exactly: 23,968 rows across 4 key schedules and 7 seeds, plus lengths up to 200,000, with zero diffs between Go's assembly and the C# port.
-- INFO: Tail loads: the C# never reads out of bounds. It zero-pads inputs of 1-15 bytes through a stackalloc copy, and on Go's side both the masked load and the end-of-page PSHUFB load produce exactly that zero-padded block.
-- INFO: Initialization is thread-safe and hash values are stable within a process: aeskeysched is filled exactly once, before any runtime code can run, and nothing writes it afterwards.
-- INFO: hashkey is seeded even when AES is selected; Go leaves it zero in that case. This cannot be observed from Go code, but it is a small departure from 'seeds its own key exactly as Go's does'.
-- INFO: On a 32-bit host, ensureHashKey's PlatformNotSupportedException now fires from a module initializer. That makes the whole runtime assembly unusable at load, where before only the first hash call failed.
-- INFO: Some comments are stale or overstated: the header SCOPE still lists the AES path as 'NOT here', and the claim that 'the Smhasher tests that Go enables only under AES run too' has no basis in the 1.24.13 test source.
-- INFO: Side effect in cpu_x86_impl.cs: GODEBUG cpu.* options are now honored at module load, and the doinit option table is populated, but only on hosts where X86Base.IsSupported. This changes internal/cpu's own test behaviour, and that row should be re-read. The option table itself matches doinit exactly.
=== KEY cpu-options
-- SURVIVING [concern] refutes=0: The seat applies GODEBUG=cpu.* on every OS. Go 1.24.13 applies it only on Unix-like systems; on Windows it ignores cpu.* entirely. So with a cpu.* GODEBUG set, the converted build on Windows diverges from Go: flags go off, the runtime hash, TLS cipher order and bytealg.MaxLen change, and warnings go to stderr, all where Go does nothing. The commit's claim 'as Go's Initialize(env) does' is wrong for Windows.
   evidence: Go source: runtime/proc.go getGodebugEarly returns "" unless GOOS is in {aix,darwin,ios,dragonfly,freebsd,netbsd,openbsd,illumos,solaris,linux}. cpuinit(godebug) passes that value to cpu.Initialize and sets cpu.DebugOptions=true only for the same list.
Measured on this Windows host with a Go 1.24.13 probe (linkname to internal/cpu.X86, internal/cpu.DebugOptions and runtime.useAeshash, built with -checklinkname=0): with no GODEBUG, cpu.aes=off, cpu.all=off and cpu.bogus=on it reports DebugOptions=false, useAeshash=true, HasAES=true and every flag unchanged, and prints nothing.
The converted seat probe under the same settings: cpu.aes=off gives HasAES=0; cpu.all=off zeroes all 21 flags; cpu.bogus=on prints 'GODEBUG: unknown cpu feature'.
In Go on Windows, TestMemHashGlobalSeed/noaes and TestIssue66841's re-exec child keep running aeshash. The converted children run the fallback. Both tests only check hash uniqueness or distribution, so both sides pass and no verdict moves, but the converted run follows a path Go does not. The same applies to anyone setting GODEBUG=cpu.aes=off for crypto/tls on Windows: Go keeps AES-GCM preferred, converted switches to ChaCha20.
   fix: In cpu_x86_impl.cs, keep registerX86Options() unconditional, because Go's doinit fills `options` on every OS. Wrap only the processOptions call so it runs outside Windows, mirroring getGodebugEarly (the corpus targets only Windows, Linux and Darwin):
`if (!OperatingSystem.IsWindows()) processOptions(Environment.GetEnvironmentVariable("GODEBUG") ?? "");`
Then correct the comments in cpu_x86_impl.cs and hash_impl.cs (lines ~72-74 and ~149) that say GODEBUG=cpu.aes=off selects the fallback: true on Linux and Darwin, not on Windows. The Windows /noaes and 66841 children will then run aeshash like Go's.
-- SURVIVING [concern] refutes=0: The seat invalidates the stated reason behind internal/cpu's two banked Linux disclosures, but leaves them unchanged. The row does not move, because DebugOptions is still false and the same skip signature still fires. The written reason, however, is now untrue.
   evidence: src/core/internal/cpu/go2cs_test_disclosures.json at 32f269a33c: TestDisableAllCapabilities says 'the managed runtime does not expose the x86 capability-disable options the GODEBUG path drives ... Nothing go2cs owns can satisfy the assertion without emulating cpuid feature masking inside the CLR', and TestDisableSSE3 says 'the managed runtime exposes no per-feature disable'. After the seat, the converted internal/cpu honours both GODEBUG values. Measured: cpu.all=off zeroes every flag that Options points at, and cpu.sse3=off clears HasSSE3 through the same table. The converted child assertions (TestAllCapabilitiesDisabled iterating Options; TestSSE3DebugOption checking X86.HasSSE3) would now pass. The only thing still producing the skip is `DebugOptions` (cpu.cs:16), which nothing sets because cpuinit never runs. Go sets it true on Linux and Darwin.
   fix: Take one of two routes, with a Linux measurement either way.
(a) Preferred, as a follow-up seat with its own measurement: set DebugOptions = true on the non-Windows branch together with processOptions, exactly as Go's cpuinit does. Then run internal/cpu's suite on Linux and expect 8 matched with the two disclosures retired. Windows stays 8/0 because Go skips there too.
(b) Or amend both reasons now to name DebugOptions/cpuinit as the remaining gate, so a stale 'cannot be satisfied' premise does not stay on a banked manifest.
-- INFO: By default nothing changes. With GODEBUG unset, empty, or holding only non-cpu keys, every internal/cpu flag reads bit-for-bit as it did before the seat. The only new state is internal/cpu's private `options` slice, which goes from 0 to 20 entries. No banked row (crypto/*, math/big, hash/*) can move from this side change.
-- INFO: No flag can go from false to true, so no assembly-only or stubbed fast path is newly reached. processOptions can only turn features off. Every flag that is true (the 15 mapped from System.Runtime.Intrinsics) has been true since acc79ab48 on 2026-09-02, and HasADX/ERMS/FSRM/RDTSCP/SHA stay false.
-- INFO: Census of every reader of internal/cpu's X86 flags in the converted corpus. None behaves differently by default. Under an explicit GODEBUG=cpu.*=off only three readers change, each onto a path that already ran before acc79ab48: the runtime hash choice, crypto/tls cipher-suite order, and bytealg.MaxLen.
-- INFO: The new warning path prints Go's GODEBUG diagnostics with extra spaces, because golib's builtin `print` joins its arguments with a space and gc's print does not. The defect is older than the seat, but processOptions now reaches it. On Linux, Go prints these warnings for malformed or unknown cpu.* keys, and the converted text will not match.
-- INFO: The guard does not cover the side change. Nothing asserts that an empty or non-cpu GODEBUG leaves the flags alone, or that cpu.aes=off clears HasAES. The table registerX86Options builds is a hand copy of converted doinit, so it can drift silently at the next Go hop.
-- INFO: hash/crc32's hand-owned local probes ignore GODEBUG=cpu.sse42/pclmulqdq/sse41, which Go on Linux honours. The CRC output is the same either way, so this is only a code-path difference, and its header comment is now out of date.
=== KEY source-paths
-- SURVIVING [blocker] refutes=0: The seat brings back four assertion failures in the banked runtime/debug row's TestStack. The existing host-identity disclosure absorbs them silently, so the sweep stays green, the verdict stays 'fail (disclosed)', and the proof page does not change. But the disclosure now covers failures its reason does not name.
   evidence: How Go's TestStack works (go1.24.13 src/runtime/debug/stack_test.go:73-136): when the GOROOT environment variable is set, it re-runs itself as a child with `GOROOT=` and reads that child's runtime.GOROOT(). It then requires every frame line to start with "\t"+filePrefix+file, where filePrefix is "" if the child answered empty.

Why the child answers empty under go2cs: the -tests pipeline always exports GOROOT=options.goRoot to the C# host (testConversion.go:8925-8931; main.go:216 always resolves goRoot). The converted GOROOT() returns gogetenv("GOROOT") and otherwise defaultGOROOT, which nothing in go2cs sets (runtime/windows/extern.cs:320,333-338). So the child prints "" and filePrefix is "". The BOARD already recorded this at lines 14038-14043: "the converted runtime.GOROOT() answers empty".

At master, frames 1-4 report the recorded forms and match. TestStack fails exactly one assert (BOARD 17361 and 17742: "exactly one line, TestExecution.cs:593").

At the seat, the records runtime/debug/stack.go (package_info.cs:64) and runtime/debug/stack_test.go (package_test_info.cs:56) are rooted at the parent's environment GOROOT. The line becomes "\t<profile>/sdk/go1.24.13/src/runtime/debug/stack.go:NN" but the test wants the prefix "\truntime/debug/stack.go".

Standalone net10 repro (<review-scratch>/c5-reach/cs/Program.cs), run with GOROOT set to the conversion root: "TestStack frames 1-4 prefix misses: master=0 seat=4 (filePrefix=\"\")".

Why the gate cannot see it: disclosure matc
   fix: Root against a modelled link-time GOROOT (runtime.defaultGOROOT), not runtime.GOROOT():

1. Have the -tests pipeline hand the host its conversion root through a channel that is not GOROOT itself. For example, runCommandWithTimeoutEnv could export GO2CS_DEFAULT_GOROOT, which the goenvs module initializer copies into defaultGOROOT once.
2. Have ResolveGoFile use defaultGOROOT.

With that change, the child launched with `GOROOT=` answers the same root the frames carry. TestStack returns to its single host-identity miss, TestTracebackSystem still finds its source, and a host without the variable keeps the recorded (trimpath-shaped) form.

Whatever fix lands, run the runtime/debug row at the candidate tip and confirm TestStack's failure output is again exactly the one fifth-frame line. Consider
-- SURVIVING [concern] refutes=0: When the GOROOT environment variable names a different Go installation than the corpus was converted from, standard-library frames are rooted into that other tree. The result pairs a file from one tree with a line from another: the 'position in neither tree' that the position-map ruling forbids. Go never does this.
   evidence: This is not hypothetical on this machine: the i7 has machine-scope GOROOT=C:\Program Files\Go, and that tree's VERSION file reads go1.23.1, while the corpus is 1.24.13. Anything not launched by the -tests pipeline inherits that value, including the behavioral MSTest runner, GolibTests and user apps.

Repro output with GOROOT='C:\Program Files\Go': `stdlib record ... seat=C:/Program Files/Go/src/runtime/debug/stack.go` and `vendor record ... seat=C:/Program Files/Go/src/vendor/golang.org/x/net/idna/idna10.0.0.go`, while the line numbers come from the 1.24.13 conversion. Files that are new in 1.24 or version-specific under vendor/ may not exist there at all. The corpus has 92 vendor/ records.

Go probe: Frame.File is unaffected by the environment GOROOT (see the blocker).

The new guard, AGorootRelativeRecordResolvesAgainstGoroot, computes its expected value from GOROOT() itself, so it passes for any environment value and cannot detect this. No behavioral golden prints a standard-library path, so there is no golden drift. This is a fidelity and misdirection problem in diagnostics, not a gate failure.
   fix: Same remedy as the blocker: root only against a conversion-supplied link-time root (defaultGOROOT) and never against the ambient GOROOT. If the environment must be honoured, root only when the tree's VERSION matches the converted Go version, and otherwise keep the recorded form.
-- INFO: Measured Go forms for the three cases, and what the seat returns for each: it matches Go's default form under the pipeline, and falls back to Go's -trimpath form when no GOROOT is set.
-- INFO: No user package today records a relative path containing '/', so the module-relative mis-rooting cannot happen. The discrimination is implicit, though, and the new guard itself roots a non-standard-library path to a file that does not exist.
-- INFO: With GOROOT empty or unset (for example a deployed .NET app on a machine without Go), the seat does not crash. It keeps the recorded relative form, which is consistent with a Go -trimpath binary.
-- INFO: Across banked rows that read Frame.File or runtime.Caller's file, only runtime/debug TestStack changes. No behavioral golden and no proof page prints a file path that the seat changes.
-- INFO: The seat reverses a rule in DESIGN-position-map.md at run time without a dated amendment. The premise that design rule rests on, and that two comments in code repeat, is false by measurement.
