// stackAppendOption_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestParseStackAppendMode(t *testing.T) {
	for value, want := range map[string]stackAppendMode{"": stackAppendAuto, "auto": stackAppendAuto, "on": stackAppendOn, "OFF": stackAppendOff, " on ": stackAppendOn} {
		if got, err := parseStackAppendMode(value); err != nil || got != want {
			t.Errorf("parseStackAppendMode(%q) = %v, %v; want %v", value, got, err, want)
		}
	}

	if _, err := parseStackAppendMode("yes"); err == nil || !strings.Contains(err.Error(), `"yes"`) {
		t.Errorf("parseStackAppendMode(\"yes\") = %v; want an error naming the value", err)
	}
}

// TestStackAppendModeResolves: auto is on for -stdlib and -tests and off for plain and -recurse conversions (the
// owner's ruling, 2026-11-24); on and off win over the conversion kind.
func TestStackAppendModeResolves(t *testing.T) {
	for _, c := range []struct {
		name    string
		options Options
		want    bool
	}{
		{"auto plain", Options{}, false},
		{"auto recurse", Options{recurse: true}, false},
		{"auto stdlib", Options{convertStdLib: true}, true},
		{"auto tests", Options{convertTests: true}, true},
		{"on plain", Options{stackAppend: stackAppendOn}, true},
		{"on recurse", Options{recurse: true, stackAppend: stackAppendOn}, true},
		{"off stdlib", Options{convertStdLib: true, stackAppend: stackAppendOff}, false},
		{"off tests", Options{convertTests: true, stackAppend: stackAppendOff}, false},
	} {
		if got := stackAppendEnabled(c.options); got != c.want {
			t.Errorf("%s: stackAppendEnabled = %v, want %v", c.name, got, c.want)
		}
	}
}

// TestStackAppendOffRunsNoProbe converts one module twice with the real harvest: at the default (auto, which a
// -recurse conversion resolves off) and with -stack-append=on. Off must emit plain append, run no harvest (so no
// `go build -gcflags=-m`: stackAppendHarvest builds the only such command) and leave the probe cache uncreated; the
// on arm is the control that the same module and seam do harvest, create the cache and emit the buffer.
// RED with the gate removed from collectStackAppendHints: the default arm harvests and declares xᴷ.
func TestStackAppendOffRunsNoProbe(t *testing.T) {
	isolateConverterPathGlobals(t)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	savedHarvest, savedResults, savedFlag := stackAppendHarvest, stackAppendHarvests, probeCacheFlag
	t.Cleanup(func() {
		stackAppendHarvest, stackAppendHarvests, probeCacheFlag = savedHarvest, savedResults, savedFlag
		packageStackAppendSites, packageStackAppendBuffers = nil, nil
	})

	runs := 0
	stackAppendHarvest = func(dir string, tests bool, target string, options Options) string {
		runs++
		return savedHarvest(dir, tests, target, options)
	}

	for _, arm := range []struct {
		name string
		mode stackAppendMode
		on   bool
	}{{"default", stackAppendAuto, false}, {"on", stackAppendOn, true}} {
		root := t.TempDir()
		appDir := filepath.Join(root, "app")
		probeCache := filepath.Join(root, "probe-cache")
		probeCacheFlag = probeCache
		stackAppendHarvests = map[string]*stackAppendHarvestResult{}
		runs = 0

		writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/sappend\n\ngo 1.25\n")
		writeModuleFile(t, filepath.Join(appDir, "sappend.go"), stackAppendEmissionFixture)

		options := Options{
			goRoot:              goRoot,
			goPath:              build.Default.GOPATH,
			go2csPath:           filepath.Join(root, "out"),
			recurse:             true,
			stackAppend:         arm.mode,
			targetPlatform:      "linux/amd64",
			indentSpaces:        4,
			preferVarDecl:       true,
			useChannelOperators: true,
		}

		build.Default.GOROOT = options.goRoot
		build.Default.GOPATH = options.goPath

		if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
			t.Fatalf("%s: ConvertModule: %v", arm.name, err)
		}

		emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "sappend", "sappend.cs")), "\r\n", "\n")
		_, statErr := os.Stat(probeCache)
		cacheExists := statErr == nil

		if arm.on {
			if runs == 0 || !cacheExists || !strings.Contains(emitted, "x = append(xᴷ, x, (int64)i);") {
				t.Errorf("on: %d harvests, probe cache created %v, want the buffer in:\n%s", runs, cacheExists, emitted)
			}

			continue
		}

		if runs != 0 {
			t.Errorf("default: ran %d harvests, want none", runs)
		}

		if cacheExists {
			t.Errorf("default: the probe cache %s was created", probeCache)
		}

		if strings.Contains(emitted, "ᴷ") || strings.Contains(emitted, "stackAppend<") || strings.Contains(emitted, "move2heap") || !strings.Contains(emitted, "x = append(x, (int64)i);") {
			t.Errorf("default: want plain append and no buffer in:\n%s", emitted)
		}
	}
}
