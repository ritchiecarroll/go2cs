// csprojTemplate_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/xml"
	"fmt"
	"io"
	"path/filepath"
	"runtime"
	"sort"
	"strings"
	"testing"
)

// fixtureSrcRoot is an absolute go2cs `src` root spelled the way the HOST spells one. The fixtures
// below used to hard-code `H:\Projects\go2cs\src`, which is not a portability wart but a real test
// defect off Windows: testsRewriteOfCorePackage compares a cleaned project path against
// filepath.Join(root, "core") + filepath.Separator, and on Linux a backslash literal is a single
// FILENAME with no separators at all -- so the structural "under <root>/core/" test can never match
// and TestValidationPackBlockSurvivesTestsRewriteOfCorePackage fails for a reason that has nothing
// to do with what it guards. Measured as one of the five real Linux-host findings (F4,
// docs/PLAN-linux-operation.md). The Windows spelling is preserved exactly, so this lane's fixtures
// are byte-identical to the ones that have always run here.
func fixtureSrcRoot() string {
	if runtime.GOOS == "windows" {
		return `H:\Projects\go2cs\src`
	}

	return "/projects/go2cs/src"
}

// fixturePath joins path elements onto fixtureSrcRoot with the host's separator.
func fixturePath(elements ...string) string {
	return filepath.Join(append([]string{fixtureSrcRoot()}, elements...)...)
}

// The embedded csproj templates are emitted verbatim into every converted project, so a malformed
// one breaks the whole corpus at once and only surfaces at COMPILE time — the behavioral suite
// caught an `--` inside an XML comment (illegal per the XML spec, MSB4025 "An XML comment cannot
// contain '--'") 457 s into a full run, in all 495 projects simultaneously. These guards are the
// same check in ~10 ms, before the exe is even used.
//
// The templates are not valid XML on their own: csproj-template.xml is a printf format string and
// test-csproj-template.xml carries `>>MARKER:…<<` placeholders. Both are substituted here exactly
// as the emitters do, so what is validated is what actually reaches disk.

// renderCsprojTemplate applies the same two-stage substitution writeProjectFile does: the printf
// verbs first, then the post-render markers. validationPack is the stdlib-only VALIDATION.md pack
// block, which collapses to "" for every other conversion — both forms reach disk, so both are
// validated here.
func renderCsprojTemplate(outputType string, reference string, validationPack string) string {
	contents := fmt.Sprintf(string(csprojTemplate),
		outputType,
		"go",
		"TestProject",
		"false",
		reference,
	)

	return strings.ReplaceAll(contents, ValidationPackMarker, validationPack)
}

func TestCsprojTemplateEmitsWellFormedXml(t *testing.T) {
	contents := renderCsprojTemplate("Exe", `    <ProjectReference Include="$(go2csPath)core\fmt\fmt.csproj" />`, "")

	if strings.Contains(contents, ">>MARKER:") {
		t.Fatalf("csproj-template.xml has an unsubstituted marker; update renderCsprojTemplate")
	}

	if err := assertWellFormedXml(contents); err != nil {
		t.Fatalf("csproj-template.xml does not emit well-formed XML: %v", err)
	}
}

// The VALIDATION.md pack block is built in Go and injected into every converted stdlib .csproj, so a
// malformed one breaks the whole published corpus at pack time. This is that block, in place.
func TestCsprojTemplateWithValidationPackEmitsWellFormedXml(t *testing.T) {
	block := validationPackBlock(fixturePath("core", "path", "filepath", "path.filepath.csproj"), Options{convertStdLib: true})

	if !strings.Contains(block, `path.filepath.md`) {
		t.Fatalf("the validation pack block does not name the package's proof sheet: %s", block)
	}

	contents := renderCsprojTemplate("Library", `    <ProjectReference Include="$(go2csPath)core\fmt\fmt.csproj" />`, block)

	if !strings.Contains(contents, `PackagePath="VALIDATION.md"`) {
		t.Fatal("the validation pack block was not substituted into the template")
	}

	if err := assertWellFormedXml(contents); err != nil {
		t.Fatalf("csproj template with the validation pack block does not emit well-formed XML: %v", err)
	}
}

