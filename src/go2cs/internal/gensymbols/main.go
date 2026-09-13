// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// gensymbols generates the two committed projections of the canonical go2cs symbol
// table (src/core/go2cs/symbols.json):
//
//   - src/go2cs/symbols.go       (Go, package main - all symbols)
//   - src/core/go2cs/Symbols.cs  (C#, class go2cs.Symbols - symbols marked csharp:true,
//     shared into golib and the go2cs-gen analyzer via the go2cs.projitems shared project)
//
// Run from src/go2cs via `go generate .` (the //go:generate directive lives in the
// generated symbols.go), or via src/check-symbol-sync.ps1 which also verifies the
// committed outputs match the table.
//
// The generator is DETERMINISTIC: output is a pure function of symbols.json (no
// timestamps; symbols emit in table order). Each output file's existing BOM and
// line-ending convention is preserved on regeneration (defaults when the file does
// not exist yet: symbols.go = no BOM + CRLF, Symbols.cs = UTF-8 BOM + CRLF), so a
// regeneration never fights the checkout's autocrlf normalization on any platform.
package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"go/format"
	"os"
	"path/filepath"
	"regexp"
	"strings"
)

type symbol struct {
	Name          string   `json:"name"`          // Constant identifier (same in Go and C#)
	Value         string   `json:"value"`         // Constant string value
	CSharp        bool     `json:"csharp"`        // Emit into Symbols.cs (C# stays minimal)
	GoEscape      bool     `json:"goEscape"`      // Emit Go value with \uXXXX escapes instead of literal glyphs
	GoExpr        string   `json:"goExpr"`        // Optional Go initializer expression (`A + B` of prior names); validated against Value
	Comment       []string `json:"comment"`       // Comment lines emitted above the const in BOTH projections
	GoComment     []string `json:"goComment"`     // Go-only comment lines (after Comment)
	CSComment     []string `json:"csComment"`     // C#-only comment lines (after Comment)
	GoTrailing    string   `json:"goTrailing"`    // Go end-of-line comment
	CSBlankBefore bool     `json:"csBlankBefore"` // Blank separator line before this entry in Symbols.cs
	Note          string   `json:"note"`          // JSON-only documentation; never emitted
}

type table struct {
	Description []string `json:"description"`
	Symbols     []symbol `json:"symbols"`
}

var identifierPattern = regexp.MustCompile(`^[A-Za-z_][A-Za-z0-9_]*$`)

var bomBytes = []byte{0xEF, 0xBB, 0xBF}

func main() {
	jsonPath := filepath.FromSlash("../core/go2cs/symbols.json")
	goOutPath := "symbols.go"
	csOutPath := filepath.FromSlash("../core/go2cs/Symbols.cs")

	if _, err := os.Stat(jsonPath); err != nil {
		fatalf("cannot find %s - gensymbols must run from src/go2cs (use `go generate .` or src/check-symbol-sync.ps1)", jsonPath)
	}

	symbolTable, err := loadTable(jsonPath)

	if err != nil {
		fatalf("%s: %v", jsonPath, err)
	}

	goSource, err := emitGo(symbolTable)

	if err != nil {
		fatalf("emitting symbols.go: %v", err)
	}

	if err := writeOutput(goOutPath, goSource, false, true); err != nil {
		fatalf("writing %s: %v", goOutPath, err)
	}

	if err := writeOutput(csOutPath, emitCSharp(symbolTable), true, true); err != nil {
		fatalf("writing %s: %v", csOutPath, err)
	}
}

func fatalf(format string, args ...any) {
	fmt.Fprintf(os.Stderr, "gensymbols: "+format+"\n", args...)
	os.Exit(1)
}

