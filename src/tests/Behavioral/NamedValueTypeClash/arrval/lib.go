// Package arrval declares a named ARRAY type called Value. Its pointer-receiver methods index, range
// and slice through the receiver, which the converter emits through the wrapper's value property.
package arrval

type Value [3]int

func (v Value) First() int { return v[0] }

func (v *Value) Set(i, x int) { v[i] = x }

func (v *Value) Bump() {
	for i := range v {
		v[i]++
	}
}

func (v *Value) Sum() (s int) {
	for _, x := range v {
		s += x
	}
	return
}

func (v *Value) Head() []int { return v[:2] }
