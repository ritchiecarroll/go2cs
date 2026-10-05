// ReflectFuncChanDir guards the channel DIRECTION of a func signature as reflect reads it. A C# signature
// drops it -- `func(c <-chan int)` and `func(c chan int)` both take a channel<nint> -- so go2cs-gen reads
// the `/*<-*/` marker the converter writes beside each such parameter and result and emits it beside the
// method's declaring type ([GoSigChanDir]), where reflect finds it by method. Every row below reads a
// direction back: a declared func used as a value, a send and a receive parameter, a multi-value result,
// an interface method, a value-receiver method and a pointer-receiver method, func-type identity and
// assignability, and go-cmp's EqualMethod shape (a named receive channel assignable to a method's
// `<-chan` parameter). The control is MakeFunc over a nil func var, which has no method behind it.
package main

import (
	"context"
	"fmt"
	"reflect"
	"time"
)

type R struct{}

func (R) Equal(y <-chan bool) bool { return true }

type S struct{ n int }

func (s *S) Feed(c chan<- int) { s.n++ }

// Notifier's member is an INTERFACE method carrying a direction.
type Notifier interface{ Done() <-chan struct{} }

type AssignD <-chan bool

func (x AssignD) Equal(y <-chan bool) bool { return true }

func recvLocal(c <-chan int)                {}
func bidiLocal(c chan int)                  {}
func sendOnly(c chan<- string)              {}
func two() (<-chan int, error)              { return nil, nil }
func mixed(a int, c chan<- int) <-chan bool { return nil }

// nestedDir's parameter is a send channel OF receive channels: only the OUTER direction is carried
// (the recorded residual), so its type is not printed below; GenTests reads its entry.
func nestedDir(c chan<- <-chan int) {}

func main() {
	var after func(time.Duration) <-chan time.Time = time.After
	fmt.Println("func value:", reflect.TypeOf(after), reflect.TypeOf(after).Out(0).ChanDir())

	fmt.Println("recv param:", reflect.TypeOf(recvLocal), reflect.TypeOf(recvLocal).In(0).ChanDir())
	fmt.Println("send param:", reflect.TypeOf(sendOnly), reflect.TypeOf(sendOnly).In(0).ChanDir())
	fmt.Println("results:", reflect.TypeOf(two), "|", reflect.TypeOf(mixed))

	done, _ := reflect.TypeOf((*context.Context)(nil)).Elem().MethodByName("Done")
	fmt.Println("interface method:", done.Type, done.Type.Out(0).ChanDir())

	nd, _ := reflect.TypeOf((*Notifier)(nil)).Elem().MethodByName("Done")
	fmt.Println("own interface method:", nd.Type, nd.Type.Out(0).ChanDir())

	eq, _ := reflect.TypeOf(R{}).MethodByName("Equal")
	fmt.Println("value method:", eq.Type, eq.Type.In(1).ChanDir())

	feed, _ := reflect.TypeOf(&S{}).MethodByName("Feed")
	fmt.Println("pointer method:", feed.Type, feed.Type.In(1).ChanDir())

	fmt.Println("identical:", reflect.TypeOf(recvLocal) == reflect.TypeOf(bidiLocal))
	fmt.Println("assignable:", reflect.TypeOf(bidiLocal).AssignableTo(reflect.TypeOf(recvLocal)))
	fmt.Println("convertible:", reflect.TypeOf(bidiLocal).ConvertibleTo(reflect.TypeOf(recvLocal)))

	// go-cmp's EqualMethod/AssignD shape: a named receive channel is assignable to `<-chan bool`.
	ad, _ := reflect.TypeOf(AssignD(nil)).MethodByName("Equal")
	fmt.Println("named recv to param:", reflect.TypeOf(AssignD(nil)).AssignableTo(ad.Type.In(1)))

	// control: MakeFunc over a nil func var (no method behind it) still builds and asserts back.
	var h func() <-chan int
	h = reflect.MakeFunc(reflect.TypeOf(h), func([]reflect.Value) []reflect.Value {
		return []reflect.Value{reflect.ValueOf(make(chan int))}
	}).Interface().(func() <-chan int)
	fmt.Println("makefunc:", h() != nil)
	_ = nestedDir
}
