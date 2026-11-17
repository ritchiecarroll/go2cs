// CmsgStress: syscall.UnixRights writes each descriptor through an interior address of a fresh byte slice.
package main

import (
	"fmt"
	"runtime"
	"syscall"
)

var sink [][]byte

func churn(stop chan struct{}) {
	for {
		select {
		case <-stop:
			return
		default:
		}
		for i := 0; i < 200; i++ {
			sink = append(sink, make([]byte, 512))
			if len(sink) > 128 {
				sink = sink[:0]
			}
		}
		runtime.GC()
	}
}

func main() {
	fds := make([]int, 64)
	for i := range fds {
		fds[i] = 1000 + i
	}
	stop := make(chan struct{})
	for g := 0; g < 3; g++ {
		go churn(stop)
	}
	bad := 0
	for n := 0; n < 300000; n++ {
		b := syscall.UnixRights(fds...)
		scms, err := syscall.ParseSocketControlMessage(b)
		if err != nil || len(scms) != 1 {
			bad++
			continue
		}
		got, err := syscall.ParseUnixRights(&scms[0])
		if err != nil || len(got) != len(fds) {
			bad++
			continue
		}
		for i := range got {
			if got[i] != fds[i] {
				bad++
				break
			}
		}
	}
	close(stop)
	fmt.Println("messages with a wrong descriptor:", bad)
}
