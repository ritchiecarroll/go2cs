// processGroupWindows.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build windows

package main

import (
	"os/exec"
	"syscall"
	"unsafe"
)

// processGroup — Windows flavour. Windows has no analogue of a killable process GROUP: its
// CREATE_NEW_PROCESS_GROUP flag only redirects console CTRL+C/CTRL+BREAK delivery and cannot force
// a kill, so it is NOT the counterpart of the unix Setpgid. The counterpart is a JOB OBJECT, which
// a process's children join automatically and which can be terminated as a unit.
//
// Two properties are asked of the job, and they cover different failures:
//
//   - TerminateJobObject at the deadline — the direct answer to the orphan the pipeline actually
//     measured: the converted host re-execs itself for a subprocess-style row, the package deadline
//     kills the host from outside while a test goroutine is blocked before its deferred
//     cmd.Process.Kill(), and the re-exec'd child survives with nobody left to signal it.
//   - JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE — the answer to the converter itself dying. The handle is
//     closed when this helper returns (and by the OS if the converter is killed), and everything
//     still in the job goes with it. The unix half has no equivalent; that asymmetry is stated
//     rather than papered over.
//
// THREE RESIDUALS, all real and all stated at the site rather than assumed away:
//
//  1. The job is created before Start and the process assigned immediately AFTER it, because
//     os/exec offers no hook between CreateProcess and the first instruction of the child (the
//     race-free form needs either CREATE_SUSPENDED plus the child's thread handle, which os/exec
//     does not expose, or PROC_THREAD_ATTRIBUTE_JOB_LIST in a STARTUPINFOEX, which it does not
//     support). A descendant spawned in the window between CreateProcess returning and the assign
//     would escape the job. The window is microseconds; the children this exists for are a CLR
//     host and the Go toolchain, neither of which forks before it has started up.
//  2. Job creation or assignment can fail outright — most plausibly ERROR_ACCESS_DENIED when the
//     converter is already inside a job that forbids nesting. The helper then DEGRADES to the
//     single-process kill that was the behaviour before this existed, rather than failing the run:
//     an orphan is a hygiene defect, and refusing to run the pipeline over it would be worse than
//     the defect. The caller reports the degrade once so it is visible and not silent.
//  3. KILL_ON_JOB_CLOSE has a COST on the normal path, not only on the abort path: when the handle
//     closes at the end of an ordinary child, anything still inside the job dies with it — which
//     for the `dotnet publish` child means a BUILD SERVER that publish itself started (VBCSCompiler,
//     an MSBuild worker node) is terminated instead of being left to be reused, so the next publish
//     in the same sweep pays a fresh server start. Bounded, and deliberately accepted: the same
//     property is what stops an interrupted pipeline from leaving a host holding a lock on
//     runtime.dll, a failure this repository has already paid for once. It cannot reap a server the
//     pipeline did not START — job membership is inherited at creation and an existing server is
//     never assigned — so the machine-global `dotnet build-server shutdown` hazard is NOT
//     reintroduced; what a sibling lane can lose is a server WE started and it later connected to,
//     where the Roslyn client falls back to compiling in-process. The wall-clock size of this is
//     UNMEASURED and named here so a sweep whose wall moves has somewhere to look first.
type processGroup struct {
	job syscall.Handle
}

const (
	// JobObjectExtendedLimitInformation, the info class SetInformationJobObject takes for the
	// limit block below.
	jobObjectExtendedLimitInformationClass = 9

	// JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE.
	jobObjectLimitKillOnJobClose = 0x00002000

	// The access the assign needs: SET_QUOTA is what AssignProcessToJobObject requires, TERMINATE
	// is what the job's own kill needs against the member.
	processTerminateAccess = 0x0001
	processSetQuotaAccess  = 0x0100
)

var (
	kernel32DLL                  = syscall.NewLazyDLL("kernel32.dll")
	procCreateJobObjectW         = kernel32DLL.NewProc("CreateJobObjectW")
	procSetInformationJobObject  = kernel32DLL.NewProc("SetInformationJobObject")
	procAssignProcessToJobObject = kernel32DLL.NewProc("AssignProcessToJobObject")
	procTerminateJobObject       = kernel32DLL.NewProc("TerminateJobObject")
)

// IO_COUNTERS.
type jobIOCounters struct {
	ReadOperationCount  uint64
	WriteOperationCount uint64
	OtherOperationCount uint64
	ReadTransferCount   uint64
	WriteTransferCount  uint64
	OtherTransferCount  uint64
}

