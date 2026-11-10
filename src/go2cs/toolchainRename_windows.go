// toolchainRename_windows.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build windows

package main

import (
	"errors"
	"syscall"
)

// errorSharingViolation is Windows' ERROR_SHARING_VIOLATION, which package syscall does not name.
const errorSharingViolation syscall.Errno = 32

// transientRenameError reports a rename failure that waiting can clear: access denied, which a file held inside a
// directory produces when the directory is renamed, and a sharing violation, which a held file produces itself. Of
// the errors the go command's robustio retries (cmd/internal/robustio/robustio_windows.go, isEphemeralError), it
// leaves out ERROR_FILE_NOT_FOUND: the staging directory was just written and checked, so its absence is real.
func transientRenameError(err error) bool {
	var errno syscall.Errno

	if !errors.As(err, &errno) {
		return false
	}

	return errno == syscall.ERROR_ACCESS_DENIED || errno == errorSharingViolation
}
