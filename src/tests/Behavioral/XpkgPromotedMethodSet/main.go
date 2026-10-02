// XpkgPromotedMethodSet is the program of the cross-package promotion guard. Each type below
// embeds a struct of ANOTHER package, and the program prints every type's value and pointer method
// set (count and sorted names) the way reflect reports them, then asks the same question through
// interface assertions and calls. Go's own output is the expectation.
//
// What each shape pins:
//   - Direct, Outer: an embed's exported methods are promoted across one and two packages.
//   - PtrDirect, PtrStamp, BufP, Broken: through a POINTER embed a pointer-receiver method is in
//     the VALUE method set; through value embeds only (Direct, Stamp) it is in the pointer set.
//   - Five, S6, OuterS5: the shallowest occurrence of a name wins.
//   - GCounter, Within, OuterFD: a name that occurs twice at one depth is dropped.
//   - Shadow, OuterFS: a field hides a deeper method of its name.
//   - HideC: an embedded interface's method collides with an embedded struct's.
//   - Mixed: a same-package hop first, then a cross-package one, beside a package var `Name`.
//   - TT: the hand-written testing.T methods, promoted through a pointer embed.
package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"reflect"
	"sort"
	"strings"
	"sync"
	"testing"
	"time"

	inner "XpkgPromotedInnerLib"
	mid "XpkgPromotedMidLib"
)

type Direct struct{ inner.Inner }

type PtrDirect struct{ *inner.Inner }

type Outer struct{ mid.Mid }

type loc struct{}

func (loc) Tok() inner.Token { return inner.Token{N: 7} }

// Five: loc.Tok (depth 1) beats Mid's Inner.Tok (depth 2).
type Five struct {
	mid.Mid
	loc
}

type local struct{ inner.Inner }

// GCounter reaches every method of Inner twice at depth 2, so all of them are ambiguous.
type GCounter struct {
	local
	mid.Mid
}

type local2 struct{ inner.Inner }

// S6: loc.Tok (depth 1) beats local2's Inner.Tok (depth 2).
type S6 struct {
	loc
	local2
}

type Within struct{ mid.Mid2 }

type Shadow struct {
	inner.Inner
	Tok int
}

type localMid struct{ inner.Inner }

type Mixed struct{ localMid }

// Name is a package var that shares a promoted method's name.
var Name = 0

type brokenState struct{ sync.Mutex }

// Broken: a pointer embed, then sync.Mutex by value. Lock and Unlock are in the VALUE set.
type Broken struct {
	io.Reader
	*brokenState
}

type Stamp struct{ time.Time }
type PtrStamp struct{ *time.Time }

type BufP struct{ *bytes.Buffer }

type deepTok struct{}

func (deepTok) Tok() inner.Token { return inner.Token{N: 5} }

type localTok struct{ deepTok }

// OuterS5: Mid3 reaches Inner.Tok at depth 3; localTok's deepTok.Tok is at depth 2 and wins.
type OuterS5 struct {
	mid.Mid3
	localTok
}

type OuterFD struct{ mid.MidF }

type OuterFS struct{ mid.PS }

type TT struct{ *testing.T }

// HideC: bytes.Buffer's WriteTo and the embedded io.WriterTo's both sit at depth 1, so Go drops
// WriteTo from both method sets.
type HideC struct {
	bytes.Buffer
	io.WriterTo
}

func methods(t reflect.Type) string {
	var names []string

	for i := 0; i < t.NumMethod(); i++ {
		names = append(names, t.Method(i).Name)
	}

	sort.Strings(names)

	return fmt.Sprintf("%d [%s]", t.NumMethod(), strings.Join(names, " "))
}

func report(label string, v any) {
	t := reflect.TypeOf(v)
	fmt.Printf("%-10s value %s\n", label, methods(t))
	fmt.Printf("%-10s ptr   %s\n", label, methods(reflect.PointerTo(t)))
}

// probe runs one check and reports a panic instead of dying on it.
func probe(label string, f func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: PANIC %v\n", label, r)
		}
	}()

	fmt.Printf("%s: %s\n", label, f())
}

