// publishBuildLayout_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"io"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// The symbol guard reads the build output the publish ACTUALLY used, in whatever layout the build chose.
//
// At TRAIN Q's union (13c0800c21) every -recurse module row was refused before any verdict: "the published test
// host's build output ... cannot be found, so its symbol files cannot be checked". A -recurse output root's
// Directory.Build.props redirects every project's BaseOutputPath to <out root>/.artifacts/bin/<hash>/, and the guard
// looked only under bin/tests/<config>/, the in-tree layout a stdlib row builds into. These guards drive
// publishTestHost itself against a stand-in dotnet that lays out a publish in either layout, so they read the whole
// route: the command the publish runs, what it prints, and the folders the guard compares.
//
// They use nothing the guard's own row (a6ba14579a) does not have, so the artifacts-layout guard is RED there.

// fakeDotnetLayoutEnv selects the stand-in dotnet: this test binary, copied as dotnet, acts as a publish in the named
// layout ("artifacts" or "intree") and exits before any test runs.
const fakeDotnetLayoutEnv = "GO2CS_FAKE_DOTNET_LAYOUT"

// fakeDotnetUnpublishedEnv names the symbol files (comma-separated) the stand-in publish leaves out of the publish
// folder, as an unbundled publish does.
const fakeDotnetUnpublishedEnv = "GO2CS_FAKE_DOTNET_UNPUBLISHED"

// fakeDotnetSilentEnv makes the stand-in publish state nothing, even when asked for a property.
const fakeDotnetSilentEnv = "GO2CS_FAKE_DOTNET_SILENT"

// fakeDotnetSymbols are the symbol files the stand-in build produces beside the test assembly.
var fakeDotnetSymbols = []string{"x.tests.pdb", "golib.pdb", "fmt.pdb"}

func init() {
	if layout := os.Getenv(fakeDotnetLayoutEnv); layout != "" {
		if err := fakeDotnetPublish(layout, os.Args[1:]); err != nil {
			fmt.Fprintln(os.Stderr, "fake dotnet:", err)
			os.Exit(1)
		}

		os.Exit(0)
	}
}

// fakeDotnetPublish is `dotnet publish <project> -c <config> ... -o <dir> [-getProperty:<name>]`: it writes the test
// assembly and its symbol files where the layout's build puts them, the host and the published symbol files into -o,
// and, when asked for TargetPath, prints the assembly's path as MSBuild does.
func fakeDotnetPublish(layout string, args []string) error {
	if len(args) < 2 || args[0] != "publish" {
		return fmt.Errorf("not a publish: %v", args)
	}

	project, config, publishDir, stateTargetPath := args[1], "", "", false

	for i := 2; i < len(args); i++ {
		switch {
		case args[i] == "-c" && i+1 < len(args):
			config = args[i+1]
		case args[i] == "-o" && i+1 < len(args):
			publishDir = args[i+1]
		case strings.EqualFold(args[i], "-getProperty:TargetPath"):
			stateTargetPath = true
		}
	}

	if config == "" || publishDir == "" {
		return fmt.Errorf("no -c or -o: %v", args)
	}

	root := filepath.Dir(project)
	var buildDir string

	switch layout {
	case "artifacts":
		buildDir = filepath.Join(root, ".artifacts", "bin", "0123456789ab", config, "net10.0", "win-x64")
	case "intree":
		buildDir = filepath.Join(root, "bin", "tests", config, "net10.0", "win-x64")
	default:
		return fmt.Errorf("unknown layout %q", layout)
	}

	unpublished := map[string]bool{}

	for _, name := range strings.Split(os.Getenv(fakeDotnetUnpublishedEnv), ",") {
		unpublished[name] = true
	}

	assembly := filepath.Join(buildDir, strings.TrimSuffix(filepath.Base(project), filepath.Ext(project))+".dll")
	files := []string{assembly, publishedTestHostPath(root, project)}

	for _, name := range fakeDotnetSymbols {
		files = append(files, filepath.Join(buildDir, name))

		if !unpublished[name] {
			files = append(files, filepath.Join(publishDir, name))
		}
	}

	for _, file := range files {
		if err := os.MkdirAll(filepath.Dir(file), 0o755); err != nil {
			return err
		}
		if err := os.WriteFile(file, nil, 0o644); err != nil {
			return err
		}
	}

	if stateTargetPath && os.Getenv(fakeDotnetSilentEnv) == "" {
		fmt.Println(assembly)
	}

	return nil
}

