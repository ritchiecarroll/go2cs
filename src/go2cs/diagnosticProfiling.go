// diagnosticProfiling.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"io"
	"net"
	"net/http"
	"os"
	"runtime"
	"strings"
	"sync"
	"time"

	// Registers the /debug/pprof handlers on http.DefaultServeMux from its init. Nothing is served
	// unless startDiagnosticProfiling actually starts a listener below.
	_ "net/http/pprof"
)

// PprofAddressEnv names the environment variable that turns on the diagnostic pprof endpoint, e.g.
//
//	GO2CS_PPROF=localhost:6060 go2cs -recurse ./myapp ./out
//
// Then, while the conversion is still running:
//
//	go tool pprof -top http://localhost:6060/debug/pprof/profile?seconds=20   # where the time goes
//	curl http://localhost:6060/debug/pprof/goroutine?debug=2                  # every goroutine's stack
const PprofAddressEnv = "GO2CS_PPROF"

// startDiagnosticProfiling starts the pprof HTTP endpoint when PprofAddressEnv is set, and does
// nothing at all otherwise.
//
// Why an endpoint and not a `-cpuprofile <file>` flag: the failure mode this exists for is a
// conversion that does not FINISH. Issue #33's chained-call exponential needed ~3^42 callee walks on
// one package, so the process never reached any on-exit profile writer — the profile had to be taken
// from a live, still-spinning process. A `-memprofile` on exit has the same blind spot. (Diagnosing
// that one without this required rebuilding the whole converter from a scratch copy of the source
// with an instrumented main, which is the cost this is here to remove. `dlv attach` is not a
// substitute: it kills its target when stdin is not a terminal.)
//
// Why an environment variable and not a flag: this is a diagnostic, not a mode of the tool. An env
// var keeps the flag surface unchanged, keeps the endpoint off by default, and can be set for one
// run without editing a script.
//
// The listener is confined to the LOOPBACK interface. A pprof endpoint exposes full goroutine stacks
// and heap contents, so an accidental `GO2CS_PPROF=:6060` on a shared machine would publish the
// converter's memory to the network; a bare port is therefore read as localhost rather than as all
// interfaces, and an explicitly non-loopback host is refused rather than silently rewritten. Forward
// a port if a remote profile is genuinely needed.
func startDiagnosticProfiling() {
	address := os.Getenv(PprofAddressEnv)

	if address == "" {
		return
	}

	host, port, err := net.SplitHostPort(address)

	if err != nil {
		fmt.Fprintf(os.Stderr, "WARNING: %s=%q is not a valid host:port address (%v) — diagnostic profiling is OFF\n",
			PprofAddressEnv, address, err)

		return
	}

	// The idiomatic Go spelling `:6060` means "every interface"; take it as loopback instead.
	if host == "" {
		host = "localhost"
	}

	if !isLoopbackHost(host) {
		fmt.Fprintf(os.Stderr, "WARNING: %s=%q names a non-loopback host — diagnostic profiling is OFF. "+
			"The endpoint serves goroutine stacks and heap contents, so it is loopback-only by design; "+
			"use a port forward to reach it remotely.\n", PprofAddressEnv, address)

		return
	}

	address = net.JoinHostPort(host, port)

	// Bind SYNCHRONOUSLY so a port that is already taken is reported now, rather than being lost on a
	// background goroutine while the user waits for an endpoint that never came up.
	listener, err := net.Listen("tcp", address)

	if err != nil {
		fmt.Fprintf(os.Stderr, "WARNING: diagnostic profiling could not listen on %s (%v) — profiling is OFF\n",
			address, err)

		return
	}

	fmt.Fprintf(os.Stderr, "Diagnostic profiling on http://%s/debug/pprof/ (set by %s)\n", address, PprofAddressEnv)

	go func() {
		if err := http.Serve(listener, nil); err != nil {
			fmt.Fprintf(os.Stderr, "WARNING: diagnostic profiling endpoint stopped: %v\n", err)
		}
	}()
}

// isLoopbackHost reports whether host names the local machine only — the literal "localhost", or any
// loopback IP (127.0.0.0/8, ::1).
func isLoopbackHost(host string) bool {
	if host == "localhost" {
		return true
	}

	ip := net.ParseIP(host)

	return ip != nil && ip.IsLoopback()
}

