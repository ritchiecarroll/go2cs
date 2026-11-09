// suppressGCTransitionSeam_test.go - Gbtc
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

// THE RAW CLASS NEVER ENTERS THE NATIVE-CALL SEAM (docs/phase4/DESIGN-native-call-seam.md §7.1, increment S-d).
//
// A P/Invoke bound with [SuppressGCTransition] keeps its thread in COOPERATIVE GC mode for the call: the GC cannot
// suspend that thread until the call returns. The transition seam, GoNativeCall, is where an M:N scheduler releases
// and reacquires a P, and its exit's slow path may PARK THE THREAD until a P frees (§6.2). A suppressed call made
// inside an open transition would therefore park a thread the GC cannot suspend, which deadlocks GC suspension
// process-wide. The design's rule: a suppressed binding is Go's RawSyscall class, short, non-blocking and
// callback-free, and it never takes the seam at all.
//
// That is a rule about CALL SITES, so this guard reads them. For every [SuppressGCTransition] binding in product code
// (src/core) it finds each call of that binding within the binding's REACH, and resolves the call's ENCLOSING MEMBER:
// the block directly under the innermost type, or, for an expression-bodied member, its statement. The member must not
// open the seam itself.
//
//   - REACH is the binding's declaring type, not its folder. Layout L3 spreads one package's partial class over
//     per-GOOS folders (syscall/windows/*.cs is the same syscall_package as syscall/*.cs). An unqualified call is in
//     reach inside any declaration of that type, or in a file with a `using static` of it. A qualified call,
//     `Type.name(`, is in reach from anywhere. A member access on any other expression (`x.name(`) is not the binding.
//   - The seam is opened by GoNativeCall.Enter / ReturnToNative, the runtime family's entersyscall /
//     entersyscallblock / reentersyscall, and the syscall package's syscallEntersyscall / syscallEntersyscallblock
//     forwarders (S-b).
//   - Comments and string or char literals are blanked before any of this is read, so a binding NAMED in prose is
//     neither a site nor a call.
//
// WHAT IT DOES NOT SEE, stated so a green is not over-read: a suppressed call reached through a SEPARATE method that
// an open transition calls (a helper called between Enter and Exit). The rule's text forbids that too. A call-graph
// walk would be the instrument for it, and none exists. The census at the guard's birth was ZERO suppressed bindings
// in product code, so the rule's first site will be read here, in its own member, by whoever adds it.
//
// Asserted: no finding. Printed, asserted by nothing: the census of suppressed bindings.

var (
	suppressGCTransitionToken = regexp.MustCompile(`\bSuppressGCTransition(Attribute)?\b`)
	seamEntryToken            = regexp.MustCompile(`\b(GoNativeCall\s*\.\s*(Enter|ReturnToNative)|syscallEntersyscall(block)?|entersyscall(block)?|reentersyscall)\s*\(`)
	csIdentifier              = regexp.MustCompile(`[A-Za-z_\p{L}][A-Za-z0-9_\p{L}\p{N}]*`)
	// A type declaration: the keyword, then the type's name. `record struct` and `record class` name the type after
	// the second keyword.
	typeDeclaration = regexp.MustCompile(`\b(?:record\s+)?(class|struct|record|interface)\s+([A-Za-z_\p{L}][A-Za-z0-9_\p{L}\p{N}]*)`)
)

type suppressedBinding struct {
	file     string
	name     string
	typeName string
}

type suppressedSeamFinding struct {
	binding suppressedBinding
	caller  string
	line    int
}

// blankCSharpTrivia returns text with every comment and string or char literal replaced by spaces, newlines kept,
// so offsets and line numbers survive and nothing inside trivia can be read as code.
func blankCSharpTrivia(text string) string {
	b := []byte(text)
	out := make([]byte, len(b))
	copy(out, b)

	blank := func(from, to int) {
		for i := from; i < to && i < len(out); i++ {
			if out[i] != '\n' && out[i] != '\r' {
				out[i] = ' '
			}
		}
	}

	for i := 0; i < len(b); {
		switch {
		case b[i] == '/' && i+1 < len(b) && b[i+1] == '/':
			j := i
			for j < len(b) && b[j] != '\n' {
				j++
			}
			blank(i, j)
			i = j
		case b[i] == '/' && i+1 < len(b) && b[i+1] == '*':
			j := i + 2
			for j+1 < len(b) && !(b[j] == '*' && b[j+1] == '/') {
				j++
			}
			j += 2
			blank(i, j)
			i = j
		case b[i] == '"' && i+2 < len(b) && b[i+1] == '"' && b[i+2] == '"':
			j := i + 3
			for j+2 < len(b) && !(b[j] == '"' && b[j+1] == '"' && b[j+2] == '"') {
				j++
			}
			j += 3
			blank(i, j)
			i = j
		case b[i] == '"':
			verbatim := i > 0 && (b[i-1] == '@' || (b[i-1] == '$' && i > 1 && b[i-2] == '@'))
			j := i + 1
			for j < len(b) {
				if verbatim && b[j] == '"' && j+1 < len(b) && b[j+1] == '"' {
					j += 2
					continue
				}
				if !verbatim && b[j] == '\\' {
					j += 2
					continue
				}
				if b[j] == '"' || (!verbatim && b[j] == '\n') {
					break
				}
				j++
			}
			j++
			blank(i, j)
			i = j
		case b[i] == '\'':
			j := i + 1
			for j < len(b) && b[j] != '\'' && b[j] != '\n' {
				if b[j] == '\\' {
					j++
				}
				j++
			}
			j++
			blank(i, j)
			i = j
		default:
			i++
		}
	}

	return string(out)
}

