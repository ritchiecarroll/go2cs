package guid

import "KeyedLiteralNamedForeignStruct/win"

// GUID is defined over ANOTHER package's struct, as go-winio's pkg/guid declares
// `type GUID windows.GUID`.
type GUID win.GUID

func (g GUID) First() uint32 { return g.Data1 }

// Outer is defined over this package's own UNEXPORTED struct.
type inner struct {
	A uint32
	B [2]byte
}

type Outer inner

func (o Outer) Sum() uint32 { return o.A + uint32(o.B[0]) + uint32(o.B[1]) }

// Pair builds elided literals of Outer INSIDE its declaring package, as values and through
// pointers.
func Pair() ([]Outer, []*Outer) {
	return []Outer{{A: 1}, {B: [2]byte{2}}}, []*Outer{{A: 3}}
}

// Zeros builds EMPTY literals of Outer inside its declaring package: typed, elided and through a
// pointer.
func Zeros() (Outer, []Outer, []*Outer) {
	return Outer{}, []Outer{{}}, []*Outer{{}}
}
