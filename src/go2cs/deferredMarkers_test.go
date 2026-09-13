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
	fileName := filepath.Join(t.TempDir(), "corrupt.cs")
	corrupt := dynamicTypeMarkerPrefix + "not-hex" + dynamicTypeMarkerSuffix

	if err := os.WriteFile(fileName, []byte("var x = "+corrupt+";"), 0644); err != nil {
		t.Fatalf("could not stage the test file: %v", err)
	}

	var seen []string

	rewriteDeferredMarkers([]string{fileName}, "dynamic type", dynamicTypeMarkerPrefix, dynamicTypeMarkerSuffix,
		func(_ string, line int, payload string) (string, bool) {
			seen = append(seen, payload)

			// The scaffold reports WHERE the marker was, which is what makes an unresolvable one
			// actionable in a file with thousands of lines. This fixture is a single line.
			if line != 1 {
				t.Errorf("resolver was told line %d, want 1", line)
			}

			if _, ok := dynamicTypeMarkerSignature(payload); ok {
				t.Fatalf("payload %q was expected to be undecodable", payload)
			}

			return payload, false
		})

	if len(seen) != 1 || seen[0] != "not-hex" {
		t.Fatalf("resolver saw payloads %v, want exactly [not-hex]", seen)
	}

	content, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatalf("could not read the rewritten file: %v", err)
	}

	if strings.Contains(string(content), dynamicTypeMarkerPrefix) {
		t.Fatalf("a marker survived the rewrite: %q", string(content))
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
