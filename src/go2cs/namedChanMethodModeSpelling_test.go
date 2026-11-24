// namedChanMethodModeSpelling_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertNamedChanShadowFixture converts behavioral NamedChanMapNumericShadow in the given channel mode and returns
// the emitted main.cs.
func convertNamedChanShadowFixture(t *testing.T, useChannelOperators bool) string {
	t.Helper()

	data, err := os.ReadFile(filepath.Join("..", "tests", "Behavioral", "NamedChanMapNumericShadow", "main.go"))

	if err != nil {
		t.Fatalf("read the fixture: %v", err)
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/chanshadow\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), string(data))

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
		useChannelOperators: useChannelOperators,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "chanshadow", "main.cs"))
}

// TestNamedChanMethodModeSpelling pins R7 seat 2's converter half. Named-method mode (-uco=false) spells a channel
// operation by member name -- Send, Receive, Received, Receiving, Sending -- and a named channel whose Go type declares
// a method of one of those names has that member moved out of the way by go2cs-gen (an explicit IChannel<T>
// implementation or a Δ name), so `p.Send(v)` would bind the Go method's extension instead. Such a channel takes the
// operator glyphs, which are the wrapper's own whatever its method set; every other channel keeps the named members.
func TestNamedChanMethodModeSpelling(t *testing.T) {
	isolateConverterPathGlobals(t)

	named := convertNamedChanShadowFixture(t, false)

	// Pipe claims all six names: its send, receive, select registrations and select tests take the glyphs.
	for _, want := range []string{
		"p." + ChannelLeftOp + "(1);",
		"q." + ChannelLeftOp + "(10);",
		ChannelLeftOp + "<nint>(p)",
		"." + ChannelRightOp + "(out ",
	} {
		if !strings.Contains(named, want) {
			t.Errorf("named-method mode: a claiming channel's emission lacks %q", want)
		}
	}

	// The operation sites only: the program also calls Pipe's own Go methods by these names (`p.Send(5)`,
	// `p.Receive()`, `p.Receiving()`, `p.Sending(4)`), which must stay exactly as written.
	for _, refuse := range []string{"p.Send(1);", "p.Send(2);", "q.Send(10);", "p.Sending(9)", "ᴛ1.Receiving", "ᴛ2.Receiving", ".Received(out var"} {
		if strings.Contains(named, refuse) {
			t.Errorf("named-method mode: the emission holds %q, a member the claiming channel Pipe has moved", refuse)
		}
	}

	if got := strings.Count(named, "p.Receive()"); got != 1 {
		t.Errorf("named-method mode: %d occurrences of p.Receive(), want 1 (the Go method call; `<-p` takes the glyph)", got)
	}

	// A channel type NAMED like one of those members claims it too (go2cs-gen's GoMethodNames carries the type's own name):
	// `type Receive chan int` declares ΔReceive and `type Send chan int` declares Send explicitly only, so both take the
	// glyphs (P3's SHOULD 3).
	for _, want := range []string{"rc." + ChannelLeftOp + "(5);", "sc." + ChannelLeftOp + "(", ChannelLeftOp + "<nint>(rc)"} {
		if !strings.Contains(named, want) {
			t.Errorf("named-method mode: a member-named channel's emission lacks %q", want)
		}
	}

	for _, refuse := range []string{"rc.Send(", "rc.Receive()", "sc.Send(", "rcᴛ1.Receiving"} {
		if strings.Contains(named, refuse) {
			t.Errorf("named-method mode: the emission holds %q, a member the member-named channel has moved", refuse)
		}
	}

	// The near-miss channel Wire and the member-named Capacity declare none of those names: they keep the named
	// members, so the mode is still read, and the guard above is not vacuous.
	for _, want := range []string{"w.Send(1);", "w.Receive()", "cc.Send(\"c\");", "cc.Receive()"} {
		if !strings.Contains(named, want) {
			t.Errorf("named-method mode: a non-claiming channel's emission lacks %q", want)
		}
	}

	// The default operator mode spells every channel with the glyphs, the claiming one included.
	operators := convertNamedChanShadowFixture(t, true)

	for _, refuse := range []string{"w.Send(", "w.Receive(", "cc.Send(", "cc.Receive(", "p.Send(1);", "ᴛ1.Receiving", ".Received(out var"} {
		if strings.Contains(operators, refuse) {
			t.Errorf("operator mode: the emission holds %q", refuse)
		}
	}

	if !strings.Contains(operators, "w."+ChannelLeftOp+"(1);") {
		t.Errorf("operator mode: the near-miss channel's send lacks the glyph")
	}
}
