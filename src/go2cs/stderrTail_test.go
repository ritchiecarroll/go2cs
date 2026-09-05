// stderrTail_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

// stderrTailHelperEnv selects a re-exec ROLE for the helpers below. The arms that exercise a real
// child re-exec THIS test binary rather than calling the extractor on a synthetic string, because
// the property under test is not "the extractor splits lines": it is that the text a dying process
// writes to fd 2 survives the pipeline's own capture funnel — one pipe shared by both descriptors,
// a context deadline, a process-group kill — and arrives in the record. A synthetic string skips
// every step that can lose it.
const stderrTailHelperEnv = "GO2CS_STDERR_TAIL_HELPER"

// stderrTailHelperDeadline is the package deadline the deadline arm hands runCommandWithTimeoutEnv.
// Sized for the SLOWEST legitimate host rather than the fastest: the measurement depends on the
// child having started and written its line BEFORE the kill, so a deadline that fires during
// process startup would report an instrument failure on a loaded box. A safety net, never a
// performance assumption.
const stderrTailHelperDeadline = 5 * time.Second

// stderrTailHelperMarker is the line the helpers write to fd 2. Shaped like the thing this field
// exists to carry — Go's `throw` prints `fatal error: ...` before it dies, and that text never
// enters the results stream because the stream carries the TEST's captured output, not the
// process's raw fd-2 writes.
const stderrTailHelperMarker = "fatal error: q59 guard marker — mallocgc called without a P"

// stderrTailHelperEvents are well-formed event lines the helpers write to fd 1, so every positive
// arm is also an anti-vacuity arm: an extractor that returned its whole input would carry the
// marker AND these, and the assertions below require the marker present and these absent. Passing
// arm one without arm two's property is therefore impossible — one helper, one stream, both
// directions measured at once.
var stderrTailHelperEvents = []string{
	`{"package":"guard","test":"TestAlpha","action":"run","elapsed":0,"output":null,"source":null,"line":null}`,
	`{"package":"guard","test":"TestAlpha","action":"pass","elapsed":0.0031,"output":null,"source":null,"line":null}`,
	`{"package":"guard","test":"","action":"fail","elapsed":0.0042,"output":null,"source":null,"line":null}`,
}

// TestStderrTailCarriesADyingChildsFd2 is the positive control for the whole field: a child that
// writes event lines to fd 1 and one fatal line to fd 2 and then exits NONZERO must yield a tail
// carrying that line verbatim and none of the events.
//
// The negative half is in the same arm rather than in a separate test, deliberately. An extractor
// that filtered nothing would pass a "contains the marker" assertion while measuring nothing at
// all, which is the false-green shape this repository names its routes after; requiring the event
// lines ABSENT is what makes the green mean the filter ran.
func TestStderrTailCarriesADyingChildsFd2(t *testing.T) {
	if os.Getenv(stderrTailHelperEnv) != "" {
		t.Skip("re-exec helper role; the guard itself runs only in the top-level process")
	}

	output, err := runStderrTailHelper(t, "exit")
	if err == nil {
		t.Fatalf("expected the helper's nonzero exit to surface as an error; output=%q", output)
	}

	tail := diagnosticOutputTail(output)
	if tail == nil {
		t.Fatalf("the child wrote to fd 2 and the tail is nil — the reading this field exists to "+
			"carry was dropped between the capture funnel and the record; output=%q", output)
	}
	if !strings.Contains(tail.Text, stderrTailHelperMarker) {
		t.Fatalf("tail does not carry the child's fd-2 line.\n  want substring: %q\n  tail: %q", stderrTailHelperMarker, tail.Text)
	}
	for _, event := range stderrTailHelperEvents {
		if strings.Contains(tail.Text, event) {
			t.Fatalf("tail carries an EVENT line, so the extractor filtered nothing and a green here "+
				"would measure nothing.\n  event: %q\n  tail: %q", event, tail.Text)
		}
	}
}

