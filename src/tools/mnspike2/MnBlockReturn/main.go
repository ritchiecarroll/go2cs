// MnBlockReturn is the s_p convoy row's fixture (COORD's ruling, inbox/G 20261122T110220Z item 2): it BLOCKS threads on
// purpose and returns them ALL AT ONCE. N goroutines (MN_BR_N, default 20,000) each wait on one sync.WaitGroup, a plain
// call in a twinned program (only Mutex.Lock and time.Sleep have twins), so each blocks its thread and gives its P back;
// main releases them with ONE Done, and every one of them takes a P back in GoScheduler.ExitBlocked at the same moment:
// the shape of the train-4 storm capture (19,228 returns queued on the one P semaphore, 7,201 spinning on its monitor).
// Then each adds its id under a Mutex. stderr reads PHASES: spawn-ms (all blocked) and return-ms (release to done, the
// convoy). Read under GO2CS_MN=1 GO2CS_MN_READING=1 also: peak OS threads. The stdout is EQUAL to Go's.
package main

import (
	"fmt"
	"os"
	"strconv"
	"sync"
	"time"
)

type counter struct {
	mu  sync.Mutex
	sum int
	n   int
}

func worker(c *counter, id int, arrived, release, done *sync.WaitGroup) {
	defer done.Done()
	arrived.Done()
	release.Wait()
	c.mu.Lock()
	c.sum += id
	c.n++
	c.mu.Unlock()
}

func main() {
	n := 20000
	if v, err := strconv.Atoi(os.Getenv("MN_BR_N")); err == nil && v > 0 {
		n = v
	}

	c := &counter{}
	var arrived, release, done sync.WaitGroup
	arrived.Add(n)
	release.Add(1)
	done.Add(n)

	t0 := time.Now()

	for i := 0; i < n; i++ {
		go worker(c, i, &arrived, &release, &done)
	}

	arrived.Wait()
	t1 := time.Now()
	release.Done()
	done.Wait()
	t2 := time.Now()
	fmt.Fprintln(os.Stderr, "PHASES spawn-ms="+strconv.FormatInt(t1.Sub(t0).Milliseconds(), 10)+" return-ms="+strconv.FormatInt(t2.Sub(t1).Milliseconds(), 10))
	fmt.Println("n:", c.n, "sum:", c.sum, "match:", c.sum == n*(n-1)/2)
}
