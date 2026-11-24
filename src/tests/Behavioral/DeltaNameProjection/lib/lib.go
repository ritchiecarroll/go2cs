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

// Lower's method is Δ followed by a lower-case letter: exported in Go, since Δ itself is upper-case.
type Lower interface{ Δfoo() string }

// CallLower calls Δfoo through the interface.
func CallLower(f Lower) string { return f.Δfoo() }

// ΔEmbL has a method Δm that another package's struct promotes through an embed.
type ΔEmbL struct{ N int }

// Δm is exported.
func (e ΔEmbL) Δm() string { return "Δm of ΔEmbL" }

// Δrec is an exported type whose name is Δ followed by a lower-case letter.
type Δrec struct{ Δt int }

// Δ is an exported type, a bare Δ, with a bare Δ field.
type Δ struct{ Δ int }