// TestStderrTailIsAbsentForASilentChild is the other direction: a child whose entire output is a
// well-formed event stream has nothing to explain, and the field must be ABSENT rather than an
// empty object claiming there was nothing to say. Absence is what keeps a clean row's record
// byte-for-byte what it was before this field existed.
func TestStderrTailIsAbsentForASilentChild(t *testing.T) {
	if os.Getenv(stderrTailHelperEnv) != "" {
		t.Skip("re-exec helper role; the guard itself runs only in the top-level process")
	}

	output, err := runStderrTailHelper(t, "clean")
	if err != nil {
		t.Fatalf("the clean helper should exit zero: %v (output=%q)", err, output)
	}
	if tail := diagnosticOutputTail(output); tail != nil {
		t.Fatalf("a child that wrote nothing but events produced a tail — every event line the "+
			"pipeline parses would be republished as diagnostics: %q", tail.Text)
	}
}

// TestStderrTailSurvivesThePackageDeadlineKill is the arm for the case where the reading was
// GENUINELY absent rather than merely unaddressable. On a deadline kill runCommandWithTimeoutEnv
// returns everything the child had written and puts NONE of it in the error, and the compare path
// reads only err.Error() — so the record's whole account of a killed run was
// `... timed out after 30m0s`. The output was arriving and being dropped on the floor.
//
// Both directions are asserted: the error must say it timed out (otherwise the arm measured a
// normal exit and proves nothing about the deadline path), and the tail must carry the line.
func TestStderrTailSurvivesThePackageDeadlineKill(t *testing.T) {
	if os.Getenv(stderrTailHelperEnv) != "" {
		t.Skip("re-exec helper role; the guard itself runs only in the top-level process")
	}

	output, err := runStderrTailHelper(t, "deadline")
	if err == nil || !strings.Contains(err.Error(), "timed out") {
		t.Fatalf("expected the package deadline to fire and kill the helper; err=%v output=%q", err, output)
	}

	tail := diagnosticOutputTail(output)
	if tail == nil || !strings.Contains(tail.Text, stderrTailHelperMarker) {
		t.Fatalf("a deadline-killed child's fd-2 line did not reach the tail, which is the case the "+
			"record could not report at all before this field.\n  want substring: %q\n  tail: %#v\n  output: %q",
			stderrTailHelperMarker, tail, output)
	}
}

// TestStderrTailAttachRuleReadsTheRawError pins the attach rule's load-bearing half: the errors it
// reads are the RAW ones, snapshotted before the disclosed and agreed-failure arms nil them out.
//
// The forgiven-exit row is exactly the shape the getg rows were read through — a host that exited
// nonzero, whose exit the disclosed signature accounts for, and whose stderr is the only surviving
// account of why. A rule that read the post-forgiveness error would see nil beside matched=true and
// publish nothing, which is indistinguishable from a clean run.
func TestStderrTailAttachRuleReadsTheRawError(t *testing.T) {
	const goDiagnostic = "go: downloading nothing\nfatal error: oracle side said something"
	const csDiagnostic = "fatal error: converted host said something"
	forgiven := fmt.Errorf("converted tests exited 1")

	// A matched row whose C# exit was FORGIVEN still carries its tail, and only on the side that
	// errored — the asymmetry is the assertion, since attaching both sides would pass this arm
	// while proving the rule never looked at the error at all.
	tails := comparisonStderrTailsFor(goDiagnostic, csDiagnostic, nil, forgiven, true)
	if tails == nil || tails.CSharp == nil {
		t.Fatalf("a forgiven nonzero exit lost its tail: %#v", tails)
	}
	if !strings.Contains(tails.CSharp.Text, csDiagnostic) {
		t.Fatalf("forgiven row's C# tail: want %q, got %q", csDiagnostic, tails.CSharp.Text)
	}
	if tails.Go != nil {
		t.Fatalf("the Go side neither errored nor mismatched, so it has nothing to explain: %q", tails.Go.Text)
	}

	// The negative control the arm above needs to mean anything: the SAME diagnostics on a clean,
	// matched, error-free comparison attach nothing. Without this a rule that attached
	// unconditionally would pass every other assertion in this file.
	if tails := comparisonStderrTailsFor(goDiagnostic, csDiagnostic, nil, nil, true); tails != nil {
		t.Fatalf("a clean matched comparison must carry no tail at all, got %#v", tails)
	}

	// A divergence attaches both sides even when neither command errored: the exit codes agreed and
	// the verdicts did not, so whatever either side printed is part of the reading.
	if tails := comparisonStderrTailsFor(goDiagnostic, csDiagnostic, nil, nil, false); tails == nil || tails.Go == nil || tails.CSharp == nil {
		t.Fatalf("an unmatched comparison must carry both sides' tails, got %#v", tails)
	}
}

