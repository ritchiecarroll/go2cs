// oneline.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package oneline

import "context"

// timeoutError reports a timeout.
type timeoutError struct{ err string }

func (e *timeoutError) Error() string     { return e.err }
func (e *timeoutError) Timeout() bool     { return true }
func (e *timeoutError) Is(err error) bool { return err == context.DeadlineExceeded }
func (*timeoutError) marker()             {}

// wrap returns a timeout error.
func wrap(s string) error {
	return &timeoutError{s}
}
