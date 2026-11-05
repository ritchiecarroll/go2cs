// stallWatch_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"errors"
	"os"
	"path/filepath"
	"reflect"
	"strings"
	"sync"
	"testing"
	"time"
)

// The stall watch (docs/phase4/DESIGN-conversion-stall-detection.md, ruled by COORD 2026-11-05): a package that
// is still converting at GO2CS_STALL_WARN (default 2m) gets a warning on stderr, repeated at 4m, 8m, 16m and then
// every 16m, and at GO2CS_STALL_DUMP (default 5m) every goroutine's stack goes to a temp-dir file named on
// stderr. It only ever warns: the conversion's result, error and panic all pass through untouched.

// lockedBuffer is a bytes.Buffer the timer goroutines and the test can share.
type lockedBuffer struct {
	mu  sync.Mutex
	buf bytes.Buffer
}

func (b *lockedBuffer) Write(p []byte) (int, error) {
	b.mu.Lock()
	defer b.mu.Unlock()

	return b.buf.Write(p)
}

func (b *lockedBuffer) String() string {
	b.mu.Lock()
	defer b.mu.Unlock()

	return b.buf.String()
}

func TestStallWatchScheduleDoublesThenRepeatsAtTheCap(t *testing.T) {
	var got []time.Duration

	at := defaultStallWarn

	for range 7 {
		got = append(got, at)
		at = nextStallWarning(at, stallRepeatCap)
	}

	want := []time.Duration{2 * time.Minute, 4 * time.Minute, 8 * time.Minute, 16 * time.Minute, 32 * time.Minute, 48 * time.Minute, 64 * time.Minute}

	if !reflect.DeepEqual(got, want) {
		t.Fatalf("warning times: got %v, want %v", got, want)
	}
}

func TestStallWatchWarnsWhileAConversionRuns(t *testing.T) {
	out := &lockedBuffer{}
	config := stallWatchConfig{warn: 10 * time.Millisecond, repeatCap: 10 * time.Millisecond, out: out}

	err := watchForStall(config, "example.com/slow", func() error {
		time.Sleep(150 * time.Millisecond)
		return nil
	})

	if err != nil {
		t.Fatalf("a slow conversion's nil result came back as %v", err)
	}

	text := out.String()

	if n := strings.Count(text, "WARNING: example.com/slow has been converting for "); n < 2 {
		t.Fatalf("want the warning at least twice over a 150ms conversion with a 10ms threshold, got %d:\n%s", n, text)
	}

	for _, must := range []string{"has NOT been interrupted", "https://github.com/ritchiecarroll/go2cs/issues", PprofAddressEnv + "="} {
		if !strings.Contains(text, must) {
			t.Errorf("the warning does not say %q:\n%s", must, text)
		}
	}
}

func TestStallWatchPassesTheResultThrough(t *testing.T) {
	sentinel := errors.New("the package failed on its own")
	config := stallWatchConfig{warn: time.Millisecond, repeatCap: time.Millisecond, out: &lockedBuffer{}}

	if err := watchForStall(config, "example.com/failing", func() error {
		time.Sleep(10 * time.Millisecond)
		return sentinel
	}); err != sentinel {
		t.Fatalf("got %v, want the conversion's own error", err)
	}

	if err := watchForStall(config, "example.com/fine", func() error { return nil }); err != nil {
		t.Fatalf("got %v, want nil", err)
	}
}

func TestStallWatchFastConversionIsSilent(t *testing.T) {
	out := &lockedBuffer{}
	config := stallWatchConfig{warn: 50 * time.Millisecond, dump: 50 * time.Millisecond, repeatCap: 50 * time.Millisecond, out: out, dumpDir: t.TempDir()}

	ran := false

	if err := watchForStall(config, "example.com/fast", func() error { ran = true; return nil }); err != nil || !ran {
		t.Fatalf("ran=%v err=%v, want the conversion run once with a nil result", ran, err)
	}

	// Past both thresholds: a disarmed watch must not speak after its package returned.
	time.Sleep(150 * time.Millisecond)

	if text := out.String(); text != "" {
		t.Fatalf("a fast conversion produced output:\n%s", text)
	}

	if dumps, _ := filepath.Glob(filepath.Join(config.dumpDir, "go2cs-stall-*")); len(dumps) != 0 {
		t.Fatalf("a fast conversion wrote %v", dumps)
	}
}

func TestStallWatchDisarmsWhenTheConversionPanics(t *testing.T) {
	out := &lockedBuffer{}
	config := stallWatchConfig{warn: 30 * time.Millisecond, repeatCap: 30 * time.Millisecond, out: out}

	recovered := func() (r any) {
		defer func() { r = recover() }()

		_ = watchForStall(config, "example.com/panicking", func() error { panic("converter fault") })

		return nil
	}()

	if recovered != "converter fault" {
		t.Fatalf("the caller's recover saw %v, want the conversion's own panic value", recovered)
	}

	time.Sleep(120 * time.Millisecond)

	if text := out.String(); text != "" {
		t.Fatalf("a panicked conversion left its watch armed:\n%s", text)
	}
}

