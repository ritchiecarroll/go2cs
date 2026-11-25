// MnLaterArg: the repro of 12.1's later-parking-argument probe row (DESIGN-mn-box-twin.md 12.1, P4's probe,
// 2026-11-16). gc evaluates the plain variable operand pp AFTER the receive in the same argument list; go2cs's C#
// reads it left to right, before. The program observes it without a race: the write to pp happens before the send,
// the send before the receive.
//
// go run (1.26.10, linux/amd64):        param-later-arg 1405
// converted, plain emission (no twins):  param-later-arg 1505
// converted, twins with byref-param:     param-later-arg 1505 (the twin keeps the plain emission's order)
// under GoTwinsBoxCopy / GoTwinsBoxArgCopy: param-later-arg 105
//
// A BOARD row (COORD 2026-11-16 15:12Z): a converter design for .5, not .4. Kept out of the behavioral corpus, where
// its Output phase would be red under every arm.
//
// In the Q1 kit (run-q1-kit.sh) this project builds the twins arm only, so the kit makes no compare for it. 1505 is
// the plain-path operand-order gap (BOARD row "Plain-path operand order"), the known baseline, and not a difference
// the twins make. Measured 2026-11-24 (go1.26.11, linux/amd64, 4 cores, Release, 20 runs each):
//   twins, GO2CS_MN=1:      1505 in 20 of 20
//   plain, GO2CS_MN unset:  1505 in 17 of 20, 1405 in 3 of 20
// The plain reading varies because the converted code reads pp BEFORE the receive, so the read races the goroutine
// that writes pp; Go reads pp after the receive, which the write happens before, so Go has no race. A twin-vs-plain
// compare of this program would therefore read EQUAL most runs and differ on some. Once the operand-order fix
// lands, both arms read 1405, as go run does, every run.
package main

import "fmt"

type cell struct{ v int }

var ready = make(chan struct{})
var resume = make(chan struct{})

func readWith(c *cell, k int) int {
	ready <- struct{}{}
	<-resume
	return c.v*100 + k
}

func writer(write func()) {
	<-ready
	write()
	resume <- struct{}{}
}

func main() {
	pc1, pc2 := &cell{v: 1}, &cell{v: 2}
	pp := pc1
	kc := make(chan int)
	go func() { pp = pc2; kc <- 5 }()
	go writer(func() { pc1.v = 15; pc2.v = 14 })
	fmt.Println("param-later-arg", readWith(pp, <-kc))
}
