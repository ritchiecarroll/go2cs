// Package plate (html) imports the text package of the same name, so in this file `plate` is the import.
package plate

import "MethodHolderAlias/text/plate"

type Plate struct{ inner plate.Plate }

// Render reads this package's function Render and the imported package's function of the same name.
func (p Plate) Render() string { return Render(p.inner.Name) + "+" + plate.Render(p.inner.Name) }

func Render(name string) string { return "html:" + name }

func New(name string) Plate { return Plate{inner: plate.Plate{Name: name}} }
