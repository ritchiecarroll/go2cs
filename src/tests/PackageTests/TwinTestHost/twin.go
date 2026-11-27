// Package twintesthost is the end-to-end fixture for the test host's twin path (docs/phase4/DESIGN-mn-test-twin.md).
// Its test project is built with twins (GoTwinsFor) and run with GO2CS_MN=1, so a test whose body parks runs as its
// twin, multiplexed, and the scheduler's reading counts its parks. test-twin-test-host.ps1 drives it.
package twintesthost

// Sum receives the values 0..n-1 from a producer goroutine and adds them. Every send and every receive parks.
func Sum(n int) int {
	c := make(chan int)
	go func() {
		for i := 0; i < n; i++ {
			c <- i
		}
		close(c)
	}()

	sum := 0
	for v := range c {
		sum += v
	}
	return sum
}
