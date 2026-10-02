// sourcemeta.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Package sourcemeta is the self-description a published go2cs conversion of a Go module carries.
package sourcemeta

import "errors"

// EntryPath is where the self-description sits inside a nupkg.
const EntryPath = "go2cs/source-metadata.txt"

// Magic is the self-description's first line.
const Magic = "#go2cs-source-metadata v1"

// Require is one third-party module the packed assemblies depend on.
type Require struct {
	Module  string
	Version string
	NuGetID string
}

// Package is one packed Go package.
type Package struct {
	ImportPath string
	Assembly   string
}

// Description is a parsed self-description.
type Description struct {
	Module        string
	ModuleVersion string
	Go2csRelease  string
	Requires      []Require
	Packages      []Package
	Sections      map[string][]string
}

// Collect returns the metadata sections for the packed packages.
func Collect(srcRoot string, packages []Package) (map[string][]string, error) {
	return nil, errors.New("not implemented")
}

// Generate renders a self-description.
func Generate(d Description) ([]byte, error) {
	return nil, errors.New("not implemented")
}

// Parse reads a self-description.
func Parse(data []byte) (Description, error) {
	return Description{}, errors.New("not implemented")
}
