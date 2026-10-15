package main

import (
	"context"
	"crypto/rand"
	"encoding/hex"
	"errors"
	"fmt"
	"io/fs"
	"log"
	"os"
	"os/exec"
	"path/filepath"
	"sort"
	"strconv"
	"strings"
	"sync"
	"time"
)

const (
	maxSourceBytes  = 256 << 10
	maxRequestBytes = maxSourceBytes + (16 << 10)
	commandTimeout  = 20 * time.Second
	// buildTimeout covers the dotnet build stages, which under the "core" and
	// "deployed" runtimes compile the lesson's full converted-stdlib project
	// closure on a cold cache (dozens of projects for an fmt import) -- minutes,
	// not seconds. This is a hang guard, not a pacing device.
	buildTimeout      = 5 * time.Minute
	dependencyTimeout = 2 * time.Minute
	// defaultToolTimeout covers the one-time `go build` of the converter that the first conversion
	// of a server process performs. It was a hardcoded 2 minutes until 2026-08-10, which made it a
	// performance assumption rather than a hang guard: measured that day on an i7-5820K under WSL2
	// with the repository on a /mnt DrvFs mount, a cold build of src/go2cs took 1m58.8s and a warm
	// one (module cache hot) 1m44.3s -- roughly one second inside the old cap. So the first
	// conversion after a restart timed out having printed only the golang.org/x/mod, x/tools and
	// x/sync downloads, and the interface reported it as a failed transpile of source the converter
	// had never seen. Sized here for the slowest host this is expected to run on, per the safety-net
	// doctrine; a caller who wants a tighter net asks for one (-tool-timeout / GO2CS_TOOL_TIMEOUT).
	defaultToolTimeout = 10 * time.Minute
	toolTimeoutEnv     = "GO2CS_TOOL_TIMEOUT"
	// converterProbeTimeout bounds the identity probe a cached converter must pass before it is
	// reused. It runs the executable with no arguments, which reaches the usage banner after at
	// most a `go env` call, so anything near this budget is a file that is not going to answer.
	converterProbeTimeout = 20 * time.Second
	conversionMaxAge      = 30 * time.Minute
	maxSavedArtifacts     = 20
	tourModulePath        = "tour.local/session"
	tourProjectName       = "tour.local.session.csproj"
)

type convertRequest struct {
	Source  string `json:"source"`
	Name    string `json:"name,omitempty"`
	Runtime string `json:"runtime,omitempty"`
}

type runRequest struct {
	ConversionID string `json:"conversionId"`
}

type convertResult struct {
	ConversionID string      `json:"conversionId,omitempty"`
	CSharp       string      `json:"csharp,omitempty"`
	PackageInfo  string      `json:"packageInfo,omitempty"`
	Project      string      `json:"project,omitempty"`
	ProjectName  string      `json:"projectName,omitempty"`
	Successful   bool        `json:"successful"`
	Runtime      string      `json:"runtime"`
	Stage        stageResult `json:"stage"`
}

type runResult struct {
	Successful bool          `json:"successful"`
	Stages     []stageResult `json:"stages"`
}

type stageResult struct {
	ID     string `json:"id"`
	Label  string `json:"label"`
	Status string `json:"status"`
	// TimedOut separates "this stage's own budget expired" from "the tool ran and reported a
	// problem". Only the first leaves the question unanswered -- nothing was measured -- and the
	// two need different words in front of the operator, which is the distinction BehavioralRunner
	// draws between Fail and NOT MEASURED. It rides the wire so the interface can draw it too.
	TimedOut   bool            `json:"timedOut,omitempty"`
	Output     string          `json:"output,omitempty"`
	Segments   []outputSegment `json:"segments,omitempty"`
	DurationMS int64           `json:"durationMs"`
}

type conversionArtifact struct {
	id        string
	workDir   string
	outputDir string
	project   string
	runtime   runtimeConfiguration
	createdAt time.Time
}

type pipelineRunner struct {
	repoRoot       string
	cacheDir       string
	slots          chan struct{}
	buildMu        sync.Mutex
	go2csBin       string
	mu             sync.Mutex
	saved          map[string]*conversionArtifact
	defaultRuntime string
	deployedRoot   string
	nugetSource    string
	nugetVersion   string
	toolTimeout    time.Duration
	warm           *warmup
}

