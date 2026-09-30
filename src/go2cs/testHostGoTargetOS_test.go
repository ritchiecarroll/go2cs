// conversionDriver_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The converted test host is built with `dotnet publish` against a csproj whose $(GoTargetOS)
// defaults to windows (platformDefaultTargetOS), and that property selects the per-GOOS source
// folder and the per-GOOS references the host compiles. The pipeline knows the platform a run is FOR
// (options.targetPlatform, the same value that routes the sources and scopes the disclosure
// manifest) but did not hand it to the build, so a linux -tests run compiled the WINDOWS flavour
// unless the caller happened to export GoTargetOS=linux: CS0426 for siginfo/sigctxt, CS0117 for
// syscall.Mmap (the i9, 2026-09-30). The build command must carry the target's GOOS itself, in both
// configurations, so the flavour a run compiles never depends on the caller's environment.

package main

import (
	"strings"
	"testing"
)

func targetOSProperties(args []string) []string {
	var found []string

	for _, arg := range args {
		if strings.HasPrefix(arg, "-p:GoTargetOS=") {
			found = append(found, arg)
		}
	}

	return found
}

func TestTestHostBuildCarriesTheTargetGOOS(t *testing.T) {
	for _, config := range []string{"Release", "Debug"} {
		for target, want := range map[string]string{
			"linux/amd64":   "-p:GoTargetOS=linux",
			"windows/amd64": "-p:GoTargetOS=windows",
			"darwin/arm64":  "-p:GoTargetOS=darwin",
		} {
			options := Options{targetPlatform: target, testConfig: config, go2csPath: "/repo/src"}

			args := publishTestHostArgs("/out", "/out/p.tests.csproj", options)
			got := targetOSProperties(args)

			if len(got) != 1 || got[0] != want {
				t.Errorf("%s host build for %s carries %v, want exactly [%s]; args: %v", config, target, got, want, args)
			}
		}
	}
}

// A run with no target platform set (a hand-built Options) must not invent one: the csproj default
// stands, exactly as before.
func TestTestHostBuildWithoutATargetLeavesTheCsprojDefault(t *testing.T) {
	args := publishTestHostArgs("/out", "/out/p.tests.csproj", Options{testConfig: "Release", go2csPath: "/repo/src"})

	if got := targetOSProperties(args); len(got) != 0 {
		t.Errorf("no target platform, yet the build carries %v", got)
	}
}

// Everything else about the command is unchanged by the property.
func TestTestHostBuildKeepsItsExistingShape(t *testing.T) {
	release := publishTestHostArgs("/out", "/out/p.tests.csproj", Options{targetPlatform: "linux/amd64", testConfig: "Release", go2csPath: "/repo/src"})
	joined := strings.Join(release, " ")

	for _, want := range []string{"publish /out/p.tests.csproj", "-c Release", "-p:go2csPath=/repo/src/", "-o "} {
		if !strings.Contains(joined, want) {
			t.Errorf("Release command %q lost %q", joined, want)
		}
	}

	debug := strings.Join(publishTestHostArgs("/out", "/out/p.tests.csproj", Options{targetPlatform: "linux/amd64"}), " ")

	if !strings.Contains(debug, "-c Debug") || strings.Contains(debug, "go2csPath") {
		t.Errorf("Debug command %q changed shape", debug)
	}
}
