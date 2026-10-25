//go:build linux || darwin

package main

import (
	"fmt"
	"io"
	"net"
)

// WritevIovecSeam guards the STRUCT-PASSING seam for writev's iovec ARRAY.
//
// net.Buffers.WriteTo is the path that reaches internal/poll's writev with a MULTI-element
// iovec vector, and the converted syscall.Iovec holds its Base as a managed reference -- a
// struct with a reference field gets AUTO layout from the CLR and is non-blittable, so handing
// the kernel &iovecs[0] made it read 16 bytes per element that are neither {void*; size_t} nor
// in that field order.
//
// The shape below is the one that MEASURED the defect on net's own row: TEN ONE-BYTE buffers,
// so a per-iovec layout fault shows as wrong CONTENT at the right LENGTH. That distinction is
// the whole point of the guard -- a short write would be a different bug, and the failing
// emission delivered ten 0x38s where 0x00..0x09 were sent.
func main() {
	ln, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		fmt.Println("listen:", err)
		return
	}
	defer ln.Close()

	done := make(chan []byte, 1)
	go func() {
		c, err := ln.Accept()
		if err != nil {
			done <- nil
			return
		}
		defer c.Close()
		got, _ := io.ReadAll(c)
		done <- got
	}()

	c, err := net.Dial("tcp", ln.Addr().String())
	if err != nil {
		fmt.Println("dial:", err)
		return
	}

	// Ten SEPARATE one-byte buffers -- ten iovecs, each with its own base.
	bufs := net.Buffers{}
	for i := 0; i < 10; i++ {
		bufs = append(bufs, []byte{byte(i)})
	}

	n, err := bufs.WriteTo(c)
	c.(*net.TCPConn).CloseWrite()
	fmt.Println("wrote:", n, "err:", err)

	got := <-done
	fmt.Println("len:", len(got))
	fmt.Println("bytes:", got)

	ordered := len(got) == 10
	for i := 0; ordered && i < 10; i++ {
		ordered = got[i] == byte(i)
	}
	fmt.Println("each iovec delivered its own byte in order:", ordered)
	c.Close()
}
