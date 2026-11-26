// Package ownf declares a FUNC spelled like its own package class.
package ownf

func ownf_package() int { return 5 }

// Get calls that func.
func Get() int { return ownf_package() + 1 }
