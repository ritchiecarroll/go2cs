// finalizerDoorGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// THE FINALIZER/CLEANUP DOOR GUARD, over the REAL corpus.
//
// The defect it locks out is a SILENT NO-OP IN A PUBLIC RUNTIME API. Go 1.24's runtime.AddCleanup
// calls createfing(), and until COORD's ruling c58b4c01d the converted createfing started the
// CONVERTED runfinq -- the body mfinal.cs's own header declares dead, which dies in gopark on its
// first park. Taken that way, runtime.AddCleanup COMPILES, returns a Cleanup, and never runs it: no
// throw, no diagnostic, nothing in the system able to say why. See docs/phase4/DESIGN-managed-getg.md
// §14 for the door map this asserts.
//
// ⚠ WHY A SOURCE GUARD AND NOT ONLY THE BEHAVIOURAL ROW. GolibTests/CleanupDispatchTests measures
// the end-to-end path, but it CANNOT attribute the runner's existence to AddCleanup: MSTest runs one
// process, and any SetFinalizer anywhere in that assembly has already called EnsureRunner by then.
// So on a regressed createfing those arms still pass whenever a finalizer test runs first, and go red
// only in a process that uses cleanups alone. This guard is order-independent and needs no runtime,
// which is the half the behavioural row cannot supply -- the two together are the acceptance, and
// neither alone is.
//
// The marker arm is the one most likely to fire in anger: a `-stdlib` reconvert regenerates any file
// that has lost its [module: GoManualConversion] line, so a marker slip restores the auto's
// createfing -- and the corpus still compiles, which is exactly the failure mode.
func TestFinalizerDoorsStayWiredToTheLiveRunner(t *testing.T) {
	coreDir := filepath.Join("..", "core")

	// ⚠ COMMENTS STRIPPED UP FRONT, before anything is matched or brace-counted. Every arm here is a
	// substring match and both files are HEAVILY COMMENTED -- createfing's own body comment names
	// createfing, EnsureRunner and runfinq -- so an arm asserting "this body CALLS X" is satisfied by a
	// comment MENTIONING X, and a commented-out call reads as a live one. Measured, not reasoned: the
	// caller arm below was written against the raw text, its negative control commented the call out,
	// and the arm stayed GREEN. G named the class the same night on their own census (a verdict line
	// reading "0 DUPLICATE patch-id(s)" satisfying an arm asserting "DUPLICATE patch-id"); this is that
	// class in a guard written an hour later, caught only because the control was aimed at it.
	//
	// Stripping FIRST also makes the brace matching sound: a `{` inside a comment would otherwise
	// unbalance a body extraction, silently.
	mfinal := codeOnly(readCorpusFile(t, filepath.Join(coreDir, "runtime", "mfinal.cs")))
	mcleanup := codeOnly(readCorpusFile(t, filepath.Join(coreDir, "runtime", "mcleanup.cs")))

	// ---- the marker arm: without it a reconvert quietly restores both auto bodies ----------
	for _, f := range []struct{ name, text string }{
		{"mfinal.cs", mfinal},
		{"mcleanup.cs", mcleanup},
	} {
		if !strings.Contains(f.text, "[module: go.GoManualConversion]") {
			t.Errorf("%s has lost its [module: go.GoManualConversion] marker -- a -stdlib reconvert will "+
				"regenerate it over the hand-own, the corpus will still COMPILE, and runtime.AddCleanup will "+
				"go back to being a silent no-op", f.name)
		}
	}

	// ---- the door arm: createfing must forward to the live runner, and start nothing else ----
	body, ok := csharpFunctionBody(mfinal, "internal static void createfing() {")
	if !ok {
		t.Fatalf("could not find createfing's body in mfinal.cs -- the guard cannot answer its question, " +
			"which is a failure and not a pass")
	}

	if !strings.Contains(body, "GoFinalizerQueue.EnsureRunner()") {
		t.Errorf("createfing no longer forwards to GoFinalizerQueue.EnsureRunner(); its body is:\n%s\n"+
			"Every caller of createfing -- runtime.AddCleanup is the first -- depends on this being the ONE "+
			"live door. A door that starts anything else starts a runner nothing drains.", body)
	}

	if strings.Contains(body, "goǃ(runfinq)") {
		t.Errorf("createfing has been restored to `goǃ(runfinq)`, which starts the CONVERTED runfinq -- the "+
			"body mfinal.cs's own header declares dead. Its body is:\n%s", body)
	}

	// ---- the caller arm: AddCleanup must still ask for a runner at all -----------------------
	//
	// Deliberately NOT an assertion that it calls createfing specifically: routing it straight to
	// EnsureRunner would be equally correct, and a guard that forbids a correct alternative is a
	// guard that gets deleted. What must never happen is AddCleanup registering with no runner
	// creation anywhere, because then the body is queued and nothing ever dequeues it.
	addCleanup, ok := csharpFunctionBody(mcleanup, "public static Cleanup AddCleanup<T, S>(")
	if !ok {
		t.Fatalf("could not find AddCleanup's body in mcleanup.cs")
	}

	if !strings.Contains(addCleanup, "createfing()") &&
		!strings.Contains(addCleanup, "GoFinalizerQueue.EnsureRunner()") {
		t.Errorf("AddCleanup asks for no finalizer runner. Go's own body calls createfing() for exactly this "+
			"reason -- the registration is what makes the runner necessary -- and without it a cleanup is "+
			"queued to a thread that does not exist. Its body is:\n%s", addCleanup)
	}
}

