package main

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"net/http/httptest"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

// stubToolchainEnv names the directory a stubbed toolchain process records its calls in. It is set
// only on the processes stubToolchain starts, which are this test binary re-entered.
const stubToolchainEnv = "TOUR_STUB_TOOLCHAIN_DIR"

// TestStubToolchainProcess is not a test: it is the toolchain the prebuild tests drive, re-entered
// from the test binary in place of go, go2cs and dotnet. It converts by writing a minimal app
// project, builds by printing a success line (holding first while the hold file exists), and runs
// by printing one line. Every build records "build-start <dir>" and, if it is not killed first,
// "build-end <dir>", so a test can count the builds that ran and where.
//
// A build also writes the program a real one would -- an assembly and its runtimeconfig.json under
// the project's bin folder -- and names it the way MSBuild's minimal console logger does, after a
// dependency's line, unless the nopath file exists. A run records how it was launched: "run-dll
// <assembly>" for `dotnet <assembly>`, "run <dir> <args>" for `dotnet run`.
func TestStubToolchainProcess(t *testing.T) {
	dir := os.Getenv(stubToolchainEnv)
	if dir == "" {
		return
	}
	args := os.Args
	for len(args) > 0 && args[0] != "--" {
		args = args[1:]
	}
	if len(args) < 2 {
		fmt.Fprintln(os.Stderr, "stub toolchain: no command")
		os.Exit(2)
	}
	tool := strings.TrimSuffix(filepath.Base(args[1]), ".exe")
	args = args[2:]
	workDir, _ := os.Getwd()

	switch {
	case tool == "go":
		os.Exit(0)
	case strings.HasPrefix(tool, "go2cs"):
		app := tourAppOutputDir(args[len(args)-1])
		if err := os.MkdirAll(app, 0o755); err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(1)
		}
		_ = os.WriteFile(filepath.Join(app, "main.cs"), []byte("class Program {}\n"), 0o600)
		_ = os.WriteFile(filepath.Join(app, tourProjectName), []byte("<Project />\n"), 0o600)
		fmt.Println("Converting tour.local/session")
		os.Exit(0)
	case tool == "dotnet" && len(args) > 0 && args[0] == "build":
		recordStubCall(dir, "build-start "+workDir)
		for {
			if _, err := os.Stat(filepath.Join(dir, "hold")); err != nil {
				break
			}
			time.Sleep(10 * time.Millisecond)
		}
		recordStubCall(dir, "build-end "+workDir)
		if _, err := os.Stat(filepath.Join(dir, "nopath")); err != nil && len(args) > 1 {
			project := args[1]
			name := strings.TrimSuffix(filepath.Base(project), filepath.Ext(project))
			output := filepath.Join(filepath.Dir(project), "bin", "Debug", "net10.0")
			assembly := filepath.Join(output, name+".dll")
			_ = os.MkdirAll(output, 0o755)
			_ = os.WriteFile(assembly, []byte("stub"), 0o600)
			_ = os.WriteFile(filepath.Join(output, name+".runtimeconfig.json"), []byte("{}"), 0o600)
			fmt.Printf("  go.lib -> %s\n", filepath.Join(dir, "go.lib.dll"))
			fmt.Printf("  %s -> %s\n", name, assembly)
		}
		fmt.Println("Build succeeded.")
		os.Exit(0)
	case tool == "dotnet" && len(args) > 0 && args[0] == "run":
		recordStubCall(dir, "run "+workDir+" "+strings.Join(args, " "))
		fmt.Println("hello from the stub")
		os.Exit(0)
	case tool == "dotnet" && len(args) == 1 && strings.HasSuffix(args[0], ".dll"):
		recordStubCall(dir, "run-dll "+args[0])
		fmt.Println("hello from the stub")
		os.Exit(0)
	}
	fmt.Fprintf(os.Stderr, "stub toolchain: unexpected command %s %v\n", tool, args)
	os.Exit(2)
}

func recordStubCall(dir, line string) {
	file, err := os.OpenFile(filepath.Join(dir, "calls.log"), os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0o600)
	if err != nil {
		return
	}
	defer file.Close()
	_, _ = file.WriteString(line + "\n")
}

type toolchainStub struct {
	dir string
}

