// typeParamFloatConversion_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards convCallExpr's FLOAT arm for a type-parameter conversion. Go 1.26 math/big's calibrate_test.go writes
// `math.IsInf(float64(v), +1)` in `func csv[T int | float64](name string, values []T)`. The type set has a float
// member, so the integer route (ConvertToUInt64) did not apply and the conversion fell through to a C# cast,
// `(float64)v`, which is CS0030 on a type parameter. The fixture is that shape and its float32 twin, plus the
// control: an integer-only set keeps its own ConvertToUInt64 route.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertTypeParamFloatFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/tpfloat\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

// POSITIVE -- math/big's shape: a mixed int | float64 set converted to float64.
func toFloat64[T int | float64](v T) float64 { return float64(v) }

// POSITIVE -- the float32 twin.
func toFloat32[T int64 | float32](v T) float32 { return float32(v) }

// CONTROL -- an integer-only set keeps ConvertToUInt64.
func toUint64[T int | int32](v T) uint64 { return uint64(v) }

func main() {
	fmt.Println(toFloat64(2), toFloat32[int64](3), toUint64(4))
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "tpfloat", "main.cs"))
}

// TestTypeParamFloatConversionRoutesThroughGolib requires each float conversion of a float-member type parameter
// to call golib's float helper, never a C# cast, and the integer-only control to keep ConvertToUInt64.
func TestTypeParamFloatConversionRoutesThroughGolib(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertTypeParamFloatFixture(t)

	if body := funcBody(t, cs, "toFloat64<"); !strings.Contains(body, "ConvertToFloat64<T>(v)") || strings.Contains(body, "(float64)v") {
		t.Errorf("float64(v) of an int | float64 type parameter does not route through ConvertToFloat64:\n%s", body)
	}

	if body := funcBody(t, cs, "toFloat32<"); !strings.Contains(body, "ConvertToFloat32<T>(v)") || strings.Contains(body, "(float32)v") {
		t.Errorf("float32(v) of an int64 | float32 type parameter does not route through ConvertToFloat32:\n%s", body)
	}

	// CONTROL: the integer-only set keeps its route.
	if body := funcBody(t, cs, "toUint64<"); !strings.Contains(body, "ConvertToUInt64<T>(v)") {
		t.Errorf("the integer-only control lost ConvertToUInt64:\n%s", body)
	}
}
