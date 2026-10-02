// nugetSubstitution.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "go2cs/internal/sourcemeta"

// nugetSubstitution is one third-party module this run references as a published package.
type nugetSubstitution struct {
	module         string
	version        string
	nugetID        string
	packageVersion string
	description    sourcemeta.Description
}

// nugetSubstitutions holds this run's substitutions by module path.
var nugetSubstitutions map[string]*nugetSubstitution

// nugetSubstitutionFor returns the substitution covering an import path, or nil.
func nugetSubstitutionFor(importPath string) *nugetSubstitution {
	return nil
}

// closeNuGetSubstitutions demotes every substitution whose package requires a module this run does not
// substitute consistently, and returns the reasons by module.
func closeNuGetSubstitutions(substitutions map[string]*nugetSubstitution, modules map[string]thirdPartyModule) map[string]string {
	return nil
}
