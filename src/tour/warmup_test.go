package main

import (
	"context"
	"encoding/json"
	"errors"
	"net/http/httptest"
	"testing"
	"time"
)

func TestWarmupMovesFromIdleThroughRunningToReady(t *testing.T) {
	w := newWarmup(true)
	if got := w.status().State; got != warmupIdle {
		t.Fatalf("new warmup state = %q, want %q", got, warmupIdle)
	}
	// Nothing has started, so nothing is worth waiting for.
	if err := w.wait(context.Background()); err != nil {
		t.Fatalf("wait on an idle warmup: %v", err)
	}

	release := make(chan struct{})
	ran := make(chan struct{})
	if !w.start(context.Background(), func(context.Context) error {
		close(ran)
		<-release
		return nil
	}) {
		t.Fatal("start on an idle warmup reported false")
	}
	<-ran
	if got := w.status().State; got != warmupRunning {
		t.Fatalf("state while working = %q, want %q", got, warmupRunning)
	}
	if w.start(context.Background(), func(context.Context) error { t.Error("a second warmup ran"); return nil }) {
		t.Fatal("a running warmup started again")
	}

	waited := make(chan error, 1)
	go func() { waited <- w.wait(context.Background()) }()
	select {
	case err := <-waited:
		t.Fatalf("wait returned while the warmup was still running: %v", err)
	case <-time.After(50 * time.Millisecond):
	}

	close(release)
	select {
	case err := <-waited:
		if err != nil {
			t.Fatalf("wait after the warmup finished: %v", err)
		}
	case <-time.After(5 * time.Second):
		t.Fatal("wait never returned after the warmup finished")
	}
	if got := w.status().State; got != warmupReady {
		t.Fatalf("state after success = %q, want %q", got, warmupReady)
	}
}

func TestWarmupFailureAndPanicEndFailedNotStuck(t *testing.T) {
	for name, work := range map[string]func(context.Context) error{
		"error": func(context.Context) error { return errors.New("restore failed") },
		"panic": func(context.Context) error { panic("boom") },
	} {
		t.Run(name, func(t *testing.T) {
			w := newWarmup(true)
			if !w.start(context.Background(), work) {
				t.Fatal("start reported false")
			}
			ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
			defer cancel()
			if err := w.wait(ctx); err != nil {
				t.Fatalf("a failed warmup must release its waiters: %v", err)
			}
			if got := w.status().State; got != warmupFailed {
				t.Fatalf("state = %q, want %q", got, warmupFailed)
			}
		})
	}
}

func TestWarmupWaitHonorsTheCallersContext(t *testing.T) {
	w := newWarmup(true)
	release := make(chan struct{})
	defer close(release)
	w.start(context.Background(), func(context.Context) error { <-release; return nil })

	ctx, cancel := context.WithTimeout(context.Background(), 20*time.Millisecond)
	defer cancel()
	if err := w.wait(ctx); !errors.Is(err, context.DeadlineExceeded) {
		t.Fatalf("wait = %v, want the caller's deadline", err)
	}
}

func TestDisabledWarmupNeverRuns(t *testing.T) {
	w := newWarmup(false)
	if got := w.status().State; got != warmupDisabled {
		t.Fatalf("state = %q, want %q", got, warmupDisabled)
	}
	if w.start(context.Background(), func(context.Context) error { t.Error("a disabled warmup ran"); return nil }) {
		t.Fatal("a disabled warmup started")
	}
	if err := w.wait(context.Background()); err != nil {
		t.Fatalf("wait on a disabled warmup: %v", err)
	}
}

func TestNoWarmupFlag(t *testing.T) {
	defaults, err := parseServerOptions(nil)
	if err != nil {
		t.Fatal(err)
	}
	if defaults.noWarmup {
		t.Fatal("warmup is off by default; it should be on")
	}

	options, err := parseServerOptions([]string{"-no-warmup", "-no-open"})
	if err != nil {
		t.Fatal(err)
	}
	if !options.noWarmup || !options.noOpen {
		t.Fatalf("-no-warmup -no-open parsed as %+v", options)
	}

	runner := newPipelineRunner(t.TempDir(), pipelineOptions{noWarmup: options.noWarmup})
	defer runner.close()
	if runner.startWarmup(context.Background()) {
		t.Fatal("-no-warmup still started the warmup")
	}
	if got := runner.warm.status().State; got != warmupDisabled {
		t.Fatalf("warmup state under -no-warmup = %q, want %q", got, warmupDisabled)
	}
}

// A build that arrives mid-warmup waits for it -- the warmup is restoring the same packages into the
// same NuGet cache -- and proceeds as usual the moment it ends.
func TestDotNetBuildWaitsForTheWarmup(t *testing.T) {
	runner := newPipelineRunner(t.TempDir())
	defer runner.close()
	release := make(chan struct{})
	runner.warm.start(context.Background(), func(context.Context) error { <-release; return nil })

	request := runRequest{ConversionID: "unknown"}
	for name, action := range map[string]func(context.Context) error{
		"build": func(ctx context.Context) error { _, err := runner.build(ctx, request); return err },
		"run":   func(ctx context.Context) error { _, err := runner.run(ctx, request); return err },
	} {
		ctx, cancel := context.WithTimeout(context.Background(), 30*time.Millisecond)
		err := action(ctx)
		cancel()
		if !errors.Is(err, context.DeadlineExceeded) {
			t.Fatalf("%s during warmup = %v, want it to wait (and hit the caller's deadline)", name, err)
		}
	}

	close(release)
	if err := runner.warm.wait(context.Background()); err != nil {
		t.Fatal(err)
	}
	var notFound conversionNotFoundError
	if _, err := runner.build(context.Background(), request); !errors.As(err, &notFound) {
		t.Fatalf("build after warmup = %v, want the ordinary expired-conversion error", err)
	}
}

func TestHealthReportsWarmupState(t *testing.T) {
	root := t.TempDir()
	runner := newPipelineRunner(root, pipelineOptions{noWarmup: true})
	defer runner.close()
	s := &server{repoRoot: &root, pipeline: runner, tour: newTourProxy("127.0.0.1:0", root)}

	recorder := httptest.NewRecorder()
	s.handleHealth(recorder, httptest.NewRequest("GET", "/api/health", nil))
	var health struct {
		Warmup warmupStatus `json:"warmup"`
	}
	if err := json.NewDecoder(recorder.Body).Decode(&health); err != nil {
		t.Fatal(err)
	}
	if health.Warmup.State != warmupDisabled {
		t.Fatalf("health warmup = %+v, want state %q", health.Warmup, warmupDisabled)
	}
}
