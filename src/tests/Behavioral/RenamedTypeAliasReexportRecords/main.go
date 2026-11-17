// Guards every spelling of a type that is collision-renamed in a package this one reaches only through an
// ALIAS (sqlite-cs W11). libc passes `unix.Signal(signum)` to an os.Signal; unix.Signal is `= syscall.Signal`,
// and syscall's type is emitted as ΔSignal. libc imports unix, not syscall, so syscall's rename records were
// never read, and the record named the type by its Go name:
// `GoImplement<syscall_package.Signal, os_package.ΔSignal>` (CS0426).
//
// Here main imports tonealias, never tone. Each statement below reaches a different writer of the type's
// name: the value and pointer implementer records and their adapters, an alias of the alias, the box of a
// local whose address is taken, and the field-access record. RenamedTypeAliasReexport guards the spellings
// the type takes in declarations and expressions.
package main

import (
	"fmt"

	"RenamedTypeAliasReexportRecords/tonealias"
)

type toner interface {
	Tone() int
	String() string
}

type raiser interface {
	Raise(by int)
	Label() string
}

// localTone is an alias of an alias: its target is the renamed type two packages away.
type localTone = tonealias.Tone

func describe(t toner) string { return fmt.Sprintf("%s -> %d", t.String(), t.Tone()) }

func main() {
	// A value implementer.
	var t toner = tonealias.Tone(3)
	fmt.Println(describe(t))
	fmt.Println(describe(tonealias.Tone(4)))
	fmt.Println(t)

	// The alias of the alias.
	var lt localTone = 5
	fmt.Println(describe(lt))

	// A pointer implementer, and a write through the interface.
	p := lt.Pitch()
	var r raiser = &p
	r.Raise(2)
	fmt.Println(r.Label())

	// The address of a field of the renamed struct.
	hz := &p.Hz
	*hz += 3
	fmt.Println(p.Hz, r.Label())
}
