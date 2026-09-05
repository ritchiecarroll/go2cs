// manualConversionScope_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Guards for manualConversionFuncs' PLATFORM SCOPE (manualTypeOperations.go).
//
// The registry is keyed by name, and a Go name is not unique across platforms — Go selects one of
// several files declaring the same function by build constraint. Unscoped, one entry spoke for every
// flavor: it turned each one's declaration into a placeholder while an implementation existed only
// where somebody had hand-written one. The two clusters that measurably suffered are the fixtures
// here, and they fail in OPPOSITE directions, which is why both are pinned:
//
//   - runtime's mutex/note cluster — BOTH flavors need hand-owning, and the entry must therefore
//     survive the fact that lock_sema.go and lock_futex.go declare notetsleep_internal with
//     DIFFERENT ARITY. The registry answers "hand-owned" for both and says nothing about shape.
//   - os.(*File).readdir — only the windows and darwin flavors need hand-owning; dir_unix.go's is
//     pure Go and converts faithfully. Unscoped, the entry deleted it too, and the Linux `os` build
//     had a placeholder with nothing to link against.
//
// A scope that silently stops matching is indistinguishable from no hand-own at all (the auto body
// is emitted, compiles, and is wrong at runtime) or from a missing implementation (a placeholder
// with no impl, which at least fails loudly at compile time). Both readings are asserted directly.

package main

