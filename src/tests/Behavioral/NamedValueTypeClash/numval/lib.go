// Package numval declares a named NUMERIC type called Value, and a second named type over it, so the
// conversions between them go through the generated implicit-conversion records.
package numval

type Value int

func (v Value) Double() Value { return v * 2 }

func (v *Value) Inc() { *v++ }

type Other Value

func Conv(o Other) Value { return Value(o) }

func Back(v Value) Other { return Other(v) }
