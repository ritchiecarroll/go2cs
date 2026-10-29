// Package lib declares a struct and never spells a pointer to it, so go2cs-gen gives the struct no
// generated operations face (IGoTypeOpsSource): TypeOpsScope grants the face only to a type whose
// pointer its own compilation spells. The pointer to it is spelled in package main, a different
// compilation, which is the shape seat #34's first gap names.
package lib

// Hidden is reached only through main's *lib.Hidden field.
type Hidden struct {
	A int
	B string
}
