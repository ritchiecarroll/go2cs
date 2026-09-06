package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// The `wsaSendtoInet4`/`wsaSendtoInet6` DEAD-CODE guard, and its whole value is the day it goes red.
//
// WHY IT EXISTS. WSASendto was registered in manualConversionFuncs on 2026-09-06 and its two
// Inet4/Inet6 siblings deliberately were NOT, on a MEASUREMENT rather than a deferral: they have no
// call site anywhere in the corpus, production or test emission, because Go's own callers of them
// are the `//go:linkname` PULL in internal/syscall/windows and this corpus answers that pull at the
// DECLARATION site (internal/syscall/windows/windows/net_windows_impl.cs). Displacing them would
// cost a registration, a placeholder and a body apiece to change the behaviour of code nothing
// reaches. That is a claim about today's corpus, and the thing that would silently falsify it is a
// converter change emitting a forwarding property for the linkname pull -- at which point both
// bodies become LIVE carrying all four of the defects WSASendto was hand-owned to fix, with nothing
// pointing at them. docs/phase4/DESIGN-windows-udp-send.md section 7.
//
// HOW IT IS WRITTEN, because a text-grepping guard is a family with known ways of going vacuous.
// It does NOT try to tell a declaration from a call by shape -- that is fragile. It asserts the
// SHAPE OF THE POPULATION instead: comment-stripped occurrences of either name appear in exactly
// one corpus file, syscall/windows/syscall_windows.cs, exactly once each, which is the declaration.
// A new file, or a second occurrence in that file, is a caller. And the count dropping to zero is
// ALSO a failure rather than a pass, because that means the guard's subject moved -- displaced,
// renamed, or removed -- and the reasoning above has to be re-read rather than silently retired.
//
// The scan itself is exercised by TestWsaSendtoScanFindsAnInjectedCall below, over a synthetic
// corpus, so the detector is proven able to fire without anyone disturbing the real one.

var wsaSendtoDeadNames = []string{"wsaSendtoInet4", "wsaSendtoInet6"}

// The one file allowed to mention them, as a slash-separated path relative to src/core.
const wsaSendtoDeclaringFile = "syscall/windows/syscall_windows.cs"

func TestWsaSendtoInetVariantsHaveNoCallers(t *testing.T) {
	coreDir := filepath.Join("..", "core")

	if _, err := os.Stat(coreDir); err != nil {
		t.Skip("src/core is not beside the converter; nothing to walk")
	}

	counts, err := scanCorpusForNames(coreDir, wsaSendtoDeadNames)

	if err != nil {
		t.Fatalf("walking src/core: %v", err)
	}

	for _, name := range wsaSendtoDeadNames {
		perFile := counts[name]

		// Vacuity: the guard must be able to see its own subject.
		if perFile[wsaSendtoDeclaringFile] == 0 {
			t.Errorf("%s: no occurrence in src/core/%s -- the guard's subject moved (displaced, "+
				"renamed or removed). Re-read docs/phase4/DESIGN-windows-udp-send.md section 7 and "+
				"either retire this guard with its reason or point it at the new spelling; do NOT "+
				"delete it silently.", name, wsaSendtoDeclaringFile)
			continue
		}

		if perFile[wsaSendtoDeclaringFile] != 1 {
			t.Errorf("%s: %d occurrences in src/core/%s, want exactly 1 (the declaration). A second "+
				"one is a caller in the declaring file itself, which makes the body LIVE with the "+
				"four defects WSASendto was hand-owned to fix.",
				name, perFile[wsaSendtoDeclaringFile], wsaSendtoDeclaringFile)
		}

		for file, n := range perFile {
			if file == wsaSendtoDeclaringFile {
				continue
			}

			t.Errorf("%s: %d occurrence(s) in src/core/%s -- this name was measured DEAD when "+
				"WSASendto was hand-owned, and a caller makes its generated body live with the "+
				"managed WSABuf, the managed sockaddr image and the unpinnable bytes-sent slot all "+
				"intact. Register it beside WSASendto, or explain why the caller is safe.",
				name, n, file)
		}
	}
}

