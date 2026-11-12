// NilFuncNamedDelegateArg: a func value passed where Go expects a NAMED func type, including a nil value. Go's
// assignment of a func value to its own named func type (or a nil func value to any func type) never panics. The TS
// relater passes `core.IfElse(reportErrors, r.reportError, nil)` to an ErrorReporter parameter (TS C47).
package main

import "fmt"

type Message struct{ text string }

// ErrorReporter has no methods: it renders as the same C# delegate as its unnamed signature.
type ErrorReporter func(m *Message, args ...any)

// Handler has a method: it renders as a C# delegate type of its own.
type Handler func(n int) string

func (h Handler) Serve(n int) string { return "served " + h(n) }

func IfElse[T any](b bool, whenTrue T, whenFalse T) T {
	if b {
		return whenTrue
	}
	return whenFalse
}

type relater struct{ count int }

func (r *relater) reportError(m *Message, args ...any) {
	r.count++
	fmt.Println("report", m.text, len(args))
}

func check(label string, report ErrorReporter) {
	if report == nil {
		fmt.Println(label, "nil reporter")
		return
	}
	report(&Message{label}, 1, 2)
}

func serve(label string, h Handler) {
	if h == nil {
		fmt.Println(label, "nil handler")
		return
	}
	fmt.Println(label, h.Serve(7))
}

func plain(label string, f func(int) string) {
	if f == nil {
		fmt.Println(label, "nil func")
		return
	}
	fmt.Println(label, f(3))
}

func double(n int) string { return fmt.Sprint(n * 2) }

type holder struct {
	report ErrorReporter
	h      Handler
	f      func(int) string
}

func (x holder) describe(label string) {
	fmt.Println(label, x.report == nil, x.h == nil, x.f == nil)
}

func main() {
	r := &relater{}

	// Same C# delegate type: the generic helper's nil and non-nil results, a method value, a literal, nil locals.
	check("ifelse-nil", IfElse(false, r.reportError, nil))
	check("ifelse-set", IfElse(true, r.reportError, nil))
	check("method", r.reportError)
	check("literal", func(m *Message, args ...any) { fmt.Println("literal", m.text, len(args)) })
	var unnamed func(*Message, ...any)
	check("unnamed-nil", unnamed)
	var named ErrorReporter
	check("named-nil", named)

	// A distinct C# delegate type: a nil func value must still arrive as nil.
	var g func(int) string
	serve("handler-nil", g)
	serve("handler-func", double)
	serve("handler-literal", func(n int) string { return fmt.Sprint(n + 1) })
	g = double
	serve("handler-set", g)

	// The mirror: a named delegate value passed to a structural func parameter.
	var nilHandler Handler
	plain("plain-nil", nilHandler)
	plain("plain-set", Handler(double))

	// Composite-literal fields take the same rules.
	var nilFunc func(int) string
	holder{report: IfElse(false, r.reportError, nil), h: nilFunc, f: nilHandler}.describe("fields-nil")
	holder{report: IfElse(true, r.reportError, nil), h: double, f: Handler(double)}.describe("fields-set")

	fmt.Println("reports", r.count)
}
