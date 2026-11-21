// clashlib declares, at package scope, types named like types of the namespaces package_info.cs takes `using`
// directives for: StackTrace (System.Diagnostics.StackTrace) and MethodImpl (the attribute short form of
// System.Runtime.CompilerServices.MethodImplAttribute). Its package_info.cs reaches the package's names through
// `using static clashlib_package;`, so with `using System.Diagnostics;` beside it the bare `StackTrace` of the
// interface record Describe's conversion writes would be ambiguous (CS0104). The package keeps the global::
// spelling of its holders' and forwarders' attributes and carries no System using.
package clashlib

import (
	"fmt"
	"strings"
)

// MethodImpl has a value method and a pointer method, so its holder carries a forwarder of each.
type MethodImpl struct{ n int }

func (m MethodImpl) Get() int { return m.n }

func (m *MethodImpl) Bump(by int) { m.n += by }

// StackTrace is a named slice with a value method of its own.
type StackTrace []string

func (s StackTrace) String() string { return strings.Join(s, " <- ") }

func (s *StackTrace) Push(frame string) { *s = append(*s, frame) }

// Describe converts the trace to fmt.Stringer, which records that StackTrace implements it.
func (s StackTrace) Describe() fmt.Stringer { return s }

// New returns a MethodImpl holding n.
func New(n int) MethodImpl { return MethodImpl{n: n} }
