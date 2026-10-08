// sstringTwin_test.go - Gbtc
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

const sstringTwinFixture = `package twin

// Format is twinned on its format parameter.
func Format(format string, a ...any) string { return format + "!" }

// Count ranges over its twinned parameter.
func Count(s string) int {
	n := 0
	for range s {
		n++
	}
	return n
}

// Mixed twins its second parameter only.
func Mixed(prefix, s string) string { return prefix + s }

// Plain is not registered.
func Plain(s string) string { return s }

type buf []byte

// write is a twinned pointer-receiver method (a [GoRecv] pair).
func (b *buf) write(s string) { *b = append(*b, s...) }

// The value-site shapes: a map entry, a typed package var, a typed local, a func-typed parameter.
var table = map[string]any{"format": Format}

var F = Format

func apply(f func(string, ...any) string) string { return f("v") }

func use() string {
	f := Format
	var b buf
	b.write("x")
	defer b.write("deferred")
	defer Count("deferred")
	go Count("go")
	return f("x") + apply(Format) + Plain("p") + Mixed("a", "b")
}
`

// A CONSUMER package: it reads the twins' published records, and its package-qualified value sites
// include an assignment's right-hand side, which convSelectorExpr's early package-selector arm used
// to emit as the method group (fmt_test's `noVetErrorf := fmt.Errorf`, runtime_test's
// `runtime.FmtSprintf = fmt.Sprintf`).
const sstringTwinConsumerFixture = `package use

import "example.com/twin"

var G = twin.Format

func Get() func(string, ...any) string {
	f := twin.Format
	return f
}

func Call() string { return twin.Format("direct %d", 1) + twin.Plain("p") }
`

// sstringTwinKeys registers the fixture's twins for the length of one test, the way the pilot's
// registry lists fmt's (docs/phase4/DESIGN-sstring-twin-pilot.md §3.1).
func registerFixtureTwins(t *testing.T) {
	t.Helper()

	entries := map[string][]int{
		"example.com/twin.Format":    {0},
		"example.com/twin.Count":     {0},
		"example.com/twin.Mixed":     {1},
		"example.com/twin.buf.write": {0},
	}

	for key, indices := range entries {
		sstringTwins[key] = indices
	}

	t.Cleanup(func() {
		for key := range entries {
			delete(sstringTwins, key)
		}
	})
}

func convertTwinFixture(t *testing.T) (emitted string, packageInfo string) {
	emitted, packageInfo, _ = convertTwinFixtures(t)
	return emitted, packageInfo
}

func convertTwinFixtures(t *testing.T) (emitted string, packageInfo string, consumer string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/twin\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "twin.go"), sstringTwinFixture)
	writeModuleFile(t, filepath.Join(appDir, "use", "use.go"), sstringTwinConsumerFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	pkgDir := filepath.Join(options.go2csPath, "src", "example.com", "twin")

	emitted = strings.ReplaceAll(readGenerated(t, filepath.Join(pkgDir, "twin.cs")), "\r\n", "\n")
	packageInfo = strings.ReplaceAll(readGenerated(t, filepath.Join(pkgDir, "package_info.cs")), "\r\n", "\n")
	consumer = strings.ReplaceAll(readGenerated(t, filepath.Join(pkgDir, "use", "use.cs")), "\r\n", "\n")

	return emitted, packageInfo, consumer
}

