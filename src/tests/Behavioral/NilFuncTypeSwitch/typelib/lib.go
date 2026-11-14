// typelib supplies named func types of ANOTHER package, each implementing the importing package's interface by
// value, and one struct that implements it by pointer (the typed nil control).
package typelib

// Step is a named func.
type Step func(int) int

// Name implements the importing package's interface by value.
func (s Step) Name() string { return "step" }

// StepE is a named func that also implements error by value.
type StepE func(int) int

// Name implements the importing package's interface by value.
func (x StepE) Name() string { return "stepe" }

// Error implements the error interface by value.
func (x StepE) Error() string { return "stepe error" }

// Box is a struct that implements the interface by POINTER.
type Box struct {
	Tag string
}

// Name implements the importing package's interface by pointer.
func (b *Box) Name() string { return "box" }

// NewStep constructs a Step.
func NewStep() Step {
	return func(x int) int { return x + 1 }
}
