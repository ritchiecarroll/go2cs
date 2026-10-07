// crashVerdict_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the crash verdict: a converted test host that dies (a stack overflow kills a .NET process outright: exit
// 0xC00000FD on windows, SIGABRT on linux) gives the test it was running a C# FAIL verdict carrying the runtime's
// overflow report, instead of leaving that test with no verdict and the crash buried in an error string. Measured
// on a scratch package (Recurse(n) = Recurse(n+1)+1): the record read csharp={TestAdd: pass}, TestOverflow absent.
// The same for a host that reaches its own package deadline with a test still running (a converted self-call the
// JIT turned into a loop spins until then): that test gets a FAIL verdict reading "timed out".
//
// The exits are REAL: each case re-executes this test binary as a stand-in host (TestHostCrashHelperProcess) and
// runs it through runCommandWithTimeout, the pipeline's own launcher, so the error the classifier reads is the
// wrapped one the comparison path holds.

package main

import (
	"fmt"
	"os"
	"runtime"
	"strings"
	"testing"
	"time"
)

const hostCrashHelperEnv = "GO2CS_HOST_CRASH_HELPER"

// The stand-in host: prints the event stream the case names, then exits the way the case names. Inert unless the
// environment variable is set, so a normal `go test` run passes it trivially.
func TestHostCrashHelperProcess(t *testing.T) {
	mode := os.Getenv(hostCrashHelperEnv)

	if mode == "" {
		return
	}

	event := func(test, action string) {
		fmt.Printf("{\"package\":\"p\",\"test\":%q,\"action\":%q}\n", test, action)
	}

	overflow := func() {
		fmt.Fprint(os.Stderr, "Stack overflow.\r\nRepeated 22369208 times:\r\n--------------------------------\r\n"+
			"   at go.p_package.Recurse(IntPtr)\r\n--------------------------------\r\n   at go.p_test_package.TestOverflow(ж`1)\r\n")
	}

	crash := func() {
		if runtime.GOOS == "windows" {
			os.Exit(-1073741571) // 0xC00000FD, STATUS_STACK_OVERFLOW
		}

		process, _ := os.FindProcess(os.Getpid())
		_ = process.Kill() // killed by a signal, as a .NET host is by its SIGABRT
		time.Sleep(time.Minute)
	}

	switch mode {
	case "overflow-in-flight":
		event("TestAdd", "run")
		event("TestAdd", "pass")
		event("TestOverflow", "run")
		overflow()
		crash()
	case "overflow-none-in-flight":
		event("TestAdd", "run")
		event("TestAdd", "pass")
		overflow()
		crash()
	case "two-in-flight":
		event("TestA", "run")
		event("TestB", "run")
		event("TestB/sub", "run")
		event("TestC", "run")
		event("TestC", "pass")
		crash()
	case "ordinary-failure":
		event("TestAdd", "run")
		event("TestAdd", "fail")
		os.Exit(1)
	case "exit-in-flight":
		event("TestExits", "run")
		os.Exit(3)
	case "host-timeout":
		event("TestBlocks", "run")
		fmt.Println("{\"package\":\"p\",\"test\":\"\",\"action\":\"timeout\",\"output\":\"package timeout after 00:00:01\"}")
		os.Exit(1)
	}

	os.Exit(0)
}

// runStandInHost runs the stand-in host in the given mode through the pipeline's launcher.
func runStandInHost(t *testing.T, mode string) (string, error) {
	t.Helper()
	t.Setenv(hostCrashHelperEnv, mode)

	return runCommandWithTimeout(time.Minute, ".", Options{}, os.Args[0], "-test.run=^TestHostCrashHelperProcess$")
}

func newCrashComparison(csharp map[string]string) *testComparison {
	return &testComparison{CSharp: csharp, Errors: []string{"converted tests: (the launcher's own error)"}}
}

func everythingEligible(results map[string]string) map[string]string { return results }

func TestHostCrashGivesTheInFlightTestAFailVerdict(t *testing.T) {
	output, err := runStandInHost(t, "overflow-in-flight")

	if err == nil {
		t.Fatal("the stand-in host exited 0; the case needs a crash")
	}

	result := newCrashComparison(terminalTestResults(output))
	applyHostCrash(result, err, output, everythingEligible)

	if result.CSharp["TestOverflow"] != "fail" {
		t.Errorf("the test in flight when the host died has C# verdict %q, want fail; verdicts %v", result.CSharp["TestOverflow"], result.CSharp)
	}

	if result.CSharp["TestAdd"] != "pass" {
		t.Errorf("a test that finished before the crash must keep its own verdict; TestAdd reads %q", result.CSharp["TestAdd"])
	}

	joined := strings.Join(result.Errors, "\n")

	for _, want := range []string{"crashed: stack overflow", "TestOverflow", "Stack overflow.", "Repeated 22369208 times:", "Recurse(IntPtr)"} {
		if !strings.Contains(joined, want) {
			t.Errorf("the record's errors do not carry %q:\n%s", want, joined)
		}
	}
}

