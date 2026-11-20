// tempNamedAfterIdentifier_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

// A switch tag or a select channel that is one plain identifier is read the way Go wrote it: in place when nothing
// can change it between the label tests, otherwise through a temp named after it (Tour visual-parity item 3).
const tempNamedAfterIdentifierSource = `package main

import (
	"fmt"
	"strings"
)

var mode = "fast"

func inPlace(os string) {
	switch os {
	case "darwin":
		fmt.Println("macOS")
	case "linux":
		fmt.Println("linux")
	}
}

func callsInLabel(label string) {
	switch label {
	case strings.ToLower("A"):
		fmt.Println("a")
	case "b":
		fmt.Println("b")
	}
}

func bodyWritesTag(n string) {
	switch n {
	case "x":
		n = "y"
		fallthrough
	case "y":
		fmt.Println(n)
	}
}

func captured(held string) func() string {
	switch held {
	case "a":
		fmt.Println("a")
	case "b":
		fmt.Println("b")
	}

	return func() string { return held }
}

func addressTaken(p string) *string {
	q := &p

	switch p {
	case "a":
		fmt.Println("a")
	case "b":
		fmt.Println("b")
	}

	return q
}

func packageVar() {
	switch mode {
	case "fast":
		fmt.Println("fast")
	case "slow":
		fmt.Println("slow")
	}
}

func keyword(base string) {
	f := func() string { return base }

	switch base {
	case "a":
		fmt.Println(f())
	case "b":
		fmt.Println("b")
	}
}

func selects(c chan int, quit chan int) {
	select {
	case c <- 1:
		fmt.Println("sent")
	case v := <-quit:
		fmt.Println(v)
	case v := <-c:
		fmt.Println(v)
	}
}

func main() {
	inPlace("linux")
	callsInLabel("b")
	bodyWritesTag("x")
	captured("a")
	addressTaken("a")
	packageVar()
	keyword("a")
	selects(make(chan int, 1), make(chan int))
}
`

func TestSwitchAndSelectTempsAreNamedAfterTheIdentifier(t *testing.T) {
	isolateConverterPathGlobals(t)
	emitted := strings.Join(convertWithComments(t, tempNamedAfterIdentifierSource), "\n")

	for _, want := range []string{
		`case {} when os == "darwin"u8:`, // in place: a parameter, no call in a label, not captured
		"var labelᴛ1 = label;",           // a label calls
		"var nᴛ1 = n;",                   // a fallthrough body writes the tag
		"var heldᴛ1 = held;",             // read by a func literal
		"var pᴛ1 = p;",                   // address-taken
		"var modeᴛ1 = mode;",             // a package variable
		"var baseᴛ1 = @base;",            // a C# keyword: the temp drops the escape
		"var cᴛ1 = c.ᐸꟷ(1, ꓸꓸꓸ);",        // a send case, named after its channel
		"var quitᴛ1 = quit;",
		"var cᴛ2 = c;", // a second case on one channel
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("missing %q", want)
		}
	}

	for _, unwanted := range []string{"var osᴛ1", "var exprᴛ", "var selᴛ"} {
		if strings.Contains(emitted, unwanted) {
			t.Errorf("found %q:\n%s", unwanted, emitted)
		}
	}
}
