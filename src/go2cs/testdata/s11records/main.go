// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The fixture for docs/PLAN-marker-comment-parity.md section 11: the inline attributes that leave converted code
// for a record (s11Records_test.go).
package main

import (
	"crypto"
	"fmt"
	"reflect"
)

// A defined type over an interface is emitted as a `using` alias, so a field of it is recorded with its
// descriptor carrier: a local one, an imported one, a grouped pair and an embedded one.
type Token any

type Named fmt.Stringer

type holder struct {
	Tok   Token
	Key   crypto.PublicKey
	A, B  Named
	Plain any
	Token
}

func main() {
	// Function-local defined types of each wrapper kind keep their Go name in a record.
	type local []byte
	type localMap map[string]int
	type localNum int
	type localPtr *int

	fmt.Println(reflect.TypeFor[local]().Name(), reflect.TypeFor[localMap]().Name(), reflect.TypeFor[localNum]().Name(), reflect.TypeFor[localPtr]().Name())

	t := reflect.TypeFor[holder]()
	for i := 0; i < t.NumField(); i++ {
		f := t.Field(i)
		fmt.Printf("%s %q %v\n", f.Name, f.Type.Name(), f.Anonymous)
	}
}
