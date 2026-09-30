// csprojMetadata_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Every project in this tree — the two converter templates and the hand-written runtime and
// analyzer — carries the same assembly/package metadata block, in the same order, derived from the
// same two roots. The block is a CHAIN: Product names the project, Description and AssemblyTitle
// fall out of it, Authors falls out of Product, Company falls out of Authors, and Copyright falls
// out of Company. Written that way there is exactly one place to edit a name, and no way for two
// projects to disagree about it.
//
// It had drifted before these guards existed, in three separate directions at once: the two hand
// projects spelled Company and Copyright as literals while the template spelled the year with a
// printf verb, the test-host template omitted Authors and Copyright altogether, and go2cs-gen
// carried a SECOND, EMPTY <Description> after its real one — MSBuild takes the last, so the shipped
// go.gen package had an empty description and an empty AssemblyTitle. That last one is why
// TestNoProjectSetsAMetadataPropertyTwice exists: a duplicate is silent everywhere except in the
// published package.

package main

import (
	"encoding/json"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// metadataOrder is the block's required sequence. Order is part of the contract, not cosmetics: the
// chain reads top-down, so a Copyright written above the Company it interpolates still evaluates
// (MSBuild expands on use) but no longer documents where its value comes from.
var metadataOrder = []string{
	"Product",
	"Description",
	"AssemblyTitle",
	"Authors",
	"Company",
	"Copyright",
	"RepositoryUrl",
	"RepositoryType",
	"ApplicationIcon",
}

// metadataDerivations are the values that must be DERIVED rather than restated. Description is
// absent on purpose: the two hand projects prefix it with package-specific prose (it is their
// nuget.org copy) and only the tail is fixed, so it is asserted separately by suffix.
var metadataDerivations = map[string]string{
	"AssemblyTitle":  "$(Description)",
	"Authors":        "$(Product) Authors",
	"Company":        "The $(Authors)",
	"Copyright":      "Copyright © 2018-2026 $(Company)",
	"RepositoryUrl":  "https://github.com/ritchiecarroll/go2cs",
	"RepositoryType": "git",
}

// descriptionSuffix is the part of Description every project shares. The converter templates are
// exactly this; golib and go2cs-gen prepend prose and end with it.
const descriptionSuffix = "$(AssemblyName) ($(TargetFramework) - $(Configuration))"

// handWrittenProjects are the two projects the converter never regenerates, addressed relative to
// this package directory (the cwd `go test` gives a test binary).
var handWrittenProjects = map[string]string{
	"golib.csproj":     "../core/golib/golib.csproj",
	"go2cs-gen.csproj": "../gen/go2cs-gen/go2cs-gen.csproj",
}

// everyProject renders the two templates and reads the two hand-written projects, so a single loop
// states the contract over all four rather than over the two the converter happens to own.
func everyProject(t *testing.T) map[string]string {
	t.Helper()

	projects := map[string]string{
		"csproj-template.xml":      renderCsprojTemplate("Library", "", ""),
		"test-csproj-template.xml": renderTestCsprojTemplate(),
	}

	for name, path := range handWrittenProjects {
		contents, err := os.ReadFile(path)

		if err != nil {
			t.Fatalf("cannot read %s (%s): %v", name, path, err)
		}

		// Both hand-written projects are UTF-8 WITH a byte-order mark (the converter-emitted ones
		// are not); encoding/xml rejects one before the declaration.
		projects[name] = strings.TrimPrefix(string(contents), "\ufeff")
	}

	return projects
}

// The order is asserted over the properties each project actually declares, so a project that
// legitimately omits one (the analyzer packs no icon of its own the way an app does) is held to the
// order of what it does declare rather than forced to carry a property it has no use for.
func TestEveryProjectCarriesTheMetadataBlockInOrder(t *testing.T) {
	for name, contents := range everyProject(t) {
		groups := propertyGroupsOf(t, name, contents)

		var seen []string

		for _, group := range groups {
			for _, property := range group.Properties {
				for _, wanted := range metadataOrder {
					if property.XMLName.Local == wanted {
						seen = append(seen, wanted)
					}
				}
			}
		}

		next := 0

		for _, got := range seen {
			for next < len(metadataOrder) && metadataOrder[next] != got {
				next++
			}

			if next == len(metadataOrder) {
				t.Errorf("%s declares the metadata block out of order:\n  got  %s\n  want %s (subsequence)",
					name, strings.Join(seen, " -> "), strings.Join(metadataOrder, " -> "))

				break
			}

			next++
		}
	}
}

// The derived values are what make the block a single source of truth. A literal that happens to
// expand to the same string is still a defect: it is the copy that goes stale.
func TestEveryProjectDerivesItsMetadataRatherThanRestatingIt(t *testing.T) {
	for name, contents := range everyProject(t) {
		groups := propertyGroupsOf(t, name, contents)

		for property, want := range metadataDerivations {
			got := groups.value(property)

			if got == "" {
				t.Errorf("%s does not set <%s>", name, property)
				continue
			}

			if got != want {
				t.Errorf("%s sets <%s>%s</%s>, want %s", name, property, got, property, want)
			}
		}

		if got := groups.value("Product"); got != "go2cs" {
			t.Errorf("%s sets <Product>%s</Product>, want go2cs", name, got)
		}

		if got := groups.value("Description"); !strings.HasSuffix(got, descriptionSuffix) {
			t.Errorf("%s sets <Description>%s</Description>, which does not end with %s", name, got, descriptionSuffix)
		}
	}
}

// The Copyright is a LITERAL year range, not a printf verb over time.Now().Year(). The verb made
// every emitted .csproj in the corpus a function of the wall clock: the same converter, the same
// sources and the same flags produced different bytes on either side of New Year's Eve, so the
// first regeneration of each year reported the entire corpus as drifted.
func TestCopyrightIsDeterministic(t *testing.T) {
	if strings.Contains(string(csprojTemplate), "%d") {
		t.Error("csproj-template.xml still carries a printf year verb; the emitted copyright must not vary with the clock")
	}

	for name, contents := range everyProject(t) {
		if got := propertyGroupsOf(t, name, contents).value("Copyright"); strings.Contains(got, "%") {
			t.Errorf("%s sets <Copyright>%s</Copyright>, which still carries a format verb", name, got)
		}
	}
}

// go2cs-gen shipped an empty description for exactly this reason: a second <Description></Description>
// sat below the real one and MSBuild takes the last value. Nothing warns, nothing fails to build, and
// the only place it shows is the published package.
func TestNoProjectSetsAMetadataPropertyTwice(t *testing.T) {
	for name, contents := range everyProject(t) {
		counts := make(map[string]int)

		for _, group := range propertyGroupsOf(t, name, contents) {
			// A CONDITIONED group is a deliberate override, not a duplicate — the publish and
			// $(go2csPath) groups both restate properties under a condition on purpose.
			if group.Condition != "" {
				continue
			}

			for _, property := range group.Properties {
				counts[property.XMLName.Local]++
			}
		}

		for _, property := range metadataOrder {
			if counts[property] > 1 {
				t.Errorf("%s sets <%s> %d times unconditionally; MSBuild keeps the last, so the earlier one is dead",
					name, property, counts[property])
			}
		}
	}
}

// The framework a project targets is owned by src/Directory.Build.props, so a .NET hop is one edit
// rather than one per csproj family plus a whole-corpus regeneration. The property survives in each
// project only as a CONDITIONED fallback, for the trees where that props file is not above it:
// deploy-core.ps1 stages the corpus under a root that deliberately excludes core's props, and a
// -recurse conversion writes generated code under an arbitrary output root. Unconditional here would
// mean the hop silently skips every emitted project; absent entirely would mean those trees do not
// build at all.
func TestTemplatesLeaveTheTargetFrameworkOverridable(t *testing.T) {
	const want = "'$(TargetFramework)'==''"

	for _, name := range []string{"csproj-template.xml", "test-csproj-template.xml"} {
		contents := renderCsprojTemplate("Library", "", "")

		if name == "test-csproj-template.xml" {
			contents = renderTestCsprojTemplate()
		}

		condition, ok := elementCondition(propertyGroupsOf(t, name, contents), "TargetFramework")

		if !ok {
			t.Errorf("%s sets no <TargetFramework> at all; a tree without src/Directory.Build.props above it would not build", name)
			continue
		}

		if strings.ReplaceAll(condition, " ", "") != want {
			t.Errorf("%s guards <TargetFramework> with %q, want %s — an unconditional value cannot be hoisted", name, condition, want)
		}
	}
}

// elementCondition returns the Condition written on the PROPERTY itself (as opposed to
// propertyGroups.conditionOf, which reports the enclosing group's), and whether the property is
// declared at all.
func elementCondition(groups propertyGroups, name string) (string, bool) {
	for _, group := range groups {
		for _, property := range group.Properties {
			if property.XMLName.Local == name {
				return property.Condition, true
			}
		}
	}

	return "", false
}

// The language version is pinned, and pinned in ONE place, exactly as the framework is. `latest` moves
// with whichever SDK builds the tree: under a .NET 11 SDK it is C# 15, whose new contextual keywords
// can break the emitted corpus the way C# 14's `field` broke the source generator (issue #34), and
// nothing in the repository would have chosen that. 14 is the .NET 10 default. The value lives in
// src/Directory.Build.props and each emitted project carries it only as a CONDITIONED fallback, for the
// same trees the TargetFramework fallback serves (deploy-core staging, -recurse output roots), so the
// next hop is one line there.
const pinnedLangVersion = "14"

func TestTemplatesPinTheLanguageVersionConditionally(t *testing.T) {
	for _, name := range []string{"csproj-template.xml", "test-csproj-template.xml"} {
		contents := renderCsprojTemplate("Library", "", "")

		if name == "test-csproj-template.xml" {
			contents = renderTestCsprojTemplate()
		}

		condition, ok := elementCondition(propertyGroupsOf(t, name, contents), "LangVersion")

		if !ok {
			t.Errorf("%s sets no <LangVersion> at all", name)
			continue
		}

		if strings.ReplaceAll(condition, " ", "") != "'$(LangVersion)'==''" {
			t.Errorf("%s guards <LangVersion> with %q, want '$(LangVersion)'=='' -- an unconditional value cannot be hoisted", name, condition)
		}

		if !strings.Contains(contents, `<LangVersion Condition="'$(LangVersion)'==''">`+pinnedLangVersion+`</LangVersion>`) {
			t.Errorf("%s does not pin <LangVersion> to %s (`latest` follows the SDK, which is not a choice this repository made)", name, pinnedLangVersion)
		}
	}
}

func TestDirectoryBuildPropsCarriesTheLanguageVersionPin(t *testing.T) {
	props, err := os.ReadFile(filepath.Join(repoRootFromPackageDir(t), "src", "Directory.Build.props"))

	if err != nil {
		t.Fatalf("cannot read src/Directory.Build.props: %v", err)
	}

	if !strings.Contains(string(props), `<LangVersion Condition="'$(LangVersion)'==''">`+pinnedLangVersion+`</LangVersion>`) {
		t.Errorf("src/Directory.Build.props does not carry the conditioned <LangVersion>%s</LangVersion> the templates defer to", pinnedLangVersion)
	}
}

// global.json pins the SDK band, and NOTHING else: a `test` key would change `dotnet test`'s runner
// (docs/DotNetMigration.md step 4). The floor 10.0.100 with `latestFeature` admits every 10.0.x SDK on
// the fleet (the 1xx and 4xx bands) and refuses an 11.x SDK and a 9.x one. The `latestMajor` family is
// the trap: it is the only roll-forward that crosses a major, so it would admit 11.x.
func TestGlobalJsonPinsTheSdkBand(t *testing.T) {
	raw, err := os.ReadFile(filepath.Join(repoRootFromPackageDir(t), "global.json"))

	if err != nil {
		t.Fatalf("cannot read global.json: %v", err)
	}

	var doc map[string]any

	if err := json.Unmarshal(raw, &doc); err != nil {
		t.Fatalf("global.json is not valid JSON: %v", err)
	}

	if len(doc) != 1 {
		t.Errorf("global.json holds %d top-level keys, want only \"sdk\" (a `test` key changes the test runner)", len(doc))
	}

	sdk, _ := doc["sdk"].(map[string]any)

	if sdk == nil {
		t.Fatalf("global.json has no \"sdk\" object")
	}

	if sdk["version"] != "10.0.100" || sdk["rollForward"] != "latestFeature" || sdk["allowPrerelease"] != false || len(sdk) != 3 {
		t.Errorf("global.json sdk = %v, want version 10.0.100, rollForward latestFeature, allowPrerelease false and nothing else", sdk)
	}
}
