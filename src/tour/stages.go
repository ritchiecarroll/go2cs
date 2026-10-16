package main

import (
	"context"
	"net/http"
	"os"
	"path/filepath"
	"regexp"
	"strings"
)

func (p *pipelineRunner) build(ctx context.Context, request runRequest) (stageResult, error) {
	if request.ConversionID == "" {
		return stageResult{}, validationError("conversionId is required")
	}
	if err := p.waitForWarmup(ctx); err != nil {
		return stageResult{}, err
	}
	// A conversion made for the page started its build the moment it was saved; that build is
	// the answer, finished or not. Only a conversion with no such build, or one whose build was
	// cancelled, is built here.
	if result, ok, err := p.awaitPrebuild(ctx, request.ConversionID); err != nil || ok {
		return result, err
	}
	artifact, err := p.savedArtifact(ctx, request.ConversionID)
	if err != nil {
		return stageResult{}, err
	}
	defer p.release()

	return p.buildStage(ctx, artifact), nil
}

// buildStage runs the .NET build of a saved conversion and records the program it produced, for
// programStage to launch. The caller holds a pipeline slot.
func (p *pipelineRunner) buildStage(ctx context.Context, artifact *conversionArtifact) stageResult {
	args := []string{"build", artifact.project, "--nologo", "--verbosity:minimal"}
	args = append(args, runtimeMSBuildArgs(artifact.runtime)...)
	stage := p.runStage(ctx, "build", "Build", artifact.outputDir, buildTimeout, "dotnet", args...)

	assembly := ""
	if stage.Status == "passed" {
		assembly = builtAssembly(stage.Output, artifact.project, p.repoRoot)
	}
	p.mu.Lock()
	artifact.assembly = assembly
	p.mu.Unlock()
	return stage
}

func (p *pipelineRunner) execute(ctx context.Context, request runRequest) (stageResult, error) {
	artifact, err := p.savedArtifact(ctx, request.ConversionID)
	if err != nil {
		return stageResult{}, err
	}
	defer p.release()

	return p.programStage(ctx, artifact), nil
}

// programStage runs a built conversion's program. The caller holds a pipeline slot.
//
// It launches the assembly the build named, `dotnet <assembly>`, rather than `dotnet run --no-build
// --project`: dotnet run evaluates the project again before every launch only to find the program
// the build already reported, and that evaluation is more than half a short lesson's run. Nothing the
// program sees changes. Its runtimeconfig.json and deps.json sit beside the assembly, and the
// runtime's MSBuild properties (go2csPath, GoStdLibVersion) are build inputs, already compiled in.
// The working directory, environment, budget and output capture are the stage's either way. A
// conversion whose build named no assembly still runs the old way.
func (p *pipelineRunner) programStage(ctx context.Context, artifact *conversionArtifact) stageResult {
	p.mu.Lock()
	assembly := artifact.assembly
	p.mu.Unlock()
	if assembly != "" {
		return p.runStage(ctx, "run", ".NET Run", artifact.outputDir, commandTimeout, "dotnet", assembly)
	}

	args := []string{"run", "--no-build", "--project", artifact.project}
	args = append(args, runtimeMSBuildArgs(artifact.runtime)...)
	return p.runStage(ctx, "run", ".NET Run", artifact.outputDir, commandTimeout, "dotnet", args...)
}

// buildOutputPattern matches the line MSBuild's console logger prints, at minimal verbosity, for each
// project it builds: "  <project name> -> <full path of its output assembly>".
var buildOutputPattern = regexp.MustCompile(`(?m)^[ \t]*(.+?) -> (.+\.dll)[ \t\r]*$`)

// builtAssembly reports the program a passed build of project produced, read from the build's
// output, or "" when that output names none that can be launched. Only the project's own line counts
// (a lesson's dependencies print theirs too), and only an assembly whose runtimeconfig.json sits
// beside it is a program `dotnet <assembly>` can start. The output is the displayed transcript,
// which spells the repository root as <repo>, so that spelling is undone first.
func builtAssembly(output, project, repoRoot string) string {
	name := strings.TrimSuffix(filepath.Base(project), filepath.Ext(project))
	for _, match := range buildOutputPattern.FindAllStringSubmatch(output, -1) {
		if strings.TrimSpace(match[1]) != name {
			continue
		}
		assembly := strings.TrimSpace(match[2])
		if repoRoot != "" {
			assembly = strings.ReplaceAll(assembly, "<repo>", repoRoot)
		}
		if !filepath.IsAbs(assembly) {
			continue
		}
		runtimeConfig := strings.TrimSuffix(assembly, filepath.Ext(assembly)) + ".runtimeconfig.json"
		if isFile(assembly) && isFile(runtimeConfig) {
			return filepath.Clean(assembly)
		}
	}
	return ""
}

func isFile(path string) bool {
	info, err := os.Stat(path)
	return err == nil && !info.IsDir()
}

func (p *pipelineRunner) savedArtifact(ctx context.Context, id string) (*conversionArtifact, error) {
	if id == "" {
		return nil, validationError("conversionId is required")
	}
	if err := p.acquire(ctx); err != nil {
		return nil, err
	}
	p.cleanupExpired()

	p.mu.Lock()
	artifact := p.saved[id]
	p.mu.Unlock()
	if artifact == nil {
		p.release()
		return nil, conversionNotFoundError("that conversion has expired; convert the current Go source again")
	}
	return artifact, nil
}

func (s *server) handleBuild(w http.ResponseWriter, r *http.Request) {
	s.handleDotNetStage(w, r, s.pipeline.build)
}

func (s *server) handleExecute(w http.ResponseWriter, r *http.Request) {
	s.handleDotNetStage(w, r, s.pipeline.execute)
}

func (s *server) handleDotNetStage(
	w http.ResponseWriter,
	r *http.Request,
	action func(context.Context, runRequest) (stageResult, error),
) {
	r.Body = http.MaxBytesReader(w, r.Body, 16<<10)
	var request runRequest
	if err := decodeJSON(r, &request); err != nil {
		writeJSON(w, http.StatusBadRequest, apiError{Error: "invalid request: " + err.Error()})
		return
	}
	result, err := action(r.Context(), request)
	if err != nil {
		s.writePipelineError(w, err)
		return
	}
	writeJSON(w, http.StatusOK, result)
}
