// g1_unix.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build unix

package main

/*
#include <sys/utsname.h>
#include <time.h>
#include <unistd.h>
*/
import "C"

import "fmt"

// osArm runs one of this OS's own arms, and reports whether arm names one.
func osArm(arm string) bool {
	switch arm {
	case "copyback":
		// uname writes a whole struct through its pointer: the struct is copied back, inline arrays and all.
		var u C.struct_utsname
		r := C.uname(&u)
		var name []byte

		for i := 0; i < len(u.sysname) && u.sysname[i] != 0; i++ {
			name = append(name, byte(u.sysname[i]))
		}

		fmt.Println("copyback", int(r), string(name))
	case "close":
		// The errno form with a scalar result: EBADF.
		r, err := C.close(C.int(-1))
		fmt.Println("close", int(r), errnoOf(err))
	case "mktime":
		// &x of a struct that holds a pointer (tm_zone): cmd/cgo's address form of its pointer check.
		var tm C.struct_tm
		tm.tm_year = C.int(2026 - 1900)
		tm.tm_mon = C.int(11 - 1)
		tm.tm_mday = C.int(12)
		tm.tm_isdst = -1
		fmt.Println("mktime", int64(C.mktime(&tm)))
	default:
		return false
	}

	return true
}
