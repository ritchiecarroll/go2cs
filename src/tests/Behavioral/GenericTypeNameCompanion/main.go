// GenericTypeNameCompanion guards the DESCRIPTOR COMPANION — the carrier at a generic
// TYPE-ARGUMENT position, which is the one position the carrier cannot reach as attribute cargo.
//
// A Go DEFINED type over a NAMED interface is emitted as a C# `global using` alias (it has exactly
// that interface's method set and can carry no methods of its own). A `using` alias is COMPILE-TIME
// only and leaves no metadata, so the Go name is erased. Where the converter can stamp the erased
// type — a struct field — an uninhabited [GoLocalName] carrier travels as cargo
// (DescriptorCarrierFieldName guards that). A generic type ARGUMENT cannot be stamped: it is bound
// per CALL. So `nameOf[eface]()` emitted `nameOf<object>()` and answered "" where Go answers
// "eface", and — the sharper half — `nameOf[eface]` and `nameOf[any]` were the SAME CLR
// instantiation, two distinct Go instantiations the runtime could not tell apart.
//
// The remedy threads a companion type parameter (`nameOf<T, Tᴺ>`) bound to the carrier at an erased
// call site and to the type argument itself everywhere else. Hence the rows below: two that MOVE,
// and five negative controls that must not.
//
//	eface       defined over the EMPTY interface  -> was Name="", must become "eface"
//	namedIface  defined over a NAMED NON-EMPTY    -> was Name="Stringer" (a DIFFERENT Go type's
//	                                                 name — the silently wrong half), must be
//	                                                 "namedIface"
//	any         the bare empty interface          -> control: Go itself answers "", and it must
//	                                                 STILL answer "" while eface answers "eface",
//	                                                 which is the collapse actually separating
//	aliasIface  a true Go type ALIAS              -> control: Go reports the TARGET's name, so a
//	                                                 carrier here would INVENT one; stays Stringer
//	inlineIface an INLINE interface definition    -> control: already a real C# interface
//	ordinary    an ordinary defined type          -> control: never erased, never a carrier
//	sizeOf[T]   a generic that reads NO name      -> control: keeps its arity, gains nothing
package main

import (
	"fmt"
	"reflect"
)

type eface any                 // class (i): defined over the empty interface
type namedIface fmt.Stringer   // class (ii): defined over a named NON-empty interface
type aliasIface = fmt.Stringer // control: a true Go alias must NOT gain a carrier
type ordinary string           // control: an ordinary defined type
type inlineIface interface {   // control: an inline definition is a real C# interface already
	Do()
}

// nameOf reads a Go NAME off a TYPE PARAMETER — the shape that gains a companion.
func nameOf[T any](label string) {
	t := reflect.TypeFor[T]()
	fmt.Printf("%-11s Name=%q String=%q PkgPath=%q Kind=%v\n", label, t.Name(), t.String(), t.PkgPath(), t.Kind())
}

// sizeOf is the NEGATIVE control at the emission level: a generic whose body reads no name must
// keep its declared arity and gain no companion at any call site. It is exercised here so a change
// that threaded EVERY generic would break this program's own output, not merely its golden.
func sizeOf[T any](label string, v T) {
	fmt.Printf("%-11s dynamic=%T\n", label, v)
}

func main() {
	nameOf[eface]("eface")
	nameOf[namedIface]("namedIface")
	nameOf[any]("any")
	nameOf[aliasIface]("aliasIface")
	nameOf[inlineIface]("inlineIface")
	nameOf[ordinary]("ordinary")

	sizeOf("neg-ordinary", ordinary("x"))
	sizeOf[eface]("neg-eface", nil)
}
