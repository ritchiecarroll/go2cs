// Package vertex is the Tour of Go's Vertex (methods), as a library package.
package vertex

import "math"

// Vertex is a point in the plane.
type Vertex struct {
	X, Y float64
}

// Abs returns the distance of v from the origin.
func (v Vertex) Abs() float64 {
	return math.Sqrt(v.X*v.X + v.Y*v.Y)
}

// Scale multiplies both coordinates of v by f.
func (v *Vertex) Scale(f float64) {
	v.X = v.X * f
	v.Y = v.Y * f
}
