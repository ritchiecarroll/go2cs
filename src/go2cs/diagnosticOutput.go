// diagnosticOutput.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns how the converter TALKS — to the person running it, and to the C# compiler.
//
// Two audiences, two mechanisms. showMessage/showWarning report progress and problems on
// stdout/stderr, in free and Visitor-scoped forms (the latter names the Go file being converted,
// so a warning raised deep in expression conversion is still traceable). addRequiredUsing and
// addMethodPackageNamespaceUsing instead speak to the compiler, recording the `using` directives
// the emitted file needs so a name written into a body actually resolves.
//
// getPrintedNode renders an AST node back to Go source, which is what makes a diagnostic quotable
// ("cannot convert `x[i:j:k]`") instead of abstract.

package main

import (
	"fmt"
	"go/ast"
	"go/printer"
	"go/types"
	"os"
	"strings"
)

func (v *Visitor) addRequiredUsing(usingName string) {
	v.requiredUsings.Add(usingName)
}

// addMethodPackageNamespaceUsing ensures the file-local `using <namespace>;` for a cross-package
// method's defining package is emitted, so its C# extension method (`Method(this ж<T>, …)`) is in
// scope at the call site even when the file does not explicitly import that package. Mirrors the
// namespace-using derivation in visitImportSpec's unaliased-import branch; a no-op for the current
// package and for root-namespace (`go`) packages, whose extensions are already visible.
func (v *Visitor) addMethodPackageNamespaceUsing(pkg *types.Package) {
	if pkg == nil || pkg == v.pkg {
		return
	}

	importPath := rootQualifyIfAmbiguous(convertImportPathToNamespace(pkg.Path(), PackageSuffix))

	lastDot := strings.LastIndex(importPath, ".")

	if lastDot == -1 {
		return
	}

	namespace := importPath[:lastDot]

	if len(namespace) > 0 && packageNamespace != fmt.Sprintf("%s.%s", RootNamespace, namespace) {
		v.addRequiredUsing(namespace)
	}
}

func (v *Visitor) getPrintedNode(node ast.Node) string {
	if node == nil {
		return ""
	}

	result := &strings.Builder{}
	printer.Fprint(result, v.fset, node)
	return result.String()
}

// showMessage reports informational progress on stdout, where it stays out of the way of a caller
// that is piping warnings elsewhere.
func showMessage(format string, a ...interface{}) {
	message := fmt.Sprintf(format, a...)
	os.Stdout.WriteString(fmt.Sprintf("INFO: %s\n", message))
}

// showWarning reports a conversion problem on stderr — something the converter worked around or
// could not express, which a reader of the emitted C# needs to know about.
func showWarning(format string, a ...interface{}) {
	message := fmt.Sprintf(format, a...)
	os.Stderr.WriteString(fmt.Sprintf("WARNING: %s\n", message))
}

// showWarning is the Visitor-scoped form: it appends the Go file currently being converted, so a
// warning raised deep in expression conversion still says which source it came from.
func (v *Visitor) showWarning(format string, a ...interface{}) {
	message := fmt.Sprintf(format, a...)
	showWarning("%s in \"%s\"", message, getShortFileName(v.file))
}
