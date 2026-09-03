// FieldDimsCargo pins the struct-FIELD half of the array-length descriptor cargo: the array
// lengths a field's type reaches through a hop that has NO value and NO initializer to measure.
//
// A Go array's length is part of its type and `array<T>` cannot hold it, so every position recovers
// it from whichever source knew it. A field that IS an array is emitted with `= new(N)` and reads
// back off the declaring type's zero instance. Two hops that route reaches nothing at:
//
//   - behind a POINTER — the zero instance's field is a nil pointer, with no pointee to measure;
//   - inside a MAP — the zero instance's field is a nil map, and even a populated one reveals
//     nothing, because Key()/Elem() answer for the TYPE, not for an entry.
//
// Both are ordinary shapes at a DECODE TARGET, which is exactly a struct nothing has populated yet.
// encoding/gob's TestEndToEnd reaches `map[[2]string][2]*float64` and `*[3]float64` fields that way
// and reads `Len()` off what Key()/Elem() hand it; with the lengths lost it reported
// "gob: length mismatch in decodeArray" and "wrong type (***[]int) for received field Direct.A".
// So the converter stamps the two accessors' cargo on the field — `[GoArrayDims]` for what Elem()
// hands down, `[GoMapKeyDims]` for what Key() does.
//
// The boundary rows below are as much of the guard as the working ones: an ALIAS resolves to its
// target and is carried, a DEFINED array type is not (its managed form is a go2cs-gen wrapper with
// no slot for cargo), and a second nesting level is not carried at all.
package main

import (
	"fmt"
	"reflect"
)

// Row is a DEFINED array type: its managed form is a generated wrapper rather than array<T>, so it
// carries no dims cargo — the same boundary a defined channel type draws for its direction.
type Row [3]int

// Target mirrors encoding/gob's T1 field-for-field where the cargo matters, plus the boundary shapes.
type Target struct {
	Marr    map[[2]string][2]*float64 // gob's T1.Marr — key AND element are arrays
	N       *[3]float64               // gob's T1.N — a pointer to an array
	Deep    ***[3]int                 // gob's Indirect.A — one stamp covers every pointer depth
	MapElem map[string][5]int         // only the element is an array
	MapKey  map[[4]byte]int           // only the key is an array
	Plain   [4]byte                   // an array field: carried by its own initializer
	Nested  [2][3]int                 // dims are outermost-first
	Named   *Row                      // a defined array type behind a pointer: NOT carried
	SlcArr  [][2]int                  // a second nesting level: NOT carried
}

func main() {
	t := reflect.TypeOf(Target{})

	// EVERY field, SlcArr included. This loop ran to NumField()-1 while `SlcArr [][2]int` rendered
	// `[][]int` — the one boundary this cargo did not close. The reason recorded here was that a
	// slice element is a "second nesting level" with nowhere to live in a one-Elem()-slot cargo,
	// and it was RIGHT while `Elem()` CONSUMED the head of the dims vector for non-pointer,
	// non-map kinds: a one-element vector handed the element nothing, so stamping a slice would
	// have been inventing cargo no accessor could read.
	//
	// A slice has no length of its own, so its dims can only ever describe its ELEMENT. `Elem()`
	// now hands a slice's and a channel's down UNSHIFTED, beside a pointer's and a map's, and the
	// converter stamps them — so the boundary is closed and the field joins the comparison.
	//
	// A genuine second nesting level (`[][][2]int`, `map[K]map[[2]string]V`) still has nowhere to
	// live, and those rows keep the original rationale in the converter's TestFieldDimsCargo.
	fmt.Println("-- field types --")

	for i := 0; i < t.NumField(); i++ {
		f := t.Field(i)
		fmt.Printf("%-8s %s\n", f.Name, f.Type)
	}

	marr := t.Field(0).Type

	fmt.Println("-- map accessors --")
	fmt.Println("key ", marr.Key(), marr.Key().Len())
	fmt.Println("elem", marr.Elem(), marr.Elem().Len())

	// The exact pair of operations gob's decodeMap performs before it fills an entry: allocate a
	// key and an element from the map's own descriptors. A dimension-less descriptor allocates a
	// ZERO-length array here, which is what "length mismatch in decodeArray" reports one frame on.
	fmt.Println("-- decodeMap allocation --")
	fmt.Println("new key len ", reflect.New(marr.Key()).Elem().Len())
	fmt.Println("new elem len", reflect.New(marr.Elem()).Elem().Len())

	// A pointer hands its cargo down UNSHIFTED at every hop, so one stamp answers at any depth —
	// which is what makes gob's `***[3]int` field compatible with a `[3]int` on the wire.
	deep := t.Field(2).Type

	fmt.Println("-- pointer chain --")
	fmt.Println("deep      ", deep)
	fmt.Println("deep elem3", deep.Elem().Elem().Elem(), deep.Elem().Elem().Elem().Len())
	fmt.Println("ptr elem  ", t.Field(1).Type.Elem(), t.Field(1).Type.Elem().Len())

	// The VALUE side of the same pointer-hop rule, which is gob's decIndirect verbatim: it walks a
	// `***[3]int` decode target by allocating each level from `value.Type().Elem()`, so EVERY hop
	// must hand the cargo down — including the intermediate ones, whose pointee is another pointer
	// rather than the array. A hop that answered from the live value alone would read the nil
	// pointer it is standing on, allocate a zero-length array from the dimension-less descriptor,
	// and the next hop would measure that zero as the truth.
	var target Target

	walk := reflect.ValueOf(&target).Elem().Field(2)

	for walk.Kind() == reflect.Pointer {
		if walk.IsNil() {
			walk.Set(reflect.New(walk.Type().Elem()))
		}

		walk = walk.Elem()
	}

	fmt.Println("-- decIndirect walk --")
	fmt.Println("landed on", walk.Type(), walk.Len(), walk.CanAddr())

	fmt.Println("-- one accessor at a time --")
	fmt.Println("elem-only key ", t.Field(3).Type.Key())
	fmt.Println("elem-only elem", t.Field(3).Type.Elem(), t.Field(3).Type.Elem().Len())
	fmt.Println("key-only key  ", t.Field(4).Type.Key(), t.Field(4).Type.Key().Len())
	fmt.Println("key-only elem ", t.Field(4).Type.Elem())

	// The array field's own initializer route, unchanged by any of the above — and the reason the
	// converter does NOT stamp an array field: the datum is already there, through a route that
	// also survives a value copy.
	fmt.Println("-- initializer route (unchanged) --")
	fmt.Println("plain ", t.Field(5).Type, t.Field(5).Type.Len())
	fmt.Println("nested", t.Field(6).Type, t.Field(6).Type.Len(), t.Field(6).Type.Elem().Len())

	// The boundaries. A defined array type reports its own name and no length through the pointer;
	// a slice element is a second nesting level the two-slot cargo has nowhere to put.
	fmt.Println("-- boundaries --")
	fmt.Println("named ptr elem kind", t.Field(7).Type.Elem().Kind())
	fmt.Println("slice elem kind    ", t.Field(8).Type.Elem().Kind())
}
