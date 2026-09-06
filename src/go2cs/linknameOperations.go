// linknameOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"strings"

	"golang.org/x/tools/go/packages"
)

// linknameHandles is the set of THIS package's symbol names that carry a definition-side
// one-argument `//go:linkname <name>` directive — Go 1.23's opt-in that AUTHORIZES other packages to
// linkname-PULL <name> (runtime/linkname.go lists overflowError, divideError, write, doInit, … each
// "used in" a named consumer). The authorization is puller-AGNOSTIC (it opens the symbol to linkname
// generally, it does not name a specific puller), so the faithful C# emission is `public` — a
// puller in a SEPARATE assembly can then reach the symbol through its forwarding property. Reset per
// package alongside projectImports; populated by collectLinknameHandles.
var linknameHandles HashSet[string]

// conversionGraph is the CONVERT-SET dependency graph, built by the -stdlib and -recurse drivers and
// nil for a single-package or -tests conversion. Set once the graph is built; read by
// linknamePullWouldCycle. currentPackagePath is the import path of the package currently being
// converted (set per package in resetPackageState).
//
// ⚠ This variable used to be documented as nil "where no cross-package cycle can arise from the one
// package under conversion", and linknamePullWouldCycle acted on that: no graph, no cycle. The
// assumption is false, for exactly one emission. Every OTHER cross-package reference the converter
// emits descends from an `import`, and Go's import graph is acyclic by construction — but a linkname
// edge is the one reference the converter emits that Go's own graph does NOT contain, which is the
// entire reason linknamePullWouldCycle was written. The shortcut assumed the danger came from the
// convert-SET; it comes from the DIRECTIVE, and a directive is visible to a single-package conversion
// exactly as it is to -stdlib.
//
// What it cost (W1, docs/phase4/DESIGN-linkname-push-cycles.md): one variable, two answers, no
// diagnostic. Converting `runtime` under -stdlib, the graph answered DependsOn(internal/syscall/
// windows, runtime) = true and runtime's `//go:linkname canUseLongPaths internal/syscall/windows.
// CanUseLongPaths` kept its plain-field form. Converting the SAME package under -tests, the nil
// shortcut answered false, the forwarding property was emitted, and internal/syscall/windows was
// queued for a ProjectReference — which Go's own internal/syscall/windows -> syscall -> runtime
// closes into a cycle no conversion ORDER can undo, MSB4006, six cycles, runtime and everything in
// them dead. The corpus-wide assertion that would have caught it is now standing in
// check-solution-integrity.ps1.
var conversionGraph *DependencyGraph
var currentPackagePath string

// linknameCycleAnswer is one memoized "what does this target import?" result. `resolved` is false
// when the question could not be ANSWERED (the target would not load, or loaded with errors that
// leave its import list incomplete) — distinct from an answer of "imports nothing", and the
// difference is the whole point: see the fail-closed arm in linknamePullWouldCycle.
type linknameCycleAnswer struct {
	closure  HashSet[string]
	resolved bool
}

// linknameTargetClosures memoizes loadLinknameTargetImportClosure per pull target. Keyed by target
// path AND target platform: the closure of `internal/syscall/windows` is a property of the platform
// being converted for, and a -platforms run walks several in one process. Not reset per package —
// the answer does not depend on which package is asking, only the membership test does.
var linknameTargetClosures = map[string]linknameCycleAnswer{}

// linknamePullWouldCycle reports whether forwarding a var pull to targetPath would create a CYCLIC C#
// project reference — targetPath is (or transitively depends on) the current package. Go's linkname is
// link-time and cycle-free; a C# project reference is compile-time and cannot be circular. A downward
// pull (math/bits → runtime) is safe; the reverse (runtime → internal/syscall/windows.CanUseLongPaths)
// is not, and keeps its plain-field form.
//
// Three ways to answer it, in cost order:
//
//  1. The convert-set graph, when a batch driver built one. Unchanged.
//  2. The CURRENT package's own transitive import closure. If this package already reaches the
//     target, the target cannot reach back — Go's import graph is acyclic, which is the same fact
//     the whole design rests on — so the pull is downward and safe. Free: resetPackageState already
//     captured the closure, and it covers every pull whose target the puller also imports.
//  3. Otherwise a targeted go/packages load of the TARGET, walking its transitive imports for the
//     current package (M1). Memoized per target, so a run pays one load per distinct pull target —
//     three in the whole standard library.
//
// If (3) cannot answer — the load fails, or the target loads with errors that leave its imports
// incomplete — the pull is REFUSED (M2, as M1's fallback). An unanswerable cycle question must not
// be answered "no": answering "no" emits a reference that may not compile at all, while answering
// "yes" emits the plain field this converter emitted for every such pull before the feature existed.
// The refusal is announced, because a silent one is indistinguishable from a correct suppression.
func linknamePullWouldCycle(targetPath string, options Options) bool {
	if targetPath == currentPackagePath {
		return true
	}

	if conversionGraph != nil {
		return conversionGraph.DependsOn(targetPath, currentPackagePath)
	}

	if _, reachedFromHere := importedPackages[targetPath]; reachedFromHere {
		return false
	}

	closure, resolved := linknameTargetImportClosure(targetPath, options)

	if !resolved {
		return true
	}

	return closure.Contains(currentPackagePath)
}

// linknameTargetImportClosure returns the pull target's transitive import closure, loading it at most
// once per target per platform.
func linknameTargetImportClosure(targetPath string, options Options) (HashSet[string], bool) {
	key := targetPath + "|" + options.targetPlatform

	if cached, found := linknameTargetClosures[key]; found {
		return cached.closure, cached.resolved
	}

	answer := loadLinknameTargetImportClosure(targetPath, options)
	linknameTargetClosures[key] = answer

	if !answer.resolved {
		// Warn ONCE per target (the memo write above is what makes it once). A pull that keeps its
		// plain-field form because the cycle question could not be answered looks exactly like a pull
		// correctly suppressed for a real cycle, and the difference matters: the first is a degraded
		// emission worth investigating, the second is the converter working.
		showWarning("could not resolve the import closure of linkname pull target %q for %s; the pull keeps its plain-field form rather than risk a cyclic project reference", targetPath, options.targetPlatform)
	}

	return answer.closure, answer.resolved
}

