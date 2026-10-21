// Probe for the generated reflect method arms (IGoMethodSet), increment 2: promoted methods (through a value embed
// and through a pointer embed) and a variadic method, reached through Method, MethodByName, Type.Method(i).Func and
// Call. The prediction and the measurement are in ../README.md.
package main

import (
	"fmt"
	"reflect"
	"strings"
)

type Inner struct{ i int }

func (x Inner) Twice() int { return x.i * 2 }
func (x *Inner) Bump()     { x.i++ }
func (x Inner) Join(sep string, parts ...string) string {
	return fmt.Sprint(x.i) + ":" + strings.Join(parts, sep)
}

type Outer struct {
	Inner
	o int
}

type Wrap struct{ *Inner }

func names(t reflect.Type) string {
	var s []string
	for i := 0; i < t.NumMethod(); i++ {
		s = append(s, t.Method(i).Name)
	}
	return strings.Join(s, ",")
}

func main() {
	o := Outer{Inner{4}, 1}
	v := reflect.ValueOf(&o)
	fmt.Println(names(reflect.TypeOf(o)), names(reflect.TypeOf(&o)), names(reflect.TypeOf(Wrap{})))

	v.MethodByName("Bump").Call(nil)
	bumped := o.i
	fmt.Println(bumped, v.MethodByName("Twice").Call(nil)[0], reflect.ValueOf(o).MethodByName("Twice").Call(nil)[0])

	m, _ := reflect.TypeOf(&o).MethodByName("Twice")
	fmt.Println(m.Func.Call([]reflect.Value{v})[0])

	j := v.MethodByName("Join")
	fmt.Println(j.Call([]reflect.Value{reflect.ValueOf("-"), reflect.ValueOf("a"), reflect.ValueOf("b")})[0])

	jf, _ := reflect.TypeOf(o).MethodByName("Join")
	fmt.Println(jf.Func.Call([]reflect.Value{reflect.ValueOf(o), reflect.ValueOf("+"), reflect.ValueOf("x"), reflect.ValueOf("y")})[0])

	w := Wrap{&Inner{7}}
	wv := reflect.ValueOf(w)
	wv.MethodByName("Bump").Call(nil)
	twice := wv.MethodByName("Twice").Call(nil)[0]
	wv.Method(0).Call(nil)
	fmt.Println(w.i, twice)
}
