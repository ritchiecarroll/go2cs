// nugetLock.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"sort"
	"strings"
)

// The -nuget-map lock (docs/PLAN-nugetgo.md 4.4): one line per MAPPED module in the recurse output root,
// beside go2cs.modules.lock and kept the same way (sorted by module, LF line endings, a '#' header). A
// later run resolves from the lock first and reports any source that now disagrees instead of adopting it;
// -nuget-map-refresh re-resolves deliberately.
//
// Seven TAB-separated fields, fixed from the first release so stage S3b needs no format change: module,
// module version, NuGet ID, status, the layer that answered (a file path or URL, which may hold spaces --
// hence TAB), and the package version and content hash S3b selects. S3a writes '-' in the last two.

const nugetLockFileName = "go2cs.nuget.lock"

const nugetLockFieldCount = 7

// nugetLockEntry is one locked mapping.
type nugetLockEntry struct {
	module         string
	version        string
	nugetID        string
	status         string
	layer          string
	packageVersion string
	contentHash    string
}

func nugetLockPath(outRoot string) string {
	return filepath.Join(outRoot, nugetLockFileName)
}

// readNuGetLock reads the output root's mapping lock; a root with no lock yet reads as empty. A malformed
// line refuses the lock by file:line rather than dropping a pinned mapping.
func readNuGetLock(outRoot string) (map[string]nugetLockEntry, error) {
	entries := make(map[string]nugetLockEntry)
	data, err := os.ReadFile(nugetLockPath(outRoot))

	if os.IsNotExist(err) {
		return entries, nil
	}

	if err != nil {
		return nil, err
	}

	for index, line := range strings.Split(string(data), "\n") {
		line = strings.TrimSuffix(line, "\r")

		if strings.TrimSpace(line) == "" || strings.HasPrefix(line, "#") {
			continue
		}

		fields := strings.Split(line, "\t")

		if len(fields) != nugetLockFieldCount {
			return nil, fmt.Errorf("%s:%d: want 7 TAB-separated fields (module, version, nuget-id, status, layer, package-version, content-hash), got %d", nugetLockFileName, index+1, len(fields))
		}

		entries[fields[0]] = nugetLockEntry{
			module: fields[0], version: fields[1], nugetID: fields[2], status: fields[3],
			layer: fields[4], packageVersion: fields[5], contentHash: fields[6],
		}
	}

	return entries, nil
}

// writeNuGetLock writes the output root's mapping lock, sorted by module.
func writeNuGetLock(outRoot string, entries map[string]nugetLockEntry) error {
	modules := make([]string, 0, len(entries))

	for modulePath := range entries {
		modules = append(modules, modulePath)
	}

	sort.Strings(modules)

	var text strings.Builder
	text.WriteString("# go2cs.nuget.lock -- the -nuget-map mappings this output root resolved; written by go2cs, do not edit.\n")
	text.WriteString("# module\tversion\tnuget-id\tstatus\tlayer\tpackage-version\tcontent-hash\n")

	for _, modulePath := range modules {
		entry := entries[modulePath]
		text.WriteString(strings.Join([]string{entry.module, entry.version, entry.nugetID, entry.status, entry.layer, entry.packageVersion, entry.contentHash}, "\t"))
		text.WriteString("\n")
	}

	if err := os.MkdirAll(outRoot, 0o755); err != nil {
		return err
	}

	return os.WriteFile(nugetLockPath(outRoot), []byte(text.String()), 0o644)
}
