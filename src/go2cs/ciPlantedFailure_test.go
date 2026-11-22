package main

import "testing"

// SCRATCH (claude/c1-scratch-ci-red, never merges): the planted failure that proves CI on master reads red.
func TestCIPlantedFailure(t *testing.T) {
	t.Fatal("planted: CI on master must read this red")
}
