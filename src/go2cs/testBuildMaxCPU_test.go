// testBuildMaxCPU_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// -test-build-maxcpu (the owner's ruling, 2026-10-18): the converted test host's dotnet publish, the
// only dotnet command a -tests run starts, takes -m:N when asked. The i9 builds under a 4-CPU cap, and
// its publishes still ran 24 MSBuild nodes: a Directory.Build.rsp in the tree, MSBUILDNODECOUNT and
// the defaults all gave 24, and only an explicit -m:4 on the command line gave 4. Unset, the command
// is exactly what it was.

package main

import (
	"slices"
	"strconv"
	"strings"
	"testing"
)

func maxCPUArguments(args []string) []string {
	var found []string

	for _, arg := range args {
		lower := strings.ToLower(arg)

		if strings.HasPrefix(lower, "-m:") || lower == "-m" || strings.HasPrefix(lower, "-maxcpucount") {
			found = append(found, arg)
		}
	}

	return found
}

func TestTestHostPublishCarriesTheMaxCPUWhenSet(t *testing.T) {
	for _, config := range []string{"Release", "Debug"} {
		for _, nodes := range []int{1, 2, 4} {
			options := Options{targetPlatform: "linux/amd64", testConfig: config, go2csPath: "/repo/src", testBuildMaxCPU: nodes}

			args := publishTestHostArgs("/out", "/out/p.tests.csproj", options)
			want := "-m:" + strconv.Itoa(nodes)

			if got := maxCPUArguments(args); len(got) != 1 || got[0] != want {
				t.Errorf("%s publish with -test-build-maxcpu %d carries %v, want exactly [%s]; args: %v", config, nodes, got, want, args)
			}
		}
	}
}

func TestTestHostPublishIsUnchangedWhenTheMaxCPUIsUnset(t *testing.T) {
	for _, config := range []string{"Release", "Debug"} {
		unset := Options{targetPlatform: "linux/amd64", testConfig: config, go2csPath: "/repo/src"}
		args := publishTestHostArgs("/out", "/out/p.tests.csproj", unset)

		if got := maxCPUArguments(args); len(got) != 0 {
			t.Errorf("%s publish without -test-build-maxcpu carries %v; args: %v", config, got, args)
		}

		set := unset
		set.testBuildMaxCPU = 2
		withCap := publishTestHostArgs("/out", "/out/p.tests.csproj", set)

		if without := slices.DeleteFunc(slices.Clone(withCap), func(arg string) bool { return arg == "-m:2" }); !slices.Equal(without, args) {
			t.Errorf("%s publish: -m:2 changed more than its own argument\n  unset: %v\n  set:   %v", config, args, withCap)
		}
	}
}
