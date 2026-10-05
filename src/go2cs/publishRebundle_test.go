// publishRebundle_test.go - Gbtc
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
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// The test host is bundled again on EVERY publish, because a publish that finds its bundle up to date
// deletes the symbol files beside it.
//
// Measured 2026-10-05 on runtime/debug (SDK 10.0.400, binary log read), two publishes of one unchanged
// project into one directory. The second skips GenerateSingleFileBundle ("all output files are
// up-to-date"), so the GenerateBundle task never answers its ExcludedFiles -- the one list that names
// the referenced assemblies' symbol files as published -- and _IncrementalCleanPublishDirectory then
// deletes all 72 of them as orphans of the first publish: 73 *.pdb before, 1 after (the test assembly's
// own, which the SDK marks ExcludeFromSingleFile itself). The converted runtime reads those files for a
// traceback's source lines, so the row then fails on frames that print no file:line. A converter that
// rewrote every source on every run never published warm; one that keeps an unchanged source's
// timestamp does on every second run.
//
// Removing the published executable alone makes the bundle target run (it is that target's one declared
// output), which is what these guards pin through publishTestHost itself. dotnet is taken off PATH for
// each of them: the publish cannot start, and the test reads only what publishTestHost does to the
// directory before it hands over to MSBuild.

// seedPublishedHost lays out what an earlier publish leaves: the single-file host, and beside it the
// loose files a test reads at run time. It returns the host and the loose files.
func seedPublishedHost(t *testing.T, out string) (host string, loose []string) {
	t.Helper()

	project := filepath.Join(out, "x.tests.csproj")
	host = publishedTestHostPath(out, project)
	publishDir := filepath.Dir(host)

	loose = []string{
		filepath.Join(publishDir, "x.tests.pdb"),
		filepath.Join(publishDir, "golib.pdb"),
		filepath.Join(publishDir, "stack_test.go"),
		filepath.Join(publishDir, "testdata", "fixture.txt"),
	}

	for _, path := range append([]string{host}, loose...) {
		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(path, []byte("an earlier publish"), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	return host, loose
}

// takeDotnetOffPath leaves the publish nothing to run.
func takeDotnetOffPath(t *testing.T) {
	t.Helper()
	t.Setenv("PATH", t.TempDir())

	if path, err := exec.LookPath("dotnet"); err == nil {
		t.Fatalf("dotnet still resolves to %s: this test must not start a real publish", path)
	}
}

func TestPublishRemovesThePreviousHostAndNothingElse(t *testing.T) {
	out := t.TempDir()
	host, loose := seedPublishedHost(t, out)
	takeDotnetOffPath(t)

	err := publishTestHost(out, filepath.Join(out, "x.tests.csproj"), Options{})
	if !errors.Is(err, exec.ErrNotFound) {
		t.Fatalf("publishTestHost returned %v; want the publish to be reached and to find no dotnet", err)
	}

	if _, statErr := os.Stat(host); !os.IsNotExist(statErr) {
		t.Errorf("the previous host is still there when dotnet publish starts, so an unchanged project skips the bundle and the publish deletes the symbol files beside it: %s", host)
	}

	for _, path := range loose {
		if _, statErr := os.Stat(path); statErr != nil {
			t.Errorf("a loose file of the previous publish was removed with the host; the rest of the directory stays incremental: %v", statErr)
		}
	}
}

// A first publish has no previous host, and that is not a failure.
func TestPublishIntoAFreshDirectoryStillReachesThePublish(t *testing.T) {
	out := t.TempDir()
	takeDotnetOffPath(t)

	err := publishTestHost(out, filepath.Join(out, "x.tests.csproj"), Options{})
	if !errors.Is(err, exec.ErrNotFound) {
		t.Errorf("publishTestHost returned %v; want the publish to be reached and to find no dotnet", err)
	}
}

// A host that cannot be removed is refused by name. The usual reason is an earlier run's host still
// running it; a directory holding a file stands in for that here, since it fails to remove on every
// platform.
func TestPublishRefusesAHostItCannotRemove(t *testing.T) {
	out := t.TempDir()
	project := filepath.Join(out, "x.tests.csproj")
	host := publishedTestHostPath(out, project)

	if err := os.MkdirAll(host, 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(host, "held"), nil, 0o644); err != nil {
		t.Fatal(err)
	}

	takeDotnetOffPath(t)

	err := publishTestHost(out, project, Options{})
	if err == nil || errors.Is(err, exec.ErrNotFound) {
		t.Fatalf("publishTestHost went on to the publish over a host it could not remove (returned %v)", err)
	}
	if !strings.Contains(err.Error(), host) {
		t.Errorf("the refusal does not name the host %s: %v", host, err)
	}
}
