// parityTrainSmallFour_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// Seat #27 of the visual-parity train (docs/PLAN-parity-train.md), four small emission shapes:
//
//   - F1: an immediately-invoked function literal reads like Go's `func() int {…}()`: `func(nint () => {…})()`,
//     the result type where Go writes it and the parameters on the lambda, through golib's `func` overloads
//     (COORD's ruling of option A). It was a cast-and-call, `((Func<nint>)(() => {…}))()`.
//   - WithDirection: a channel narrowed to a directional type re-stamps through `ch.AsRecvOnly()` / `ch.AsSendOnly()`
//     rather than `ch.WithDirection(GoChanDir.Recv)`.
//   - for (;;): Go's `for {` emits `for (;;)`, and a three-clause loop without a condition `for (init; ; post)`,
//     where both wrote golib's constant-true `ᐧ`.
//   - Shift-assign: a compound `<<=`/`>>=` whose count is an integer literal drops the `(int)(N)` cast. A
//     non-constant count keeps its guarded `LshAssign`/`RshAssign` form, and a named-numeric left operand keeps
//     its cast for the generated count overloads.

const smallFourFixture = `package main

import "fmt"

type count uint8

type holder struct{ bits uint64 }

func narrow(ch chan int) (<-chan int, chan<- int) {
	var r <-chan int = ch
	var s chan<- int = ch
	return r, s
}

func loops() int {
	i := 0
	for {
		i++
		if i > 3 {
			break
		}
	}
	for j := 0; ; j++ {
		if j > 2 {
			return i + j
		}
	}
}

func shifts(x uint64, n uint, c count, h *holder) (uint64, count, uint64) {
	x <<= 3
	x >>= 2
	x <<= n
	h.bits <<= 4
	h.bits >>= n
	c <<= 1
	return x, c, h.bits
}

func pick[T any](v T) T {
	return func() T { return v }()
}

func plusOne() int {
	return func() int { return 1 }() + 1
}

func main() {
	v := func() int { return 42 }()
	func() { fmt.Println("void") }()
	w := func(a int) int { return a * 2 }(5)
	s, err := func() (string, error) { return "s", nil }()
	fmt.Println(v, w, s, err, pick(7), plusOne(), loops())
	fmt.Printf("%T\n", func() int32 { return 1 }())

	ch := make(chan int, 1)
	r, snd := narrow(ch)
	snd <- 9
	fmt.Println(<-r)

	h := &holder{bits: 1}
	fmt.Println(shifts(1, 1, 1, h))
}
`

func convertSmallFourFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/smallfour\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), smallFourFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "smallfour", "main.cs"))
}

func TestSmallFourEmission(t *testing.T) {
	isolateConverterPathGlobals(t)
	mainCs := convertSmallFourFixture(t)

	wants := map[string][]string{
		"F1": {
			`nint v = func(nint () => `,
			`func(() => `,
			`nint w = func(nint (nint a) => `,
			`func((@string, error) () => `,
			`func(T () => `,
			`func(int32 () => `,
		},
		"direction": {`ch.AsRecvOnly()`, `ch.AsSendOnly()`},
		"loops":     {`for (;;) {`, `for (nint j = 0; ; j++) {`},
		"shifts":    {`x <<= 3;`, `x >>= 2;`, `x.LshAssign(n);`, `h.bits <<= 4;`, `h.bits.RshAssign(n);`, `c <<= (int)(1);`},
	}

	for item, fragments := range wants {
		for _, want := range fragments {
			if !strings.Contains(mainCs, want) {
				t.Errorf("%s: no emitted line carries %q", item, want)
			}
		}
	}

	refusals := map[string]*regexp.Regexp{
		"F1":        regexp.MustCompile(`\(\((Action|Func)\b[^)]*\)\(`),
		"direction": regexp.MustCompile(`WithDirection\(`),
		"loops":     regexp.MustCompile(`while \(ᐧ\)|; ᐧ ?;`),
		"shifts":    regexp.MustCompile(`(x|h\.bits) (<<|>>)= \(int\)\(\d+\)`),
	}

	for item, refusal := range refusals {
		if found := refusal.FindString(mainCs); found != "" {
			t.Errorf("%s: the old shape is still emitted: %q", item, found)
		}
	}

	if t.Failed() {
		t.Logf("emitted main.cs:\n%s", mainCs)
	}
}