// stubToolchain replaces every command the pipeline starts with TestStubToolchainProcess for the
// rest of the test, and points GO2CS_BIN at a file so no converter is built.
func stubToolchain(t *testing.T) *toolchainStub {
	t.Helper()
	dir := t.TempDir()
	converter := filepath.Join(dir, "go2cs-stub.exe")
	if err := os.WriteFile(converter, []byte("stub"), 0o600); err != nil {
		t.Fatal(err)
	}
	t.Setenv("GO2CS_BIN", converter)

	previous := newCommand
	newCommand = func(ctx context.Context, name string, args ...string) *exec.Cmd {
		stubArgs := append([]string{"-test.run=^TestStubToolchainProcess$", "--", name}, args...)
		command := exec.CommandContext(ctx, os.Args[0], stubArgs...)
		command.Env = append(os.Environ(), stubToolchainEnv+"="+dir)
		return command
	}
	t.Cleanup(func() { newCommand = previous })
	return &toolchainStub{dir: dir}
}

// hold makes every build that starts from now on wait until release.
func (s *toolchainStub) hold(t *testing.T) {
	t.Helper()
	if err := os.WriteFile(filepath.Join(s.dir, "hold"), nil, 0o600); err != nil {
		t.Fatal(err)
	}
}

func (s *toolchainStub) release(t *testing.T) {
	t.Helper()
	if err := os.Remove(filepath.Join(s.dir, "hold")); err != nil && !os.IsNotExist(err) {
		t.Fatal(err)
	}
}

// omitAssemblyPath makes every build from now on print no output-assembly line, as a build whose
// logger does not name its output would.
func (s *toolchainStub) omitAssemblyPath(t *testing.T) {
	t.Helper()
	if err := os.WriteFile(filepath.Join(s.dir, "nopath"), nil, 0o600); err != nil {
		t.Fatal(err)
	}
}

// calls reports the recorded calls that begin with prefix.
func (s *toolchainStub) calls(prefix string) []string {
	content, _ := os.ReadFile(filepath.Join(s.dir, "calls.log"))
	var found []string
	for _, line := range strings.Split(string(content), "\n") {
		if strings.HasPrefix(line, prefix) {
			found = append(found, line)
		}
	}
	return found
}

// count reports how many recorded calls begin with prefix.
func (s *toolchainStub) count(prefix string) int {
	return len(s.calls(prefix))
}

// awaitCount waits until at least want calls beginning with prefix are recorded.
func (s *toolchainStub) awaitCount(t *testing.T, prefix string, want int) {
	t.Helper()
	deadline := time.Now().Add(10 * time.Second)
	for s.count(prefix) < want {
		if time.Now().After(deadline) {
			t.Fatalf("%d %q calls recorded, want %d", s.count(prefix), prefix, want)
		}
		time.Sleep(10 * time.Millisecond)
	}
}

// convertForPage converts a minimal program the way the page does, through /api/convert.
func convertForPage(t *testing.T, s *server) string {
	t.Helper()
	body := `{"source":"package main\n\nfunc main() {}\n","name":"x","runtime":"core"}`
	recorder := httptest.NewRecorder()
	s.handleConvert(recorder, httptest.NewRequest("POST", "/api/convert", strings.NewReader(body)))
	var result convertResult
	if err := json.NewDecoder(recorder.Body).Decode(&result); err != nil {
		t.Fatalf("convert response (%d): %v", recorder.Code, err)
	}
	if !result.Successful || result.ConversionID == "" {
		t.Fatalf("conversion failed:\n%s", result.Stage.Output)
	}
	return result.ConversionID
}

// buildInBackground calls build and delivers what it returned.
func buildInBackground(runner *pipelineRunner, id string) <-chan error {
	done := make(chan error, 1)
	go func() {
		ctx, cancel := context.WithTimeout(context.Background(), 30*time.Second)
		defer cancel()
		result, err := runner.build(ctx, runRequest{ConversionID: id})
		if err == nil && result.Status != "passed" {
			err = fmt.Errorf("build %s:\n%s", result.Status, result.Output)
		}
		done <- err
	}()
	return done
}

func awaitResult(t *testing.T, done <-chan error) error {
	t.Helper()
	select {
	case err := <-done:
		return err
	case <-time.After(10 * time.Second):
		t.Fatal("build never returned")
		return nil
	}
}

