// Package slog mirrors logrus' hooks/slog: a struct named Handler whose POINTER implements log/slog's
// Handler interface, returned by a constructor.
package slog

import (
	"context"
	"log/slog"
)

type MyHandler struct{ n int }

func NewHandler() *MyHandler { return &MyHandler{} }

func (h *MyHandler) Enabled(context.Context, slog.Level) bool { return true }

func (h *MyHandler) Handle(_ context.Context, r slog.Record) error { h.n++; return nil }

func (h *MyHandler) WithAttrs([]slog.Attr) slog.Handler { return h }

func (h *MyHandler) WithGroup(string) slog.Handler { return h }

func (h *MyHandler) Count() int { return h.n }
