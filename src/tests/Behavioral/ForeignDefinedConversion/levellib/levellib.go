// Package levellib declares the foreign base: a named integer, as logrus declares Level.
package levellib

// Level is a named uint32, like logrus.Level.
type Level uint32

// Double returns twice the level.
func (l Level) Double() uint32 { return uint32(l) * 2 }
