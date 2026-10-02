package main

import (
	"encoding/json"
	"encoding/xml"
	"fmt"
	"reflect"
)

// An embedded INTERFACE is an anonymous field to reflect, exactly as an embedded struct is:
// Anonymous is true, the name is the interface's, and an unexported interface's field carries
// the package path. Everything below reads that one bit through a different consumer.

type E struct{ Code int }

func (E) Error() string    { return "e" }
func (E) GoString() string { return "E!" }

type S struct{ N int }

func (S) String() string { return "s" }

// An unexported interface embed (error) and an exported one (fmt.Stringer).
type w struct{ error }

type Pub struct{ fmt.Stringer }

// The same field, NAMED: never anonymous.
type Named struct{ Stringer fmt.Stringer }

type Tagged struct {
	fmt.Stringer `json:"key"`
	N            int
}

type Omitted struct {
	fmt.Stringer `json:"-"`
	N            int
}

type X struct {
	error
	N int
}

func describe(label string, t reflect.Type) {
	for i := 0; i < t.NumField(); i++ {
		f := t.Field(i)
		fmt.Printf("%s field %d: name=%s anonymous=%v exported=%v pkgpath=%q index=%v\n", label, i, f.Name, f.Anonymous, f.IsExported(), f.PkgPath, f.Index)
	}
}

func visible(label string, t reflect.Type) {
	for _, f := range reflect.VisibleFields(t) {
		fmt.Printf("%s visible: name=%s anonymous=%v index=%v\n", label, f.Name, f.Anonymous, f.Index)
	}
}

func main() {
	describe("w", reflect.TypeOf(w{}))
	describe("Pub", reflect.TypeOf(Pub{}))
	describe("Named", reflect.TypeOf(Named{}))
	describe("X", reflect.TypeOf(X{}))

	visible("Pub", reflect.TypeOf(Pub{}))
	visible("X", reflect.TypeOf(X{}))

	// An unexported embedded field is read-only, and Elem keeps the read-only bit as
	// "reached through an unexported field", not as "embedded": what it holds is no embed.
	v := reflect.ValueOf(w{E{7}})
	fmt.Println("w.error CanInterface:", v.Field(0).CanInterface())
	fmt.Println("w.error.Elem CanInterface:", v.Field(0).Elem().CanInterface())
	fmt.Println("w.error.Elem.Code CanInterface:", v.Field(0).Elem().Field(0).CanInterface())
	fmt.Println("w.error.Elem.Code:", v.Field(0).Elem().Field(0).Int())

	p := reflect.ValueOf(Pub{S{3}})
	fmt.Println("Pub.Stringer CanInterface:", p.Field(0).CanInterface())
	fmt.Println("Pub.Stringer.Elem.N CanInterface:", p.Field(0).Elem().Field(0).CanInterface())

	fmt.Printf("%v\n", w{E{7}})
	fmt.Printf("%+v\n", Pub{S{3}})
	fmt.Printf("%#v\n", w{E{7}})
	fmt.Printf("%#v\n", Pub{S{3}})
	fmt.Printf("%#v\n", Named{S{3}})

	// Convertibility demands the same Embedded bit on every field.
	twin := reflect.StructOf([]reflect.StructField{{Name: "Stringer", Type: reflect.TypeOf((*fmt.Stringer)(nil)).Elem(), Anonymous: true}})
	plain := reflect.StructOf([]reflect.StructField{{Name: "Stringer", Type: reflect.TypeOf((*fmt.Stringer)(nil)).Elem()}})
	fmt.Println("Pub -> anonymous twin:", reflect.TypeOf(Pub{}).ConvertibleTo(twin))
	fmt.Println("Pub -> named twin:", reflect.TypeOf(Pub{}).ConvertibleTo(plain))
	fmt.Println("Named -> anonymous twin:", reflect.TypeOf(Named{}).ConvertibleTo(twin))
	fmt.Println("Named -> named twin:", reflect.TypeOf(Named{}).ConvertibleTo(plain))
	fmt.Println("Pub -> Named:", reflect.TypeOf(Pub{}).ConvertibleTo(reflect.TypeOf(Named{})))

	// encoding/json treats an anonymous interface field as a NAMED field.
	for _, value := range []any{Pub{S{3}}, Named{S{3}}, Tagged{S{3}, 1}, Omitted{S{3}, 1}, Pub{}} {
		b, err := json.Marshal(value)
		fmt.Printf("json %T: %s %v\n", value, b, err)
	}

	// encoding/xml admits an anonymous field whatever its name's case.
	for _, value := range []any{X{E{7}, 1}, Pub{S{3}}, Named{S{3}}} {
		b, err := xml.Marshal(value)
		fmt.Printf("xml %T: %s %v\n", value, b, err)
	}
}
