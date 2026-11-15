// packageInfoLongLine_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards sqlite-cs W2: modernc.org/sqlite/lib's package_info.cs carries a GoPositionMap record of 123,448 bytes
// (sqlite_windows.go's position map) on ONE line. The reader stopped at bufio.Scanner's 64 KB token limit, so every
// importer failed to load ANY of lib's alias records ("token too long", a warning), and each alias of lib's
// anonymous structs (vtab.go's cIndexConstraint, pagecache.go's pcacheMethods2) fell back to raw Go text. A record
// the converter writes must be one its own reader can read back, whatever a line's length.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestPackageInfoReaderReadsALineLongerThanTheScannerDefault(t *testing.T) {
	long := `[assembly: GoPositionMap("lib/sqlite_windows.go", "sqlite_windows.cs", "` + strings.Repeat("A", 200*1024) + `")]`

	content := strings.Join([]string{
		"// <ExportedTypeAliases>",
		`[assembly: GoTypeAlias("Tindex_constraint", "go.example.lib.sqlite3_package.Tindex_constraintᴛ1")]`,
		long,
		`[assembly: GoTypeAlias("Tpcache_methods2", "go.example.lib.sqlite3_package.Tpcache_methods2ᴛ1")]`,
		"// </ExportedTypeAliases>",
	}, "\r\n")

	path := filepath.Join(t.TempDir(), "package_info.cs")

	if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
		t.Fatal(err)
	}

	aliases, err := parseExportedTypeAliases(path)

	if err != nil {
		t.Fatalf("a package_info.cs with a %d-byte line must parse: %v", len(long), err)
	}

	got := map[string]string{}

	for _, alias := range aliases {
		got[alias[0]] = alias[1]
	}

	for source, target := range map[string]string{
		"Tindex_constraint": "go.example.lib.sqlite3_package.Tindex_constraintᴛ1",
		"Tpcache_methods2":  "go.example.lib.sqlite3_package.Tpcache_methods2ᴛ1",
	} {
		if got[source] != target {
			t.Errorf("alias %s: got %q, want %q (records on both sides of the long line must load)", source, got[source], target)
		}
	}
}