func newPipelineRunner(repoRoot string, supplied ...pipelineOptions) *pipelineRunner {
	options := pipelineOptions{}
	if len(supplied) > 0 {
		options = supplied[0]
	}
	options = resolvePipelineOptions(repoRoot, options)
	return &pipelineRunner{
		repoRoot:       repoRoot,
		cacheDir:       filepath.Join(repoRoot, "src", "tour", ".cache"),
		slots:          make(chan struct{}, 2),
		saved:          make(map[string]*conversionArtifact),
		defaultRuntime: options.defaultRuntime,
		deployedRoot:   options.deployedRoot,
		nugetSource:    options.nugetSource,
		nugetVersion:   options.nugetVersion,
		toolTimeout:    options.toolTimeout,
		warm:           newWarmup(!options.noWarmup),
	}
}

func (p *pipelineRunner) convert(ctx context.Context, request convertRequest) (convertResult, error) {
	source := strings.TrimPrefix(request.Source, "\ufeff")
	if len(source) == 0 {
		return convertResult{}, validationError("the Go editor is empty")
	}
	if len(source) > maxSourceBytes {
		return convertResult{}, validationError("source exceeds the 256 KiB local limit")
	}
	runtime, err := p.resolveRuntime(request.Runtime)
	if err != nil {
		return convertResult{}, err
	}

	if err := p.acquire(ctx); err != nil {
		return convertResult{}, err
	}
	defer p.release()
	p.cleanupExpired()

	workDir, err := os.MkdirTemp("", "tour-go2cs-")
	if err != nil {
		return convertResult{}, err
	}
	keepWorkspace := false
	defer func() {
		if !keepWorkspace {
			_ = os.RemoveAll(workDir)
		}
	}()

	inputDir := filepath.Join(workDir, "go")
	outputDir := filepath.Join(workDir, "cs")
	if err := os.MkdirAll(inputDir, 0o755); err != nil {
		return convertResult{}, err
	}
	if err := os.WriteFile(filepath.Join(inputDir, "main.go"), []byte(source), 0o600); err != nil {
		return convertResult{}, err
	}
	if err := os.WriteFile(filepath.Join(inputDir, "go.mod"), []byte("module "+tourModulePath+"\n\ngo 1.23.1\n"), 0o600); err != nil {
		return convertResult{}, err
	}

	resolve := p.runStage(ctx, "resolve", "Resolve Go dependencies", inputDir, dependencyTimeout,
		"go", "mod", "tidy")

	if resolve.Status != "passed" {
		resolve.ID = "transpile"
		resolve.Label = "Transpile"
		// A transcript is assembled prose, not the command's streams, so the
		// tagged segments it was built from no longer describe it.
		resolve.Output, resolve.Segments = formatTranspileTranscript(resolve.Output, "", outputDir, resolve.Status, runtime.label), nil
		return convertResult{Runtime: runtime.mode, Stage: resolve}, nil
	}

	go2csBin, toolStage := p.ensureGo2CS(ctx)
	if toolStage != nil && toolStage.Status != "passed" {
		// The stage keeps its own identity. Rewriting it to "transpile"/"Transpile" (as this did
		// until 2026-08-10) charged a converter BUILD that produced no binary to the conversion of
		// the operator's code: on a slow host the build's fixed budget expired and the interface
		// reported "Transpile failed" for source go2cs had never been handed.
		toolStage.Output, toolStage.Segments = formatConverterBuildTranscript(
			resolve.Output, toolStage.Output, cleanDisplayPath(go2csBin, p.repoRoot), toolStage.Status, toolStage.TimedOut), nil
		return convertResult{Runtime: runtime.mode, Stage: *toolStage}, nil
	}

	transpile := p.runStage(ctx, "transpile", "Transpile", p.repoRoot, commandTimeout,
		go2csBin, go2csConvertArgs(runtime, inputDir, outputDir)...)
	if toolStage != nil {
		transpile.DurationMS += toolStage.DurationMS
		if toolStage.Output != "" {
			transpile.Output = strings.TrimSpace(toolStage.Output + "\n" + transpile.Output)
		}
	}

	result := convertResult{Runtime: runtime.mode, Stage: transpile}
	appOutputDir := tourAppOutputDir(outputDir)
	result.CSharp, err = collectCSharp(appOutputDir)
	result.PackageInfo, _ = readGeneratedFile(appOutputDir, "package_info.cs")
	project := filepath.Join(appOutputDir, tourProjectName)

	if err != nil && result.Stage.Status == "passed" {
		result.Stage.Status = "failed"
		result.Stage.Output = appendOutputLine(result.Stage.Output, err.Error())
	}

	if _, statErr := os.Stat(project); statErr == nil {
		if err := prepareRuntimeProject(project, outputDir, runtime); err != nil {
			result.Stage.Status = "failed"
			result.Stage.Output = appendOutputLine(result.Stage.Output, err.Error())
		}
		projectBytes, readErr := os.ReadFile(project)
		if readErr == nil {
			result.Project = string(projectBytes)
			result.ProjectName = filepath.Base(project)
		}
	} else if result.Stage.Status == "passed" {
		result.Stage.Status = "failed"
		result.Stage.Output = appendOutputLine(result.Stage.Output, "go2cs did not emit the submitted app project.")
	}
	result.Stage.Output, result.Stage.Segments = formatTranspileTranscript(resolve.Output, result.Stage.Output, outputDir, result.Stage.Status, runtime.label), nil
	if result.Stage.Status != "passed" {
		return result, nil
	}

	id, err := newConversionID()
	if err != nil {
		return convertResult{}, err
	}
	artifact := &conversionArtifact{
		id:        id,
		workDir:   workDir,
		outputDir: outputDir,
		project:   project,
		runtime:   runtime,
		createdAt: time.Now(),
	}
	p.mu.Lock()
	p.saved[id] = artifact
	p.mu.Unlock()

	keepWorkspace = true
	result.ConversionID = id
	result.Successful = true
	return result, nil
}

