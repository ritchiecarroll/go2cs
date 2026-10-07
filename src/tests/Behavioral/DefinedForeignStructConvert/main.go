// A defined type over a struct declared in ANOTHER package (`type customTime time.Time`) has the
// same underlying struct as that type, and Go's reflect identity compares the struct's own package
// path: the package that DECLARES the struct's fields, not the package declaring the defined type.
// testify's assert.Compare asks `reflect.ValueOf(x).CanConvert(timeType)` of exactly such a value.
package main

import (
	"fmt"
	"reflect"
	"time"

	"DefinedForeignStructConvert/rec"
)

type customTime time.Time

type customHidden rec.Hidden

type customOpen rec.Open

type local struct {
	n int
	s string
}

type customLocal local

func check(name string, x any, to reflect.Type) {
	v := reflect.ValueOf(x)
	fmt.Println(name, "CanConvert:", v.CanConvert(to), "ConvertibleTo:", v.Type().ConvertibleTo(to), "back:", to.ConvertibleTo(v.Type()))
}

func main() {
	when := time.Date(2020, 1, 2, 3, 4, 5, 0, time.UTC)
	timeType := reflect.TypeOf(time.Time{})

	check("customTime -> time.Time", customTime(when), timeType)
	check("customHidden -> rec.Hidden", customHidden(rec.NewHidden(1, "a")), reflect.TypeOf(rec.Hidden{}))
	check("customOpen -> rec.Open", customOpen(rec.Open{N: 2, S: "b"}), reflect.TypeOf(rec.Open{}))
	check("customLocal -> local", customLocal(local{3, "c"}), reflect.TypeOf(local{}))

	// Not identical: rec.Hidden and a local struct with the same fields differ by package path.
	check("customLocal -> rec.Hidden", customLocal(local{4, "d"}), reflect.TypeOf(rec.Hidden{}))

	// testify's compare(): convert, then assert back to time.Time.
	v := reflect.ValueOf(customTime(when))
	if v.CanConvert(timeType) {
		back := v.Convert(timeType).Interface().(time.Time)
		fmt.Println("converted time:", back.Equal(when), back.UnixNano() == when.UnixNano())
	}

	h := reflect.ValueOf(customHidden(rec.NewHidden(5, "e"))).Convert(reflect.TypeOf(rec.Hidden{})).Interface().(rec.Hidden)
	fmt.Println("converted hidden:", h.N, h.S())
}
