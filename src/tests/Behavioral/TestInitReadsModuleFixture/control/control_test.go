package control_test

import (
	"os"
	"testing"

	"go2cs/TestInitReadsModuleFixture/control"
)

// CONTROL: the same relative read inside the test body, which already runs in the staged sandbox.
func TestKeyReadInTest(t *testing.T) {
	key, err := os.ReadFile("keys/k")
	if err != nil {
		t.Fatal(err)
	}
	if got := control.Key(key); got != "secret" {
		t.Fatalf("got %q", got)
	}
}