func main() {
	report("Direct", Direct{})
	report("PtrDirect", PtrDirect{})
	report("Outer", Outer{})
	report("Five", Five{})
	report("GCounter", GCounter{})
	report("S6", S6{})
	report("Within", Within{})
	report("Shadow", Shadow{})
	report("Mixed", Mixed{})
	report("Broken", Broken{})
	report("Stamp", Stamp{})
	report("PtrStamp", PtrStamp{})
	report("BufP", BufP{})
	report("HideC", HideC{})
	report("OuterS5", OuterS5{})
	report("OuterFD", OuterFD{})
	report("OuterFS", OuterFS{})
	report("TT", TT{})

	probe("S5 dispatch Tok", func() string { return fmt.Sprint(any(OuterS5{}).(interface{ Tok() inner.Token }).Tok().N) })
	probe("TT value has Error", func() string { _, ok := any(TT{}).(interface{ Error(...any) }); return fmt.Sprint(ok) })

	fmt.Println(Direct{}, Outer{}, Mixed{})

	var f any = Five{}

	if t, ok := f.(interface{ Tok() inner.Token }); ok {
		fmt.Println("Five Tok N:", t.Tok().N)
	}

	var s6 any = S6{}

	if t, ok := s6.(interface{ Tok() inner.Token }); ok {
		fmt.Println("S6 Tok N:", t.Tok().N)
	}

	_, ok := any(Stamp{}).(json.Unmarshaler)
	fmt.Println("Stamp value is json.Unmarshaler:", ok)
	_, ok = any(&Stamp{}).(json.Unmarshaler)
	fmt.Println("*Stamp is json.Unmarshaler:", ok)
	_, ok = any(PtrStamp{}).(json.Unmarshaler)
	fmt.Println("PtrStamp value is json.Unmarshaler:", ok)
	_, ok = reflect.TypeOf(Direct{}).MethodByName("Set")
	fmt.Println("Direct value MethodByName Set:", ok)
	_, ok = reflect.TypeOf(PtrDirect{}).MethodByName("Set")
	fmt.Println("PtrDirect value MethodByName Set:", ok)

	var l any = Broken{}
	_, ok = l.(sync.Locker)
	fmt.Println("Broken value is sync.Locker:", ok)
	fmt.Println("Name var:", Name)

	// A promoted method called directly, through an assertion on the embedding type, and through
	// one on a type two packages out.
	probe("Where direct call", func() string { return fmt.Sprint(Direct{}.Where()) })
	probe("Where via assert Direct", func() string { return fmt.Sprint(any(Direct{}).(interface{ Where() bool }).Where()) })
	probe("Where via assert Outer", func() string { return fmt.Sprint(any(Outer{}).(interface{ Where() bool }).Where()) })

	// Set through the pointer set and through a pointer embed's value set: the call reaches Inner.
	d := &Direct{}
	any(d).(interface{ Set(int) }).Set(4)
	fmt.Println("Direct after Set through *Direct:", d.X)

	pd := PtrDirect{&inner.Inner{}}
	any(pd).(interface{ Set(int) }).Set(6)
	fmt.Println("PtrDirect after Set through the value:", pd.X)

	// The same-package promotions of a pointer-receiver method in XpkgPromotedMidLib.
	probe("SameP value methods", func() string { return methods(reflect.TypeOf(mid.NewSameP())) })
	probe("SameP ptr methods", func() string { return methods(reflect.TypeOf(&mid.SameP{})) })
	probe("SameV value methods", func() string { return methods(reflect.TypeOf(mid.SameV{})) })
	probe("SameV ptr methods", func() string { return methods(reflect.TypeOf(&mid.SameV{})) })
	probe("SameP value Method(0) call", func() string {
		return fmt.Sprint(reflect.ValueOf(mid.NewSameP()).Method(0).Call(nil)[0].Int())
	})
	probe("SameP value assert+call Ping", func() string {
		p, ok := any(mid.NewSameP()).(interface{ Ping() int })

		if !ok {
			return "not satisfied"
		}

		return fmt.Sprint(p.Ping())
	})
	probe("SameV value assert Ping", func() string {
		_, ok := any(mid.SameV{}).(interface{ Ping() int })

		return fmt.Sprint(ok)
	})
	probe("*SameV assert+call Ping", func() string {
		p, ok := any(&mid.SameV{}).(interface{ Ping() int })

		if !ok {
			return "not satisfied"
		}

		return fmt.Sprint(p.Ping())
	})
}
