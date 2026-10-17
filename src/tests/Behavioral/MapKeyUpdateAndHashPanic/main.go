// Map key semantics a Go map gets from its runtime and a managed dictionary does not:
//
//  1. KEY UPDATE on overwrite. Go's map replaces the stored KEY when a key type's == admits
//     distinguishable values (NeedKeyUpdate: floats, complex, interfaces, strings and composites
//     of them), so `m[+0] = v; m[-0] = v` leaves -0 as the key. runtime.TestNegativeZero and
//     reflect.TestMapOfKeyUpdate assert it.
//  2. HASH PANIC on an unhashable dynamic key. An interface-keyed map hashes the key before it
//     looks anything up -- even when the map is EMPTY or NIL -- so a slice, map or func key panics
//     "hash of unhashable type T" on every operation (Go 1.26 words the empty- and nil-map doors
//     "hash of unhashable type: T", without the "runtime error: " prefix), and both are runtime.Errors.
//     runtime.TestEmptyMapWithInterfaceKey and reflect.TestMapOfKeyPanic assert it.
//  3. RANGE UNDER MUTATION. An entry overwritten during a range is produced with the bucket's
//     CURRENT key and value when the range reaches it.
package main

import (
	"fmt"
	"math"
	"reflect"
	"runtime"
)

var negZero = math.Copysign(0, -1)

func sign(f float64) string {
	if math.Signbit(f) {
		return "-0"
	}
	return "+0"
}

func keyUpdate() {
	m := map[float64]bool{}
	m[0] = true
	m[negZero] = true
	for k := range m {
		fmt.Println("float64 key after +0 then -0:", sign(k), "len", len(m))
	}

	m2 := map[float64]bool{}
	m2[negZero] = true
	m2[0] = true
	for k := range m2 {
		fmt.Println("float64 key after -0 then +0:", sign(k), "len", len(m2))
	}

	mi := map[any]int{}
	mi[0.0] = 1
	mi[negZero] = 2
	for k, v := range mi {
		fmt.Println("interface key after +0 then -0:", sign(k.(float64)), "value", v, "len", len(mi))
	}

	mc := map[complex128]int{}
	mc[complex(0, 0)] = 1
	mc[complex(negZero, negZero)] = 2
	for k, v := range mc {
		fmt.Println("complex128 key after overwrite:", sign(real(k)), sign(imag(k)), "value", v)
	}

	type pt struct{ x, y float64 }
	ms := map[pt]int{}
	ms[pt{0, 1}] = 1
	ms[pt{negZero, 1}] = 2
	for k, v := range ms {
		fmt.Println("struct{float64} key after overwrite:", sign(k.x), "value", v)
	}

	// A plain overwrite of an int key is unaffected: nothing to update.
	mn := map[int]int{7: 1}
	mn[7] = 2
	fmt.Println("int key overwrite:", mn[7], len(mn))

	// DEFINED types over float and complex update their keys the same way.
	mf := map[F64]bool{}
	mf[0] = true
	mf[F64(negZero)] = true
	for k := range mf {
		fmt.Println("defined float64 key after overwrite:", sign(float64(k)), "len", len(mf))
	}
	mf32 := map[F32]bool{}
	mf32[0] = true
	mf32[F32(negZero)] = true
	for k := range mf32 {
		fmt.Println("defined float32 key after overwrite:", sign(float64(k)), "len", len(mf32))
	}
	mc2 := map[C128]bool{}
	mc2[C128(complex(0, 0))] = true
	mc2[C128(complex(negZero, 0))] = true
	for k := range mc2 {
		fmt.Println("defined complex128 key after overwrite:", sign(real(k)), "len", len(mc2))
	}
}

type (
	F64  float64
	F32  float32
	C128 complex128
)

// nanKeys: a NaN equals nothing, itself included, so every NaN store is a NEW entry.
func nanKeys() {
	nan := math.NaN()
	m := map[float64]int{}
	m[nan] = 1
	m[nan] = 2
	m[0] = 3
	m[negZero] = 4
	fmt.Println("float64 NaN twice then ±0:", len(m))

	mi := map[any]int{}
	mi[nan] = 1
	mi[nan] = 2
	fmt.Println("interface NaN twice:", len(mi))

	mf := map[F64]int{}
	mf[F64(nan)] = 1
	mf[F64(nan)] = 2
	fmt.Println("defined float64 NaN twice:", len(mf))
}

