// manualTypeReceiverRefusal.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/token"

	"golang.org/x/tools/go/packages"
)

// manualTypeReceiverRefusal is the error refuseManualTypeReceiverOperands returns: one canonical
// diagnostic per conversion it refuses.
type manualTypeReceiverRefusal struct {
	pkgPath string
	sites   []manualTypeReceiverRefusalSite
}

type manualTypeReceiverRefusalSite struct {
	position token.Position
	text     string
}

// refuseManualTypeReceiverOperands is not yet implemented.
func refuseManualTypeReceiverOperands(pkg *packages.Package, goos string) *manualTypeReceiverRefusal {
	return nil
}
