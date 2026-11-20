// deferredMarkers_test.go - Gbtc
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

// A deferred marker's payload is hex so it survives every string transform between emission and
// resolution. These guards pin what happens when that guarantee does NOT hold — the case whose
// decode result both resolvers used to discard.
//
// The distinction matters because the two failures want opposite responses: an unresolved SIGNATURE
// means an anonymous type was never lifted (expected, and the signature names it), while an
// undecodable PAYLOAD means the marker text was corrupted or was never ours at all (a bug, and
// there is no signature to name). Discarding the decode result collapsed the second into the first,
// which reported the empty string as though it were a type.

func TestDynamicTypeMarkerRoundTrips(t *testing.T) {
	signature := "struct{a []byte; b map[string]int}"
	marker := dynamicTypeMarker(signature)
	payload := marker[len(dynamicTypeMarkerPrefix) : len(marker)-len(dynamicTypeMarkerSuffix)]

	decoded, ok := dynamicTypeMarkerSignature(payload)

	if !ok {
		t.Fatalf("a payload this package produced did not decode: %q", payload)
	}

	if decoded != signature {
		t.Fatalf("round trip = %q, want %q", decoded, signature)
	}
}

func TestDynamicTypeMarkerSignatureRejectsUndecodablePayload(t *testing.T) {
	// Non-hex characters, and an odd digit count: the two ways hex.DecodeString fails.
	for _, payload := range []string{"struct{a int}", "abc"} {
		if decoded, ok := dynamicTypeMarkerSignature(payload); ok {
			t.Fatalf("payload %q decoded to %q, want a rejection", payload, decoded)
		}
	}
}

// TestRewriteDeferredMarkersReplacesUndecodablePayload covers the resolver end to end, including
// the property that is easy to lose: whatever a resolver returns, the marker must be REPLACED.
// Leaving it in place would re-match on the next pass of the rewrite loop and never terminate, so
// this test would hang rather than fail — which is itself the signal.
func TestRewriteDeferredMarkersReplacesUndecodablePayload(t *testing.T) {
	// P3's review of the godebug seat (2026-11-19): this guard used a scaffold resolver that warned and returned the
	// payload, which is what the real dynamic-type resolver did. It now drives the real resolver, whose undecodable arm
	// refuses as GO2CS2014 naming file and line, and still replaces the marker so the rewrite loop ends.
	isolateConverterPathGlobals(t)
	refusals := observeMarkerRefusals(t)
	t.Cleanup(func() { takeUnresolvedDynamicTypes() })

	fileName := filepath.Join(t.TempDir(), "corrupt.cs")
	corrupt := dynamicTypeMarkerPrefix + "not-hex" + dynamicTypeMarkerSuffix

	if _, ok := dynamicTypeMarkerSignature("not-hex"); ok {
		t.Fatal("the payload was expected to be undecodable")
	}

	if err := os.WriteFile(fileName, []byte("var x = "+corrupt+";"), 0644); err != nil {
		t.Fatalf("could not stage the test file: %v", err)
	}

	resolveDynamicTypeMarkers([]string{fileName})

	if len(*refusals) != 1 || !strings.Contains((*refusals)[0], "corrupt.cs:1: ") ||
		!strings.Contains((*refusals)[0], "dynamic-type marker's payload \"not-hex\" does not decode") {
		t.Fatalf("an undecodable payload refuses once, naming file and line; got %q", *refusals)
	}

	content, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatalf("could not read the rewritten file: %v", err)
	}

	if string(content) != "var x = not-hex;" {
		t.Fatalf("rewritten content = %q, want the payload left in place of the marker", string(content))
	}
}

// The implicit-conversion resolver drops the record either way, so only its report distinguishes a
// corrupt marker from an unlifted type. What this pins is that the corrupt case takes the rejecting
// branch at all — before the fix it fell through to a lookup of the empty signature, which happened
// to reach the same return by accident rather than by decision.
func TestResolveImplicitConvTypeNameRejectsUndecodableMarker(t *testing.T) {
	corrupt := dynamicTypeMarkerPrefix + "not-hex" + dynamicTypeMarkerSuffix

	if resolved, ok := resolveImplicitConvTypeName(corrupt); ok {
		t.Fatalf("a corrupt marker resolved to %q, want a rejection", resolved)
	}

	// A name that is not a marker at all passes through untouched — the overwhelmingly common case,
	// and the one a too-eager rejection would break.
	if resolved, ok := resolveImplicitConvTypeName("MyStruct"); !ok || resolved != "MyStruct" {
		t.Fatalf("plain name resolved to (%q, %v), want (\"MyStruct\", true)", resolved, ok)
	}
}

