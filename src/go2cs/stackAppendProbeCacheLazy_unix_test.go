// stackAppendProbeCacheLazy_unix_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build unix

package main

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

const probeCacheHarvestHelperEnv = "GO2CS_PROBE_CACHE_HARVEST_HELPER"

// probeCacheRefusal is the text every refusal of the probe cache carries.
const probeCacheRefusal = "escape probe's build cache"

// The probe cache is resolved when a harvest command is built, not at start-up, so a host whose cache cannot be
// resolved refuses only a run that would probe. Both arms run on such a host: no user cache directory, and the
// per-user fallback planted with mode 0777, which probeCacheTempFallback refuses.
func TestStackAppendProbeCacheResolvesOnlyForAHarvest(t *testing.T) {
	temp := t.TempDir()
	t.Setenv("TMPDIR", temp)
	t.Setenv("HOME", "")
	t.Setenv("XDG_CACHE_HOME", "")
	t.Setenv(probeCacheEnv, "")
	// The converter resolves GOPATH from HOME when GOPATH is unset, and stops there; name one so the run reaches the
	// probe cache.
	t.Setenv("GOPATH", t.TempDir())

	if _, err := os.UserCacheDir(); err == nil {
		t.Skip("os.UserCacheDir still resolves with HOME and XDG_CACHE_HOME unset")
	}

	planted := filepath.Join(temp, fmt.Sprintf("go2cs-probe-cache-%d", os.Getuid()))

	if err := os.Mkdir(planted, 0o700); err != nil {
		t.Fatal(err)
	}

	if err := os.Chmod(planted, 0o777); err != nil {
		t.Fatal(err)
	}

	// A mode that converts nothing (-test-action build acts on existing artifacts) is not refused for the cache: it
	// fails, here, only for want of the artifacts it was pointed at.
	output := runCLIUnchecked(t, temp, "-tests", "-test-action", "build", t.TempDir(), t.TempDir())

	if strings.Contains(output, probeCacheRefusal) || !strings.Contains(output, "test manifest is missing") {
		t.Errorf("-test-action build, which converts nothing, did not reach its own action past the probe cache:\n%s", output)
	}

	// A harvest is refused before its go command runs, naming the way out.
	cmd := exec.Command(os.Args[0], "-test.run=^TestStackAppendProbeCacheHarvestHelperProcess$")
	cmd.Env = append(os.Environ(), probeCacheHarvestHelperEnv+"=1")
	harvest, err := cmd.CombinedOutput()

	if err == nil || !strings.Contains(string(harvest), probeCacheRefusal) || !strings.Contains(string(harvest), "-probe-cache") {
		t.Errorf("a harvest on a host whose probe cache is refused was not refused naming -probe-cache (err %v):\n%s", err, harvest)
	}
}

// The stand-in harvest: builds one harvest command, which resolves the probe cache. Inert unless the environment
// variable is set.
func TestStackAppendProbeCacheHarvestHelperProcess(t *testing.T) {
	if _, ok := os.LookupEnv(probeCacheHarvestHelperEnv); !ok {
		return
	}

	command := newStackAppendHarvestCommand(false, "linux/amd64", Options{})
	fmt.Println(strings.Join(command.env, " "))
	os.Exit(0)
}

// runCLIUnchecked runs the converter with args from dir and returns its combined output, whatever its exit status.
func runCLIUnchecked(t *testing.T, dir string, args ...string) string {
	t.Helper()

	cmd := exec.Command(os.Args[0], "-test.run=^TestCLIPathsHelperProcess$")
	cmd.Dir = dir
	quoted := make([]string, len(args))

	for i, arg := range args {
		quoted[i] = fmt.Sprintf("%q", arg)
	}

	cmd.Env = append(os.Environ(), cliPathsHelperEnv+"=["+strings.Join(quoted, ",")+"]")
	output, _ := cmd.CombinedOutput()

	return string(output)
}
