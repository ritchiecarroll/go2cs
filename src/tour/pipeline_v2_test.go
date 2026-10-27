package main

import (
	"context"
	"os"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

func TestCollectCSharpPrefersSourceFiles(t *testing.T) {
	root := t.TempDir()
	if err := os.WriteFile(filepath.Join(root, "main.cs"), []byte("class Main {}"), 0o600); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(root, "package_info.cs"), []byte("metadata"), 0o600); err != nil {
		t.Fatal(err)
	}

	got, err := collectCSharp(root)
	if err != nil {
		t.Fatal(err)
	}
	if !strings.Contains(got, "class Main") {
		t.Fatal("source file was not returned")
	}
	if strings.Contains(got, "metadata") {
		t.Fatal("package metadata obscures the matching source view")
	}
	packageInfo, err := readGeneratedFile(root, "package_info.cs")
	if err != nil {
		t.Fatal(err)
	}
	if packageInfo != "metadata" {
		t.Fatalf("package info = %q, want metadata", packageInfo)
	}
}

func TestGo2CSConvertArgsPreserveComments(t *testing.T) {
	runtime := runtimeConfiguration{mode: runtimeCore, converterRoot: "core"}
	got := strings.Join(go2csConvertArgs(runtime, "input", "output"), " ")
	want := "-comments -recurse -go2cspath core input output"
	if got != want {
		t.Fatalf("go2csConvertArgs = %q, want %q", got, want)
	}
}

func TestGo2CSConvertArgsUseNuGetRecursion(t *testing.T) {
	runtime := runtimeConfiguration{mode: runtimeNuGet, converterRoot: "core"}
	got := strings.Join(go2csConvertArgs(runtime, "input", "output"), " ")
	want := "-comments -recurse=nuget -go2cspath core input output"
	if got != want {
		t.Fatalf("go2csConvertArgs = %q, want %q", got, want)
	}
}

func TestRunStageReportsKilledContext(t *testing.T) {
	ctx, cancel := context.WithCancel(context.Background())
	cancel()

	runner := newPipelineRunner(t.TempDir())
	stage := runner.runStage(ctx, "run", ".NET Run", t.TempDir(), time.Second, "go", "version")
	if stage.Status != "killed" {
		t.Fatalf("status = %q, want killed; output: %s", stage.Status, stage.Output)
	}
	if stage.Output != "Program exited: killed" {
		t.Fatalf("output = %q, want Tour-compatible killed message", stage.Output)
	}
}

func TestConversionIDsAreUnique(t *testing.T) {
	first, err := newConversionID()
	if err != nil {
		t.Fatal(err)
	}
	second, err := newConversionID()
	if err != nil {
		t.Fatal(err)
	}
	if first == second || len(first) != 32 || len(second) != 32 {
		t.Fatalf("unexpected conversion IDs: %q %q", first, second)
	}
}

func TestProgramExitMessageMatchesTour(t *testing.T) {
	if got := programExitMessage(context.Background(), context.Background(), nil); got != "Program exited." {
		t.Fatalf("successful exit = %q", got)
	}

	parent, cancel := context.WithCancel(context.Background())
	cancel()
	if got := programExitMessage(parent, parent, context.Canceled); got != "Program exited: killed" {
		t.Fatalf("killed exit = %q", got)
	}

	command, commandCancel := context.WithDeadline(context.Background(), time.Now().Add(-time.Second))
	defer commandCancel()
	if got := programExitMessage(context.Background(), command, context.DeadlineExceeded); got != "Program exited: process took too long." {
		t.Fatalf("timed-out exit = %q", got)
	}
}

// The Tour writes its exit notice after a single newline and never trims what
// the program produced, so the blank line ahead of the notice is the program's
// own trailing newline -- present for a Println, absent for an io.Copy that
// stops mid-line.
func TestAppendSystemSegmentMatchesTourSpacing(t *testing.T) {
	endedLine := appendSystemSegment([]outputSegment{{Kind: outputStdout, Text: "hello\n"}}, "Program exited.")
	if joined := joinSegments(endedLine); joined != "hello\n\nProgram exited." {
		t.Fatalf("transcript after a completed line = %q", joined)
	}
	if len(endedLine) != 2 || endedLine[0].Kind != outputStdout || endedLine[1].Kind != outputSystem {
		t.Fatalf("appended segments = %+v, want the notice tagged as a system message", endedLine)
	}

	midLine := appendSystemSegment([]outputSegment{{Kind: outputStdout, Text: "You cracked the code!"}}, "Program exited.")
	if joined := joinSegments(midLine); joined != "You cracked the code!\nProgram exited." {
		t.Fatalf("transcript after an unterminated line = %q, want no blank line", joined)
	}

	empty := appendSystemSegment(nil, "Program exited: killed")
	if len(empty) != 1 || empty[0].Kind != outputSystem || empty[0].Text != "Program exited: killed" {
		t.Fatalf("empty appendSystemSegment = %+v", empty)
	}
}