// A non-stdlib conversion must emit the .csproj it always did: the marker's whole line collapses to
// the blank line the template has always had between the README and source-generator sections.
// Behavioral-test and -recurse output is byte-compared against goldens, so "no block" is not enough
// — the surrounding bytes have to be unchanged too.
func TestValidationPackMarkerCollapsesToBlankLine(t *testing.T) {
	if block := validationPackBlock(`C:\out\src\tests\Behavioral\Arrays\Arrays.csproj`, Options{}); block != "" {
		t.Fatalf("a non-stdlib conversion emitted a validation pack block: %q", block)
	}

	contents := renderCsprojTemplate("Library", "", "")

	if !strings.Contains(contents, "</ItemGroup>\r\n\r\n  <!-- Expose output of source generators as local files -->") {
		t.Fatal("collapsing the validation pack marker did not leave the template's original blank line")
	}
}

// A -tests run re-emits the production .csproj of the package under test, and for a converted
// stdlib package that rewrite must PRESERVE the validation pack block: gating on convertStdLib
// alone stripped it on every pipeline run (the "0 8" restore family), which would have silently
// un-shipped every validated package's proof sheet at the next NuGet pack. The -tests arm is
// structural — output under the runtime root's core\ tree — so a fixture or end-user module
// still collapses to the historical blank line.
// The fixture paths are composed with filepath.Join rather than written as Windows literals: the
// predicate under test is a real path-prefix comparison (filepath.Clean over a separator-joined
// root), so a `H:\…\src\core\time\time.csproj` literal is ONE filename on Linux and macOS and the
// prefix never matches — the guard failed there for a reason that has nothing to do with what it
// guards. Built from segments it asserts the same property on every host.
func TestValidationPackBlockSurvivesTestsRewriteOfCorePackage(t *testing.T) {
	root := filepath.Join(t.TempDir(), "src")
	testsOverCore := Options{convertTests: true, go2csPath: root}

	corePackage := filepath.Join(root, "core", "time", "time.csproj")
	block := validationPackBlock(corePackage, testsOverCore)

	if !strings.Contains(block, `time.md`) || !strings.Contains(block, `PackagePath="VALIDATION.md"`) {
		t.Fatalf("a -tests rewrite of a core package's production .csproj lost the validation pack block: %q", block)
	}

	outsideCore := filepath.Join(root, "tests", "PackageTests", "ConvertedTestHarness", "value.csproj")

	if block := validationPackBlock(outsideCore, testsOverCore); block != "" {
		t.Fatalf("a -tests conversion outside the core tree emitted a validation pack block: %q", block)
	}

	if block := validationPackBlock(corePackage, Options{convertTests: true}); block != "" {
		t.Fatalf("a -tests conversion with no resolved runtime root emitted a validation pack block: %q", block)
	}

	// The SINGLE-PACKAGE form — `go2cs <goroot-pkg> <core-pkg>`, how a lane regenerates one corpus
	// package after a converter change — is neither -stdlib nor -tests, and it stripped the block
	// exactly as the -tests path once did. The predicate is on the output LOCATION, so it holds
	// whatever the invocation mode.
	singlePackage := Options{go2csPath: root}

	if block := validationPackBlock(corePackage, singlePackage); !strings.Contains(block, `time.md`) {
		t.Fatalf("a single-package reconvert of a core package's .csproj lost the validation pack block: %q", block)
	}

	if block := validationPackBlock(outsideCore, singlePackage); block != "" {
		t.Fatalf("a single-package conversion outside the core tree emitted a validation pack block: %q", block)
	}
}