// i9's F1 (FLEET 2026-11-19): a marker pass that cannot leave a file free of markers used to warn and let the run exit
// 0 over an uncompilable file. It now refuses through refuseMarkerPass, observed here instead of ending the process. A
// file that does not exist is still skipped: a faulted visit writes nothing and is reported where it happens.
func TestRewriteDeferredMarkersRefusesWhatItCannotResolve(t *testing.T) {
	refusals := observeMarkerRefusals(t)

	resolveAll := func(fileName string, line int, payload string) (string, bool) { return "Resolved", true }
	dir := t.TempDir()

	missing := filepath.Join(dir, "missing.cs")
	rewriteDeferredMarkers([]string{missing}, "adapter-name", adapterNameMarkerPrefix, adapterNameMarkerSuffix, resolveAll)

	if len(*refusals) != 0 {
		t.Fatalf("a file that does not exist is skipped, not refused; got %q", *refusals)
	}

	// A directory where a file is expected: the read fails with an error that is not "does not exist".
	unreadable := filepath.Join(dir, "unreadable.cs")

	if err := os.Mkdir(unreadable, 0755); err != nil {
		t.Fatal(err)
	}

	rewriteDeferredMarkers([]string{unreadable}, "adapter-name", adapterNameMarkerPrefix, adapterNameMarkerSuffix, resolveAll)

	if len(*refusals) != 1 || !strings.Contains((*refusals)[0], "cannot read") || !strings.Contains((*refusals)[0], "unreadable.cs") {
		t.Fatalf("an unreadable file refuses, naming it; got %q", *refusals)
	}

	*refusals = nil
	unterminated := filepath.Join(dir, "unterminated.cs")
	content := "line one\nvar a = " + adapterNameMarkerPrefix + "00" + adapterNameMarkerSuffix + ";\nvar b = " + adapterNameMarkerPrefix + "00;\n"

	if err := os.WriteFile(unterminated, []byte(content), 0644); err != nil {
		t.Fatal(err)
	}

	rewriteDeferredMarkers([]string{unterminated}, "adapter-name", adapterNameMarkerPrefix, adapterNameMarkerSuffix, resolveAll)

	if len(*refusals) != 1 || !strings.Contains((*refusals)[0], "unterminated.cs:3: ") || !strings.Contains((*refusals)[0], "no closing sentinel") {
		t.Fatalf("a marker with no closing sentinel refuses, naming its file and line; got %q", *refusals)
	}

	*refusals = nil
	clean := filepath.Join(dir, "clean.cs")

	if err := os.WriteFile(clean, []byte("var a = "+adapterNameMarkerPrefix+"00"+adapterNameMarkerSuffix+";\n"), 0644); err != nil {
		t.Fatal(err)
	}

	rewriteDeferredMarkers([]string{clean}, "adapter-name", adapterNameMarkerPrefix, adapterNameMarkerSuffix, resolveAll)

	if got, _ := os.ReadFile(clean); len(*refusals) != 0 || strings.Contains(string(got), adapterNameMarkerPrefix) {
		t.Fatalf("a resolvable file is rewritten without a refusal; refusals %q, content %q", *refusals, got)
	}
}

// observeMarkerRefusals swaps refuseMarkerPass for a recorder for the rest of the test, so a refusal is seen without
// ending the process. Each entry is "position: text", the form the real refusal prints.
func observeMarkerRefusals(t *testing.T) *[]string {
	t.Helper()

	refusals := &[]string{}
	saved := refuseMarkerPass
	refuseMarkerPass = func(position, text string) { *refusals = append(*refusals, position+": "+text) }
	t.Cleanup(func() { refuseMarkerPass = saved })

	return refusals
}

