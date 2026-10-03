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

	"golang.org/x/tools/go/packages"
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
	if namespace, ok := v.methodPackageNamespace(pkg); ok {
		v.addRequiredUsing(namespace)
	}
}

// methodPackageNamespace is the `using <namespace>;` addMethodPackageNamespaceUsing emits for a
// method's defining package, or false when it emits none. Split out so collectMethodNamespaceUsings
// can know, before a file's body is visited, every namespace the body will import this way.
func (v *Visitor) methodPackageNamespace(pkg *types.Package) (string, bool) {
	if pkg == nil || pkg == v.pkg {
		return "", false
	}

	importPath := rootQualifyIfAmbiguous(convertImportPathToNamespace(pkg.Path(), PackageSuffix))

	lastDot := strings.LastIndex(importPath, ".")

	if lastDot == -1 {
		return "", false
	}

	namespace := importPath[:lastDot]

	if len(namespace) > 0 && packageNamespace != fmt.Sprintf("%s.%s", RootNamespace, namespace) {
		return namespace, true
	}

	return "", false
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

// Canonical diagnostic codes: GO2CS1xxx is a package that did not load cleanly, as go/packages
// classifies the error; GO2CS2xxx is an input the converter refuses.
const (
	diagnosticLoadError  = "GO2CS1000"
	diagnosticListError  = "GO2CS1001"
	diagnosticParseError = "GO2CS1002"
	diagnosticTypeError  = "GO2CS1003"
	diagnosticCgoRefused = "GO2CS2001"
)

func canonicalDiagnostic(position, code, text string) string {
	return ""
}

func loadErrorCode(kind packages.ErrorKind) string {
	return ""
}