// loadLinknameTargetImportClosure loads the pull TARGET by import path and walks its transitive
// imports. Metadata only (no syntax, no types), from the package under conversion's own directory —
// the target has to be importable from there for the emitted reference to mean anything — carrying
// the run's build tags and target platform so the closure is the one this conversion will emit
// against. A GOOS-specific package has a GOOS-specific closure, which is exactly the case W1 lives in.
//
// The sibling test closure (collectSiblingTestClosure) is deliberately NOT reused: it records import
// PATHS, not edges, so it can say the target is in the test closure but never what the target itself
// imports — which is the only question here.
func loadLinknameTargetImportClosure(targetPath string, options Options) linknameCycleAnswer {
	targetParts := strings.Split(options.targetPlatform, "/")

	if len(targetParts) != 2 || packageSourceDir == "" {
		return linknameCycleAnswer{}
	}

	loaded, err := packages.Load(&packages.Config{
		Mode:       packages.NeedName | packages.NeedImports | packages.NeedDeps,
		Dir:        packageSourceDir,
		BuildFlags: options.loaderBuildFlags(),
		Env: append(os.Environ(),
			fmt.Sprintf("GOOS=%s", targetParts[0]),
			fmt.Sprintf("GOARCH=%s", targetParts[1])),
	}, targetPath)

	if err != nil {
		return linknameCycleAnswer{}
	}

	for _, pkg := range loaded {
		if pkg.PkgPath != targetPath {
			continue
		}

		// A package that loaded WITH errors may have dropped imports, and a closure missing one edge
		// is precisely how a cycle stays invisible. Unanswerable, not empty.
		if len(pkg.Errors) > 0 {
			return linknameCycleAnswer{}
		}

		closure := HashSet[string]{}

		var walk func(pkg *packages.Package)

		walk = func(pkg *packages.Package) {
			for path, imported := range pkg.Imports {
				if !closure.Add(path) {
					continue
				}

				walk(imported)
			}
		}

		walk(pkg)

		return linknameCycleAnswer{closure: closure, resolved: true}
	}

	return linknameCycleAnswer{}
}

// collectLinknameHandles scans every file's comments for a definition-side one-argument
// `//go:linkname <name>` handle and records <name> in linknameHandles. A TWO-argument form
// (`//go:linkname local pkgpath.remote`) is a PULL, not a handle, and is skipped — the pull is
// emitted as a forwarding property on the LOCAL side (see varLinknamePull). Mirrors the
// collectPublicizedTypes analysis pass: a package-wide pre-pass whose result the per-file emission
// consults (a handle in runtime/linkname.go must widen a var defined in runtime/panic.go).
func collectLinknameHandles(files []*ast.File) {
	for _, file := range files {
		for _, group := range file.Comments {
			for _, comment := range group.List {
				fields := strings.Fields(comment.Text)

				// One-arg handle: exactly the directive + the authorized symbol name.
				if len(fields) == 2 && fields[0] == "//go:linkname" {
					linknameHandles.Add(fields[1])
				}
			}
		}
	}
}

// packageVarAccess returns the C# access modifier for a package-level var. It is normally the Go
// name's exported-ness (getAccess), but a var carrying a definition-side one-arg //go:linkname handle
// is emitted `public`: Go 1.23 has deliberately opened it to cross-package linkname pulls, so a
// puller in another assembly must be able to reach it — a lowercase-name `internal` would hide it.
//
// A handle var is publicized ONLY when its TYPE is itself publicly accessible: a public member cannot
// expose a less-accessible type (CS0052/CS0053), and runtime's handle list includes deep-internal
// state whose type is unexported (`sched` of `schedt`, `writeBarrier` of an anonymous struct,
// `lastmoduledatap` of `*moduledata`). Such a var could not be linkname-pulled across a C# assembly
// boundary anyway — a foreign package cannot name the internal type — so it keeps its `internal` form.
func packageVarAccess(goIDName string, varType types.Type) string {
	if linknameHandles.Contains(goIDName) && typeIsPubliclyAccessible(varType) {
		return "public"
	}

	// The INVERTED-ALIAS direction's mirror image, and the exact shape packageFuncAccess already
	// carries for linknamePushSources: this var is the STORAGE side of a var alias whose forwarding
	// property lives in another package, so that property reads it across an assembly boundary and an
	// unexported Go name would otherwise be `internal` there. Its own package never opened it with a
	// one-arg handle — runtime's canUseLongPaths carries the two-ARG pull, and the handle authorizing
	// the alias sits on the OTHER side — so this arm reads the alias registry alone, exactly as the
	// push arm reads the push registry alone.
	//
	// The type gate is the handle arm's, for the handle arm's reason (CS0052/CS0053: a public member
	// cannot expose a less-accessible type). A row whose storage type fails it is a broken row rather
	// than a silent degradation — the forwarder on the other side then cannot see the member and says
	// so at compile time — and TestLinknameVarAliasPublicizesTheStorageSide is what keeps the live row
	// from becoming one.
	if linknameVarAliasStorage[currentPackagePath+"."+goIDName] && typeIsPubliclyAccessible(varType) {
		return "public"
	}

	return getAccess(goIDName)
}

