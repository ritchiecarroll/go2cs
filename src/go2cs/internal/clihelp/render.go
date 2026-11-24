// render.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Package clihelp holds the converter's command-line options as one table, and renders it two ways: as the help text
// `go2cs -help` prints, and as the two generated regions of docs/CommandLine.md. One table is the point: the help and
// the page are written from the same entries, so they cannot say different things.
package clihelp

import (
	"fmt"
	"io"
	"regexp"
	"sort"
	"strings"
)

// Class says who an option is for. It decides where the option is listed: `go2cs -help` lists the Common options, and
// the page's reference sorts Common first, then Advanced, then Experimental.
type Class int

const (
	// Common options are what a first-time user needs to convert and build a program or module, or to check a
	// conversion against the package's Go tests.
	Common Class = iota
	// Advanced options change a default, tune a run, or drive a maintainer's instrument.
	Advanced
	// Experimental options make output that is not validated.
	Experimental
)

func (c Class) String() string {
	switch c {
	case Common:
		return "Common"
	case Advanced:
		return "Advanced"
	default:
		return "Experimental"
	}
}

// Option is one command-line option.
type Option struct {
	Name    string // without the dash
	Value   string // the value syntax after the name, such as "<dir>"; empty for a switch
	Class   Class
	Group   string // one of Groups
	Summary string // one line, no Markdown, no final period
	Details string // the full explanation, in Markdown; paragraphs are separated by a blank line
}

// Groups orders options inside a class.
var Groups = []string{"Converting", "Testing", "NuGet mapping", "Output", "Multi-platform and census", "Diagnostics",
	"Information", "Development"}

// Lookup returns the option named name, with or without its leading dash.
func Lookup(name string) (Option, bool) {
	name = strings.TrimLeft(name, "-")

	for _, option := range options {
		if option.Name == name {
			return option, true
		}
	}

	return Option{}, false
}

// Summary returns the one-line summary of the option named name, the text the flag set reports as its usage. It
// panics on a name the table does not hold, so a flag added without an entry fails at start-up and in every test.
func Summary(name string) string {
	option, ok := Lookup(name)

	if !ok {
		panic("clihelp: no entry for option -" + name)
	}

	return option.Summary
}

// Options returns every option in table order: by class, then by group, then as listed.
func Options() []Option {
	return append([]Option(nil), options...)
}

// Suggest returns the option names closest to name, nearest first: a name name is a prefix of, or one at most two
// edits away. It returns nothing when no name is that close.
func Suggest(name string) []string {
	name = strings.TrimLeft(name, "-")

	type scored struct {
		name  string
		score int
	}

	var candidates []scored

	for _, option := range options {
		switch distance := editDistance(name, option.Name); {
		case distance <= 2:
			candidates = append(candidates, scored{option.Name, distance})
		case len(name) >= 3 && strings.HasPrefix(option.Name, name):
			candidates = append(candidates, scored{option.Name, 3})
		}
	}

	sort.SliceStable(candidates, func(i, j int) bool { return candidates[i].score < candidates[j].score })

	var names []string

	for i, candidate := range candidates {
		if i == 3 {
			break
		}

		names = append(names, "-"+candidate.name)
	}

	return names
}

func editDistance(a, b string) int {
	previous := make([]int, len(b)+1)
	current := make([]int, len(b)+1)

	for j := range previous {
		previous[j] = j
	}

	for i := 1; i <= len(a); i++ {
		current[0] = i

		for j := 1; j <= len(b); j++ {
			cost := 1

			if a[i-1] == b[j-1] {
				cost = 0
			}

			current[j] = min(previous[j]+1, current[j-1]+1, previous[j-1]+cost)
		}

		previous, current = current, previous
	}

	return previous[len(b)]
}

// Usage is the first line every help form prints.
const Usage = "Usage: go2cs [options] <input> [output_dir]"

// Footer closes the common help.
const Footer = `go2cs -help all lists every option. go2cs -help <option> explains one, for example go2cs -help tests.
go2cs toolchain -help manages the Go toolchain go2cs converts with.
The full reference is docs/CommandLine.md in the go2cs repository.`

// examples are the common help's examples: what a first conversion looks like.
var examples = [][2]string{
	{"go2cs package_dir out", "convert one package"},
	{"go2cs -recurse=nuget module_dir out", "convert a module, with the standard library from NuGet"},
	{"go2cs -tests -test-action all pkg_dir out", "convert a package and its tests, run them, compare with go test"},
	{"go2cs -stdlib fmt strings", "convert standard library packages"},
}

const width = 100

