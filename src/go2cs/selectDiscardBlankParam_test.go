// selectDiscardBlankParam_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a select case that receives and drops the value (`case <-ch:`) inside a function whose
// lone blank parameter would otherwise stay `_`. The case is emitted `selᴛN.ꟷᐳ(out _)`, and C#
// reads a bare `_` as a discard only while no variable named `_` is in scope: a parameter named `_`
// takes the `out` instead. net/http's transport_dial_test closure `func(_ context.Context, network,
// address string)` failed that way at go1.26 (CS1503, `out Context` to `out EmptyStruct`), which
// kept every net/http test from building. bodyUsesBlankDiscard now counts the receive, so the
// parameter is renamed `_ΔpN`, as a `_ = …` in the body already did.

package main

import (
	"strings"
	"testing"
)

const selectDiscardBlankParamSource = `package main

import (
	"context"
	"fmt"
)

func closure(ctx context.Context) string {
	f := func(_ context.Context, name string) string {
		done := make(chan struct{})
		close(done)
		select {
		case <-done:
			return "done " + name
		}
	}
	return f(ctx, "lit")
}

func named(_ context.Context, ch chan int) int {
	select {
	case <-ch:
		return 1
	}
}

func noDiscard(_ context.Context) int {
	return 2
}

func bound(_ context.Context, ch chan int) int {
	select {
	case v := <-ch:
		return v
	}
}

func main() {
	ch := make(chan int, 2)
	ch <- 1
	ch <- 3
	fmt.Println(closure(context.Background()), named(context.Background(), ch), noDiscard(nil), bound(nil, ch))
}
`

func TestSelectReceiveDiscardRenamesTheBlankParam(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithoutComments(t, selectDiscardBlankParamSource)
	emitted := strings.Join(lines, "\n")

	for _, c := range []struct {
		name, want, not string
	}{
		{"the closure's blank parameter", "@string f(context.Context _Δp0, @string name)", "@string f(context.Context _, @string name)"},
		{"the named function's blank parameter", "named(context.Context _Δp0, channel<nint> ch)", "named(context.Context _, channel<nint> ch)"},
	} {
		if !strings.Contains(emitted, c.want) {
			t.Errorf("%s: want %q", c.name, c.want)
		}

		if strings.Contains(emitted, c.not) {
			t.Errorf("%s: still %q, so the select's `out _` binds to it", c.name, c.not)
		}
	}

	// The discard itself is unchanged: the parameter moves out of its way.
	if got := strings.Count(emitted, ".ꟷᐳ(out _)"); got != 2 {
		t.Errorf("want 2 `.ꟷᐳ(out _)` discards, got %d", got)
	}

	// Controls: with no discard in the body, a lone blank parameter keeps the Go-like `_`.
	for _, want := range []string{"noDiscard(context.Context _)", "bound(context.Context _, channel<nint> ch)"} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want %q (no discard in the body, so `_` stays)", want)
		}
	}

	if t.Failed() {
		t.Logf("emitted:\n%s", emitted)
	}
}