// TestWsaSendtoScanFindsAnInjectedCall is the detector's positive control: a synthetic corpus
// carrying a declaration, a CALL, and two decoys that must NOT count -- the name inside a line
// comment and inside a block comment, which is exactly how the real corpus's companion headers
// mention it. Without this arm the guard above could be green because it finds nothing at all.
func TestWsaSendtoScanFindsAnInjectedCall(t *testing.T) {
	root := t.TempDir()
	pkg := filepath.Join(root, "syscall", "windows")

	if err := os.MkdirAll(pkg, 0o755); err != nil {
		t.Fatalf("staging: %v", err)
	}

	declaring := "internal static error /*err*/ wsaSendtoInet4(ΔHandle s) {\n" +
		"    // wsaSendtoInet4 is named here in prose and must NOT count.\n" +
		"    /* and wsaSendtoInet4 here too, in a block comment. */\n" +
		"    return default!;\n}\n"

	if err := os.WriteFile(filepath.Join(pkg, "syscall_windows.cs"), []byte(declaring), 0o644); err != nil {
		t.Fatalf("staging declaration: %v", err)
	}

	caller := filepath.Join(root, "internal", "poll", "windows")

	if err := os.MkdirAll(caller, 0o755); err != nil {
		t.Fatalf("staging: %v", err)
	}

	if err := os.WriteFile(filepath.Join(caller, "fd_windows.cs"),
		[]byte("var err = Δsyscall.wsaSendtoInet4(fd);\n"), 0o644); err != nil {
		t.Fatalf("staging caller: %v", err)
	}

	counts, err := scanCorpusForNames(root, wsaSendtoDeadNames)

	if err != nil {
		t.Fatalf("scanning: %v", err)
	}

	perFile := counts["wsaSendtoInet4"]

	if got := perFile[wsaSendtoDeclaringFile]; got != 1 {
		t.Errorf("declaration: counted %d, want 1 (the two comment decoys must not count)", got)
	}

	if got := perFile["internal/poll/windows/fd_windows.cs"]; got != 1 {
		t.Errorf("injected caller: counted %d, want 1 -- the detector cannot see a call, so the "+
			"guard above proves nothing", got)
	}

	if got := len(counts["wsaSendtoInet6"]); got != 0 {
		t.Errorf("wsaSendtoInet6: counted occurrences in %d file(s), want 0 (negative control)", got)
	}
}

// scanCorpusForNames counts comment-stripped, whole-token occurrences of each name in every `.cs`
// file under root, keyed by name and then by slash-separated path relative to root.
func scanCorpusForNames(root string, names []string) (map[string]map[string]int, error) {
	counts := make(map[string]map[string]int, len(names))

	for _, name := range names {
		counts[name] = make(map[string]int)
	}

	err := filepath.Walk(root, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		if info.IsDir() {
			// Build output is not the corpus.
			switch info.Name() {
			case "bin", "obj", "Generated":
				return filepath.SkipDir
			}

			return nil
		}

		if !strings.HasSuffix(info.Name(), ".cs") {
			return nil
		}

		content, err := os.ReadFile(path)

		if err != nil {
			return err
		}

		// The converter's OWN comment lexer, reused rather than re-derived: it already carries the
		// ordering hazard (a `/*` inside a `//` opens nothing) that a second copy would have to
		// re-learn, and three derivations of one predicate drift.
		var stripped strings.Builder
		inBlockComment := false

		for _, line := range strings.Split(string(content), "\n") {
			stripped.WriteString(stripCSharpComments(line, &inBlockComment))
			stripped.WriteByte('\n')
		}

		relative, err := filepath.Rel(root, path)

		if err != nil {
			return err
		}

		relative = filepath.ToSlash(relative)

		code := stripped.String()

		for _, name := range names {
			if n := countCallTokens(code, name); n > 0 {
				counts[name][relative] = n
			}
		}

		return nil
	})

	return counts, err
}

// countCallTokens counts occurrences of name followed by '(' where the character before is not part
// of an identifier -- so `Δsyscall.wsaSendtoInet4(` counts and a longer identifier ending in the
// name does not. Deliberately does NOT distinguish a declaration from a call: the guard asserts the
// shape of the whole population instead, which is the check a shape heuristic cannot get wrong.
func countCallTokens(text string, name string) int {
	count, from := 0, 0

	for {
		at := strings.Index(text[from:], name)

		if at < 0 {
			return count
		}

		at += from
		from = at + len(name)

		if at > 0 && isIdentifierByte(text[at-1]) {
			continue
		}

		rest := strings.TrimLeft(text[from:], " \t\r\n")

		if strings.HasPrefix(rest, "(") {
			count++
		}
	}
}

func isIdentifierByte(b byte) bool {
	return b == '_' || b >= 'a' && b <= 'z' || b >= 'A' && b <= 'Z' || b >= '0' && b <= '9' || b >= 0x80
}