// TestSStringTwinEmission controls the twin's declaring side, value sites, defer/go form and record
// both ways. RED at claude/c2-arm-c 2d068bca59, where no twin was emitted, and at
// claude/c2-sstring-twin-pilot 5b691c9c01, where the converter emitted the companions itself.
func TestSStringTwinEmission(t *testing.T) {
	registerFixtureTwins(t)

	emitted, packageInfo, consumer := convertTwinFixtures(t)

	// the consumer package: every package-qualified value site names the canonical delegate, and a
	// call keeps the method (the priority binds the twin)
	for _, want := range []string{"G = twin.Formatᶠ;", "f = twin.Formatᶠ;", "twin.Format(directDˢ, (nint)(1))", "twin.Plain(\"p\"u8)"} {
		if !strings.Contains(consumer, want) {
			t.Errorf("consumer: want %q in:\n%s", want, consumer)
		}
	}

	for _, want := range []string{
		// the sstring member: the body and the retyped registered parameter, with no mark: go2cs-gen
		// selects it by its sstring parameter (face lift S, docs/PLAN-marker-comment-parity.md, 5.7)
		"\npublic static @string Format(sstring format, params ꓸꓸꓸany aʗp) {",
		"\npublic static nint Count(sstring s) {",
		"\npublic static @string Mixed(@string prefix, sstring s) {",
		"\ninternal static void write(this ref buf b, sstring s) {",

		// the sstring gaps the pilot closes: range and spread over the view
		"foreach ((_, _) in s) {",
		"b = append(b, s.ꓸꓸꓸ);",
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want %q in:\n%s", want, emitted)
		}
	}

	// the companions are go2cs-gen's (StrGenerator): the converter emits neither the @string
	// forwarder nor the canonical delegate, and no priority, so the visible file keeps one method per
	// Go function. RED at claude/c2-sstring-twin-pilot 5b691c9c01, which emitted all three inline.
	for _, refuse := range []string{"GoStr", "OverloadResolutionPriority", "GoTwinForwarder", "static readonly Funcꓸꓸꓸ<@string, any, @string> Formatᶠ", "Countᶠ =", "Count(@string s)", "write(this ref buf b, @string s)"} {
		if strings.Contains(emitted, refuse) {
			t.Errorf("the converter must not emit a twin companion (%q):\n%s", refuse, emitted)
		}
	}

	// every value site names the canonical delegate, never the method group (probe e/e4: CS0123)
	for _, want := range []string{
		"= Formatᶠ;",     // the typed package var and the typed local
		"(Formatᶠ)",      // the map entry's delegate cast
		"apply(Formatᶠ)", // a func-typed argument
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want the value site %q in:\n%s", want, emitted)
		}
	}

	// a deferred or go'd twin takes the lambda form (a method group would be CS0123)
	body := liftFunctionBody(t, emitted, "internal static partial @string use(") // use executes a go: a no-inline carrier

	for _, want := range []string{"defer(ᴛ1 => Ꮡb.ValueSlot.write(ᴛ1),", "defer(ᴛ1 => Count(ᴛ1),", "goǃ(ᴛ1 => Count(ᴛ1),"} {
		if !strings.Contains(body, want) {
			t.Errorf("use: want the lambda form %q:\n%s", want, body)
		}
	}

	for _, refuse := range []string{"defer(Ꮡb.write,", "defer(Count,", "goǃ(Count,"} {
		if strings.Contains(body, refuse) {
			t.Errorf("use: a twin must not be deferred as a method group (%q):\n%s", refuse, body)
		}
	}

	// an unregistered function and an unregistered parameter keep today's emission
	if strings.Contains(emitted, "Plain(sstring") || strings.Contains(emitted, "Plainᶠ") || strings.Contains(emitted, "Mixedᶠ") && !strings.Contains(emitted, "Mixed(@string prefix, sstring s)") {
		t.Errorf("an unregistered function or parameter must be untouched:\n%s", emitted)
	}

	// exported package-level twins are published; the method and nothing unexported is
	for _, want := range []string{`[assembly: GoSStringTwin("Count")]`, `[assembly: GoSStringTwin("Format")]`, `[assembly: GoSStringTwin("Mixed")]`} {
		if !strings.Contains(packageInfo, want) {
			t.Errorf("want the record %q in package_info.cs:\n%s", want, packageInfo)
		}
	}

	if strings.Contains(packageInfo, `GoSStringTwin("write")`) || strings.Contains(packageInfo, `GoSStringTwin("Plain")`) {
		t.Errorf("only exported package-level twins are published:\n%s", packageInfo)
	}
}

// TestSStringTwinSectionAbsentWhenEmpty: a package with no twin carries no trace of the record section.
func TestSStringTwinSectionAbsentWhenEmpty(t *testing.T) {
	_, packageInfo := convertTwinFixture(t)

	if strings.Contains(packageInfo, sstringTwinSectionStart) || strings.Contains(packageInfo, "GoSStringTwin") {
		t.Errorf("no registered twin, so no section:\n%s", packageInfo)
	}
}