func newStubbedServer(t *testing.T, options pipelineOptions) *server {
	t.Helper()
	root := t.TempDir()
	if options.defaultRuntime == "" {
		options.defaultRuntime = runtimeCore
	}
	runner := newPipelineRunner(root, options)
	t.Cleanup(runner.close)
	return &server{repoRoot: &root, pipeline: runner}
}

// A conversion made for the page starts its build at once, and the build the page asks for next is
// that one: no second build runs.
func TestConversionForThePageStartsItsBuild(t *testing.T) {
	stub := stubToolchain(t)
	s := newStubbedServer(t, pipelineOptions{noWarmup: true})

	id := convertForPage(t, s)
	stub.awaitCount(t, "build-end", 1)

	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()
	build, err := s.pipeline.build(ctx, runRequest{ConversionID: id})
	if err != nil {
		t.Fatal(err)
	}
	if build.ID != "build" || build.Status != "passed" || !strings.Contains(build.Output, "Build succeeded.") {
		t.Fatalf("build = %+v, want the background build's passed stage", build)
	}
	if got := stub.count("build-start"); got != 1 {
		t.Fatalf("%d builds ran, want exactly the one the conversion started", got)
	}

	run, err := s.pipeline.execute(ctx, runRequest{ConversionID: id})
	if err != nil {
		t.Fatal(err)
	}
	if run.Status != "passed" || !strings.Contains(run.Output, "hello from the stub") {
		t.Fatalf("execute = %+v, want the program's output", run)
	}
}

// A build requested while the background build is still running waits for it and returns its
// result.
func TestBuildWaitsForTheRunningPrebuild(t *testing.T) {
	stub := stubToolchain(t)
	s := newStubbedServer(t, pipelineOptions{noWarmup: true})
	stub.hold(t)
	defer stub.release(t)

	id := convertForPage(t, s)
	stub.awaitCount(t, "build-start", 1)

	done := buildInBackground(s.pipeline, id)
	select {
	case err := <-done:
		t.Fatalf("build returned while the background build was held: %v", err)
	case <-time.After(150 * time.Millisecond):
	}

	stub.release(t)
	if err := awaitResult(t, done); err != nil {
		t.Fatal(err)
	}
	if got := stub.count("build-start"); got != 1 {
		t.Fatalf("%d builds ran, want exactly the one the conversion started", got)
	}
}

// A conversion forgotten while its build runs -- expired, or the server closing -- cancels that
// build, and a waiter gets the ordinary expired-conversion error rather than hanging.
func TestForgottenConversionCancelsItsPrebuild(t *testing.T) {
	for name, forget := range map[string]func(*pipelineRunner, string){
		"expired": func(runner *pipelineRunner, id string) {
			runner.mu.Lock()
			runner.saved[id].createdAt = time.Now().Add(-2 * conversionMaxAge)
			runner.mu.Unlock()
			runner.cleanupExpired()
		},
		"closed": func(runner *pipelineRunner, _ string) { runner.close() },
	} {
		t.Run(name, func(t *testing.T) {
			stub := stubToolchain(t)
			s := newStubbedServer(t, pipelineOptions{noWarmup: true})
			stub.hold(t)
			defer stub.release(t)

			id := convertForPage(t, s)
			stub.awaitCount(t, "build-start", 1)
			s.pipeline.mu.Lock()
			workDir := s.pipeline.saved[id].workDir
			s.pipeline.mu.Unlock()

			done := buildInBackground(s.pipeline, id)
			time.Sleep(50 * time.Millisecond)
			forget(s.pipeline, id)

			var notFound conversionNotFoundError
			if err := awaitResult(t, done); !errors.As(err, &notFound) {
				t.Fatalf("build of a forgotten conversion = %v, want the expired-conversion error", err)
			}
			if got := stub.count("build-end"); got != 0 {
				t.Fatalf("a cancelled build ran to the end (%d build-end calls)", got)
			}
			deadline := time.Now().Add(10 * time.Second)
			for {
				if _, err := os.Stat(workDir); os.IsNotExist(err) {
					break
				}
				if time.Now().After(deadline) {
					t.Fatalf("the forgotten conversion's workspace %s was not removed", workDir)
				}
				time.Sleep(10 * time.Millisecond)
			}
		})
	}
}