func TestStallWatchDumpsEveryStackToAFileOnce(t *testing.T) {
	out := &lockedBuffer{}
	config := stallWatchConfig{dump: 10 * time.Millisecond, out: out, dumpDir: t.TempDir()}

	if err := watchForStall(config, "example.com/stuck", func() error {
		time.Sleep(120 * time.Millisecond)
		return nil
	}); err != nil {
		t.Fatalf("got %v, want nil", err)
	}

	dumps, _ := filepath.Glob(filepath.Join(config.dumpDir, "go2cs-stall-*.txt"))

	if len(dumps) != 1 {
		t.Fatalf("want exactly one dump file, got %v; output:\n%s", dumps, out.String())
	}

	content, err := os.ReadFile(dumps[0])

	if err != nil {
		t.Fatal(err)
	}

	for _, must := range []string{"example.com/stuck", "goroutine ", "TestStallWatchDumpsEveryStackToAFileOnce"} {
		if !strings.Contains(string(content), must) {
			t.Errorf("the dump does not hold %q", must)
		}
	}

	text := out.String()

	if !strings.Contains(text, dumps[0]) || !strings.Contains(text, "has NOT been interrupted") {
		t.Errorf("stderr does not name the dump file and say the conversion continues:\n%s", text)
	}
}

func TestStallWatchThresholdsFromTheEnvironment(t *testing.T) {
	cases := []struct {
		name       string
		warn, dump string
		wantWarn   time.Duration
		wantDump   time.Duration
		complaint  string
	}{
		{"unset takes the ruled defaults", "", "", 2 * time.Minute, 5 * time.Minute, ""},
		{"Go durations", "90s", "1h", 90 * time.Second, time.Hour, ""},
		{"0 turns each off", "0", "0", 0, 0, ""},
		{"a word is refused by name", "soon", "", 2 * time.Minute, 5 * time.Minute, stallWarnEnv},
		{"a negative duration is refused by name", "", "-1m", 2 * time.Minute, 5 * time.Minute, stallDumpEnv},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			out := &lockedBuffer{}
			env := map[string]string{stallWarnEnv: c.warn, stallDumpEnv: c.dump}
			config := stallWatchConfigFrom(func(name string) string { return env[name] }, out)

			if config.warn != c.wantWarn || config.dump != c.wantDump || config.repeatCap != stallRepeatCap {
				t.Fatalf("warn=%v dump=%v cap=%v, want %v %v %v", config.warn, config.dump, config.repeatCap, c.wantWarn, c.wantDump, stallRepeatCap)
			}

			text := out.String()

			if c.complaint == "" && text != "" {
				t.Fatalf("unexpected complaint: %s", text)
			}

			if c.complaint != "" && !strings.Contains(text, c.complaint) {
				t.Fatalf("want a complaint naming %s, got %q", c.complaint, text)
			}
		})
	}
}

// TestStallWatchCoversTheBatchDrivers runs each of the three ruled loops over one package that cannot convert,
// with the watch replaced by a recorder that does not run the conversion: -recurse's convertAll, -stdlib's
// convertAllPackages, and the conversion step of -tests -recurse phase B. A single-package conversion is not
// watched (ruled scope), so main's own path is not in this list.
func TestStallWatchCoversTheBatchDrivers(t *testing.T) {
	var labels []string

	saved := stallWatch
	stallWatch = func(label string, convert func() error) error {
		labels = append(labels, label)
		return errors.New("recorded, not converted")
	}

	defer func() { stallWatch = saved }()

	root := t.TempDir()
	missing := filepath.Join(root, "no-such-package")

	graph := func() *DependencyGraph {
		g := NewDependencyGraph()
		g.AddPackage("example.com/one", missing)
		g.sortedQueue = []string{"example.com/one"}
		return g
	}

	options := Options{go2csPath: root, targetPlatform: "linux/amd64"}
	module := NewModuleConverter(options)
	module.graph = graph()
	module.convertAll()

	stdlib := &StdLibConverter{go2csPath: root, options: options, graph: graph(), totalCount: 1, startTime: time.Now()}

	if err := stdlib.convertAllPackages(); err != nil {
		t.Fatalf("convertAllPackages: %v", err)
	}

	if err := convertAndActOnPackageTests(root, filepath.Join(root, "out"), Options{testAction: "convert", targetPlatform: "linux/amd64"}, "example.com/tested"); err == nil {
		t.Fatalf("phase B's conversion step did not return the recorder's error")
	}

	want := []string{"example.com/one", "example.com/one", "example.com/tested"}

	if !reflect.DeepEqual(labels, want) {
		t.Fatalf("watched packages: got %v, want %v (-recurse, -stdlib, -tests -recurse phase B)", labels, want)
	}
}