// The stall watch: a package that is still converting after GO2CS_STALL_WARN (default 2m) gets a warning on
// stderr, repeated at 4m, 8m and 16m and then every 16m, and after GO2CS_STALL_DUMP (default 5m) every goroutine's
// stack is written once to a temp-dir file that stderr names. docs/phase4/DESIGN-conversion-stall-detection.md is
// the design; COORD ruled its five questions on 2026-11-05.
//
// Why: a user watching a `[1440/1726]` progress line sit still cannot tell a converter bug from slow work, and
// issue #33's hang took two round trips because its first report could carry no stack. The watch names the
// package, says the run is abnormal while it is still running, and leaves the stack where a report can attach it.
//
// It only ever warns. Nothing is aborted, skipped or failed on the strength of this clock, and the conversion's
// result, error and panic pass through untouched (the -stdlib driver's own -convert-timeout cap is separate and
// unchanged). The three batch loops call it through stallWatch: -recurse's convertAll, -stdlib's
// convertAllPackages, and the conversion step of -tests -recurse phase B. A single-package conversion is not
// watched: there is only one package, and the user is watching the command they just typed.
const (
	stallWarnEnv     = "GO2CS_STALL_WARN"
	stallDumpEnv     = "GO2CS_STALL_DUMP"
	defaultStallWarn = 2 * time.Minute
	defaultStallDump = 5 * time.Minute
	stallRepeatCap   = 16 * time.Minute
)

// stallWatchConfig is one run's watch settings. A zero warn or dump turns that half off.
type stallWatchConfig struct {
	warn      time.Duration // first warning; later ones follow nextStallWarning
	dump      time.Duration // the one goroutine dump
	repeatCap time.Duration // the widest gap between two warnings
	out       io.Writer     // where warnings go (stderr)
	dumpDir   string        // where the dump file goes; empty means os.TempDir()
}

// stallWatch is what the batch drivers call, so a test can observe which packages are watched.
var stallWatch = withStallWatch

// stallWatchSettings reads the environment once per process, so a malformed value is reported once.
var stallWatchSettings = sync.OnceValue(func() stallWatchConfig {
	return stallWatchConfigFrom(os.Getenv, os.Stderr)
})

// stallOutputLock keeps one report whole when two watches, or a watch and its dump, write at once.
var stallOutputLock sync.Mutex

// withStallWatch runs convert, warning while it runs past this process's thresholds.
func withStallWatch(label string, convert func() error) error {
	return watchForStall(stallWatchSettings(), label, convert)
}

// stallWatchConfigFrom resolves the thresholds from the environment, complaining to out about a value that is
// not a non-negative Go duration and keeping the default for it.
func stallWatchConfigFrom(getenv func(string) string, out io.Writer) stallWatchConfig {
	return stallWatchConfig{
		warn:      stallThreshold(getenv, stallWarnEnv, defaultStallWarn, out),
		dump:      stallThreshold(getenv, stallDumpEnv, defaultStallDump, out),
		repeatCap: stallRepeatCap,
		out:       out,
	}
}

func stallThreshold(getenv func(string) string, name string, fallback time.Duration, out io.Writer) time.Duration {
	value := strings.TrimSpace(getenv(name))

	if value == "" {
		return fallback
	}

	threshold, err := time.ParseDuration(value)

	if err != nil || threshold < 0 {
		fmt.Fprintf(out, "WARNING: %s=%q is not a non-negative Go duration (such as 90s, 10m, or 0 for off); using %s\n",
			name, value, fallback)

		return fallback
	}

	return threshold
}

// nextStallWarning is when the warning after the one at `at` is due: the gap doubles, up to repeatCap.
func nextStallWarning(at time.Duration, repeatCap time.Duration) time.Duration {
	if repeatCap > 0 && at > repeatCap {
		return at + repeatCap
	}

	return at + at
}

// watchForStall runs convert under config's thresholds. The timers fire on their own goroutines; the deferred
// disarm runs on every exit, a panic included, and holds the watch's lock, so nothing is written once convert
// has returned.
func watchForStall(config stallWatchConfig, label string, convert func() error) error {
	if config.warn <= 0 && config.dump <= 0 {
		return convert()
	}

	watch := &stallWatcher{config: config, label: label, start: time.Now()}
	watch.arm()
	defer watch.disarm()

	return convert()
}

