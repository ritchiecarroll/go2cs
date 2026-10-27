// The sample program for github.com/spf13/cobra v1.10.2: a two-level command tree run with fixed arguments (never
// os.Args) and its output captured, so the output is the same on every run and every host.
package main

import (
	"bytes"
	"fmt"
	"strings"

	"github.com/spf13/cobra"
)

func run(args ...string) {
	var out bytes.Buffer
	root := &cobra.Command{Use: "demo", Short: "A cobra sample", SilenceUsage: true}
	var loud bool
	greet := &cobra.Command{
		Use:   "greet [name]",
		Short: "Greet someone",
		Args:  cobra.ExactArgs(1),
		RunE: func(cmd *cobra.Command, args []string) error {
			msg := "hello, " + args[0]
			if loud {
				msg = strings.ToUpper(msg)
			}
			fmt.Fprintln(cmd.OutOrStdout(), msg)
			return nil
		},
	}
	greet.Flags().BoolVarP(&loud, "loud", "l", false, "shout")
	root.AddCommand(greet)
	root.CompletionOptions.DisableDefaultCmd = true
	root.SetOut(&out)
	root.SetErr(&out)
	root.SetArgs(args)
	err := root.Execute()
	fmt.Printf("$ demo %s\n%serr: %v\n\n", strings.Join(args, " "), out.String(), err)
}

func main() {
	run("greet", "go2cs")
	run("greet", "--loud", "nugetgo")
	run("greet")
	run("--help")
}