// P3's review of the godebug seat (2026-11-19): the adapter-name resolver's undecodable-payload arm warned and went on,
// so `new «ADAPTER:..»(x)` became `new (x)` under an exit code of 0. It now refuses as GO2CS2014, naming file and line,
// and still replaces the marker so the rewrite loop ends. The dynamic-type arm's twin is
// TestRewriteDeferredMarkersReplacesUndecodablePayload.
func TestResolveAdapterNameMarkersRefusesAnUndecodablePayload(t *testing.T) {
	isolateConverterPathGlobals(t)
	refusals := observeMarkerRefusals(t)
	fileName := filepath.Join(t.TempDir(), "adapter-name.cs")

	if err := os.WriteFile(fileName, []byte("line one\nvar x = new "+adapterNameMarkerPrefix+"not-hex"+adapterNameMarkerSuffix+"(y);\n"), 0644); err != nil {
		t.Fatal(err)
	}

	resolveAdapterNameMarkers([]string{fileName})

	if len(*refusals) != 1 || !strings.Contains((*refusals)[0], "adapter-name.cs:2: ") ||
		!strings.Contains((*refusals)[0], "adapter-name marker's payload does not decode") {
		t.Fatalf("an undecodable payload refuses once, naming file and line; got %q", *refusals)
	}

	if content, _ := os.ReadFile(fileName); strings.Contains(string(content), adapterNameMarkerPrefix) {
		t.Fatalf("the marker is still replaced so the rewrite loop ends; got %q", content)
	}
}

// deferredMarkerPrefixes are the opening sentinels of every deferred-marker pass (rewriteDeferredMarkers' callers).
var deferredMarkerPrefixes = []string{adapterNameMarkerPrefix, dynamicTypeMarkerPrefix, dynamicBaseMarkerPrefix, dynamicNewMarkerPrefix}

// deferredMarkerLines names each line of content that holds a deferred-marker opening sentinel.
func deferredMarkerLines(content string) []int {
	var lines []int

	for i, line := range strings.Split(content, "\n") {
		for _, prefix := range deferredMarkerPrefixes {
			if strings.Contains(line, prefix) {
				lines = append(lines, i+1)
				break
			}
		}
	}

	return lines
}

// TestNoDeferredMarkerInTheCorpus is the class guard behind the three above. A deferred marker reaches a committed file
// either when its pass refuses too quietly (F1) or when no pass runs over the file at all, which no refusal inside the
// pass can see: internal/godebug's godebug.cs.auto held an adapter-name marker from 60d0f629ce, because the fully
// hand-owned path never ran the adapter-name pass over its review siblings. Every tracked .cs, .cs.auto and .cs.target
// file under src/core, src/tests (the behavioral emissions and their goldens) and src/go2cs/testdata is read. Tracked
// files live outside this module, so run with -count=1.
func TestNoDeferredMarkerInTheCorpus(t *testing.T) {
	// Positive control: each kind's sentinel is seen, and a line without one is not.
	control := "a\n" + strings.Join(deferredMarkerPrefixes, "x\n") + "x\nb\n"

	if got := deferredMarkerLines(control); len(got) != len(deferredMarkerPrefixes) || got[0] != 2 {
		t.Fatalf("the scanner names lines %v of its control; want %d lines from line 2", got, len(deferredMarkerPrefixes))
	}

	root, err := filepath.Abs(filepath.Join("..", ".."))

	if err != nil {
		t.Fatal(err)
	}

	scanned := map[string]int{}

	for _, tree := range []string{"src/core", "src/tests", "src/go2cs/testdata"} {
		for _, rel := range gitIndexFiles(t, root, tree) {
			if !strings.HasSuffix(rel, ".cs") && !strings.HasSuffix(rel, ".cs.auto") && !strings.HasSuffix(rel, ".cs.target") {
				continue
			}

			content, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

			if err != nil {
				t.Fatalf("reading %s: %v", rel, err)
			}

			scanned[tree]++

			if lines := deferredMarkerLines(string(content)); len(lines) > 0 {
				t.Errorf("%s holds an unresolved deferred marker at line(s) %v", rel, lines)
			}
		}
	}

	// Floors well under today's counts (src/core about 4,700 files, src/tests about 4,100): a tree read as nearly
	// empty is a broken enumeration, not a clean corpus.
	if scanned["src/core"] < 1000 || scanned["src/tests"] < 1000 {
		t.Fatalf("VACUOUS: files scanned per tree %v", scanned)
	}

	t.Logf("files scanned per tree: %v", scanned)
}
