// MnMutexTwin: 7a's red row for sync's hand twin Lockꞏtwin (DESIGN-mn-twin-set.md 12.4-12.5, P3's suspect,
// confirmed by a dump of MnHttpClosure hung under GO2CS_MN=1: main parked on SyncMutexLock). With sync built with
// twins, an UNCONTENDED Lock reached through sync's own twinned code (Once.Do's slow path, RWMutex.Lock) must not
// park; with the old-protocol twin it hangs before "once". The last section (P3's read, finding 3) is CONTENDED: a
// goroutine parks inside once2.Do, holding the Once's mutex, and main's once2.Do waits on it through sync's twinned
// slow path (doSlowꞏtwin awaits Lockꞏtwin, which takes the lock through Lock and holds main's carrier until the
// holder, woken by a timer, returns). Go runs the first Do's function only, so "never" is never printed.
package main

import (
	"fmt"
	"sync"
	"time"
)

var once sync.Once
var once2 sync.Once

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

	locked := make(chan bool)
	release := make(chan bool)
	finished := make(chan bool)
	go func() {
		once2.Do(func() {
			locked <- true
			<-release // the holder parks inside Do, holding the Once's mutex
		})
		finished <- true
	}()
	<-locked
	go func() {
		time.Sleep(20 * time.Millisecond)
		release <- true
	}()
	once2.Do(func() { fmt.Println("never") }) // contended: Once's twinned slow path awaits Lockꞏtwin
	<-finished
	fmt.Println("contended")
}