// typeHeaderName returns the declared type's name when header (the text before a '{') declares a type, or "". A
// generic constraint's keyword (`where T : class`, `where U : class, new()`) follows ':' or ',' and declares nothing.
func typeHeaderName(header string) string {
	for _, m := range typeDeclaration.FindAllStringSubmatchIndex(header, -1) {
		prev := strings.TrimRight(header[:m[0]], " \t\r\n")
		if strings.HasSuffix(prev, ":") || strings.HasSuffix(prev, ",") {
			continue
		}
		return header[m[4]:m[5]]
	}

	return ""
}

// enclosingTypeNames returns the names of the type blocks that contain offset, outermost first.
func enclosingTypeNames(code string, offset int) []string {
	var stack []string
	stmtStart := 0

	for i := 0; i < offset && i < len(code); i++ {
		switch code[i] {
		case '{':
			stack = append(stack, typeHeaderName(code[stmtStart:i]))
			stmtStart = i + 1
		case '}':
			if len(stack) > 0 {
				stack = stack[:len(stack)-1]
			}
			stmtStart = i + 1
		case ';':
			stmtStart = i + 1
		}
	}

	var names []string
	for _, name := range stack {
		if name != "" {
			names = append(names, name)
		}
	}

	return names
}

// sgtEnclosingMember returns the [start, end) span of the member that contains offset: the block directly under the
// innermost type block, or, where the member is expression-bodied, the statement around offset at type-block depth.
func sgtEnclosingMember(code string, offset int) (int, int) {
	type open struct {
		pos    int
		isType bool
	}

	var stack []open
	stmtStart := 0

	for i := 0; i < len(code); i++ {
		switch code[i] {
		case '{':
			stack = append(stack, open{pos: i, isType: typeHeaderName(code[stmtStart:i]) != ""})
			stmtStart = i + 1
		case '}':
			if len(stack) > 0 {
				top := stack[len(stack)-1]
				stack = stack[:len(stack)-1]
				if top.pos < offset && offset < i && len(stack) > 0 && stack[len(stack)-1].isType && !top.isType {
					// A member body: walk back to its header's start (the previous ';', '{' or '}').
					h := strings.LastIndexAny(code[:top.pos], ";{}")
					return h + 1, i + 1
				}
			}
			stmtStart = i + 1
		case ';':
			if offset < i && offset >= stmtStart && len(stack) > 0 && stack[len(stack)-1].isType {
				return stmtStart, i + 1
			}
			stmtStart = i + 1
		}
	}

	return 0, len(code)
}

// suppressedBindingsIn returns the [SuppressGCTransition] bindings declared in code (blanked), each with the name of
// the type that declares it.
func suppressedBindingsIn(file, code string) []suppressedBinding {
	var bindings []suppressedBinding

	for _, loc := range suppressGCTransitionToken.FindAllStringIndex(code, -1) {
		// The attribute section ends at the next ']'; further sections may follow before the declaration.
		i := strings.IndexByte(code[loc[1]:], ']')
		if i < 0 {
			continue
		}
		i += loc[1] + 1

		for {
			rest := strings.TrimLeft(code[i:], " \t\r\n")
			if !strings.HasPrefix(rest, "[") {
				break
			}
			i = len(code) - len(rest)
			j := strings.IndexByte(code[i:], ']')
			if j < 0 {
				break
			}
			i += j + 1
		}

		paren := strings.IndexByte(code[i:], '(')
		if paren < 0 {
			continue
		}

		ids := csIdentifier.FindAllString(code[i:i+paren], -1)
		types := enclosingTypeNames(code, loc[0])
		if len(ids) == 0 || len(types) == 0 {
			continue
		}

		bindings = append(bindings, suppressedBinding{file: file, name: ids[len(ids)-1], typeName: types[len(types)-1]})
	}

	return bindings
}

