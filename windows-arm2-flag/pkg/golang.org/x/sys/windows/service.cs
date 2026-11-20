// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class windows_package {

public static UntypedInt SC_MANAGER_CONNECT => 1;
public static UntypedInt SC_MANAGER_CREATE_SERVICE => 2;
public static UntypedInt SC_MANAGER_ENUMERATE_SERVICE => 4;
public static UntypedInt SC_MANAGER_LOCK => 8;
public static UntypedInt SC_MANAGER_QUERY_LOCK_STATUS => 16;
public static UntypedInt SC_MANAGER_MODIFY_BOOT_CONFIG => 32;
public static UntypedInt SC_MANAGER_ALL_ACCESS => 0xf003f;

public static UntypedInt SERVICE_KERNEL_DRIVER => 1;
public static UntypedInt SERVICE_FILE_SYSTEM_DRIVER => 2;
public static UntypedInt SERVICE_ADAPTER => 4;
public static UntypedInt SERVICE_RECOGNIZER_DRIVER => 8;
public static UntypedInt SERVICE_WIN32_OWN_PROCESS => 16;
public static UntypedInt SERVICE_WIN32_SHARE_PROCESS => 32;
public static UntypedInt SERVICE_WIN32 => /* SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS */ 48;
public static UntypedInt SERVICE_INTERACTIVE_PROCESS => 256;
public static UntypedInt SERVICE_DRIVER => /* SERVICE_KERNEL_DRIVER | SERVICE_FILE_SYSTEM_DRIVER | SERVICE_RECOGNIZER_DRIVER */ 11;
public static UntypedInt SERVICE_TYPE_ALL => /* SERVICE_WIN32 | SERVICE_ADAPTER | SERVICE_DRIVER | SERVICE_INTERACTIVE_PROCESS */ 319;
public static UntypedInt SERVICE_BOOT_START => 0;
public static UntypedInt SERVICE_SYSTEM_START => 1;
public static UntypedInt SERVICE_AUTO_START => 2;
public static UntypedInt SERVICE_DEMAND_START => 3;
public static UntypedInt SERVICE_DISABLED => 4;
public static UntypedInt SERVICE_ERROR_IGNORE => 0;
public static UntypedInt SERVICE_ERROR_NORMAL => 1;
public static UntypedInt SERVICE_ERROR_SEVERE => 2;
public static UntypedInt SERVICE_ERROR_CRITICAL => 3;
public static UntypedInt SC_STATUS_PROCESS_INFO => 0;
public static UntypedInt SC_ACTION_NONE => 0;
public static UntypedInt SC_ACTION_RESTART => 1;
public static UntypedInt SC_ACTION_REBOOT => 2;
public static UntypedInt SC_ACTION_RUN_COMMAND => 3;
public static UntypedInt SERVICE_STOPPED => 1;
public static UntypedInt SERVICE_START_PENDING => 2;
public static UntypedInt SERVICE_STOP_PENDING => 3;
public static UntypedInt SERVICE_RUNNING => 4;
public static UntypedInt SERVICE_CONTINUE_PENDING => 5;
public static UntypedInt SERVICE_PAUSE_PENDING => 6;
public static UntypedInt SERVICE_PAUSED => 7;
public static UntypedInt SERVICE_NO_CHANGE => 0xffffffff;
public static UntypedInt SERVICE_ACCEPT_STOP => 1;
public static UntypedInt SERVICE_ACCEPT_PAUSE_CONTINUE => 2;
public static UntypedInt SERVICE_ACCEPT_SHUTDOWN => 4;
public static UntypedInt SERVICE_ACCEPT_PARAMCHANGE => 8;
public static UntypedInt SERVICE_ACCEPT_NETBINDCHANGE => 16;
public static UntypedInt SERVICE_ACCEPT_HARDWAREPROFILECHANGE => 32;
public static UntypedInt SERVICE_ACCEPT_POWEREVENT => 64;
public static UntypedInt SERVICE_ACCEPT_SESSIONCHANGE => 128;
public static UntypedInt SERVICE_ACCEPT_PRESHUTDOWN => 256;
public static UntypedInt SERVICE_CONTROL_STOP => 1;
public static UntypedInt SERVICE_CONTROL_PAUSE => 2;
public static UntypedInt SERVICE_CONTROL_CONTINUE => 3;
public static UntypedInt SERVICE_CONTROL_INTERROGATE => 4;
public static UntypedInt SERVICE_CONTROL_SHUTDOWN => 5;
public static UntypedInt SERVICE_CONTROL_PARAMCHANGE => 6;
public static UntypedInt SERVICE_CONTROL_NETBINDADD => 7;
public static UntypedInt SERVICE_CONTROL_NETBINDREMOVE => 8;
public static UntypedInt SERVICE_CONTROL_NETBINDENABLE => 9;
public static UntypedInt SERVICE_CONTROL_NETBINDDISABLE => 10;
public static UntypedInt SERVICE_CONTROL_DEVICEEVENT => 11;
public static UntypedInt SERVICE_CONTROL_HARDWAREPROFILECHANGE => 12;
public static UntypedInt SERVICE_CONTROL_POWEREVENT => 13;
public static UntypedInt SERVICE_CONTROL_SESSIONCHANGE => 14;
public static UntypedInt SERVICE_CONTROL_PRESHUTDOWN => 15;
public static UntypedInt SERVICE_ACTIVE => 1;
public static UntypedInt SERVICE_INACTIVE => 2;
public static UntypedInt SERVICE_STATE_ALL => 3;
public static UntypedInt SERVICE_QUERY_CONFIG => 1;
public static UntypedInt SERVICE_CHANGE_CONFIG => 2;
public static UntypedInt SERVICE_QUERY_STATUS => 4;
public static UntypedInt SERVICE_ENUMERATE_DEPENDENTS => 8;
public static UntypedInt SERVICE_START => 16;
public static UntypedInt SERVICE_STOP => 32;
public static UntypedInt SERVICE_PAUSE_CONTINUE => 64;
public static UntypedInt SERVICE_INTERROGATE => 128;
public static UntypedInt SERVICE_USER_DEFINED_CONTROL => 256;
public static UntypedInt SERVICE_ALL_ACCESS => /* STANDARD_RIGHTS_REQUIRED | SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS | SERVICE_START | SERVICE_STOP | SERVICE_PAUSE_CONTINUE | SERVICE_INTERROGATE | SERVICE_USER_DEFINED_CONTROL */ 983551;
public static UntypedInt SERVICE_RUNS_IN_SYSTEM_PROCESS => 1;
public static UntypedInt SERVICE_CONFIG_DESCRIPTION => 1;
public static UntypedInt SERVICE_CONFIG_FAILURE_ACTIONS => 2;
public static UntypedInt SERVICE_CONFIG_DELAYED_AUTO_START_INFO => 3;
public static UntypedInt SERVICE_CONFIG_FAILURE_ACTIONS_FLAG => 4;
public static UntypedInt SERVICE_CONFIG_SERVICE_SID_INFO => 5;
public static UntypedInt SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO => 6;
public static UntypedInt SERVICE_CONFIG_PRESHUTDOWN_INFO => 7;
public static UntypedInt SERVICE_CONFIG_TRIGGER_INFO => 8;
public static UntypedInt SERVICE_CONFIG_PREFERRED_NODE => 9;
public static UntypedInt SERVICE_CONFIG_LAUNCH_PROTECTED => 12;
public static UntypedInt SERVICE_SID_TYPE_NONE => 0;
public static UntypedInt SERVICE_SID_TYPE_UNRESTRICTED => 1;
public static UntypedInt SERVICE_SID_TYPE_RESTRICTED => /* 2 | SERVICE_SID_TYPE_UNRESTRICTED */ 3;
public static UntypedInt SC_ENUM_PROCESS_INFO => 0;
public static UntypedInt SERVICE_NOTIFY_STATUS_CHANGE => 2;
public static UntypedInt SERVICE_NOTIFY_STOPPED => 0x00000001;
public static UntypedInt SERVICE_NOTIFY_START_PENDING => 0x00000002;
public static UntypedInt SERVICE_NOTIFY_STOP_PENDING => 0x00000004;
public static UntypedInt SERVICE_NOTIFY_RUNNING => 0x00000008;
public static UntypedInt SERVICE_NOTIFY_CONTINUE_PENDING => 0x00000010;
public static UntypedInt SERVICE_NOTIFY_PAUSE_PENDING => 0x00000020;
public static UntypedInt SERVICE_NOTIFY_PAUSED => 0x00000040;
public static UntypedInt SERVICE_NOTIFY_CREATED => 0x00000080;
public static UntypedInt SERVICE_NOTIFY_DELETED => 0x00000100;
public static UntypedInt SERVICE_NOTIFY_DELETE_PENDING => 0x00000200;
public static UntypedInt SC_EVENT_DATABASE_CHANGE => 0;
public static UntypedInt SC_EVENT_PROPERTY_CHANGE => 1;
public static UntypedInt SC_EVENT_STATUS_CHANGE => 2;
public static UntypedInt SERVICE_START_REASON_DEMAND => 0x00000001;
public static UntypedInt SERVICE_START_REASON_AUTO => 0x00000002;
public static UntypedInt SERVICE_START_REASON_TRIGGER => 0x00000004;
public static UntypedInt SERVICE_START_REASON_RESTART_ON_FAILURE => 0x00000008;
public static UntypedInt SERVICE_START_REASON_DELAYEDAUTO => 0x00000010;
public static UntypedInt SERVICE_DYNAMIC_INFORMATION_LEVEL_START_REASON => 1;

