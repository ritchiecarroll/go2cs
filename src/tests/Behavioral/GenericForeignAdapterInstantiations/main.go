// GenericForeignAdapterInstantiations guards C33: several instantiations of ONE foreign generic
// type, each converted through a pointer to the same foreign interface. go2cs-gen builds one
// adapter per closed instantiation of a foreign generic, so each needs its own adapter name, and
// the cast sites the converter writes must use the same names. Both sides spelled the name without
// type arguments (`fieldlib_ExpectedжField` for every instantiation), so the generator declared one
// class several times (CS0102/CS0111/CS0579/CS8646) and every cast site named the same one.
//
// Each instantiation is called through the interface, so Output checks that every adapter
// forwards to its own instantiation, not only that the program compiles. Box[string] as Getter is
// the control: its required method mentions T, so it keeps the closed per-instantiation adapter.
package main

import (
	"fmt"

	"GenericForeignAdapterInstantiations/fieldlib"
)

func show(label string, f fieldlib.Field) {
	fmt.Println(label, f.IsPresent(), f.Describe())
}

func main() {
	name := fieldlib.Expected[string]{Value: "pkg", Valid: true}
	deps := fieldlib.Expected[map[string]string]{Value: map[string]string{"a": "1"}, Valid: false}
	size := fieldlib.Expected[int]{Value: 42, Valid: true}

	show("string", &name)
	show("map", &deps)
	show("int", &size)

	// The same instantiations through a slice of the interface, and a second cast of one of them.
	all := []fieldlib.Field{&name, &deps, &size, &name}
	present := 0
	for _, f := range all {
		if f.IsPresent() {
			present++
		}
	}
	fmt.Println("present", present, "of", len(all))

	// Control: the closed route, where the required method mentions T.
	box := fieldlib.Box[string]{Item: "boxed"}
	var g fieldlib.Getter = &box
	fmt.Println("getter", g.Get())
}