// packageFuncAccess returns the C# access modifier for a package-level FUNC. It is normally the Go
// name's exported-ness (getAccess), with ONE exception: a func that this converter FORWARDS a
// cross-package linkname pull to (linknameForwardTargets) is emitted `public`, because the puller
// compiles into a different assembly and an unexported Go name would otherwise be `internal` there.
//
// This is deliberately far narrower than the var rule above, which publicizes on the one-arg
// `//go:linkname` HANDLE alone. Go 1.23 carries 340 such handles outside cmd/ — publicizing every
// one would widen the corpus's whole surface for pulls that are never emitted. The forward-target
// list is the converter's own record of which pulls actually become a call, so gating on it moves
// exactly the symbols that need to move. The handle is still required: it is Go's authorization for
// the pull, and forwarding to a symbol its own package never opened would not be faithful.
func packageFuncAccess(goIDName string, isFreeFunction bool) string {
	if isFreeFunction && linknameHandles.Contains(goIDName) && linknameForwardTargets[currentPackagePath+"."+goIDName] {
		return "public"
	}

	// The PUSH direction's mirror image: the pushing DEFINITION (runtime's
	// unique_runtime_registerUniqueMapCleanup) is what the consumer's forwarder calls across the
	// assembly boundary, so it must be public for the same reason. Its own package never opened it
	// with a handle — a push carries its authorization on the CONSUMER's side — so this arm reads
	// the push registry alone. See linknamePushTargets.
	if isFreeFunction && linknamePushSources[currentPackagePath+"."+goIDName] {
		return "public"
	}

	return getAccess(goIDName)
}

// linknamePush is the disposition of a `//go:linkname` PUSH — the direction where the DEFINING
// package carries the body and names ANOTHER package's bodyless declaration as the symbol it
// defines (`//go:linkname unique_runtime_registerUniqueMapCleanup unique.runtime_registerUniqueMapCleanup`
// in runtime/mgc.go). The consuming side is an ordinary bodyless func under a one-arg handle, so
// without this the converter emitted a throwing PartialStubGenerator stub and the pushed body was
// unreachable.
//
// `source` always names the pushing definition ("<pkgPath>.<funcName>"). An empty `reason` means the
// pushed body is honorable and the consumer's declaration becomes a FORWARDER to it. A non-empty
// `reason` says why the pushed body CANNOT be honored in the managed model, and the declaration
// becomes a stub that panics naming both halves of the pair — never a fabricated body that would
// look truthful (the inverse of the atomic rule: a contract that cannot be honored must announce
// itself, not answer plausibly).
type linknamePush struct {
	source string
	reason string

	// bareDecl records that the CONSUMER declares the symbol with NO `//go:linkname` directive of its
	// own — a plain bodyless func whose only directive is the two-arg one on the PUSHING side
	// (`func runtime_envs() []string // in package runtime` in syscall/env_unix.go, pushed by
	// runtime/runtime.go). That is the standard library's older push shape, every bit as legal as the
	// one-arg-handle form `unique` and `internal/weak` use, and the corpus contains both — so the
	// matcher has to accept both.
	//
	// It is recorded per entry rather than inferred, so the match still FAILS CLOSED in both
	// directions: a handle entry will not forward a declaration that carries no handle, and a bare
	// entry will not forward one that does. Neither shape is verifiable from the consumer's own
	// syntax — the pushing package's directive is invisible while converting the consumer, which is
	// the whole reason this registry is curated — so the consumer's shape is part of the same
	// recorded judgment as the disposition.
	bareDecl bool

	// selfSymbolPull records the THIRD consumer shape, and it is a NARROW EXTENSION for one measured
	// destination rather than general machinery: a TWO-arg `//go:linkname <thisFunc> <ownPkg>.<symbol>`
	// naming a symbol in the CONSUMER'S OWN package that no file in that package defines, so the
	// declaration is a local pull of a name another package pushes in. Go writes it exactly once in the
	// pinned toolchain outside cmd/ where the declaration is bodyless and the target is its own package:
	// runtime/pprof.pprof_cyclesPerSecond, pushed by runtime/cpuprof.go. ONE member today.
	//
	// Without it the matcher refuses this shape by design -- and rightly, for the other two arms: a
	// two-arg directive is normally a PULL, a different mechanism entirely, and admitting it blindly
	// would let a mis-keyed row forward an unrelated declaration. What makes THIS shape safe to admit
	// is that the target names the consumer's own package: a pull naming ANOTHER package stays refused,
	// so the widening cannot reach the mechanism it is distinguished from.
	selfSymbolPull bool
}

