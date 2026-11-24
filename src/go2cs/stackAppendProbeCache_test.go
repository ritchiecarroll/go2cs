// stackAppendProbeCache_test.go - Gbtc
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
	"slices"
	"strings"
	"testing"
)

// The probe's go commands build the standard library once per target under their own flag set, which no ordinary
// build shares, so their entries go to a cache of their own and the user's GOCACHE grows nothing from them.

// probeCacheDir is the probe cache resolveProbeCacheDir names, or "" when it cannot be resolved. Production takes its
// GOCACHE from probeCacheDirOrExit; this test-only helper lets the resolution be read without ending the process.
func probeCacheDir() string {
	dir, err := resolveProbeCacheDir()

	if err != nil {
		return ""
	}

	return dir
}

func TestStackAppendProbeCacheResolution(t *testing.T) {
	flagDir, envDir := t.TempDir(), t.TempDir()

	t.Setenv(probeCacheEnv, envDir)

	if got := withProbeCacheFlag(t, flagDir, probeCacheDir); got != flagDir {
		t.Errorf("-probe-cache %s resolves to %s; the flag wins over %s", flagDir, got, probeCacheEnv)
	}

	if got := probeCacheDir(); got != envDir {
		t.Errorf("%s=%s resolves to %s", probeCacheEnv, envDir, got)
	}

	t.Setenv(probeCacheEnv, "")

	if userCache, err := os.UserCacheDir(); err == nil {
		if want, got := filepath.Join(userCache, "go2cs", "probe-cache"), probeCacheDir(); got != want {
			t.Errorf("the default probe cache is %s, want %s", got, want)
		}
	}

	if got := withProbeCacheFlag(t, "relative/cache", probeCacheDir); !filepath.IsAbs(got) {
		t.Errorf("a relative -probe-cache resolves to %s; go refuses a GOCACHE that is not absolute", got)
	}

	command := withProbeCacheFlag(t, flagDir, func() stackAppendHarvestCommand { return newStackAppendHarvestCommand(false, "linux/amd64", Options{}) })

	if !slices.Contains(command.env, "GOCACHE="+flagDir) {
		t.Errorf("the harvest's environment %q does not name the probe cache %s", strings.Join(command.env, " "), flagDir)
	}
}

func TestStackAppendHarvestWritesNothingIntoGOCACHE(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	userCache, probeCache := t.TempDir(), t.TempDir()
	t.Setenv("GOCACHE", userCache)

	dir := filepath.Join(t.TempDir(), "app")
	writeModuleFile(t, filepath.Join(dir, "go.mod"), "module example.com/probecache\n\ngo 1.25\n")
	writeModuleFile(t, filepath.Join(dir, "probecache.go"), "package probecache\n\nfunc Sum(n int) (s int) {\n\tvar x []int\n\tfor i := 0; i < n; i++ {\n\t\tx = append(x, i)\n\t}\n\tfor _, v := range x {\n\t\ts += v\n\t}\n\treturn\n}\n")

	options := Options{goRoot: goRoot}
	saved := probeCacheFlag
	probeCacheFlag = probeCache
	t.Cleanup(func() { probeCacheFlag = saved })

	if output := stackAppendHarvest(dir, false, "linux/amd64", options); !strings.Contains(output, "does not escape") {
		t.Fatalf("the harvest reported no escape answers, so this test measured nothing:\n%s", output)
	}

	if entries, err := os.ReadDir(userCache); err != nil || len(entries) != 0 {
		t.Errorf("the probe wrote %d entries into the user's GOCACHE (%v); it must write only into its own cache", len(entries), err)
	}

	if entries, err := os.ReadDir(probeCache); err != nil || len(entries) == 0 {
		t.Errorf("the probe cache %s holds %d entries (%v); the harvest's build did not use it", probeCache, len(entries), err)
	}
}

// withProbeCacheFlag answers f with -probe-cache set to dir, restoring the flag afterwards.
func withProbeCacheFlag[T any](t *testing.T, dir string, f func() T) T {
	t.Helper()
	saved := probeCacheFlag
	probeCacheFlag = dir
	defer func() { probeCacheFlag = saved }()

	return f()
}
