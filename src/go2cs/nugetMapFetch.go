// nugetMapFetch.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"net/http"
	"os"
	"path/filepath"
	"time"
)

// nugetMapRegistryURL is the official registry, appended last unless -nuget-map-only. A variable so a
// test can point it at a local fixture; nothing else assigns it.
var nugetMapRegistryURL = "https://nugetgo.net/v1/mappings.txt"

// nugetMapMaxBytes caps one mapping source. A variable only so a test can lower it.
var nugetMapMaxBytes int64 = 8 << 20

// nugetMapHTTPClient fetches every URL source. A variable so a test can inject a fixture's client.
var nugetMapHTTPClient = &http.Client{Timeout: 30 * time.Second}

// nugetMapCacheRoot is where fetched sources are cached. A variable so a test can inject t.TempDir().
var nugetMapCacheRoot = func() (string, error) {
	dir, err := os.UserCacheDir()

	if err != nil {
		return "", err
	}

	return filepath.Join(dir, "go2cs", "nuget-map"), nil
}

// nugetMapSourceList is the ordered list of sources a run reads.
func nugetMapSourceList(o nugetMapOptions) []string {
	return nil
}

// loadNuGetMapSource reads and parses one source.
func loadNuGetMapSource(src string, refresh bool) (nugetMapSource, []string, error) {
	return nugetMapSource{}, nil, errors.New("not implemented")
}