// linknamePushTargets is the registry of `//go:linkname` PUSH destinations the converter resolves,
// keyed "<consumerPkgPath>.<symbol>" — the consumer's own fully-qualified declaration.
//
// Curated, for the same reason linknameForwardTargets is: a bodyless func under a one-arg handle is
// INDISTINGUISHABLE at conversion time from an ordinary assembly stub, and the converter never sees
// the pushing package's comments while converting the consumer (a package is converted from its own
// syntax; dependencies contribute types, not directives). Go 1.23 carries ~200 pushes outside cmd/,
// of which the converted corpus exposes ELEVEN as bodyless one-arg-handle declarations — and most of
// those (time's timer trio, internal/syscall/windows's stdcall wrappers, internal/coverage/cfile's
// linker-section walk) push a body the managed model cannot run, several already answered by a
// hand-written companion that a converter-emitted body would collide with. Linking them wholesale
// would be a regression dressed as a feature, so each entry is a judgment recorded here.
var linknamePushTargets = map[string]linknamePush{
	// unique's cleanup registration, pushed by runtime/mgc.go. The pushed body is ORDINARY converted
	// Go — it makes a `chan struct{}` and starts a goroutine that drains it and calls the callback —
	// so the managed model runs the real thing: the registration succeeds and the cleanup goroutine
	// parks on the channel. Nothing signals it, because the converted runtime's clearpools() is
	// driven by Go's own GC, which does not run; that is Go's OWN behavior for a program whose GC
	// never fires (unique's map simply keeps its entries), not a fabricated answer. Before this the
	// stub threw out of `unique.Make`'s setupMake.Do, taking net/netip and gob's TestNetIP with it.
	"unique.runtime_registerUniqueMapCleanup": {source: "runtime.unique_runtime_registerUniqueMapCleanup"},
	// runtime/pprof's cycles-per-second -- the ONE member of the self-symbol shape (see selfSymbolPull).
	// runtime/cpuprof.go carries the body (`return ticksPerSecond()`) under the two-arg directive
	// `//go:linkname pprof_cyclesPerSecond runtime/pprof.runtime_cyclesPerSecond`, and
	// runtime_cyclesPerSecond exists nowhere in runtime/pprof: the consumer pulls a name its own package
	// never defines. The forwarder is on the CONSUMER side, across the runtime/pprof -> runtime edge that
	// already exists, so the graph cost is zero -- measured, along with the 38/36/36 cycles the literal
	// reading of Go's push direction would have cost.
	//
	// MEASURED PAYOFF, on a restored scratch probe before this was cut: net/http/pprof goes from
	// host-fatal at 0 of 15 with 15 empty verdicts, to failing at 11 of 15 with ZERO empty and a host
	// that survives -- seven /debug/pprof subtests recovered from infrastructure-error. It does NOT bank
	// the row: the residual is the execution tracer (the same capability runtime/trace refuses by name),
	// CPU profile collection, one skip and the parent shadow of those two.
	"runtime/pprof.pprof_cyclesPerSecond": {source: "runtime.pprof_cyclesPerSecond", selfSymbolPull: true},
	// syscall's environment snapshot, pushed by runtime/runtime.go. The pushed body is ordinary
	// converted Go — `append([]string{}, envs...)` — and `runtime.envs` is genuinely populated in the
	// managed model by the hand-owned runtime/goenvs_impl.cs module initializer, so the forwarder
	// hands back the real process environment rather than a plausible-looking empty one.
	//
	// This is the BARE consumer shape: syscall/env_unix.go declares `func runtime_envs() []string //
	// in package runtime` with no directive of its own. Until this row existed the declaration took a
	// throwing PartialStubGenerator stub, and because `envs` is a package-level var INITIALIZED from
	// it, the throw came out of syscall's type initializer — taking os.init() and therefore every
	// Linux program that so much as touches fmt down with it. The Windows corpus never surfaced it
	// because env_unix.go is `//go:build unix || (js && wasm) || plan9 || wasip1`, so the declaration
	// does not exist there at all.
	"syscall.runtime_envs": {source: "runtime.syscall_runtime_envs", bareDecl: true},
	// os's command-line snapshot, pushed by runtime/runtime.go — the exact sibling of the envs row
	// above, in the same BARE consumer shape: os/proc.go declares `func runtime_args() []string //
	// in package runtime` with no directive of its own, and runtime pushes it with
	// `//go:linkname os_runtime_args os.runtime_args`.
	//
	// The pushed body is ordinary converted Go (`append([]string{}, argslice...)`), and — this is
	// the part that had to be TRUE before the row could be honorable — `runtime.argslice` is
	// genuinely populated in the managed model by the hand-owned runtime/goargs_impl.cs module
	// initializer, which fills it from Environment.GetCommandLineArgs(). Without that companion the
	// forwarder would have returned an EMPTY os.Args: not an error, just a plausible-looking wrong
	// answer, which is the failure mode this project rules against. Forwarding and populating are
	// therefore one change, not two.
	//
	// Windows is unaffected in both halves: os.init() returns early there (Args comes from
	// exec_windows.go) and goargs_impl.cs keeps goargs()'s own `if GOOS == "windows" { return }`
	// guard, so argslice stays unset exactly as in Go.
	"os.runtime_args": {source: "runtime.os_runtime_args", bareDecl: true},
	// syscall's reader-starvation probe for the ForkLock upgrade dance, pushed by sync/rwmutex.go
	// (`//go:linkname syscall_hasWaitingReaders syscall.hasWaitingReaders`). The consumer is the
	// BARE shape — forkpipe2.go declares `func hasWaitingReaders(rw *sync.RWMutex) bool` with no
	// directive of its own, "defined in the sync package" — and the pushed body is ORDINARY
	// CONVERTED Go over RWMutex's own fields, so the forwarder calls something that genuinely
	// works. No new project reference: syscall already imports sync for ForkLock itself.
	//
	// What the stub was costing on Linux (the first flavor whose exec seam makes acquireForkLock's
	// slow path reachable): os/exec's TestPipes drove ForkLock contention into the probe and died
	// on the announcing stub — the last named residual of the exec-wall arc's own row.
	"syscall.hasWaitingReaders": {source: "sync.syscall_hasWaitingReaders", bareDecl: true},
	// net's hidden *os.File constructor for a dup'd socket descriptor, pushed by os/file_unix.go
	// (`//go:linkname net_newUnixFile net.newUnixFile`). BARE consumer shape: net/fd_unix.go declares
	// `func newUnixFile(fd int, name string) *os.File` under the prose comment "Defined in os
	// package", with no directive of its own — the syscall.runtime_envs shape.
	//
	// The pushed body is three lines of ORDINARY CONVERTED Go — a negative-fd panic and
	// `newFile(fd, name, kindSock, true)` — and the precondition the os.runtime_args and
	// GetSystemDirectory rows insist on is already TRUE here rather than needing a companion:
	// os.newFile is the same function os.NewFile and os.Pipe reach, it is exercised on every Linux
	// row that opens anything, and the kindSock path differs from the kindPipe path only in skipping
	// the SetNonblock call (the descriptor is already non-blocking) while still registering with the
	// poller. So the forwarder calls something that genuinely works, and nothing else had to move.
	//
	// os.NewFile is NOT a faithful substitute for the forward, which is why this is a registry row
	// and not a hand-patch in net: `kind` is exactly the distinction Go's own comment on
	// net_newUnixFile exists to preserve. kindSock sets `f.nonblock = true` so a later `Fd()` hands
	// back a BLOCKING descriptor — the historical behavior net.conn.File callers depend on — whereas
	// kindNewFile on an already-non-blocking descriptor leaves it non-blocking. A substitution would
	// compile, run, and quietly change the descriptor's mode: a plausible-looking wrong answer.
	//
	// No new project reference and no cycle: net already imports os (fd_unix.go's own dup() calls
	// os.NewSyscallError), so net.csproj's `core/os` reference and the file's `using os = os_package;`
	// are both already there, and os imports no part of net.
	//
	// What the stub was costing on Linux: (*net.TCPListener).File() -> netFD.dup() bottoms out here,
	// so os/exec's TestExtraFilesRace — which builds its ExtraFiles out of listener files — died on
	// the PartialStubGenerator throw as an infrastructure-error. Windows never surfaced it: both
	// halves are unix-only (net/fd_unix.go is `//go:build unix`, os/file_unix.go is
	// `//go:build unix || (js && wasm) || wasip1`), so neither declaration exists there at all.
	"net.newUnixFile": {source: "os.net_newUnixFile", bareDecl: true},
	// internal/syscall/windows's system-directory query, pushed by runtime/os_windows.go. Unlike the
	// two rows above this is the HANDLE consumer shape — security_windows.go carries its own one-arg
	// `//go:linkname GetSystemDirectory` above a bodyless `func GetSystemDirectory() string` — so
	// bareDecl stays false. It is the first FORWARDED handle-shape row since `unique`.
	//
	// The pushed body is one line of ordinary converted Go (`unsafe.String(&sysDirectory[0],
	// sysDirectoryLen)`), and the precondition the os.runtime_args row spells out had to be made true
	// first. Go fills `runtime.sysDirectory` in initSysDirectory() with
	// `stdcall2(_GetSystemDirectoryA, …)`, called from osinit — and NEITHER half runs in the managed
	// model: osinit is Go's runtime bootstrap, which the converter emits already marked not-run, and
	// stdcall bottoms out in asmstdcall, a throwing stub. So the buffer stays all-zero and its length
	// zero, and a forwarder ALONE would have returned "" — turning net's
	// `hostsFilePath = windows.GetSystemDirectory() + "/Drivers/etc/hosts"` into
	// "/Drivers/etc/hosts", a plausible-looking wrong answer. The hand-owned
	// runtime/windows/os_windows_impl.cs module initializer fills the buffer from
	// Environment.GetFolderPath(SpecialFolder.System), trailing backslash and all, so forwarding and
	// populating are one change here too.
	//
	// What the stub was costing: the throw came out of a package-level VAR INITIALIZER, so it
	// surfaced from net_package's type initializer — and every httptest consumer dies in net's cctor,
	// whatever it was actually testing (net/http/cgi's TestCopyError is where it was found).
	"internal/syscall/windows.GetSystemDirectory": {source: "runtime.windows_GetSystemDirectory"},
	// runtime/metrics's test-only name reader, pushed by runtime/metrics.go — the first row whose
	// consumer is an EXTERNAL TEST package. The -tests conversion sets currentPackagePath to the
	// variant's own PkgPath (`runtime/metrics_test`), so the consumer-side match needs no new
	// machinery: the key simply spells the test package path, and any production package pushing
	// into its own `_test` package takes the same shape. The pushed body is ordinary converted Go
	// (metricsLock/initMetrics, then collect the map keys), running against the managed model's own
	// metrics table — the same table `metrics.All()` reads, which is exactly the agreement TestNames
	// exists to check. metricsLock's semaphore needed its managed form first (manualConversionFuncs
	// "metricsLock" — forwarding and unblocking the pushed body are one change, the
	// GetSystemDirectory precedent). HANDLE consumer shape: description_test.go carries its own
	// one-arg `//go:linkname runtime_readMetricNames` above the bodyless declaration.
	"runtime/metrics_test.runtime_readMetricNames": {source: "runtime.readMetricNames"},
	// runtime/metrics's Read entry point, pushed by the same runtime/metrics.go directive block —
	// the row the one above surfaced: TestNames calls metrics.Read after reading the names. BARE
	// consumer shape: sample.go declares `func runtime_readMetrics(unsafe.Pointer, int, int)` under
	// a prose comment ("is defined in the runtime") with no directive of its own, exactly the
	// syscall.runtime_envs shape.
	//
	// UNHONORABLE, and measured so: a forwarder was tried first and the pushed body ran to
	// readMetricsLocked's slice-header reconstruct — `*(*[]metricSample)(unsafe.Pointer(&sl))` over
	// a raw first-element address — which no managed pointer can alias (the L10 address-reinterpret
	// seam), so the fabricated slice read garbage @string names. The deployed corpus never emits
	// this declaration at all: runtime/metrics/sample.cs is hand-owned and its Read crosses through
	// runtime.readMetricsManaged (managed_impl.cs), which carries names in and computed values out
	// as plain managed data. This row exists for a conversion into a root WITHOUT that hand-own,
	// where the bodyless declaration reappears — and must announce the wall, not fabricate past it.
	"runtime/metrics.runtime_readMetrics": {
		source:   "runtime.readMetrics",
		bareDecl: true,
		reason:   "the pushed body reconstructs a []metricSample from the raw address of the caller's slice, which the managed pointer model cannot alias; use the hand-owned managed crossing in runtime/metrics/sample.cs (metrics.Read -> runtime.readMetricsManaged)",
	},
	// internal/weak's two halves, pushed by runtime/mheap.go — the UNHONORABLE class. Both pushed
	// bodies reach the span allocator: registerWeakPointer → getOrAddWeakHandle → spanOfHeap →
	// `throw("getWeakHandle on invalid pointer")`, and makeStrongFromWeak reads a handle word out of
	// the heap and re-derives a pointer from it. The managed model populates no mheap_ span metadata
	// and cannot re-derive an object from an address, so a forwarder here would either fault or, far
	// worse, hand back a plausible-looking pointer derived from garbage.
	//
	// The remedy these two announced has since LANDED: src/core/internal/weak/pointer.cs is a
	// hand-owned managed weak reference over the ж<T> box under [module: go.GoManualConversion] —
	// System.WeakReference plus a ConditionalWeakTable canonical index (see
	// ConversionStrategies-Reference, "internal/weak.Pointer"). These rows therefore no longer
	// describe the deployed corpus, where the marked file is never regenerated; they describe what a
	// conversion into a root that does NOT already carry the hand-own emits, and that must still be
	// the loud pair rather than a fabricated body. Keep them, with the reason naming the file the
	// caller should be reaching for.
	"internal/weak.runtime_registerWeakPointer": {
		source: "runtime.internal_weak_runtime_registerWeakPointer",
		reason: "the pushed body walks mheap_ span metadata the managed model does not populate; use the hand-owned managed weak reference in internal/weak/pointer.cs",
	},
	"internal/weak.runtime_makeStrongFromWeak": {
		source: "runtime.internal_weak_runtime_makeStrongFromWeak",
		reason: "the pushed body re-derives an object pointer from a heap address, which the managed model cannot do; use the hand-owned managed weak reference in internal/weak/pointer.cs",
	},
	// os/signal's SIX runtime primitives, pushed by runtime/sigqueue.go. BARE consumer shape in every
	// case: signal_unix.go declares the five under one `// Defined by the runtime package.` comment
	// and signal.go declares signalWaitUntilIdle under its own prose comment, none of them carrying a
	// directive — the syscall.runtime_envs shape. signal_unix.go's build constraint includes windows,
	// so these are the declarations Windows compiles.
	//
	// The pushed bodies are runtime/sigqueue.go's own state machine, converted whole and unmodified:
	// the {sigIdle, sigReceiving, sigSending} CAS protocol, the wanted/ignored/mask/recv bitsets, and
	// sigsend, which the OS-side handler calls to queue a signal. Nothing about it is reimplemented
	// here — the registry's job is only to let os/signal reach it.
	//
	// The GetSystemDirectory precedent ("forwarding and populating are one change") governs, and it
	// bit twice over, because the pushed bodies bottomed out in TWO dead ends rather than one:
	//
	//   1. NOBODY WAS QUEUEING. Go arms the delivery path in osinit with
	//      `stdcall2(_SetConsoleCtrlHandler, ctrlHandlerPC, 1)`, and neither half runs in the managed
	//      model — osinit is Go's bootstrap, emitted already marked not-run, and stdcall bottoms out
	//      in asmstdcall, a throwing stub. So ctrlHandler was never reached, sigsend never called, and
	//      a forwarder alone would have made signal.Notify SUCCEED and then never deliver: a
	//      plausible-looking wrong answer, not an error. The hand-owned
	//      runtime/windows/signal_windows_impl.cs supplies exactly that missing edge and nothing else
	//      — a managed SetConsoleCtrlHandler whose callback calls the CONVERTED ctrlHandler.
	//   2. NOBODY COULD WAIT. signal_recv's block is `notetsleepg(&sig.note, -1)`, whose Go prologue
	//      is getg() — still an unimplemented intrinsic — so the receive loop threw before it reached
	//      the note. notetsleepg therefore joins the mutex/note family in manualConversionFuncs and
	//      gains a real blocking wait in runtime/lock_managed_impl.cs.
	//
	// With both landed the pushed bodies run end to end, and Go's OWN Windows semantics fall out of
	// them unaltered — including the ones a POSIX reading would get wrong. sigenable/sigdisable/
	// sigignore really are empty on Windows (signal_windows.go's "Following are not implemented"), so
	// the wanted bitset is the only gate: Notify makes ^C/^BREAK deliver os.Interrupt and the program
	// survive, Stop/Reset restore the default, and Ignore — which clears wanted and sets ignored —
	// leaves ^C terminating the process while Ignored() truthfully reports true. That last one is not
	// a go2cs limit to declare; it is what Go does on Windows, and os/signal's own doc.go says so by
	// documenting only Notify/Reset/Stop under "# Windows". Signals with no Windows source (anything
	// but SIGINT/SIGTERM, which are all ctrlHandler can produce) simply never arrive, in Go and here
	// alike. Faithfulness is the whole mechanism: none of it is decided in this file.
	"os/signal.signal_disable":      {source: "runtime.signal_disable", bareDecl: true},
	"os/signal.signal_enable":       {source: "runtime.signal_enable", bareDecl: true},
	"os/signal.signal_ignore":       {source: "runtime.signal_ignore", bareDecl: true},
	"os/signal.signal_ignored":      {source: "runtime.signal_ignored", bareDecl: true},
	"os/signal.signal_recv":         {source: "runtime.signal_recv", bareDecl: true},
	"os/signal.signalWaitUntilIdle": {source: "runtime.signalWaitUntilIdle", bareDecl: true},
	// reflect's FOUR offset bridges to the runtime, pushed by runtime/runtime1.go. Until these rows
	// existed all four took throwing PartialStubGenerator stubs, and reflect's entire name/type/text
	// offset surface was unimplemented — `addReflectOff` is simply the one a test reached first.
	//
	// WHAT IT COST, measured (reflect, 2026-08-30): TestOffsetLock's four goroutines each threw
	// `NotImplementedException: addReflectOff` in under a second. The dying goroutines never reached
	// their `wg.Done()`, so the test parked in `sync.Wait` forever — an UNBOUNDED HANG that ate a
	// 40-minute deadline and truncated reflect's whole suite to a meaningless 99 pass / 93 fail /
	// 1 skip. (The hang half is answered separately in the test host; this row answers the throw.)
	//
	// THE DIRECTION IS SAFE, and that is the load-bearing fact rather than an assumption:
	// `src/core/reflect/reflect.csproj` already carries a ProjectReference on
	// `core/runtime/runtime.csproj`, so the pusher is UPSTREAM of the target and the reference the
	// forwarder needs is already there. This is the mirror of W1's direction — the case
	// linknamePullWouldCycle exists to refuse — and binding it closes no cycle at all.
	//
	// EACH IS HONORABLE, by this registry's own standard (the os.runtime_args test: a forwarder must
	// not hand back "a plausible-looking wrong answer"). All four pushed bodies are ordinary converted
	// Go over `runtime.reflectOffs`, a MANAGED map, and the resolvers share one shape:
	//
	//   1. walk `firstmoduledata` for a compile-time offset — empty in the managed model, so no match;
	//   2. fall back to the `reflectOffs` map, which is exactly where addReflectOff put it;
	//   3. otherwise `throw` in Go's own shape.
	//
	// So the reflect-MINTED round trip (addReflectOff -> resolveNameOff/TypeOff/TextOff) runs end to
	// end through the managed map — the round trip TestOffsetLock exercises — and an offset that
	// genuinely has no managed answer fails LOUDLY rather than quietly. Nothing here can return a
	// confident wrong value, which is the only condition on which these rows are honorable.
	//
	// SHAPES DIFFER and are recorded per row, because the matcher fails closed in both directions:
	// reflect/type.go gives addReflectOff a one-arg `//go:linkname addReflectOff` HANDLE, while the
	// three resolvers are the older BARE shape — `//go:noescape` and a prose "Implemented in the
	// runtime package", no directive of their own.
	//
	// internal/reflectlite's siblings are deliberately NOT added here. runtime pushes them too
	// (reflectlite_resolveNameOff / reflectlite_resolveTypeOff), but reflectlite already answers those
	// partials from a hand-written `type_impl.cs`, and a converter-emitted body would collide with it —
	// the exact hazard this registry's header warns about. Those placeholders `return default!`, which
	// IS the plausible-wrong-answer shape, so replacing them with real forwarders is a genuine
	// improvement — but it means REMOVING a hand-own, which is its own change with its own gates.
	"reflect.addReflectOff":  {source: "runtime.reflect_addReflectOff"},
	"reflect.resolveNameOff": {source: "runtime.reflect_resolveNameOff", bareDecl: true},
	"reflect.resolveTypeOff": {source: "runtime.reflect_resolveTypeOff", bareDecl: true},
	"reflect.resolveTextOff": {source: "runtime.reflect_resolveTextOff", bareDecl: true},
}

