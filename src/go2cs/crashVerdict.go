// crashVerdict.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A CONVERTED TEST HOST THAT CRASHES, OR HOLDS A TEST UNTIL ITS PACKAGE DEADLINE.
//
// A test the host was running when the process ended has a "run" event and no terminal event, so the comparison read
// it as NO VERDICT and the cause survived only inside the launcher's error string. Two measured shapes:
//   - a CRASH: a stack overflow kills a .NET process outright (exit 0xC00000FD on windows, SIGABRT with "Stack
//     overflow." on linux), within seconds;
//   - a LIVE host: a converted self-call the JIT turns into a loop (G's `Sort(this StringSlice x) { Sort(x); }`) never
//     overflows, and the host's own package deadline ends it, with a package "timeout" event and exit 1.
// Either way the test in flight gets a C# FAIL verdict, and the record says which: "crashed" with how the host
// exited (and the runtime's overflow report, when there is one), or "timed out (in flight at the package deadline)".
//
// Applied ONLY where the converted side's failure already stands (testConversion.go calls it inside its csErr arm,
// after both forgiveness arms), so a record that validates is never touched: the verdicts added here can only change
// a record that is already failing. A crash with no test in flight stays a package-level error, pinned on no test.

package main

import (
	"encoding/json"
	"errors"
	"fmt"
	"os/exec"
	"strings"
)

// hostStackOverflowStatus is windows' STATUS_STACK_OVERFLOW, the exit status of a .NET process whose stack overflowed.
const hostStackOverflowStatus = 0xC00000FD

// hostCrash is what a converted test host's abnormal end says about the run.
type hostCrash struct {
	Label    string   // "stack overflow, exit status 0xc00000fd", "signal: aborted (core dumped)", "exit status 3", or the host's own timeout text
	InFlight []string // tests with a run event and no terminal event, in stream order
	Head     string   // the first lines of the runtime's own stack-overflow report, " | "-joined ("" when none)
	TimedOut bool     // the host reached its own package deadline (a live host), rather than dying
}

// inFlightTests names every test the stream started and never finished, in the order they started. A subtest in
// flight is named beside its parent, which is in flight too.
func inFlightTests(output string) []string {
	var started []string
	finished := map[string]bool{}

	for _, line := range testStreamLines(output) {
		var event normalizedTestEvent

		if json.Unmarshal([]byte(line), &event) != nil || event.Test == "" {
			continue
		}

		switch event.Action {
		case "run":
			started = append(started, event.Test)
		case "pass", "fail", "skip", "timeout", "infrastructure-error":
			finished[event.Test] = true
		}
	}

	var inFlight []string

	for _, name := range started {
		if !finished[name] {
			inFlight = append(inFlight, name)
			finished[name] = true // a name started twice (-count) is named once
		}
	}

	return inFlight
}

// hostPackageTimeout returns the Output of the host's own package-level "timeout" event, when it wrote one.
func hostPackageTimeout(output string) (string, bool) {
	for _, line := range testStreamLines(output) {
		var event normalizedTestEvent

		if json.Unmarshal([]byte(line), &event) == nil && event.Test == "" && event.Action == "timeout" {
			return event.Output, true
		}
	}

	return "", false
}

// stackOverflowHead is the runtime's own report of a stack overflow, its first lines " | "-joined ("" when absent).
func stackOverflowHead(output string) string {
	lines := strings.Split(strings.ReplaceAll(output, "\r\n", "\n"), "\n")

	for index, line := range lines {
		if strings.TrimSpace(line) != "Stack overflow." {
			continue
		}

		var head []string

		for _, next := range lines[index:] {
			next = strings.TrimSpace(next)

			if next == "" || strings.Trim(next, "-") == "" {
				continue
			}

			if head = append(head, next); len(head) == 4 {
				break
			}
		}

		return strings.Join(head, " | ")
	}

	return ""
}