type stallWatcher struct {
	config   stallWatchConfig
	label    string
	start    time.Time
	lock     sync.Mutex
	stopped  bool
	nextWarn time.Duration
	warning  *time.Timer
	dumping  *time.Timer
}

func (w *stallWatcher) arm() {
	w.lock.Lock()
	defer w.lock.Unlock()

	if w.config.warn > 0 {
		w.nextWarn = w.config.warn
		w.warning = time.AfterFunc(w.config.warn, w.warn)
	}

	if w.config.dump > 0 {
		w.dumping = time.AfterFunc(w.config.dump, w.dump)
	}
}

func (w *stallWatcher) disarm() {
	w.lock.Lock()
	defer w.lock.Unlock()

	w.stopped = true

	if w.warning != nil {
		w.warning.Stop()
	}

	if w.dumping != nil {
		w.dumping.Stop()
	}
}

func (w *stallWatcher) warn() {
	w.lock.Lock()
	defer w.lock.Unlock()

	if w.stopped {
		return
	}

	w.nextWarn = nextStallWarning(w.nextWarn, w.config.repeatCap)
	w.warning = time.AfterFunc(time.Until(w.start.Add(w.nextWarn)), w.warn)

	writeStallReport(w.config.out, fmt.Sprintf(
		"WARNING: %s has been converting for %s.\n"+
			"         A package usually converts in a few seconds, so this is unusual and worth reporting:\n"+
			"         https://github.com/ritchiecarroll/go2cs/issues\n"+
			"         The conversion has NOT been interrupted and will continue.\n"+
			"         For a diagnosis, re-run with %s=:6060 (a bare port serves on loopback only). That run\n"+
			"         prints its profiling endpoint; while the package is converting, capture where the time goes:\n"+
			"             go tool pprof -top <endpoint>/debug/pprof/profile?seconds=20\n",
		w.label, formatDuration(time.Since(w.start)), PprofAddressEnv))
}

func (w *stallWatcher) dump() {
	w.lock.Lock()
	defer w.lock.Unlock()

	if w.stopped {
		return
	}

	elapsed := formatDuration(time.Since(w.start))
	stacks := allGoroutineStacks()
	header := fmt.Sprintf("go2cs stall dump: %s had been converting for %s\n\n", w.label, elapsed)
	path, err := writeStallDump(w.config.dumpDir, header, stacks)

	if err != nil {
		writeStallReport(w.config.out, fmt.Sprintf(
			"WARNING: %s has been converting for %s, and its stack dump could not be written (%v).\n"+
				"         The conversion has NOT been interrupted. Every goroutine's stack follows:\n%s",
			w.label, elapsed, err, stacks))

		return
	}

	writeStallReport(w.config.out, fmt.Sprintf(
		"WARNING: %s has been converting for %s. Every goroutine's stack is in\n"+
			"         %s\n"+
			"         Attach that file to the report. The conversion has NOT been interrupted and will continue.\n",
		w.label, elapsed, path))
}

// allGoroutineStacks is runtime.Stack for every goroutine, with the buffer grown until the dump fits rather than
// truncated: the escape analysis runs a goroutine per file, so a large package has hundreds of stacks. It stops
// the world while it copies them, which is affordable at a dump threshold measured in minutes.
func allGoroutineStacks() []byte {
	buffer := make([]byte, 1<<20)

	for {
		n := runtime.Stack(buffer, true)

		if n < len(buffer) {
			return buffer[:n]
		}

		buffer = make([]byte, 2*len(buffer))
	}
}

func writeStallDump(dir string, header string, stacks []byte) (string, error) {
	file, err := os.CreateTemp(dir, "go2cs-stall-*.txt")

	if err != nil {
		return "", err
	}

	_, err = file.WriteString(header)

	if err == nil {
		_, err = file.Write(stacks)
	}

	if closeErr := file.Close(); err == nil {
		err = closeErr
	}

	return file.Name(), err
}

// writeStallReport writes one report in a single call, so it reaches a merged stdout/stderr stream whole.
func writeStallReport(out io.Writer, report string) {
	stallOutputLock.Lock()
	defer stallOutputLock.Unlock()

	_, _ = io.WriteString(out, report)
}