// linknamePushSources is the reverse index of the FORWARDED entries of linknamePushTargets: the set
// of pushing definitions ("<pkgPath>.<funcName>") a forwarder calls, so packageFuncAccess can emit
// each public. Built once from the registry so the two can never disagree. An unhonorable entry is
// excluded deliberately — nothing calls it, so widening it would be noise.
var linknamePushSources = func() map[string]bool {
	sources := map[string]bool{}

	for _, push := range linknamePushTargets {
		if push.reason == "" {
			sources[push.source] = true
		}
	}

	return sources
}()

// linknameVarAlias is the disposition of a `//go:linkname` VAR alias whose storage the converter
// INVERTS. `storage` names the package and member that hold the one real variable
// ("<pkgPath>.<member>"); the package this entry is keyed under emits a forwarding property to it
// instead of a field of its own.
//
// A Go var alias is a link-time identity — `runtime.canUseLongPaths` and
// `internal/syscall/windows.CanUseLongPaths` are the same word of memory, arranged with no import in
// either direction. C# has no link-time identity: one assembly must hold the field and the other must
// reach it through a member reference, which is a COMPILE-TIME edge and therefore must be acyclic. So
// for every aliased pair the converter has to answer one question — WHICH SIDE HOLDS THE STORAGE —
// and the project graph forces the answer: storage goes in whichever package the other one already
// depends on.
//
// varLinknamePull answers it exactly one way: storage always goes to the package named on the RIGHT
// of the two-argument directive. That is right whenever the pull points DOWN the dependency order
// (math/bits -> runtime) and wrong whenever it points UP, where linknamePullWouldCycle can only
// refuse — emitting two unrelated fields, which compiles and is silently incorrect. This registry is
// how the upward case is INVERTED instead of given up on.
type linknameVarAlias struct {
	storage string
}

