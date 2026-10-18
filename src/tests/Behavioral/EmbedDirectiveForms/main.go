package main

// The //go:embed forms the default-set packages use at go1.26.9: a string in internal/strconv's
// fp_test.go, and a []byte in crypto/internal/fips140test's acvp_test.go (inside a grouped var,
// below comment lines) and acvp_fips140v1.26_test.go (top level). Each imports embed blank.

import (
	_ "embed"
	"fmt"
	"strings"
)

// The internal/strconv form: a string, from a file under testdata.
//
//go:embed testdata/testfp.txt
var testfp string

var (
	// The acvp_test.go form: a []byte declared in a var group, with comment lines above the
	// directive.
	//go:embed capabilities.json
	entropyCapabilitiesJson []byte

	unrelated = 42
)

// The acvp_fips140v1.26_test.go form: a top-level []byte.
//
//go:embed capabilities.json
var capabilitiesJson []byte

// A second string embedding of the same file.
//
//go:embed testdata/testfp.txt
var testfpAgain string

func main() {
	fmt.Printf("testfp: %d bytes, %d lines\n", len(testfp), strings.Count(testfp, "\n"))
	for i, line := range strings.Split(strings.TrimSuffix(testfp, "\n"), "\n") {
		fmt.Printf("  %d: %q\n", i, line)
	}
	fmt.Println("testfp == testfpAgain:", testfp == testfpAgain)

	fmt.Printf("entropyCapabilitiesJson: %d bytes, %q\n", len(entropyCapabilitiesJson), entropyCapabilitiesJson)
	fmt.Printf("capabilitiesJson: %d bytes, %q\n", len(capabilitiesJson), capabilitiesJson)
	fmt.Println("unrelated:", unrelated)

	// Each []byte variable holds its own copy of the file: a write through one is not seen
	// through the other.
	entropyCapabilitiesJson[0] = '['
	fmt.Printf("after write: %c %c\n", entropyCapabilitiesJson[0], capabilitiesJson[0])

	var sum int
	for _, b := range capabilitiesJson {
		sum += int(b)
	}
	fmt.Println("byte sum:", sum)
}
