// elidedConstraints_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// RED 8 (d): the elided-constraint ratchet.
//
// Before RED 8 (a), a generic whose constraint is a method-set interface over a union of pointer-to-
// named terms lost that constraint at emission: the declaration kept `new()` and the real constraint
// went into a COMMENT, `where P : /* Point[P] */ new()`. The C# that results compiles and accepts
// type arguments Go would reject, so the loss is silent at every gate the corpus runs. RED 8 (a)
// renders those constraints for real; this guard holds the result.
//
// THE POPULATION BEFORE THE CURE, on the record from three lanes and four instruments (C1's textual
// census and the converter's own :1204 diagnostic at 8e1eafae30, C2's constraint-side reading at
// 57dd991807 and its three-target arm at 1a95944e50, G's declaration split and seed control at
// 61f00a0ace and d4cb0939e1): EXACTLY 29 sites -- crypto/internal/fips140/ecdsa 17,
// crypto/internal/fips140/ecdh 6, crypto/ecdsa 6 -- spelled 23 bare `Point[P]` and 6 qualified
// `ecdsa.Point[P]`, identical on windows, linux and darwin.
//
// WHAT THIS GUARD IS, stated so it is not read as more: a RATCHET. It holds that the elision does not
// come back. It does not prove RED 8 (a) cured anything -- that is (a)'s own A/B and i9's gate build.
//
// THREE FORMS, ONE GATED (the third, the methodless-union breadcrumb, is stated at methodlessUnionPattern;
// it is REPORTED, never gated). The converter has a SECOND, legitimate erasure for pointer-core constraints
// (`/* where P : *T (erased: P renders as ж<T>) */`), which moves the whole clause into a comment
// rather than reducing it to `new()`. That form is REPORTED here and NOT gated: it is a deliberate
// emission with its own precedent. Reporting it is what gives the guard's zero for that form a
// meaning -- C1 read 0 for it at 8e1eafae30 and said the zero was WEAK because the predicate had
// never been shown to fire. TestElidedConstraintScannerFiresOnBothForms plants both and requires
// both to be seen, which is what retires that weakness (COORD's addition, 429f995).
//
// AND A KNOWN NEGATIVE FROM THE REAL CORPUS, the second control COORD's v2 lesson requires
// (6a3706de6c): a plant proves a predicate CAN fire, not that it is the RULE. At the version tip 144
// lines carry a commented constraint -- `where X : /* … */` -- and only 29 are the elision. The other
// 115 are the converter's ordinary breadcrumb, a Go constraint rendered as a comment with the REAL C#
// clause after it (`where E : /* cmp.Ordered */ IAdditionOperators<E, E, E>, …`). The rule is what
// FOLLOWS the comment: `new()` and nothing else. The control plants one of those 115 shapes and
// requires it NOT to match, so the guard is shown to discriminate against the near-miss class the
// corpus actually contains rather than only against a clean line.

// elidedConstraintPattern is the GATED form: a type-parameter constraint reduced to `new()` with the
// real constraint left in a comment beside it.
// `.*?` and not `[^*]*`, for the reason the erased form's predicate taught an hour after this one was
// written: a class excluding `*` cannot cross a pointer type inside the comment. MEASURED at the
// version tip before the change: 0 of the 29 gated matches carry an asterisk inside their comment, so
// the two spellings read the same population TODAY and the non-greedy one cannot start hiding members
// tomorrow.
var elidedConstraintPattern = regexp.MustCompile(`where\s+[A-Za-z_][A-Za-z0-9_]*\s*:\s*/\*.*?\*/\s*new\(\)`)

// erasedConstraintPattern is the REPORTED form: the pointer-core erasure, whole clause commented.
// ⚠ `[^*]*` CANNOT BE USED HERE, and the control is what found that out. The erased clause's own
// constraint text is a Go POINTER type -- `*nistec.P256Point` -- so a character class excluding `*`
// stops dead at the asterisk and the predicate never matches. The first run of the plant COORD added
// refused the pattern on its first use: the zero this column read against the corpus was not merely
// unproven, it came from a predicate that could not have fired. Non-greedy `.*?` crosses the
// asterisk and still stops at the first `*/`, and `.` does not cross a newline, so the match stays
// on one line.
var erasedConstraintPattern = regexp.MustCompile(`/\*\s*where\s+[A-Za-z_][A-Za-z0-9_]*\s*:.*?\(erased:.*?\)\s*\*/`)