// (a) A crash with NO test in flight stays a package-level error, never pinned on a test.
func TestHostCrashWithNoTestInFlightStaysPackageLevel(t *testing.T) {
	output, err := runStandInHost(t, "overflow-none-in-flight")

	result := newCrashComparison(terminalTestResults(output))
	applyHostCrash(result, err, output, everythingEligible)

	if len(result.CSharp) != 1 || result.CSharp["TestAdd"] != "pass" {
		t.Errorf("no test was in flight, so no verdict may be added or changed; verdicts %v", result.CSharp)
	}

	joined := strings.Join(result.Errors, "\n")

	if !strings.Contains(joined, "crashed: stack overflow") || !strings.Contains(joined, "no test in flight") {
		t.Errorf("the crash must still be recorded, at package level:\n%s", joined)
	}
}

// (b) More than one test in flight (parallel tests, a subtest inside its parent) names ALL of them, none silently.
func TestHostCrashNamesEveryTestInFlight(t *testing.T) {
	output, err := runStandInHost(t, "two-in-flight")

	result := newCrashComparison(terminalTestResults(output))
	applyHostCrash(result, err, output, everythingEligible)

	for _, name := range []string{"TestA", "TestB", "TestB/sub"} {
		if result.CSharp[name] != "fail" {
			t.Errorf("%s was in flight when the host died; its C# verdict reads %q, want fail", name, result.CSharp[name])
		}

		if !strings.Contains(strings.Join(result.Errors, "\n"), name) {
			t.Errorf("%s was in flight and is not named in the record's errors:\n%s", name, strings.Join(result.Errors, "\n"))
		}
	}

	if result.CSharp["TestC"] != "pass" {
		t.Errorf("TestC finished before the crash; its verdict reads %q, want pass", result.CSharp["TestC"])
	}
}

// Any exit with a test in flight is the crash case too: the test never reached a verdict of its own.
func TestHostExitWithATestInFlightIsACrash(t *testing.T) {
	output, err := runStandInHost(t, "exit-in-flight")

	result := newCrashComparison(terminalTestResults(output))
	applyHostCrash(result, err, output, everythingEligible)

	if result.CSharp["TestExits"] != "fail" {
		t.Errorf("TestExits was in flight when the host exited 3; its verdict reads %q, want fail", result.CSharp["TestExits"])
	}

	if !strings.Contains(strings.Join(result.Errors, "\n"), "exit status 3") {
		t.Errorf("the record must say how the host exited:\n%s", strings.Join(result.Errors, "\n"))
	}
}

// The LIVE host: a test that never returns (G's case: the converted `Sort(this StringSlice x) { Sort(x); }`, which
// the JIT turns into a loop, so nothing overflows) holds the host until its own package deadline, which writes a
// package "timeout" event and exits 1. Every test still in flight gets a C# FAIL verdict saying so.
func TestHostTimeoutGivesTheInFlightTestAFailVerdict(t *testing.T) {
	output, err := runStandInHost(t, "host-timeout")

	result := newCrashComparison(terminalTestResults(output))
	applyHostCrash(result, err, output, everythingEligible)

	if result.CSharp["TestBlocks"] != "fail" {
		t.Errorf("TestBlocks was in flight at the package deadline; its verdict reads %q, want fail", result.CSharp["TestBlocks"])
	}

	joined := strings.Join(result.Errors, "\n")

	for _, want := range []string{"TestBlocks", "timed out (in flight at the package deadline)", "package timeout after 00:00:01"} {
		if !strings.Contains(joined, want) {
			t.Errorf("the record's errors do not carry %q:\n%s", want, joined)
		}
	}

	if strings.Contains(joined, "crashed") {
		t.Errorf("a host that reached its own deadline did not crash, and the record must not say it did:\n%s", joined)
	}
}