func go2csConvertArgs(runtime runtimeConfiguration, inputDir, outputDir string) []string {
	recurse := "-recurse"

	if runtime.mode == runtimeNuGet {
		recurse = "-recurse=nuget"
	}

	return []string{"-comments", recurse, "-go2cspath", runtime.converterRoot, inputDir, outputDir}
}

func tourAppOutputDir(outputRoot string) string {
	return filepath.Join(outputRoot, "src", filepath.FromSlash(tourModulePath))
}

func (p *pipelineRunner) run(ctx context.Context, request runRequest) (runResult, error) {
	if strings.TrimSpace(request.ConversionID) == "" {
		return runResult{}, validationError("conversionId is required")
	}
	if err := p.waitForWarmup(ctx); err != nil {
		return runResult{}, err
	}
	return p.runConversion(ctx, request)
}

// waitForWarmup holds a .NET build until a running warmup ends. The warmup is restoring the same
// go.* packages into the same NuGet cache and starting the same build servers, so a build that
// waits for it gets a warm toolchain instead of racing it for one. It waits before taking a pipeline
// slot: the warmup needs a slot of its own to finish.
func (p *pipelineRunner) waitForWarmup(ctx context.Context) error {
	return p.warm.wait(ctx)
}

func (p *pipelineRunner) runConversion(ctx context.Context, request runRequest) (runResult, error) {
	if err := p.acquire(ctx); err != nil {
		return runResult{}, err
	}
	defer p.release()
	p.cleanupExpired()

	p.mu.Lock()
	artifact := p.saved[request.ConversionID]
	p.mu.Unlock()
	if artifact == nil {
		return runResult{}, conversionNotFoundError("that conversion has expired; convert the current Go source again")
	}

	buildArgs := []string{"build", artifact.project, "--nologo", "--verbosity:minimal"}
	buildArgs = append(buildArgs, runtimeMSBuildArgs(artifact.runtime)...)
	build := p.runStage(ctx, "build", "Build", artifact.outputDir, buildTimeout, "dotnet", buildArgs...)
	result := runResult{Stages: []stageResult{build}}
	if build.Status != "passed" {
		result.Stages = append(result.Stages, skippedStage("run", ".NET Run"))
		return result, nil
	}

	runArgs := []string{"run", "--no-build", "--project", artifact.project}
	runArgs = append(runArgs, runtimeMSBuildArgs(artifact.runtime)...)
	run := p.runStage(ctx, "run", ".NET Run", artifact.outputDir, commandTimeout, "dotnet", runArgs...)
	result.Stages = append(result.Stages, run)
	result.Successful = run.Status == "passed"
	return result, nil
}

