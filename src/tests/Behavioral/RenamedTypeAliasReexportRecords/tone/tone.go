// Package tone declares types that share a name with a method of the package, as syscall.Signal has
// Signal(): go2cs renames each type (ΔTone, ΔPitch), since a C# member cannot share its type's name.
package tone

import "fmt"

type Tone int

func (t Tone) Tone() int { return int(t) * 10 }

func (t Tone) String() string { return fmt.Sprintf("tone %d", int(t)) }

// Pitch is renamed because Tone has a method named Pitch.
type Pitch struct {
	Hz int
}

func (t Tone) Pitch() Pitch { return Pitch{Hz: 400 + int(t)*10} }

func (p *Pitch) Raise(by int) { p.Hz += by }

func (p *Pitch) Label() string { return fmt.Sprintf("pitch %d Hz", p.Hz) }
