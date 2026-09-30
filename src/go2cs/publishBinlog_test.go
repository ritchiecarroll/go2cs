// publishBinlog_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// -test-publish-binlog (coordinator ruling 2026-09-30): the test host's dotnet publish writes an MSBuild
// binary log from the FIRST attempt, KEPT only when the publish fails. It exists for the transient build
// failure that heals on a re-run -- the i9's D5 reflect row, CS0246 on go2cs-gen types with no generator
// diagnostic in the returned output -- where only a binlog names the /analyzer list csc received and the input
// that made a project recompile. It costs ~4 s on a warm reflect publish (+35%), so it is a switch, not a
// default. These guards pin the three behaviors that make it evidence: passed only when asked, a stale log
// never survives into a new attempt, and a passing publish leaves none behind.

func TestPublishBinlogIsPassedOnlyWhenAsked(t *testing.T) {
	out := t.TempDir()

	off := withPublishBinlog(publishTestHostArgs(out, "x.tests.csproj", Options{testConfig: "Release", go2csPath: "/src"}), "")
	for _, arg := range off {
		if strings.HasPrefix(arg, "-bl") {
			t.Errorf("without -test-publish-binlog the publish still carries %q", arg)
		}
	}

	path := publishBinlogPath(out)
	on := withPublishBinlog(publishTestHostArgs(out, "x.tests.csproj", Options{testConfig: "Release", go2csPath: "/src"}), path)
	if on[len(on)-1] != "-bl:"+path {
		t.Errorf("with the binlog asked for, the publish's last argument is %q; want %q", on[len(on)-1], "-bl:"+path)
	}

	debug := withPublishBinlog(publishTestHostArgs(out, "x.tests.csproj", Options{testConfig: "Debug"}), path)
	if debug[len(debug)-1] != "-bl:"+path {
		t.Errorf("the Debug publish does not carry the binlog either: %v", debug)
	}
}

func TestAStaleBinlogNeverSurvivesIntoANewAttempt(t *testing.T) {
	out := t.TempDir()
	path := publishBinlogPath(out)

	if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(path, []byte("an earlier attempt's log"), 0o644); err != nil {
		t.Fatal(err)
	}

	if got := preparePublishBinlog(out, Options{testPublishBinlog: false}); got != "" {
		t.Errorf("preparePublishBinlog without the switch answered %q; want no binlog", got)
	}
	if _, err := os.Stat(path); err != nil {
		t.Errorf("without the switch the file is not this pipeline's to touch, but it was removed")
	}

	if got := preparePublishBinlog(out, Options{testPublishBinlog: true}); got != path {
		t.Errorf("preparePublishBinlog answered %q; want %q", got, path)
	}
	if _, err := os.Stat(path); !os.IsNotExist(err) {
		t.Errorf("a stale binlog survived into the new attempt, where it would read as this publish's evidence")
	}
}

func TestTheBinlogIsKeptOnlyForAFailedPublish(t *testing.T) {
	out := t.TempDir()
	path := publishBinlogPath(out)

	write := func() {
		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(path, []byte("log"), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	write()
	if err := settlePublishBinlog(path, nil); err != nil {
		t.Fatalf("a passing publish returned %v", err)
	}
	if _, err := os.Stat(path); !os.IsNotExist(err) {
		t.Errorf("a PASSING publish left its binlog behind")
	}

	write()
	failure := errors.New("dotnet publish failed")
	err := settlePublishBinlog(path, failure)
	if !errors.Is(err, failure) {
		t.Errorf("a failed publish's error no longer wraps the original: %v", err)
	}
	if err == nil || !strings.Contains(err.Error(), path) {
		t.Errorf("a failed publish's error does not name the kept binlog %s: %v", path, err)
	}
	if _, statErr := os.Stat(path); statErr != nil {
		t.Errorf("a FAILED publish lost its binlog: %v", statErr)
	}

	if err := settlePublishBinlog("", failure); err != failure {
		t.Errorf("with no binlog asked for, the error changed: %v", err)
	}
}
