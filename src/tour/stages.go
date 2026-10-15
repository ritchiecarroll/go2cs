package main

import (
	"context"
	"net/http"
)

func (p *pipelineRunner) build(ctx context.Context, request runRequest) (stageResult, error) {
	if request.ConversionID == "" {
		return stageResult{}, validationError("conversionId is required")
	}
	if err := p.waitForWarmup(ctx); err != nil {
		return stageResult{}, err
	}
	artifact, err := p.savedArtifact(ctx, request.ConversionID)
	if err != nil {
		return stageResult{}, err
	}
	defer p.release()

	args := []string{"build", artifact.project, "--nologo", "--verbosity:minimal"}
	args = append(args, runtimeMSBuildArgs(artifact.runtime)...)
	return p.runStage(ctx, "build", "Build", artifact.outputDir, buildTimeout, "dotnet", args...), nil
}

func (p *pipelineRunner) execute(ctx context.Context, request runRequest) (stageResult, error) {
	artifact, err := p.savedArtifact(ctx, request.ConversionID)
	if err != nil {
		return stageResult{}, err
	}
	defer p.release()

	args := []string{"run", "--no-build", "--project", artifact.project}
	args = append(args, runtimeMSBuildArgs(artifact.runtime)...)
	return p.runStage(ctx, "run", ".NET Run", artifact.outputDir, commandTimeout, "dotnet", args...), nil
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
