// goModFileSpelling_windows_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build windows

package main

import (
	"syscall"
	"testing"
)

// secondSpellingOf returns dir spelled through its 8.3 short names, as a hosted runner's TEMP is spelled (its user
// segment is short). A volume with short names off returns dir as it was, which the caller reads as no second
// spelling.
func secondSpellingOf(t *testing.T, dir string) string {
	t.Helper()

	long, err := syscall.UTF16PtrFromString(dir)

	if err != nil {
		t.Fatalf("UTF16PtrFromString(%s): %v", dir, err)
	}

	buffer := make([]uint16, syscall.MAX_LONG_PATH)
	n, err := syscall.GetShortPathName(long, &buffer[0], uint32(len(buffer)))

	if err != nil || n == 0 || int(n) > len(buffer) {
		t.Logf("GetShortPathName(%s): %v", dir, err)
		return dir
	}

	return syscall.UTF16ToString(buffer[:n])
}
