// warningEntries_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the per-file warning entries (warningEntries.go): each fact the converter derives, the
// writer's ownership rules, and the layout L3 merge that composes the corpus file from every target.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertWarningEntriesFixture converts a main package made of the given files and returns the
// folder its emission went to.
func convertWarningEntriesFixture(t *testing.T, root, module string, files map[string]string) string {
	t.Helper()

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	appDir := filepath.Join(root, "app")

	if err := os.RemoveAll(appDir); err != nil {
		t.Fatal(err)
	}

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/"+module+"\n\ngo 1.23\n")

	for name, source := range files {
		writeModuleFile(t, filepath.Join(appDir, name), source)
	}

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return filepath.Join(options.go2csPath, "src", "example.com", module)
}

// readWarningEntriesFixture reads the emitted file's model; a missing file reads as no entries.
func readWarningEntriesFixture(t *testing.T, dir string) (warningEntrySet, bool) {
	t.Helper()

	entries, exists, owned, err := readWarningEntries(filepath.Join(dir, warningEntriesFileName))

	if err != nil {
		t.Fatal(err)
	}

	if exists && !owned {
		t.Fatalf("%s has no marker", warningEntriesFileName)
	}

	return entries, exists
}

// warningEntryCodes lists a model as `path:code` pairs, sorted, for one-line comparisons.
func warningEntryCodes(entries warningEntrySet) string {
	var pairs []string

	for _, relPath := range sortedStringKeys(entries) {
		for _, code := range sortedStringKeys(entries[relPath]) {
			pairs = append(pairs, relPath+":"+code)
		}
	}

	return strings.Join(pairs, " ")
}

const warningEntriesMain = `package main

func main() {
	println(sized(), half(), running, lowBits(1, 2), wide(3, 4), countWrites())
}
`

// TestWarningEntryFactsFollowTheEmission converts one fixture per fact shape, each in its own file so
// each is falsified on its own, beside controls that must NOT draw an entry, and reads the written
// sections back. The shapes are the census's own: runtime check()'s Sizeof-folded locals and bufio's
// folded local const (CS0219), exithook's never-written `running` and a blank package var (CS0649),
// time's `uint64(nsec) |` (CS0675).
func TestWarningEntryFactsFollowTheEmission(t *testing.T) {
	root := t.TempDir()

	dir := convertWarningEntriesFixture(t, root, "wef", map[string]string{
		"main.go": warningEntriesMain,
		// CS0219: `a` is only an unsafe.Sizeof operand, which folds.
		"sized.go": `package main

import "unsafe"

func sized() bool {
	var a int8
	return unsafe.Sizeof(a) == 1
}
`,
		// CS0219: `maxInt` is only inside a constant expression, which folds.
		"scan.go": `package main

func half() int {
	const maxInt = int(^uint(0) >> 1)
	return maxInt / 2
}
`,
		// CS0649: `running` is read and never written.
		"hooks.go": `package main

var running bool
`,
		// CS0649: `_` can never be written.
		"blank.go": `package main

type token struct{ p *int }

var _ token
`,
		// CS0675: a signed int32 widened to uint64 and or-ed with a variable.
		"bits.go": `package main

func lowBits(sec int64, nsec int32) uint64 {
	return uint64(sec)<<32 | uint64(nsec)
}
`,
		// Controls: a local that is read, a package var written in another file and one whose
		// address is taken, an exported var, and zero-extending (unsigned) and same-width or-s.
		"controls.go": `package main

var writes int

var addressed int

var Exported int

func wide(a uint32, b int64) uint64 {
	var c int8
	c++
	p := &addressed
	*p = int(c)
	return uint64(a) | uint64(b) | uint64(Exported)
}
`,
		"writer.go": `package main

func countWrites() int {
	writes++
	return writes
}
`,
	})

	entries, exists := readWarningEntriesFixture(t, dir)

	if !exists {
		t.Fatalf("no %s written beside the project file in %s", warningEntriesFileName, dir)
	}

	if got, want := warningEntryCodes(entries), "bits.cs:CS0675 blank.cs:CS0649 hooks.cs:CS0649 scan.cs:CS0219 sized.cs:CS0219"; got != want {
		t.Fatalf("entries = %q, want %q", got, want)
	}

	goos := runtime.GOOS

	for relPath, codes := range entries {
		for code, flavours := range codes {
			if len(flavours) != 1 || !flavours[goos] {
				t.Errorf("[/%s] %s flavours = %v, want only %s", relPath, code, sortedStringKeys(flavours), goos)
			}
		}
	}

	contents, err := os.ReadFile(filepath.Join(dir, warningEntriesFileName))

	if err != nil {
		t.Fatal(err)
	}

	text := string(contents)

	for _, want := range []string{
		warningEntriesMarker + "\r\n",
		"\r\n[/sized.cs]\r\n# CS0219: " + goos + "\r\ndotnet_diagnostic.CS0219.severity = none\r\n",
	} {
		if !strings.Contains(text, want) {
			t.Errorf("%s does not contain %q:\n%s", warningEntriesFileName, want, text)
		}
	}

	if strings.Contains(text, "root") {
		t.Errorf("%s must not declare root = true: the repository's own .editorconfig still applies\n%s", warningEntriesFileName, text)
	}

	// The facts go away with the code that held them, and with them the file.
	convertWarningEntriesFixture(t, root, "wef", map[string]string{
		"main.go": `package main

func main() {}
`,
	})

	if _, exists := readWarningEntriesFixture(t, dir); exists {
		t.Fatalf("%s survived a conversion with no facts left", warningEntriesFileName)
	}
}

