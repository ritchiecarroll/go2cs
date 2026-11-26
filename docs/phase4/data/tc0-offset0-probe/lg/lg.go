package lg

import "runtime"

type Logger struct{ n int }

var Where string

func (l *Logger) record() {
	var pcs [8]uintptr
	n := runtime.Callers(1, pcs[:])
	frames := runtime.CallersFrames(pcs[:n])
	Where = ""
	for {
		f, more := frames.Next()
		Where += " " + f.Function + ":" + itoa(f.Line)
		if !more {
			break
		}
	}
}

func (l *Logger) Log(msg string, args ...any) { l.record() }

func (l *Logger) Info(msg string) { l.record() }

func Top(msg string, args ...any) { std.record() }

var std = &Logger{}

func itoa(n int) string {
	if n == 0 {
		return "0"
	}
	s := ""
	for n > 0 {
		s = string(rune('0'+n%10)) + s
		n /= 10
	}
	return s
}
