package twintesthost

import "testing"

// TestParks parks on a channel and in a select. Run as its twin under GO2CS_MN=1, the reading counts its parks.
func TestParks(t *testing.T) {
	c := make(chan int)
	done := make(chan struct{})
	go func() {
		for i := 0; i < 100; i++ {
			c <- i
		}
		close(done)
	}()

	sum := 0
	for {
		select {
		case v := <-c:
			sum += v
		case <-done:
			// Every send completed before done closed (c is unbuffered), so all 100 values were received.
			if sum != 4950 {
				t.Fatalf("sum = %d, want 4950", sum)
			}
			return
		}
	}
}

// TestFatalInTwin parks, then calls t.Fatal on the test's own goroutine. DELIBERATELY FAILS: the host must report a
// failed test, not an infrastructure error, when the call comes from inside the twin.
func TestFatalInTwin(t *testing.T) {
	if got := Sum(10); got != 45 {
		t.Fatalf("Sum(10) = %d, want 45", got)
	}
	t.Fatal("twin fatal")
}

// TestFatalFromGoroutine calls t.Fatal from a goroutine the test started, not the test's own. DELIBERATELY refused by
// the host under the twin too: its owner check compares goroutines, and this is another goroutine. The method value
// keeps go vet's testinggoroutine check from refusing the package.
func TestFatalFromGoroutine(t *testing.T) {
	fatal := t.Fatal
	done := make(chan struct{})
	go func() {
		defer close(done)
		fatal("from a goroutine the test started")
	}()
	<-done
}

// TestPlainOnly never parks, so it has no twin and runs plain even under GO2CS_MN=1: the near-miss.
func TestPlainOnly(t *testing.T) {
	if got := 2 + 2; got != 4 {
		t.Fatalf("2 + 2 = %d", got)
	}
}

// TestParallelParks waits at t.Parallel's gate (a managed wait in a twin) and then parks.
func TestParallelParks(t *testing.T) {
	t.Parallel()
	if got := Sum(50); got != 1225 {
		t.Fatalf("Sum(50) = %d, want 1225", got)
	}
}