// classifyHostCrash reads the converted host's run. It applies only to a host that EXITED (the launcher's error wraps
// an *exec.ExitError): the driver's own deadline and a host that never started carry no process state and are not
// this class. A plain non-zero exit with every test finished is an ordinary failure, not a crash.
func classifyHostCrash(rawErr error, output string) (hostCrash, bool) {
	var exitErr *exec.ExitError

	if !errors.As(rawErr, &exitErr) {
		return hostCrash{}, false
	}

	crash := hostCrash{InFlight: inFlightTests(output), Head: stackOverflowHead(output)}

	if timeoutText, ok := hostPackageTimeout(output); ok {
		crash.Label, crash.TimedOut = timeoutText, true
		return crash, len(crash.InFlight) > 0
	}

	code := exitErr.ExitCode()
	overflow := crash.Head != "" || uint32(code) == hostStackOverflowStatus
	abnormal := code < 0 || uint32(code) >= 0xC0000000 // killed by a signal; a windows NTSTATUS error

	crash.Label = exitErr.Error()

	if overflow {
		crash.Label = "stack overflow, " + crash.Label
	}

	return crash, overflow || abnormal || len(crash.InFlight) > 0
}

// applyHostCrash records a crash, or a live host's package deadline, on a comparison whose converted side's failure
// already stands: each eligible test in flight gets a C# FAIL verdict, and the record's errors say why. eligible is the
// manifest's filter, so a test no terminal verdict could reach gets none here either.
func applyHostCrash(result *testComparison, rawErr error, output string, eligible func(map[string]string) map[string]string) {
	crash, ok := classifyHostCrash(rawErr, output)

	if !ok {
		return
	}

	verdicts := map[string]string{}

	for _, name := range crash.InFlight {
		if _, terminal := result.CSharp[name]; !terminal {
			verdicts[name] = "fail"
		}
	}

	verdicts = eligible(verdicts)

	var named []string

	for _, name := range crash.InFlight {
		if _, ok := verdicts[name]; ok {
			named = append(named, name)
			result.CSharp[name] = "fail"
		}
	}

	if crash.TimedOut {
		if len(named) == 0 {
			return
		}

		result.Errors = append(result.Errors, fmt.Sprintf("converted tests timed out at the package deadline (%s); in flight: %s",
			crash.Label, strings.Join(named, ", ")))

		for _, name := range named {
			result.Errors = append(result.Errors, fmt.Sprintf("%s: C# timed out (in flight at the package deadline)", name))
		}

		rewriteInFlightMismatches(result, named, "fail (in flight at the package deadline)")
		return
	}

	head := ""

	if crash.Head != "" {
		head = ": " + crash.Head
	}

	if len(named) == 0 {
		result.Errors = append(result.Errors, fmt.Sprintf("converted tests crashed: %s; no test in flight (package-level)%s", crash.Label, head))
		return
	}

	result.Errors = append(result.Errors, fmt.Sprintf("converted tests crashed: %s; in flight: %s", crash.Label, strings.Join(named, ", ")))

	for _, name := range named {
		result.Errors = append(result.Errors, fmt.Sprintf("%s: C# crashed in flight (%s)%s", name, crash.Label, head))
	}

	rewriteInFlightMismatches(result, named, "fail (crashed in flight)")
}

// rewriteInFlightMismatches brings the mismatch line matching wrote for each named test into agreement with the verdict
// this arm just gave it. Matching ran first, saw no C# verdict, and wrote `<name>: Go="..." C#=""`; that exact line,
// and no other, now reads the verdict. The `: Go=` boundary keeps a parent's name from matching its subtest's line.
func rewriteInFlightMismatches(result *testComparison, named []string, verdict string) {
	for index, line := range result.Errors {
		for _, name := range named {
			if strings.HasPrefix(line, name+": Go=") && strings.HasSuffix(line, ` C#=""`) {
				result.Errors[index] = strings.TrimSuffix(line, `""`) + fmt.Sprintf("%q", verdict)
				break
			}
		}
	}
}
