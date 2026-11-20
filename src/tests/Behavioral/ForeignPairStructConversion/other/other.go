package other

import "ForeignPairStructConversion/lib"

type Pitch struct {
	Hz int
}

func (p Pitch) Octave() int { return p.Hz * 2 }

// Tagged differs from Pitch by a field tag only, which a conversion ignores.
type Tagged struct {
	Hz int `json:"hz"`
}

type Note struct {
	Name   string
	Pitch  lib.Pitch
	Marks  []int
	Ref    *lib.Pitch
	Weight float64
}

type Rest struct{}

// Tag is written over lib's type: its own conversion from that base, not a pair of two declarations.
type Tag lib.Pitch

func (t Tag) Triple() int { return lib.Pitch(t).Hz * 3 }

func Low() Pitch { return Pitch{Hz: 110} }
