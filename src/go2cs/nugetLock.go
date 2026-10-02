// nugetLock.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"path/filepath"
)

const nugetLockFileName = "go2cs.nuget.lock"

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

// readNuGetLock reads the output root's mapping lock.
func readNuGetLock(outRoot string) (map[string]nugetLockEntry, error) {
	return nil, errors.New("not implemented")
}

// writeNuGetLock writes the output root's mapping lock.
func writeNuGetLock(outRoot string, entries map[string]nugetLockEntry) error {
	return errors.New("not implemented")
}
