// platformFilterSkip_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Unit gates for a -stdlib package filter under a multi-target run (-platforms, -platform-census).
// A package can exist on only some GOOS, so a target whose conversion finds none of the filtered
// packages is skipped with a notice; the run fails only when NO target matches. A single-target
// conversion keeps the plain "no matching packages" error. None of this needs a standard-library
// conversion: the per-target converter is replaced by a fake that reports what it would have found.

package main

import (
	"errors"
	"fmt"
	"io"
	"os"
	"reflect"
	"strings"
	"testing"
)

func TestFilterStdLibQueueReportsNoMatchWithSentinel(t *testing.T) {
	queue := []string{"internal/goarch", "internal/runtime/syscall/linux", "os"}

	got, err := filterStdLibQueue(queue, []string{"os", "internal/goarch", "nope"})

	if err != nil {
		t.Fatalf("filter with matches: %v", err)
	}

	if want := []string{"internal/goarch", "os"}; !reflect.DeepEqual(got, want) {
		t.Fatalf("filtered queue keeps dependency order: got %v, want %v", got, want)
	}

	if _, err := filterStdLibQueue(queue, []string{"nope"}); !errors.Is(err, errNoMatchingPackages) {
		t.Fatalf("filter matching nothing must return errNoMatchingPackages, got %v", err)
	}

	if err := errNoMatchingPackages; err.Error() != "no matching packages found after filtering" {
		t.Fatalf("the single-target message is unchanged, got %q", err.Error())
	}
}

// fakeTargetRun stands in for runCensusTarget: matching targets emit, the rest report the sentinel
// the way the converter does (wrapped, as runCensusTarget and the callers wrap it).
func fakeTargetRun(matching map[string]bool, ran *[]string) func(string) (*platformEmission, error) {
	return func(target string) (*platformEmission, error) {
		*ran = append(*ran, target)

		if !matching[target] {
			return nil, fmt.Errorf("census target %s failed: %w", target,
				fmt.Errorf("standard library conversion failed: %w", errNoMatchingPackages))
		}

		return &platformEmission{target: target}, nil
	}
}

func emissionTargets(emissions []*platformEmission) []string {
	targets := []string{}

	for _, emission := range emissions {
		targets = append(targets, emission.target)
	}

	return targets
}

func TestMultiTargetFilterSkipsTargetsWithNoMatch(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}
	filter := []string{"internal/runtime/syscall/linux"}
	ran := []string{}

	emissions, err := runTargetsSkippingUnmatched(targets, filter, "census",
		fakeTargetRun(map[string]bool{"linux/amd64": true}, &ran))

	if err != nil {
		t.Fatalf("a filter matching one target must not abort the run: %v", err)
	}

	if want := []string{"linux/amd64"}; !reflect.DeepEqual(emissionTargets(emissions), want) {
		t.Fatalf("emitting targets: got %v, want %v", emissionTargets(emissions), want)
	}

	if !reflect.DeepEqual(ran, targets) {
		t.Fatalf("every target is still attempted, in order: got %v, want %v", ran, targets)
	}
}

func TestMultiTargetFilterNamesSkippedTargetsAndFilter(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}
	filter := []string{"internal/runtime/syscall/linux", "other/pkg"}
	ran := []string{}

	output := captureFilterSkipStdout(t, func() {
		if _, err := runTargetsSkippingUnmatched(targets, filter, "census",
			fakeTargetRun(map[string]bool{"linux/amd64": true}, &ran)); err != nil {
			t.Fatalf("unexpected error: %v", err)
		}
	})

	for _, skipped := range []string{"windows/amd64", "darwin/amd64"} {
		line := ""

		for _, candidate := range strings.Split(output, "\n") {
			if strings.Contains(candidate, "skipping") && strings.Contains(candidate, skipped) {
				line = candidate
			}
		}

		if line == "" {
			t.Fatalf("no skip notice names %s in:\n%s", skipped, output)
		}

		if !strings.Contains(line, "internal/runtime/syscall/linux") || !strings.Contains(line, "other/pkg") {
			t.Fatalf("the skip notice for %s must name the filter, got %q", skipped, line)
		}
	}

	if strings.Contains(output, "skipping linux/amd64") {
		t.Fatalf("a target that matched must not be reported skipped:\n%s", output)
	}
}

func TestMultiTargetFilterMatchingNowhereStillFails(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64"}
	ran := []string{}

	_, err := runTargetsSkippingUnmatched(targets, []string{"nope"}, "census", fakeTargetRun(map[string]bool{}, &ran))

	if !errors.Is(err, errNoMatchingPackages) {
		t.Fatalf("a filter matching on no target must fail with errNoMatchingPackages, got %v", err)
	}

	if !strings.Contains(err.Error(), "windows/amd64") || !strings.Contains(err.Error(), "linux/amd64") {
		t.Fatalf("the failure names every target it tried, got %q", err.Error())
	}
}

func TestMultiTargetOtherFailuresStayFatal(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64"}
	boom := errors.New("convert exploded")

	_, err := runTargetsSkippingUnmatched(targets, []string{"os"}, "emission", func(target string) (*platformEmission, error) {
		return nil, fmt.Errorf("emission target %s failed: %w", target, boom)
	})

	if !errors.Is(err, boom) || errors.Is(err, errNoMatchingPackages) {
		t.Fatalf("only an empty filter match is skippable, got %v", err)
	}
}

func TestSingleTargetFilterKeepsTheError(t *testing.T) {
	ran := []string{}

	_, err := runTargetsSkippingUnmatched([]string{"linux/amd64"}, []string{"nope"}, "census", fakeTargetRun(map[string]bool{}, &ran))

	if !errors.Is(err, errNoMatchingPackages) {
		t.Fatalf("a lone target matching nothing keeps the error, got %v", err)
	}
}

// captureFilterSkipStdout runs fn with os.Stdout redirected to a pipe and returns what it wrote; the skip notice goes there.
func captureFilterSkipStdout(t *testing.T, fn func()) string {
	t.Helper()

	reader, writer, err := os.Pipe()

	if err != nil {
		t.Fatalf("os.Pipe: %v", err)
	}

	saved := os.Stdout
	os.Stdout = writer

	func() {
		defer func() { os.Stdout = saved }()
		fn()
	}()

	writer.Close()
	captured, _ := io.ReadAll(reader)
	reader.Close()

	return string(captured)
}
