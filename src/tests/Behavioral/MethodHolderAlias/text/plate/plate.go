// Package plate (text) is imported by the package of the same name under html.
package plate

type Plate struct{ Name string }

func (p Plate) Render() string { return Render(p.Name) }

func Render(name string) string { return "text:" + name }
