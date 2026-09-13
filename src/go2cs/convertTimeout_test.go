// convertTimeout_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"strings"
	"testing"
	"time"
)

// The -convert-timeout flag: the -stdlib driver's per-package conversion cap.
//
// The cap was hard-coded at ten minutes until 2026-09-02, when concurrent lane load pushed one
// package's conversion past it mid two-seeded A/B -- which would have reported a whole package as
// a spurious emission difference. These tests pin the three properties that make the knob
// discoverable and safe: the default is unchanged for anyone who does not pass the flag, a
// non-positive value dies at the command line rather than firing on every package, and the fired
// message names the flag so the next reader learns the knob from the failure text.

// TestConvertTimeoutDefaultIsTenMinutes pins the default at the value the cap was hard-coded to,
// so adding the flag changed nothing for a caller who does not pass it. The source assertion is
// the load-bearing half: the constant alone could stay 10m while main.go registered a literal.
func TestConvertTimeoutDefaultIsTenMinutes(t *testing.T) {
	if defaultConvertTimeout != 10*time.Minute {
		t.Errorf("defaultConvertTimeout = %s, want 10m0s -- the flag's default must reproduce the cap that was hard-coded before it existed", defaultConvertTimeout)
	}

	source := readConverterSource(t, "main.go")

	const registration = `commandLine.Duration("convert-timeout", defaultConvertTimeout,`

	if !strings.Contains(source, registration) {
		t.Errorf("main.go does not register -convert-timeout with defaultConvertTimeout as its default (looked for %q); a literal there can drift from the constant this test pins", registration)
	}
}

// TestConvertTimeoutRejectsNonPositive pins the fail-fast posture -test-timeout established: a
// zero or negative cap fires the instant a conversion starts, so it must be rejected at the
// command line -- otherwise every package "times out" and the run reads as a corpus-wide failure.
func TestConvertTimeoutRejectsNonPositive(t *testing.T) {
	for _, bad := range []time.Duration{0, -time.Nanosecond, -30 * time.Minute} {
		err := validateConvertTimeout(bad)

		if err == nil {
			t.Errorf("validateConvertTimeout(%s) = nil, want an error -- a non-positive cap fires immediately", bad)
			continue
		}

		if !strings.Contains(err.Error(), "-convert-timeout") {
			t.Errorf("validateConvertTimeout(%s) error %q does not name the flag", bad, err)
		}
	}

	for _, good := range []time.Duration{time.Nanosecond, time.Second, defaultConvertTimeout, 90 * time.Minute} {
		if err := validateConvertTimeout(good); err != nil {
			t.Errorf("validateConvertTimeout(%s) = %v, want nil", good, err)
		}
	}

	// The validation is only fail-fast if main() actually calls it and dies on the result; a
	// helper nothing consults is a guard that cannot fire.
	source := readConverterSource(t, "main.go")

	const call = `if err := validateConvertTimeout(options.convertTimeout); err != nil {`

	index := strings.Index(source, call)

	if index < 0 {
		t.Fatalf("main.go does not validate -convert-timeout (looked for %q)", call)
	}

	window := source[index:min(index+len(call)+200, len(source))]

	if !strings.Contains(window, "log.Fatal") {
		t.Errorf("main.go validates -convert-timeout but does not die on the result; the branch reads:\n%s", window)
	}
}

// TestConvertTimeoutFiredMessageNamesFlag drives the timeout path deterministically -- the work
// function blocks until the test releases it, so the cap is the only thing that can complete the
// select -- and pins that the message carries both the elapsed budget and the flag that raises it.
func TestConvertTimeoutFiredMessageNamesFlag(t *testing.T) {
	release := make(chan struct{})
	defer close(release)

	err := runPackageConversionWithTimeout(5*time.Millisecond, func() error {
		<-release
		return nil
	})

	if err == nil {
		t.Fatal("runPackageConversionWithTimeout returned nil for a conversion that never completes; the cap did not fire")
	}

	message := err.Error()

	for _, want := range []string{"timed out", "5ms", "-convert-timeout", "safety net"} {
		if !strings.Contains(message, want) {
			t.Errorf("fired-cap message %q does not contain %q", message, want)
		}
	}
}

// TestConvertTimeoutPassesWorkThrough is the negative control for the test above: the same helper
// must return the conversion's own result untouched when the work finishes inside the cap, and
// must recover a panic in the conversion goroutine rather than taking the process down.
func TestConvertTimeoutPassesWorkThrough(t *testing.T) {
	if err := runPackageConversionWithTimeout(time.Minute, func() error { return nil }); err != nil {
		t.Errorf("runPackageConversionWithTimeout = %v, want nil for work that succeeds inside the cap", err)
	}

	err := runPackageConversionWithTimeout(time.Minute, func() error { panic("boom") })

	if err == nil || !strings.Contains(err.Error(), "panic in package conversion") {
		t.Errorf("runPackageConversionWithTimeout = %v, want the recovered-panic error", err)
	}
}

// TestPackageConvertTimeoutResolution pins the driver's own resolution: the flag's value when one
// was parsed, and the default when the Options were built in code (unit tests, internal drivers)
// rather than from the command line -- a zero there must not fire the cap instantly.
func TestPackageConvertTimeoutResolution(t *testing.T) {
	cases := []struct {
		name    string
		options time.Duration
		want    time.Duration
	}{
		{"flag value honored", 90 * time.Minute, 90 * time.Minute},
		{"zero takes the default", 0, defaultConvertTimeout},
		{"negative takes the default", -time.Second, defaultConvertTimeout},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			converter := NewStdLibConverter(Options{convertTimeout: tc.options})

			if got := converter.packageConvertTimeout(); got != tc.want {
				t.Errorf("packageConvertTimeout() = %s, want %s", got, tc.want)
			}
		})
	}
}

// TestConvertPackageComposesTheCap pins the driver seam -- convertPackage, the method the -stdlib
// loop calls, must run the conversion through the capped helper using the RESOLVED per-run cap.
// Both halves it composes are pinned behaviorally above; this is what makes them reachable.
//
// The cap is deliberately NOT driven end to end through a real conversion here, and the reason is
// worth stating rather than discovering: a fired cap ABANDONS its conversion goroutine (that is
// what the timeout means), and processConversion both mutates the converter's package-level global
// state and log.Fatalf's on a write failure. An abandoned real conversion would therefore race
// every later test in this package for that global state, and could take the whole binary down
// from a directory the test had already cleaned up -- an unacceptable trade in a suite every lane
// runs as a gate. The helper test above drives the identical select with work that blocks until
// released, which is deterministic and side-effect-free.
func TestConvertPackageComposesTheCap(t *testing.T) {
	source := readConverterSource(t, "stdLibConverter.go")

	const composition = `runPackageConversionWithTimeout(c.packageConvertTimeout(), func() error {`

	if !strings.Contains(source, composition) {
		t.Errorf("convertPackage does not run the conversion through the capped helper with the resolved cap (looked for %q)", composition)
	}
}

// readConverterSource reads one of the converter's own source files for the pins above.
func readConverterSource(t *testing.T, name string) string {
	t.Helper()

	contents, err := os.ReadFile(name)

	if err != nil {
		t.Fatalf("failed to read %s: %v", name, err)
	}

	return string(contents)
}