// TestStderrTailBoundsSayWhatTheyDropped pins the bound and, more importantly, that truncation is
// SAID. A tail that silently dropped the first half of a stack reads as a complete one, which is
// the same quiet dishonesty as a gated record that reads as a full run.
func TestStderrTailBoundsSayWhatTheyDropped(t *testing.T) {
	var lines []string
	for i := 0; i < stderrTailMaxLines*3; i++ {
		lines = append(lines, fmt.Sprintf("frame %03d at some.name.space.Method(arg, arg, arg)", i))
	}
	last := lines[len(lines)-1]

	tail := diagnosticOutputTail(strings.Join(lines, "\n"))
	if tail == nil {
		t.Fatal("a long diagnostic produced no tail at all")
	}
	if !tail.Truncated {
		t.Fatalf("a %d-line diagnostic was bounded to %d lines without saying so", len(lines), stderrTailMaxLines)
	}
	if got := strings.Count(tail.Text, "\n") + 1; got > stderrTailMaxLines {
		t.Fatalf("tail kept %d lines, bound is %d", got, stderrTailMaxLines)
	}
	if len(tail.Text) > stderrTailMaxBytes {
		t.Fatalf("tail is %d bytes, bound is %d", len(tail.Text), stderrTailMaxBytes)
	}
	if tail.OmittedLines == 0 || tail.OmittedBytes == 0 {
		t.Fatalf("truncation reported no counts: %#v", tail)
	}
	// The END is where a death says why, so the cut takes the FRONT.
	if !strings.HasSuffix(tail.Text, last) {
		t.Fatalf("truncation dropped the LAST line, which is the one a reader needs:\n  want suffix: %q\n  tail ends: %q",
			last, tail.Text[max(0, len(tail.Text)-len(last)-40):])
	}

	// One line longer than the whole budget — a host that printed without a newline is exactly the
	// case a line-based bound cannot hold, and the byte cut must land on a rune boundary or
	// json.Marshal replaces the broken bytes with U+FFFD and the record carries bytes the process
	// never wrote. The filler is multi-byte precisely so an off-by-one cut would show.
	long := strings.Repeat("é", stderrTailMaxBytes) + "END"
	tail = diagnosticOutputTail(long)
	if tail == nil || !tail.Truncated || len(tail.Text) > stderrTailMaxBytes {
		t.Fatalf("single over-long line was not bounded: %#v", tail)
	}
	if !strings.HasSuffix(tail.Text, "END") {
		t.Fatalf("single over-long line lost its end: %q", tail.Text[max(0, len(tail.Text)-40):])
	}
	if encoded, err := json.Marshal(tail.Text); err != nil {
		t.Fatalf("bounded tail does not marshal: %v", err)
	} else if strings.Contains(string(encoded), `�`) {
		t.Fatal("the byte cut split a rune: the record would carry U+FFFD bytes the process never wrote")
	}
}

