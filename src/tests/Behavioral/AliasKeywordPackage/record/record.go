// Package record is named by a contextual C# word that is a legal alias name but begins a record declaration at member
// start, so a return type spelled record.Entry fails unless the alias is @record (csReservedTypeNames).
package record

// Entry is a defined type named in flags' method signature.
type Entry int