// inReach reports whether the call matched at loc (a submatch index with an optional qualifier group) can be a call
// of binding.
func inReach(code string, loc []int, binding suppressedBinding, usingStatic *regexp.Regexp) bool {
	if loc[2] >= 0 {
		return code[loc[2]:loc[3]] == binding.typeName
	}

	// `expr.name(` is a member access on something other than the declaring type.
	if strings.HasSuffix(strings.TrimRight(code[:loc[0]], " \t\r\n"), ".") {
		return false
	}

	for _, name := range enclosingTypeNames(code, loc[0]) {
		if name == binding.typeName {
			return true
		}
	}

	return usingStatic.MatchString(code)
}

// scanSuppressedSeamEntries reads files (slash path -> content) and returns the suppressed bindings and every call of
// one, within its reach, whose enclosing member opens the seam.
func scanSuppressedSeamEntries(files map[string]string) ([]suppressedBinding, []suppressedSeamFinding) {
	code := map[string]string{}
	paths := make([]string, 0, len(files))

	for p, text := range files {
		code[p] = blankCSharpTrivia(text)
		paths = append(paths, p)
	}

	sort.Strings(paths)

	var bindings []suppressedBinding
	var findings []suppressedSeamFinding

	for _, p := range paths {
		bindings = append(bindings, suppressedBindingsIn(p, code[p])...)
	}

	for _, binding := range bindings {
		call := regexp.MustCompile(`(?:([A-Za-z_\p{L}][A-Za-z0-9_\p{L}\p{N}]*)\s*\.\s*)?\b` + regexp.QuoteMeta(binding.name) + `\s*\(`)
		usingStatic := regexp.MustCompile(`\busing\s+static\s+(?:global::)?(?:[A-Za-z_\p{L}][A-Za-z0-9_\p{L}\p{N}]*\s*\.\s*)*` + regexp.QuoteMeta(binding.typeName) + `\s*;`)

		for _, q := range paths {
			text := code[q]

			for _, loc := range call.FindAllStringSubmatchIndex(text, -1) {
				if !inReach(text, loc, binding, usingStatic) {
					continue
				}

				start, end := sgtEnclosingMember(text, loc[0])
				member := text[start:end]

				// The binding's own declaration is not a call.
				if suppressGCTransitionToken.MatchString(member) {
					continue
				}

				if seamEntryToken.MatchString(member) {
					findings = append(findings, suppressedSeamFinding{binding: binding, caller: q, line: strings.Count(text[:loc[0]], "\n") + 1})
				}
			}
		}
	}

	return bindings, findings
}

func TestSuppressGCTransitionBindingsNeverEnterTheNativeSeam(t *testing.T) {
	root := repoRootFromPackageDir(t)
	files := map[string]string{}

	for _, rel := range gitTrackedFiles(t, root) {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))
		if err != nil {
			t.Fatalf("read %s: %v", rel, err)
		}

		files[rel] = string(data)
	}

	if len(files) == 0 {
		t.Fatal("no product .cs file was read: the scan measured nothing")
	}

	bindings, findings := scanSuppressedSeamEntries(files)

	t.Logf("READING: %d product .cs files scanned; %d [SuppressGCTransition] binding(s) in product code", len(files), len(bindings))

	for _, b := range bindings {
		t.Logf("  binding %s.%s in %s", b.typeName, b.name, b.file)
	}

	for _, f := range findings {
		t.Errorf("%s:%d calls [SuppressGCTransition] %s.%s (declared in %s) inside a member that opens the native-call seam: a suppressed call keeps its thread in cooperative GC mode, and the seam's exit may park that thread (DESIGN-native-call-seam §7.1). Make the call outside the transition, or drop the attribute.",
			f.caller, f.line, f.binding.typeName, f.binding.name, f.binding.file)
	}
}

