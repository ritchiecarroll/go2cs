// platformEmitStage_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// These pin where a multi-platform emission stages its per-target conversions when no -platform-stage
// is given. The stage used to be a fresh directory under the system temporary directory, removed by a
// deferred call that a log.Fatal, os.Exit or kill skips, so every such ending left a full copy of the
// corpus where no later run would look. The stage is now a fixed directory beside the corpus that the
// next run finds, reclaims and reports, and refuses to touch while a live run holds it.

package main

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// The default stage is `<root>/.go2cs-platform-stage`, beside `core`, never inside it, and the
// repository's .gitignore ignores it, so a leftover is never swept into a commit.
func TestEmissionStageSitsBesideTheCorpusAndIsIgnored(t *testing.T) {
	root := t.TempDir()

	stageDir, release, _, err := resolveEmissionStageDir("", root)

	if err != nil {
		t.Fatalf("resolveEmissionStageDir: %v", err)
	}

	want := filepath.Join(root, ".go2cs-platform-stage")

	if stageDir != want {
		t.Fatalf("staging directory = %q, want %q (beside the corpus under the output root)", stageDir, want)
	}

	if info, err := os.Stat(stageDir); err != nil || !info.IsDir() {
		t.Fatalf("staging directory %q was not created: %v", stageDir, err)
	}

	release()

	if _, err := os.Stat(stageDir); !os.IsNotExist(err) {
		t.Fatalf("release left the staging directory %q behind (stat error %v)", stageDir, err)
	}

	repoRoot, err := filepath.Abs(filepath.Join("..", ".."))

	if err != nil {
		t.Fatal(err)
	}

	// The in-place regen passes `-go2cspath <repo>/src`, so that is where the stage lands in a clone.
	probe := "src/.go2cs-platform-stage/windows-amd64/src/core/fmt/print.cs"
	out, err := exec.Command("git", "-C", repoRoot, "check-ignore", "--no-index", probe).Output()

	if err != nil || strings.TrimSpace(string(out)) != probe {
		t.Fatalf("the repository's .gitignore does not ignore %s (git check-ignore: %q, %v)", probe, out, err)
	}
}

// A stage left by a run that ended before its cleanup (its lock names a process that is gone) is
// emptied at the start of the next run, and the run reports how much it removed.
func TestEmissionStageReclaimsALeftoverAtStart(t *testing.T) {
	root := t.TempDir()
	stageDir := filepath.Join(root, ".go2cs-platform-stage")

	exited := exec.Command(os.Args[0], "-test.run=^$")

	if err := exited.Run(); err != nil {
		t.Fatalf("starting a process to take a finished PID from: %v", err)
	}

	plantStageFile(t, filepath.Join(stageDir, "owner.pid"), fmt.Sprintf("%d\n", exited.Process.Pid))
	plantStageFile(t, filepath.Join(stageDir, "windows-amd64", "src", "core", "fmt", "print.cs"), "leftover one")
	plantStageFile(t, filepath.Join(stageDir, "linux-amd64", "src", "core", "fmt", "print.cs"), "leftover two!")

	gotDir, release, reclaimed, err := resolveEmissionStageDir("", root)

	if err != nil {
		t.Fatalf("resolveEmissionStageDir over a leftover stage: %v", err)
	}

	defer release()

	if gotDir != stageDir {
		t.Fatalf("staging directory = %q, want %q", gotDir, stageDir)
	}

	if reclaimed.files != 2 || reclaimed.bytes != int64(len("leftover one")+len("leftover two!")) {
		t.Fatalf("reclaimed %d files / %d bytes, want 2 files / %d bytes",
			reclaimed.files, reclaimed.bytes, len("leftover one")+len("leftover two!"))
	}

	for _, target := range []string{"windows-amd64", "linux-amd64"} {
		if _, err := os.Stat(filepath.Join(stageDir, target)); !os.IsNotExist(err) {
			t.Fatalf("leftover %s was not reclaimed (stat error %v)", target, err)
		}
	}

	if holder := readStageLockHolder(filepath.Join(stageDir, "owner.pid")); holder != os.Getpid() {
		t.Fatalf("staging lock holds %d, want this run's %d", holder, os.Getpid())
	}
}

// A stage whose lock names a live process is never emptied: the run refuses, naming the holder.
func TestEmissionStageRefusesWhenALiveRunHoldsIt(t *testing.T) {
	root := t.TempDir()
	stageDir := filepath.Join(root, ".go2cs-platform-stage")
	inFlight := filepath.Join(stageDir, "windows-amd64", "src", "core", "fmt", "print.cs")

	plantStageFile(t, filepath.Join(stageDir, "owner.pid"), fmt.Sprintf("%d\n", os.Getpid()))
	plantStageFile(t, inFlight, "a live run's output")

	_, release, _, err := resolveEmissionStageDir("", root)

	if err == nil {
		release()
		t.Fatal("resolveEmissionStageDir took a staging directory a live process holds")
	}

	if !strings.Contains(err.Error(), fmt.Sprint(os.Getpid())) {
		t.Fatalf("the refusal does not name the holding process %d: %v", os.Getpid(), err)
	}

	if content, err := os.ReadFile(inFlight); err != nil || string(content) != "a live run's output" {
		t.Fatalf("the refusal touched the live run's staged file (%q, %v)", content, err)
	}
}

func plantStageFile(t *testing.T, path string, content string) {
	t.Helper()

	if err := os.MkdirAll(filepath.Dir(path), 0755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(path, []byte(content), 0644); err != nil {
		t.Fatal(err)
	}
}
