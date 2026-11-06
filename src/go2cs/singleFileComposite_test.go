// singleFileComposite_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the single-file publish of a converted program (csproj-template.xml): a plain `dotnet publish
// -p:PublishSingleFile=true` compiles ReadyToRun per assembly, as the shipped single-file profiles already do
// (TestSingleFileProfilesCompileReadyToRunPerAssembly). The SDK otherwise defaults a self-contained single-file
// ReadyToRun publish to ONE composite image (Microsoft.NET.CrossGen.targets), and the template compresses the
// bundle, so every process start decompresses it: about 3.5 s of start-up on win-x64 (docs/Tuning.md). The
// template's value yields to one the caller sets, so a composite build stays one property away.

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// TestTemplateTurnsTheCompositeReadyToRunImageOffUnlessAsked requires the executable group to set
// PublishReadyToRunComposite to false only where the caller has not set it.
func TestTemplateTurnsTheCompositeReadyToRunImageOffUnlessAsked(t *testing.T) {
	properties := propertyGroupsOf(t, "csproj-template.xml", renderCsprojTemplate("Exe", "", ""))
	condition, found := properties.conditionOf("PublishReadyToRunComposite")

	if !found {
		t.Fatal("the rendered project sets no <PublishReadyToRunComposite>: a plain single-file publish takes the SDK's composite image")
	}

	if compression, _ := properties.conditionOf("EnableCompressionInSingleFile"); condition != compression {
		t.Errorf("<PublishReadyToRunComposite> is in a PropertyGroup with Condition %q, not EnableCompressionInSingleFile's %q", condition, compression)
	}

	if !strings.Contains(renderCsprojTemplate("Exe", "", ""), `<PublishReadyToRunComposite Condition="'$(PublishReadyToRunComposite)'==''">false</PublishReadyToRunComposite>`) {
		t.Error("<PublishReadyToRunComposite> must be false, and must yield to a value the caller sets")
	}
}

// TestPlainSingleFilePublishEvaluatesReadyToRunPerAssembly asks the installed SDK what a plain single-file publish
// of the rendered project evaluates to: per assembly, and composite when the caller asks for it.
func TestPlainSingleFilePublishEvaluatesReadyToRunPerAssembly(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: evaluates with the installed .NET SDK")
	}

	dotnet, err := exec.LookPath("dotnet")

	if err != nil {
		t.Skip("NOT RUN: no dotnet on PATH")
	}

	rid, ok := hostRuntimeIdentifier(runtime.GOOS, runtime.GOARCH)

	if !ok {
		t.Fatalf("no .NET runtime identifier is mapped for %s/%s; add one to hostRuntimeIdentifier", runtime.GOOS, runtime.GOARCH)
	}

	root := t.TempDir()
	project := filepath.Join(root, "app", "app.csproj")
	writeModuleFile(t, project, renderCsprojTemplate("Exe", "", ""))

	composite := regexp.MustCompile(`"PublishReadyToRunComposite":\s*"([^"]*)"`)
	selfContained := regexp.MustCompile(`"SelfContained":\s*"([^"]*)"`)

	// _IsPublishing is what `dotnet publish` sets: with it a single-file publish is self-contained, the case in which
	// the SDK's composite default applies at all.
	evaluate := func(arm string, extra ...string) string {
		t.Helper()
		args := append([]string{"msbuild", project, "-getProperty:PublishReadyToRunComposite", "-getProperty:SelfContained", "-p:PublishSingleFile=true",
			"-p:_IsPublishing=true", "-p:RuntimeIdentifier=" + rid, "-nodeReuse:false"}, extra...)
		cmd := exec.Command(dotnet, args...)
		cmd.Dir = root
		cmd.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
		output, err := cmd.CombinedOutput()

		if err != nil {
			t.Fatalf("%s arm: `dotnet %s` failed, so nothing was evaluated: %v\n%s", arm, strings.Join(args, " "), err, lastLines(string(output), 25))
		}

		match := composite.FindStringSubmatch(string(output))
		contained := selfContained.FindStringSubmatch(string(output))

		if match == nil || contained == nil {
			t.Fatalf("%s arm: the evaluation printed no PublishReadyToRunComposite or SelfContained:\n%s", arm, lastLines(string(output), 25))
		}

		// The SDK forces the composite image off for a framework-dependent publish, so only a self-contained one tests
		// the template's value.
		if contained[1] != "true" {
			t.Fatalf("%s arm: the publish evaluated SelfContained=%q, so the SDK's composite default never applied", arm, contained[1])
		}

		return match[1]
	}

	if got := evaluate("plain"); got != "false" {
		t.Errorf("a plain single-file publish evaluates PublishReadyToRunComposite=%q, want false (per-assembly ReadyToRun)", got)
	}

	if got := evaluate("opt-in", "-p:PublishReadyToRunComposite=true"); got != "true" {
		t.Errorf("a single-file publish that asks for the composite image evaluates PublishReadyToRunComposite=%q, want true", got)
	}
}
