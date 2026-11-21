// stackAppendProbeCacheOwner_notunix.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build !unix

package main

import "os"

// probeCacheOwnerIs cannot read a file's owner here, so no shared directory is ever trusted: os.Getuid is -1 on
// Windows and probeCacheTempFallback refuses before it would ask.
func probeCacheOwnerIs(os.FileInfo, int) bool {
	return false
}