func (p *pipelineRunner) acquire(ctx context.Context) error {
	select {
	case p.slots <- struct{}{}:
		return nil
	case <-ctx.Done():
		return ctx.Err()
	}
}

func (p *pipelineRunner) release() {
	<-p.slots
}

func (p *pipelineRunner) ensureGo2CS(ctx context.Context) (string, *stageResult) {
	if configured := strings.TrimSpace(os.Getenv("GO2CS_BIN")); configured != "" {
		if _, err := os.Stat(configured); err == nil {
			return configured, nil
		}
	}

	name := "go2cs"
	if filepath.Separator == '\\' {
		name += ".exe"
	}
	target := filepath.Join(p.cacheDir, name)

	p.buildMu.Lock()
	defer p.buildMu.Unlock()
	if p.go2csBin != "" {
		if _, err := os.Stat(p.go2csBin); err == nil {
			return p.go2csBin, nil
		}
	}
	if err := os.MkdirAll(p.cacheDir, 0o755); err != nil {
		stage := failedStage("tool", "Build go2cs", err.Error())
		return target, &stage
	}
	// This is the first conversion of this server process, so the cached executable is one an
	// earlier process left behind. What must never happen is a converter built from an older
	// checkout being driven by a newer pipeline; rebuilding unconditionally certainly prevents
	// that, but it also charges every restart a full converter build (~2 minutes on the machine
	// the budget above was measured on) to re-derive a binary that is already current. The
	// freshness test the behavioral harnesses apply to go2cs.exe answers the same question
	// directly -- newer than every converter source -- and the probe additionally requires the
	// file to identify itself, so nothing is trusted on its path alone.
	if converterIsCurrent(ctx, target, p.converterSourceDir()) {
		p.go2csBin = target
		return target, nil
	}
	// Discard the rejected executable before invoking `go build`: Go may refuse to overwrite a
	// stale or invalid Windows executable.
	if err := os.Remove(target); err != nil && !os.IsNotExist(err) {
		stage := failedStage("tool", "Build go2cs", fmt.Sprintf("remove stale converter %q: %v", target, err))
		return target, &stage
	}

	stage := p.runStage(ctx, "tool", "Build go2cs", p.converterSourceDir(),
		p.buildToolTimeout(), "go", "build", "-o", target, ".")
	if stage.Status == "passed" {
		p.go2csBin = target
	}
	return target, &stage
}

func (p *pipelineRunner) converterSourceDir() string {
	return filepath.Join(p.repoRoot, "src", "go2cs")
}

// buildToolTimeout reports the resolved converter-build budget, defaulting an unset one rather
// than letting a zero value through: a zero budget expires before the child starts, which would
// report every host as too slow to build the converter at all.
func (p *pipelineRunner) buildToolTimeout() time.Duration {
	if p.toolTimeout > 0 {
		return p.toolTimeout
	}
	return defaultToolTimeout
}

// converterIsCurrent reports whether the cached executable can stand in for a fresh build: newer
// than every converter source, and able to say what it is when run.
//
// The two halves answer different questions and both are load-bearing. The timestamp answers "was
// this built from the checkout the pipeline is about to drive?" -- a checkout that moves rewrites
// its source timestamps, so a converter predating the move is rejected. The probe answers "is this
// a converter at all?", which no timestamp can: a partially written build, or a file another tool
// parked at the cache path, is newer than everything and runs nothing.
func converterIsCurrent(ctx context.Context, target, sourceDir string) bool {
	return converterIsFresh(target, sourceDir) && converterIdentifies(ctx, target)
}

// converterIsFresh reports whether target is newer than every file the converter is built from.
// go.mod and go.sum count alongside the sources, since a dependency change alters the binary while
// leaving every .go file untouched. An unreadable source tree reports false: the build that
// follows will produce a far better diagnostic than a reuse decision could.
func converterIsFresh(target, sourceDir string) bool {
	binary, err := os.Stat(target)
	if err != nil {
		return false
	}

	newest, err := newestConverterSource(sourceDir)
	if err != nil {
		return false
	}
	return binary.ModTime().After(newest)
}

