package b

import "testing"

// The test file imports nothing from package a. It only calls the Client from b.
func TestLookup(t *testing.T) {
	c := new(Client)

	lines, err := c.Lookup("example.com/m", "v1.0.0")
	if err != nil {
		t.Fatal(err)
	}

	if len(lines) != 1 {
		t.Fatalf("got %d lines, want 1", len(lines))
	}
}
