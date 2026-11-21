// Package scoped is named with a C# contextual keyword that C# refuses as a type or alias name.
package scoped

// Name reports which package answered.
func Name() string { return "scoped" }

// Kind is a type the importer names through the package alias.
type Kind int

// Double returns twice k.
func (k Kind) Double() Kind { return k * 2 }