func newestConverterSource(sourceDir string) (time.Time, error) {
	var newest time.Time
	found := false

	err := filepath.WalkDir(sourceDir, func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if entry.IsDir() {
			return nil
		}
		switch strings.ToLower(filepath.Ext(entry.Name())) {
		case ".go", ".mod", ".sum":
		default:
			return nil
		}
		info, err := entry.Info()
		if err != nil {
			return err
		}
		if modified := info.ModTime(); modified.After(newest) {
			newest = modified
		}
		found = true
		return nil
	})
	if err != nil {
		return time.Time{}, err
	}
	if !found {
		return time.Time{}, fmt.Errorf("no converter sources under %s", sourceDir)
	}
	return newest, nil
}

// converterIdentifies runs the cached executable with no arguments, where go2cs prints its usage
// banner and exits 1 -- so the banner, not the exit status, is what identifies it. A file that
// merely occupies the cache path prints nothing, and neither does a text file with the executable
// bit set, which is what the integration test plants to prove the cache is not trusted on its
// filename alone.
func converterIdentifies(ctx context.Context, target string) bool {
	ctx, cancel := context.WithTimeout(ctx, converterProbeTimeout)
	defer cancel()

	command := newCommand(ctx, target)
	hideCommandWindow(command)
	var banner strings.Builder
	command.Stdout = &banner
	command.Stderr = &banner
	_ = command.Run()

	return strings.Contains(banner.String(), "Usage: go2cs")
}

// resolveToolTimeout reports the converter-build budget for a supplied option, falling back to the
// environment and then to the default. A malformed FLAG is a usage error the caller reports,
// because it was typed deliberately; a malformed VARIABLE only warns and falls back, because it
// may have been inherited from a shell the operator never set up.
func resolveToolTimeout(supplied time.Duration) time.Duration {
	if supplied > 0 {
		return supplied
	}

	setting := strings.TrimSpace(os.Getenv(toolTimeoutEnv))
	if setting == "" {
		return defaultToolTimeout
	}

	timeout, err := parseTimeoutSetting(setting)
	if err != nil {
		log.Printf("ignoring %s: %v; using %s", toolTimeoutEnv, err, defaultToolTimeout)
		return defaultToolTimeout
	}
	return timeout
}

// parseTimeoutSetting reads a budget written either in Go duration syntax ("90s", "15m") or as a
// bare whole number of SECONDS, the unit every sibling GO2CS_*_TIMEOUT variable is written in. An
// empty value means "not set" and reports zero so the caller can fall through to the next source;
// anything else that does not name a positive duration is an error rather than a silent zero,
// since a zero budget expires before its child starts and would report a healthy host as a slow one.
func parseTimeoutSetting(value string) (time.Duration, error) {
	value = strings.TrimSpace(value)
	if value == "" {
		return 0, nil
	}

	timeout, err := time.ParseDuration(value)
	if err != nil {
		seconds, secondsErr := strconv.Atoi(value)
		if secondsErr != nil {
			return 0, fmt.Errorf("%q is neither a duration (10m) nor a whole number of seconds (600)", value)
		}
		timeout = time.Duration(seconds) * time.Second
	}
	if timeout <= 0 {
		return 0, fmt.Errorf("%q is not a positive duration", value)
	}
	// A bare number this large is far likelier to be milliseconds, copied from a harness that
	// stores its budgets that way, than a genuine multi-day budget for a two-minute build.
	if timeout > 24*time.Hour {
		return 0, fmt.Errorf("%q exceeds a day; write seconds (600) or a duration (10m), not milliseconds", value)
	}
	return timeout, nil
}

func formatTranspileTranscript(resolveOutput, output, root, status, runtime string) string {
	lines := []string{"$ go mod tidy"}
	if resolveOutput = strings.TrimSpace(resolveOutput); resolveOutput != "" {
		lines = append(lines, resolveOutput)
	}
	lines = append(lines, "$ go2cs -recurse main.go", "Runtime: "+runtime)
	if output = strings.TrimSpace(output); output != "" {
		lines = append(lines, output)
	}
	if files, err := generatedArtifactNames(root); err == nil && len(files) > 0 {
		lines = append(lines, "Generated files:")
		for _, file := range files {
			lines = append(lines, "  "+file)
		}
	}
	switch status {
	case "passed":
		lines = append(lines, "Transpile completed.")
	case "killed":
		lines = append(lines, "Transpile killed.")
	default:
		lines = append(lines, "Transpile failed.")
	}
	return strings.Join(lines, "\n")
}

