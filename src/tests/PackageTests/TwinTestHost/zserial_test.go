package twintesthost

import "testing"

// TestSerialParks is arm G's second test (test-twin-test-host.ps1), after G's control on 5015b12085. It must run AFTER
// TestParallelParks, so it lives in a file whose name sorts after twin_test.go: go test, and the converted host with it,
// run the tests in file-name order, and a serial test run first finds t.Parallel's gate not yet held. At GOMAXPROCS=1
// the parallel test's carrier holds the only P at that gate, this twin waits for a P, and the scheduler retakes the P
// (sensed >= 1).
func TestSerialParks(t *testing.T) {
	if got := Sum(200); got != 19900 {
		t.Fatalf("Sum(200) = %d, want 19900", got)
	}
}
