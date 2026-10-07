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

// A host that cannot be removed is refused by name. The usual reason is an earlier run's host that is
// still running; a directory holding a file stands in for that here, since it fails to remove on every
// platform.
//
// The refusal is this attempt's failure, so the binary log an EARLIER failed publish kept is gone by
// the time it is returned: the sweep files whatever log it finds beside a failed row as that row's
// evidence (preparePublishBinlog's own rule, which the refusal does not step around).
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

	staleBinlog := publishBinlogPath(out)

	if err := os.WriteFile(staleBinlog, []byte("an earlier failed publish"), 0o644); err != nil {
		t.Fatal(err)
	}

	takeDotnetOffPath(t)

	err := publishTestHost(out, project, Options{testPublishBinlog: true})
	if err == nil || errors.Is(err, exec.ErrNotFound) {
		t.Fatalf("publishTestHost went on to the publish over a host it could not remove (returned %v)", err)
	}
	if !strings.Contains(err.Error(), host) {
		t.Errorf("the refusal does not name the host %s: %v", host, err)
	}
	if _, statErr := os.Stat(staleBinlog); !os.IsNotExist(statErr) {
		t.Errorf("an earlier attempt's binary log survives the refusal and would be read as this one's: %s", staleBinlog)
	}
}

// THE OUTCOME GUARD. The three guards above pin the LEVER (the previous host is gone before dotnet runs); this one pins
// the OUTCOME, by any route: after a successful publish, every symbol file the publish's own build produced must be
// beside the published host. A publish that skipped its bundle, an SDK that stops re-bundling, or a by-hand second
// publish into the same root each leave the build's .pdb files behind and the publish folder without them, and frames
// in those assemblies then print no file:line. The predicate compares the two folders by name.

// seedHostBuild lays out a -tests output root: the publish's build output under bin/tests/<config>/net10.0/<rid>/ holding
// the test assembly and the given symbol files, and the publish folder holding the host and the given symbol files. It
// returns the build output too, as publishBuildOutput reads it from the publish.
func seedHostBuild(t *testing.T, config string, built, published []string) (out, project, buildDir string) {
	t.Helper()

	out = t.TempDir()
	project = filepath.Join(out, "x.tests.csproj")
	buildDir = filepath.Join(out, "bin", "tests", config, "net10.0", "win-x64")
	publishDir := filepath.Dir(publishedTestHostPath(out, project))

	for _, file := range append([]string{filepath.Join(buildDir, "x.tests.dll"), publishedTestHostPath(out, project)}, nil...) {
		writeEmpty(t, file)
	}
	for _, name := range built {
		writeEmpty(t, filepath.Join(buildDir, name))
	}
	for _, name := range published {
		writeEmpty(t, filepath.Join(publishDir, name))
	}

	return out, project, buildDir
}

func writeEmpty(t *testing.T, file string) {
	t.Helper()

	if err := os.MkdirAll(filepath.Dir(file), 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(file, nil, 0o644); err != nil {
		t.Fatal(err)
	}
}

func TestPublishedHostLackingDependencySymbolsIsNamed(t *testing.T) {
	out, project, buildDir := seedHostBuild(t, "Release",
		[]string{"x.tests.pdb", "golib.pdb", "fmt.pdb"},
		[]string{"x.tests.pdb"})

	missing, err := publishedSymbolsMissing(buildDir, out, project)
	if err != nil {
		t.Fatal(err)
	}
	if strings.Join(missing, ",") != "fmt.pdb,golib.pdb" {
		t.Fatalf("missing = %v, want [fmt.pdb golib.pdb]: the host keeps only its own symbols, as an unbundled second publish leaves it", missing)
	}
}

func TestPublishedHostWithEverySymbolIsSilent(t *testing.T) {
	out, project, buildDir := seedHostBuild(t, "Release",
		[]string{"x.tests.pdb", "golib.pdb"},
		[]string{"x.tests.pdb", "golib.pdb"})

	missing, err := publishedSymbolsMissing(buildDir, out, project)
	if err != nil || len(missing) != 0 {
		t.Fatalf("a host beside every symbol file its build produced was flagged: missing=%v err=%v", missing, err)
	}
}

// The check reads the build output THIS publish stated only: a Debug tree left beside a Release run (or a Release tree
// beside a Debug one) is another build's output, not this host's.
func TestTheOtherConfigurationsBuildOutputIsNotRead(t *testing.T) {
	out, project, buildDir := seedHostBuild(t, "Release",
		[]string{"x.tests.pdb", "golib.pdb"},
		[]string{"x.tests.pdb", "golib.pdb"})
	writeEmpty(t, filepath.Join(out, "bin", "tests", "Debug", "net10.0", "win-x64", "x.tests.dll"))
	writeEmpty(t, filepath.Join(out, "bin", "tests", "Debug", "net10.0", "win-x64", "debugonly.pdb"))

	stated, err := publishBuildOutput(filepath.Join(buildDir, "x.tests.dll")+"\n", project)
	if err != nil {
		t.Fatal(err)
	}

	missing, err := publishedSymbolsMissing(stated, out, project)
	if err != nil || len(missing) != 0 {
		t.Fatalf("the Debug build output was read for a Release host: missing=%v err=%v", missing, err)
	}
}

// After a successful publish the build output exists; when it cannot be found the check cannot be made, and saying
// nothing would make it a guard that passes by absence. Two ways it cannot be found: the publish states no build
// output, or the one it states is not there.
func TestAMissingBuildOutputIsRefusedNotPassed(t *testing.T) {
	out := t.TempDir()
	project := filepath.Join(out, "x.tests.csproj")
	writeEmpty(t, publishedTestHostPath(out, project))

	if _, err := publishBuildOutput("", project); err == nil {
		t.Error("the publish stated no build output, and the check passed: a guard that cannot see must say so")
	}

	if _, err := publishBuildOutput(filepath.Join(out, "bin", "tests", "Release", "net10.0", "win-x64", "x.tests.dll")+"\n", project); err == nil {
		t.Error("the publish stated a build output that is not there, and the check passed: a guard that cannot see must say so")
	}
}
