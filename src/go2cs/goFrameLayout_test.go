// goFrameLayout_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the frame LAYOUT of a deferring function (goFrameOperations.go): the frame opens on the
// signature line, the body sits at Go's indentation, and the catch, the finally, the named-result
// exit and the method's own closing brace share one closing line. The shapes are the defer-parity
// scout's five (docs/phase4/SCOUT-defer-emission-parity.md, section 11) plus the frame's other
// parts: an unnamed result's catch return, a defer lowered into the finally (its flag line led by the Go
// defer's own text, the owner's form), a named-result
// literal nested in a frame, and a literal passed as an argument.

package main

import (
	"regexp"
	"strings"
	"testing"
)

const goFrameLayoutSource = `package main

import (
	"errors"
	"fmt"
	"sync"
)

// try is the plain defer and recover shape.
func try(name string, register func()) { // try's trailing comment
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: panic: %v\n", name, r)
		}
	}()
	register()
	fmt.Printf("%s: registered\n", name)
}

type writer struct{ n int }

func (f *writer) Write(p []byte) (int, error) { f.n += len(p); return len(p), nil }

type pooled struct {
	mu sync.Mutex
	fw *writer
}

// Write is the deferred Unlock in a pointer-receiver method, with named results and an early return.
func (w *pooled) Write(p []byte) (n int, err error) {
	w.mu.Lock()
	defer w.mu.Unlock()
	if w.fw == nil {
		return 0, errors.New("Write after Close")
	}
	return w.fw.Write(p)
}

// loop is a defer inside a loop.
func loop() {
	for i := 0; i < 3; i++ {
		defer func() { fmt.Println("closure", i) }()
	}
}

// guarded is a defer that writes the named results.
func guarded(boom bool) (code int, msg string) {
	defer func() {
		if r := recover(); r != nil {
			code = -1
			msg = "recovered"
		}
	}()
	if boom {
		panic("kaboom")
	}
	code = 0
	msg = "ok"
	return code, msg
}

type elem struct{ x uint64 }

func consume(p *uint64, marker uint64) { *p += marker }

func eagerFrom(e *elem) { consume(&e.x, 1) }

// tryEager is a ref-lowered parameter plus a defer.
func tryEager(label string, e *elem) {
	defer func() { fmt.Println(label, "recovered:", recover()) }()
	eagerFrom(e)
	fmt.Println(label, "no panic")
}

// value is an unnamed result: the catch arm returns the zero result.
func value() int {
	defer fmt.Println("value")
	return 7
}

type counter struct {
	mu sync.Mutex
	n  int
}

// inc is a defer lowered into the finally (a reached flag before the try, the call in the finally).
func (c *counter) inc() {
	c.mu.Lock()
	defer c.mu.Unlock()
	c.n++
}

// dec's lowered defer carries the user's trailing comment after the Go defer's own text.
func (c *counter) dec() {
	c.mu.Lock()
	defer c.mu.Unlock() // released on every return
	c.n--
}

// literals holds function-literal frames: a named-result literal nested in a deferring function.
func literals() (total int) {
	defer func() { total++ }()
	f := func() (n int) {
		defer func() { n *= 2 }()
		if total > 100 {
			return -1
		}
		n = 3
		return n
	}
	total = f()
	return total
}

func run(f func()) { f() }

// argument passes a deferring literal as an argument: a lambda frame.
func argument() {
	run(func() {
		defer fmt.Println("argument")
		fmt.Println("body")
	})
}

func main() {
	try("ok", func() {})
	w := &pooled{fw: &writer{}}
	fmt.Println(w.Write([]byte("abc")))
	loop()
	fmt.Println(guarded(true))
	tryEager("live", &elem{})
	fmt.Println(value())
	c := &counter{}
	c.inc()
	fmt.Println(literals())
	argument()
}
`

// goFrameTailText is the frame's catch on the closing line, up to the catch body's end: the frame-local tail,
// where the filter stores the panic in the frame and the catch body parks it (GoFrame.Is, GoFrame.Capture).
const goFrameTailText = "catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture();"

var goFrameLayoutName = regexp.MustCompile(`([@\w]+)\(`)

// goFrameLayoutBodies converts goFrameLayoutSource and returns each top-level function, keyed by name,
// from its signature line through the first line at column 0 that closes it ("}" or "} catch …").
func goFrameLayoutBodies(t *testing.T) map[string]string {
	t.Helper()
	isolateConverterPathGlobals(t)

	lines := convertWithComments(t, goFrameLayoutSource)
	bodies := map[string]string{}

	for i := 0; i < len(lines); i++ {
		line := lines[i]

		if !strings.HasPrefix(line, "internal static ") {
			continue
		}

		// The name is the first identifier written against its `(`: a tuple result type is preceded
		// by a space (`static (nint n, error err) Write(`), so it never matches.
		match := goFrameLayoutName.FindStringSubmatch(line)

		if match == nil || !strings.Contains(line, "{") {
			continue
		}

		name := match[1]
		end := i

		// A one-line method closes on its own line.
		if !strings.HasSuffix(strings.TrimSpace(line), "}") {
			for end+1 < len(lines) && !strings.HasPrefix(lines[end+1], "}") {
				end++
			}

			end++
		}

		bodies[name] = strings.Join(lines[i:end+1], "\n")
		i = end
	}

	return bodies
}

