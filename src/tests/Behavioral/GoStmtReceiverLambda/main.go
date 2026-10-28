// Guards the go-statement receiver-capture promotion: a `go` statement calling a VALUE-RETURNING
// method through the enclosing method's POINTER receiver forces the synthesized-lambda emission
// (goǃ has only void Action overloads), and that lambda references the receiver — which must
// therefore arrive as the capturable direct-ж box, not a `ref` receiver (CS1628; crypto/tls
// quic.go's `go q.conn.HandshakeContext(ctx)` inside `func (q *QUICConn) Start`). Output proves
// the goroutine ran against the ORIGINAL receiver object (write-visibility through the chain).
package main

import "fmt"

// counter is reached through a pointer field of engine; bump/report have POINTER receivers and
// return a value, so neither can bind goǃ as a bare method group.
type counter struct {
	n    int
	done chan bool
}

func (c *counter) bump(delta int) error {
	c.n += delta
	c.done <- true
	return nil
}

func (c *counter) report() error {
	c.done <- true
	return nil
}

type engine struct {
	tally *counter
}

// start exercises the ARGUMENT arm: the value-returning callee forces
// `goǃ((ᴛ0, ᴛ1) => ᴛ0.bump(ᴛ1), …tally, delta)`, the receiver chain rooted at e and evaluated at the go statement.
func (e *engine) start(delta int) {
	go e.tally.bump(delta)
}

// ping exercises the NULLARY arm (`goǃ(ᴛ0 => ᴛ0.report(), …tally)`).
func (e *engine) ping() {
	go e.tally.report()
}

// valueSender's methods have VALUE receivers: a go-statement's bare method group over such a
// method is an extension over a struct VALUE — C# forbids delegates over value-type extension
// receivers (CS1113; net/http/httputil's `go spc.copyToBackend(errc)`, switchProtocolCopier) —
// so both arities force the lambda form (`goǃ(() => vsʗ1.send(7))`;
// `goǃ(() => vsʗ2.ping())`). The receiver snapshot still evaluates at go-statement time, and
// the blocking receives prove both goroutines ran to completion.
type valueSender struct{ c chan int }

func (v valueSender) send(n int) { v.c <- n }
func (v valueSender) ping()      { v.c <- 99 }

func main() {
	e := &engine{tally: &counter{done: make(chan bool, 1)}}

	e.start(5)
	<-e.tally.done
	fmt.Println("after start:", e.tally.n)

	e.start(7)
	<-e.tally.done
	fmt.Println("after second start:", e.tally.n)

	e.ping()
	<-e.tally.done
	fmt.Println("pinged:", e.tally.n)

	vs := valueSender{c: make(chan int)}
	go vs.send(7)
	fmt.Println("value-recv go:", <-vs.c)
	go vs.ping()
	fmt.Println("value-recv nullary go:", <-vs.c)
}
