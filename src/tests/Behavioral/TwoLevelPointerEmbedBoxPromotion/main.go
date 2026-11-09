package main

// A pointer-receiver method whose converted primary is a BOX receiver (`this ж<T>`, because the
// method takes the address of one of its fields) promoted through a POINTER embed. It is the
// go-winio pipe shape: win32MessageBytePipe embeds win32Pipe, which embeds *win32File, and the
// net.Conn implementation for *win32MessageBytePipe calls Close and Set*Deadline two levels
// down. A second chain embeds a type named `file`, which C# escapes as `@file` (os.File's shape),
// one level down.

import (
	"errors"
	"fmt"
	"io"
	"net"
	"sync"
	"time"
)

// rawFile is the bottom of the chain, like go-winio's win32File.
type rawFile struct {
	mu     sync.Mutex
	closed bool
	rd, wd deadline
	data   []byte
}

// deadline is like go-winio's deadlineHandler: its set method captures its receiver, so a
// *rawFile method that calls it takes the field's address.
type deadline struct{ t time.Time }

func (d *deadline) set(t time.Time) error {
	fire := func() { d.t = t }
	fire()
	return nil
}

func (f *rawFile) Close() error {
	f.mu.Lock()
	f.closed = true
	f.mu.Unlock()
	return nil
}

func (f *rawFile) Read(b []byte) (int, error)         { return copy(b, f.data), nil }
func (f *rawFile) Write(b []byte) (int, error)        { f.data = append(f.data, b...); return len(b), nil }
func (f *rawFile) SetReadDeadline(t time.Time) error  { return f.rd.set(t) }
func (f *rawFile) SetWriteDeadline(t time.Time) error { return f.wd.set(t) }
func (f *rawFile) Flush() error                       { return nil }

type addr string

func (a addr) Network() string { return "pipe" }
func (a addr) String() string  { return string(a) }

// pipe embeds *rawFile, like win32Pipe.
type pipe struct {
	*rawFile
	path string
}

func (p *pipe) LocalAddr() net.Addr  { return addr(p.path) }
func (p *pipe) RemoteAddr() net.Addr { return addr(p.path) }
func (p *pipe) Disconnect() error    { return nil }

func (p *pipe) SetDeadline(t time.Time) error {
	_ = p.SetReadDeadline(t)
	return p.SetWriteDeadline(t)
}

// msgPipe embeds pipe by value and overrides Read and Write, like win32MessageBytePipe.
type msgPipe struct {
	pipe
	writeClosed bool
}

func (m *msgPipe) CloseWrite() error { m.writeClosed = true; return nil }

func (m *msgPipe) Write(b []byte) (int, error) {
	if m.writeClosed {
		return 0, errors.New("closed")
	}
	return m.rawFile.Write(b)
}

func (m *msgPipe) Read(b []byte) (int, error) { return m.rawFile.Read(b) }

// PipeConn is go-winio's PipeConn: net.Conn plus two more methods.
type PipeConn interface {
	net.Conn
	Disconnect() error
	Flush() error
}

var (
	_ PipeConn = (*pipe)(nil)
	_ PipeConn = (*msgPipe)(nil)
	_ net.Conn = (*msgPipe)(nil)
)

func use(c PipeConn) {
	_, _ = c.Write([]byte("hi"))
	b := make([]byte, 4)
	n, _ := c.Read(b)
	when := time.Unix(100, 0)
	_ = c.SetReadDeadline(when)
	_ = c.SetWriteDeadline(when)
	_ = c.SetDeadline(when)
	_ = c.Flush()
	fmt.Println(n, string(b[:n]), c.LocalAddr().String(), c.RemoteAddr().Network())
	_ = c.Close()
}

// file is named like os's file type, which C# escapes as @file.
type file struct {
	mu sync.Mutex
	n  int
}

func (f *file) bump() int {
	f.mu.Lock()
	defer f.mu.Unlock()
	f.n++
	return f.n
}

// handle embeds *file, like os.File.
type handle struct {
	*file
}

type bumper interface{ bump() int }

func main() {
	f1 := &rawFile{}
	use(&pipe{rawFile: f1, path: `\.\pipe\a`})
	fmt.Println(f1.closed, f1.rd.t.Unix(), f1.wd.t.Unix())

	f2 := &rawFile{}
	m := &msgPipe{pipe: pipe{rawFile: f2, path: `\.\pipe\b`}}
	use(m)
	fmt.Println(f2.closed, f2.rd.t.Unix(), f2.wd.t.Unix(), m.CloseWrite() == nil, m.writeClosed)

	// A pointer on the path puts the promoted method in the VALUE method set too.
	f3 := &rawFile{}
	var c io.Closer = msgPipe{pipe: pipe{rawFile: f3}}
	_ = c.Close()
	fmt.Println(f3.closed)

	h := handle{&file{}}
	var b bumper = &h
	fmt.Println(b.bump(), b.bump())
	b = h
	fmt.Println(b.bump(), h.n)

	// The same method sets read at run time. A value msgPipe has Close through the pointer on its
	// path, but not Read or Write: its own Read and Write take a pointer receiver and shadow
	// rawFile's, so it is an io.Closer and not a net.Conn.
	var x any = msgPipe{pipe: pipe{rawFile: &rawFile{}}}
	_, isCloser := x.(io.Closer)
	_, isConn := x.(net.Conn)
	x = &msgPipe{pipe: pipe{rawFile: &rawFile{}}}
	_, isPipeConn := x.(PipeConn)
	x = h
	_, isBumper := x.(bumper)
	fmt.Println(isCloser, isConn, isPipeConn, isBumper)
}
