// Package goossplit is a nugetgo-pack.ps1 fixture: its exported Platform function has a different body on Windows,
// so its converted assemblies differ in meaning between flavors and the package must carry RID-specific assets.
package goossplit

// Describe names the platform the package was built for.
func Describe() string { return "built for " + Platform() }