// methodlessUnionPattern is the SECOND REPORTED form: a gated-form line whose breadcrumb is an inline
// union of two or more type terms. Go forbids a multi-term union to contain an interface that
// specifies methods, so no method set existed for `new()` to elide. That `new()` is the converter's
// own pinned methodless-union answer (constraintProxyPointerUnion_test.go, the `valueKeyCurve`
// control), not the RED 8 loss. A union of struct types has no C# constraint to render, where
// `Point[P]` had one.
// First corpus member, TRAIN K (2026-10-01): runtime/pprof's committed test source, at
// testProfileRecordNullPadding[T runtime.StackRecord | runtime.MemProfileRecord | runtime.BlockProfileRecord].
// The term grammar admits no braces, parentheses, semicolons or pointer stars. An inline interface
// that carries methods, and a pointer term (whose methodless union emits no constraint at all, so it
// can never reach `new()` legitimately), therefore stay in the gated column.
var methodlessUnionPattern = regexp.MustCompile(`where\s+[A-Za-z_][A-Za-z0-9_]*\s*:\s*/\*\s*` + unionTerm + `(?:\s*\|\s*` + unionTerm + `)+\s*\*/\s*new\(\)`)

const unionTerm = `~?[A-Za-z_][A-Za-z0-9_.]*(?:\[[A-Za-z0-9_., \[\]]*\])?`

// declaredElidedConstraints is the declared set: "<path relative to src/core>:<line>". It SHRINKS as
// seats land and it NEVER GROWS -- a new elided constraint is a converter defect, fixed at the
// converter, never admitted here. Empty once RED 8 (a) is at the tip.
var declaredElidedConstraints = []string{}

type elidedScan struct {
	elided []string // "<path>:<line>" for the gated form
	erased []string // "<path>:<line>" for the reported form
	union  []string // "<path>:<line>" for the methodless-union breadcrumb (reported)
	files  int
}

// scanElidedConstraints walks the tracked .cs under src/core and reports both forms. tracked is
// `git ls-files` output (forward slashes, relative to root).
func scanElidedConstraints(t *testing.T, root string, tracked []string) elidedScan {
	t.Helper()

	var scan elidedScan

	for _, p := range tracked {
		rest, ok := strings.CutPrefix(p, "src/core/")

		if !ok || !strings.HasSuffix(p, ".cs") {
			continue
		}

		body, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))

		if err != nil {
			t.Fatalf("read %s: %v", p, err)
		}

		scan.files++

		for i, line := range strings.Split(strings.ReplaceAll(string(body), "\r\n", "\n"), "\n") {
			if elidedConstraintPattern.MatchString(line) {
				if methodlessUnionPattern.MatchString(line) {
					scan.union = append(scan.union, fmt.Sprintf("%s:%d", rest, i+1))
				} else {
					scan.elided = append(scan.elided, fmt.Sprintf("%s:%d", rest, i+1))
				}
			}

			if erasedConstraintPattern.MatchString(line) {
				scan.erased = append(scan.erased, fmt.Sprintf("%s:%d", rest, i+1))
			}
		}
	}

	sort.Strings(scan.elided)
	sort.Strings(scan.erased)
	sort.Strings(scan.union)

	return scan
}

// TestNoElidedConstraintsInCommittedSources is the guard.
func TestNoElidedConstraintsInCommittedSources(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("VACUOUS: git ls-files reported %d tracked files; the guard cannot have scanned the corpus", len(tracked))
	}

	scan := scanElidedConstraints(t, root, tracked)

	if scan.files < 1000 {
		t.Fatalf("VACUOUS: scanned %d .cs under src/core; the corpus is larger than that", scan.files)
	}

	declared := map[string]bool{}

	for _, row := range declaredElidedConstraints {
		declared[row] = true
	}

	var undeclared []string

	for _, row := range scan.elided {
		if !declared[row] {
			undeclared = append(undeclared, row)
		}
	}

	if len(undeclared) != 0 {
		t.Errorf("%d elided constraint(s) outside the declared set. A constraint reduced to `new()` with its real "+
			"clause in a comment accepts type arguments Go rejects, and compiles, so no other gate sees it. Fix it "+
			"at the converter; never add a row here:\n  %s", len(undeclared), strings.Join(undeclared, "\n  "))
	}

	for _, row := range declaredElidedConstraints {
		found := false

		for _, got := range scan.elided {
			if got == row {
				found = true
				break
			}
		}

		if !found {
			t.Errorf("declared row %q is no longer present; delete it from declaredElidedConstraints in this commit", row)
		}
	}

	t.Logf("scanned %d .cs · declared %d · measured %d · pointer-core erasures reported (not gated) %d · "+
		"methodless-union breadcrumbs reported (not gated) %d: %s",
		scan.files, len(declaredElidedConstraints), len(scan.elided), len(scan.erased), len(scan.union),
		strings.Join(scan.union, ", "))
}