// The Tour colors a program's standard error, so a stage has to keep the two
// streams apart all the way to the interface.
func TestRunStageTagsProgramStreams(t *testing.T) {
	runner := newPipelineRunner(t.TempDir())
	defer runner.close()

	written := runner.runStage(context.Background(), "run", ".NET Run", t.TempDir(), 30*time.Second, "go", "version")
	if written.Status != "passed" {
		t.Fatalf("status = %q, want passed; output: %s", written.Status, written.Output)
	}
	if len(written.Segments) != 2 || written.Segments[0].Kind != outputStdout || written.Segments[1].Kind != outputSystem {
		t.Fatalf("segments = %+v, want tagged standard output then the exit notice", written.Segments)
	}

	failed := runner.runStage(context.Background(), "run", ".NET Run", t.TempDir(), 30*time.Second, "go", "nosuchcommand")
	if failed.Status != "failed" {
		t.Fatalf("status = %q, want failed; output: %s", failed.Status, failed.Output)
	}
	if len(failed.Segments) == 0 || failed.Segments[0].Kind != outputStderr {
		t.Fatalf("segments = %+v, want the diagnostic tagged as standard error", failed.Segments)
	}
	if failed.Output != joinSegments(failed.Segments) {
		t.Fatalf("output %q does not match its segments %q", failed.Output, joinSegments(failed.Segments))
	}
}

func TestFormatTranspileTranscriptListsGeneratedFiles(t *testing.T) {
	root := t.TempDir()
	for name := range map[string]struct{}{
		"main.cs":                {},
		"package_info.cs":        {},
		"tour.local.demo.csproj": {},
	} {
		if err := os.WriteFile(filepath.Join(root, name), []byte("generated"), 0o600); err != nil {
			t.Fatal(err)
		}
	}

	got := formatTranspileTranscript("module diagnostic", "go2cs diagnostic", root, "passed", "Core source")
	for _, want := range []string{"$ go mod tidy", "module diagnostic", "$ go2cs -recurse main.go", "Runtime: Core source", "go2cs diagnostic", "main.cs", "tour.local.demo.csproj", "Transpile completed."} {
		if !strings.Contains(got, want) {
			t.Fatalf("transcript missing %q:\n%s", want, got)
		}
	}
}

func TestCollectCSharpRejectsMetadataOnlyOutput(t *testing.T) {
	root := t.TempDir()
	if err := os.WriteFile(filepath.Join(root, "package_info.cs"), []byte("metadata"), 0o600); err != nil {
		t.Fatal(err)
	}

	if _, err := collectCSharp(root); err == nil {
		t.Fatal("metadata-only output was accepted as converted app source")
	}
}

func TestParseTimeoutSetting(t *testing.T) {
	for _, testCase := range []struct {
		value string
		want  time.Duration
	}{
		{"", 0},
		{"15m", 15 * time.Minute},
		{"90s", 90 * time.Second},
		// A bare number is seconds, the unit every sibling GO2CS_*_TIMEOUT variable is written in.
		{"600", 10 * time.Minute},
		{" 5m ", 5 * time.Minute},
	} {
		got, err := parseTimeoutSetting(testCase.value)
		if err != nil {
			t.Fatalf("parseTimeoutSetting(%q) failed: %v", testCase.value, err)
		}
		if got != testCase.want {
			t.Fatalf("parseTimeoutSetting(%q) = %s, want %s", testCase.value, got, testCase.want)
		}
	}

	// A budget that cannot guard anything is a usage error, never a silent zero: a zero budget
	// expires before its child starts, so every host would look too slow to build the converter.
	for _, value := range []string{"0", "-5m", "later", "600000"} {
		if got, err := parseTimeoutSetting(value); err == nil {
			t.Fatalf("parseTimeoutSetting(%q) = %s, want an error", value, got)
		}
	}
}

