// cgoRecurseSubstitution.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// refuseCgoSplitSubstitutions refuses, under -recurse -cgo, a module SUBSTITUTED by a published package whose
// packages Go builds from different files with cgo on (P3's INC 1 review, finding 1; COORD's ruling). RED STUB: it
// refuses nothing yet.
func (m *ModuleConverter) refuseCgoSplitSubstitutions(moduleDir string, substitutions map[string]*nugetSubstitution) error {
	return nil
}
