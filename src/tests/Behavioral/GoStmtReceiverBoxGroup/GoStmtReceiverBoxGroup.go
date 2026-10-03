// A go statement in method-group form on a pointer method's OWN receiver binds the receiver box's method
// group, so the enclosing method is promoted to direct-ж (gopkg.in/yaml.v3's tests). A value field chain
// rooted at the receiver takes the same promotion; a pointer PARAMETER has its own box.

package main

import "fmt"

type inner struct{ ch chan int }

func (in *inner) send() { in.ch <- 3 }

type tracker struct {
	done chan int
	n    int
	in   inner
}

func (t *tracker) loop()         { t.done <- t.n }
func (t *tracker) loopArg(k int) { t.done <- k }
func (t *tracker) bump()         { t.n++ }

func (t *tracker) start()           { go t.loop() }
func (t *tracker) startArg()        { go t.loopArg(9) }
func (t *tracker) startInner()      { go t.in.send() }
func (t *tracker) stop()            { defer t.bump() }
func (t *tracker) other(o *tracker) { go o.loop() }

func main() {
	t := &tracker{done: make(chan int), n: 7, in: inner{ch: make(chan int)}}
	t.start()
	fmt.Println(<-t.done)
	t.startArg()
	fmt.Println(<-t.done)
	t.startInner()
	fmt.Println(<-t.in.ch)
	t.stop()
	fmt.Println(t.n)
	o := &tracker{done: make(chan int), n: 11}
	t.other(o)
	fmt.Println(<-o.done)
}