// linknameVarAliasTargets is the registry of `//go:linkname` VAR aliases whose storage is inverted,
// keyed "<declaringPkgPath>.<Symbol>" — the declaration that becomes the FORWARDER.
//
// Curated, for the identical reason linknamePushTargets is, and the constraint is worth restating
// because it is what makes a registry the only available mechanism: converting
// `internal/syscall/windows`, the converter cannot see `runtime`'s directive at all. A package is
// converted from its own syntax, and its dependencies contribute TYPES, not comments. From isw's side
// `var CanUseLongPaths bool` under a one-arg handle is indistinguishable from any other opened var;
// nothing in it names runtime, and nothing can. The missing half is recorded here as a judgment, and
// TestLinknameVarAliasRegistryMatchesGoSource re-derives both halves from GOROOT so the judgment
// cannot rot.
//
// One row today, because the corpus exposes exactly one upward pair. The three DOWNWARD pulls
// (math/bits's overflowError and divideError, time/sleep_test's haveHighResSleep) need no row: their
// forwarding property already points the safe way and varLinknamePull emits it.
var linknameVarAliasTargets = map[string]linknameVarAlias{
	// Windows long-path awareness. runtime/os_windows.go carries the two-argument directive
	// (`//go:linkname canUseLongPaths internal/syscall/windows.CanUseLongPaths`) and isw's
	// syscall_windows.go carries the one-argument handle that authorizes it — so Go's own write lives
	// in runtime and the alias points UP, out of runtime into a package that reaches back.
	//
	// It reaches back through Go's OWN imports, which is why no conversion order and no pruning of
	// converter-introduced edges can help: `internal/syscall/windows -> syscall -> runtime` is three
	// real import edges, so a project reference `runtime -> internal/syscall/windows` is a cycle
	// however it is emitted (measured: that single edge takes the corpus graph from 0 cycles to 6,
	// MSB4006). Inverting costs ZERO new references — isw already references runtime for the
	// GetSystemDirectory push forwarder, and would reach it through syscall regardless.
	//
	// Storage in `runtime` is also where it belongs on the merits: that is where Go's own write is
	// (initLongPathSupport sets canUseLongPaths = true), and it is the side that keeps the graph
	// acyclic. FORWARDING ALONE WOULD BE A NO-OP — the managed model runs neither osinit nor the
	// stdcall the Go body uses — so the populate half is part of the same change: golib records
	// whether it actually set the PEB IsLongPathAwareProcess bit, and runtime/windows/
	// os_windows_impl.cs copies that OUTCOME into canUseLongPaths. Tying the flag to the outcome
	// rather than the intent is the point: if the bit was not set, telling os to stop prefixing
	// produces paths that silently fail — a plausible-looking wrong answer, the failure mode this
	// project rules against. See docs/phase4/DESIGN-linkname-push-cycles.md.
	"internal/syscall/windows.CanUseLongPaths": {storage: "runtime.canUseLongPaths"},
}