// TestWarningEntriesNoUnassignedFieldUnderTheFriendGrant pins the CS0649 fact to the assembly C#
// reports it in: an in-package test file gives the production project the InternalsVisibleTo grant,
// under which C# never reports CS0649 on an internal field (net/http, runtime and time all carry it).
// The CS0219 fact in the same package still holds.
func TestWarningEntriesNoUnassignedFieldUnderTheFriendGrant(t *testing.T) {
	root := t.TempDir()

	dir := convertWarningEntriesFixture(t, root, "wfg", map[string]string{
		"main.go": `package main

import "unsafe"

var running bool

func main() {
	var a int8
	println(running, unsafe.Sizeof(a) == 1)
}
`,
		"main_test.go": `package main

import "testing"

func TestRunning(t *testing.T) {
	running = true
}
`,
	})

	entries, exists := readWarningEntriesFixture(t, dir)

	if !exists {
		t.Fatalf("no %s written beside the project file in %s", warningEntriesFileName, dir)
	}

	if got, want := warningEntryCodes(entries), "main.cs:CS0219"; got != want {
		t.Fatalf("entries = %q, want %q", got, want)
	}
}

// TestWarningEntriesLeaveAnUnmarkedFileAlone pins ruling 6: a package's own .editorconfig is never
// touched, whatever the conversion derives.
func TestWarningEntriesLeaveAnUnmarkedFileAlone(t *testing.T) {
	dir := t.TempDir()
	fileName := filepath.Join(dir, warningEntriesFileName)
	user := "[*.cs]\nindent_size = 4\n"

	if err := os.WriteFile(fileName, []byte(user), 0644); err != nil {
		t.Fatal(err)
	}

	if err := updateWarningEntries(dir, "linux", false, map[string][]string{"scan.cs": {warningUnusedLocal}}); err != nil {
		t.Fatal(err)
	}

	if err := updateWarningEntries(dir, "linux", false, nil); err != nil {
		t.Fatal(err)
	}

	contents, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatal(err)
	}

	if string(contents) != user {
		t.Fatalf("unmarked file changed:\n%s", contents)
	}

	if !reportedUnownedWarningEntries[unownedWarningEntriesKey(fileName)] {
		t.Fatalf("no warning recorded for the unmarked file")
	}
}

// TestWarningEntriesOwnership pins the two ownership splits: a run replaces only its own GOOS, and a
// production run never touches `_test.cs` sections (nor a -tests run the production ones).
func TestWarningEntriesOwnership(t *testing.T) {
	dir := t.TempDir()

	steps := []struct {
		goos      string
		testOwner bool
		facts     map[string][]string
	}{
		{"windows", false, map[string][]string{"windows/runtime1.cs": {warningUnusedLocal}, "time.cs": {warningSignExtendedOr}}},
		{"linux", false, map[string][]string{"linux/runtime1.cs": {warningUnusedLocal}, "time.cs": {warningSignExtendedOr}}},
		{"linux", true, map[string][]string{"scan_test.cs": {warningUnusedLocal}, "ignored.cs": {warningUnusedLocal}}},
		// windows reconverts and time.cs no longer holds the fact there: linux's flavour stays.
		{"windows", false, map[string][]string{"windows/runtime1.cs": {warningUnusedLocal}}},
	}

	for _, step := range steps {
		if err := updateWarningEntries(dir, step.goos, step.testOwner, step.facts); err != nil {
			t.Fatal(err)
		}
	}

	entries, _ := readWarningEntriesFixture(t, dir)

	want := warningEntrySet{}
	want.add("windows/runtime1.cs", warningUnusedLocal, "windows")
	want.add("linux/runtime1.cs", warningUnusedLocal, "linux")
	want.add("time.cs", warningSignExtendedOr, "linux")
	want.add("scan_test.cs", warningUnusedLocal, "linux")

	if got, wantText := entries.render(), want.render(); got != wantText {
		t.Fatalf("entries:\n%s\nwant:\n%s", got, wantText)
	}

	// A -tests run with no facts left removes only its own sections; the last production run with
	// none deletes the file.
	if err := updateWarningEntries(dir, "linux", true, nil); err != nil {
		t.Fatal(err)
	}

	for _, goos := range []string{"windows", "linux"} {
		if err := updateWarningEntries(dir, goos, false, nil); err != nil {
			t.Fatal(err)
		}
	}

	if _, err := os.Stat(filepath.Join(dir, warningEntriesFileName)); !os.IsNotExist(err) {
		t.Fatalf("file left behind with no entries: %v", err)
	}
}

