package main

import (
	"bufio"
	"bytes"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"time"
)

// Guards the crash report of a panic in a goroutine other than main: Go prints the
// CRASHING goroutine's own header, `goroutine N [running]:` with its real id, and under
// its frames the `go` statement that started it, `created by main.child in goroutine 1`
// with that statement's position on a tab-indented line beneath (runtime/traceback.go's
// goroutineheader and printcreatedby). A report that says `goroutine 1` for every crash,
// and names no creator, points a reader at the wrong goroutine.
//
// A program cannot read its own crash report, so the program re-runs itself as a child
// that crashes and prints, from the child's stderr, only what is the same on every host:
// the panic line, whether the header names a goroutine other than main, the crashing
// function, the `created by` line, and the base name of the file under it. The goroutine
// id itself varies (Go's runtime starts goroutines of its own before main's first `go`),
// so only "not 1" is printed, as Go's own runtime tests match `goroutine \d+`.

func main() {
	if os.Getenv("CRASH_GOROUTINE_CHILD") == "1" {
		child()
		return
	}
	parent()
}

func child() {
	go worker()
	time.Sleep(time.Minute)
}

func worker() {
	panic("boom in worker")
}

var header = regexp.MustCompile(`^goroutine (\d+) \[running\]:$`)

func parent() {
	cmd := exec.Command(os.Args[0])
	cmd.Env = append(os.Environ(), "CRASH_GOROUTINE_CHILD=1", "GOTRACEBACK=single")
	var stderr bytes.Buffer
	cmd.Stderr = &stderr
	err := cmd.Run()

	code := -1
	if exit, ok := err.(*exec.ExitError); ok {
		code = exit.ExitCode()
	}
	fmt.Println("child exit code:", code)

	var lines []string
	scanner := bufio.NewScanner(&stderr)
	for scanner.Scan() {
		lines = append(lines, scanner.Text())
	}

	for i, line := range lines {
		switch {
		case strings.HasPrefix(line, "panic: "):
			fmt.Println(line)
		case header.MatchString(line):
			id := header.FindStringSubmatch(line)[1]
			fmt.Println("header names a goroutine other than main:", id != "1")
			if i+1 < len(lines) {
				fmt.Println("crashing frame:", lines[i+1])
			}
		case strings.HasPrefix(line, "created by "):
			fmt.Println(line)
			if i+1 < len(lines) {
				position := strings.TrimPrefix(lines[i+1], "\t")
				if space := strings.IndexByte(position, ' '); space >= 0 {
					position = position[:space]
				}
				if colon := strings.LastIndexByte(position, ':'); colon >= 0 {
					position = position[:colon]
				}
				fmt.Println("created at file:", filepath.Base(position))
			}
		}
	}
}
