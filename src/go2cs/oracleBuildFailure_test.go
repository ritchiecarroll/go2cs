// oracleBuildFailure_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "testing"

// When the Go side of a comparison does not BUILD, the reading names why -- the first line the build
// printed -- instead of leaving a bare "[build failed]" and a column of Go="" verdicts. The fixture is
// github.com/pkg/errors' oracle stream under vet's printf check (2026-10-02).
func TestOracleBuildFailureNamesTheFirstBuildLine(t *testing.T) {
	stream := `{"ImportPath":"github.com/pkg/errors [github.com/pkg/errors.test]","Action":"build-output","Output":"# github.com/pkg/errors\n"}
{"ImportPath":"github.com/pkg/errors [github.com/pkg/errors.test]","Action":"build-output","Output":"# [github.com/pkg/errors]\n"}
{"ImportPath":"github.com/pkg/errors [github.com/pkg/errors.test]","Action":"build-output","Output":".\\errors_test.go:128:24: non-constant format string in call to github.com/pkg/errors.Wrapf\n"}
{"ImportPath":"github.com/pkg/errors [github.com/pkg/errors.test]","Action":"build-output","Output":".\\errors_test.go:220:31: non-constant format string in call to github.com/pkg/errors.WithMessagef\n"}
{"ImportPath":"github.com/pkg/errors [github.com/pkg/errors.test]","Action":"build-fail"}
{"Time":"2026-10-02T17:09:24Z","Action":"output","Package":"github.com/pkg/errors","Output":"FAIL\tgithub.com/pkg/errors [build failed]\n"}
{"Time":"2026-10-02T17:09:24Z","Action":"fail","Package":"github.com/pkg/errors","Elapsed":0,"FailedBuild":"github.com/pkg/errors [github.com/pkg/errors.test]"}
`

	want := `.\errors_test.go:128:24: non-constant format string in call to github.com/pkg/errors.Wrapf`

	if got := oracleBuildFailure(stream); got != want {
		t.Fatalf("want %q, got %q", want, got)
	}

	// A stream that BUILT carries no build line, so nothing is named.
	built := `{"Time":"2026-10-02T17:09:24Z","Action":"run","Package":"p","Test":"TestX"}
{"Time":"2026-10-02T17:09:24Z","Action":"pass","Package":"p","Test":"TestX"}
`

	if got := oracleBuildFailure(built); got != "" {
		t.Fatalf("a stream that built: want nothing named, got %q", got)
	}
}