partial struct ENUM_SERVICE_STATUS {
    public ж<uint16> ServiceName;
    public ж<uint16> DisplayName;
    public SERVICE_STATUS ServiceStatus;
}

partial struct SERVICE_STATUS {
    public uint32 ServiceType;
    public uint32 CurrentState;
    public uint32 ControlsAccepted;
    public uint32 Win32ExitCode;
    public uint32 ServiceSpecificExitCode;
    public uint32 CheckPoint;
    public uint32 WaitHint;
}

partial struct SERVICE_TABLE_ENTRY {
    public ж<uint16> ServiceName;
    public uintptr ServiceProc;
}

partial struct QUERY_SERVICE_CONFIG {
    public uint32 ServiceType;
    public uint32 StartType;
    public uint32 ErrorControl;
    public ж<uint16> BinaryPathName;
    public ж<uint16> LoadOrderGroup;
    public uint32 TagId;
    public ж<uint16> Dependencies;
    public ж<uint16> ServiceStartName;
    public ж<uint16> DisplayName;
}

partial struct SERVICE_DESCRIPTION {
    public ж<uint16> Description;
}

partial struct SERVICE_DELAYED_AUTO_START_INFO {
    public uint32 IsDelayedAutoStartUp;
}

partial struct SERVICE_STATUS_PROCESS {
    public uint32 ServiceType;
    public uint32 CurrentState;
    public uint32 ControlsAccepted;
    public uint32 Win32ExitCode;
    public uint32 ServiceSpecificExitCode;
    public uint32 CheckPoint;
    public uint32 WaitHint;
    public uint32 ProcessId;
    public uint32 ServiceFlags;
}

partial struct ENUM_SERVICE_STATUS_PROCESS {
    public ж<uint16> ServiceName;
    public ж<uint16> DisplayName;
    public SERVICE_STATUS_PROCESS ServiceStatusProcess;
}

partial struct SERVICE_NOTIFY {
    public uint32 Version;
    public uintptr NotifyCallback;
    public uintptr Context;
    public uint32 NotificationStatus;
    public SERVICE_STATUS_PROCESS ServiceStatus;
    public uint32 NotificationTriggered;
    public ж<uint16> ServiceNames;
}

partial struct SERVICE_FAILURE_ACTIONS {
    public uint32 ResetPeriod;
    public ж<uint16> RebootMsg;
    public ж<uint16> Command;
    public uint32 ActionsCount;
    public ж<SC_ACTION> Actions;
}

partial struct SERVICE_FAILURE_ACTIONS_FLAG {
    public int32 FailureActionsOnNonCrashFailures;
}

partial struct SC_ACTION {
    public uint32 Type;
    public uint32 Delay;
}

partial struct QUERY_SERVICE_LOCK_STATUS {
    public uint32 IsLocked;
    public ж<uint16> LockOwner;
    public uint32 LockDuration;
}

} // end windows_package
