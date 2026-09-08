// handOwnReferences_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// A csproj is minted from the Go import set plus the linkname destinations the converter itself
// resolved. That derivation is exact for what the converter EMITS — a three-target census of the
// emission found zero root-escape `global::go.<pkg>_package.` spellings without a matching reference
// across every converted file. What it structurally cannot see is a reference only HAND-WRITTEN C#
// needs.
//
// THE MEASURED CASE, and the reason this file exists. `internal/godebug` is hand-owned by
// consequence: every production file carries the manual-conversion marker, so the driver emits
// `godebug.cs.auto` and the compiled `godebug.cs` is human-written. That hand-written file completes
// the bodyless `registerMetric` partial by calling `global::go.runtime_package.godebugRegisterMetric`
// (godebug.cs:125) — while `godebug.cs.auto`, the converter's OWN output for the same file, carries
// an unqualified call and no `runtime_package` at all. Go's own source does not import runtime
// either, and there is no push-registry row for `internal/godebug.registerMetric`. So NO derivation
// from Go sources or from converter output can reach that reference: it is a property of C# a person
// wrote, and before the metadata un-freeze it survived only because the csproj was frozen and a human
// had put it there.
//
// The remedy is a DECLARATION the converter preserves, never an inference — which is what these two
// arms pin, in both directions.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

const handOwnFixtureBlock = "  <ItemGroup Label=\"GoHandOwnReferences\">\r\n" +
	"    <!-- godebug.cs:125 binds go.runtime_package.godebugRegisterMetric; the Go source does not import runtime. -->\r\n" +
	"    <ProjectReference Include=\"$(go2csPath)core/runtime/runtime.csproj\" />\r\n" +
	"  </ItemGroup>\r\n"

// mintedCsproj is a freshly minted csproj as writeProjectFile would hand it to the preservation step:
// carrying the DERIVED references and no hand-own block, because nothing in the derivation can
// produce one.
const mintedCsproj = "<Project Sdk=\"Microsoft.NET.Sdk\">\r\n" +
	"\r\n" +
	"  <ItemGroup>\r\n" +
	"    <ProjectReference Include=\"$(go2csPath)core/golib/golib.csproj\" />\r\n" +
	"    <ProjectReference Include=\"$(go2csPath)core/sync/sync.csproj\" />\r\n" +
	"  </ItemGroup>\r\n" +
	"\r\n" +
	"</Project>\r\n"

// TestADeclaredHandOwnReferenceSurvivesAReMint is the arm that pays for this file: the block a human
// declared is still there, with its reference, after a mint that could not have derived it.
func TestADeclaredHandOwnReferenceSurvivesAReMint(t *testing.T) {
	dir := t.TempDir()
	projectFileName := filepath.Join(dir, "internal.godebug.csproj")

	// The csproj already at the output path — the state a re-mint of a hand-owned package meets.
	existing := strings.Replace(mintedCsproj, "\r\n</Project>", "\r\n"+handOwnFixtureBlock+"\r\n</Project>", 1)

	if !strings.Contains(existing, HandOwnReferencesLabel) {
		t.Fatalf("fixture is not carrying the block; the test would pass vacuously")
	}

	if err := os.WriteFile(projectFileName, []byte(existing), 0644); err != nil {
		t.Fatalf("failed to plant the fixture csproj: %v", err)
	}

	got := string(preserveHandOwnReferences(projectFileName, []byte(mintedCsproj)))

	if !strings.Contains(got, HandOwnReferencesLabel) {
		t.Errorf("the declared %s block did not survive the re-mint", HandOwnReferencesLabel)
	}

	if !strings.Contains(got, "core/runtime/runtime.csproj") {
		t.Errorf("the declared reference did not survive the re-mint:\n%s", got)
	}

	// The reason travels with the reference, or the next reader cannot tell a live declaration from
	// a stale one.
	if !strings.Contains(got, "godebug.cs:125") {
		t.Errorf("the declaration's reason comment did not survive the re-mint")
	}

	// Exactly one block, and still inside the project.
	if n := strings.Count(got, HandOwnReferencesLabel); n != 1 {
		t.Errorf("expected exactly 1 preserved block, got %d", n)
	}

	if !strings.HasSuffix(strings.TrimRight(got, "\r\n"), "</Project>") {
		t.Errorf("the preserved block was inserted after </Project>:\n%s", got)
	}

	// The derived references are untouched by preservation.
	if !strings.Contains(got, "core/golib/golib.csproj") || !strings.Contains(got, "core/sync/sync.csproj") {
		t.Errorf("preservation disturbed the derived reference list:\n%s", got)
	}
}

// TestAReMintInventsNoHandOwnReferenceBlock is the arm that keeps the mechanism a DECLARATION. A
// csproj without the block must come back byte-identical: the absence of a declaration is not an
// invitation to infer one, and an inferred block would be exactly the guessing this design exists to
// avoid.
func TestAReMintInventsNoHandOwnReferenceBlock(t *testing.T) {
	dir := t.TempDir()
	projectFileName := filepath.Join(dir, "internal.godebug.csproj")

	if err := os.WriteFile(projectFileName, []byte(mintedCsproj), 0644); err != nil {
		t.Fatalf("failed to plant the fixture csproj: %v", err)
	}

	got := string(preserveHandOwnReferences(projectFileName, []byte(mintedCsproj)))

	if got != mintedCsproj {
		t.Errorf("a csproj with no declared block was modified by preservation:\ngot:\n%s\nwant:\n%s", got, mintedCsproj)
	}

	if strings.Contains(got, HandOwnReferencesLabel) {
		t.Errorf("preservation INVENTED a %s block", HandOwnReferencesLabel)
	}
}

// TestAFirstConversionHasNothingToPreserve: no csproj at the output path yet. Returned untouched
// rather than treated as an error, which is the ordinary first-conversion path.
func TestAFirstConversionHasNothingToPreserve(t *testing.T) {
	dir := t.TempDir()
	projectFileName := filepath.Join(dir, "does.not.exist.csproj")

	got := string(preserveHandOwnReferences(projectFileName, []byte(mintedCsproj)))

	if got != mintedCsproj {
		t.Errorf("a first conversion's contents were modified:\ngot:\n%s", got)
	}
}

// TestPreservationIsEOLAgnostic: the existing csproj is read BACK off disk, so its line endings are
// the checkout's, not the converter's. An LF working tree (any non-Windows clone, or
// core.autocrlf=false) must preserve just as a CRLF one does — the same reason writePackageInfoFile
// splits EOL-agnostically.
func TestPreservationIsEOLAgnostic(t *testing.T) {
	dir := t.TempDir()
	projectFileName := filepath.Join(dir, "internal.godebug.csproj")

	existing := strings.Replace(mintedCsproj, "\r\n</Project>", "\r\n"+handOwnFixtureBlock+"\r\n</Project>", 1)
	lfExisting := strings.ReplaceAll(existing, "\r\n", "\n")

	if strings.Contains(lfExisting, "\r") {
		t.Fatalf("the LF fixture still carries a CR; the arm would not vary the axis it claims to")
	}

	if err := os.WriteFile(projectFileName, []byte(lfExisting), 0644); err != nil {
		t.Fatalf("failed to plant the LF fixture: %v", err)
	}

	got := string(preserveHandOwnReferences(projectFileName, []byte(mintedCsproj)))

	if !strings.Contains(got, "core/runtime/runtime.csproj") {
		t.Errorf("an LF-checkout csproj lost its declared reference on re-mint:\n%s", got)
	}
}
