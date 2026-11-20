// stackAppendProbeCacheTemp_unix_test.go - Gbtc
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
	"path/filepath"
	"strings"
	"syscall"
	"testing"
)

// With no user cache directory the probe cache falls back to the temporary directory, which is shared and, on Linux,
// world-writable: a fixed <temp>/go2cs/probe-cache could be created first by another user. The fallback is a per-user
// directory created 0700, used only while it is a directory this user owns that group and others cannot open.
func TestStackAppendProbeCacheWithoutUserCacheDir(t *testing.T) {
	temp := t.TempDir()
	t.Setenv("TMPDIR", temp)
	t.Setenv("HOME", "")
	t.Setenv("XDG_CACHE_HOME", "")
	t.Setenv(probeCacheEnv, "")

	if _, err := os.UserCacheDir(); err == nil {
		t.Skip("os.UserCacheDir still resolves with HOME and XDG_CACHE_HOME unset")
	}

	shared := filepath.Join(temp, "go2cs", "probe-cache")
	want := filepath.Join(temp, fmt.Sprintf("go2cs-probe-cache-%d", os.Getuid()))
	got := withProbeCacheFlag(t, "", probeCacheDir)

	if got == shared {
		t.Fatalf("the probe cache is the shared %s, which another user can create first", shared)
	}

	if got != want {
		t.Fatalf("the probe cache is %s; want the per-user %s", got, want)
	}

	info, err := os.Lstat(want)

	if err != nil || !info.IsDir() {
		t.Fatalf("%s was not created as a directory: %v", want, err)
	}

	if perm := info.Mode().Perm(); perm != 0o700 {
		t.Errorf("%s has mode %o; want 0700", want, perm)
	}

	if stat, ok := info.Sys().(*syscall.Stat_t); !ok || int(stat.Uid) != os.Getuid() {
		t.Errorf("%s is not owned by this user", want)
	}

	// A directory group or others can open is refused, never loosened or used.
	if err := os.Chmod(want, 0o777); err != nil {
		t.Fatal(err)
	}

	if got := withProbeCacheFlag(t, "", probeCacheDir); got == want {
		t.Errorf("a %s with mode 0777 was used as the probe cache", want)
	}

	// The refusal names the way out, and a harvest exits on it before its go command runs (probeCacheDirOrExit).
	if err := withProbeCacheFlag(t, "", func() error { _, err := resolveProbeCacheDir(); return err }); err == nil || !strings.Contains(err.Error(), "-probe-cache") {
		t.Errorf("the refusal of a mode-0777 %s does not name -probe-cache: %v", want, err)
	}

	// So is a link in its place, wherever it points.
	if err := os.Remove(want); err != nil {
		t.Fatal(err)
	}

	if err := os.Symlink(t.TempDir(), want); err != nil {
		t.Fatal(err)
	}

	if got := withProbeCacheFlag(t, "", probeCacheDir); got == want {
		t.Errorf("a link at %s was used as the probe cache", want)
	}

	// An explicit -probe-cache still wins: the refusal asks for exactly that.
	explicit := t.TempDir()

	if got := withProbeCacheFlag(t, explicit, probeCacheDir); got != explicit {
		t.Errorf("-probe-cache %s resolves to %s", explicit, got)
	}
}