// rangeUnderOverwrite: two entries; the body overwrites the zero entry with -0 on the first visit.
// Go's range order is random, so each trial checks an ORDER-INDEPENDENT invariant: if the zero
// entry is reached AFTER the overwrite it is produced with the updated (-0) key, and if it is
// reached first it is produced with the original (+0) key.
func rangeUnderOverwrite() {
	bad, after, before := 0, 0, 0
	for trial := 0; trial < 400; trial++ {
		// Alternate INSERTION order: go2cs ranges a map in insertion order, so a fixed literal
		// would only ever reach the zero entry on one side of the overwrite.
		m := map[float64]int{}
		if trial%2 == 0 {
			m[0], m[1] = 1, 2
		} else {
			m[1], m[0] = 2, 1
		}
		overwritten := false
		for k, v := range m {
			if k == 0 {
				want := "+0"
				if overwritten {
					want = "-0"
					after++
					if v != 3 {
						bad++
					}
				} else {
					before++
				}
				if sign(k) != want {
					bad++
				}
			}
			if !overwritten {
				m[negZero] = 3
				overwritten = true
			}
		}
	}
	fmt.Println("range under overwrite: inconsistent", bad, "both orders seen", after > 0 && before > 0)

	// The same through an interface-keyed map.
	bad, after, before = 0, 0, 0
	for trial := 0; trial < 400; trial++ {
		m := map[any]int{}
		if trial%2 == 0 {
			m[0.0], m["one"] = 1, 2
		} else {
			m["one"], m[0.0] = 2, 1
		}
		overwritten := false
		for k, v := range m {
			if f, ok := k.(float64); ok {
				want := "+0"
				if overwritten {
					want = "-0"
					after++
					if v != 3 {
						bad++
					}
				} else {
					before++
				}
				if sign(f) != want {
					bad++
				}
			}
			if !overwritten {
				m[negZero] = 3
				overwritten = true
			}
		}
	}
	fmt.Println("interface range under overwrite: inconsistent", bad, "both orders seen", after > 0 && before > 0)
}

func try(label string, f func()) {
	defer func() {
		if r := recover(); r != nil {
			// Both Go 1.26 values satisfy runtime.Error: maps.unhashableTypeError (the empty- and nil-map
			// doors) and runtime.errorString (the hash). A recovered plain string would print false.
			_, isRuntimeError := r.(runtime.Error)
			fmt.Println(label+": panic:", r, "runtime.Error:", isRuntimeError)
		}
	}()
	f()
	fmt.Println(label + ": no panic")
}

func hashPanic() {
	var slice []int
	empty := map[any]bool{}
	full := map[any]bool{1: true, "a": true}
	var nilMap map[any]bool

	try("empty map lookup []int", func() { _ = empty[slice] })
	try("empty map comma-ok map key", func() { _, _ = empty[map[string]int{}] })
	try("empty map delete func key", func() { delete(empty, func() {}) })
	try("empty map assign []int", func() { empty[slice] = true })
	try("full map lookup []int", func() { _ = full[slice] })
	try("full map assign []int", func() { full[[]string{"x"}] = true })
	try("nil map lookup []int", func() { _ = nilMap[slice] })
	try("nil map delete []int", func() { delete(nilMap, slice) })
	try("hashable key on empty map", func() { _ = empty[[2]int{1, 2}] })
	fmt.Println("full map len after:", len(full), "empty map len after:", len(empty))
}

func reflectSide() {
	m := reflect.MakeMap(reflect.MapOf(reflect.TypeFor[any](), reflect.TypeFor[bool]()))
	var slice []int
	try("reflect MapIndex []int on empty MapOf(any)", func() { m.MapIndex(reflect.ValueOf(slice)) })
	try("reflect SetMapIndex []int", func() { m.SetMapIndex(reflect.ValueOf(slice), reflect.ValueOf(true)) })
	nilm := reflect.Zero(reflect.TypeFor[map[any]bool]())
	try("reflect MapIndex []int on nil map", func() { nilm.MapIndex(reflect.ValueOf(slice)) })
	try("reflect delete []int on nil map", func() { nilm.SetMapIndex(reflect.ValueOf(slice), reflect.Value{}) })

	fm := reflect.MakeMap(reflect.MapOf(reflect.TypeFor[float64](), reflect.TypeFor[bool]()))
	fm.SetMapIndex(reflect.ValueOf(0.0), reflect.ValueOf(true))
	fm.SetMapIndex(reflect.ValueOf(negZero), reflect.ValueOf(true))
	iter := fm.MapRange()
	for iter.Next() {
		fmt.Println("reflect float64 key after +0 then -0:", sign(iter.Key().Float()), "len", fm.Len())
	}
}

func main() {
	keyUpdate()
	nanKeys()
	rangeUnderOverwrite()
	hashPanic()
	reflectSide()
}