func TestResolveToolTimeoutPrefersFlagThenEnvironment(t *testing.T) {
	t.Setenv(toolTimeoutEnv, "45m")
	if got := resolveToolTimeout(0); got != 45*time.Minute {
		t.Fatalf("environment budget = %s, want 45m", got)
	}
	if got := resolveToolTimeout(90 * time.Second); got != 90*time.Second {
		t.Fatalf("supplied budget = %s, want the caller's 90s to win over the environment", got)
	}

	// An unreadable variable may have been inherited from a shell the operator never set up, so it
	// warns and falls back rather than refusing to start.
	t.Setenv(toolTimeoutEnv, "whenever")
	if got := resolveToolTimeout(0); got != defaultToolTimeout {
		t.Fatalf("malformed environment budget = %s, want the default %s", got, defaultToolTimeout)
	}

	t.Setenv(toolTimeoutEnv, "")
	if got := resolveToolTimeout(0); got != defaultToolTimeout {
		t.Fatalf("unset budget = %s, want the default %s", got, defaultToolTimeout)
	}

	runner := newPipelineRunner(t.TempDir(), pipelineOptions{toolTimeout: 3 * time.Minute})
	defer runner.close()
	if got := runner.buildToolTimeout(); got != 3*time.Minute {
		t.Fatalf("runner budget = %s, want the configured 3m", got)
	}
	if got := newPipelineRunner(t.TempDir()).buildToolTimeout(); got != defaultToolTimeout {
		t.Fatalf("default runner budget = %s, want %s", got, defaultToolTimeout)
	}
}

// A cached converter is reused only while it is newer than the checkout it will be driven with,
// which is the freshness test the behavioral harnesses apply to go2cs.exe.
func TestConverterIsFreshComparesAgainstEveryConverterSource(t *testing.T) {
	root := t.TempDir()
	sourceDir := filepath.Join(root, "go2cs")
	if err := os.MkdirAll(filepath.Join(sourceDir, "internal"), 0o755); err != nil {
		t.Fatal(err)
	}
	sources := []string{
		filepath.Join(sourceDir, "main.go"),
		filepath.Join(sourceDir, "go.mod"),
		filepath.Join(sourceDir, "internal", "nested.go"),
	}
	for _, source := range sources {
		if err := os.WriteFile(source, []byte("package main"), 0o600); err != nil {
			t.Fatal(err)
		}
	}

	target := filepath.Join(root, "go2cs.bin")
	if err := os.WriteFile(target, []byte("binary"), 0o700); err != nil {
		t.Fatal(err)
	}

	built := time.Now()
	if err := os.Chtimes(target, built, built); err != nil {
		t.Fatal(err)
	}
	for _, source := range sources {
		converted := built.Add(-time.Hour)
		if err := os.Chtimes(source, converted, converted); err != nil {
			t.Fatal(err)
		}
	}
	if !converterIsFresh(target, sourceDir) {
		t.Fatal("a converter newer than every source was treated as stale")
	}

	// Any one source moving ahead of the binary is enough, including a nested one and a dependency
	// change that leaves every .go file untouched.
	for _, source := range sources {
		touched := built.Add(time.Minute)
		if err := os.Chtimes(source, touched, touched); err != nil {
			t.Fatal(err)
		}
		if converterIsFresh(target, sourceDir) {
			t.Fatalf("a converter older than %s was reused", filepath.Base(source))
		}
		restored := built.Add(-time.Hour)
		if err := os.Chtimes(source, restored, restored); err != nil {
			t.Fatal(err)
		}
	}

	if converterIsFresh(filepath.Join(root, "absent.bin"), sourceDir) {
		t.Fatal("a missing converter was reported fresh")
	}
	if converterIsFresh(target, filepath.Join(root, "absent")) {
		t.Fatal("a converter was reported fresh against a source tree that does not exist")
	}
}

