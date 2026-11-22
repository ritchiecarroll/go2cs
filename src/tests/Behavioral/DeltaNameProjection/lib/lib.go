// Package lib declares Δ-led names that another package uses: Go exports a name whose first letter is an upper-case
// Unicode letter, and Δ is one, so Δt, ΔF and ΔK are exported.
package lib

// ΔK is an exported constant.
const ΔK = 7

// ΔV is an exported variable.
var ΔV = 3

// Rec has an exported Δ-led field.
type Rec struct{ Δt int }

// ΔF is an exported function.
func ΔF(n int) int { return n * 2 }

// Fooer's method is Δ-led; main's S satisfies it from another package.
type Fooer interface{ ΔFoo() string }

// Call calls ΔFoo through the interface.
func Call(f Fooer) string { return f.ΔFoo() }