// linknameVarAliasStorage is the reverse index of linknameVarAliasTargets: the set of STORAGE members
// ("<pkgPath>.<member>") a forwarding property reads, so packageVarAccess can emit each public. Built
// once from the registry so the two can never disagree — the same derivation, for the same reason, as
// linknamePushSources.
//
// The derivation is not a convenience. Two hand-maintained lists of the same fact are exactly how the
// storage side and the forwarding side come to disagree, and a disagreement here is silent in the
// worst direction: the forwarder compiles against a member it cannot see only if some OTHER rule
// happened to publicize it, and stops compiling the day that rule changes.
var linknameVarAliasStorage = func() map[string]bool {
	storage := map[string]bool{}

	for _, alias := range linknameVarAliasTargets {
		storage[alias.storage] = true
	}

	return storage
}()

// typeIsPubliclyAccessible reports whether a value of type t can be exposed by a `public` member —
// every NAMED type it references must be exported (or already publicized, or a universe type like
// `error`). Composites recurse to their element; an anonymous struct/signature and any other shape is
// conservatively NOT accessible (its emission may reference unexported members), so its handle var
// stays internal rather than risk CS0052/CS0053.
func typeIsPubliclyAccessible(t types.Type) bool {
	switch t := t.(type) {
	case *types.Basic:
		return true
	case *types.Alias:
		return typeIsPubliclyAccessible(types.Unalias(t))
	case *types.Named:
		obj := t.Obj()

		// A universe type (`error`, `comparable`) has no package and is always accessible.
		if obj == nil || obj.Pkg() == nil {
			return true
		}

		return obj.Exported() || packagePublicizedTypes[obj]
	case *types.Pointer:
		return typeIsPubliclyAccessible(t.Elem())
	case *types.Slice:
		return typeIsPubliclyAccessible(t.Elem())
	case *types.Array:
		return typeIsPubliclyAccessible(t.Elem())
	default:
		return false
	}
}