// The cases that are NOT a crash leave the record exactly as the comparison built it.
func TestHostCrashLeavesNonCrashesUntouched(t *testing.T) {
	for _, mode := range []string{"ordinary-failure"} {
		output, err := runStandInHost(t, mode)

		if err == nil {
			t.Fatalf("%s: the stand-in host exited 0", mode)
		}

		result := newCrashComparison(terminalTestResults(output))
		before := fmt.Sprint(result.CSharp, result.Errors)
		applyHostCrash(result, err, output, everythingEligible)

		if after := fmt.Sprint(result.CSharp, result.Errors); after != before {
			t.Errorf("%s is not a crash, and the record moved:\nbefore %s\nafter  %s", mode, before, after)
		}
	}

	// The DRIVER's own deadline is not a host exit: its error carries no process state at all.
	result := newCrashComparison(map[string]string{})
	output := "{\"package\":\"p\",\"test\":\"TestSlow\",\"action\":\"run\"}\n"
	applyHostCrash(result, fmt.Errorf("host timed out after 1m0s"), output, everythingEligible)

	if len(result.CSharp) != 0 || len(result.Errors) != 1 {
		t.Errorf("a driver timeout is not a crash; the record moved: %v %v", result.CSharp, result.Errors)
	}
}

// A test the manifest does not make eligible gets no verdict, exactly as its terminal verdict would not.
func TestHostCrashHonoursEligibility(t *testing.T) {
	output, err := runStandInHost(t, "two-in-flight")

	result := newCrashComparison(map[string]string{})
	applyHostCrash(result, err, output, func(results map[string]string) map[string]string {
		delete(results, "TestA")
		return results
	})

	if _, ok := result.CSharp["TestA"]; ok {
		t.Errorf("TestA is not eligible and must get no verdict; verdicts %v", result.CSharp)
	}

	if result.CSharp["TestB"] != "fail" {
		t.Errorf("TestB is eligible and was in flight; its verdict reads %q, want fail", result.CSharp["TestB"])
	}
}

// The comparison wrote its MISMATCH line for an in-flight test before the crash arm ran, so it read C#="" while the
// record's verdict said fail (x/sync singleflight in TRAIN P's battery: the record held the fails, the printed
// comparison line still read Go="pass" C#=""). The crash arm brings the crashed names' mismatch lines into agreement,
// and leaves every other line, including another test's identical-looking one, as matching wrote it.
func TestHostCrashRewritesTheInFlightTestsMismatchLines(t *testing.T) {
	output, err := runStandInHost(t, "overflow-in-flight")

	result := newCrashComparison(terminalTestResults(output))
	result.Errors = append(result.Errors, `TestOverflow: Go="pass" C#=""`, `TestElsewhere: Go="pass" C#=""`)
	applyHostCrash(result, err, output, everythingEligible)

	joined := strings.Join(result.Errors, "\n")

	if !strings.Contains(joined, `TestOverflow: Go="pass" C#="fail (crashed in flight)"`) {
		t.Errorf("the in-flight test's mismatch line must read its crash verdict:\n%s", joined)
	}

	if strings.Contains(joined, `TestOverflow: Go="pass" C#=""`) {
		t.Errorf("the in-flight test's mismatch line still reads no verdict:\n%s", joined)
	}

	if !strings.Contains(joined, `TestElsewhere: Go="pass" C#=""`) {
		t.Errorf("a test that was NOT in flight keeps its mismatch line as matching wrote it:\n%s", joined)
	}
}

// The live-host twin: a test held until the package deadline reads "fail (in flight at the package deadline)".
func TestHostTimeoutRewritesTheInFlightTestsMismatchLine(t *testing.T) {
	output, err := runStandInHost(t, "host-timeout")

	result := newCrashComparison(terminalTestResults(output))
	result.Errors = append(result.Errors, `TestBlocks: Go="pass" C#=""`)
	applyHostCrash(result, err, output, everythingEligible)

	joined := strings.Join(result.Errors, "\n")

	if !strings.Contains(joined, `TestBlocks: Go="pass" C#="fail (in flight at the package deadline)"`) || strings.Contains(joined, `TestBlocks: Go="pass" C#=""`) {
		t.Errorf("the in-flight test's mismatch line must read its deadline verdict:\n%s", joined)
	}
}

func TestInFlightTestsReadsTheStreamInOrder(t *testing.T) {
	output := strings.Join([]string{
		`{"package":"p","test":"","action":"run"}`,
		`{"package":"p","test":"TestA","action":"run"}`,
		`{"package":"p","test":"TestA","action":"pass"}`,
		`{"package":"p","test":"TestZ","action":"run"}`,
		`not an event`,
		`{"package":"p","test":"TestB","action":"run"}`,
		`{"package":"p","test":"TestB/one","action":"run"}`,
		`{"package":"p","test":"TestB/one","action":"skip"}`,
		`{"package":"p","test":"TestB/two","action":"run"}`,
	}, "\n")

	got := strings.Join(inFlightTests(output), ",")

	if want := "TestZ,TestB,TestB/two"; got != want {
		t.Errorf("in flight: got %q, want %q", got, want)
	}
}
