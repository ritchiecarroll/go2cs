// nativeFieldArrayViewTranscription_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"strings"
	"testing"
)

// TestDarwinSockaddrTranscriptionMatchesItsDeclaration holds GolibTests' transcription of darwin's
// converted RawSockaddrInet4 equal to the declaration it stands for. GolibTests cannot load darwin's
// syscall on a linux or windows host, so its native field-view arms root on that transcription; this is
// what keeps a converter change to the darwin struct from leaving the arms measuring a struct that no
// longer exists. Field lines are compared whitespace-trimmed, in order.
func TestDarwinSockaddrTranscriptionMatchesItsDeclaration(t *testing.T) {
	root := repoRootFromPackageDir(t)

	declared := structFieldLines(t, readTrackedText(t, root, "src/core/syscall/darwin/ztypes_darwin_amd64.cs"), "partial struct RawSockaddrInet4 {", "}")
	test := readTrackedText(t, root, "src/tests/GolibTests/NativeFieldArrayViewTests.cs")
	begin := strings.Index(test, "// BEGIN TRANSCRIPTION: syscall/darwin/ztypes_darwin_amd64.cs RawSockaddrInet4")

	if begin < 0 {
		t.Fatalf("NativeFieldArrayViewTests.cs carries no transcription marker; the arms root on a struct nothing holds to its declaration")
	}

	transcribed := structFieldLines(t, test[begin:], "private struct DarwinRawSockaddrInet4", "// END TRANSCRIPTION")

	if len(declared) < 5 || strings.Join(declared, "\n") != strings.Join(transcribed, "\n") {
		t.Errorf("the transcription has drifted from darwin's converted RawSockaddrInet4\n declared:\n  %s\n transcribed:\n  %s",
			strings.Join(declared, "\n  "), strings.Join(transcribed, "\n  "))
	}
}

// structFieldLines returns the trimmed field-declaration lines between a struct's opening line and the
// end marker: every line that declares a public field, a trailing comment included.
func structFieldLines(t *testing.T, text, open, end string) []string {
	t.Helper()

	start := strings.Index(text, open)

	if start < 0 {
		t.Fatalf("no %q in the text", open)
	}

	var fields []string

	for _, line := range strings.Split(text[start+len(open):], "\n") {
		trimmed := strings.TrimSpace(line)

		if strings.HasPrefix(trimmed, end) {
			break
		}

		if strings.HasPrefix(trimmed, "public ") && strings.Contains(trimmed, ";") {
			fields = append(fields, trimmed)
		}
	}

	return fields
}
