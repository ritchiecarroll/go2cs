// goModFileSpelling_other_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build !windows

package main

import (
	"os"
	"path/filepath"
	"testing"
)

// secondSpellingOf returns dir reached through a symlink to its parent, the nearest a host with no short names comes
// to the 8.3 spelling a Windows runner's TEMP carries: a path that names dir but is not its resolved spelling. A host
// that cannot make the symlink returns dir as it was, which the caller reads as no second spelling.
func secondSpellingOf(t *testing.T, dir string) string {
	t.Helper()

	via := filepath.Join(t.TempDir(), "via")

	if err := os.Symlink(filepath.Dir(dir), via); err != nil {
		t.Logf("no symlink here: %v", err)
		return dir
	}

	return filepath.Join(via, filepath.Base(dir))
}
