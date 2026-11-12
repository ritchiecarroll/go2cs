// goFrameOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strconv"
	"strings"
)

// A Go function that defers or recovers is emitted with its body INLINE inside try/catch/finally,
// beside a GoFrame local that holds this call's defer list:
//
//	internal static void main() { GoFrame ᒐ = default; try {
//	    fmt.Println(openFileˢ);
//	    deferǃ(fmt.Println, closeFileˢ, ref ᒐ);
//	} catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }
//
// The frame opens on the signature line and closes on the last line, and the body sits at Go's
// indentation, one Go line to one C# line: a Go reader sees the function they wrote, with the frame
// at its two edges. The layout is whitespace only; the statements, and so the IL, are the same as a
// frame laid out over its own lines (docs/phase4/SCOUT-defer-emission-parity.md, option 4).
//
// It replaces the `func<T>((defer, recover) => …)` execution context, which modelled a catch, a
// finally and a defer list as an OBJECT owning the body. Owning the body forced the body to be a
// delegate; a delegate forced a display class for everything the body touched; and a display class
// forced the GoFunc<TRef1…TRef16> ladder for everything a delegate cannot capture. None of it was
// ever needed — try/catch/finally are STATEMENTS, and recover() reads a static thread-local rather
// than a handle on the frame — so only the defer LIST is genuinely per-call, and a ref struct holds
// it for free.
//
// Two consequences beyond the allocation, both of which the emitted text shows directly: the body
// captures NOTHING (a whole class of divergence where the lambda closed over variables the Go
// original never closed over cannot arise), and the deferred calls sit in the function that wrote
// them rather than one lambda level in.
//
// Full design, including the staged migration this predicate walks: docs/Phase4/DESIGN-closure-emission.md §4.

// goFrameName is the emitted GoFrame local; the rest of the frame vocabulary composes from it, so
// the whole shape moves with the one symbol.
//
// A nested defer scope — a function literal that defers inside a function that also does — declares
// its OWN frame under the SAME name, which is legal: a C# lambda or local function may shadow an
// enclosing method local (verified empirically; the pre-C#-8 CS0136 rule no longer fires here), and
// the inner declaration precedes every inner use. Keeping one name is deliberate rather than
// incidental — every frame in every emitted function reads identically.
func (v *Visitor) goFrameName() string {
	return GoFrameVar
}

// goFrameExceptionName is the emitted catch clause's exception variable.
func (v *Visitor) goFrameExceptionName() string {
	return GoFrameVar + "e"
}

// goFrameExitLabel is the label a named-result function's early `return` jumps to (§4.4): the
// results are declared before the try and returned after the finally, so an exit from inside the
// try has to leave through a goto, which runs the finally exactly as a return would.
//
// This is the ONE name in the frame vocabulary that must be depth-numbered. LABELS do not shadow the
// way locals do: a label inside a lambda that repeats one from the enclosing method is CS0158 ("the
// label shadows another label by the same name in a contained scope"), which locals were checked
// against and are exempt from. So the nesting depth is appended, and only here.
func (v *Visitor) goFrameExitLabel() string {
	if v.openGoFrames < 2 {
		return GoFrameVar + "done"
	}

	return GoFrameVar + "done" + strconv.Itoa(v.openGoFrames-1)
}

// goFrameEligible reports whether this function's defer/recover scope is emitted as a GoFrame
// rather than as the `func((defer, recover) => …)` execution context. It is the migration's dial:
// each stage of DESIGN-closure-emission.md §4.8 widens it by removing one clause, and every clause
// removed is a lowering rule that has been written and gated. hasDefer/hasRecover/
// namedReturnDeferMode must already be set for THIS function.
func (v *Visitor) goFrameEligible(funcDecl *ast.FuncDecl, signature *types.Signature) bool {
	if funcDecl == nil || funcDecl.Body == nil || signature == nil {
		return false
	}

	if !(v.hasDefer || v.hasRecover) {
		return false
	}

	return true
}