// loadTable reads and validates the symbol table: unique identifier names, non-empty
// values, and any goExpr must concatenate previously defined symbol values to exactly
// the declared value (so the expression form can never drift from the canonical value).
func loadTable(path string) (*table, error) {
	data, err := os.ReadFile(path)

	if err != nil {
		return nil, err
	}

	decoder := json.NewDecoder(bytes.NewReader(data))
	decoder.DisallowUnknownFields()

	symbolTable := &table{}

	if err := decoder.Decode(symbolTable); err != nil {
		return nil, fmt.Errorf("parsing JSON: %w", err)
	}

	if len(symbolTable.Symbols) == 0 {
		return nil, fmt.Errorf("no symbols defined")
	}

	defined := map[string]string{}

	for _, sym := range symbolTable.Symbols {
		if !identifierPattern.MatchString(sym.Name) {
			return nil, fmt.Errorf("symbol name %q is not a valid identifier", sym.Name)
		}

		if _, exists := defined[sym.Name]; exists {
			return nil, fmt.Errorf("duplicate symbol name %q", sym.Name)
		}

		if sym.Value == "" {
			return nil, fmt.Errorf("symbol %q has an empty value", sym.Name)
		}

		if sym.GoExpr != "" {
			resolved, err := resolveGoExpr(sym.GoExpr, defined)

			if err != nil {
				return nil, fmt.Errorf("symbol %q goExpr: %w", sym.Name, err)
			}

			if resolved != sym.Value {
				return nil, fmt.Errorf("symbol %q goExpr %q resolves to %q which does not match value %q", sym.Name, sym.GoExpr, resolved, sym.Value)
			}
		}

		defined[sym.Name] = sym.Value
	}

	return symbolTable, nil
}

// resolveGoExpr resolves a `A + B + ...` concatenation of previously defined symbol names.
func resolveGoExpr(expr string, defined map[string]string) (string, error) {
	var resolved strings.Builder

	for _, term := range strings.Split(expr, "+") {
		name := strings.TrimSpace(term)

		value, ok := defined[name]

		if !ok {
			return "", fmt.Errorf("references %q which is not a previously defined symbol", name)
		}

		resolved.WriteString(value)
	}

	return resolved.String(), nil
}

// emitGo renders the Go projection and normalizes it through go/format so the committed
// file is always gofmt-canonical (which also makes trailing comment alignment stable).
func emitGo(symbolTable *table) ([]byte, error) {
	var builder strings.Builder

	builder.WriteString("// symbols.go - Gbtc\n// Copyright © 2026 The go2cs Authors. All rights reserved.\n//\n// SPDX-License-Identifier: AGPL-3.0-only\n// Use of this source code is governed by the GNU Affero General Public License\n// version 3 only, which can be found in the LICENSE file.\n// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).\n\n")
	builder.WriteString("// Code generated by gensymbols from ../core/go2cs/symbols.json; DO NOT EDIT.\n")
	builder.WriteString("\n")
	builder.WriteString("// This file is one of two projections of the canonical go2cs symbol table\n")
	builder.WriteString("// (src/core/go2cs/symbols.json); the other is the C# projection\n")
	builder.WriteString("// src/core/go2cs/Symbols.cs (class go2cs.Symbols, shared into golib and the\n")
	builder.WriteString("// go2cs-gen analyzer via the go2cs.projitems shared project). To change a\n")
	builder.WriteString("// symbol, edit symbols.json and regenerate BOTH projections with\n")
	builder.WriteString("// `go generate .` from src/go2cs, or run src/check-symbol-sync.ps1 (which\n")
	builder.WriteString("// also verifies the committed outputs are in sync with the table).\n")
	builder.WriteString("\n")
	builder.WriteString("//go:generate go run ./internal/gensymbols\n")
	builder.WriteString("\n")
	builder.WriteString("package main\n")
	builder.WriteString("\n")

	for i, sym := range symbolTable.Symbols {
		commentLines := make([]string, 0, len(sym.Comment)+len(sym.GoComment))
		commentLines = append(commentLines, sym.Comment...)
		commentLines = append(commentLines, sym.GoComment...)

		if len(commentLines) > 0 && i > 0 {
			builder.WriteString("\n")
		}

		for _, line := range commentLines {
			builder.WriteString("// " + line + "\n")
		}

		initializer := sym.GoExpr

		if initializer == "" {
			initializer = goQuote(sym.Value, sym.GoEscape)
		}

		builder.WriteString("const " + sym.Name + " = " + initializer)

		if sym.GoTrailing != "" {
			builder.WriteString(" // " + sym.GoTrailing)
		}

		builder.WriteString("\n")
	}

	return format.Source([]byte(builder.String()))
}

