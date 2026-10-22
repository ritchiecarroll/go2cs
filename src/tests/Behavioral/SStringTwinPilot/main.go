package main

import (
	"errors"
	"fmt"
	"reflect"
	"runtime"
	"unicode/utf8"
)

// The sstring twin pilot (docs/phase4/DESIGN-sstring-twin-pilot.md): fmt's format functions and
// unicode/utf8's string readers are twinned. Every call form and value-site shape below must keep
// Go's behaviour, and a func value must keep Go's identity and name.

func apply(f func(string, ...any) string, format string) string { return f(format, "arg") }

func main() {
	// call forms: a literal, a variable, a string(b) conversion, a concatenation
	fmt.Println(fmt.Sprintf("xxx"))
	dynamic := "dyn %d"
	fmt.Println(fmt.Sprintf(dynamic, 1))
	b := []byte("from bytes %d")
	fmt.Println(fmt.Sprintf(string(b), 2))
	fmt.Println(fmt.Sprintf("con"+"cat %s", "ok"))
	fmt.Printf("printf %v %q\n", 3, "q")

	// value sites: a local, a map entry, a func-typed argument
	f := fmt.Sprintf
	fmt.Println(f("via local %s", "ok"))
	table := map[string]any{"printf": fmt.Sprintf}
	fmt.Println(table["printf"].(func(string, ...any) string)("via table %d", 4))
	fmt.Println(apply(fmt.Sprintf, "via argument %s"))

	// identity and name: one func value, whichever site takes it
	p1 := reflect.ValueOf(fmt.Sprintf).Pointer()
	p2 := reflect.ValueOf(f).Pointer()
	fmt.Println("same pointer:", p1 == p2)
	fmt.Println("name:", runtime.FuncForPC(p1).Name())

	// the other twins
	var n int
	count, err := fmt.Sscanf("42", "%d", &n)
	fmt.Println(count, err, n)
	fmt.Println(fmt.Errorf("wrapped: %w", errors.New("inner")))
	fmt.Println(string(fmt.Appendf(nil, "appended %d", 6)))
	fmt.Println(utf8.RuneCountInString("héllo, 世界"))
	r, size := utf8.DecodeRuneInString("é!")
	fmt.Println(r, size)
	// a string(b) argument at a utf8 twin reads the bytes in place (utf8.RuneCount's own non-ASCII path)
	raw := []byte("héllo, 世界")
	fmt.Println(utf8.RuneCountInString(string(raw)), utf8.RuneCount(raw[1:]))
	r2, size2 := utf8.DecodeRuneInString(string(raw[1:]))
	fmt.Println(r2, size2)
	// a deferred call's argument is evaluated now: the later write to raw must not reach it
	defer fmt.Println("deferred count", utf8.RuneCountInString(string(raw)))
	raw[1] = 'e'
	fmt.Println(utf8.RuneCountInString(string(raw)))
	fmt.Printf("%x %X %08b %5.2f|%-6s|%6s|\n", 255, "hi", 5, 3.14159, "left", "right")

	defer fmt.Printf("deferred %s\n", "printf")
	fmt.Println("done")
}
