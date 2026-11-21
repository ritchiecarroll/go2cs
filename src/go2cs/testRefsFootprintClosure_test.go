// testRefsFootprintClosure_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "golang.org/x/tools/go/packages"

// refsFootprintClosure is the one call TestRefsFootprintOverCommittedTestProjects measures: the reference closure
// testConversion.go appends when it writes a -tests project. A run at an older ref replaces this file only, with that
// ref's form of the same call (before the record seeds existed: declarationClosureImports, without testInfoPath).
func refsFootprintClosure(roots []*packages.Package, compileExcluded map[string]bool, referenced []string,
	recordedBases map[string][]string, foreignBases func(string) map[string][]string, testInfoPath string) []string {
	return declarationClosureImportsFrom(roots, compileExcluded, referenced, recordedBases, foreignBases,
		testRecordInterfaces(testInfoPath, roots))
}
