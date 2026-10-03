package fixture_test

import (
	"os"
	"testing"

	"go2cs/TestInitReadsModuleFixture/fixture"
)

// A test package's init reads a fixture by RELATIVE path, from a directory that is not testdata/
// (github.com/golang-jwt/jwt/v5's hmac_example_test.go: os.ReadFile("test/hmacTestKey")). `go test`
// starts the binary in the package's source directory, so the read succeeds before any test runs.
var key []byte

func init() {
	data, err := os.ReadFile("keys/k")
	if err != nil {
		panic(err)
	}
	key = data
}

func TestKeyReadAtInit(t *testing.T) {
	if got := fixture.Key(key); got != "secret" {
		t.Fatalf("got %q", got)
	}
}
