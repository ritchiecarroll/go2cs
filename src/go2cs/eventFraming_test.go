// eventFraming_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

// The comparer reads the converted host's event stream the way cmd/test2json reads a Go test binary run
// under -test.v=test2json (H2).
//
// A test that writes to os.Stdout without a trailing newline leaves the stream mid-line, and the next
// event lands on the end of that output. Go's test binary marks every framing line with a leading ^V
// (testing.go, chattyFlag.prefix), and test2json ends a line at "\n" OR just before a ^V that does not
// begin one (test2json.go, indexEOL), so the partial output and the event read as two lines. The
// converted host marks its JSON event lines the same way; these pin the reading side. Measured
// 2026-10-02 (linux): x/mod/sumdb/tlog's TestCertificateTransparency writes an HTTP body with
// os.Stdout.Write under -v, and its pass event read C#="" against Go's pass.

const framedPassEvent = "\x16{\"package\":\"example\",\"test\":\"TestBody\",\"action\":\"pass\",\"elapsed\":0.1}"

// TestTerminalResultsReadAnEventAfterUnterminatedOutput is the verdict reader on the tlog shape.
func TestTerminalResultsReadAnEventAfterUnterminatedOutput(t *testing.T) {
	stream := "{\"consistency\":[\"80Sfbj6U\"]}" + framedPassEvent + "\n"

	if got := terminalTestResults(stream)["TestBody"]; got != "pass" {
		t.Errorf("terminalTestResults read TestBody as %q from %q, want \"pass\"", got, stream)
	}
}

// TestTerminalResultsReadAnEventAtALineStart is the ordinary case, marked: the marker is stripped.
func TestTerminalResultsReadAnEventAtALineStart(t *testing.T) {
	stream := "partial output\n" + framedPassEvent + "\n"

	if got := terminalTestResults(stream)["TestBody"]; got != "pass" {
		t.Errorf("terminalTestResults read TestBody as %q from %q, want \"pass\"", got, stream)
	}
}

// TestDiagnosticTailKeepsTheOutputAndDropsTheEvent: the partial output is diagnostic text, the event is not.
func TestDiagnosticTailKeepsTheOutputAndDropsTheEvent(t *testing.T) {
	stream := "body without a newline" + framedPassEvent + "\n"

	tail := diagnosticOutputTail(stream)
	if tail == nil {
		t.Fatalf("diagnosticOutputTail dropped the partial output of %q", stream)
	}
	if tail.Text != "body without a newline" {
		t.Errorf("diagnostic tail = %q, want only the partial output", tail.Text)
	}
	if strings.ContainsRune(tail.Text, '\x16') {
		t.Errorf("diagnostic tail kept the framing marker: %q", tail.Text)
	}
}