// WriteCommon writes the help `go2cs -help` prints: the usage line, the Common options, examples and the footer.
func WriteCommon(w io.Writer) {
	fmt.Fprintf(w, "%s\n\nCommon options:\n", Usage)
	writeList(w, filter(func(o Option) bool { return o.Class == Common }))
	fmt.Fprintf(w, "\nExamples:\n")

	for _, example := range examples {
		fmt.Fprintf(w, "  %-44s %s\n", example[0], example[1])
	}

	fmt.Fprintf(w, "\n%s\n", Footer)
}

// WriteAll writes the help `go2cs -help all` prints: every option, one line each, under its class and group.
func WriteAll(w io.Writer) {
	fmt.Fprintf(w, "%s\n", Usage)

	for _, section := range sections() {
		fmt.Fprintf(w, "\n%s:\n", section.title)
		writeList(w, section.options)
	}

	fmt.Fprintf(w, "\ngo2cs -help <option> explains one option.\n")
}

// WriteOption writes the help `go2cs -help <option>` prints. defaultValue is the flag's default as the flag set
// reports it; a switch that is off by default and an empty default print no default line.
func WriteOption(w io.Writer, option Option, defaultValue string) {
	fmt.Fprintf(w, "%s\n\n", syntax(option))
	fmt.Fprintf(w, "  %s\n", option.Summary)
	fmt.Fprintf(w, "  %s option, %s.", option.Class, strings.ToLower(option.Group))

	if defaultValue != "" && defaultValue != "false" {
		fmt.Fprintf(w, " Default: %s", defaultValue)
	}

	fmt.Fprintf(w, "\n")

	for _, paragraph := range strings.Split(option.Details, "\n\n") {
		fmt.Fprintf(w, "\n%s\n", wrap(PlainText(paragraph), width, "  "))
	}
}

type section struct {
	title   string
	options []Option
}

// sections splits the table for `-help all`: one section for the Common options, then one per group of each other
// class.
func sections() []section {
	var result []section

	result = append(result, section{"Common options", filter(func(o Option) bool { return o.Class == Common })})

	for _, class := range []Class{Advanced, Experimental} {
		for _, group := range Groups {
			members := filter(func(o Option) bool { return o.Class == class && o.Group == group })

			if len(members) > 0 {
				result = append(result, section{fmt.Sprintf("%s options: %s", class, strings.ToLower(group)), members})
			}
		}
	}

	return result
}

func filter(keep func(Option) bool) []Option {
	var result []Option

	for _, option := range options {
		if keep(option) {
			result = append(result, option)
		}
	}

	return result
}

func syntax(option Option) string {
	switch {
	case option.Value == "":
		return "-" + option.Name
	case strings.HasPrefix(option.Value, "["):
		return "-" + option.Name + option.Value
	default:
		return "-" + option.Name + " " + option.Value
	}
}

// writeList writes one line per option: its syntax, then its summary, wrapped under a common column.
func writeList(w io.Writer, list []Option) {
	column := 0

	for _, option := range list {
		column = max(column, len(syntax(option)))
	}

	column = min(column, 30)

	for _, option := range list {
		name := syntax(option)
		indent := strings.Repeat(" ", 2+column+2)

		if len(name) > column {
			fmt.Fprintf(w, "  %s\n%s\n", name, wrap(option.Summary, width, indent))
			continue
		}

		text := wrap(option.Summary, width, indent)
		fmt.Fprintf(w, "  %-*s  %s\n", column, name, strings.TrimLeft(text, " "))
	}
}

var (
	commentPattern = regexp.MustCompile(`(?s)<!--.*?-->`)
	linkPattern    = regexp.MustCompile(`\[([^\]]*)\]\([^)]*\)`)
	strongPattern  = regexp.MustCompile(`\*\*([^*]+)\*\*`)
	spacePattern   = regexp.MustCompile(`\s+`)
)

// PlainText turns one paragraph of Markdown into terminal text: HTML comments go, a link keeps its text, strong
// emphasis and code spans lose their marks, and a backslash escape keeps the character it escapes.
func PlainText(markdown string) string {
	text := commentPattern.ReplaceAllString(markdown, "")
	text = linkPattern.ReplaceAllString(text, "$1")
	text = strongPattern.ReplaceAllString(text, "$1")
	text = strings.ReplaceAll(text, "`", "")
	text = strings.ReplaceAll(text, `\`, "")

	return strings.TrimSpace(spacePattern.ReplaceAllString(text, " "))
}

// wrap fills text to width columns, every line starting with indent.
func wrap(text string, width int, indent string) string {
	var lines []string
	line := indent

	for _, word := range strings.Fields(text) {
		if len(line) > len(indent) && len(line)+1+len(word) > width {
			lines = append(lines, line)
			line = indent
		}

		if len(line) > len(indent) {
			line += " "
		}

		line += word
	}

	return strings.Join(append(lines, line), "\n")
}
