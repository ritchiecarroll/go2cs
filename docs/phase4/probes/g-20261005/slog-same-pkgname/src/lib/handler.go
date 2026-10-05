// Package slog mirrors logrus' hooks/slog: a struct named Handler whose POINTER implements log/slog's
// Handler interface, returned by a constructor.
package slog

import (
	"context"
	"log/slog"
)

type Handler struct{ n int }

func NewHandler() *Handler { return &Handler{} }

func (h *Handler) Enabled(context.Context, slog.Level) bool { return true }

func (h *Handler) Handle(_ context.Context, r slog.Record) error { h.n++; return nil }

func (h *Handler) WithAttrs([]slog.Attr) slog.Handler { return h }

func (h *Handler) WithGroup(string) slog.Handler { return h }

func (h *Handler) Count() int { return h.n }

type Writer struct{ n int }

func NewWriter() *Writer { return &Writer{} }

func (w *Writer) Write(p []byte) (int, error) { w.n += len(p); return len(p), nil }
