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
	"regexp"
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

// Canonical diagnostics: the converter's failures on its INPUT -- a package that did not load cleanly,
// a source it refuses -- are also printed on stderr in MSBuild's canonical form,
//
//	file(line,col): error GO2CSnnnn: text
//
// so CI logs and editor problem matchers pick them up. Each site keeps the human-readable summary line
// it printed before: the behavioral runner and check-no-regression key on that wording.
//
// GO2CS1xxx is a package that did not load cleanly, coded by how go/packages classifies the error;
// GO2CS2xxx is an input the converter refuses.
const (
	diagnosticLoadError  = "GO2CS1000"
	diagnosticListError  = "GO2CS1001"
	diagnosticParseError = "GO2CS1002"
	diagnosticTypeError  = "GO2CS1003"
	diagnosticCgoRefused = "GO2CS2001"
)

// diagnosticPositionPattern splits "file:line[:col]". The file part is matched lazily up to the LAST
// line/column pair the anchor allows, so a Windows drive letter ("C:\src\te.go:3:16") stays in it.
var diagnosticPositionPattern = regexp.MustCompile(`^(.+?):(\d+)(?::(\d+))?$`)

var diagnosticLineBreaks = regexp.MustCompile(`\s*[\r\n]+\s*`)

// canonicalDiagnostic formats one error. position is go/packages' or token.Position's spelling,
// "file:line:col", "file:line" or "file"; empty or "-" names no file, and the origin is then the tool.
// A multi-line text is joined onto one line, since a matcher reads one line per diagnostic.
func canonicalDiagnostic(position, code, text string) string {
	origin := "go2cs"

	if position != "" && position != "-" {
		origin = position

		if match := diagnosticPositionPattern.FindStringSubmatch(position); match != nil {
			origin = match[1] + "(" + match[2]

			if match[3] != "" {
				origin += "," + match[3]
			}

			origin += ")"
		}
	}

	return fmt.Sprintf("%s: error %s: %s", origin, code, diagnosticLineBreaks.ReplaceAllString(strings.TrimSpace(text), " "))
}

// reportDiagnostic prints one canonical diagnostic on stderr, without the log package's timestamp,
// which would put text before the origin a matcher anchors on.
func reportDiagnostic(position, code, text string) {
	os.Stderr.WriteString(canonicalDiagnostic(position, code, text) + "\n")
}

// loadErrorCode is the GO2CS1xxx code for a go/packages error kind.
func loadErrorCode(kind packages.ErrorKind) string {
	switch kind {
	case packages.ListError:
		return diagnosticListError
	case packages.ParseError:
		return diagnosticParseError
	case packages.TypeError:
		return diagnosticTypeError
	default:
		return diagnosticLoadError
	}
}