// JOBOBJECT_BASIC_LIMIT_INFORMATION.
type jobObjectBasicLimitInformation struct {
	PerProcessUserTimeLimit int64
	PerJobUserTimeLimit     int64
	LimitFlags              uint32
	MinimumWorkingSetSize   uintptr
	MaximumWorkingSetSize   uintptr
	ActiveProcessLimit      uint32
	Affinity                uintptr
	PriorityClass           uint32
	SchedulingClass         uint32
}

// JOBOBJECT_EXTENDED_LIMIT_INFORMATION. The length handed to SetInformationJobObject below is
// unsafe.Sizeof of THIS declaration rather than a literal, so a layout that does not match the
// platform's is refused by the API with ERROR_BAD_LENGTH and takes the documented degrade path —
// loud in its consequence rather than silently writing LimitFlags at the wrong offset, which is
// the failure mode the corpus-side struct-passing class is named for.
type jobObjectExtendedLimitInformation struct {
	BasicLimitInformation jobObjectBasicLimitInformation
	IoInfo                jobIOCounters
	ProcessMemoryLimit    uintptr
	JobMemoryLimit        uintptr
	PeakProcessMemoryUsed uintptr
	PeakJobMemoryUsed     uintptr
}

// newProcessGroup prepares cmd so its descendants can be killed as a unit. Called BEFORE Start.
// A zero job handle is the DEGRADE path, not a refusal: the group is returned either way and the
// kill falls back to the single-process form, with the error handed back so the caller can say
// once that it happened rather than leaving the degrade silent.
func newProcessGroup(cmd *exec.Cmd) (*processGroup, error) {
	job, err := createKillOnCloseJob()
	if err != nil {
		return &processGroup{}, err
	}
	return &processGroup{job: job}, nil
}

// attach puts the started child in the job. Called immediately AFTER Start — see residual 1.
func (g *processGroup) attach(cmd *exec.Cmd) error {
	if g.job == 0 || cmd.Process == nil || cmd.Process.Pid <= 0 {
		return nil
	}

	// os/exec still holds its own handle to the child at this point, so the pid cannot have been
	// reused by another process: OpenProcess here can only reach the child we started.
	handle, err := syscall.OpenProcess(processSetQuotaAccess|processTerminateAccess, false, uint32(cmd.Process.Pid))
	if err != nil {
		return err
	}
	defer syscall.CloseHandle(handle)

	if err := procAssignProcessToJobObject.Find(); err != nil {
		return err
	}
	if r, _, e := syscall.SyscallN(procAssignProcessToJobObject.Addr(), uintptr(g.job), uintptr(handle)); r == 0 {
		return winCallError(e)
	}
	return nil
}

// kill takes the child's whole job, then falls back to the single-process kill that was the
// behaviour before this existed. It is what cmd.Cancel runs when the package deadline expires, so
// the job goes first and the safety net stays behind it.
func (g *processGroup) kill(cmd *exec.Cmd) error {
	if g.job != 0 {
		if err := procTerminateJobObject.Find(); err == nil {
			if r, _, _ := syscall.SyscallN(procTerminateJobObject.Addr(), uintptr(g.job), 1); r != 0 {
				return nil
			}
		}
	}
	if cmd.Process == nil {
		return nil
	}
	return cmd.Process.Kill()
}

// close releases the job handle. Anything still inside it dies here, by KILL_ON_JOB_CLOSE — which
// is why this is deferred by the caller rather than left to the garbage collector.
func (g *processGroup) close() {
	if g.job != 0 {
		syscall.CloseHandle(g.job)
		g.job = 0
	}
}

func createKillOnCloseJob() (syscall.Handle, error) {
	if err := procCreateJobObjectW.Find(); err != nil {
		return 0, err
	}
	r, _, e := syscall.SyscallN(procCreateJobObjectW.Addr(), 0, 0)
	if r == 0 {
		return 0, winCallError(e)
	}
	job := syscall.Handle(r)

	if err := procSetInformationJobObject.Find(); err != nil {
		syscall.CloseHandle(job)
		return 0, err
	}

	var info jobObjectExtendedLimitInformation
	info.BasicLimitInformation.LimitFlags = jobObjectLimitKillOnJobClose

	r, _, e = syscall.SyscallN(procSetInformationJobObject.Addr(), uintptr(job),
		jobObjectExtendedLimitInformationClass, uintptr(unsafe.Pointer(&info)), unsafe.Sizeof(info))
	if r == 0 {
		syscall.CloseHandle(job)
		return 0, winCallError(e)
	}
	return job, nil
}

// winCallError turns the Errno a failed kernel32 call reports into an error, never returning a
// non-nil interface holding a zero Errno — the "succeeded but reported failure" shape that reads as
// an error with no message.
func winCallError(e syscall.Errno) error {
	if e != 0 {
		return e
	}
	return syscall.EINVAL
}
