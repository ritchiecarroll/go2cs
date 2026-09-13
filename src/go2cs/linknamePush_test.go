// linknamePush_test.go - Gbtc
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
	"runtime"
	"strings"
	"testing"
)

// TestRecurseLinknamePush guards the cross-package //go:linkname PUSH direction — the mirror of the
// PULL forwarder. A bodyless function under a one-arg `//go:linkname <thisFunc>` handle is a symbol
// ANOTHER package defines by naming it in a two-arg directive (runtime/mgc.go pushes its body into
// unique's `runtime_registerUniqueMapCleanup`). Before this the consumer got a throwing
// PartialStubGenerator stub and the pushed body was unreachable, which is what took `unique.Make`,
// net/netip's initializer and gob's TestNetIP down.
//
// Four behaviors are asserted, because the disposition is a JUDGMENT recorded per pair and every
// arm of it has to hold:
//
//   - a FORWARDED entry emits a body calling the pushing definition;
//   - the pushing definition is emitted `public` (it is called across an assembly boundary);
//   - an UNHONORABLE entry emits a panic naming BOTH halves of the pair and the reason, never a
//     fabricated body;
//   - a bodyless handle with no registry entry, and a registry entry whose declaration carries no
//     handle (Go's authorization for the push), both stay bodyless stubs.
//
// The registry is keyed by stdlib import paths, so the fixture's entries are injected for the
// duration of the test rather than parked in production. Transpile-only: the emitted C# is asserted
// as text, never compiled.
func TestRecurseLinknamePush(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real -recurse converter over a module fixture")
	}

	const (
		consumerPath = "example.com/lnpush"
		pusherPath   = "example.com/lnpush/pusher"
	)

	// Inject the fixture's push dispositions, then restore the registry so no other test (or a
	// later stdlib run in the same process) sees them.
	// The last three rows are the 2x2 over the consumer's SHAPE. `unauthorized` and `bare` have
	// identical syntax — a bodyless func with no directive at all — and differ only in what their
	// registry row records, so together they prove the recorded shape is what decides. `bareMismatch`
	// closes the other diagonal: a row that says bare will not forward a declaration carrying a handle.
	entries := map[string]linknamePush{
		consumerPath + ".pushed":       {source: pusherPath + ".push_pushed"},
		consumerPath + ".unhonorable":  {source: pusherPath + ".push_unhonorable", reason: "the fixture's pushed body has no managed form"},
		consumerPath + ".unauthorized": {source: pusherPath + ".push_unauthorized"},
		consumerPath + ".bare":         {source: pusherPath + ".push_bare", bareDecl: true},
		consumerPath + ".bareMismatch": {source: pusherPath + ".push_bareMismatch", bareDecl: true},
	}

	for key, push := range entries {
		linknamePushTargets[key] = push

		if push.reason == "" {
			linknamePushSources[push.source] = true
		}
	}

	defer func() {
		for key, push := range entries {
			delete(linknamePushTargets, key)
			delete(linknamePushSources, push.source)
		}
	}()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module "+consumerPath+"\n\ngo 1.23\n")

	// The consumer. `pushed` and `unhonorable` carry the one-arg handle and ARE in the registry;
	// `notPushed` carries the handle but is not; `unauthorized` is in the registry but never opened
	// itself to a push, so forwarding to it would not be faithful. `bare` is the standard library's
	// older push shape — bodyless with no directive at all, exactly `syscall.runtime_envs` — and its
	// row says so; `bareMismatch`'s row says bare but the declaration carries a handle.
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t_ \"unsafe\"\n\n\t_ \""+pusherPath+"\"\n)\n\n"+
			"//go:linkname pushed\nfunc pushed(x int) int\n\n"+
			"//go:linkname unhonorable\nfunc unhonorable(x int) int\n\n"+
			"//go:linkname notPushed\nfunc notPushed(x int) int\n\n"+
			"func unauthorized(x int) int\n\n"+
			"func bare(x int) int // in package pusher\n\n"+
			"//go:linkname bareMismatch\nfunc bareMismatch(x int) int\n\n"+
			"func main() {\n\tprintln(pushed(1), unhonorable(2), notPushed(3), unauthorized(4), bare(5), bareMismatch(6))\n}\n")

	// The pushing side: an ordinary body naming the consumer's symbol.
	writeModuleFile(t, filepath.Join(appDir, "pusher", "pusher.go"),
		"package pusher\n\nimport _ \"unsafe\"\n\n"+
			"//go:linkname push_pushed "+consumerPath+".pushed\nfunc push_pushed(x int) int { return x * 2 }\n\n"+
			"//go:linkname push_bare "+consumerPath+".bare\nfunc push_bare(x int) int { return x + 7 }\n")

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

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	outRoot := filepath.Join(options.go2csPath, "src", "example.com", "lnpush")
	mainCs := readGenerated(t, filepath.Join(outRoot, "main.cs"))
	pusherCs := readGenerated(t, filepath.Join(outRoot, "pusher", "pusher.cs"))

	// The FORWARDED entry becomes a body calling the pushing definition.
	if !strings.Contains(mainCs, ".push_pushed(x);") {
		t.Errorf("linkname push forwarder body missing its call to the pushing definition (emitted a stub?):\n%s", mainCs)
	}

	if strings.Contains(mainCs, "int pushed(nint x);") {
		t.Errorf("linkname push consumer emitted as a bodyless partial stub, not a forwarder:\n%s", mainCs)
	}

	// The pushing definition is reached across an assembly boundary, so it must be public even
	// though its Go name is unexported.
	if !strings.Contains(pusherCs, "public static nint push_pushed(nint x)") {
		t.Errorf("pushing definition not publicized for the cross-assembly forwarder:\n%s", pusherCs)
	}

	// The UNHONORABLE entry announces the pair and the reason instead of fabricating a body.
	if !strings.Contains(mainCs, `throw panic("go2cs: //go:linkname push `+pusherPath+`.push_unhonorable -> `+consumerPath+`.unhonorable is not honored: the fixture's pushed body has no managed form")`) {
		t.Errorf("unhonorable linkname push did not emit the pair-naming panic:\n%s", mainCs)
	}

	if strings.Contains(mainCs, ".push_unhonorable(") {
		t.Errorf("unhonorable linkname push was forwarded to the pushed body:\n%s", mainCs)
	}

	// A handle with no registry entry stays a bodyless stub (the pre-feature behavior): a bodyless
	// one-arg-handle func is indistinguishable from an ordinary assembly stub.
	if !strings.Contains(mainCs, "int notPushed(nint x);") {
		t.Errorf("un-registered linkname handle was not left as a bodyless stub:\n%s", mainCs)
	}

	// A registry entry whose declaration never carried the handle is NOT forwarded — a HANDLE row is
	// asserting Go opened the symbol that way, and this declaration did not.
	if !strings.Contains(mainCs, "int unauthorized(nint x);") || strings.Contains(mainCs, ".push_unauthorized(") {
		t.Errorf("linkname push forwarded to a declaration that carries no handle:\n%s", mainCs)
	}

	// The BARE shape — syntactically identical to `unauthorized`, and forwarded purely because its row
	// records that shape. This is the standard library's pre-handle push consumer (syscall.runtime_envs,
	// os.runtime_args), which took a throwing stub until the matcher learned the shape.
	if !strings.Contains(mainCs, ".push_bare(x);") {
		t.Errorf("bare-shape linkname push consumer was not forwarded to the pushing definition:\n%s", mainCs)
	}

	if strings.Contains(mainCs, "int bare(nint x);") {
		t.Errorf("bare-shape linkname push consumer emitted as a bodyless partial stub, not a forwarder:\n%s", mainCs)
	}

	if !strings.Contains(pusherCs, "public static nint push_bare(nint x)") {
		t.Errorf("bare-shape pushing definition not publicized for the cross-assembly forwarder:\n%s", pusherCs)
	}

	// The other diagonal: a row recording the bare shape must NOT forward a declaration that carries a
	// handle. The shape is half the judgment, so a mismatch fails closed rather than guessing.
	if !strings.Contains(mainCs, "int bareMismatch(nint x);") || strings.Contains(mainCs, ".push_bareMismatch(") {
		t.Errorf("bare-shape row forwarded a declaration that carries a handle:\n%s", mainCs)
	}
}