// formatConverterBuildTranscript reports the one-time converter build in its own words, so a build
// that never finished cannot be read as a conversion that went wrong. A timed-out build says
// plainly that the converter never ran -- nothing about the submitted Go was measured -- and names
// the two ways out, since neither the budget nor GO2CS_BIN is discoverable from the transcript.
func formatConverterBuildTranscript(resolveOutput, output, target, status string, timedOut bool) string {
	lines := []string{"$ go mod tidy"}
	if resolveOutput = strings.TrimSpace(resolveOutput); resolveOutput != "" {
		lines = append(lines, resolveOutput)
	}
	lines = append(lines, "$ go build -o "+target+" ./src/go2cs")
	if output = strings.TrimSpace(output); output != "" {
		lines = append(lines, output)
	}
	switch {
	case timedOut:
		lines = append(lines,
			"Building go2cs ran out of time, so the converter never ran and nothing was converted.",
			"Raise the budget with -tool-timeout (or "+toolTimeoutEnv+"), or set GO2CS_BIN to a prebuilt converter.")
	case status == "killed":
		lines = append(lines, "Building go2cs killed.")
	default:
		lines = append(lines, "Building go2cs failed.")
	}
	return strings.Join(lines, "\n")
}

func generatedArtifactNames(root string) ([]string, error) {
	var files []string
	err := filepath.WalkDir(root, func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if entry.IsDir() {
			switch entry.Name() {
			case "bin", "obj", "Generated":
				return filepath.SkipDir
			}
			return nil
		}
		extension := strings.ToLower(filepath.Ext(entry.Name()))
		if extension != ".cs" && extension != ".csproj" {
			return nil
		}
		relative, err := filepath.Rel(root, path)
		if err != nil {
			return err
		}
		files = append(files, filepath.ToSlash(relative))
		return nil
	})
	sort.Strings(files)
	return files, err
}

func appendOutputLine(output, line string) string {
	output = strings.TrimSpace(output)
	if output == "" {
		return line
	}
	return output + "\n" + line
}

func programExitMessage(parent, command context.Context, err error) string {
	switch {
	case err == nil:
		return "Program exited."
	case errors.Is(parent.Err(), context.Canceled):
		return "Program exited: killed"
	case errors.Is(command.Err(), context.DeadlineExceeded):
		return "Program exited: process took too long."
	}

	var exitError *exec.ExitError
	if errors.As(err, &exitError) && exitError.ExitCode() >= 0 {
		return fmt.Sprintf("Program exited: status %d.", exitError.ExitCode())
	}
	return "Program exited: " + strings.TrimSuffix(strings.TrimSpace(err.Error()), ".") + "."
}
func (p *pipelineRunner) runStage(parent context.Context, id, label, dir string, timeout time.Duration, name string, args ...string) stageResult {
	started := time.Now()
	ctx, cancel := context.WithTimeout(parent, timeout)
	defer cancel()

	command := newCommand(ctx, name, args...)
	command.Dir = dir
	hideCommandWindow(command)
	// Keep the two streams apart rather than merging them into one buffer: the
	// interface colors standard error the way the Tour does.
	var stream outputStream
	command.Stdout = stream.writer(outputStdout)
	command.Stderr = stream.writer(outputStderr)
	err := command.Run()

	segments := stream.collect()
	for index := range segments {
		segments[index].Text = cleanDisplayPath(segments[index].Text, p.repoRoot)
	}
	// A tool transcript is tidied, but a program's output is handed on exactly as
	// it was written: its own trailing newline is what decides whether a blank
	// line precedes the exit notice, as it does in the Tour.
	if id != "run" {
		segments = trimSegments(segments)
	}
	status := "passed"
	timedOut := false
	if err != nil {
		status = "failed"
		switch {
		case errors.Is(parent.Err(), context.Canceled):
			status = "killed"
			if id != "run" {
				segments = appendSystemSegment(segments, "Killed.")
			}
		case errors.Is(ctx.Err(), context.DeadlineExceeded):
			timedOut = true
			if id != "run" {
				segments = appendSystemSegment(segments, fmt.Sprintf("Timed out after %s.", timeout))
			}
		case id != "run" && len(segments) == 0:
			segments = appendSystemSegment(segments, err.Error())
		}
	}
	if id == "run" {
		segments = appendSystemSegment(segments, programExitMessage(parent, ctx, err))
	}

	return stageResult{
		ID:         id,
		Label:      label,
		Status:     status,
		TimedOut:   timedOut,
		Output:     joinSegments(segments),
		Segments:   segments,
		DurationMS: time.Since(started).Milliseconds(),
	}
}

