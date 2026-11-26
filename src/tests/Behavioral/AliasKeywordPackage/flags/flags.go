// Package flags declares methods on a defined non-struct type whose signatures name types of the two keyword-named
// packages, as go-json-experiment's jsonflags does with internal.NotForPublicUse: each method's forwarder in
// package_info.cs needs both packages' aliases there.
package flags

import (
	"AliasKeywordPackage/checked"
	"AliasKeywordPackage/internal"
	"AliasKeywordPackage/partial"
	"AliasKeywordPackage/record"
	"AliasKeywordPackage/union"

	lock "AliasKeywordPackage/tokens"
)

// Bools is a set of option bits, a defined integer type as jsonflags.Bools is.
type Bools uint64

// Flag aliases a type of the internal package, so its alias lands in the imported type aliases too.
type Flag = internal.Flag

// Options reports the bits, taking the internal marker only to name its type.
func (b Bools) Options(_ internal.NotForPublicUse) uint64 { return uint64(b) }

// Check combines the bits with a checked token.
func (b Bools) Check(t checked.Token) checked.Token { return t.Twice() + checked.Token(b) }

// Has reports whether bit f is set.
func (b Bools) Has(f Flag) bool { return b&(1<<f) != 0 }

// Lock names a plainly named package through the explicit keyword alias lock.
func (b Bools) Lock(k lock.Kind) lock.Kind { return k + lock.Kind(b&1) }

// Union names a type of the package named union.
func (b Bools) Union(s union.Set) union.Set { return s | union.Set(b) }

// Record names a type of the package named record, as a return type too (its alias is @record).
func (b Bools) Record(e record.Entry) record.Entry { return e * record.Entry(b) }

// Partial names a type of the package named partial, as a return type too (its alias stays bare).
func (b Bools) Partial(p partial.Part) partial.Part { return p - partial.Part(b) }
