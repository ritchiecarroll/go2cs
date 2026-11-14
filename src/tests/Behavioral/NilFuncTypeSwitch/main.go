// NilFuncTypeSwitch: a typed nil func held in an interface, read by a type switch. Go keeps the func type as the
// dynamic type, so the case naming that type is taken and its binding is the nil func. Carriers: a named func of
// another package (step), one that is also an error (stepe), the same two of THIS package (local, locale), an
// unnamed func (bare), named funcs without a method (hook; left and right, which share one signature), and
// controls: a live value of each shape and a typed nil pointer (nilptr). Arms: "direct" (the value in an any),
// "va" (a static conversion to an interface), "sh" (the any asserted to that interface) and "er" (a static
// conversion to error).
package main

import (
	"fmt"
	"reflect"

	"NilFuncTypeSwitch/typelib"
)

type namer interface {
	Name() string
}

type localStep func(int) int

func (s localStep) Name() string { return "local" }

type localStepE func(int) int

func (s localStepE) Name() string { return "locale" }

func (s localStepE) Error() string { return "locale error" }

// hook has no method and a signature of its own.
type hook func(string) int

// left and right have no method and share one signature.
type left func(bool) bool

type right func(bool) bool

type probe struct {
	name string
	run  func(a any) string
}

func viaAny(a any) namer {
	return a.(namer)
}

var kinds = []string{
	"step", "stepe", "local", "locale", "bare", "hook", "left", "right",
	"livestep", "livelocale", "livebare", "livehook", "nilptr",
}

func armsFor(kind string) []string {
	switch kind {
	case "bare", "hook", "left", "right", "livebare", "livehook":
		return []string{"direct"}
	case "stepe", "locale", "livelocale":
		return []string{"direct", "va", "sh", "er"}
	}
	return []string{"direct", "va", "sh"}
}

func hasName(kind string) bool {
	return len(armsFor(kind)) > 1
}

func concrete(kind string) any {
	switch kind {
	case "step":
		return typelib.Step(nil)
	case "stepe":
		return typelib.StepE(nil)
	case "local":
		return localStep(nil)
	case "locale":
		return localStepE(nil)
	case "bare":
		return (func(int) int)(nil)
	case "hook":
		return hook(nil)
	case "left":
		return left(nil)
	case "right":
		return right(nil)
	case "livestep":
		return typelib.NewStep()
	case "livelocale":
		return localStepE(func(x int) int { return x + 2 })
	case "livebare":
		return func(x int) int { return x + 3 }
	case "livehook":
		return hook(func(s string) int { return len(s) })
	case "nilptr":
		return (*typelib.Box)(nil)
	}
	return nil
}

func static(kind string) namer {
	var n namer
	switch kind {
	case "step":
		n = typelib.Step(nil)
	case "stepe":
		n = typelib.StepE(nil)
	case "local":
		n = localStep(nil)
	case "locale":
		n = localStepE(nil)
	case "livestep":
		n = typelib.NewStep()
	case "livelocale":
		n = localStepE(func(x int) int { return x + 2 })
	case "nilptr":
		n = (*typelib.Box)(nil)
	}
	return n
}

func asError(kind string) error {
	var e error
	switch kind {
	case "stepe":
		e = typelib.StepE(nil)
	case "locale":
		e = localStepE(nil)
	case "livelocale":
		e = localStepE(func(x int) int { return x + 2 })
	}
	return e
}

func build(arm string, kind string) any {
	switch arm {
	case "direct":
		return concrete(kind)
	case "va":
		var a any = static(kind)
		return a
	case "sh":
		var a any = viaAny(concrete(kind))
		return a
	case "er":
		var a any = asError(kind)
		return a
	}
	return nil
}

