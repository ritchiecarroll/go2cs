package main

import (
	"fmt"
	"reflect"
	"runtime"
	"strings"
)

// A method promoted through an embedded struct has no frame of its own in Go: the frame above the method is the
// function that made the call. Each line below reads the stack from inside a promoted method.
//
// Three shapes are left out because the converted program still differs from Go on them, for reasons that are not the
// promotion (docs/phase4/BOARD-next-validation-candidates.md, 2026-11-10, P1): a value method reached through an
// interface that holds a struct value, a call through a method value of a value method, and the name and the frame
// of a method value of a pointer method.

type inner struct{ n int }

// caller names the function skip frames above the method that calls it.
func caller(skip int) string {
	pc, _, _, ok := runtime.Caller(skip + 1)
	if !ok {
		return "?"
	}
	return runtime.FuncForPC(pc).Name()
}

// stack lists this program's frames, innermost first, from the method that calls it.
func stack() string {
	pcs := make([]uintptr, 16)
	n := runtime.Callers(2, pcs)
	frames := runtime.CallersFrames(pcs[:n])
	var names []string
	for {
		f, more := frames.Next()
		if strings.HasPrefix(f.Function, "main.") {
			names = append(names, f.Function)
		}
		if !more {
			break
		}
	}
	return strings.Join(names, " < ")
}

func (i inner) Val() string       { return caller(1) }
func (i *inner) Ptr() string      { return caller(1) }
func (i inner) Self() string      { return caller(0) }
func (i inner) ValStack() string  { return stack() }
func (i *inner) PtrStack() string { return stack() }

type mid struct{ inner }

// outer promotes inner's methods through two value embeds.
type outer struct{ mid }

// pouter promotes them through a pointer embed.
type pouter struct{ *inner }

type ptrer interface{ Ptr() string }

func name(f any) string {
	return runtime.FuncForPC(reflect.ValueOf(f).Pointer()).Name()
}

//go:noinline
func direct(o *outer, p pouter) {
	fmt.Println("direct value:", o.Val())
	fmt.Println("direct pointer:", o.Ptr())
	fmt.Println("direct self:", o.Self())
	fmt.Println("pointer embed value:", p.Val())
	fmt.Println("pointer embed pointer:", p.Ptr())
	fmt.Println("direct value stack:", o.ValStack())
	fmt.Println("direct pointer stack:", o.PtrStack())
}

//go:noinline
func viaIface(o *outer) {
	var q ptrer = o
	fmt.Println("iface pointer:", q.Ptr())
}

//go:noinline
func viaValue(o *outer) {
	fv := o.ValStack
	fmt.Println("method value stack:", fv())
}

//go:noinline
func viaExpr(o *outer) {
	ev := outer.Val
	ep := (*outer).Ptr
	fmt.Println("method expr call:", ev(*o), ep(o))
	fmt.Println("method expr name:", name(ev), name(ep))
	tv, _ := reflect.TypeOf(*o).MethodByName("Val")
	tp, _ := reflect.TypeOf(o).MethodByName("Ptr")
	fmt.Println("method table name:", name(tv.Func.Interface()), name(tp.Func.Interface()))
	es := outer.ValStack
	fmt.Println("method expr stack:", es(*o))
}

func main() {
	o := &outer{}
	direct(o, pouter{&inner{}})
	viaIface(o)
	viaValue(o)
	viaExpr(o)
}
