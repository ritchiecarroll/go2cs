// MnMutexTwin: 7a's red row for sync's hand twin Lockꞏtwin (DESIGN-mn-twin-set.md 12.4, P3's suspect, confirmed by
// a dump of MnHttpClosure hung under GO2CS_MN=1: main parked on SyncMutexLock). With sync built with twins, an
// UNCONTENDED Lock reached through sync's own twinned code (Once.Do's slow path, RWMutex.Lock) must not park. Run
// under GO2CS_MN=1 it prints the same three lines as Go; with the old-protocol twin it hangs before "once".
package main

import (
	"fmt"
	"sync"
)

var once sync.Once

func main() {
	ch := make(chan int, 1)
	go func() { ch <- 1 }()
	<-ch // main parks once, so it runs as its twin

	once.Do(func() { fmt.Println("once") })

	var rw sync.RWMutex
	rw.Lock()
	rw.Unlock()
	fmt.Println("rwmutex")

	var mu sync.Mutex
	mu.Lock()
	mu.Unlock()
	fmt.Println("done")
}