// which binds the value in every case.
func which(a any) string {
	switch v := a.(type) {
	case nil:
		return "nil"
	case typelib.Step:
		if v != nil {
			return "Step live " + fmt.Sprint(v(1))
		}
		return "Step nil"
	case typelib.StepE:
		if v != nil {
			return "StepE live " + fmt.Sprint(v(1))
		}
		return "StepE nil"
	case localStep:
		if v != nil {
			return "localStep live " + fmt.Sprint(v(1))
		}
		return "localStep nil"
	case localStepE:
		if v != nil {
			return "localStepE live " + fmt.Sprint(v(1))
		}
		return "localStepE nil"
	case func(int) int:
		if v != nil {
			return "func live " + fmt.Sprint(v(1))
		}
		return "func nil"
	case hook:
		if v != nil {
			return "hook live " + fmt.Sprint(v("abc"))
		}
		return "hook nil"
	case *typelib.Box:
		return "Box " + fmt.Sprint(v == nil)
	}
	return "other"
}

// unbound names the same types and binds nothing.
func unbound(a any) string {
	switch a.(type) {
	case nil:
		return "nil"
	case typelib.Step:
		return "Step"
	case typelib.StepE:
		return "StepE"
	case localStep:
		return "localStep"
	case localStepE:
		return "localStepE"
	case func(int) int:
		return "func"
	case hook:
		return "hook"
	case *typelib.Box:
		return "Box"
	}
	return "other"
}

// multi names two types per clause, so the binding keeps the interface type.
func multi(a any) string {
	switch v := a.(type) {
	case nil:
		return "nil"
	case typelib.Step, typelib.StepE:
		return "lib " + fmt.Sprint(v == nil)
	case localStep, localStepE:
		return "local " + fmt.Sprint(v == nil)
	case func(int) int, hook:
		return "plain " + fmt.Sprint(v == nil)
	case *typelib.Box:
		return "Box " + fmt.Sprint(v == nil)
	}
	return "other"
}

// mixed puts interface cases before, between and after func cases: the first case that holds is taken.
func mixed(a any) string {
	switch v := a.(type) {
	case error:
		return "error " + v.Error()
	case typelib.Step:
		return "Step " + fmt.Sprint(v == nil)
	case func(int) int:
		return "func " + fmt.Sprint(v == nil)
	case namer:
		return "namer " + v.Name()
	case hook:
		return "hook " + fmt.Sprint(v == nil)
	}
	return "other"
}

func stepIsNil(p *typelib.Step) bool {
	return *p == nil
}

func bareIsNil(p *func(int) int) bool {
	return *p == nil
}

func hookIsNil(p *hook) bool {
	return *p == nil
}

// escape takes the binding's address in every func case.
func escape(a any) string {
	switch v := a.(type) {
	case typelib.Step:
		return "Step " + fmt.Sprint(stepIsNil(&v))
	case func(int) int:
		return "func " + fmt.Sprint(bareIsNil(&v))
	case hook:
		return "hook " + fmt.Sprint(hookIsNil(&v))
	}
	return "other"
}

// merged names two func types of one signature with the same body.
func merged(a any) string {
	switch a.(type) {
	case left:
		return "pred"
	case right:
		return "pred"
	}
	return "other"
}

var probes = []probe{
	{"bound", which},
	{"unbound", unbound},
	{"multi", multi},
	{"mixed", mixed},
	{"escape", escape},
	{"merged", merged},
	{"reswitch", func(a any) string {
		var b any = a.(namer)
		return which(b)
	}},
	{"ifacesw", func(a any) string { return which(reflect.ValueOf(a).Interface()) }},
}

func runOne(arm string, kind string, p probe) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println(arm, kind, p.name, "= PANIC", r)
		}
	}()
	a := build(arm, kind)
	fmt.Println(arm, kind, p.name, "=", p.run(a))
}

// skip reports the reads that do not apply: only a type with the Name method asserts to the interface.
func skip(kind string, name string) bool {
	return name == "reswitch" && !hasName(kind)
}

func main() {
	for _, kind := range kinds {
		for _, arm := range armsFor(kind) {
			for _, p := range probes {
				if skip(kind, p.name) {
					continue
				}
				runOne(arm, kind, p)
			}
		}
	}
}