import (
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// parseManualScopeFixture parses a fixture source and indexes its function declarations by the same
// "name" / "recvType.name" key manualConversionFuncs uses.
func parseManualScopeFixture(t *testing.T, fileName string, source string) map[string]*ast.FuncDecl {
	t.Helper()

	file, err := parser.ParseFile(token.NewFileSet(), fileName, source, parser.SkipObjectResolution)

	if err != nil {
		t.Fatalf("parse %s: %v", fileName, err)
	}

	decls := map[string]*ast.FuncDecl{}

	for _, decl := range file.Decls {
		funcDecl, ok := decl.(*ast.FuncDecl)

		if !ok {
			continue
		}

		key := funcDecl.Name.Name

		if funcDecl.Recv != nil && len(funcDecl.Recv.List) > 0 {
			recvType := funcDecl.Recv.List[0].Type

			if starExpr, ok := recvType.(*ast.StarExpr); ok {
				recvType = starExpr.X
			}

			if ident, ok := recvType.(*ast.Ident); ok {
				key = ident.Name + "." + key
			}
		}

		decls[key] = funcDecl
	}

	return decls
}

// TestLockFlavorsAreBothHandOwnedAtTheirOwnArity is the fixture the mutex/note cluster is named for.
// Go gives the SAME function two signatures: lock_sema.go's notetsleep_internal takes (n, ns, gp,
// deadline) and lock_futex.go's takes (n, ns). Neither OS primitive underneath — semaphore or futex —
// has a managed realization, so both flavors are hand-owned and both converge on one managed core;
// the divergence lives in each flavor's own *_impl.cs, never in the registry.
//
// The failure this locks out is precise: a scope narrowed to the semaphore family (the plausible
// mistake, since lock_sema is what the host platform builds) leaves Linux emitting a REAL converted
// lock_futex body whose futexsleep/futexwakeup are OS-metal stubs — a conversion that compiles and
// deadlocks — instead of a placeholder its hand-own fills.
func TestLockFlavorsAreBothHandOwnedAtTheirOwnArity(t *testing.T) {
	semaphore := parseManualScopeFixture(t, "lock_sema.go", `package runtime

func mutexContended(l *mutex) bool { return false }
func lock2(l *mutex) {}
func unlock2(l *mutex) {}
func notewakeup(n *note) {}
func notesleep(n *note) {}
func notetsleep_internal(n *note, ns int64, gp *g, deadline int64) bool { return false }
func notetsleep(n *note, ns int64) bool { return false }
func noteclear(n *note) {}
`)

	futex := parseManualScopeFixture(t, "lock_futex.go", `package runtime

func key32(p *uintptr) *uint32 { return nil }
func mutexContended(l *mutex) bool { return false }
func lock2(l *mutex) {}
func unlock2(l *mutex) {}
func notewakeup(n *note) {}
func notesleep(n *note) {}
func notetsleep_internal(n *note, ns int64) bool { return false }
func notetsleep(n *note, ns int64) bool { return false }
func noteclear(n *note) {}
`)

	handOwned := []string{"mutexContended", "lock2", "unlock2", "notewakeup", "notesleep", "notetsleep_internal"}

	for _, name := range handOwned {
		for _, goos := range []string{"windows", "darwin"} {
			if !isManualFuncDeclInPackage("runtime", goos, semaphore[name]) {
				t.Errorf("runtime.%s (lock_sema.go flavor) must be hand-owned on %s: the semaphore park has no managed form", name, goos)
			}
		}

		if !isManualFuncDeclInPackage("runtime", "linux", futex[name]) {
			t.Errorf("runtime.%s (lock_futex.go flavor) must be hand-owned on linux: futexsleep/futexwakeup are OS-metal stubs, "+
				"so the auto body compiles and then deadlocks", name)
		}
	}

	// The 4-vs-2 divergence is real and the registry is deliberately blind to it: both declarations
	// answer the same, and each flavor's *_impl.cs carries its own signature.
	if len(semaphore["notetsleep_internal"].Type.Params.List) != 4 || len(futex["notetsleep_internal"].Type.Params.List) != 2 {
		t.Fatal("fixture drift: notetsleep_internal is 4 parameters in lock_sema.go and 2 in lock_futex.go; " +
			"if Go changed that, the *_impl.cs siblings change with it")
	}

	// The thin wrappers around the cluster stay auto on BOTH flavors — they are ordinary Go.
	for _, name := range []string{"notetsleep", "noteclear"} {
		if isManualFuncDeclInPackage("runtime", "windows", semaphore[name]) || isManualFuncDeclInPackage("runtime", "linux", futex[name]) {
			t.Errorf("runtime.%s must stay auto-converted on every flavor", name)
		}
	}

	// lock_futex.go's key32 has no lock_sema counterpart and needs no hand-own.
	if isManualFuncDeclInPackage("runtime", "linux", futex["key32"]) {
		t.Error("runtime.key32 must stay auto-converted: it is a plain Reinterpret of the key slot")
	}
}

// TestReaddirIsHandOwnedOnlyWhereGoHandsOSMemoryToAGoStruct pins the entry whose unscoped form
// invented a hand-own gap. dir_windows.go reinterprets the buffer GetFileInformationByHandleEx fills
// as a Go struct and dir_darwin.go hands libc's readdir_r the address of a Go syscall.Dirent — both
// non-blittable, both hand-owned. dir_unix.go's readdir is pure Go over internal/poll's ReadDirent,
// so it converts faithfully; scoping Linux OUT is what gives it a body at all.
func TestReaddirIsHandOwnedOnlyWhereGoHandsOSMemoryToAGoStruct(t *testing.T) {
	decls := parseManualScopeFixture(t, "dir.go", `package os

func (f *File) readdir(n int, mode readdirMode) (names []string, dirents []DirEntry, infos []FileInfo, err error) {
	return nil, nil, nil, nil
}

func (f *File) Readdir(n int) ([]FileInfo, error) { return nil, nil }
`)

	for _, goos := range []string{"windows", "darwin"} {
		if !isManualFuncDeclInPackage("os", goos, decls["File.readdir"]) {
			t.Errorf("os.(*File).readdir must be hand-owned on %s: the flavor there hands OS memory to a Go struct "+
				"whose managed layout does not match the bytes", goos)
		}
	}

	if isManualFuncDeclInPackage("os", "linux", decls["File.readdir"]) {
		t.Error("os.(*File).readdir must NOT be hand-owned on linux: dir_unix.go's is pure Go over internal/poll, " +
			"and skipping it leaves the Linux os build with a placeholder and nothing to link against")
	}

	// The exported wrapper above it is ordinary Go on every platform.
	if isManualFuncDeclInPackage("os", "windows", decls["File.Readdir"]) {
		t.Error("os.(*File).Readdir must stay auto-converted; only the unexported readdir is hand-owned")
	}
}

// TestWindowsOnlyEntriesAreScopedToWindows covers the other direction the scope records: syscall's
// generated wrappers and os.readReparseLink are declared only in Go's Windows sources, so the entries
// were already inert elsewhere. Scoping them changes no emission — it states the fact, so a unix
// declaration that later takes one of these names cannot silently inherit a Windows hand-own the way
// readdir did.
func TestWindowsOnlyEntriesAreScopedToWindows(t *testing.T) {
	windowsOnly := map[string][]string{
		"syscall": {"GetTimeZoneInformation", "findFirstFile1", "findNextFile1", "Process32First", "Process32Next",
			"GetAddrInfoW", "FreeAddrInfoW"},
		"os": {"readReparseLink"},
		// net's Windows interface enumeration. interface_windows.go is the only file in net that
		// declares adapterAddresses — every other platform reaches interfaceTable by a completely
		// different route (route sockets on BSD, netlink on Linux) — so an unscoped entry is inert
		// today and a trap the moment one of those grows a same-named helper.
		"net": {"adapterAddresses"},
	}

	for pkgPath, names := range windowsOnly {
		for _, name := range names {
			scope, listed := manualConversionFuncs[pkgPath][name]

			if !listed {
				t.Errorf("%s.%s is no longer registered", pkgPath, name)
				continue
			}

			if !scope.includes("windows") {
				t.Errorf("%s.%s must apply on windows — that is the only platform Go declares it on", pkgPath, name)
			}

			for _, goos := range []string{"linux", "darwin"} {
				if scope.includes(goos) {
					t.Errorf("%s.%s must not apply on %s: Go declares it only in its Windows sources, so a %s "+
						"declaration of that name would be a DIFFERENT function inheriting this hand-own", pkgPath, name, goos, goos)
				}
			}
		}
	}
}

// TestLinuxOnlyEntriesAreScopedToLinux is the mirror of the Windows guard above for the first
// Linux-scoped members: syscall's Fstat and fstatat are hand-owned on linux because the converted
// Stat_t is not blittable and the generated wrappers hand the kernel its managed image — but darwin
// declares BOTH names too (syscall_darwin.go), with libc-backed bodies that are not defective, so an
// entry that also matched darwin would turn working wrappers into placeholders with nothing behind
// them (os.(*File).readdir's lesson). Windows is named too: Go's Windows sources do not declare
// either, and an entry that claimed them there would be inert today and a trap tomorrow.
func TestLinuxOnlyEntriesAreScopedToLinux(t *testing.T) {
	linuxOnly := map[string][]string{
		"syscall": {"Fstat", "fstatat", "wait4"},
	}

	for pkgPath, names := range linuxOnly {
		for _, name := range names {
			scope, listed := manualConversionFuncs[pkgPath][name]

			if !listed {
				t.Errorf("%s.%s is no longer registered", pkgPath, name)
				continue
			}

			if !scope.includes("linux") {
				t.Errorf("%s.%s must apply on linux — that is the flavor whose Stat_t mirror it names", pkgPath, name)
			}

			for _, goos := range []string{"windows", "darwin"} {
				if scope.includes(goos) {
					t.Errorf("%s.%s must not apply on %s: darwin's same-named libc wrapper is not the defective one and "+
						"Windows declares neither, so a %s match would displace a working body or nothing at all", pkgPath, name, goos, goos)
				}
			}
		}
	}
}

// TestSockaddrFamilyIsScopedToEachHandOwningFlavor pins the shape the sockaddr family has taken in
// three steps: windows on 2026-08-22 (syscall_windows_impl.cs, L10), linux the same day
// (sockaddr_linux_impl.cs, the mirror), and darwin on 2026-09-05 (sockaddr_darwin_impl.cs, the twin —
// increment 8, the five behavioral net rows dying in SockaddrInet4.sockaddr() <- Bind on both mac
// legs). One entry, each flavor's file the authority on its own body — the lock_sema/lock_futex
// precedent — so the INET encoders and Bind/Connect/Getsockname/Getpeername apply EVERYWHERE, and the
// free-function half Go declares only in its unix sources (anyToSockaddr, Recvfrom, Sendto,
// recvmsgRaw, SendmsgN) applies on the two unix flavors. ConnectEx and RawSockaddrAny.Sockaddr exist
// only in Go's Windows sources; Accept4 only in its Linux sources; Accept as a BODIED function only
// in its BSD sources (linux's Accept is pure Go over Accept4 and stays auto).
//
// The scope is checked against the FILE that hand-owns it, both ways: a flavor is in scope exactly
// when a marked hand-own under src/core/syscall/<goos>/ declares the member. Widening a scope
// without its body, or dropping a body while the scope stands, reads RED here by name — the
// two-sided seam ledger CLAUDE.md asks of every registration.
func TestSockaddrFamilyIsScopedToEachHandOwningFlavor(t *testing.T) {
	everywhere := []string{"SockaddrInet4.sockaddr", "SockaddrInet6.sockaddr", "Bind", "Connect", "Getsockname", "Getpeername"}
	windowsOnly := []string{"ConnectEx", "RawSockaddrAny.Sockaddr"}
	linuxOnly := []string{"Accept4"}
	unixOnly := []string{"anyToSockaddr", "Recvfrom", "Sendto", "recvmsgRaw", "SendmsgN"}
	darwinOnly := []string{"Accept"}

	flavors := []string{"windows", "linux", "darwin"}
	handOwns := sockaddrHandOwnSources(t)

	check := func(name string, want map[string]bool) {
		scope, listed := manualConversionFuncs["syscall"][name]

		if !listed {
			t.Errorf("syscall.%s is no longer registered", name)
			return
		}

		// The receiver-qualified keys name the member the hand-own spells: `sockaddr(this ж<SockaddrInet4>`
		// for the encoders, `Sockaddr(this ж<RawSockaddrAny>` for the windows decode.
		witness := name
		if dot := strings.LastIndex(name, "."); dot >= 0 {
			witness = name[dot+1:] + "(this ж<" + name[:dot] + ">"
		} else {
			witness = " " + name + "("
		}

		for _, goos := range flavors {
			inScope := scope.includes(goos)
			bodied := strings.Contains(handOwns[goos], witness)

			if inScope != want[goos] {
				t.Errorf("syscall.%s: applies on %s = %v, want %v", name, goos, inScope, want[goos])
			}

			if inScope && !bodied {
				t.Errorf("syscall.%s is displaced on %s but no marked hand-own under src/core/syscall/%s/ declares %q: the placeholder would leave it unimplemented", name, goos, goos, strings.TrimSpace(witness))
			}

			if !inScope && bodied {
				t.Errorf("syscall.%s has a hand-own body under src/core/syscall/%s/ but is NOT displaced there: the generated body survives beside it (CS0111)", name, goos)
			}
		}
	}

	scopes := func(goos ...string) map[string]bool {
		want := map[string]bool{}
		for _, g := range goos {
			want[g] = true
		}
		return want
	}

	for _, name := range everywhere {
		check(name, scopes("windows", "linux", "darwin"))
	}

	for _, name := range windowsOnly {
		check(name, scopes("windows"))
	}

	for _, name := range linuxOnly {
		check(name, scopes("linux"))
	}

	for _, name := range unixOnly {
		check(name, scopes("linux", "darwin"))
	}

	for _, name := range darwinOnly {
		check(name, scopes("darwin"))
	}
}

// sockaddrHandOwnSources concatenates, per flavor, the text of every marked hand-own under
// src/core/syscall/<goos>/ — the witness the family guard checks each scope against. A clone without
// src/core beside the converter has nothing to witness and skips.
func sockaddrHandOwnSources(t *testing.T) map[string]string {
	t.Helper()

	sources := map[string]string{}

	for _, goos := range []string{"windows", "linux", "darwin"} {
		dir := filepath.Join("..", "core", "syscall", goos)
		entries, err := os.ReadDir(dir)

		if err != nil {
			t.Skipf("src/core/syscall/%s is not beside the converter; nothing to witness: %v", goos, err)
		}

		var text strings.Builder

		for _, entry := range entries {
			if entry.IsDir() || !strings.HasSuffix(entry.Name(), ".cs") {
				continue
			}

			body, err := os.ReadFile(filepath.Join(dir, entry.Name()))

			if err != nil {
				t.Fatal(err)
			}

			// The converter's own line-anchored marker (testConversion.go), never a comment's mention of it.
			if manualConversionMarker.Match(body) {
				text.Write(body)
				text.WriteByte('\n')
			}
		}

		sources[goos] = text.String()
	}

	return sources
}

// TestEveryManualConversionScopeNamesAKnownGOOS is the typo guard. A scope naming "win" or "macos"
// matches no target at all, which silently turns the entry off everywhere — the auto body is emitted
// and compiles, and the hand-own it was protecting is simply gone. Nothing else in the pipeline can
// report that, because "not hand-owned" is a legitimate answer for every other declaration.
func TestEveryManualConversionScopeNamesAKnownGOOS(t *testing.T) {
	scoped := 0

	for pkgPath, funcScopes := range manualConversionFuncs {
		for name, scope := range funcScopes {
			if len(scope) == 0 {
				continue
			}

			scoped++

			for _, goos := range scope {
				if !isKnownGOOS(goos) {
					t.Errorf("%s.%s is scoped to %q, which is not a known GOOS: the entry matches nothing and the hand-own "+
						"it names is silently unprotected", pkgPath, name, goos)
				}
			}
		}
	}

	if scoped == 0 {
		t.Error("no entry carries a platform scope; goosScope has no live use and this guard proves nothing")
	}
}

// TestGoosAnyIsTheDefaultScope pins the zero value's meaning. Most entries name declarations that
// exist identically on every target, and an entry that says nothing about platforms must keep the
// registry's original name-keyed behavior rather than matching nothing.
func TestGoosAnyIsTheDefaultScope(t *testing.T) {
	if !goosAny.includes("windows") || !goosAny.includes("linux") || !goosAny.includes("darwin") || !goosAny.includes("plan9") {
		t.Error("goosAny must include every target, including one no build currently produces")
	}

	if scope := manualConversionFuncs["sync"]["copyChecker.check"]; len(scope) != 0 {
		t.Error("sync.copyChecker.check names a platform-neutral defect (a boxed copy in a CAS) and must stay goosAny")
	}

	for _, goos := range []string{"windows", "linux", "darwin"} {
		decls := parseManualScopeFixture(t, "cond.go", `package sync

type copyChecker uintptr

func (c *copyChecker) check() {}
`)

		if !isManualFuncDeclInPackage("sync", goos, decls["copyChecker.check"]) {
			t.Errorf("sync copyChecker.check must be hand-owned on %s", goos)
		}
	}
}

// Recvfrom is hand-owned on the two UNIX flavors — linux (sockaddr_linux_impl.cs, 2026-08-30) and
// darwin (sockaddr_darwin_impl.cs, 2026-09-05) — because the generated body hands the kernel a managed
// RawSockaddrAny by address, which the kernel overwrites: the class that kills net.Interfaces() with an
// AccessViolation on linux. Windows has its own Recvfrom surface (WSARecvFrom) and is NOT displaced.
func TestRecvfromIsScopedToTheUnixHandOwningFlavors(t *testing.T) {
	entry, ok := manualConversionFuncs["syscall"]["Recvfrom"]

	if !ok {
		t.Fatal("syscall.Recvfrom is not registered as a manual conversion; the unix hand-owns in " +
			"sockaddr_linux_impl.cs / sockaddr_darwin_impl.cs would be shadowed by the generated body that causes the AV")
	}

	for _, goos := range []string{"linux", "darwin"} {
		if !entry.includes(goos) {
			t.Errorf("syscall.Recvfrom must be displaced on %s -- that is where a hand-own lives", goos)
		}
	}

	if entry.includes("windows") {
		t.Error("syscall.Recvfrom must NOT be displaced on windows: no hand-own exists there, so the " +
			"placeholder would leave the function unimplemented")
	}
}

// The UDP send helpers are hand-owned on WINDOWS only: their bodies live in
// internal/syscall/windows/windows/net_windows_impl.cs, and the linux/darwin flavors of this package
// do not compile that folder at all -- displacing them there would leave the declaration unfilled.
func TestWSASendtoIsScopedToWindowsOnly(t *testing.T) {
	for _, name := range []string{"WSASendtoInet4", "WSASendtoInet6"} {
		entry, ok := manualConversionFuncs["internal/syscall/windows"][name]

		if !ok {
			t.Fatalf("internal/syscall/windows.%s is not registered; the hand-own would be shadowed "+
				"by the generated body that hands the kernel a managed sockaddr", name)
		}

		if !entry.includes("windows") {
			t.Errorf("%s must be displaced on windows -- that is where the hand-own lives", name)
		}

		for _, goos := range []string{"linux", "darwin"} {
			if entry.includes(goos) {
				t.Errorf("%s must NOT be displaced on %s: no hand-own exists there", name, goos)
			}
		}
	}
}
