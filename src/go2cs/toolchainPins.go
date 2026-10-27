// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

//go:generate go run ./internal/gentoolchainpins

// The Go release a release build of go2cs downloads on first use, and the hashes it accepts. The release
// is the converter's own build Go (go.mod's go line), which is also the release its go.* packages are
// converted from. TestToolchainPinsMatchTheConverterGo holds the two together.
//
// pinnedGoModules are the golang.org/toolchain module hashes recorded by sum.golang.org
// (https://sum.golang.org/lookup/golang.org/toolchain@v0.0.1-go1.26.10.<goos>-<goarch>).
// pinnedGoArchives are the release archives' SHA-256 values from go.dev/dl. Written by hand until the
// generator (internal/gentoolchainpins) runs on a host that reaches go.dev, which fills the archives.

const pinnedGoVersion = "go1.26.10"

type goArchivePin struct {
	file   string
	sha256 string
}

var pinnedGoArchives = map[string]goArchivePin{}

var pinnedGoModules = map[string]string{
	"darwin-amd64":  "h1:0JApOD+sH1DW6WwvTcnIvl6MSylguxhQuhTbt6RJ1uo=",
	"darwin-arm64":  "h1:SfEWw1me2He9ojf4S+M6xTnGOP6CosVO88yaBsEgjJ0=",
	"linux-amd64":   "h1:5VAGHlY8yl8pTPVSNIez3SNMy6fWabs+QMR25f3oMT0=",
	"linux-arm64":   "h1:TuAzKmE5TqYnXpnhgY3GtBJCVg4AfNDA/K1gLyX5Hig=",
	"windows-amd64": "h1:ZCie1G8rYCijtritoBcOAGoxxCrhTVDHTvM423D7b/o=",
	"windows-arm64": "h1:9xf/jE9BbIQ7N+XMyPB2VYrfuwklVAYhHD6R8GNA6BE=",
}
