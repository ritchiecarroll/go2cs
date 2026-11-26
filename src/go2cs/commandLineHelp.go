// commandLineHelp.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The converter's help. Every option's text lives in one table, internal/clihelp/options.go: the flag set reports
// each option's summary from it, `go2cs -help` prints it, and docs/CommandLine.md is generated from it.

package main

//go:generate go run ./internal/gencommandline

import (
	"errors"
	"flag"
	"fmt"
	"io"
	"strings"

	"go2cs/internal/clihelp"
)

// helpRequest reports whether args ask for help, and the topic asked about: "" for the common help, "all" for every
// option, or an option name. Help is -h, -help or --help anywhere before a "--". The topic follows an equals sign, or
// is the next argument when that is "all" or names an option, with or without its dash; any other next argument, such
// as the input of `go2cs -h pkg_dir out`, leaves the common help.
func helpRequest(args []string) (topic string, ok bool) {
	for i, arg := range args {
		if arg == "--" {
			return "", false
		}

		if !strings.HasPrefix(arg, "-") {
			continue
		}

		name, value, hasValue := strings.Cut(strings.TrimLeft(arg, "-"), "=")

		if name != "h" && name != "help" {
			continue
		}

		if hasValue {
			return value, true
		}

		if i+1 < len(args) {
			if next := args[i+1]; strings.EqualFold(next, "all") {
				return next, true
			} else if _, known := clihelp.Lookup(next); known {
				return next, true
			}
		}

		return "", true
	}

	return "", false
}

// writeHelp prints the help topic names, from the flags in commandLine, to stdout. It returns the process exit code:
// 0, or 1 when the topic names no option, after an error and the nearest option names on stderr.
func writeHelp(commandLine *flag.FlagSet, topic string, stdout, stderr io.Writer) int {
	switch strings.ToLower(topic) {
	case "":
		clihelp.WriteCommon(stdout)
		return 0
	case "all":
		clihelp.WriteAll(stdout)
		return 0
	}

	option, ok := clihelp.Lookup(topic)

	if !ok {
		fmt.Fprintf(stderr, "go2cs: there is no option -%s.%s\n", strings.TrimLeft(topic, "-"), suggestion(topic))
		fmt.Fprintln(stderr, "Run go2cs -help all for every option.")
		return 1
	}

	defaultValue := ""

	if f := commandLine.Lookup(option.Name); f != nil {
		defaultValue = f.DefValue
	}

	clihelp.WriteOption(stdout, option, defaultValue)
	return 0
}

// suggestion returns " Did you mean ...?" naming the options closest to name, or "" when none is close.
func suggestion(name string) string {
	names := clihelp.Suggest(name)

	switch len(names) {
	case 0:
		return ""
	case 1:
		return fmt.Sprintf(" Did you mean %s?", names[0])
	default:
		return fmt.Sprintf(" Did you mean %s or %s?", strings.Join(names[:len(names)-1], ", "), names[len(names)-1])
	}
}

// undefinedFlagPrefix is how the flag package reports an option it does not know.
const undefinedFlagPrefix = "flag provided but not defined: "

// writeUsageError prints a command-line mistake on stderr: err when there is one, else that no input was given, then
// the nearest option names for an unknown option and where the help is. A run with no arguments at all prints the
// common help instead, since it is most often someone looking for it.
func writeUsageError(err error, args []string, stderr io.Writer) {
	if err == nil && len(args) == 0 {
		clihelp.WriteCommon(stderr)
		return
	}

	switch {
	case err == nil:
		fmt.Fprintln(stderr, "go2cs: no input given.")
	case strings.HasPrefix(err.Error(), undefinedFlagPrefix):
		name := strings.TrimPrefix(err.Error(), undefinedFlagPrefix)
		fmt.Fprintf(stderr, "go2cs: there is no option %s.%s\n", name, suggestion(name))
	case errors.Is(err, flag.ErrHelp):
		// Not reached: helpRequest answers -h and -help before the flags are parsed.
		clihelp.WriteCommon(stderr)
		return
	default:
		fmt.Fprintf(stderr, "go2cs: %s\n", err)
	}

	fmt.Fprintf(stderr, "%s\nRun go2cs -help for the common options, or go2cs -help all for every option.\n",
		clihelp.Usage)
}
