// g1_windows.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

/*
#include <windows.h>
*/
import "C"

import "fmt"

// osArm runs one of this OS's own arms, and reports whether arm names one.
func osArm(arm string) bool {
	switch arm {
	case "copyback":
		// GetSystemTime writes a whole struct through its pointer. The time itself differs run to run, so the arm
		// prints only what any present time satisfies.
		var st C.SYSTEMTIME
		C.GetSystemTime(&st)
		fmt.Println("copyback", st.wYear >= 2026, st.wMonth >= 1 && st.wMonth <= 12, st.wDay >= 1 && st.wDay <= 31)
	default:
		return false
	}

	return true
}
