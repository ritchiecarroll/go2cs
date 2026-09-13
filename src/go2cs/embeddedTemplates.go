// embeddedTemplates.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the files COMPILED INTO the converter binary.
//
// Every one of these is a template or asset the converter must emit but cannot expect to find on
// disk at run time — .csproj scaffolding, the package_info.cs skeleton, the icons a generated
// project references, the publish profiles. Embedding them makes go2cs.exe a single
// self-contained executable: copy it anywhere and conversions still produce complete projects.
//
// The //go:embed directive above each variable is load-bearing, not a comment. Keep it attached.

package main

import (
	"embed"
)

//go:embed csproj-template.xml
var csprojTemplate []byte

//go:embed test-csproj-template.xml
var testCsprojTemplate []byte

//go:embed package_info-template.txt
var packageInfoTemplate []byte

//go:embed go2cs.ico
var iconFileBytes []byte

//go:embed go2cs.png
var pngFileBytes []byte

//go:embed profiles/*
var publishProfiles embed.FS
