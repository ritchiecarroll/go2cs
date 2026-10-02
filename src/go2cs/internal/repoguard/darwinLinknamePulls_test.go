// darwinLinknamePulls_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// DARWIN'S LINKNAME PULLS, AND WHICH OF THEM HAVE NOTHING BEHIND THEM.
//
// A `//go:linkname local pkg.target` PULL declares a bodyless function whose body lives in another
// package. The converter emits it as a bodyless `partial`, which PartialStubGenerator fills with a
// throw unless something supplies a body: a hand-owned companion's implementation part (the darwin
// precedent, internal/syscall/unix/darwin/net_darwin_impl.cs), or the converter's own forwarder for a
// whitelisted target (linknameForwardTargets in src/go2cs/visitFuncDecl.go, which emits the pull WITH
// a forwarding body, so it is no longer bodyless at all). One with neither dies at first use with
// "no implementation reached this compilation (... a linkname whose push did not arrive)".
//
// THAT IS WHAT THE 1.24.13 DARWIN RE-BASELINE MEASURED (run 36947612442, 2026-10-02): three of the
// four failing projects on both mac legs died on exactly this class -- vendor/golang.org/x/net/route's
// sysctl, internal/poll's fdopendir and internal/syscall/unix's unlinkat -- and the darwin flavor at
// 172d437e66 held 24 such pulls, 14 filled by a companion and 10 with nothing behind them.
//
// So this census holds the class: every UNFILLED darwin linkname pull must be declared dormant below,
// with the reason it is never reached. A new unfilled pull fails by name; a declared one that has
// since been filled is logged for sweeping. It reads the TRACKED tree (git ls-files) and the files on
// disk, like nativeCallGateDarwin_test.go.
//
// POSITIVE CONTROL: TestDarwinLinknamePullScannerFires drives the same scanner over a synthetic
// package with two filled pulls (companion implementation parts, one with a tuple return and its
// directive written after the declaration), one unfilled pull, and one ordinary bodyless partial with no
// linkname, of which only the unfilled pull may be reported.

// declaredDormantDarwinPulls maps "<package>.<name>" to why the unfilled pull is never reached.
var declaredDormantDarwinPulls = map[string]string{
	// The managed program's entry point is the converted main package's own Main, reached by the CLR,
	// never through runtime's linkname of main.main.
	"runtime.main_main": "the managed entry point is reached by the CLR, never through runtime.main_main",

	// x/sys/cpu's darwin AVX-512 probe. Its only importer in GOROOT, chacha20poly1305_amd64.go, is
	// built `gc && !purego`; purego is the corpus's tag, so no corpus project references
	// vendor.golang.org.x.sys.cpu and its init never runs. It wakes, with the package's own bodyless
	// cpuid/xgetbv, the day purego stops applying to that importer.
	"vendor/golang.org/x/sys/cpu.syscall_syscall6": "unreachable under purego: x/sys/cpu has no importer in the corpus",

	// os's darwin readdir is hand-owned (os/darwin/dir_darwin_impl.cs, dropped from the auto form by
	// manualConversionFuncs) and calls libc's readdir_r directly, because syscall.Dirent is not blittable
	// in the conversion. The pull's only caller is gone, so a forward would route nowhere live -- and to
	// the very struct-passing seam the companion exists to avoid.
	"os.readdir_r": "its only caller, readdir, is hand-owned in dir_darwin_impl.cs and calls libc directly",
}

// darwinPull is one bodyless darwin declaration carrying a //go:linkname pull.
type darwinPull struct {
	key    string // "<package>.<name>"
	target string // the linkname target, "pkg.symbol"
	file   string // the declaring file, relative to the repository root
}

var (
	darwinLinknameComment = regexp.MustCompile(`^\s*//go:linkname\s+([A-Za-z_][A-Za-z0-9_]*)\s+(\S+\.\S+)`)
	darwinPartialName     = regexp.MustCompile(`^\s*([A-Za-z_][A-Za-z0-9_]*)\s*(?:<[^>]*>)?\s*\(`)
)

// partialMember reads one line declaring a `partial` method: its name, and whether the part is bodyless
// (ends in `;`) rather than an implementation part (a `{` or `=>` body). ok is false for any other line.
// The return type may be a tuple, so a leading balanced `(...)` is skipped before the name is read.
func partialMember(line string) (name string, bodyless bool, ok bool) {
	at := strings.Index(line, "partial ")

	if at < 0 || strings.Contains(line[:at], "//") || strings.Contains(line, "partial class") || strings.Contains(line, "partial struct") {
		return "", false, false
	}

	head := strings.TrimSpace(line[at+len("partial "):])

	// Each token of the return type and the name, until the parameter list opens.
	for len(head) > 0 {
		if head[0] == '(' {
			depth := 0

			for i, r := range head {
				if r == '(' {
					depth++
				} else if r == ')' {
					if depth--; depth == 0 {
						head = strings.TrimSpace(head[i+1:])
						break
					}
				}
			}

			continue
		}

		if m := darwinPartialName.FindStringSubmatch(head); m != nil {
			trimmed := strings.TrimSpace(line)
			return m[1], strings.HasSuffix(trimmed, ";") && !strings.Contains(trimmed, "=>"), true
		}

		// Skip one return-type token: up to the next space outside angle brackets.
		depth, cut := 0, len(head)

		for i, r := range head {
			if r == '<' {
				depth++
			} else if r == '>' {
				depth--
			} else if r == ' ' && depth == 0 {
				cut = i
				break
			}
		}

		if cut == len(head) {
			return "", false, false
		}

		head = strings.TrimSpace(head[cut:])
	}

	return "", false, false
}

