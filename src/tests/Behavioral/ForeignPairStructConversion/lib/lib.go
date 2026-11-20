package lib

type Pitch struct {
	Hz int
}

func (p Pitch) Double() int { return p.Hz * 2 }

// Twin has Pitch's fields in Pitch's own package.
type Twin struct {
	Hz int
}

func (t Twin) Half() int { return t.Hz / 2 }

type Note struct {
	Name   string
	Pitch  Pitch
	Marks  []int
	Ref    *Pitch
	Weight float64
}

type Rest struct{}

func Middle() Pitch { return Pitch{Hz: 440} }
