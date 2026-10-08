// bridgeTypeAccess_test.go - Gbtc
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

	"github.com/ritchiecarroll/hashset"
)

// TestBridgeTypeAccessIsAnchoredOrRefusedByName holds section 11 rows 1 and 2 for the -tests white-box bridge unit
// (docs/PLAN-marker-comment-parity.md): the attribute-only partials the bridge's types record land in the metadata file
// whose first class is the bridge, and a line recorded for any other class is refused by name, never written where it
// would declare an empty phantom type while the type it describes loses the attribute.
func TestBridgeTypeAccessIsAnchoredOrRefusedByName(t *testing.T) {
	saved := packageBridgeTypeAccess
	defer func() { packageBridgeTypeAccess = saved }()

	packageBridgeTypeAccess = map[string]hashset.HashSet[string]{
		"json_internal_test_package": hashset.NewHashSet([]string{`[GoLocalName("embed1")] partial struct TestUnmarshal_embed1 {}`, `[GoValueClone("h")] partial struct wrapper {}`}),
	}

	lines, err := bridgeTypeAccessFor("json_internal_test_package")

	if err != nil || len(lines) != 2 {
		t.Fatalf("the bridge's own lines: got %v, %v; want both and no error", lines.Keys(), err)
	}

	packageBridgeTypeAccess["json_test_package"] = hashset.NewHashSet([]string{`[GoLocalName("x")] partial struct TestX_x {}`})

	if _, err := bridgeTypeAccessFor("json_internal_test_package"); err == nil || !strings.Contains(err.Error(), "json_test_package") || !strings.Contains(err.Error(), "TestX_x") {
		t.Errorf("a line recorded for another class must be refused naming the class and the type, got %v", err)
	}
}
