package lib

type Pitch struct {
	Hz int
}

func (p Pitch) Double() int { return p.Hz * 2 }

type Note struct {
	Name   string
	Pitch  Pitch
	Marks  []int
	Next   *Note
	Weight float64
}

func Middle() Pitch { return Pitch{Hz: 440} }
