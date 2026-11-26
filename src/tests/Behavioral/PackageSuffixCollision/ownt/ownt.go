// Package ownt declares a TYPE spelled like its own package class, ownt_package.
package ownt

// ownt_package is a package-level type named <ownpkg>_package.
type ownt_package int

// Double is a method on that type.
func (o ownt_package) Double() ownt_package { return o * 2 }

// Get uses the type from inside its own package.
func Get(n int) int { return int(ownt_package(n).Double()) }