// TestWarningEntriesRoundTrip pins that the rendered file parses back to the same model, so a run
// that changes nothing writes nothing.
func TestWarningEntriesRoundTrip(t *testing.T) {
	entries := warningEntrySet{}
	entries.add("linux/runtime1.cs", warningUnusedLocal, "linux")
	entries.add("hooks.cs", warningUnassignedField, "windows")
	entries.add("hooks.cs", warningUnassignedField, "darwin")
	entries.add("hooks.cs", warningUnassignedField, "linux")

	rendered := entries.render()
	parsed, owned := parseWarningEntries(rendered)

	if !owned || parsed.render() != rendered {
		t.Fatalf("round trip changed the file (owned=%v):\n%s\nvs\n%s", owned, rendered, parsed.render())
	}

	if !strings.Contains(rendered, "\r\n[/hooks.cs]\r\n# CS0649: darwin linux windows\r\n") {
		t.Fatalf("flavours not listed in order:\n%s", rendered)
	}
}

// TestMergeWarningEntriesAnchorsByThePlan composes a corpus file from two staged targets: a file
// both targets emitted identically lands flat with the union of their facts, a file that varies
// lands under each GOOS with that target's facts only, the corpus's other flavours and `_test.cs`
// sections stay, and a section whose file the run did not re-plan keeps its staged anchor.
func TestMergeWarningEntriesAnchorsByThePlan(t *testing.T) {
	root := t.TempDir()
	coreDir := filepath.Join(root, "core")
	targets := []string{"linux/amd64", "windows/amd64"}

	write := func(dir string, entries warningEntrySet) {
		t.Helper()

		if err := os.MkdirAll(dir, 0755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(filepath.Join(dir, warningEntriesFileName), []byte(entries.render()), 0644); err != nil {
			t.Fatal(err)
		}
	}

	corpus := warningEntrySet{}
	corpus.add("runtime1.cs", warningUnusedLocal, "linux")  // was flat on linux; stale after the merge
	corpus.add("runtime1.cs", warningUnusedLocal, "darwin") // another flavour: stays
	corpus.add("proc_test.cs", warningUnusedLocal, "linux") // a -tests section: stays
	write(filepath.Join(coreDir, "runtime"), corpus)

	var emissions []*platformEmission

	for _, target := range targets {
		goos := goosOfTarget(target)
		stageRoot := filepath.Join(root, "stage", goos, "src")
		staged := warningEntrySet{}
		staged.add(goos+"/runtime1.cs", warningUnusedLocal, goos)
		staged.add("time.cs", warningSignExtendedOr, goos)
		staged.add("mfinal.cs", warningUnassignedField, goos)
		write(filepath.Join(stageRoot, "core", "runtime"), staged)
		emissions = append(emissions, &platformEmission{target: target, goos: goos, root: stageRoot})
	}

	plans := map[string]mergedArtifact{
		"runtime/runtime1.cs": {class: "varies"},
		"runtime/time.cs":     {class: artifactIdentical},
		// mfinal.cs is not re-planned: its staged (flat) anchor stands.
	}

	if _, err := mergeWarningEntries(coreDir, targets, emissions, plans); err != nil {
		t.Fatal(err)
	}

	merged, _ := readWarningEntriesFixture(t, filepath.Join(coreDir, "runtime"))

	want := warningEntrySet{}
	want.add("runtime1.cs", warningUnusedLocal, "darwin")
	want.add("proc_test.cs", warningUnusedLocal, "linux")
	want.add("linux/runtime1.cs", warningUnusedLocal, "linux")
	want.add("windows/runtime1.cs", warningUnusedLocal, "windows")
	want.add("time.cs", warningSignExtendedOr, "linux")
	want.add("time.cs", warningSignExtendedOr, "windows")
	want.add("mfinal.cs", warningUnassignedField, "linux")
	want.add("mfinal.cs", warningUnassignedField, "windows")

	if got, wantText := merged.render(), want.render(); got != wantText {
		t.Fatalf("merged:\n%s\nwant:\n%s", got, wantText)
	}
}
