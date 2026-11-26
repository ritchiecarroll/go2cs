// Package opts imports only flags and names flags.Flag (an alias of internal.Flag) in a method signature: its forwarder
// must resolve the type without opts importing internal.
package opts

import "AliasKeywordPackage/flags"

// Mode selects one flag bit.
type Mode uint8

// Pick reports whether b has bit f set, spelled through flags' alias of internal.Flag.
func (m Mode) Pick(b flags.Bools, f flags.Flag) bool { return m != 0 && b.Has(f) }