// The controls, on fixtures, because the corpus has no suppressed binding to make the guard fail on.
func TestSuppressGCTransitionSeamGuardReadsItsShapes(t *testing.T) {
	const binding = `namespace go;
partial class p_package {
    [LibraryImport("libc", EntryPoint = "getpid")]
    [SuppressGCTransition]
    private static partial int libcGetpid();

    // libcGetpid() named in a comment is no call, and "GoNativeCall.Enter( libcGetpid(" in a string opens nothing.
    private static readonly string s_note = "GoNativeCall.Enter( libcGetpid(";
`

	cases := []struct {
		name string
		body string
		want int
	}{
		{"a planted call inside an open transition", `
    internal static int Planted() {
        GoNativeCall.Enter(NativeCallHint.MayBlock);
        try { return libcGetpid(); }
        finally { GoNativeCall.Exit(); }
    }
}`, 1},
		{"a call through the runtime's forwarder", `
    internal static int ViaForwarder() {
        runtime_package.syscallEntersyscall();
        try { return libcGetpid(); }
        finally { runtime_package.syscallExitsyscall(); }
    }
}`, 1},
		{"a generic caller whose header reads `where T : class`, the seam on another statement", `
    internal static int Generic<T>() where T : class {
        GoNativeCall.Enter(NativeCallHint.MayBlock);
        int r = 0;
        try { r = libcGetpid(); } finally { GoNativeCall.Exit(); }
        return r;
    }
}`, 1},
		{"a generic caller with two constraint clauses", `
    internal static int Generic2<T, U>() where T : class where U : struct {
        GoNativeCall.Enter(NativeCallHint.MayBlock);
        try { return libcGetpid(); } finally { GoNativeCall.Exit(); }
    }
}`, 1},
		{"a call inside a lambda inside a member that enters", `
    internal static int InLambda() {
        GoNativeCall.Enter(NativeCallHint.MayBlock);
        try { Func<int> f = () => libcGetpid(); return f(); }
        finally { GoNativeCall.Exit(); }
    }
}`, 1},
		{"an expression-bodied caller that opens nothing", `
    internal static int Clean() => libcGetpid();
}`, 0},
		{"a block caller that opens nothing, beside a member that does", `
    internal static int Clean() { return libcGetpid(); }
    internal static void Other() { GoNativeCall.Enter(NativeCallHint.MayBlock); GoNativeCall.Exit(); }
}`, 0},
	}

	for _, c := range cases {
		files := map[string]string{"src/core/p/p_impl.cs": binding + c.body}
		bindings, findings := scanSuppressedSeamEntries(files)

		if len(bindings) != 1 || bindings[0].name != "libcGetpid" || bindings[0].typeName != "p_package" {
			t.Errorf("%s: read %+v, want the one binding p_package.libcGetpid", c.name, bindings)
		}

		if len(findings) != c.want {
			t.Errorf("%s: %d finding(s), want %d: %+v", c.name, len(findings), c.want, findings)
		}
	}

	// REACH, over several files: the attribute-list spelling declares the binding; every file below has a member that
	// opens the seam and calls something named win32Ticks.
	enters := func(call string) string {
		return `
    internal static ulong Caller() {
        GoNativeCall.Enter(NativeCallHint.Blocks);
        try { return ` + call + `; } finally { GoNativeCall.Exit(); }
    }
}`
	}

	files := map[string]string{
		"src/core/q/a_impl.cs": `namespace go;
partial class q_package {
    [LibraryImport("kernel32.dll", EntryPoint = "GetTickCount64"), SuppressGCTransition]
    private static partial ulong win32Ticks();
}`,
		// IN reach: a sibling file of the same type.
		"src/core/q/b_impl.cs": "namespace go;\npartial class q_package {" + enters("win32Ticks()"),
		// IN reach: the same partial type in a per-GOOS folder (layout L3).
		"src/core/q/windows/c_impl.cs": "namespace go;\npartial class q_package {" + enters("win32Ticks()"),
		// IN reach: qualified by the declaring type, from another type.
		"src/core/s/d_impl.cs": "namespace go;\npartial class s_package {" + enters("go.q_package.win32Ticks()"),
		// IN reach: unqualified, under a using static of the declaring type.
		"src/core/t/e_impl.cs": "using static go.q_package;\nnamespace go;\npartial class t_package {" + enters("win32Ticks()"),
		// OUT of reach: another type, unqualified, no using static.
		"src/core/r/f_impl.cs": "namespace go;\npartial class r_package {" + enters("win32Ticks()"),
		// OUT of reach: a member access on some other expression.
		"src/core/u/g_impl.cs": "namespace go;\npartial class u_package {" + enters("s_clock.win32Ticks()"),
		// OUT of reach: qualified by another type.
		"src/core/v/h_impl.cs": "namespace go;\npartial class v_package {" + enters("r_package.win32Ticks()"),
	}

	bindings, findings := scanSuppressedSeamEntries(files)

	if len(bindings) != 1 || bindings[0].name != "win32Ticks" || bindings[0].typeName != "q_package" {
		t.Errorf("the attribute-list spelling: read %+v, want the one binding q_package.win32Ticks", bindings)
	}

	var got []string
	for _, f := range findings {
		got = append(got, f.caller)
	}
	sort.Strings(got)

	want := []string{"src/core/q/b_impl.cs", "src/core/q/windows/c_impl.cs", "src/core/s/d_impl.cs", "src/core/t/e_impl.cs"}
	if strings.Join(got, "|") != strings.Join(want, "|") {
		t.Errorf("reach: findings in %v, want exactly %v", got, want)
	}
}