// useFakeDotnet puts this test binary on an otherwise empty PATH as dotnet, acting as a publish in the given layout.
func useFakeDotnet(t *testing.T, layout string) {
	t.Helper()

	self, err := os.Executable()
	if err != nil {
		t.Fatal(err)
	}

	bin := t.TempDir()
	name := "dotnet"

	if runtime.GOOS == "windows" {
		name += ".exe"
	}

	src, err := os.Open(self)
	if err != nil {
		t.Fatal(err)
	}
	defer src.Close()

	dst, err := os.OpenFile(filepath.Join(bin, name), os.O_CREATE|os.O_WRONLY|os.O_TRUNC, 0o755)
	if err != nil {
		t.Fatal(err)
	}
	if _, err := io.Copy(dst, src); err != nil {
		dst.Close()
		t.Fatal(err)
	}
	if err := dst.Close(); err != nil {
		t.Fatal(err)
	}

	t.Setenv("PATH", bin)
	t.Setenv(fakeDotnetLayoutEnv, layout)
}

// A -recurse module's host builds into the artifacts layout. Its publish is good, and the guard must find the build
// output and pass it. RED at a6ba14579a: "the published test host's build output ... cannot be found".
func TestPublishFindsTheArtifactsLayoutBuildOutput(t *testing.T) {
	out := t.TempDir()
	useFakeDotnet(t, "artifacts")

	if err := publishTestHost(out, filepath.Join(out, "x.tests.csproj"), Options{}); err != nil {
		t.Fatalf("a good publish in the artifacts layout was refused: %v", err)
	}
}

// The guard keeps refusing a host that lacks a dependency symbol file, in the artifacts layout too.
func TestPublishInTheArtifactsLayoutStillNamesAMissingSymbolFile(t *testing.T) {
	out := t.TempDir()
	useFakeDotnet(t, "artifacts")
	t.Setenv(fakeDotnetUnpublishedEnv, "golib.pdb")

	err := publishTestHost(out, filepath.Join(out, "x.tests.csproj"), Options{})
	if err == nil || !strings.Contains(err.Error(), "lacks 1 dependency symbol file(s)") || !strings.Contains(err.Error(), "golib.pdb") {
		t.Fatalf("a host without golib.pdb, in the artifacts layout, was not refused by name: %v", err)
	}
}

// The in-tree layout a stdlib row builds into reads as before: silent on a good publish, the missing file named on a
// bad one, in both configurations.
func TestPublishInTheInTreeLayoutIsUnchanged(t *testing.T) {
	for _, config := range []string{"Debug", "Release"} {
		useFakeDotnet(t, "intree")
		options := Options{testConfig: config, go2csPath: t.TempDir()}

		out := t.TempDir()
		t.Setenv(fakeDotnetUnpublishedEnv, "")
		if err := publishTestHost(out, filepath.Join(out, "x.tests.csproj"), options); err != nil {
			t.Errorf("%s: a good in-tree publish was refused: %v", config, err)
		}

		out = t.TempDir()
		t.Setenv(fakeDotnetUnpublishedEnv, "fmt.pdb")
		err := publishTestHost(out, filepath.Join(out, "x.tests.csproj"), options)
		if err == nil || !strings.Contains(err.Error(), "fmt.pdb") || strings.Contains(err.Error(), "golib.pdb") {
			t.Errorf("%s: an in-tree host without fmt.pdb was not refused naming exactly it: %v", config, err)
		}
	}
}

// A host whose build output cannot be found is refused, never passed: here the publish states no build output at all,
// and nothing beside the host stands in for it.
func TestAPublishThatStatesNoBuildOutputIsRefused(t *testing.T) {
	out := t.TempDir()
	useFakeDotnet(t, "artifacts")
	t.Setenv(fakeDotnetSilentEnv, "1")

	err := publishTestHost(out, filepath.Join(out, "x.tests.csproj"), Options{})
	if err == nil || !strings.Contains(err.Error(), "symbol files cannot be checked") {
		t.Fatalf("a publish whose build output cannot be found was passed: %v", err)
	}
}
