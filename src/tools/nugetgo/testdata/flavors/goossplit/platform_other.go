//go:build !windows

package goossplit

// Platform reports the platform family this flavor was converted for.
func Platform() string { return "unix" }
