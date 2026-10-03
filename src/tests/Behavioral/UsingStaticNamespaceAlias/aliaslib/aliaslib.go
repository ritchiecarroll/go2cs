// Package aliaslib exports Go names that two .NET namespaces also declare as types: Unsafe and
// Closure (System.Runtime.CompilerServices), Marshal (System.Runtime.InteropServices).
package aliaslib

// Unsafe is a package variable, as go/types' own Unsafe is.
var Unsafe = "unsafe-var"

// Closure is an exported function.
func Closure(n int) int { return n * 3 }

// Marshal is an exported function, as encoding/json's is.
func Marshal(s string) string { return "<" + s + ">" }
