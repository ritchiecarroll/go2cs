package main

import (
	"context"
	"os"
	"sync"
	"time"
)

// closeWaitLimit bounds how long close waits for cancelled builds to exit before it removes their
// workspaces anyway. A cancelled build is killed, so this is a hang guard, not an expected wait.
const closeWaitLimit = 10 * time.Second

// prebuild is the .NET build a conversion made for the page starts in the background, so that a
// Run arriving moments later finds its build finished or under way rather than starting one.
//
// result and ok are written once, before done is closed, and read only after it is. ok is false
// when the build was cancelled before it produced a result; the waiter then builds for itself,
// as it would have with no prebuild at all.
type prebuild struct {
	cancel context.CancelFunc
	done   chan struct{}
	result stageResult
	ok     bool
}

// startPrebuild starts the background build of a saved conversion, at most once per conversion,
// and reports whether it did.
//
// Starting one also cancels any earlier conversion's build that is still waiting or running. The
// page builds the conversion it made last; every Tour page converts itself on arrival, so a visitor
// paging through lessons would otherwise queue a build per page, each holding a pipeline slot the
// current page's conversion needs. A cancelled build costs nothing it was going to deliver: a Run
// of that earlier conversion, should one still come, builds as it did before prebuilds existed.
func (p *pipelineRunner) startPrebuild(id string) bool {
	p.mu.Lock()
	artifact := p.saved[id]
	if artifact == nil || artifact.prebuild != nil {
		p.mu.Unlock()
		return false
	}
	for _, other := range p.saved {
		if other.prebuild != nil {
			other.prebuild.cancel()
		}
	}
	ctx, cancel := context.WithCancel(p.life)
	build := &prebuild{cancel: cancel, done: make(chan struct{})}
	artifact.prebuild = build
	p.mu.Unlock()

	go func() {
		defer close(build.done)
		defer cancel()
		build.result, build.ok = p.runPrebuild(ctx, artifact)
	}()
	return true
}

// runPrebuild builds the way a requested build does: after any running warmup, inside a pipeline
// slot. A build its cancellation killed is no result: it reports ok false.
func (p *pipelineRunner) runPrebuild(ctx context.Context, artifact *conversionArtifact) (stageResult, bool) {
	if err := p.waitForWarmup(ctx); err != nil {
		return stageResult{}, false
	}
	if err := p.acquire(ctx); err != nil {
		return stageResult{}, false
	}
	defer p.release()
	// acquire may take a free slot even when the context ended at the same moment.
	if ctx.Err() != nil {
		return stageResult{}, false
	}

	stage := p.buildStage(ctx, artifact)
	return stage, stage.Status != "killed"
}

// awaitPrebuild waits for the background build of a saved conversion and reports its result. ok is
// false when the conversion has no prebuild, or its prebuild was cancelled; the caller then builds
// for itself. It holds no pipeline slot while it waits, since the build it waits for needs one.
func (p *pipelineRunner) awaitPrebuild(ctx context.Context, id string) (stageResult, bool, error) {
	p.cleanupExpired()

	p.mu.Lock()
	var build *prebuild
	if artifact := p.saved[id]; artifact != nil {
		build = artifact.prebuild
	}
	p.mu.Unlock()
	if build == nil {
		return stageResult{}, false, nil
	}

	select {
	case <-build.done:
		return build.result, build.ok, nil
	case <-ctx.Done():
		return stageResult{}, false, ctx.Err()
	}
}

// retiredWorkspace is a conversion that has been forgotten but whose workspace is still on disk.
type retiredWorkspace struct {
	workDir string
	build   *prebuild
}

// retire records a forgotten conversion for removeWorkspaces. The caller holds p.mu, which guards
// the artifact's prebuild.
func retire(artifact *conversionArtifact) retiredWorkspace {
	return retiredWorkspace{workDir: artifact.workDir, build: artifact.prebuild}
}

// removeWorkspaces cancels each retired conversion's background build and deletes its workspace
// once that build has stopped: a killed dotnet build holds files in the tree until it exits, and on
// Windows a held file cannot be deleted. Behind a running build the removal happens in the
// background, so a request never waits on a process exit; close passes wait, so the workspaces are
// gone (or close has waited closeWaitLimit) before the server exits.
func removeWorkspaces(retired []retiredWorkspace, wait bool) {
	var pending sync.WaitGroup
	for _, workspace := range retired {
		if workspace.build == nil {
			_ = os.RemoveAll(workspace.workDir)
			continue
		}
		workspace.build.cancel()
		pending.Add(1)
		go func() {
			defer pending.Done()
			<-workspace.build.done
			_ = os.RemoveAll(workspace.workDir)
		}()
	}
	if !wait {
		return
	}

	finished := make(chan struct{})
	go func() {
		pending.Wait()
		close(finished)
	}()
	select {
	case <-finished:
	case <-time.After(closeWaitLimit):
	}
}
