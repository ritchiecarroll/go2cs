// MnLockFair is row 7 pieces 2+3's fairness row (COORD's gate): N goroutines (MN_LF_N, default 4,000) contend ONE
// sync.Mutex, half through the field, `c.mu.Lock()`, which a twinned program parks on (sync's box-receiver
// Lockꞏtwin), and half through a sync.Locker, whose interface call keeps the plain Lock and BLOCKS its thread. Both
// kinds wait on the mutex's one gate, and every goroutine must acquire: the count and the sum print, EQUAL to Go's,
// and the run ends inside its deadline. Go's starvation mode (a waiter starved for 1 ms takes the lock next) is not
// modelled; the gate serves waiters in SemaphoreSlim's order. Read: GO2CS_MN=1 GO2CS_MN_READING=1, parks AND
// thread-blocks above zero (both kinds of waiter were present).
package main

import (
	"fmt"
	"os"
	"strconv"
	"sync"
	"time"
)

type counter struct {
	mu   sync.Mutex
	sum  int
	n    int
	park int
	plug int
}

func parked(c *counter, id int, wg *sync.WaitGroup) {
	defer wg.Done()
	time.Sleep(time.Millisecond)
	c.mu.Lock()
	c.sum += id
	c.n++
	c.park++
	c.mu.Unlock()
}

func blocked(c *counter, l sync.Locker, id int, wg *sync.WaitGroup) {
	defer wg.Done()
	time.Sleep(time.Millisecond)
	l.Lock()
	c.sum += id
	c.n++
	c.plug++
	l.Unlock()
}

func main() {
	n := 4000
	if v, err := strconv.Atoi(os.Getenv("MN_LF_N")); err == nil && v > 0 {
		n = v
	}

	c := &counter{}
	var l sync.Locker = &c.mu
	var wg sync.WaitGroup
	wg.Add(n)

	for i := 0; i < n; i++ {
		if i%2 == 0 {
			go parked(c, i, &wg)
		} else {
			go blocked(c, l, i, &wg)
		}
	}

	wg.Wait()
	fmt.Println("n:", c.n, "sum:", c.sum, "match:", c.sum == n*(n-1)/2)
	fmt.Println("through the field:", c.park, "through a Locker:", c.plug)
}
