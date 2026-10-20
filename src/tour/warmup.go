package main

import (
	"context"
	"fmt"
	"log"
	"strings"
	"sync"
	"time"
)

// The first lesson after a start pays for everything the toolchains do once: building the
// converter, restoring the go.* packages into the NuGet cache, and starting MSBuild and the C#
// compiler. The warmup pays it instead, at server start, by sending a small fixed program through
// the same convert -> build -> run path a lesson takes. The sample is the Tour's own welcome page,
// so the packages it restores are the ones the page a visitor sees first needs.
const warmupSource = `package main

import "fmt"

func main() {
	fmt.Println("Hello, 世界")
}
`

const (
	warmupIdle     = "idle"
	warmupDisabled = "disabled"
	warmupRunning  = "running"
	warmupReady    = "ready"
	warmupFailed   = "failed"
)

// warmupStatus is what /api/health reports. It carries no failure detail: a failed warmup only
// means the first lesson pays the cold cost, which is not news for the page. The log has the detail.
type warmupStatus struct {
	State      string `json:"state"`
	DurationMS int64  `json:"durationMs,omitempty"`
}

// warmup runs one piece of background work at most once and lets callers wait for it to end.
// Its states move idle -> running -> ready or failed; disabled is terminal from the start.
type warmup struct {
	mu       sync.Mutex
	state    string
	started  time.Time
	finished time.Time
	done     chan struct{}
}

func newWarmup(enabled bool) *warmup {
	w := &warmup{state: warmupIdle, done: make(chan struct{})}
	if !enabled {
		w.state = warmupDisabled
		close(w.done)
	}
	return w
}

// start runs work in the background if the warmup has never run, and reports whether it did.
// A panic in work ends the warmup failed rather than leaving its waiters blocked.
func (w *warmup) start(ctx context.Context, work func(context.Context) error) bool {
	w.mu.Lock()
	if w.state != warmupIdle {
		w.mu.Unlock()
		return false
	}
	w.state = warmupRunning
	w.started = time.Now()
	w.mu.Unlock()

	go func() {
		err := fmt.Errorf("warmup did not finish")
		defer func() {
			if recovered := recover(); recovered != nil {
				err = fmt.Errorf("warmup panicked: %v", recovered)
			}
			w.finish(err)
		}()
		err = work(ctx)
	}()
	return true
}

func (w *warmup) finish(err error) {
	w.mu.Lock()
	defer w.mu.Unlock()
	w.finished = time.Now()
	elapsed := w.finished.Sub(w.started).Round(100 * time.Millisecond)
	if err != nil {
		w.state = warmupFailed
		log.Printf(".NET warmup failed after %s; the first lesson pays the cold start instead: %v", elapsed, err)
	} else {
		w.state = warmupReady
		log.Printf(".NET warmup ready in %s", elapsed)
	}
	close(w.done)
}

// wait blocks until a running warmup ends, or the caller gives up. A warmup that never started has
// nothing to wait for.
func (w *warmup) wait(ctx context.Context) error {
	w.mu.Lock()
	idle := w.state == warmupIdle
	w.mu.Unlock()
	if idle {
		return nil
	}
	select {
	case <-w.done:
		return nil
	case <-ctx.Done():
		return ctx.Err()
	}
}

func (w *warmup) status() warmupStatus {
	w.mu.Lock()
	defer w.mu.Unlock()
	status := warmupStatus{State: w.state}
	if !w.finished.IsZero() {
		status.DurationMS = w.finished.Sub(w.started).Milliseconds()
	}
	return status
}

// startWarmup sends the sample through the default runtime's pipeline in the background, unless
// the operator turned the warmup off. It holds one pipeline slot while it works, as any request
// does, and its scratch workspace is removed when it ends.
func (p *pipelineRunner) startWarmup(ctx context.Context) bool {
	started := p.warm.start(ctx, p.warmupPipeline)
	if started {
		log.Printf("warming up .NET: converting, building and running a hello-world sample (-no-warmup skips this)")
	}
	return started
}

func (p *pipelineRunner) warmupPipeline(ctx context.Context) error {
	converted, err := p.convert(ctx, convertRequest{Source: warmupSource, Name: "warmup"})
	if err != nil {
		return err
	}
	if !converted.Successful {
		return stageError(converted.Stage)
	}
	defer p.discard(converted.ConversionID)

	// runConversion, not run: run waits for the warmup, which is this.
	result, err := p.runConversion(ctx, runRequest{ConversionID: converted.ConversionID})
	if err != nil {
		return err
	}
	for _, stage := range result.Stages {
		if stage.Status != "passed" {
			return stageError(stage)
		}
	}
	return nil
}

func stageError(stage stageResult) error {
	status := stage.Status
	if stage.TimedOut {
		status = "timed out"
	}
	output := strings.TrimSpace(stage.Output)
	if lines := strings.Split(output, "\n"); len(lines) > 12 {
		output = strings.Join(lines[len(lines)-12:], "\n")
	}
	return fmt.Errorf("%s %s:\n%s", stage.Label, status, output)
}
