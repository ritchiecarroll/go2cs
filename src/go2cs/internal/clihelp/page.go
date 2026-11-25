// page.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package clihelp

import (
	"fmt"
	"strings"
	"unicode/utf8"
)

// PagePath is docs/CommandLine.md, relative to src/go2cs, where `go generate` runs.
const PagePath = "../../docs/CommandLine.md"

// Region is one generated region of docs/CommandLine.md: the lines between its begin and end markers are written from
// the table and nothing else.
type Region struct {
	Name   string
	render func(b *strings.Builder)
}

// Regions are the page's generated regions, in page order.
var Regions = []Region{
	{"common-options", writeCommonTable},
	{"option-reference", writeReference},
}

// BeginMarker and EndMarker delimit the region named name. The page carries them as HTML comments, which the published
// site does not show.
func BeginMarker(name string) string {
	return "<!-- BEGIN GENERATED " + name + ": written from src/go2cs/internal/clihelp/options.go by `go generate` in " +
		"src/go2cs. Edit options.go, not this region; TestCommandLinePageInSync fails on a hand edit. -->"
}

func EndMarker(name string) string {
	return "<!-- END GENERATED " + name + " -->"
}

// Page returns page with every generated region rewritten from the table. It keeps the page's line endings. It fails
// when a region's markers are missing, repeated or out of order.
func Page(page string) (string, error) {
	crlf := strings.Contains(page, "\r\n")
	text := strings.ReplaceAll(page, "\r\n", "\n")

	for _, region := range Regions {
		begin, end := BeginMarker(region.Name)+"\n", "\n"+EndMarker(region.Name)

		if strings.Count(text, begin) != 1 || strings.Count(text, end) != 1 {
			return "", fmt.Errorf("%s: the %s region needs exactly one begin and one end marker", PagePath, region.Name)
		}

		start := strings.Index(text, begin) + len(begin)
		stop := strings.Index(text, end)

		if stop < start-1 {
			return "", fmt.Errorf("%s: the %s region ends before it begins", PagePath, region.Name)
		}

		var b strings.Builder
		region.render(&b)
		text = text[:start] + strings.TrimSuffix(b.String(), "\n") + text[stop:]
	}

	if crlf {
		text = strings.ReplaceAll(text, "\n", "\r\n")
	}

	return text, nil
}

// RegionText returns the lines between the region's markers in page, or false when the markers are not there.
func RegionText(page, name string) (string, bool) {
	text := strings.ReplaceAll(page, "\r\n", "\n")
	begin, end := BeginMarker(name)+"\n", "\n"+EndMarker(name)
	start, stop := strings.Index(text, begin), strings.Index(text, end)

	if start < 0 || stop < start+len(begin)-1 {
		return "", false
	}

	return text[start+len(begin) : stop], true
}

// Anchor is the id of an option's sub-section in the reference. The page sets it explicitly, because the site's
// generated heading ids drop the leading dash.
func Anchor(option Option) string {
	return "option-" + option.Name
}

// markdownSyntax is the option's syntax as a code span. A pipe in it (a value such as on|off|auto) is
// escaped, since a table would otherwise read it as a cell boundary.
func markdownSyntax(option Option) string {
	return "`" + strings.ReplaceAll(syntax(option), "|", `\|`) + "`"
}

// optionHeader is a table's "Option" header padded with no-break spaces, so that the column is never narrower than
// its longest unbreakable option token. GitHub sizes the column to its widest unbreakable content when the description
// column is long, and a code span can break after any hyphen, so without the padding the column takes the width of
// the token minus its leading dash and the dash wraps alone (-recurse[=module,nuget] reads "-" then "recurse[=...").
// GitHub strips <wbr>, and a no-break hyphen or word joiner inside the token would be copied into a command the
// parser refuses, so the header is the one place the width can come from. Two no-break spaces per character, less
// six, covers a code character (0.6em at 85%) against the narrowest no-break space of the fonts GitHub asks for
// (about 0.25em, bold) with a few pixels to spare.
func optionHeader(members []Option) string {
	longest := 0

	for _, option := range members {
		for _, token := range strings.Fields(syntax(option)) {
			longest = max(longest, utf8.RuneCountInString(token))
		}
	}

	return "Option" + strings.Repeat("&nbsp;", max(0, 2*longest-6))
}

// writeCommonTable writes the Common options as a table: each name links to its sub-section, with its summary. A blank
// line separates the table from each region marker: without one the site's kramdown reads the marker comment and the
// table as one paragraph and shows the rows as text (GitHub renders the table either way).
func writeCommonTable(b *strings.Builder) {
	members := filter(func(o Option) bool { return o.Class == Common })
	fmt.Fprintf(b, "\n| %s | Description |\n|:--|:--|\n", optionHeader(members))

	for _, option := range members {
		fmt.Fprintf(b, "| [%s](#%s) | %s. |\n", markdownSyntax(option), Anchor(option), option.Summary)
	}

	b.WriteString("\n")
}

var classIntros = map[Class]string{
	Common: "What a first conversion needs: convert and build a program or module, or check a conversion " +
		"against the package's Go tests. `go2cs -help` lists these.",
	Advanced: "Options that change a default, tune a run, or drive a maintainer's instrument. `go2cs -help all` " +
		"lists them with the common ones.",
	Experimental: "Options that make output that is not validated.",
}

// writeReference writes every option under its class, Common first, then Advanced, then Experimental. A class other
// than Common opens with an index table of its options; each option then gets a sub-section with its full details.
func writeReference(b *strings.Builder) {
	for _, class := range []Class{Common, Advanced, Experimental} {
		members := filter(func(o Option) bool { return o.Class == class })

		fmt.Fprintf(b, "### %s options\n\n%s\n\n", class, classIntros[class])

		if class != Common {
			fmt.Fprintf(b, "| %s | Group | Description |\n|:--|:--|:--|\n", optionHeader(members))

			for _, option := range members {
				fmt.Fprintf(b, "| [%s](#%s) | %s | %s. |\n", markdownSyntax(option), Anchor(option), option.Group,
					option.Summary)
			}

			b.WriteString("\n")
		}

		for _, option := range members {
			fmt.Fprintf(b, "#### <a id=\"%s\"></a>%s\n\n", Anchor(option), markdownSyntax(option))
			fmt.Fprintf(b, "%s option, %s.\n\n", class, strings.ToLower(option.Group))

			for _, paragraph := range strings.Split(option.Details, "\n\n") {
				fmt.Fprintf(b, "%s\n\n", paragraph)
			}
		}
	}
}