func (p *pipelineRunner) cleanupExpired() {
	now := time.Now()
	var remove []*conversionArtifact

	p.mu.Lock()
	for id, artifact := range p.saved {
		if now.Sub(artifact.createdAt) > conversionMaxAge {
			remove = append(remove, artifact)
			delete(p.saved, id)
		}
	}
	if len(p.saved) > maxSavedArtifacts {
		artifacts := make([]*conversionArtifact, 0, len(p.saved))
		for _, artifact := range p.saved {
			artifacts = append(artifacts, artifact)
		}
		sort.Slice(artifacts, func(i, j int) bool {
			return artifacts[i].createdAt.Before(artifacts[j].createdAt)
		})
		for len(p.saved) > maxSavedArtifacts {
			artifact := artifacts[0]
			artifacts = artifacts[1:]
			remove = append(remove, artifact)
			delete(p.saved, artifact.id)
		}
	}
	p.mu.Unlock()

	for _, artifact := range remove {
		_ = os.RemoveAll(artifact.workDir)
	}
}

// discard forgets one saved conversion and removes its workspace.
func (p *pipelineRunner) discard(id string) {
	p.mu.Lock()
	artifact := p.saved[id]
	delete(p.saved, id)
	p.mu.Unlock()
	if artifact != nil {
		_ = os.RemoveAll(artifact.workDir)
	}
}

func (p *pipelineRunner) close() {
	p.mu.Lock()
	artifacts := make([]*conversionArtifact, 0, len(p.saved))
	for _, artifact := range p.saved {
		artifacts = append(artifacts, artifact)
	}
	p.saved = make(map[string]*conversionArtifact)
	p.mu.Unlock()
	for _, artifact := range artifacts {
		_ = os.RemoveAll(artifact.workDir)
	}
}

func newConversionID() (string, error) {
	value := make([]byte, 16)
	if _, err := rand.Read(value); err != nil {
		return "", err
	}
	return hex.EncodeToString(value), nil
}

func skippedStage(id, label string) stageResult {
	return stageResult{ID: id, Label: label, Status: "skipped", Output: "Skipped because an earlier stage failed."}
}

func failedStage(id, label, output string) stageResult {
	return stageResult{ID: id, Label: label, Status: "failed", Output: output}
}

func collectCSharp(root string) (string, error) {
	if _, err := os.Stat(root); err != nil {
		if os.IsNotExist(err) {
			return "", errors.New("go2cs did not emit an output project for the submitted app")
		}
		return "", err
	}

	var sourceFiles []string
	err := filepath.WalkDir(root, func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if entry.IsDir() {
			if entry.Name() == "obj" || entry.Name() == "bin" || entry.Name() == "Generated" {
				return filepath.SkipDir
			}
			return nil
		}
		if !strings.HasSuffix(strings.ToLower(entry.Name()), ".cs") {
			return nil
		}
		name := strings.ToLower(entry.Name())
		if name != "package_info.cs" && name != "go2cs_test_host.cs" {
			sourceFiles = append(sourceFiles, path)
		}
		return nil
	})
	if err != nil {
		return "", err
	}
	if len(sourceFiles) == 0 {
		return "", errors.New("go2cs did not emit C# source for the submitted app")
	}
	sort.Strings(sourceFiles)

	var result strings.Builder
	for _, file := range sourceFiles {
		content, err := os.ReadFile(file)
		if err != nil {
			return "", err
		}
		if result.Len() > 0 {
			relative, _ := filepath.Rel(root, file)
			result.WriteString("\n\n// -- ")
			result.WriteString(filepath.ToSlash(relative))
			result.WriteString(" --\n")
		}
		result.Write(content)
	}
	return result.String(), nil
}

func readGeneratedFile(root, name string) (string, error) {
	content, err := os.ReadFile(filepath.Join(root, name))
	if errors.Is(err, os.ErrNotExist) {
		return "", nil
	}
	if err != nil {
		return "", err
	}
	return string(content), nil
}

type conversionNotFoundError string

func (e conversionNotFoundError) Error() string { return string(e) }
