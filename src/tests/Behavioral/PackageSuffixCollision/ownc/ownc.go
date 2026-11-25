// Package ownc declares a CONST spelled like its own package class.
package ownc

const ownc_package = 4

// Get reads that const.
func Get() int { return ownc_package * 10 }