// scanDarwinLinknamePulls reports every bodyless declaration under a darwin folder that carries a
// //go:linkname pull, and the subset no implementation part fills. tracked is `git ls-files` output.
func scanDarwinLinknamePulls(t *testing.T, root string, tracked []string) (all []darwinPull, unfilled []darwinPull) {
	t.Helper()

	// Each package's .cs sources: its root and its darwin folder, where an implementation part can live.
	packageFiles := map[string][]string{}

	for _, path := range tracked {
		if !strings.HasPrefix(path, "src/core/") || !strings.HasSuffix(path, ".cs") {
			continue
		}

		rel := strings.TrimPrefix(path, "src/core/")

		if pkg, _, found := strings.Cut(rel, "/darwin/"); found {
			packageFiles[pkg] = append(packageFiles[pkg], path)
		} else if dir := filepath.ToSlash(filepath.Dir(rel)); dir != "." {
			packageFiles[dir] = append(packageFiles[dir], path)
		}
	}

	read := func(path string) []string {
		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(path)))

		if err != nil {
			t.Fatalf("reading %s: %v", path, err)
		}

		return strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n")
	}

	for pkg, files := range packageFiles {
		implemented := map[string]bool{}
		var declared []darwinPull

		for _, file := range files {
			lines := read(file)

			// A //go:linkname directive applies to its name anywhere in the file; x/sys/cpu writes it after
			// the declaration.
			pulls := map[string]string{}

			for _, line := range lines {
				if c := darwinLinknameComment.FindStringSubmatch(line); c != nil {
					pulls[c[1]] = c[2]
				}
			}

			for _, line := range lines {
				name, bodyless, ok := partialMember(line)

				if !ok {
					continue
				}

				if !bodyless {
					implemented[name] = true
				} else if target, pulled := pulls[name]; pulled && strings.Contains(file, "/darwin/") {
					declared = append(declared, darwinPull{key: pkg + "." + name, target: target, file: file})
				}
			}
		}

		for _, pull := range declared {
			all = append(all, pull)

			name := pull.key[strings.LastIndex(pull.key, ".")+1:]

			if !implemented[name] {
				unfilled = append(unfilled, pull)
			}
		}
	}

	sort.Slice(all, func(i, j int) bool { return all[i].key < all[j].key })
	sort.Slice(unfilled, func(i, j int) bool { return unfilled[i].key < unfilled[j].key })

	return all, unfilled
}

// TestDarwinLinknamePullsAreFilledOrDeclared is the guard.
func TestDarwinLinknamePullsAreFilledOrDeclared(t *testing.T) {
	root := repoRootFromPackageDir(t)
	all, unfilled := scanDarwinLinknamePulls(t, root, gitTrackedFiles(t, root))

	if len(all) < 15 {
		t.Fatalf("VACUOUS: %d darwin linkname pulls found; the scan is measuring nothing", len(all))
	}

	t.Logf("darwin linkname pulls %d · filled %d · unfilled %d · declared dormant %d",
		len(all), len(all)-len(unfilled), len(unfilled), len(declaredDormantDarwinPulls))

	measured := map[string]bool{}

	for _, pull := range unfilled {
		measured[pull.key] = true

		if _, dormant := declaredDormantDarwinPulls[pull.key]; !dormant {
			t.Errorf("UNFILLED DARWIN LINKNAME PULL: %s -> %s (%s)\n"+
				"nothing supplies its body, so it throws at first use. Give it one: a linknameForwardTargets row when the "+
				"target is ordinary converted Go authorized by a one-arg handle in its package, or a companion "+
				"implementation part. Declare it in declaredDormantDarwinPulls only with the reason it is never reached",
				pull.key, pull.target, pull.file)
		}
	}

	for key := range declaredDormantDarwinPulls {
		if !measured[key] {
			t.Logf("DECLARED DORMANT BUT NO LONGER UNFILLED, SWEEP IT: %s", key)
		}
	}
}

// TestDarwinLinknamePullScannerFires is the positive control: the same scanner over a synthetic tree.
func TestDarwinLinknamePullScannerFires(t *testing.T) {
	root := t.TempDir()

	write := func(rel, content string) {
		full := filepath.Join(root, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(full, []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	write("src/core/pkg/darwin/pulls.cs", `partial class pkg_package {
//go:linkname filled syscall.filled
internal static partial nint filled(nint fd);

//go:linkname missing syscall.missing
internal static partial (uintptr r, error err) missing(nint fd);

internal static partial (uintptr r1, Errno err) tupled(uintptr fn);

//go:linkname tupled syscall.tupled

internal static partial void plainAssembly();
}
`)
	write("src/core/pkg/darwin/pulls_impl.cs", `partial class pkg_package {
internal static partial nint filled(nint fd) => fd;

    internal static partial (uintptr r1, Errno err) tupled(uintptr fn) {
        return (fn, default!);
    }
}
`)

	all, unfilled := scanDarwinLinknamePulls(t, root, []string{"src/core/pkg/darwin/pulls.cs", "src/core/pkg/darwin/pulls_impl.cs"})

	if len(all) != 3 || len(unfilled) != 1 || unfilled[0].key != "pkg.missing" || unfilled[0].target != "syscall.missing" {
		t.Fatalf("scanner: all %v, unfilled %v; want 3 pulls and exactly pkg.missing -> syscall.missing unfilled", all, unfilled)
	}
}
