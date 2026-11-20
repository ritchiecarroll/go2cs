// Package geom has a child package, so the name `geom` is also a namespace where geom's files are read.
package geom

import "MethodHolderAlias/geom/part"

const Unit = 3

type Shape struct{ W, H part.Side }

// Area reads the package function Area, which shares its name.
func (s Shape) Area() int { return Area(s.W.Len, s.H.Len) }

// Unit reads the package constant Unit.
func (s *Shape) Unit() int { return Unit + s.W.Double() }

func Area(w, h int) int { return w * h }