// The frame opens on the signature line and closes on one line; every body line is at Go's indentation.
func TestGoFrameLayoutOpensOnTheSignatureAndClosesOnOneLine(t *testing.T) {
	tail := func(catchReturn, finally, exit string) string {
		return "} " + goFrameTailText + catchReturn + " } finally { " + finally + "ᒐ.Run(); }" + exit + " }"
	}

	want := map[string]string{
		"@try": `internal static void @try(@string name, Action register) { GoFrame ᒐ = default; try {
    // try's trailing comment
    defer(() => {
        var r = recover(); if (r != nil) {
            fmt.Printf("%s: panic: %v\n"u8, name, r);
        }
    }, ref ᒐ);
    register();
    fmt.Printf("%s: registered\n"u8, name);
` + tail("", "", ""),
		"Write": `internal static (nint n, error err) Write(this ж<pooled> Ꮡw, slice<byte> p) { nint n = default; error err = default; GoFrame ᒐ = default; try {
    ref var w = ref Ꮡw.DerefOrNull();

    Ꮡw.Ꮡmu.Lock();
    defer(Ꮡw.Ꮡmu.Unlock, ref ᒐ);
    if (w.fw == nil) {
        (n, err) = (0, errors.New("Write after Close")); goto ᒐdone;
    }
    (n, err) = w.fw.Write(p);
` + tail("", "", " ᒐdone: return (n, err);"),
		"loop": `internal static void loop() { GoFrame ᒐ = default; try {
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        defer(() => {
            fmt.Println("closure", i);
        }, ref ᒐ);
    }
` + tail("", "", ""),
		"guarded": `internal static (nint code, @string msg) guarded(bool boom) { nint code = default; @string msg = default; GoFrame ᒐ = default; try {
    defer(() => {
        var r = recover(); if (r != nil) {
            code = -1;
            msg = "recovered";
        }
    }, ref ᒐ);
    if (boom) {
        throw panic("kaboom");
    }
    code = 0;
    msg = "ok";
` + tail("", "", " return (code, msg);"),
		"tryEager": `internal static void tryEager(@string label, ref elem e) { GoFrame ᒐ = default; try {
    defer(() => {
        fmt.Println(label, "recovered:", recover());
    }, ref ᒐ);
    eagerFrom(ref e);
    fmt.Println(label, "no panic");
` + tail("", "", ""),
		"value": `internal static nint value() { GoFrame ᒐ = default; try {
    defer(() => fmt.Println("value"), ref ᒐ);
    return 7;
` + tail(" return default;", "", ""),
		"inc": `internal static void inc(this ж<counter> Ꮡc) { GoFrame ᒐ = default; bool ᒐd1 = false; try {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.Ꮡmu.Lock();
    /* defer c.mu.Unlock() */ ᒐd1 = true;
    c.n++;
` + tail("", "if (ᒐd1) Ꮡc.Ꮡmu.Unlock(); ", ""),
		"dec": `internal static void dec(this ж<counter> Ꮡc) { GoFrame ᒐ = default; bool ᒐd1 = false; try {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.Ꮡmu.Lock();
    /* defer c.mu.Unlock() */ ᒐd1 = true; // released on every return
    c.n--;
` + tail("", "if (ᒐd1) Ꮡc.Ꮡmu.Unlock(); ", ""),
		"literals": `internal static nint /*total*/ literals() { nint total = default; GoFrame ᒐ = default; try {
    defer(() => {
        total++;
    }, ref ᒐ);
    nint /*n*/ f() { nint n = default; GoFrame ᒐ = default; try {
        defer(() => {
            n *= 2;
        }, ref ᒐ);
        if (total > 100) {
            n = -1; goto ᒐdone1;
        }
        n = 3;
        goto ᒐdone1;
    ` + tail("", "", " ᒐdone1: return n;") + `
    total = f();
` + tail("", "", " return total;"),
		"argument": `internal static void argument() {
    run(() => { GoFrame ᒐ = default; try {
        defer(() => fmt.Println("argument"), ref ᒐ);
        fmt.Println("body");
    ` + tail("", "", "") + `);
}`,
	}

	got := goFrameLayoutBodies(t)

	for name, body := range want {
		if got[name] != body {
			t.Errorf("%s emitted\n%s\nwant\n%s", name, got[name], body)
		}
	}
}

// The owner's amendment (COORD 2026-11-13 06:30Z): a LOWERED defer's flag line carries the Go defer statement as a
// leading block comment, so it reads where Go wrote it. A text holding "*/" would end the block comment early, so it
// takes a trailing line comment instead. Today's gates admit no argument, so no lowered defer can carry one; the
// helper is guarded directly.
func TestLoweredDeferFlagLineCarriesTheGoDefer(t *testing.T) {
	for _, c := range []struct{ text, want string }{
		{"defer c.mu.Unlock()", "/* defer c.mu.Unlock() */ ᒐd1 = true;"},
		{"defer c.log(\"*/\")", "ᒐd1 = true; // defer c.log(\"*/\")"},
	} {
		if got := loweredDeferFlagLine("ᒐd1", c.text); got != c.want {
			t.Errorf("loweredDeferFlagLine(%q) = %q, want %q", c.text, got, c.want)
		}
	}
}
