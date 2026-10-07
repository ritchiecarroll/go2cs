// Package rec declares structs for a defined type in ANOTHER package to be defined over.
package rec

// Hidden holds an unexported field, so its struct type carries this package's path.
type Hidden struct {
	N int
	s string
}

// Open holds only exported fields, so its struct type carries no package path.
type Open struct {
	N int
	S string
}

func NewHidden(n int, s string) Hidden { return Hidden{N: n, s: s} }

func (h Hidden) S() string { return h.s }
