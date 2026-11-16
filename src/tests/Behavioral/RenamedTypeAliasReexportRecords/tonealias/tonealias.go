// Package tonealias re-exports tone's types under aliases, as golang.org/x/sys/unix's `type Signal =
// syscall.Signal` does.
package tonealias

import "RenamedTypeAliasReexportRecords/tone"

type Tone = tone.Tone

type Pitch = tone.Pitch
