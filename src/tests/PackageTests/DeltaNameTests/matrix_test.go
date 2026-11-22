package deltanametests

import (
	"reflect"
	"testing"
)

// ΔHelper is declared by the white-box test file, beside the production ΔMatrix.
type ΔHelper struct{ Δh int }

func (h ΔHelper) Name() string { return "ΔHelper" }

func TestWhiteboxNames(t *testing.T) {
	for _, tc := range []struct {
		n    Namer
		want string
		typ  string
	}{
		{newMatrix(1), "Matrix 1", "deltanametests.Matrix"},
		{ΔNew(2), "ΔMatrix 2", "deltanametests.ΔMatrix"},
		{ΔHelper{3}, "ΔHelper", "deltanametests.ΔHelper"},
		{&Matrix{4}, "Matrix 4", "*deltanametests.Matrix"},
		{&ΔMatrix{5}, "ΔMatrix 5", "*deltanametests.ΔMatrix"},
	} {
		if got := tc.n.Name(); got != tc.want {
			t.Errorf("Name() = %q, want %q", got, tc.want)
		}
		if got := reflect.TypeOf(tc.n).String(); got != tc.typ {
			t.Errorf("type = %q, want %q", got, tc.typ)
		}
	}
	if got := reflect.TypeOf(ΔHelper{}).Field(0).Name; got != "Δh" {
		t.Errorf("field = %q, want Δh", got)
	}
}
