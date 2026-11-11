// A pointer type argument against a NON-self-referential method-set constraint (TS readiness C35, tsc's
// internal/tsoptions convertMapToOptions[O optionParser]). Go instantiates O as the pointer: the result IS the caller's
// pointer, a write through it is a write through the original, and the zero O is a nil pointer.
package main

import (
	"errors"
	"fmt"
	"reflect"
	"strings"
)

type optionParser interface {
	ParseOption(key string, value any) bool
	UnknownOptionDiagnostic() string
}

type compilerOptionsParser struct {
	seen []string
}

func (p *compilerOptionsParser) ParseOption(key string, value any) bool {
	p.seen = append(p.seen, fmt.Sprintf("%s=%v", key, value))
	return key != "bad"
}

func (p *compilerOptionsParser) UnknownOptionDiagnostic() string { return "unknown compiler option" }

func convertMapToOptions[O optionParser](m map[string]any, keys []string, parser O) O {
	for _, k := range keys {
		if !parser.ParseOption(k, m[k]) {
			fmt.Println(parser.UnknownOptionDiagnostic(), k)
		}
	}
	return parser
}

// zeroOr returns the parser, or the zero O (a nil pointer), which is returned and compared, never called.
func zeroOr[O optionParser](ok bool, parser O) O {
	var zero O
	if ok {
		return parser
	}
	return zero
}

// parse returns (O, error).
func parse[O optionParser](parser O, key string) (O, error) {
	if !parser.ParseOption(key, 1) {
		return parser, errors.New(parser.UnknownOptionDiagnostic())
	}
	return parser, nil
}

// each hands the parser to a func(O) callback.
func each[O optionParser](parser O, keys []string, f func(O, string)) {
	for _, k := range keys {
		f(parser, k)
	}
}

// A FOREIGN constraint, and one that EMBEDS another interface.
type label struct{ name string }

func (l *label) String() string { return "label:" + l.name }
func (l *label) Name() string   { return l.name }

type named interface {
	fmt.Stringer
	Name() string
}

func show[S fmt.Stringer](s S) S { fmt.Println("show:", s.String()); return s }

func describe[N named](n N) N { fmt.Println("describe:", n.String(), n.Name()); return n }

// A VALUE-receiver method satisfies the constraint through *T too.
type temp struct{ c int }

func (t temp) String() string { return fmt.Sprintf("%dC", t.c) }

// The value-argument control: no pointer, no proxy.
type plain struct{ k string }

func (p plain) ParseOption(key string, value any) bool { return key != p.k }
func (p plain) UnknownOptionDiagnostic() string        { return "plain refuses " + p.k }

// The SELF-REFERENTIAL route's result: reading a field of it.
type Cloner[T any] interface{ Clone() T }

type impl struct{ n int }

func (p *impl) Clone() *impl { return &impl{p.n + 1} }

func dup[T Cloner[T]](x T) T { return x.Clone() }

func main() {
	original := &compilerOptionsParser{}
	p := convertMapToOptions(map[string]any{"strict": true, "bad": 1}, []string{"strict", "bad"}, original)
	fmt.Println("result fields:", p.seen, len(p.seen))

	// The result is the caller's pointer: same identity, and a write through it is seen through the original.
	p.seen = append(p.seen, "written-through-result")
	fmt.Println("identity:", p == original, strings.Join(original.seen, ","))

	// The zero O is a nil pointer.
	fmt.Println("zero:", zeroOr(false, original) == nil, zeroOr(true, original) == original)

	// (O, error), and a discarded result.
	q, err := parse(original, "bad")
	fmt.Println("tuple:", q == original, err)
	_, err = parse(original, "ok")
	fmt.Println("tuple ok:", err == nil, len(original.seen))
	convertMapToOptions(nil, []string{"discarded"}, original)
	fmt.Println("discarded:", original.seen[len(original.seen)-1])

	// A func(O) callback receives the pointer itself.
	each(original, []string{"x", "y"}, func(o *compilerOptionsParser, k string) { o.seen = append(o.seen, "cb-"+k) })
	fmt.Println("callback:", original.seen[len(original.seen)-2:])

	// reflect and a type switch see *compilerOptionsParser.
	var asAny any = convertMapToOptions(nil, nil, original)
	fmt.Println("reflect:", reflect.TypeOf(asAny))
	switch v := asAny.(type) {
	case *compilerOptionsParser:
		fmt.Println("switch: *compilerOptionsParser", v == original)
	default:
		fmt.Println("switch: other")
	}

	// Foreign and embedded constraints.
	l := &label{name: "a"}
	fmt.Println("foreign:", show(l).name, show(l) == l)
	fmt.Println("embedded:", describe(l).name)

	// A value-receiver method, satisfied through *T.
	t := &temp{c: 21}
	show(t).c++
	fmt.Println("value receiver:", t.c)

	// The control: a value type argument.
	fmt.Println("value arg:", convertMapToOptions(map[string]any{"k": 1}, []string{"k"}, plain{k: "k"}).k)

	// The self-referential route: a field of the result.
	fmt.Println("self route:", dup(&impl{n: 1}).n)
}

// COORD's proxy-RESULT repro (00:31Z), on the SELF-referential route: a generic result typed as the proxy made a field
// read on it CS1929. Each Go position of the result: `:=` with an explicit instantiation, `var x =`, a multi-value
// result, a field read on the call, a non-generic parameter, a return, and a write through the result.
type Chain[T any] interface {
	Next() T
	IsEnd() bool
}

type Node struct {
	Name string
	next *Node
}

func (n *Node) Next() *Node { return n.next }
func (n *Node) IsEnd() bool { return n.next == nil }

func Last[N Chain[N]](n N) N {
	for !n.IsEnd() {
		n = n.Next()
	}
	return n
}

func LastAndDepth[N Chain[N]](n N) (N, int) {
	d := 0
	for !n.IsEnd() {
		n = n.Next()
		d++
	}
	return n, d
}

func describeNode(n *Node) string { return "node " + n.Name }

func lastOf(a *Node) *Node { return Last(a) }

func init() {
	c := &Node{Name: "c"}
	a := &Node{Name: "a", next: &Node{Name: "b", next: c}}

	last := Last[*Node](a)
	fmt.Println("chain:", last.Name, last == c)

	var viaVar = Last(a)
	fmt.Println("chain var:", viaVar.Name, viaVar == c)

	end, depth := LastAndDepth(a)
	fmt.Println("chain tuple:", end.Name, depth, end == c)

	fmt.Println("chain field on call:", Last(a).Name)
	fmt.Println("chain non-generic param:", describeNode(Last(a)))
	fmt.Println("chain returned:", lastOf(a) == c)

	Last(a).Name = "C"
	fmt.Println("chain write through:", c.Name)
}
