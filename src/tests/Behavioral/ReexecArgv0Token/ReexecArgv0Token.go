// ReexecArgv0Token: argv0 round trip, the logrus alt_exit_test re-exec pattern: the parent re-execs its own
// executable with argv[0] set to a TOKEN, and the child knows it is the child ONLY by that token.
// SAFETY CAP: every generation carries ARGV0_DEPTH; past 3 the process refuses and exits, so a
// runtime that loses argv[0] produces three refusals in the output, never a fork bomb.
package main

import (
	"fmt"
	"os"
	"os/exec"
	"strconv"
	"strings"
)

const token = "argv0-token-7f3a"

func main() {
	depth, _ := strconv.Atoi(os.Getenv("ARGV0_DEPTH"))
	if depth > 3 {
		fmt.Printf("depth %d: REFUSED (cap)\n", depth)
		os.Exit(3)
	}
	if os.Args[0] == token {
		fmt.Printf("depth %d: child saw the token in os.Args[0]; extra args %q\n", depth, os.Args[1:])
		os.Exit(0)
	}
	if depth > 0 {
		fmt.Printf("depth %d: child did NOT see the token (os.Args[0] ends %q); re-executing again\n", depth, tail(os.Args[0]))
	}
	exe, err := os.Executable()
	if err != nil {
		fmt.Println("os.Executable:", err)
		os.Exit(1)
	}
	cmd := exec.Command(exe)
	cmd.Path = exe
	cmd.Args = []string{token, "-marker", "x"}
	cmd.Env = append(os.Environ(), "ARGV0_DEPTH="+strconv.Itoa(depth+1))
	out, err := cmd.CombinedOutput()
	fmt.Print(string(out))
	if depth == 0 {
		fmt.Printf("parent: child exit error = %v\n", err)
	}
}

func tail(s string) string {
	if i := strings.LastIndex(s, "/"); i >= 0 {
		return s[i+1:]
	}
	return s
}