// The friend-assembly grant is inserted AFTER template rendering — never as a template verb, so a
// user-supplied `-csproj` template (which cannot know about the slot) keeps rendering correctly —
// and this guard validates the INSERTED document, the shape that actually reaches disk for every
// package with build-selected in-package tests.
func TestCsprojTemplateWithFriendAssemblyAccessEmitsWellFormedXml(t *testing.T) {
	contents := renderCsprojTemplate("Exe", `    <ProjectReference Include="$(go2csPath)core\fmt\fmt.csproj" />`, "")

	contents = insertFriendAssemblyAccess(contents)

	if !strings.Contains(contents, `<InternalsVisibleTo Include="$(AssemblyName).tests" />`) {
		t.Fatal("the friend-assembly ItemGroup was not inserted")
	}

	if err := assertWellFormedXml(contents); err != nil {
		t.Fatalf("csproj template with friend-assembly access does not emit well-formed XML: %v", err)
	}
}

// renderTestCsprojTemplate applies the same marker substitution writeTestProject (testConversion.go)
// does, so what is validated is what actually reaches disk.
func renderTestCsprojTemplate() string {
	contents := string(testCsprojTemplate)

	for marker, value := range map[string]string{
		TestRootNamespaceMarker:     "go",
		TestAssemblyNameMarker:      "TestProject.tests",
		TestGo2CSRelativePathMarker: `..\..\`,
		TestCompileItemsMarker:      "\r\n    <Compile Include=\"value.cs\" />",
		TestFixtureItemsMarker:      "",
		TestProjectReferencesMarker: "\r\n    <ProjectReference Include=\"$(go2csPath)core\\testing\\testing.csproj\" />",
	} {
		contents = strings.ReplaceAll(contents, marker, value)
	}

	return contents
}

func TestTestCsprojTemplateEmitsWellFormedXml(t *testing.T) {
	contents := renderTestCsprojTemplate()

	if strings.Contains(contents, ">>MARKER:") {
		t.Fatalf("test-csproj-template.xml has an unsubstituted marker; update this guard's marker map")
	}

	if err := assertWellFormedXml(contents); err != nil {
		t.Fatalf("test-csproj-template.xml does not emit well-formed XML: %v", err)
	}
}

// F5 guard — the emitted corpus has ONE separator form, and it is `/`.
//
// MSBuild accepts forward slashes in every path context on Windows, and normalizes backslashes to
// forward ones on Unix, so BOTH forms build on both hosts. What does not survive the round trip is
// the converter's own hand-rolled path arithmetic (filepath.Join treats `\` as an ordinary filename
// character on Unix) and every consumer of an emitted reference that is NOT MSBuild —
// BehavioralRunner/PerformanceRunner resolve `ProjectReference Include=` through
// Path.GetFullPath, which on Linux does not split on `\` either. Emitting `/` universally is what
// makes one corpus correct for every host; see docs/PLAN-linux-operation.md §A1.1.
//
// The templates carry no backslash for any other purpose, so the assertion can be the whole file:
// anything a future edit adds — an OutDir, an Exists() probe, a fixed reference — is covered without
// this guard needing to enumerate it.
func TestEmbeddedCsprojTemplatesUseForwardSlashesOnly(t *testing.T) {
	for name, contents := range map[string]string{
		"csproj-template.xml":      string(csprojTemplate),
		"test-csproj-template.xml": string(testCsprojTemplate),
	} {
		if idx := strings.IndexByte(contents, '\\'); idx >= 0 {
			t.Errorf("%s carries a backslash at offset %d (%q); emitted MSBuild paths must use forward slashes",
				name, idx, lineAround(contents, idx))
		}
	}
}

// The VALIDATION.md pack block is composed in Go rather than in the template, so it needs the same
// assertion applied where it is built.
func TestValidationPackBlockUsesForwardSlashesOnly(t *testing.T) {
	block := validationPackBlock(filepath.Join("root", "core", "fmt", "fmt.csproj"), Options{convertStdLib: true})

	if block == "" {
		t.Fatal("validationPackBlock returned empty for a stdlib conversion")
	}

	if strings.Contains(block, `\`) {
		t.Errorf("validation pack block carries a backslash:\n%s", block)
	}
}

// lineAround returns the line containing idx, for a legible failure message.
func lineAround(contents string, idx int) string {
	start := strings.LastIndexByte(contents[:idx], '\n') + 1
	end := strings.IndexByte(contents[idx:], '\r')

	if end < 0 {
		return contents[start:]
	}

	return contents[start : idx+end]
}

// A reference path is the one part of a rendered csproj built from user-controlled text rather than
// from the template or from a Go import path. Under `-recurse` it starts as an absolute path beneath
// the user's output root and is only made relative when filepath.Rel succeeds, which it cannot do
// across Windows volumes — so an output root like `D:\R&D\out` reaches the emitter with its `&`
// intact. writeProjectFile escapes it; this guard is that escape, with its own negative control, so
// a future edit cannot quietly drop the call and still pass.
func TestProjectReferenceWithXmlSpecialsStaysWellFormed(t *testing.T) {
	reference := `..\..\pkg\R&D\"lib"\<lib>.csproj`

	escaped := renderCsprojTemplate("Library", fmt.Sprintf(`    <ProjectReference Include="%s" />`, escapeXMLAttributeValue(reference)), "")

	if err := assertWellFormedXml(escaped); err != nil {
		t.Fatalf("an escaped reference path does not emit well-formed XML: %v", err)
	}

	// Negative control: the same path unescaped must NOT parse. Without this the test would pass
	// even if escapeXMLAttributeValue became the identity function.
	unescaped := renderCsprojTemplate("Library", fmt.Sprintf(`    <ProjectReference Include="%s" />`, reference), "")

	if err := assertWellFormedXml(unescaped); err == nil {
		t.Fatal("an unescaped reference path parsed as well-formed XML, so this guard proves nothing")
	}
}

// structuralNoWarnCodes is the suppression policy ruled in docs/phase4/DESIGN-warning-suppression.md
// (§3 re-justifies the original eight, §4 classifies the six added at r46b). Every entry is a
// diagnostic that is STRUCTURAL to the Go emission model — Go type names (CS8981, CS8860), a struct
// split across its type file and package_info.cs (CS0282), Go comparison operators on value types
// (CS0660, CS0661), Go zero values (CS8618), generated unreachable code and labels (CS0162, CS0164),
// the generated named-return self-assignment (CS1717), the NaN idiom (CS1718), a function value in a
// `map[string]any` (CS8974), `init()` as a module initializer (CA2255), and the two IDE codes that
// are inert at the CLI but deafening in Visual Studio.
//
// The list is asserted EXACTLY, in both templates, for two reasons. Adding a code is a policy
// decision that belongs in the design doc, and this makes the doc update unskippable. Removing one
// is almost always wrong: the converter re-creates these on the very next conversion, so a dropped
// entry does not fix anything, it only makes the corpus build loud again. Codes deliberately left
// VISIBLE (CS0219, CS8778, CS0675, CS8500, CS8826, CS0252, CS0649, CS1522) are converter or golib
// defects wearing a warning's clothes — the doc's do-not-suppress rulings — and must never appear here.
var structuralNoWarnCodes = []string{
	"CS0162", "CS0164", "CS0282", "CS0660", "CS0661", "CS1717", "CS1718",
	"CS8618", "CS8860", "CS8974", "CS8981", "IDE0060", "IDE1006", "CA2255",
}

// The suppression policy is ONE decision applied to TWO templates: csproj-template.xml (production)
// and test-csproj-template.xml (the -tests host). They are separate files with separate edit paths,
// so nothing but a guard keeps them in step — and a package under -tests recompiles its production
// sources into the test assembly, where a divergent list would make the same source warn in one
// build and not the other.
func TestBothCsprojTemplatesCarryTheSameSuppressionPolicy(t *testing.T) {
	production := renderCsprojTemplate("Library", "", "")
	tests := renderTestCsprojTemplate()

	for name, contents := range map[string]string{
		"csproj-template.xml":      production,
		"test-csproj-template.xml": tests,
	} {
		properties := propertyGroupsOf(t, name, contents)

		if got := properties.value("Nullable"); got != "annotations" {
			t.Errorf("%s sets <Nullable>%s</Nullable>; Go has no non-nullable reference type, so the policy is `annotations`", name, got)
		}

		got := strings.Split(properties.value("NoWarn"), ";")
		sort.Strings(got)

		want := append([]string(nil), structuralNoWarnCodes...)
		sort.Strings(want)

		if strings.Join(got, ";") != strings.Join(want, ";") {
			t.Errorf("%s <NoWarn> is\n  %s\nbut the design ruled\n  %s", name, strings.Join(got, ";"), strings.Join(want, ";"))
		}
	}
}

// The publish properties are scoped off Library projects because PublishTrimmed doubles as
// EnableTrimAnalyzer at BUILD time — every IL#### warning in the corpus came from that one line, on
// projects that are never published. AllowUnsafeBlocks shares the group's history but NOT its
// reasoning: it is a compile setting, and the converted stdlib is full of library packages that do
// not compile without it. Moving it under the condition would break the corpus in a way no warning
// count would reveal, so both halves are pinned here.
func TestPublishPropertiesAreScopedOffLibrariesButAllowUnsafeBlocksIsNot(t *testing.T) {
	properties := propertyGroupsOf(t, "csproj-template.xml", renderCsprojTemplate("Library", "", ""))

	condition, found := properties.conditionOf("PublishTrimmed")

	if !found {
		t.Fatal("the rendered project sets no <PublishTrimmed> at all")
	}

	if !strings.Contains(condition, "'$(OutputType)'!='Library'") {
		t.Errorf("<PublishTrimmed> is in a PropertyGroup with Condition %q; it must be scoped off Library projects", condition)
	}

	condition, found = properties.conditionOf("AllowUnsafeBlocks")

	if !found {
		t.Fatal("the rendered project sets no <AllowUnsafeBlocks> at all")
	}

	if condition != "" {
		t.Errorf("<AllowUnsafeBlocks> is in a PropertyGroup with Condition %q; it is a compile setting and must apply to every project", condition)
	}
}

// msbuildProperty is one element inside a <PropertyGroup>: its name, its own Condition and
// its text. The element-level Condition matters independently of the enclosing group's: it is
// how ONE property is made overridable (TargetFramework defers to src/Directory.Build.props
// that way) without splitting it into a conditioned group of its own.
type msbuildProperty struct {
	XMLName   xml.Name
	Condition string `xml:"Condition,attr"`
	Value     string `xml:",chardata"`
}

// propertyGroup is a rendered project's <PropertyGroup>, paired with its Condition — enough to
// answer both "what value is set" and "under what condition".
type propertyGroup struct {
	Condition  string            `xml:"Condition,attr"`
	Properties []msbuildProperty `xml:",any"`
}

type propertyGroups []propertyGroup

// value returns the LAST value set for name, which is the one MSBuild evaluates; "" when unset.
func (groups propertyGroups) value(name string) string {
	value := ""

	for _, group := range groups {
		for _, property := range group.Properties {
			if property.XMLName.Local == name {
				value = property.Value
			}
		}
	}

	return value
}

// conditionOf returns the Condition of the first group that sets name ("" for an unconditional
// group), and whether any group sets it at all.
func (groups propertyGroups) conditionOf(name string) (string, bool) {
	for _, group := range groups {
		for _, property := range group.Properties {
			if property.XMLName.Local == name {
				return group.Condition, true
			}
		}
	}

	return "", false
}

// propertyGroupsOf parses a rendered project through encoding/xml — the properties are read from the
// document MSBuild would load, not from a substring of the template.
func propertyGroupsOf(t *testing.T, name string, contents string) propertyGroups {
	t.Helper()

	var document struct {
		Groups propertyGroups `xml:"PropertyGroup"`
	}

	if err := xml.Unmarshal([]byte(contents), &document); err != nil {
		t.Fatalf("%s did not parse: %v", name, err)
	}

	return document.Groups
}

// assertWellFormedXml streams the document through encoding/xml, which enforces the comment rules
// MSBuild's loader enforces (no `--` in a comment body, no trailing `-`).
func assertWellFormedXml(contents string) error {
	decoder := xml.NewDecoder(strings.NewReader(contents))

	for {
		_, err := decoder.Token()

		if err == io.EOF {
			return nil
		}

		if err != nil {
			return err
		}
	}
}
