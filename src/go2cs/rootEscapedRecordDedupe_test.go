// rootEscapedRecordDedupe_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"reflect"
	"testing"
)

// TestRootEscapedRecordDedupeCollapsesOneType pins the net/http case: the SAME
// (impl, interface) pair registered from one bare cast site and one `global::`-escaped site is a
// single record, and the escaped spelling is the one that survives. Emitting both made go2cs-gen
// mint `http_HandlerFuncᴠΔHandler` twice (`-val.g.cs` + `-val.1.g.cs`): CS0102 + CS0111 ×5 + CS8646 ×2.
func TestRootEscapedRecordDedupeCollapsesOneType(t *testing.T) {
	// As emitted and sorted by the writer, escaped and bare spellings of one pair.
	input := []string{
		"[assembly: GoImplement<global::go.net.http_package.HandlerFunc, global::go.net.http_package.ΔHandler>]",
		"[assembly: GoImplement<go.net.http_package.HandlerFunc, go.net.http_package.ΔHandler>]",
	}

	want := []string{
		"[assembly: GoImplement<global::go.net.http_package.HandlerFunc, global::go.net.http_package.ΔHandler>]",
	}

	if got := dedupeRootEscapedRecords(input); !reflect.DeepEqual(got, want) {
		t.Fatalf("dedupeRootEscapedRecords:\n got %q\nwant %q", got, want)
	}
}

// TestRootEscapedRecordDedupePrefersEscapedRegardlessOfOrder proves the winner is chosen by the
// escape count, not by which spelling the sort happened to put first — the escaped form is
// shadow-proof by construction and keeping the bare one could reintroduce the shadow the escape
// exists to defeat.
func TestRootEscapedRecordDedupePrefersEscapedRegardlessOfOrder(t *testing.T) {
	bare := "[assembly: GoImplicitConv<a.b_package.T, a.b_package.U>]"
	escaped := "[assembly: GoImplicitConv<global::a.b_package.T, global::a.b_package.U>]"

	for _, input := range [][]string{{bare, escaped}, {escaped, bare}} {
		got := dedupeRootEscapedRecords(input)

		if len(got) != 1 || got[0] != escaped {
			t.Fatalf("dedupeRootEscapedRecords(%q) = %q, want [%q]", input, got, escaped)
		}
	}
}

// TestRootEscapedRecordDedupeKeepsDistinctRecords is the negative control: records that differ by
// anything other than a root escape must ALL survive, and the result must stay sorted. Without it a
// green dedupe test proves nothing — a helper that returned one line would pass the cases above.
func TestRootEscapedRecordDedupeKeepsDistinctRecords(t *testing.T) {
	input := []string{
		"[assembly: GoImplement<A, X>]",
		"[assembly: GoImplement<A, Y>]",
		"[assembly: GoImplement<B, X>(Pointer = true)]",
		"[assembly: GoImplement<B, X>(Promoted = true)]",
		"[assembly: GoImplement<global::A, X>]",
	}

	want := []string{
		"[assembly: GoImplement<A, Y>]",
		"[assembly: GoImplement<B, X>(Pointer = true)]",
		"[assembly: GoImplement<B, X>(Promoted = true)]",
		"[assembly: GoImplement<global::A, X>]",
	}

	if got := dedupeRootEscapedRecords(input); !reflect.DeepEqual(got, want) {
		t.Fatalf("dedupeRootEscapedRecords:\n got %q\nwant %q", got, want)
	}
}

// TestRootEscapedRecordDedupeIsInertWithoutEscapes pins the production case: no record carries a
// root escape outside a `-tests` conversion, so the pass must be an exact identity there. This is
// what makes the whole corpus provably unaffected.
func TestRootEscapedRecordDedupeIsInertWithoutEscapes(t *testing.T) {
	input := []string{
		"[assembly: GoImplement<dirEntry, go.io.fs_package.DirEntry>]",
		"[assembly: GoImplement<unixDirent, DirEntry>(Pointer = true)]",
		"[assembly: GoImplicitConv<Duration, nint>(Inverted = true)]",
	}

	want := append([]string(nil), input...)

	if got := dedupeRootEscapedRecords(input); !reflect.DeepEqual(got, want) {
		t.Fatalf("dedupeRootEscapedRecords must be identity without escapes:\n got %q\nwant %q", got, want)
	}
}
