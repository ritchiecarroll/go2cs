// Package extension is named with a C# contextual keyword that C# refuses as a type or alias name.
package extension

// Name reports which package answered.
func Name() string { return "extension" }

// Kind is a type the importer names through the package alias.
type Kind int

// Double returns twice k.
func (k Kind) Double() Kind { return k * 2 }

// extension is a type spelled like the package; C# refuses it as a type name, so this package renames the name.
type extension struct{ n int }

// Scaled returns n times ten through a value of that type.
func Scaled(n int) int {
	v := extension{n}
	return v.n * 10
}
