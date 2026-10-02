package main

import (
	"fmt"
	"io"
	"os"
)

// A function literal bound with := keeps its DECLARED signature, here a result list that names an
// interface while the body returns the concrete *os.File, and so can be passed to a parameter of
// that function type afterwards.

func use(open func(string) (io.ReadCloser, error)) {
	rc, err := open("no-such-file-for-go2cs")
	fmt.Println(rc == nil, err != nil)
}

func main() {
	open := func(name string) (io.ReadCloser, error) {
		return os.Open(name)
	}

	use(open)
}