// varLinknamePull recognizes a package var carrying a TWO-argument `//go:linkname <name>
// <pkgpath>.<remote>` PULL directive (name matches the var). It returns the fully-qualified C#
// reference to the remote symbol (`go.runtime_package.overflowError`) and the remote's import path,
// so the var is emitted as a forwarding property to it and the path is queued for a project
// reference. The fully-qualified form resolves inside `namespace go;` without a file-local using.
// Go 1.23 requires the remote's package to authorize the pull with a matching one-arg handle, which
// the converter honors by emitting that remote public (see packageVarAccess) — so the forwarding
// property compiles across the assembly boundary. The comment may sit on the GenDecl or the spec.
func varLinknamePull(name string, options Options, docs ...*ast.CommentGroup) (ref string, pkgPath string, ok bool) {
	for _, doc := range docs {
		if doc == nil {
			continue
		}

		for _, comment := range doc.List {
			fields := strings.Fields(comment.Text)

			// //go:linkname <local> <pkgpath>.<remote>
			if len(fields) != 3 || fields[0] != "//go:linkname" || fields[1] != name {
				continue
			}

			target := fields[2]
			dot := strings.LastIndex(target, ".")

			if dot < 0 {
				continue
			}

			pkgPath = target[:dot]

			// A pull whose forwarding reference would form a project-ref cycle keeps its plain-field
			// form (the pre-feature behavior) — runtime pulling internal/syscall/windows.CanUseLongPaths.
			if linknamePullWouldCycle(pkgPath, options) {
				return "", "", false
			}

			remote := getSanitizedIdentifier(target[dot+1:])
			class := RootNamespace + "." + convertImportPathToNamespace(pkgPath, PackageSuffix)

			return class + "." + remote, pkgPath, true
		}
	}

	return "", "", false
}

// varLinknameAliasForward recognizes a package var that is the FORWARDING side of an inverted
// `//go:linkname` var alias (linknameVarAliasTargets) and returns the C# reference to the member that
// holds the storage — `go.runtime_package.canUseLongPaths` — so the var is emitted as a forwarding
// property to it rather than a field of its own. This is varLinknamePull's inverse: there the LOCAL
// declaration carries the two-arg directive and forwards to the remote it names, here the local
// declaration carries only Go's one-arg HANDLE and the directive naming it lives in the package that
// keeps the storage, invisible from this side. Hence the registry.
//
// Go's authorization is still required. The one-arg handle is what opens a var to a linkname alias at
// all, and a registry row is a recorded judgment about a pair, not a license to rewrite an ordinary
// declaration into a reference to somebody else's field: a row that named a var its own package never
// opened would not be faithful, and — the failure mode that matters — a row left behind after Go
// retired the handle would silently keep forwarding. Requiring the handle makes both fail CLOSED, to
// the plain field this converter emitted before the feature existed. It is the same requirement
// funcLinknamePush's handle shape enforces for the push direction.
//
// linknameTargetAlias both QUEUES the storage package for a project reference and spells the
// qualifier this file must use to reach it. Queuing is not a formality even where the reference
// already exists for another reason — for this pair isw already references runtime via the
// GetSystemDirectory push forwarder, and would reach it through syscall regardless — because the
// emission's correctness must not depend on some other emission happening to queue it first. It is a
// set, so for a package already referenced it is a no-op and the emitted `.csproj` does not change.
func (v *Visitor) varLinknameAliasForward(goIDName string) (ref string, ok bool) {
	alias, isAliased := linknameVarAliasTargets[currentPackagePath+"."+goIDName]

	if !isAliased || !linknameHandles.Contains(goIDName) {
		return "", false
	}

	dot := strings.LastIndex(alias.storage, ".")

	if dot <= 0 || dot == len(alias.storage)-1 {
		return "", false
	}

	return v.linknameTargetAlias(alias.storage[:dot]) + "." + getSanitizedIdentifier(alias.storage[dot+1:]), true
}
