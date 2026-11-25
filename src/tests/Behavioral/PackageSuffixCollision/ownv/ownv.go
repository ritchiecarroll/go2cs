// Package ownv declares a VAR spelled like its own package class.
package ownv

var ownv_package = 3

// Get reads and writes that var.
func Get() int { ownv_package++; return ownv_package }