// emitCSharp renders the C# projection (class go2cs.Symbols). The license header and
// generated-file banner are static template text - no timestamps, so regeneration is
// byte-stable. Glyph values are emitted as literal UTF-8 characters (the file carries a
// UTF-8 BOM), matching the established Symbols.cs convention. netstandard2.0-compatible:
// `public const string` members only.
func emitCSharp(symbolTable *table) []byte {
	lines := []string{
		"// Symbols.cs - Gbtc",
		"// Copyright © 2026 The go2cs Authors. All rights reserved.",
		"//",
		"// Use of this source code is governed by an MIT-style license",
		"// that can be found in the LICENSE file.",
		"",
		"// THIS FILE IS AUTO-GENERATED by gensymbols from the canonical go2cs symbol table",
		"// src/core/go2cs/symbols.json (its Go projection is src/go2cs/symbols.go). DO NOT EDIT",
		"// this file directly - edit symbols.json and regenerate both projections with",
		"// `go generate .` from src/go2cs, or run src/check-symbol-sync.ps1.",
		"",
		"namespace go2cs;",
		"",
		"public static class Symbols",
		"{",
	}

	first := true

	for _, sym := range symbolTable.Symbols {
		if !sym.CSharp {
			continue
		}

		if sym.CSBlankBefore && !first {
			lines = append(lines, "")
		}

		commentLines := make([]string, 0, len(sym.Comment)+len(sym.CSComment))
		commentLines = append(commentLines, sym.Comment...)
		commentLines = append(commentLines, sym.CSComment...)

		for _, line := range commentLines {
			lines = append(lines, "    // "+line)
		}

		lines = append(lines, "    public const string "+sym.Name+" = "+csQuote(sym.Value)+";")
		first = false
	}

	lines = append(lines, "}", "")

	return []byte(strings.Join(lines, "\n"))
}

// goQuote renders a Go interpreted string literal. With escapeNonASCII, every non-ASCII
// rune is emitted as \uXXXX (or \UXXXXXXXX beyond the BMP) so the value spelling in
// symbols.go is glyph-mangling-proof; otherwise non-ASCII runes are written literally.
func goQuote(value string, escapeNonASCII bool) string {
	var builder strings.Builder

	builder.WriteByte('"')

	for _, r := range value {
		switch {
		case r == '"':
			builder.WriteString(`\"`)
		case r == '\\':
			builder.WriteString(`\\`)
		case r < 0x20 || r == 0x7F:
			builder.WriteString(fmt.Sprintf(`\u%04X`, r))
		case r > 0x7F && escapeNonASCII:
			if r > 0xFFFF {
				builder.WriteString(fmt.Sprintf(`\U%08X`, r))
			} else {
				builder.WriteString(fmt.Sprintf(`\u%04X`, r))
			}
		default:
			builder.WriteRune(r)
		}
	}

	builder.WriteByte('"')

	return builder.String()
}

// csQuote renders a C# string literal. Non-ASCII runes are written literally (the
// established Symbols.cs convention - the file carries a UTF-8 BOM); quotes, backslashes
// and control characters are escaped.
func csQuote(value string) string {
	var builder strings.Builder

	builder.WriteByte('"')

	for _, r := range value {
		switch {
		case r == '"':
			builder.WriteString(`\"`)
		case r == '\\':
			builder.WriteString(`\\`)
		case r < 0x20 || r == 0x7F:
			builder.WriteString(fmt.Sprintf(`\u%04X`, r))
		default:
			builder.WriteRune(r)
		}
	}

	builder.WriteByte('"')

	return builder.String()
}

// writeOutput writes content (which uses \n newlines) to path, preserving the existing
// file's BOM and line-ending convention (or the given defaults when the file does not
// exist). No-ops byte-identical content so repeated runs never touch timestamps.
func writeOutput(path string, content []byte, defaultBOM, defaultCRLF bool) error {
	useBOM, useCRLF := defaultBOM, defaultCRLF

	existing, err := os.ReadFile(path)
	exists := err == nil

	if exists {
		useBOM = bytes.HasPrefix(existing, bomBytes)
		useCRLF = bytes.Contains(existing, []byte("\r\n"))
	}

	if bytes.Contains(content, []byte("\r")) {
		return fmt.Errorf("internal error: emitted content already contains CR bytes")
	}

	output := content

	if useCRLF {
		output = bytes.ReplaceAll(output, []byte("\n"), []byte("\r\n"))
	}

	if useBOM {
		output = append(append([]byte{}, bomBytes...), output...)
	}

	if exists && bytes.Equal(existing, output) {
		fmt.Printf("gensymbols: %s up to date\n", path)
		return nil
	}

	if err := os.WriteFile(path, output, 0o644); err != nil {
		return err
	}

	fmt.Printf("gensymbols: %s written\n", path)

	return nil
}
