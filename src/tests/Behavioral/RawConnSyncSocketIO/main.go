// RawConnSyncSocketIO guards a caller's own synchronous Winsock call on a socket the poller owns.
//
// syscall.RawConn hands its callback the socket's handle, and a caller may issue Winsock calls on it
// directly. net's TestRawConnReadWrite does exactly this with a NIL overlapped: syscall.WSASend and
// syscall.WSARecv, synchronous by Winsock's contract, on a socket that internal/poll has bound to
// its completion port. Such a call carries no operation of internal/poll's, so nothing of the
// poller's (its per-operation record, its route, its event) may be asked of it.
//
//  1. a RawConn write (WSASend, nil overlapped) arrives at the peer, read back the ordinary way;
//  2. a RawConn read (WSARecv, nil overlapped) of what the peer wrote the ordinary way;
//  3. the ordinary path still works on both sockets after the raw calls;
//  4. two connections exchanging raw writes and reads at the same time, so nothing is shared
//     between sockets for the length of a synchronous call.
//
// Every step runs under a guard, so a hang is printed by name rather than stalling the run.
package main

import (
	"fmt"
	"io"
	"net"
	"sync"
	"syscall"
	"time"
)

func writeRawConn(c syscall.RawConn, b []byte) error {
	var operr error
	err := c.Write(func(s uintptr) bool {
		var written uint32
		buf := syscall.WSABuf{Len: uint32(len(b)), Buf: &b[0]}
		operr = syscall.WSASend(syscall.Handle(s), &buf, 1, &written, 0, nil, nil)
		return true
	})
	if err != nil {
		return err
	}
	return operr
}

func readRawConn(c syscall.RawConn, b []byte) (int, error) {
	var operr error
	var n int
	err := c.Read(func(s uintptr) bool {
		var read uint32
		var flags uint32
		var buf syscall.WSABuf
		buf.Buf = &b[0]
		buf.Len = uint32(len(b))
		operr = syscall.WSARecv(syscall.Handle(s), &buf, 1, &read, &flags, nil, nil)
		n = int(read)
		return true
	})
	if err != nil {
		return n, err
	}
	return n, operr
}

// pair returns the two ends of one loopback TCP connection.
func pair() (*net.TCPConn, *net.TCPConn) {
	ln, err := net.Listen("tcp", "127.0.0.1:0")
	if err != nil {
		panic(fmt.Sprintf("Listen: %v", err))
	}
	defer ln.Close()

	accepted := make(chan net.Conn, 1)
	go func() {
		c, err := ln.Accept()
		if err != nil {
			panic(fmt.Sprintf("Accept: %v", err))
		}
		accepted <- c
	}()

	client, err := net.Dial("tcp", ln.Addr().String())
	if err != nil {
		panic(fmt.Sprintf("Dial: %v", err))
	}

	return client.(*net.TCPConn), (<-accepted).(*net.TCPConn)
}

func rawOf(c *net.TCPConn) syscall.RawConn {
	rc, err := c.SyscallConn()
	if err != nil {
		panic(fmt.Sprintf("SyscallConn: %v", err))
	}
	return rc
}

// readFull reads exactly n bytes the ordinary way.
func readFull(c *net.TCPConn, n int) string {
	b := make([]byte, n)
	if _, err := io.ReadFull(c, b); err != nil {
		return "<" + err.Error() + ">"
	}
	return string(b)
}

// rawReadFull reads exactly n bytes through RawConn.
func rawReadFull(c *net.TCPConn, n int) string {
	rc := rawOf(c)
	got := make([]byte, 0, n)
	b := make([]byte, n)

	for len(got) < n {
		k, err := readRawConn(rc, b[:n-len(got)])
		if err != nil {
			return string(got) + "<" + err.Error() + ">"
		}
		if k == 0 {
			return string(got) + "<EOF>"
		}
		got = append(got, b[:k]...)
	}

	return string(got)
}

func step(name string, body func() string) {
	done := make(chan string, 1)
	go func() { done <- body() }()

	select {
	case got := <-done:
		fmt.Printf("%s: %s\n", name, got)
	case <-time.After(10 * time.Second):
		fmt.Printf("%s: HANG (no return in 10s)\n", name)
	}
}

func main() {
	client, server := pair()
	defer client.Close()
	defer server.Close()

	step("1 raw write", func() string {
		if err := writeRawConn(rawOf(client), []byte("raw hello")); err != nil {
			return "write: " + err.Error()
		}
		return readFull(server, len("raw hello"))
	})

	step("2 raw read", func() string {
		if _, err := server.Write([]byte("plain reply")); err != nil {
			return "write: " + err.Error()
		}
		return rawReadFull(client, len("plain reply"))
	})

	step("3 ordinary after raw", func() string {
		if _, err := client.Write([]byte("ping")); err != nil {
			return "client write: " + err.Error()
		}
		got := readFull(server, 4)
		if _, err := server.Write([]byte("pong")); err != nil {
			return "server write: " + err.Error()
		}
		return got + " " + readFull(client, 4)
	})

	step("4 two connections at once", func() string {
		const rounds = 200
		a1, a2 := pair()
		b1, b2 := pair()
		defer a1.Close()
		defer a2.Close()
		defer b1.Close()
		defer b2.Close()

		results := make([]string, 2)
		var wg sync.WaitGroup
		run := func(i int, from, to *net.TCPConn, tag byte) {
			defer wg.Done()
			msg := []byte{tag, tag, tag, tag}
			for r := 0; r < rounds; r++ {
				if err := writeRawConn(rawOf(from), msg); err != nil {
					results[i] = fmt.Sprintf("round %d write: %v", r, err)
					return
				}
				if got := rawReadFull(to, len(msg)); got != string(msg) {
					results[i] = fmt.Sprintf("round %d read %q", r, got)
					return
				}
			}
			results[i] = fmt.Sprintf("%c ok", tag)
		}

		wg.Add(2)
		go run(0, a1, a2, 'a')
		go run(1, b1, b2, 'b')
		wg.Wait()

		return results[0] + ", " + results[1]
	})
}
