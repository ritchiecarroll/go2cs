// NilFuncInterfaceReads: a typed nil func held in an interface, read by the language and by reflect. Go keeps the
// func type as the dynamic type and nil as the value, whichever way the value reached the interface. Carriers: a
// named func of another package (step), one that is also an error (stepe), the same two of THIS package (local,
// locale), an unnamed func (bare), and two controls (livestep: a non-nil named func; nilptr: a typed nil pointer).
// Arms: "direct" (the value in an any), "va" (a static conversion to an interface), "sh" (the any asserted to that
// interface) and "er" (a static conversion to error). Type switches over these values are not read here.
package main

import (
	"fmt"
	"reflect"

	"NilFuncInterfaceReads/typelib"
)

type namer interface {
	Name() string
}

type localStep func(int) int

func (s localStep) Name() string { return "local" }

type localStepE func(int) int

func (s localStepE) Name() string { return "locale" }

func (s localStepE) Error() string { return "locale error" }

type probe struct {
	name string
	run  func(a any) string
}

func viaAny(a any) namer {
	return a.(namer)
}

var kinds = []string{"step", "stepe", "local", "locale", "bare", "livestep", "nilptr"}

func armsFor(kind string) []string {
	switch kind {
	case "bare":
		return []string{"direct"}
	case "stepe", "locale":
		return []string{"direct", "va", "sh", "er"}
	}
	return []string{"direct", "va", "sh"}
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
	case "livestep":
		return typelib.NewStep()
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

var namerType = reflect.TypeOf((*namer)(nil)).Elem()

var probes = []probe{
	// The language alone: no reflect call in the program text of these.
	{"l-isnil", func(a any) string { return fmt.Sprint(a == nil) }},
	{"l-namer", func(a any) string {
		n, ok := a.(namer)
		if !ok {
			return "miss"
		}
		return n.Name()
	}},
	{"l-reassert", func(a any) string {
		var b any = a.(namer)
		n, ok := b.(namer)
		if !ok {
			return "miss"
		}
		return n.Name()
	}},
	{"l-namernil", func(a any) string { return fmt.Sprint(a.(namer) == nil) }},
	{"l-error", func(a any) string {
		e, ok := a.(error)
		if !ok {
			return "miss"
		}
		return e.Error()
	}},
	{"l-errnamer", func(a any) string {
		var b any = a.(error)
		n, ok := b.(namer)
		if !ok {
			return "miss"
		}
		return n.Name()
	}},
	{"l-eqself", func(a any) string { return fmt.Sprint(a == a) }},
	{"l-fmtT", func(a any) string { return fmt.Sprintf("%T", a) }},
	{"l-fmtv", func(a any) string { return fmt.Sprintf("%v", a) }},
	// reflect.
	{"kind", func(a any) string { return reflect.ValueOf(a).Kind().String() }},
	{"type", func(a any) string { return reflect.ValueOf(a).Type().String() }},
	{"typeof", func(a any) string { return reflect.TypeOf(a).String() }},
	{"implements", func(a any) string { return fmt.Sprint(reflect.TypeOf(a).Implements(namerType)) }},
	{"zero", func(a any) string { return fmt.Sprint(reflect.Zero(reflect.TypeOf(a)).IsNil()) }},
	{"isnil", func(a any) string { return fmt.Sprint(reflect.ValueOf(a).IsNil()) }},
	{"iszero", func(a any) string { return fmt.Sprint(reflect.ValueOf(a).IsZero()) }},
	{"pointer", func(a any) string { return fmt.Sprint(reflect.ValueOf(a).Pointer() == 0) }},
	{"nummethod", func(a any) string { return fmt.Sprint(reflect.ValueOf(a).NumMethod()) }},
	{"iface", func(a any) string { return reflect.ValueOf(a).Interface().(namer).Name() }},
	{"ifacenil", func(a any) string { return fmt.Sprint(reflect.ValueOf(a).Interface() == nil) }},
	{"ifaceT", func(a any) string { return fmt.Sprintf("%T", reflect.ValueOf(a).Interface()) }},
	{"method", func(a any) string { return reflect.ValueOf(a).MethodByName("Name").Call(nil)[0].String() }},
	{"method0", func(a any) string {
		v := reflect.ValueOf(a)
		return reflect.TypeOf(a).Method(0).Name + " " + v.Method(0).Call(nil)[0].String()
	}},
	{"call", func(a any) string {
		out := reflect.ValueOf(a).Call([]reflect.Value{reflect.ValueOf(1)})
		return fmt.Sprint(out[0].Int())
	}},
	{"same", func(a any) string { return fmt.Sprint(reflect.ValueOf(a) == reflect.ValueOf(a)) }},
	{"deep", func(a any) string { return fmt.Sprint(reflect.DeepEqual(a, a)) }},
	{"setslot", func(a any) string {
		var slot namer
		reflect.ValueOf(&slot).Elem().Set(reflect.ValueOf(a))
		return slot.Name()
	}},
	{"setany", func(a any) string {
		var slot any
		reflect.ValueOf(&slot).Elem().Set(reflect.ValueOf(a))
		return fmt.Sprintf("%T", slot) + " " + fmt.Sprint(slot == nil)
	}},
	{"setself", func(a any) string {
		v := reflect.ValueOf(a)
		s := reflect.New(v.Type()).Elem()
		s.Set(v)
		return fmt.Sprint(s.IsNil())
	}},
	{"convert", func(a any) string {
		return reflect.ValueOf(a).Convert(namerType).Interface().(namer).Name()
	}},
	{"fmtvalue", func(a any) string { return fmt.Sprintf("%v", reflect.ValueOf(a)) }},
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

// skip reports the reads that are not comparable: a live func prints its address.
func skip(kind string, name string) bool {
	return kind == "livestep" && (name == "l-fmtv" || name == "fmtvalue")
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
