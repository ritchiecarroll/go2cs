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

// 量 is unexported in Go (its first letter has no case), so a dot import does not bring it in; C# reads it as
// public.
var 量 = 1000

// 尺 is unexported the same way; C# reads the type as public too.
type 尺 struct{ n int }
