package main

import (
	"bufio"
	"bytes"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
)

// Guards the deadlock report: after `fatal error: all goroutines are asleep - deadlock!`
// Go prints a blank line and one block per goroutine, a header with its wait reason
// (`goroutine 1 [select (no cases)]:`), its frames, and for a goroutine a `go` statement
// started, the `created by` line (runtime/proc.go checkdead, runtime/traceback.go).
//
// A program cannot read its own fatal report, so it re-runs itself as a child for each
// shape and prints the child's exit code and stderr with what varies by host removed:
// the directory of each file (only its base name is kept) and the `+0x` code offsets.
// Goroutine ids other than main's vary too (Go's runtime starts goroutines of its own
// before main's first `go`), so each id is printed as the order it first appears in.

func recvNil() {
	var c chan int
	<-c
}

func sendNil() {
	var c chan int
	c <- 1
}

// child takes no arguments: Go's traceback prints a frame's argument words, which no
// managed build reproduces.
func child() {
	switch os.Getenv("DEADLOCK_BLOCKS_CHILD") {
	case "select":
		select {}
	case "three":
		go recvNil()
		go sendNil()
		var c chan int
		for range c {
		}
	case "goexit":
		go recvNil()
		runtime.Goexit()
	}
}

var (
	goroutineID = regexp.MustCompile(`goroutine (\d+)`)
	offset      = regexp.MustCompile(` \+0x[0-9a-f]+$`)
)

func main() {
	if os.Getenv("DEADLOCK_BLOCKS_CHILD") != "" {
		child()
		return
	}

	for _, shape := range []string{"select", "three", "goexit"} {
		report(shape)
	}
}

func report(shape string) {
	cmd := exec.Command(os.Args[0])
	cmd.Env = append(os.Environ(), "DEADLOCK_BLOCKS_CHILD="+shape)
	var stderr bytes.Buffer
	cmd.Stderr = &stderr
	err := cmd.Run()

	code := -1
	if exit, ok := err.(*exec.ExitError); ok {
		code = exit.ExitCode()
	}
	fmt.Printf("== %s: exit code %d\n", shape, code)

	ids := map[string]int{}
	scanner := bufio.NewScanner(&stderr)
	for scanner.Scan() {
		line := scanner.Text()
		if strings.HasPrefix(line, "\t") {
			position := offset.ReplaceAllString(line[1:], "")
			if colon := strings.LastIndexByte(position, ':'); colon >= 0 {
				position = filepath.Base(position[:colon]) + position[colon:]
			}
			line = "\t" + position
		}
		line = goroutineID.ReplaceAllStringFunc(line, func(m string) string {
			id := goroutineID.FindStringSubmatch(m)[1]
			if _, seen := ids[id]; !seen {
				ids[id] = len(ids) + 1
			}
			return fmt.Sprintf("goroutine #%d", ids[id])
		})
		fmt.Println(line)
	}
}