// Timestamps cannot tell a converter from a file that merely occupies the cache path, so a cached
// executable is also made to identify itself before it is trusted.
func TestConverterIdentifiesRejectsAnythingButTheConverter(t *testing.T) {
	root := t.TempDir()
	sourceDir := filepath.Join(root, "go2cs")
	if err := os.MkdirAll(sourceDir, 0o755); err != nil {
		t.Fatal(err)
	}
	source := filepath.Join(sourceDir, "main.go")
	if err := os.WriteFile(source, []byte("package main"), 0o600); err != nil {
		t.Fatal(err)
	}
	// Dated deliberately: two files written in one clock tick carry the same timestamp, which
	// reads as stale and would make this test pass for the wrong reason.
	written := time.Now().Add(-time.Hour)
	if err := os.Chtimes(source, written, written); err != nil {
		t.Fatal(err)
	}

	// In a directory of its own: without the .exe suffix, root/go2cs is the source directory.
	imposter := filepath.Join(root, "bin", "go2cs")
	if err := os.MkdirAll(filepath.Dir(imposter), 0o755); err != nil {
		t.Fatal(err)
	}
	if filepath.Separator == '\\' {
		imposter += ".exe"
	}
	if err := os.WriteFile(imposter, []byte("stale converter"), 0o700); err != nil {
		t.Fatal(err)
	}

	if converterIdentifies(context.Background(), imposter) {
		t.Fatal("a file that is not the converter identified itself as one")
	}
	if !converterIsFresh(imposter, sourceDir) {
		t.Fatal("test setup: the imposter should be newer than the sources, so only the probe rejects it")
	}
	if converterIsCurrent(context.Background(), imposter, sourceDir) {
		t.Fatal("a newer-but-unrunnable file was accepted as the current converter")
	}
}

// A converter build that fails keeps its own identity all the way to the interface. It used to be
// rewritten to the transpile stage, which reported a build that produced no converter as a failed
// conversion of source the converter had never been handed.
func TestEnsureGo2CSReportsTheBuildAsItsOwnStage(t *testing.T) {
	t.Setenv("GO2CS_BIN", "")

	// A repository root with no src/go2cs fails the build immediately, so this costs no compile.
	runner := newPipelineRunner(t.TempDir())
	defer runner.close()

	_, stage := runner.ensureGo2CS(context.Background())
	if stage == nil {
		t.Fatal("no stage was reported for a converter build that cannot run")
	}
	if stage.Status == "passed" {
		t.Fatalf("build of an absent converter passed: %s", stage.Output)
	}
	if stage.ID != "tool" || stage.Label != "Build go2cs" {
		t.Fatalf("stage identity = %q/%q, want tool/Build go2cs", stage.ID, stage.Label)
	}
}

func TestFormatConverterBuildTranscriptNamesTheBuildNotTheConversion(t *testing.T) {
	timedOut := formatConverterBuildTranscript("module diagnostic", "go: downloading golang.org/x/mod",
		"<repo>/src/tour/.cache/go2cs", "failed", true)
	for _, want := range []string{
		"$ go mod tidy",
		"module diagnostic",
		"$ go build -o <repo>/src/tour/.cache/go2cs ./src/go2cs",
		"go: downloading golang.org/x/mod",
		"the converter never ran",
		"-tool-timeout",
		toolTimeoutEnv,
		"GO2CS_BIN",
	} {
		if !strings.Contains(timedOut, want) {
			t.Fatalf("timed-out transcript missing %q:\n%s", want, timedOut)
		}
	}
	if strings.Contains(timedOut, "Transpile") {
		t.Fatalf("a converter build that never ran was reported as a transpile:\n%s", timedOut)
	}

	failed := formatConverterBuildTranscript("", "undefined: symbol", "go2cs", "failed", false)
	if !strings.Contains(failed, "Building go2cs failed.") || strings.Contains(failed, "-tool-timeout") {
		t.Fatalf("failed build transcript = %q, want a plain failure with no budget advice", failed)
	}

	killed := formatConverterBuildTranscript("", "", "go2cs", "killed", false)
	if !strings.Contains(killed, "Building go2cs killed.") {
		t.Fatalf("killed build transcript = %q", killed)
	}
}

func TestTourAppOutputDir(t *testing.T) {
	root := filepath.FromSlash("C:/temp/output")
	want := filepath.Join(root, "src", "tour.local", "session")

	if got := tourAppOutputDir(root); got != want {
		t.Fatalf("tourAppOutputDir = %q, want %q", got, want)
	}
}