// codeOnly strips C# comments from a body so an arm asserting that it CALLS something cannot be
// satisfied by a comment MENTIONING it.
//
// It CONSULTS the converter's own stripCSharpComments (directiveOperations.go) rather than carrying
// a private copy. The first draft of this file did carry one, and the repo rule it broke is written
// down for exactly that reason: an instrument's private copy of a rule the system already defines
// drifts, and a drifted instrument copy files its own target under the sound bucket. The converter's
// version also already knows something this guard's author did not -- that a `/*` INSIDE a `//`
// comment opens nothing -- which is the bug that once made the converter read a hand-owned file as
// not hand-owned. Same predicate, same hazard, one definition.
func codeOnly(body string) string {
	var out strings.Builder

	inBlockComment := false

	for _, line := range strings.Split(body, "\n") {
		out.WriteString(stripCSharpComments(line, &inBlockComment))
		out.WriteByte('\n')
	}

	return out.String()
}

func readCorpusFile(t *testing.T, path string) string {
	t.Helper()

	data, err := os.ReadFile(path)
	if err != nil {
		t.Fatalf("cannot read %s: %v", path, err)
	}

	return string(data)
}

// csharpFunctionBody returns the text between the brace opened on the line matching `signature` and
// its matching close. Brace-matched rather than line-counted: every line-offset citation this lane
// wrote today went stale within the hour, and a guard reading the WRONG lines fails in the direction
// that looks like a pass.
//
// It is naive about braces inside STRING LITERALS, which is safe because it is only ever handed
// comment-stripped source (see the caller) and these bodies contain no literal spelling a brace. If
// that stops being true the arm FAILS to find its marker rather than silently reading half a body:
// the depth never returns to zero and ok is false.
func csharpFunctionBody(source, signature string) (string, bool) {
	start := strings.Index(source, signature)
	if start < 0 {
		return "", false
	}

	open := strings.IndexByte(source[start:], '{')
	if open < 0 {
		return "", false
	}

	open += start
	depth := 0

	for i := open; i < len(source); i++ {
		switch source[i] {
		case '{':
			depth++
		case '}':
			depth--
			if depth == 0 {
				return source[open : i+1], true
			}
		}
	}

	return "", false
}