// With a single pipeline slot the background build holds it, and a build request waiting for that
// build must not hold it too.
func TestSinglePipelineSlotDoesNotDeadlock(t *testing.T) {
	stub := stubToolchain(t)
	s := newStubbedServer(t, pipelineOptions{noWarmup: true})
	s.pipeline.slots = make(chan struct{}, 1)
	stub.hold(t)
	defer stub.release(t)

	id := convertForPage(t, s)
	stub.awaitCount(t, "build-start", 1)
	done := buildInBackground(s.pipeline, id)
	time.Sleep(50 * time.Millisecond)
	stub.release(t)
	if err := awaitResult(t, done); err != nil {
		t.Fatal(err)
	}

	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()
	run, err := s.pipeline.run(ctx, runRequest{ConversionID: id})
	if err != nil {
		t.Fatal(err)
	}
	if !run.Successful || len(run.Stages) != 2 {
		t.Fatalf("run = %+v, want the build and a passed run", run)
	}
	if got := stub.count("build-start"); got != 1 {
		t.Fatalf("%d builds ran, want exactly the one the conversion started (run reuses it too)", got)
	}
}

// The warmup converts and builds for itself and starts no background build; a conversion made for
// the page during the warmup builds only after the warmup ends, as any .NET build does.
func TestWarmupDoesNotStartAPrebuild(t *testing.T) {
	stub := stubToolchain(t)
	s := newStubbedServer(t, pipelineOptions{})
	stub.hold(t)
	defer stub.release(t)

	if !s.pipeline.startWarmup(context.Background()) {
		t.Fatal("the warmup did not start")
	}
	stub.awaitCount(t, "build-start", 1)

	id := convertForPage(t, s)
	time.Sleep(200 * time.Millisecond)
	if got := stub.count("build-start"); got != 1 {
		t.Fatalf("%d builds started mid-warmup, want only the warmup's own", got)
	}

	stub.release(t)
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()
	if err := s.pipeline.warm.wait(ctx); err != nil {
		t.Fatal(err)
	}
	if got := s.pipeline.warm.status().State; got != warmupReady {
		t.Fatalf("warmup state = %q, want %q", got, warmupReady)
	}
	// The page's conversion builds once the warmup is over, before anything asks it to.
	stub.awaitCount(t, "build-end", 2)
	if err := awaitResult(t, buildInBackground(s.pipeline, id)); err != nil {
		t.Fatal(err)
	}
	if builds, runs := stub.count("build-start"), stub.count("run"); builds != 2 || runs != 1 {
		t.Fatalf("%d builds and %d runs, want the warmup's build and run plus the page's one build", builds, runs)
	}
}

// A newer conversion cancels an earlier conversion's background build, and a build of the earlier
// conversion, if one is still asked for, builds as it would have with no background build.
func TestNewerConversionCancelsAnEarlierPrebuild(t *testing.T) {
	stub := stubToolchain(t)
	s := newStubbedServer(t, pipelineOptions{noWarmup: true})
	stub.hold(t)
	defer stub.release(t)

	first := convertForPage(t, s)
	stub.awaitCount(t, "build-start", 1)
	s.pipeline.mu.Lock()
	firstBuild := s.pipeline.saved[first].prebuild
	s.pipeline.mu.Unlock()

	second := convertForPage(t, s)
	select {
	case <-firstBuild.done:
	case <-time.After(10 * time.Second):
		t.Fatal("the earlier conversion's background build was not cancelled")
	}
	if firstBuild.ok {
		t.Fatal("a cancelled background build reported a result")
	}
	stub.awaitCount(t, "build-start", 2)

	stub.release(t)
	if err := awaitResult(t, buildInBackground(s.pipeline, second)); err != nil {
		t.Fatal(err)
	}
	if got := stub.count("build-start"); got != 2 {
		t.Fatalf("%d builds ran, want one per conversion so far", got)
	}
	if err := awaitResult(t, buildInBackground(s.pipeline, first)); err != nil {
		t.Fatal(err)
	}
	if got := stub.count("build-start"); got != 3 {
		t.Fatalf("%d builds ran, want the earlier conversion built again on request", got)
	}
}
