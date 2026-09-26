// Package AliasImportLib exports plain type aliases, a struct and func types of several shapes, for
// AliasImport to consume.
package AliasImportLib

import "time"

type Box struct{ V int }

// Exported aliases of a struct and of func types: basic, same-package, cross-package, no result,
// multiple results, named results, variadic.
type B2 = Box
type IntFn = func(int) int
type BoxFn = func(Box) Box
type DurFn = func(time.Duration) int
type Act = func()
type Multi = func(int) (int, error)
type Named = func(int) (n int, ok bool)
type Var = func(...int) int

func ApplyVar(f Var, xs ...int) int { return f(xs...) }
