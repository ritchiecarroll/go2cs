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
	"net"
	"net/http"
	"os"

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
