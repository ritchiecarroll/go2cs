// Package deltanametests is the -tests fixture for Go names that start with Δ: Matrix beside ΔMatrix, and a test-file
// type beside a production one, through the white-box bridge and the external test package.
package deltanametests

import "fmt"

// Matrix and ΔMatrix are two types one Δ apart; Go exports both.
type Matrix struct{ m int }

type ΔMatrix struct{ Δn int }

// Namer is satisfied by both.
type Namer interface{ Name() string }

func (x Matrix) Name() string { return fmt.Sprint("Matrix ", x.m) }

func (x ΔMatrix) Name() string { return fmt.Sprint("ΔMatrix ", x.Δn) }

// ΔNew builds a ΔMatrix; ΔScale is a pointer method.
func ΔNew(n int) ΔMatrix { return ΔMatrix{n} }

func (x *ΔMatrix) ΔScale(k int) { x.Δn *= k }

func newMatrix(m int) Matrix { return Matrix{m} }
