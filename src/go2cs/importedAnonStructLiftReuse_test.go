// importedAnonStructLiftReuse_test.go - Gbtc
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

// An alias of an anonymous struct identical to one an imported package lifted publicly names that lift: Go calls
// the two one type (sqlite's TLARGE_INTEGER and libc's largeInteger), so C# must declare one.
func TestImportedAnonStructLiftIsReused(t *testing.T) {
	isolateConverterPathGlobals(t)

	lib := `package lib

type largeInteger = struct {
	QuadPart int64
}

func Seek(distance largeInteger) int64 { return distance.QuadPart }
`
	app := `package main

import (
	"fmt"

	"example.com/app/lib"
)

type TLARGE_INTEGER = struct {
	QuadPart int64
}

func main() {
	fmt.Println(lib.Seek(TLARGE_INTEGER{QuadPart: 3}))
}
`
	emitted := convertModuleMainWithLib(t, "lib", lib, app)

	if !strings.Contains(emitted, "global using TLARGE_INTEGER = go.example.com.app.lib_package.largeIntegerᴛ1;") {
		t.Errorf("the alias does not name lib's lift:\n%s", emitted)
	}

	if strings.Contains(emitted, "struct TLARGE_INTEGERᴛ1") {
		t.Errorf("main declares a second struct:\n%s", emitted)
	}
}

// Two structs with the same text are different Go types when a field is unexported, and an internal lift cannot be
// named from another assembly: in both cases the importer keeps its own lift.
func TestImportedAnonStructLiftIsNotReusedWhenNotIdenticalOrNotPublic(t *testing.T) {
	cases := map[string]string{
		"unexported field": `package lib

type pair = struct {
	n int64
}

func Seek(p pair) int64 { return p.n }
`,
		"internal lift": `package lib

type pair = struct {
	N int64
}

func seek(p pair) int64 { return p.N }

var Seek = func(n int64) int64 { return seek(pair{N: n}) }
`,
	}

	appField := map[string]string{"unexported field": "n", "internal lift": "N"}

	for name, lib := range cases {
		t.Run(name, func(t *testing.T) {
			isolateConverterPathGlobals(t)

			field := appField[name]
			app := `package main

import (
	"fmt"

	"example.com/app/lib"
)

type pair = struct {
	` + field + ` int64
}

func main() {
	var p pair
	fmt.Println(p, lib.Seek)
}
`
			emitted := convertModuleMainWithLib(t, "lib", lib, app)

			if strings.Contains(emitted, "lib_package.pairᴛ1") {
				t.Errorf("main names lib's lift:\n%s", emitted)
			}

			if !strings.Contains(emitted, "struct pairᴛ1") {
				t.Errorf("main does not declare its own lift:\n%s", emitted)
			}
		})
	}
}
