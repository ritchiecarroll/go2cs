package deltanametests_test

import (
	"reflect"
	"testing"

	"go2cs/deltanametests"
)

func TestExternalNames(t *testing.T) {
	x := deltanametests.ΔNew(4)
	x.ΔScale(3)
	if got := x.Name(); got != "ΔMatrix 12" {
		t.Errorf("Name() = %q, want ΔMatrix 12", got)
	}
	var m deltanametests.Matrix
	if got := reflect.TypeOf(m).Name(); got != "Matrix" {
		t.Errorf("Matrix name = %q", got)
	}
	if got := reflect.TypeOf(x).Name(); got != "ΔMatrix" {
		t.Errorf("ΔMatrix name = %q", got)
	}
	if got := reflect.TypeOf(&x).Method(0).Name; got != "Name" {
		t.Errorf("method 0 = %q, want Name", got)
	}
	if got := reflect.TypeOf(&x).Method(1).Name; got != "ΔScale" {
		t.Errorf("method 1 = %q, want ΔScale", got)
	}
	if got := reflect.TypeOf(x).Field(0).Name; got != "Δn" {
		t.Errorf("field = %q, want Δn", got)
	}
	// Both types one Δ apart, by value and by pointer, through one interface from another package.
	want := []string{"Matrix 0", "ΔMatrix 12", "Matrix 0", "ΔMatrix 12"}
	for i, n := range []deltanametests.Namer{m, x, &m, &x} {
		if got := n.Name(); got != want[i] {
			t.Errorf("Namer %d = %q, want %q", i, got, want[i])
		}
	}
}
