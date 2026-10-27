// The sample program for github.com/spf13/pflag v1.0.9: a flag set parses a fixed argument list (never os.Args), so
// its output is the same on every run and every host.
package main

import (
	"fmt"
	"time"

	"github.com/spf13/pflag"
)

func main() {
	fs := pflag.NewFlagSet("demo", pflag.ContinueOnError)
	name := fs.StringP("name", "n", "world", "who to greet")
	count := fs.IntP("count", "c", 1, "how many times")
	verbose := fs.BoolP("verbose", "v", false, "say more")
	wait := fs.Duration("wait", time.Second, "how long to wait")
	tags := fs.StringSlice("tag", nil, "tags")
	err := fs.Parse([]string{"-n", "go2cs", "--count=3", "-v", "--wait", "1m30s", "--tag", "a,b", "--tag", "c", "rest", "--", "-x"})
	fmt.Println("parse:", err)
	fmt.Println(*name, *count, *verbose, *wait, *tags)
	fmt.Println("args:", fs.Args(), "changed count:", fs.Changed("count"), "changed wait:", fs.Changed("wait"))
	fmt.Println("unknown:", fs.Parse([]string{"--nope"}))
	fmt.Print(fs.FlagUsages())
}