// litGoFrameEligible is goFrameEligible for a function LITERAL. A literal's frame lives in the
// lambda (or local function) the literal emits as, which is an ordinary scope for a ref-struct
// LOCAL - only CAPTURING one is forbidden, and an inline body captures nothing. Nested frames are
// declared under the SAME name (a C# lambda or local function may shadow an enclosing method local);
// only the named-result exit LABEL is depth-numbered, because labels do not shadow (CS0158).
func (v *Visitor) litGoFrameEligible(litSig *types.Signature, hasDefer, hasRecover bool, funcLit *ast.FuncLit) bool {
	if litSig == nil || funcLit == nil || funcLit.Body == nil {
		return false
	}

	return hasDefer || hasRecover
}

// joinGoFrameLines puts a run of newline-led declaration lines (the named results, the reached
// flags) onto the frame's opening line, each after one space. A line that carries a `//` comment
// cannot be followed on its line, so it and every line after it keep their own lines: correct C#
// either way, and no declaration the frame emits today carries one.
func (v *Visitor) joinGoFrameLines(lines string) string {
	if lines == "" {
		return lines
	}

	parts := strings.Split(lines, v.newline)
	result := strings.Builder{}

	for i, part := range parts {
		trimmed := strings.TrimSpace(part)

		if trimmed == "" {
			continue
		}

		if strings.Contains(trimmed, "//") {
			result.WriteString(v.newline)
			result.WriteString(strings.Join(parts[i:], v.newline))
			break
		}

		result.WriteString(" ")
		result.WriteString(trimmed)
	}

	return result.String()
}

// goFrameHead renders the text that replaces the execution-context marker — everything between the
// signature's closing paren and the body block, which the body then opens with ` {`. It is one
// line: ` { <named results> GoFrame ᒐ = default; <reached flags> try`.
// namedResultDecls is the named-result declaration block (§4.4), each line already newline-led, or
// empty for the unnamed-result form: those results are declared BEFORE the try because deferred
// code mutates them and the exit after the finally reads them back.
func (v *Visitor) goFrameHead(indentLevel int, namedResultDecls string) string {
	// Any defer→finally lowered site's reached-flag is declared alongside the frame, before the
	// try: the finally reads it, and a local declared inside a try is not in scope there.
	return fmt.Sprintf(" {%s GoFrame %s = default;%s try",
		v.joinGoFrameLines(namedResultDecls),
		v.goFrameName(),
		v.joinGoFrameLines(v.goFrameLoweredFlagDecls(indentLevel+1)))
}

// goFrameTail renders the catch and the finally of a frame-form function body, on the line of the
// try block's closing brace: ` catch (…) { … } finally { … }`. The caller appends the named-result
// exit, if any, and the method's own closing brace to the same line.
// resultsZeroValue is the `return` the catch arm ends with for a value-returning function whose
// results are UNNAMED: a recovered panic returns Go's zero results, and an UNrecovered one never
// reaches the return because Run() re-throws from the finally. It is empty for a void function and
// for the named-result form, whose exit runs through the label instead. endPos, when valid, marks the
// finally line with that Go position: the closing brace of a body that can fall off its end (see
// deferEpilogueEndPos), the line Go reports for a deferred call's caller on that exit.
func (v *Visitor) goFrameTail(indentLevel int, catchReturn string, endPos token.Pos) string {
	if catchReturn != "" {
		catchReturn = " " + catchReturn
	}

	// The frame-local tail: the filter ADOPTS the panic into the frame (GoFrame.Is) and the catch
	// body PARKS it (GoFrame.Capture), in that order. Parking in the filter would let a finally
	// between the throw and this frame overwrite the parked panic (GoFrame.Is).
	//
	// Lowered defers run BEFORE Run(): Run() re-throws an unrecovered panic, which would leave
	// anything after it in the finally unexecuted. They are already in Go's LIFO order.
	return fmt.Sprintf(" catch (Exception %s) when (%s.Is(%s)) { %s.Capture();%s } finally { %s%s%s.Run(); }",
		v.goFrameExceptionName(), v.goFrameName(), v.goFrameExceptionName(), v.goFrameName(), catchReturn,
		v.positionSentinelText(endPos), v.goFrameLoweredFinallyCalls(), v.goFrameName())
}
