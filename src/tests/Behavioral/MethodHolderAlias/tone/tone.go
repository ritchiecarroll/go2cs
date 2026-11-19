// Package tone declares a type named like the package.
package tone

type tone int

// Pitch reads the package function Pitch from a method of the type that carries the package's name.
func (t tone) Pitch() int { return Pitch(int(t)) + Base }

// Base reads the package constant Base.
func (t tone) Base() int { return Base * 2 }

const Base = 100

func Pitch(n int) int { return n * 11 }

func Play(n int) (int, int) { return tone(n).Pitch(), tone(n).Base() }