// TestComparisonRecordCarriesAndOmitsTheStderrKey reads the property at the artifact rather than at
// the struct, because the deliverable is a FILE somebody greps. Both directions: a record with a
// tail publishes it under the documented key path, and a clean record has no `stderr` key at all —
// which is the mechanical form of "a passing row's record is byte-for-byte what it was before".
func TestComparisonRecordCarriesAndOmitsTheStderrKey(t *testing.T) {
	read := func(t *testing.T, result *testComparison) map[string]any {
		t.Helper()
		dir := t.TempDir()
		if err := writeComparisonRecord(dir, result, ""); err != nil {
			t.Fatalf("write comparison record: %v", err)
		}
		data, err := os.ReadFile(filepath.Join(dir, "go2cs_test_comparison.json"))
		if err != nil {
			t.Fatalf("read comparison record: %v", err)
		}
		var record map[string]any
		if err := json.Unmarshal(data, &record); err != nil {
			t.Fatalf("comparison record is not valid JSON: %v", err)
		}
		return record
	}

	clean := &testComparison{Package: "errors", Status: "validated", Matched: true}
	if _, present := read(t, clean)["stderr"]; present {
		t.Fatal("a clean comparison published a stderr key; a passing row's record must be unchanged by this field")
	}

	dying := &testComparison{
		Package: "runtime", Status: "failing", Matched: false,
		Stderr: comparisonStderrTailsFor("", stderrTailHelperMarker, nil, fmt.Errorf("host died"), false),
	}
	record := read(t, dying)
	section, present := record["stderr"].(map[string]any)
	if !present {
		t.Fatalf("a dying comparison published no stderr key: %#v", record["stderr"])
	}
	side, present := section["csharp"].(map[string]any)
	if !present {
		t.Fatalf("stderr section carries no csharp side: %#v", section)
	}
	if text, _ := side["text"].(string); !strings.Contains(text, stderrTailHelperMarker) {
		t.Fatalf("stderr.csharp.text does not carry the marker: %q", text)
	}
	if _, present := section["go"]; present {
		t.Fatalf("the Go side had nothing to explain and must be omitted: %#v", section)
	}
}

// runStderrTailHelper re-execs this test binary in one of the helper roles and returns exactly what
// the pipeline's own funnel captured, so the arms above measure the real capture path.
func runStderrTailHelper(t *testing.T, role string) (string, error) {
	t.Helper()
	return runCommandWithTimeoutEnv(stderrTailHelperDeadline, t.TempDir(), Options{},
		[]string{stderrTailHelperEnv + "=" + role},
		os.Args[0], "-test.run=^TestStderrTailHelper$", "-test.timeout=0")
}

// TestStderrTailHelper is the re-exec'd child. It writes its streams and then calls os.Exit
// directly rather than returning: the testing framework's own PASS/FAIL line would otherwise land
// in the captured stream as a non-event line, and the arms above would be asserting against the
// harness's output instead of the child's. Skipped in an ordinary run.
func TestStderrTailHelper(t *testing.T) {
	role := os.Getenv(stderrTailHelperEnv)
	if role == "" {
		t.Skip("helper: runs only when re-exec'd by the stderr-tail guards")
	}

	for _, event := range stderrTailHelperEvents {
		fmt.Fprintln(os.Stdout, event)
	}

	switch role {
	case "clean":
		os.Exit(0)
	case "exit":
		fmt.Fprintln(os.Stderr, stderrTailHelperMarker)
		os.Exit(1)
	case "deadline":
		fmt.Fprintln(os.Stderr, stderrTailHelperMarker)
		// Well past the deadline, and SLEEPING rather than parking every goroutine forever: a
		// process whose goroutines are all blocked trips Go's deadlock detector and exits on its
		// own, which is the one outcome that would make the kill unnecessary and the measurement
		// meaningless. A self-imposed ceiling so a RED run cannot leave a stray behind.
		time.Sleep(2 * time.Minute)
		os.Exit(0)
	default:
		fmt.Fprintf(os.Stderr, "unknown helper role %q\n", role)
		os.Exit(2)
	}
}