// TestElidedConstraintScannerFiresOnBothForms is the positive control, and it plants BOTH forms.
//
// The second plant is the one COORD added: the pointer-core erasure's own spelling, so the zero the
// reported column carries against the corpus is a zero from a predicate SHOWN TO FIRE rather than one
// that has never matched anything.
func TestElidedConstraintScannerFiresOnBothForms(t *testing.T) {
	root := t.TempDir()

	write := func(rel, body string) {
		full := filepath.Join(root, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatalf("mkdir for %s: %v", rel, err)
		}

		if err := os.WriteFile(full, []byte(body), 0o644); err != nil {
			t.Fatalf("write %s: %v", rel, err)
		}
	}

	write("src/core/planted/elided.cs", "public static void f<P>()\n    where P : /* Point[P] */ new()\n{\n}\n")
	write("src/core/planted/qualified.cs", "    where P : /* ecdsa.Point[P] */ new()\n")
	write("src/core/planted/erased.cs", "    /* where P : *nistec.P256Point (erased: P renders as ж<nistec.P256Point>) */\n")
	write("src/core/planted/clean.cs", "public static void g<P>()\n    where P : Point<P>\n{\n}\n")
	// The KNOWN NEGATIVE, in the shape of the 115 real corpus lines that share the gated form's
	// prefix: a commented constraint followed by a REAL C# clause. It must NOT match.
	write("src/core/planted/nearmiss.cs", "    where E : /* cmp.Ordered */ IAdditionOperators<E, E, E>, IEqualityOperators<E, E, bool>\n")
	write("src/notcore/planted/elided.cs", "    where P : /* Point[P] */ new()\n")
	// The methodless-union breadcrumb, in the corpus member's own spelling and with approximation terms:
	// REPORTED, never gated.
	write("src/core/planted/union.cs", "    where T : /* runtime.StackRecord | runtime.MemProfileRecord | runtime.BlockProfileRecord */ new()\n")
	write("src/core/planted/tilde.cs", "    where T : /* ~int | ~string */ new()\n")
	// Its two near-misses stay GATED: an inline interface that carries methods beside its union, and a
	// pointer term. Without them the union arm would pass for a change that admits every `|`.
	write("src/core/planted/unionmethods.cs", "    where T : /* interface{ ~int | ~string; String() string } */ new()\n")
	write("src/core/planted/unionptr.cs", "    where T : /* *Node | Leaf */ new()\n")

	tracked := []string{
		"src/core/planted/elided.cs",
		"src/core/planted/qualified.cs",
		"src/core/planted/erased.cs",
		"src/core/planted/clean.cs",
		"src/core/planted/nearmiss.cs",
		"src/notcore/planted/elided.cs",
		"src/core/planted/union.cs",
		"src/core/planted/tilde.cs",
		"src/core/planted/unionmethods.cs",
		"src/core/planted/unionptr.cs",
	}

	scan := scanElidedConstraints(t, root, tracked)

	wantElided := []string{"planted/elided.cs:2", "planted/qualified.cs:1", "planted/unionmethods.cs:1", "planted/unionptr.cs:1"}

	if strings.Join(scan.elided, ",") != strings.Join(wantElided, ",") {
		t.Errorf("gated form: want %v, got %v", wantElided, scan.elided)
	}

	wantErased := []string{"planted/erased.cs:1"}

	if strings.Join(scan.erased, ",") != strings.Join(wantErased, ",") {
		t.Errorf("reported form: want %v, got %v -- a zero from a predicate that has never fired says nothing, "+
			"which is why this plant exists", wantErased, scan.erased)
	}

	wantUnion := []string{"planted/tilde.cs:1", "planted/union.cs:1"}

	if strings.Join(scan.union, ",") != strings.Join(wantUnion, ",") {
		t.Errorf("methodless-union form: want %v, got %v", wantUnion, scan.union)
	}

	if scan.files != 9 {
		t.Errorf("scanned %d files, want 9 (the src/core plants only; src/notcore is out of scope)", scan.files)
	}
}
