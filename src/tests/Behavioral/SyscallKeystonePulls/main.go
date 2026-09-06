package main

// SyscallKeystonePulls guards the //go:linkname PULLS internal/syscall/unix makes of the syscall
// package's keystone family (syscall_syscall, syscall_syscallPtr, syscall_syscall6, syscall_syscall6X,
// syscall_syscall9 -- net_darwin.go on darwin). Each was a bodyless partial filled by the stub
// generator's throw in the darwin corpus until darwin increment 10 (a), so the FIRST line of any
// exec.Command by bare name (LookPath -> unix.Eaccess -> faccessat -> syscall_syscall6) died with
// NotImplementedException on both mac legs -- the SigIgnoreDisposition probe's finding, 2026-09-05.
// LookPath needs no fork and no terminal, so it is the reach increment 10 (a) opens and the reach
// this row asserts.
//
// THE ASSERTION THIS ROW LOST AND HAS NOW REGAINED (COORD 1daa96a11, amended 2cd045a88; regained
// at darwin increment 11, 2026-09-06). It first also asserted user.Current(), which reaches the same
// five pulls through getpwuid_r. That was NOT usable as a guard at increment 10 (a), and the
// measurement said why: on both mac legs (run 33975385965) Go printed the real record while the
// converted side answered
//
//     user.Current: error: user: lookup userid 501: internal buffer exceeds 1048576 bytes
//
// which is Go's own retryWithBuffer (os/user/cgo_lookup_unix.go:182) giving up after doubling the
// buffer from 1 KB to 1 MB against getpwuid_r answering ERANGE (34) EVERY time. So the call was
// reached -- the keystone bodies worked -- and the failure was downstream, in the PTROUT class:
// unix.Getpwuid handed libc a converted *Passwd (a managed struct whose six ж<byte> fields are
// object references, so the CLR lays it out AUTO and reorders it) and a **Passwd out-parameter that
// arrives NULL while the box is still nil.
//
// A two-arm probe then measured both halves rather than arguing them (runs 34026852472 and
// 34034875069, both mac legs, every buffer size). The NULL out-parameter is what darwin reports as
// ERANGE, so the buffer was never the problem and the message named the wrong argument; and with an
// honest native cell the call SUCCEEDS while the managed record still reads Uid=0 Name=nil, because
// libc's 72 bytes land beside the CLR's reordered fields rather than on them. Both halves are
// answered together by internal/syscall/unix/darwin/user_darwin_impl.cs, and this row is what
// asserts them end to end.
//
// WHAT THE FOUR LOOKUPS BELOW COVER on darwin, one per hand-owned member: user.Current is getpwuid_r,
// user.Lookup is getpwnam_r, user.LookupGroupId is getgrgid_r and user.LookupGroup is getgrnam_r.
// Each is checked by a ROUND TRIP through the value the previous one returned, so a transcription
// that fills the record with the wrong bytes fails here rather than merely returning something.
//
// IT PRINTS ONLY HOST-INVARIANT FACTS -- whether a lookup succeeded, and whether the identifiers it
// must fill came back non-empty and consistent -- never the account's name, home directory or
// numeric ids. Go and the converted program are compared on the SAME host, so a boolean carries the
// whole divergence (before the fix: Go false, C# an error), while printing the record itself would
// put the running account into every behavioral log and CI transcript and buy no extra evidence.
import (
	"fmt"
	"os/exec"
	"os/user"
	"path/filepath"
	"runtime"
	"strings"
)

func main() {
	p, err := exec.LookPath("go")
	if err != nil {
		fmt.Println("LookPath(go): error:", err)
	} else {
		fmt.Println("LookPath(go):", strings.TrimSuffix(filepath.Base(p), ".exe"), "absolute:", filepath.IsAbs(p))
	}

	_, err = exec.LookPath("no-such-executable-go2cs-keystone")
	fmt.Println("LookPath(missing): error:", err != nil)

	// DARWIN ONLY, and the gate is the point rather than caution: these four lookups exist here to
	// assert the darwin PTROUT members, and on the other two targets os/user reaches an entirely
	// different implementation this cut does not touch -- Windows SIDs, and on Linux a file reader
	// whose answer depends on whether the host's accounts live in /etc/passwd or only in NSS, which
	// would make the row's verdict a property of the runner rather than of the conversion. Both
	// sides of an output comparison run on the SAME host, so both take the same branch.
	if runtime.GOOS != "darwin" {
		fmt.Println("done")
		return
	}

	u, err := user.Current()
	fmt.Println("user.Current: error:", err != nil)
	if err == nil {
		fmt.Println("user.Current: uid set:", u.Uid != "", "gid set:", u.Gid != "",
			"username set:", u.Username != "")

		byName, e := user.Lookup(u.Username)
		fmt.Println("user.Lookup(username): error:", e != nil,
			"uid round-trips:", e == nil && byName.Uid == u.Uid)

		g, e := user.LookupGroupId(u.Gid)
		fmt.Println("user.LookupGroupId(gid): error:", e != nil,
			"name set:", e == nil && g.Name != "")
		if e == nil {
			byGroup, e2 := user.LookupGroup(g.Name)
			fmt.Println("user.LookupGroup(name): error:", e2 != nil,
				"gid round-trips:", e2 == nil && byGroup.Gid == g.Gid)
		}
	}

	fmt.Println("done")
}
