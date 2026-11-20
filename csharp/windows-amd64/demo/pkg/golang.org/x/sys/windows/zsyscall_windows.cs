namespace go.golang.org.x.sys;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class windows_package {

internal static @unsafe.Pointer _ᴛ1ʗ;

internal static UntypedInt errnoERROR_IO_PENDING => 997;

internal static error errERROR_IO_PENDING = ((syscall.Errno)errnoERROR_IO_PENDING);
internal static error errERROR_EINVAL = ((syscall.Errno)syscall.EINVAL);

internal static error errnoErr(syscall.Errno e) {
    switch (e) {
    case var ᴛ when ᴛ == (syscall.Errno)(0): {
        return errERROR_EINVAL;
    }
    case var ᴛ when ᴛ == errnoERROR_IO_PENDING: {
        return errERROR_IO_PENDING;
    }}

    return e;
}

internal static ж<LazyDLL> modCfgMgr32 = NewLazySystemDLL("CfgMgr32.dll");
internal static ж<LazyDLL> modadvapi32 = NewLazySystemDLL("advapi32.dll");
internal static ж<LazyDLL> modcrypt32 = NewLazySystemDLL("crypt32.dll");
internal static ж<LazyDLL> moddnsapi = NewLazySystemDLL("dnsapi.dll");
internal static ж<LazyDLL> moddwmapi = NewLazySystemDLL("dwmapi.dll");
internal static ж<LazyDLL> modiphlpapi = NewLazySystemDLL("iphlpapi.dll");
internal static ж<LazyDLL> modkernel32 = NewLazySystemDLL("kernel32.dll");
internal static ж<LazyDLL> modmswsock = NewLazySystemDLL("mswsock.dll");
internal static ж<LazyDLL> modnetapi32 = NewLazySystemDLL("netapi32.dll");
internal static ж<LazyDLL> modntdll = NewLazySystemDLL("ntdll.dll");
internal static ж<LazyDLL> modole32 = NewLazySystemDLL("ole32.dll");
internal static ж<LazyDLL> modpsapi = NewLazySystemDLL("psapi.dll");
internal static ж<LazyDLL> modsechost = NewLazySystemDLL("sechost.dll");
internal static ж<LazyDLL> modsecur32 = NewLazySystemDLL("secur32.dll");
internal static ж<LazyDLL> modsetupapi = NewLazySystemDLL("setupapi.dll");
internal static ж<LazyDLL> modshell32 = NewLazySystemDLL("shell32.dll");
internal static ж<LazyDLL> moduser32 = NewLazySystemDLL("user32.dll");
internal static ж<LazyDLL> moduserenv = NewLazySystemDLL("userenv.dll");
internal static ж<LazyDLL> modversion = NewLazySystemDLL("version.dll");
internal static ж<LazyDLL> modwinmm = NewLazySystemDLL("winmm.dll");
internal static ж<LazyDLL> modwintrust = NewLazySystemDLL("wintrust.dll");
internal static ж<LazyDLL> modws2_32 = NewLazySystemDLL("ws2_32.dll");
internal static ж<LazyDLL> modwtsapi32 = NewLazySystemDLL("wtsapi32.dll");
internal static ж<LazyProc> procCM_Get_DevNode_Status = modCfgMgr32.NewProc("CM_Get_DevNode_Status");
internal static ж<LazyProc> procCM_Get_Device_Interface_ListW = modCfgMgr32.NewProc("CM_Get_Device_Interface_ListW");
internal static ж<LazyProc> procCM_Get_Device_Interface_List_SizeW = modCfgMgr32.NewProc("CM_Get_Device_Interface_List_SizeW");
internal static ж<LazyProc> procCM_MapCrToWin32Err = modCfgMgr32.NewProc("CM_MapCrToWin32Err");
internal static ж<LazyProc> procAdjustTokenGroups = modadvapi32.NewProc("AdjustTokenGroups");
internal static ж<LazyProc> procAdjustTokenPrivileges = modadvapi32.NewProc("AdjustTokenPrivileges");
internal static ж<LazyProc> procAllocateAndInitializeSid = modadvapi32.NewProc("AllocateAndInitializeSid");
internal static ж<LazyProc> procBuildSecurityDescriptorW = modadvapi32.NewProc("BuildSecurityDescriptorW");
internal static ж<LazyProc> procChangeServiceConfig2W = modadvapi32.NewProc("ChangeServiceConfig2W");
internal static ж<LazyProc> procChangeServiceConfigW = modadvapi32.NewProc("ChangeServiceConfigW");
internal static ж<LazyProc> procCheckTokenMembership = modadvapi32.NewProc("CheckTokenMembership");
internal static ж<LazyProc> procCloseServiceHandle = modadvapi32.NewProc("CloseServiceHandle");
internal static ж<LazyProc> procControlService = modadvapi32.NewProc("ControlService");
internal static ж<LazyProc> procConvertSecurityDescriptorToStringSecurityDescriptorW = modadvapi32.NewProc("ConvertSecurityDescriptorToStringSecurityDescriptorW");
internal static ж<LazyProc> procConvertSidToStringSidW = modadvapi32.NewProc("ConvertSidToStringSidW");
internal static ж<LazyProc> procConvertStringSecurityDescriptorToSecurityDescriptorW = modadvapi32.NewProc("ConvertStringSecurityDescriptorToSecurityDescriptorW");
internal static ж<LazyProc> procConvertStringSidToSidW = modadvapi32.NewProc("ConvertStringSidToSidW");
internal static ж<LazyProc> procCopySid = modadvapi32.NewProc("CopySid");
internal static ж<LazyProc> procCreateProcessAsUserW = modadvapi32.NewProc("CreateProcessAsUserW");
internal static ж<LazyProc> procCreateServiceW = modadvapi32.NewProc("CreateServiceW");
internal static ж<LazyProc> procCreateWellKnownSid = modadvapi32.NewProc("CreateWellKnownSid");
internal static ж<LazyProc> procCryptAcquireContextW = modadvapi32.NewProc("CryptAcquireContextW");
internal static ж<LazyProc> procCryptGenRandom = modadvapi32.NewProc("CryptGenRandom");
internal static ж<LazyProc> procCryptReleaseContext = modadvapi32.NewProc("CryptReleaseContext");
internal static ж<LazyProc> procDeleteService = modadvapi32.NewProc("DeleteService");
internal static ж<LazyProc> procDeregisterEventSource = modadvapi32.NewProc("DeregisterEventSource");
internal static ж<LazyProc> procDuplicateTokenEx = modadvapi32.NewProc("DuplicateTokenEx");
internal static ж<LazyProc> procEnumDependentServicesW = modadvapi32.NewProc("EnumDependentServicesW");
internal static ж<LazyProc> procEnumServicesStatusExW = modadvapi32.NewProc("EnumServicesStatusExW");
internal static ж<LazyProc> procEqualSid = modadvapi32.NewProc("EqualSid");
internal static ж<LazyProc> procFreeSid = modadvapi32.NewProc("FreeSid");
internal static ж<LazyProc> procGetAce = modadvapi32.NewProc("GetAce");
internal static ж<LazyProc> procGetLengthSid = modadvapi32.NewProc("GetLengthSid");
internal static ж<LazyProc> procGetNamedSecurityInfoW = modadvapi32.NewProc("GetNamedSecurityInfoW");
internal static ж<LazyProc> procGetSecurityDescriptorControl = modadvapi32.NewProc("GetSecurityDescriptorControl");
internal static ж<LazyProc> procGetSecurityDescriptorDacl = modadvapi32.NewProc("GetSecurityDescriptorDacl");
internal static ж<LazyProc> procGetSecurityDescriptorGroup = modadvapi32.NewProc("GetSecurityDescriptorGroup");
internal static ж<LazyProc> procGetSecurityDescriptorLength = modadvapi32.NewProc("GetSecurityDescriptorLength");
internal static ж<LazyProc> procGetSecurityDescriptorOwner = modadvapi32.NewProc("GetSecurityDescriptorOwner");
internal static ж<LazyProc> procGetSecurityDescriptorRMControl = modadvapi32.NewProc("GetSecurityDescriptorRMControl");
internal static ж<LazyProc> procGetSecurityDescriptorSacl = modadvapi32.NewProc("GetSecurityDescriptorSacl");
internal static ж<LazyProc> procGetSecurityInfo = modadvapi32.NewProc("GetSecurityInfo");
internal static ж<LazyProc> procGetSidIdentifierAuthority = modadvapi32.NewProc("GetSidIdentifierAuthority");
internal static ж<LazyProc> procGetSidSubAuthority = modadvapi32.NewProc("GetSidSubAuthority");
internal static ж<LazyProc> procGetSidSubAuthorityCount = modadvapi32.NewProc("GetSidSubAuthorityCount");
internal static ж<LazyProc> procGetTokenInformation = modadvapi32.NewProc("GetTokenInformation");
internal static ж<LazyProc> procImpersonateSelf = modadvapi32.NewProc("ImpersonateSelf");
internal static ж<LazyProc> procInitializeSecurityDescriptor = modadvapi32.NewProc("InitializeSecurityDescriptor");
internal static ж<LazyProc> procInitiateSystemShutdownExW = modadvapi32.NewProc("InitiateSystemShutdownExW");
internal static ж<LazyProc> procIsTokenRestricted = modadvapi32.NewProc("IsTokenRestricted");
internal static ж<LazyProc> procIsValidSecurityDescriptor = modadvapi32.NewProc("IsValidSecurityDescriptor");
internal static ж<LazyProc> procIsValidSid = modadvapi32.NewProc("IsValidSid");
internal static ж<LazyProc> procIsWellKnownSid = modadvapi32.NewProc("IsWellKnownSid");
internal static ж<LazyProc> procLookupAccountNameW = modadvapi32.NewProc("LookupAccountNameW");
internal static ж<LazyProc> procLookupAccountSidW = modadvapi32.NewProc("LookupAccountSidW");
internal static ж<LazyProc> procLookupPrivilegeValueW = modadvapi32.NewProc("LookupPrivilegeValueW");
internal static ж<LazyProc> procMakeAbsoluteSD = modadvapi32.NewProc("MakeAbsoluteSD");
internal static ж<LazyProc> procMakeSelfRelativeSD = modadvapi32.NewProc("MakeSelfRelativeSD");
internal static ж<LazyProc> procNotifyServiceStatusChangeW = modadvapi32.NewProc("NotifyServiceStatusChangeW");
internal static ж<LazyProc> procOpenProcessToken = modadvapi32.NewProc("OpenProcessToken");
internal static ж<LazyProc> procOpenSCManagerW = modadvapi32.NewProc("OpenSCManagerW");
internal static ж<LazyProc> procOpenServiceW = modadvapi32.NewProc("OpenServiceW");
internal static ж<LazyProc> procOpenThreadToken = modadvapi32.NewProc("OpenThreadToken");
internal static ж<LazyProc> procQueryServiceConfig2W = modadvapi32.NewProc("QueryServiceConfig2W");
internal static ж<LazyProc> procQueryServiceConfigW = modadvapi32.NewProc("QueryServiceConfigW");
internal static ж<LazyProc> procQueryServiceDynamicInformation = modadvapi32.NewProc("QueryServiceDynamicInformation");
internal static ж<LazyProc> procQueryServiceLockStatusW = modadvapi32.NewProc("QueryServiceLockStatusW");
internal static ж<LazyProc> procQueryServiceStatus = modadvapi32.NewProc("QueryServiceStatus");
internal static ж<LazyProc> procQueryServiceStatusEx = modadvapi32.NewProc("QueryServiceStatusEx");
internal static ж<LazyProc> procRegCloseKey = modadvapi32.NewProc("RegCloseKey");
internal static ж<LazyProc> procRegEnumKeyExW = modadvapi32.NewProc("RegEnumKeyExW");
internal static ж<LazyProc> procRegNotifyChangeKeyValue = modadvapi32.NewProc("RegNotifyChangeKeyValue");
internal static ж<LazyProc> procRegOpenKeyExW = modadvapi32.NewProc("RegOpenKeyExW");
internal static ж<LazyProc> procRegQueryInfoKeyW = modadvapi32.NewProc("RegQueryInfoKeyW");
internal static ж<LazyProc> procRegQueryValueExW = modadvapi32.NewProc("RegQueryValueExW");
internal static ж<LazyProc> procRegisterEventSourceW = modadvapi32.NewProc("RegisterEventSourceW");
internal static ж<LazyProc> procRegisterServiceCtrlHandlerExW = modadvapi32.NewProc("RegisterServiceCtrlHandlerExW");
internal static ж<LazyProc> procReportEventW = modadvapi32.NewProc("ReportEventW");
internal static ж<LazyProc> procRevertToSelf = modadvapi32.NewProc("RevertToSelf");
internal static ж<LazyProc> procSetEntriesInAclW = modadvapi32.NewProc("SetEntriesInAclW");
internal static ж<LazyProc> procSetKernelObjectSecurity = modadvapi32.NewProc("SetKernelObjectSecurity");
internal static ж<LazyProc> procSetNamedSecurityInfoW = modadvapi32.NewProc("SetNamedSecurityInfoW");
internal static ж<LazyProc> procSetSecurityDescriptorControl = modadvapi32.NewProc("SetSecurityDescriptorControl");
internal static ж<LazyProc> procSetSecurityDescriptorDacl = modadvapi32.NewProc("SetSecurityDescriptorDacl");
internal static ж<LazyProc> procSetSecurityDescriptorGroup = modadvapi32.NewProc("SetSecurityDescriptorGroup");
internal static ж<LazyProc> procSetSecurityDescriptorOwner = modadvapi32.NewProc("SetSecurityDescriptorOwner");
internal static ж<LazyProc> procSetSecurityDescriptorRMControl = modadvapi32.NewProc("SetSecurityDescriptorRMControl");
internal static ж<LazyProc> procSetSecurityDescriptorSacl = modadvapi32.NewProc("SetSecurityDescriptorSacl");
internal static ж<LazyProc> procSetSecurityInfo = modadvapi32.NewProc("SetSecurityInfo");
internal static ж<LazyProc> procSetServiceStatus = modadvapi32.NewProc("SetServiceStatus");
internal static ж<LazyProc> procSetThreadToken = modadvapi32.NewProc("SetThreadToken");
internal static ж<LazyProc> procSetTokenInformation = modadvapi32.NewProc("SetTokenInformation");
internal static ж<LazyProc> procStartServiceCtrlDispatcherW = modadvapi32.NewProc("StartServiceCtrlDispatcherW");
internal static ж<LazyProc> procStartServiceW = modadvapi32.NewProc("StartServiceW");
internal static ж<LazyProc> procCertAddCertificateContextToStore = modcrypt32.NewProc("CertAddCertificateContextToStore");
internal static ж<LazyProc> procCertCloseStore = modcrypt32.NewProc("CertCloseStore");
internal static ж<LazyProc> procCertCreateCertificateContext = modcrypt32.NewProc("CertCreateCertificateContext");
internal static ж<LazyProc> procCertDeleteCertificateFromStore = modcrypt32.NewProc("CertDeleteCertificateFromStore");
internal static ж<LazyProc> procCertDuplicateCertificateContext = modcrypt32.NewProc("CertDuplicateCertificateContext");
internal static ж<LazyProc> procCertEnumCertificatesInStore = modcrypt32.NewProc("CertEnumCertificatesInStore");
internal static ж<LazyProc> procCertFindCertificateInStore = modcrypt32.NewProc("CertFindCertificateInStore");
internal static ж<LazyProc> procCertFindChainInStore = modcrypt32.NewProc("CertFindChainInStore");
internal static ж<LazyProc> procCertFindExtension = modcrypt32.NewProc("CertFindExtension");
internal static ж<LazyProc> procCertFreeCertificateChain = modcrypt32.NewProc("CertFreeCertificateChain");
internal static ж<LazyProc> procCertFreeCertificateContext = modcrypt32.NewProc("CertFreeCertificateContext");
internal static ж<LazyProc> procCertGetCertificateChain = modcrypt32.NewProc("CertGetCertificateChain");
internal static ж<LazyProc> procCertGetNameStringW = modcrypt32.NewProc("CertGetNameStringW");
internal static ж<LazyProc> procCertOpenStore = modcrypt32.NewProc("CertOpenStore");
internal static ж<LazyProc> procCertOpenSystemStoreW = modcrypt32.NewProc("CertOpenSystemStoreW");
internal static ж<LazyProc> procCertVerifyCertificateChainPolicy = modcrypt32.NewProc("CertVerifyCertificateChainPolicy");
internal static ж<LazyProc> procCryptAcquireCertificatePrivateKey = modcrypt32.NewProc("CryptAcquireCertificatePrivateKey");
internal static ж<LazyProc> procCryptDecodeObject = modcrypt32.NewProc("CryptDecodeObject");
internal static ж<LazyProc> procCryptProtectData = modcrypt32.NewProc("CryptProtectData");
internal static ж<LazyProc> procCryptQueryObject = modcrypt32.NewProc("CryptQueryObject");
internal static ж<LazyProc> procCryptUnprotectData = modcrypt32.NewProc("CryptUnprotectData");
internal static ж<LazyProc> procPFXImportCertStore = modcrypt32.NewProc("PFXImportCertStore");
internal static ж<LazyProc> procDnsNameCompare_W = moddnsapi.NewProc("DnsNameCompare_W");
internal static ж<LazyProc> procDnsQuery_W = moddnsapi.NewProc("DnsQuery_W");
internal static ж<LazyProc> procDnsRecordListFree = moddnsapi.NewProc("DnsRecordListFree");
internal static ж<LazyProc> procDwmGetWindowAttribute = moddwmapi.NewProc("DwmGetWindowAttribute");
internal static ж<LazyProc> procDwmSetWindowAttribute = moddwmapi.NewProc("DwmSetWindowAttribute");
internal static ж<LazyProc> procCancelMibChangeNotify2 = modiphlpapi.NewProc("CancelMibChangeNotify2");
internal static ж<LazyProc> procFreeMibTable = modiphlpapi.NewProc("FreeMibTable");
internal static ж<LazyProc> procGetAdaptersAddresses = modiphlpapi.NewProc("GetAdaptersAddresses");
internal static ж<LazyProc> procGetAdaptersInfo = modiphlpapi.NewProc("GetAdaptersInfo");
internal static ж<LazyProc> procGetBestInterfaceEx = modiphlpapi.NewProc("GetBestInterfaceEx");
internal static ж<LazyProc> procGetIfEntry = modiphlpapi.NewProc("GetIfEntry");
internal static ж<LazyProc> procGetIfEntry2Ex = modiphlpapi.NewProc("GetIfEntry2Ex");
internal static ж<LazyProc> procGetIfTable2Ex = modiphlpapi.NewProc("GetIfTable2Ex");
internal static ж<LazyProc> procGetIpForwardEntry2 = modiphlpapi.NewProc("GetIpForwardEntry2");
internal static ж<LazyProc> procGetIpForwardTable2 = modiphlpapi.NewProc("GetIpForwardTable2");
internal static ж<LazyProc> procGetIpInterfaceEntry = modiphlpapi.NewProc("GetIpInterfaceEntry");
internal static ж<LazyProc> procGetIpInterfaceTable = modiphlpapi.NewProc("GetIpInterfaceTable");
internal static ж<LazyProc> procGetUnicastIpAddressEntry = modiphlpapi.NewProc("GetUnicastIpAddressEntry");
internal static ж<LazyProc> procGetUnicastIpAddressTable = modiphlpapi.NewProc("GetUnicastIpAddressTable");
internal static ж<LazyProc> procNotifyIpInterfaceChange = modiphlpapi.NewProc("NotifyIpInterfaceChange");
internal static ж<LazyProc> procNotifyRouteChange2 = modiphlpapi.NewProc("NotifyRouteChange2");
internal static ж<LazyProc> procNotifyUnicastIpAddressChange = modiphlpapi.NewProc("NotifyUnicastIpAddressChange");
internal static ж<LazyProc> procAddDllDirectory = modkernel32.NewProc("AddDllDirectory");
internal static ж<LazyProc> procAssignProcessToJobObject = modkernel32.NewProc("AssignProcessToJobObject");
internal static ж<LazyProc> procCancelIo = modkernel32.NewProc("CancelIo");
internal static ж<LazyProc> procCancelIoEx = modkernel32.NewProc("CancelIoEx");
internal static ж<LazyProc> procClearCommBreak = modkernel32.NewProc("ClearCommBreak");
internal static ж<LazyProc> procClearCommError = modkernel32.NewProc("ClearCommError");
internal static ж<LazyProc> procCloseHandle = modkernel32.NewProc("CloseHandle");
internal static ж<LazyProc> procClosePseudoConsole = modkernel32.NewProc("ClosePseudoConsole");
internal static ж<LazyProc> procConnectNamedPipe = modkernel32.NewProc("ConnectNamedPipe");
internal static ж<LazyProc> procCreateDirectoryW = modkernel32.NewProc("CreateDirectoryW");
internal static ж<LazyProc> procCreateEventExW = modkernel32.NewProc("CreateEventExW");
internal static ж<LazyProc> procCreateEventW = modkernel32.NewProc("CreateEventW");
internal static ж<LazyProc> procCreateFileMappingW = modkernel32.NewProc("CreateFileMappingW");
internal static ж<LazyProc> procCreateFileW = modkernel32.NewProc("CreateFileW");
internal static ж<LazyProc> procCreateHardLinkW = modkernel32.NewProc("CreateHardLinkW");
internal static ж<LazyProc> procCreateIoCompletionPort = modkernel32.NewProc("CreateIoCompletionPort");
internal static ж<LazyProc> procCreateJobObjectW = modkernel32.NewProc("CreateJobObjectW");
internal static ж<LazyProc> procCreateMutexExW = modkernel32.NewProc("CreateMutexExW");
internal static ж<LazyProc> procCreateMutexW = modkernel32.NewProc("CreateMutexW");
internal static ж<LazyProc> procCreateNamedPipeW = modkernel32.NewProc("CreateNamedPipeW");
internal static ж<LazyProc> procCreatePipe = modkernel32.NewProc("CreatePipe");
internal static ж<LazyProc> procCreateProcessW = modkernel32.NewProc("CreateProcessW");
internal static ж<LazyProc> procCreatePseudoConsole = modkernel32.NewProc("CreatePseudoConsole");
internal static ж<LazyProc> procCreateSymbolicLinkW = modkernel32.NewProc("CreateSymbolicLinkW");
internal static ж<LazyProc> procCreateToolhelp32Snapshot = modkernel32.NewProc("CreateToolhelp32Snapshot");
internal static ж<LazyProc> procDefineDosDeviceW = modkernel32.NewProc("DefineDosDeviceW");
internal static ж<LazyProc> procDeleteFileW = modkernel32.NewProc("DeleteFileW");
internal static ж<LazyProc> procDeleteProcThreadAttributeList = modkernel32.NewProc("DeleteProcThreadAttributeList");
internal static ж<LazyProc> procDeleteVolumeMountPointW = modkernel32.NewProc("DeleteVolumeMountPointW");
internal static ж<LazyProc> procDeviceIoControl = modkernel32.NewProc("DeviceIoControl");
internal static ж<LazyProc> procDisconnectNamedPipe = modkernel32.NewProc("DisconnectNamedPipe");
internal static ж<LazyProc> procDuplicateHandle = modkernel32.NewProc("DuplicateHandle");
internal static ж<LazyProc> procEscapeCommFunction = modkernel32.NewProc("EscapeCommFunction");
internal static ж<LazyProc> procExitProcess = modkernel32.NewProc("ExitProcess");
internal static ж<LazyProc> procExpandEnvironmentStringsW = modkernel32.NewProc("ExpandEnvironmentStringsW");
internal static ж<LazyProc> procFindClose = modkernel32.NewProc("FindClose");
internal static ж<LazyProc> procFindCloseChangeNotification = modkernel32.NewProc("FindCloseChangeNotification");
internal static ж<LazyProc> procFindFirstChangeNotificationW = modkernel32.NewProc("FindFirstChangeNotificationW");
internal static ж<LazyProc> procFindFirstFileW = modkernel32.NewProc("FindFirstFileW");
internal static ж<LazyProc> procFindFirstVolumeMountPointW = modkernel32.NewProc("FindFirstVolumeMountPointW");
internal static ж<LazyProc> procFindFirstVolumeW = modkernel32.NewProc("FindFirstVolumeW");
internal static ж<LazyProc> procFindNextChangeNotification = modkernel32.NewProc("FindNextChangeNotification");
internal static ж<LazyProc> procFindNextFileW = modkernel32.NewProc("FindNextFileW");
internal static ж<LazyProc> procFindNextVolumeMountPointW = modkernel32.NewProc("FindNextVolumeMountPointW");
internal static ж<LazyProc> procFindNextVolumeW = modkernel32.NewProc("FindNextVolumeW");
internal static ж<LazyProc> procFindResourceW = modkernel32.NewProc("FindResourceW");
internal static ж<LazyProc> procFindVolumeClose = modkernel32.NewProc("FindVolumeClose");
internal static ж<LazyProc> procFindVolumeMountPointClose = modkernel32.NewProc("FindVolumeMountPointClose");
internal static ж<LazyProc> procFlushConsoleInputBuffer = modkernel32.NewProc("FlushConsoleInputBuffer");
internal static ж<LazyProc> procFlushFileBuffers = modkernel32.NewProc("FlushFileBuffers");
internal static ж<LazyProc> procFlushViewOfFile = modkernel32.NewProc("FlushViewOfFile");
internal static ж<LazyProc> procFormatMessageW = modkernel32.NewProc("FormatMessageW");
internal static ж<LazyProc> procFreeEnvironmentStringsW = modkernel32.NewProc("FreeEnvironmentStringsW");
internal static ж<LazyProc> procFreeLibrary = modkernel32.NewProc("FreeLibrary");
internal static ж<LazyProc> procGenerateConsoleCtrlEvent = modkernel32.NewProc("GenerateConsoleCtrlEvent");
internal static ж<LazyProc> procGetACP = modkernel32.NewProc("GetACP");
internal static ж<LazyProc> procGetActiveProcessorCount = modkernel32.NewProc("GetActiveProcessorCount");
internal static ж<LazyProc> procGetCommModemStatus = modkernel32.NewProc("GetCommModemStatus");
internal static ж<LazyProc> procGetCommState = modkernel32.NewProc("GetCommState");
internal static ж<LazyProc> procGetCommTimeouts = modkernel32.NewProc("GetCommTimeouts");
internal static ж<LazyProc> procGetCommandLineW = modkernel32.NewProc("GetCommandLineW");
internal static ж<LazyProc> procGetComputerNameExW = modkernel32.NewProc("GetComputerNameExW");
internal static ж<LazyProc> procGetComputerNameW = modkernel32.NewProc("GetComputerNameW");
internal static ж<LazyProc> procGetConsoleCP = modkernel32.NewProc("GetConsoleCP");
internal static ж<LazyProc> procGetConsoleMode = modkernel32.NewProc("GetConsoleMode");
internal static ж<LazyProc> procGetConsoleOutputCP = modkernel32.NewProc("GetConsoleOutputCP");
internal static ж<LazyProc> procGetConsoleScreenBufferInfo = modkernel32.NewProc("GetConsoleScreenBufferInfo");
internal static ж<LazyProc> procGetCurrentDirectoryW = modkernel32.NewProc("GetCurrentDirectoryW");
internal static ж<LazyProc> procGetCurrentProcessId = modkernel32.NewProc("GetCurrentProcessId");
internal static ж<LazyProc> procGetCurrentThreadId = modkernel32.NewProc("GetCurrentThreadId");
internal static ж<LazyProc> procGetDiskFreeSpaceExW = modkernel32.NewProc("GetDiskFreeSpaceExW");
internal static ж<LazyProc> procGetDriveTypeW = modkernel32.NewProc("GetDriveTypeW");
internal static ж<LazyProc> procGetEnvironmentStringsW = modkernel32.NewProc("GetEnvironmentStringsW");
internal static ж<LazyProc> procGetEnvironmentVariableW = modkernel32.NewProc("GetEnvironmentVariableW");
internal static ж<LazyProc> procGetExitCodeProcess = modkernel32.NewProc("GetExitCodeProcess");
internal static ж<LazyProc> procGetFileAttributesExW = modkernel32.NewProc("GetFileAttributesExW");
internal static ж<LazyProc> procGetFileAttributesW = modkernel32.NewProc("GetFileAttributesW");
internal static ж<LazyProc> procGetFileInformationByHandle = modkernel32.NewProc("GetFileInformationByHandle");
internal static ж<LazyProc> procGetFileInformationByHandleEx = modkernel32.NewProc("GetFileInformationByHandleEx");
internal static ж<LazyProc> procGetFileTime = modkernel32.NewProc("GetFileTime");
internal static ж<LazyProc> procGetFileType = modkernel32.NewProc("GetFileType");
internal static ж<LazyProc> procGetFinalPathNameByHandleW = modkernel32.NewProc("GetFinalPathNameByHandleW");
internal static ж<LazyProc> procGetFullPathNameW = modkernel32.NewProc("GetFullPathNameW");
internal static ж<LazyProc> procGetLargePageMinimum = modkernel32.NewProc("GetLargePageMinimum");
internal static ж<LazyProc> procGetLastError = modkernel32.NewProc("GetLastError");
internal static ж<LazyProc> procGetLogicalDriveStringsW = modkernel32.NewProc("GetLogicalDriveStringsW");
internal static ж<LazyProc> procGetLogicalDrives = modkernel32.NewProc("GetLogicalDrives");
internal static ж<LazyProc> procGetLongPathNameW = modkernel32.NewProc("GetLongPathNameW");
internal static ж<LazyProc> procGetMaximumProcessorCount = modkernel32.NewProc("GetMaximumProcessorCount");
internal static ж<LazyProc> procGetModuleFileNameW = modkernel32.NewProc("GetModuleFileNameW");
internal static ж<LazyProc> procGetModuleHandleExW = modkernel32.NewProc("GetModuleHandleExW");
internal static ж<LazyProc> procGetNamedPipeClientProcessId = modkernel32.NewProc("GetNamedPipeClientProcessId");
internal static ж<LazyProc> procGetNamedPipeHandleStateW = modkernel32.NewProc("GetNamedPipeHandleStateW");
internal static ж<LazyProc> procGetNamedPipeInfo = modkernel32.NewProc("GetNamedPipeInfo");
internal static ж<LazyProc> procGetNamedPipeServerProcessId = modkernel32.NewProc("GetNamedPipeServerProcessId");
internal static ж<LazyProc> procGetNumberOfConsoleInputEvents = modkernel32.NewProc("GetNumberOfConsoleInputEvents");
internal static ж<LazyProc> procGetOverlappedResult = modkernel32.NewProc("GetOverlappedResult");
internal static ж<LazyProc> procGetPriorityClass = modkernel32.NewProc("GetPriorityClass");
internal static ж<LazyProc> procGetProcAddress = modkernel32.NewProc("GetProcAddress");
internal static ж<LazyProc> procGetProcessId = modkernel32.NewProc("GetProcessId");
internal static ж<LazyProc> procGetProcessPreferredUILanguages = modkernel32.NewProc("GetProcessPreferredUILanguages");
internal static ж<LazyProc> procGetProcessShutdownParameters = modkernel32.NewProc("GetProcessShutdownParameters");
internal static ж<LazyProc> procGetProcessTimes = modkernel32.NewProc("GetProcessTimes");
internal static ж<LazyProc> procGetProcessWorkingSetSizeEx = modkernel32.NewProc("GetProcessWorkingSetSizeEx");
internal static ж<LazyProc> procGetQueuedCompletionStatus = modkernel32.NewProc("GetQueuedCompletionStatus");
internal static ж<LazyProc> procGetShortPathNameW = modkernel32.NewProc("GetShortPathNameW");
internal static ж<LazyProc> procGetStartupInfoW = modkernel32.NewProc("GetStartupInfoW");
internal static ж<LazyProc> procGetStdHandle = modkernel32.NewProc("GetStdHandle");
internal static ж<LazyProc> procGetSystemDirectoryW = modkernel32.NewProc("GetSystemDirectoryW");
internal static ж<LazyProc> procGetSystemPreferredUILanguages = modkernel32.NewProc("GetSystemPreferredUILanguages");
internal static ж<LazyProc> procGetSystemTimeAsFileTime = modkernel32.NewProc("GetSystemTimeAsFileTime");
internal static ж<LazyProc> procGetSystemTimePreciseAsFileTime = modkernel32.NewProc("GetSystemTimePreciseAsFileTime");
internal static ж<LazyProc> procGetSystemWindowsDirectoryW = modkernel32.NewProc("GetSystemWindowsDirectoryW");
internal static ж<LazyProc> procGetTempPathW = modkernel32.NewProc("GetTempPathW");
internal static ж<LazyProc> procGetThreadPreferredUILanguages = modkernel32.NewProc("GetThreadPreferredUILanguages");
internal static ж<LazyProc> procGetTickCount64 = modkernel32.NewProc("GetTickCount64");
internal static ж<LazyProc> procGetTimeZoneInformation = modkernel32.NewProc("GetTimeZoneInformation");
internal static ж<LazyProc> procGetUserPreferredUILanguages = modkernel32.NewProc("GetUserPreferredUILanguages");
internal static ж<LazyProc> procGetVersion = modkernel32.NewProc("GetVersion");
internal static ж<LazyProc> procGetVolumeInformationByHandleW = modkernel32.NewProc("GetVolumeInformationByHandleW");
internal static ж<LazyProc> procGetVolumeInformationW = modkernel32.NewProc("GetVolumeInformationW");
internal static ж<LazyProc> procGetVolumeNameForVolumeMountPointW = modkernel32.NewProc("GetVolumeNameForVolumeMountPointW");
internal static ж<LazyProc> procGetVolumePathNameW = modkernel32.NewProc("GetVolumePathNameW");
internal static ж<LazyProc> procGetVolumePathNamesForVolumeNameW = modkernel32.NewProc("GetVolumePathNamesForVolumeNameW");
internal static ж<LazyProc> procGetWindowsDirectoryW = modkernel32.NewProc("GetWindowsDirectoryW");
internal static ж<LazyProc> procInitializeProcThreadAttributeList = modkernel32.NewProc("InitializeProcThreadAttributeList");
internal static ж<LazyProc> procIsProcessorFeaturePresent = modkernel32.NewProc("IsProcessorFeaturePresent");
internal static ж<LazyProc> procIsWow64Process = modkernel32.NewProc("IsWow64Process");
internal static ж<LazyProc> procIsWow64Process2 = modkernel32.NewProc("IsWow64Process2");
internal static ж<LazyProc> procLoadLibraryExW = modkernel32.NewProc("LoadLibraryExW");
internal static ж<LazyProc> procLoadLibraryW = modkernel32.NewProc("LoadLibraryW");
internal static ж<LazyProc> procLoadResource = modkernel32.NewProc("LoadResource");
internal static ж<LazyProc> procLocalAlloc = modkernel32.NewProc("LocalAlloc");
internal static ж<LazyProc> procLocalFree = modkernel32.NewProc("LocalFree");
internal static ж<LazyProc> procLockFileEx = modkernel32.NewProc("LockFileEx");
internal static ж<LazyProc> procLockResource = modkernel32.NewProc("LockResource");
internal static ж<LazyProc> procMapViewOfFile = modkernel32.NewProc("MapViewOfFile");
internal static ж<LazyProc> procModule32FirstW = modkernel32.NewProc("Module32FirstW");
internal static ж<LazyProc> procModule32NextW = modkernel32.NewProc("Module32NextW");
internal static ж<LazyProc> procMoveFileExW = modkernel32.NewProc("MoveFileExW");
internal static ж<LazyProc> procMoveFileW = modkernel32.NewProc("MoveFileW");
internal static ж<LazyProc> procMultiByteToWideChar = modkernel32.NewProc("MultiByteToWideChar");
internal static ж<LazyProc> procOpenEventW = modkernel32.NewProc("OpenEventW");
internal static ж<LazyProc> procOpenMutexW = modkernel32.NewProc("OpenMutexW");
internal static ж<LazyProc> procOpenProcess = modkernel32.NewProc("OpenProcess");
internal static ж<LazyProc> procOpenThread = modkernel32.NewProc("OpenThread");
internal static ж<LazyProc> procPostQueuedCompletionStatus = modkernel32.NewProc("PostQueuedCompletionStatus");
internal static ж<LazyProc> procProcess32FirstW = modkernel32.NewProc("Process32FirstW");
internal static ж<LazyProc> procProcess32NextW = modkernel32.NewProc("Process32NextW");
internal static ж<LazyProc> procProcessIdToSessionId = modkernel32.NewProc("ProcessIdToSessionId");
internal static ж<LazyProc> procPulseEvent = modkernel32.NewProc("PulseEvent");
internal static ж<LazyProc> procPurgeComm = modkernel32.NewProc("PurgeComm");
internal static ж<LazyProc> procQueryDosDeviceW = modkernel32.NewProc("QueryDosDeviceW");
internal static ж<LazyProc> procQueryFullProcessImageNameW = modkernel32.NewProc("QueryFullProcessImageNameW");
internal static ж<LazyProc> procQueryInformationJobObject = modkernel32.NewProc("QueryInformationJobObject");
internal static ж<LazyProc> procReadConsoleW = modkernel32.NewProc("ReadConsoleW");
internal static ж<LazyProc> procReadDirectoryChangesW = modkernel32.NewProc("ReadDirectoryChangesW");
internal static ж<LazyProc> procReadFile = modkernel32.NewProc("ReadFile");
internal static ж<LazyProc> procReadProcessMemory = modkernel32.NewProc("ReadProcessMemory");
internal static ж<LazyProc> procReleaseMutex = modkernel32.NewProc("ReleaseMutex");
internal static ж<LazyProc> procRemoveDirectoryW = modkernel32.NewProc("RemoveDirectoryW");
internal static ж<LazyProc> procRemoveDllDirectory = modkernel32.NewProc("RemoveDllDirectory");
internal static ж<LazyProc> procResetEvent = modkernel32.NewProc("ResetEvent");
internal static ж<LazyProc> procResizePseudoConsole = modkernel32.NewProc("ResizePseudoConsole");
internal static ж<LazyProc> procResumeThread = modkernel32.NewProc("ResumeThread");
internal static ж<LazyProc> procSetCommBreak = modkernel32.NewProc("SetCommBreak");
internal static ж<LazyProc> procSetCommMask = modkernel32.NewProc("SetCommMask");
internal static ж<LazyProc> procSetCommState = modkernel32.NewProc("SetCommState");
internal static ж<LazyProc> procSetCommTimeouts = modkernel32.NewProc("SetCommTimeouts");
internal static ж<LazyProc> procSetConsoleCP = modkernel32.NewProc("SetConsoleCP");
internal static ж<LazyProc> procSetConsoleCursorPosition = modkernel32.NewProc("SetConsoleCursorPosition");
internal static ж<LazyProc> procSetConsoleMode = modkernel32.NewProc("SetConsoleMode");
internal static ж<LazyProc> procSetConsoleOutputCP = modkernel32.NewProc("SetConsoleOutputCP");
internal static ж<LazyProc> procSetCurrentDirectoryW = modkernel32.NewProc("SetCurrentDirectoryW");
internal static ж<LazyProc> procSetDefaultDllDirectories = modkernel32.NewProc("SetDefaultDllDirectories");
internal static ж<LazyProc> procSetDllDirectoryW = modkernel32.NewProc("SetDllDirectoryW");
internal static ж<LazyProc> procSetEndOfFile = modkernel32.NewProc("SetEndOfFile");
internal static ж<LazyProc> procSetEnvironmentVariableW = modkernel32.NewProc("SetEnvironmentVariableW");
internal static ж<LazyProc> procSetErrorMode = modkernel32.NewProc("SetErrorMode");
internal static ж<LazyProc> procSetEvent = modkernel32.NewProc("SetEvent");
internal static ж<LazyProc> procSetFileAttributesW = modkernel32.NewProc("SetFileAttributesW");
internal static ж<LazyProc> procSetFileCompletionNotificationModes = modkernel32.NewProc("SetFileCompletionNotificationModes");
internal static ж<LazyProc> procSetFileInformationByHandle = modkernel32.NewProc("SetFileInformationByHandle");
internal static ж<LazyProc> procSetFilePointer = modkernel32.NewProc("SetFilePointer");
internal static ж<LazyProc> procSetFileTime = modkernel32.NewProc("SetFileTime");
internal static ж<LazyProc> procSetFileValidData = modkernel32.NewProc("SetFileValidData");
internal static ж<LazyProc> procSetHandleInformation = modkernel32.NewProc("SetHandleInformation");
internal static ж<LazyProc> procSetInformationJobObject = modkernel32.NewProc("SetInformationJobObject");
internal static ж<LazyProc> procSetNamedPipeHandleState = modkernel32.NewProc("SetNamedPipeHandleState");
internal static ж<LazyProc> procSetPriorityClass = modkernel32.NewProc("SetPriorityClass");
internal static ж<LazyProc> procSetProcessPriorityBoost = modkernel32.NewProc("SetProcessPriorityBoost");
internal static ж<LazyProc> procSetProcessShutdownParameters = modkernel32.NewProc("SetProcessShutdownParameters");
internal static ж<LazyProc> procSetProcessWorkingSetSizeEx = modkernel32.NewProc("SetProcessWorkingSetSizeEx");
internal static ж<LazyProc> procSetStdHandle = modkernel32.NewProc("SetStdHandle");
internal static ж<LazyProc> procSetVolumeLabelW = modkernel32.NewProc("SetVolumeLabelW");
internal static ж<LazyProc> procSetVolumeMountPointW = modkernel32.NewProc("SetVolumeMountPointW");
internal static ж<LazyProc> procSetupComm = modkernel32.NewProc("SetupComm");
internal static ж<LazyProc> procSizeofResource = modkernel32.NewProc("SizeofResource");
internal static ж<LazyProc> procSleepEx = modkernel32.NewProc("SleepEx");
internal static ж<LazyProc> procTerminateJobObject = modkernel32.NewProc("TerminateJobObject");
internal static ж<LazyProc> procTerminateProcess = modkernel32.NewProc("TerminateProcess");
internal static ж<LazyProc> procThread32First = modkernel32.NewProc("Thread32First");
internal static ж<LazyProc> procThread32Next = modkernel32.NewProc("Thread32Next");
internal static ж<LazyProc> procUnlockFileEx = modkernel32.NewProc("UnlockFileEx");
internal static ж<LazyProc> procUnmapViewOfFile = modkernel32.NewProc("UnmapViewOfFile");
internal static ж<LazyProc> procUpdateProcThreadAttribute = modkernel32.NewProc("UpdateProcThreadAttribute");
internal static ж<LazyProc> procVirtualAlloc = modkernel32.NewProc("VirtualAlloc");
internal static ж<LazyProc> procVirtualFree = modkernel32.NewProc("VirtualFree");
internal static ж<LazyProc> procVirtualLock = modkernel32.NewProc("VirtualLock");
internal static ж<LazyProc> procVirtualProtect = modkernel32.NewProc("VirtualProtect");
internal static ж<LazyProc> procVirtualProtectEx = modkernel32.NewProc("VirtualProtectEx");
internal static ж<LazyProc> procVirtualQuery = modkernel32.NewProc("VirtualQuery");
internal static ж<LazyProc> procVirtualQueryEx = modkernel32.NewProc("VirtualQueryEx");
internal static ж<LazyProc> procVirtualUnlock = modkernel32.NewProc("VirtualUnlock");
internal static ж<LazyProc> procWTSGetActiveConsoleSessionId = modkernel32.NewProc("WTSGetActiveConsoleSessionId");
internal static ж<LazyProc> procWaitCommEvent = modkernel32.NewProc("WaitCommEvent");
internal static ж<LazyProc> procWaitForMultipleObjects = modkernel32.NewProc("WaitForMultipleObjects");
internal static ж<LazyProc> procWaitForSingleObject = modkernel32.NewProc("WaitForSingleObject");
internal static ж<LazyProc> procWriteConsoleW = modkernel32.NewProc("WriteConsoleW");
internal static ж<LazyProc> procWriteFile = modkernel32.NewProc("WriteFile");
internal static ж<LazyProc> procWriteProcessMemory = modkernel32.NewProc("WriteProcessMemory");
internal static ж<LazyProc> procAcceptEx = modmswsock.NewProc("AcceptEx");
internal static ж<LazyProc> procGetAcceptExSockaddrs = modmswsock.NewProc("GetAcceptExSockaddrs");
internal static ж<LazyProc> procTransmitFile = modmswsock.NewProc("TransmitFile");
internal static ж<LazyProc> procNetApiBufferFree = modnetapi32.NewProc("NetApiBufferFree");
internal static ж<LazyProc> procNetGetJoinInformation = modnetapi32.NewProc("NetGetJoinInformation");
internal static ж<LazyProc> procNetUserEnum = modnetapi32.NewProc("NetUserEnum");
internal static ж<LazyProc> procNetUserGetInfo = modnetapi32.NewProc("NetUserGetInfo");
internal static ж<LazyProc> procNtCreateFile = modntdll.NewProc("NtCreateFile");
internal static ж<LazyProc> procNtCreateNamedPipeFile = modntdll.NewProc("NtCreateNamedPipeFile");
internal static ж<LazyProc> procNtQueryEaFile = modntdll.NewProc("NtQueryEaFile");
internal static ж<LazyProc> procNtQueryInformationFile = modntdll.NewProc("NtQueryInformationFile");
internal static ж<LazyProc> procNtQueryInformationProcess = modntdll.NewProc("NtQueryInformationProcess");
internal static ж<LazyProc> procNtQuerySystemInformation = modntdll.NewProc("NtQuerySystemInformation");
internal static ж<LazyProc> procNtSetEaFile = modntdll.NewProc("NtSetEaFile");
internal static ж<LazyProc> procNtSetInformationFile = modntdll.NewProc("NtSetInformationFile");
internal static ж<LazyProc> procNtSetInformationProcess = modntdll.NewProc("NtSetInformationProcess");
internal static ж<LazyProc> procNtSetSystemInformation = modntdll.NewProc("NtSetSystemInformation");
internal static ж<LazyProc> procRtlAddFunctionTable = modntdll.NewProc("RtlAddFunctionTable");
internal static ж<LazyProc> procRtlDefaultNpAcl = modntdll.NewProc("RtlDefaultNpAcl");
internal static ж<LazyProc> procRtlDeleteFunctionTable = modntdll.NewProc("RtlDeleteFunctionTable");
internal static ж<LazyProc> procRtlDosPathNameToNtPathName_U_WithStatus = modntdll.NewProc("RtlDosPathNameToNtPathName_U_WithStatus");
internal static ж<LazyProc> procRtlDosPathNameToRelativeNtPathName_U_WithStatus = modntdll.NewProc("RtlDosPathNameToRelativeNtPathName_U_WithStatus");
internal static ж<LazyProc> procRtlGetCurrentPeb = modntdll.NewProc("RtlGetCurrentPeb");
internal static ж<LazyProc> procRtlGetNtVersionNumbers = modntdll.NewProc("RtlGetNtVersionNumbers");
internal static ж<LazyProc> procRtlGetVersion = modntdll.NewProc("RtlGetVersion");
internal static ж<LazyProc> procRtlInitString = modntdll.NewProc("RtlInitString");
internal static ж<LazyProc> procRtlInitUnicodeString = modntdll.NewProc("RtlInitUnicodeString");
internal static ж<LazyProc> procRtlNtStatusToDosErrorNoTeb = modntdll.NewProc("RtlNtStatusToDosErrorNoTeb");
internal static ж<LazyProc> procCLSIDFromString = modole32.NewProc("CLSIDFromString");
internal static ж<LazyProc> procCoCreateGuid = modole32.NewProc("CoCreateGuid");
internal static ж<LazyProc> procCoGetObject = modole32.NewProc("CoGetObject");
internal static ж<LazyProc> procCoInitializeEx = modole32.NewProc("CoInitializeEx");
internal static ж<LazyProc> procCoTaskMemFree = modole32.NewProc("CoTaskMemFree");
internal static ж<LazyProc> procCoUninitialize = modole32.NewProc("CoUninitialize");
internal static ж<LazyProc> procStringFromGUID2 = modole32.NewProc("StringFromGUID2");
internal static ж<LazyProc> procEnumProcessModules = modpsapi.NewProc("EnumProcessModules");
internal static ж<LazyProc> procEnumProcessModulesEx = modpsapi.NewProc("EnumProcessModulesEx");
internal static ж<LazyProc> procEnumProcesses = modpsapi.NewProc("EnumProcesses");
internal static ж<LazyProc> procGetModuleBaseNameW = modpsapi.NewProc("GetModuleBaseNameW");
internal static ж<LazyProc> procGetModuleFileNameExW = modpsapi.NewProc("GetModuleFileNameExW");
internal static ж<LazyProc> procGetModuleInformation = modpsapi.NewProc("GetModuleInformation");
internal static ж<LazyProc> procQueryWorkingSetEx = modpsapi.NewProc("QueryWorkingSetEx");
internal static ж<LazyProc> procSubscribeServiceChangeNotifications = modsechost.NewProc("SubscribeServiceChangeNotifications");
internal static ж<LazyProc> procUnsubscribeServiceChangeNotifications = modsechost.NewProc("UnsubscribeServiceChangeNotifications");
internal static ж<LazyProc> procGetUserNameExW = modsecur32.NewProc("GetUserNameExW");
internal static ж<LazyProc> procTranslateNameW = modsecur32.NewProc("TranslateNameW");
internal static ж<LazyProc> procSetupDiBuildDriverInfoList = modsetupapi.NewProc("SetupDiBuildDriverInfoList");
internal static ж<LazyProc> procSetupDiCallClassInstaller = modsetupapi.NewProc("SetupDiCallClassInstaller");
internal static ж<LazyProc> procSetupDiCancelDriverInfoSearch = modsetupapi.NewProc("SetupDiCancelDriverInfoSearch");
internal static ж<LazyProc> procSetupDiClassGuidsFromNameExW = modsetupapi.NewProc("SetupDiClassGuidsFromNameExW");
internal static ж<LazyProc> procSetupDiClassNameFromGuidExW = modsetupapi.NewProc("SetupDiClassNameFromGuidExW");
internal static ж<LazyProc> procSetupDiCreateDeviceInfoListExW = modsetupapi.NewProc("SetupDiCreateDeviceInfoListExW");
internal static ж<LazyProc> procSetupDiCreateDeviceInfoW = modsetupapi.NewProc("SetupDiCreateDeviceInfoW");
internal static ж<LazyProc> procSetupDiDestroyDeviceInfoList = modsetupapi.NewProc("SetupDiDestroyDeviceInfoList");
internal static ж<LazyProc> procSetupDiDestroyDriverInfoList = modsetupapi.NewProc("SetupDiDestroyDriverInfoList");
internal static ж<LazyProc> procSetupDiEnumDeviceInfo = modsetupapi.NewProc("SetupDiEnumDeviceInfo");
internal static ж<LazyProc> procSetupDiEnumDriverInfoW = modsetupapi.NewProc("SetupDiEnumDriverInfoW");
internal static ж<LazyProc> procSetupDiGetClassDevsExW = modsetupapi.NewProc("SetupDiGetClassDevsExW");
internal static ж<LazyProc> procSetupDiGetClassInstallParamsW = modsetupapi.NewProc("SetupDiGetClassInstallParamsW");
internal static ж<LazyProc> procSetupDiGetDeviceInfoListDetailW = modsetupapi.NewProc("SetupDiGetDeviceInfoListDetailW");
internal static ж<LazyProc> procSetupDiGetDeviceInstallParamsW = modsetupapi.NewProc("SetupDiGetDeviceInstallParamsW");
internal static ж<LazyProc> procSetupDiGetDeviceInstanceIdW = modsetupapi.NewProc("SetupDiGetDeviceInstanceIdW");
internal static ж<LazyProc> procSetupDiGetDevicePropertyW = modsetupapi.NewProc("SetupDiGetDevicePropertyW");
internal static ж<LazyProc> procSetupDiGetDeviceRegistryPropertyW = modsetupapi.NewProc("SetupDiGetDeviceRegistryPropertyW");
internal static ж<LazyProc> procSetupDiGetDriverInfoDetailW = modsetupapi.NewProc("SetupDiGetDriverInfoDetailW");
internal static ж<LazyProc> procSetupDiGetSelectedDevice = modsetupapi.NewProc("SetupDiGetSelectedDevice");
internal static ж<LazyProc> procSetupDiGetSelectedDriverW = modsetupapi.NewProc("SetupDiGetSelectedDriverW");
internal static ж<LazyProc> procSetupDiOpenDevRegKey = modsetupapi.NewProc("SetupDiOpenDevRegKey");
internal static ж<LazyProc> procSetupDiSetClassInstallParamsW = modsetupapi.NewProc("SetupDiSetClassInstallParamsW");
internal static ж<LazyProc> procSetupDiSetDeviceInstallParamsW = modsetupapi.NewProc("SetupDiSetDeviceInstallParamsW");
internal static ж<LazyProc> procSetupDiSetDeviceRegistryPropertyW = modsetupapi.NewProc("SetupDiSetDeviceRegistryPropertyW");
internal static ж<LazyProc> procSetupDiSetSelectedDevice = modsetupapi.NewProc("SetupDiSetSelectedDevice");
internal static ж<LazyProc> procSetupDiSetSelectedDriverW = modsetupapi.NewProc("SetupDiSetSelectedDriverW");
internal static ж<LazyProc> procSetupUninstallOEMInfW = modsetupapi.NewProc("SetupUninstallOEMInfW");
internal static ж<LazyProc> procCommandLineToArgvW = modshell32.NewProc("CommandLineToArgvW");
internal static ж<LazyProc> procSHGetKnownFolderPath = modshell32.NewProc("SHGetKnownFolderPath");
internal static ж<LazyProc> procShellExecuteW = modshell32.NewProc("ShellExecuteW");
internal static ж<LazyProc> procEnumChildWindows = moduser32.NewProc("EnumChildWindows");
internal static ж<LazyProc> procEnumWindows = moduser32.NewProc("EnumWindows");
internal static ж<LazyProc> procExitWindowsEx = moduser32.NewProc("ExitWindowsEx");
internal static ж<LazyProc> procGetClassNameW = moduser32.NewProc("GetClassNameW");
internal static ж<LazyProc> procGetDesktopWindow = moduser32.NewProc("GetDesktopWindow");
internal static ж<LazyProc> procGetForegroundWindow = moduser32.NewProc("GetForegroundWindow");
internal static ж<LazyProc> procGetGUIThreadInfo = moduser32.NewProc("GetGUIThreadInfo");
internal static ж<LazyProc> procGetKeyboardLayout = moduser32.NewProc("GetKeyboardLayout");
internal static ж<LazyProc> procGetShellWindow = moduser32.NewProc("GetShellWindow");
internal static ж<LazyProc> procGetWindowThreadProcessId = moduser32.NewProc("GetWindowThreadProcessId");
internal static ж<LazyProc> procIsWindow = moduser32.NewProc("IsWindow");
internal static ж<LazyProc> procIsWindowUnicode = moduser32.NewProc("IsWindowUnicode");
internal static ж<LazyProc> procIsWindowVisible = moduser32.NewProc("IsWindowVisible");
internal static ж<LazyProc> procLoadKeyboardLayoutW = moduser32.NewProc("LoadKeyboardLayoutW");
internal static ж<LazyProc> procMessageBoxW = moduser32.NewProc("MessageBoxW");
internal static ж<LazyProc> procToUnicodeEx = moduser32.NewProc("ToUnicodeEx");
internal static ж<LazyProc> procUnloadKeyboardLayout = moduser32.NewProc("UnloadKeyboardLayout");
internal static ж<LazyProc> procCreateEnvironmentBlock = moduserenv.NewProc("CreateEnvironmentBlock");
internal static ж<LazyProc> procDestroyEnvironmentBlock = moduserenv.NewProc("DestroyEnvironmentBlock");
internal static ж<LazyProc> procGetUserProfileDirectoryW = moduserenv.NewProc("GetUserProfileDirectoryW");
internal static ж<LazyProc> procGetFileVersionInfoSizeW = modversion.NewProc("GetFileVersionInfoSizeW");
internal static ж<LazyProc> procGetFileVersionInfoW = modversion.NewProc("GetFileVersionInfoW");
internal static ж<LazyProc> procVerQueryValueW = modversion.NewProc("VerQueryValueW");
internal static ж<LazyProc> proctimeBeginPeriod = modwinmm.NewProc("timeBeginPeriod");
internal static ж<LazyProc> proctimeEndPeriod = modwinmm.NewProc("timeEndPeriod");
internal static ж<LazyProc> procWinVerifyTrustEx = modwintrust.NewProc("WinVerifyTrustEx");
internal static ж<LazyProc> procFreeAddrInfoW = modws2_32.NewProc("FreeAddrInfoW");
internal static ж<LazyProc> procGetAddrInfoW = modws2_32.NewProc("GetAddrInfoW");
internal static ж<LazyProc> procWSACleanup = modws2_32.NewProc("WSACleanup");
internal static ж<LazyProc> procWSADuplicateSocketW = modws2_32.NewProc("WSADuplicateSocketW");
internal static ж<LazyProc> procWSAEnumProtocolsW = modws2_32.NewProc("WSAEnumProtocolsW");
internal static ж<LazyProc> procWSAGetOverlappedResult = modws2_32.NewProc("WSAGetOverlappedResult");
internal static ж<LazyProc> procWSAIoctl = modws2_32.NewProc("WSAIoctl");
internal static ж<LazyProc> procWSALookupServiceBeginW = modws2_32.NewProc("WSALookupServiceBeginW");
internal static ж<LazyProc> procWSALookupServiceEnd = modws2_32.NewProc("WSALookupServiceEnd");
internal static ж<LazyProc> procWSALookupServiceNextW = modws2_32.NewProc("WSALookupServiceNextW");
internal static ж<LazyProc> procWSARecv = modws2_32.NewProc("WSARecv");
internal static ж<LazyProc> procWSARecvFrom = modws2_32.NewProc("WSARecvFrom");
internal static ж<LazyProc> procWSASend = modws2_32.NewProc("WSASend");
internal static ж<LazyProc> procWSASendTo = modws2_32.NewProc("WSASendTo");
internal static ж<LazyProc> procWSASocketW = modws2_32.NewProc("WSASocketW");
internal static ж<LazyProc> procWSAStartup = modws2_32.NewProc("WSAStartup");
internal static ж<LazyProc> procbind = modws2_32.NewProc("bind");
internal static ж<LazyProc> procclosesocket = modws2_32.NewProc("closesocket");
internal static ж<LazyProc> procconnect = modws2_32.NewProc("connect");
internal static ж<LazyProc> procgethostbyname = modws2_32.NewProc("gethostbyname");
internal static ж<LazyProc> procgetpeername = modws2_32.NewProc("getpeername");
internal static ж<LazyProc> procgetprotobyname = modws2_32.NewProc("getprotobyname");
internal static ж<LazyProc> procgetservbyname = modws2_32.NewProc("getservbyname");
internal static ж<LazyProc> procgetsockname = modws2_32.NewProc("getsockname");
internal static ж<LazyProc> procgetsockopt = modws2_32.NewProc("getsockopt");
internal static ж<LazyProc> proclisten = modws2_32.NewProc("listen");
internal static ж<LazyProc> procntohs = modws2_32.NewProc("ntohs");
internal static ж<LazyProc> procrecvfrom = modws2_32.NewProc("recvfrom");
internal static ж<LazyProc> procsendto = modws2_32.NewProc("sendto");
internal static ж<LazyProc> procsetsockopt = modws2_32.NewProc("setsockopt");
internal static ж<LazyProc> procshutdown = modws2_32.NewProc("shutdown");
internal static ж<LazyProc> procsocket = modws2_32.NewProc("socket");
internal static ж<LazyProc> procWTSEnumerateSessionsW = modwtsapi32.NewProc("WTSEnumerateSessionsW");
internal static ж<LazyProc> procWTSFreeMemory = modwtsapi32.NewProc("WTSFreeMemory");
internal static ж<LazyProc> procWTSQueryUserToken = modwtsapi32.NewProc("WTSQueryUserToken");

internal static CONFIGRET /*ret*/ cm_Get_DevNode_Status(ж<uint32> status, ж<uint32> problemNumber, DEVINST devInst, uint32 flags) {
    CONFIGRET ret = default;

    var ᴋ0 = status;
    var ᴋ1 = problemNumber;
        var (r0, _, _) = syscall.SyscallN(procCM_Get_DevNode_Status.Addr(), (uintptr)ᴋ0, (uintptr)ᴋ1, (uintptr)devInst, (uintptr)flags);
    System.GC.KeepAlive(ᴋ0);
    System.GC.KeepAlive(ᴋ1);
    ret = (CONFIGRET)(uint32)r0;
    return ret;
}

internal static CONFIGRET /*ret*/ cm_Get_Device_Interface_List(ж<GUID> interfaceClass, ж<uint16> deviceID, ж<uint16> buffer, uint32 bufferLen, uint32 flags) {
    CONFIGRET ret = default;

    var ᴋ2 = interfaceClass;
    var ᴋ3 = deviceID;
    var ᴋ4 = buffer;
        var (r0, _, _) = syscall.SyscallN(procCM_Get_Device_Interface_ListW.Addr(), (uintptr)ᴋ2, (uintptr)ᴋ3, (uintptr)ᴋ4, (uintptr)bufferLen, (uintptr)flags);
    System.GC.KeepAlive(ᴋ2);
    System.GC.KeepAlive(ᴋ3);
    System.GC.KeepAlive(ᴋ4);
    ret = (CONFIGRET)(uint32)r0;
    return ret;
}

internal static CONFIGRET /*ret*/ cm_Get_Device_Interface_List_Size(ж<uint32> len, ж<GUID> interfaceClass, ж<uint16> deviceID, uint32 flags) {
    CONFIGRET ret = default;

    var ᴋ5 = len;
    var ᴋ6 = interfaceClass;
    var ᴋ7 = deviceID;
        var (r0, _, _) = syscall.SyscallN(procCM_Get_Device_Interface_List_SizeW.Addr(), (uintptr)ᴋ5, (uintptr)ᴋ6, (uintptr)ᴋ7, (uintptr)flags);
    System.GC.KeepAlive(ᴋ5);
    System.GC.KeepAlive(ᴋ6);
    System.GC.KeepAlive(ᴋ7);
    ret = (CONFIGRET)(uint32)r0;
    return ret;
}

internal static Errno /*ret*/ cm_MapCrToWin32Err(CONFIGRET configRet, Errno defaultWin32Error) {
    Errno ret = default;

    var (r0, _, _) = syscall.SyscallN(procCM_MapCrToWin32Err.Addr(), (uintptr)configRet, (uintptr)defaultWin32Error);
    ret = (Errno)r0;
    return ret;
}

public static error /*err*/ AdjustTokenGroups(Token token, bool resetToDefault, ж<Tokengroups> newstate, uint32 buflen, ж<Tokengroups> prevstate, ж<uint32> returnlen) {
    error err = default;

    uint32 _p0 = default;
    if (resetToDefault) {
        _p0 = 1;
    }
    var ᴋ8 = newstate;
    var ᴋ9 = prevstate;
    var ᴋ10 = returnlen;
        var (r1, _, e1) = syscall.SyscallN(procAdjustTokenGroups.Addr(), (uintptr)token, (uintptr)_p0, (uintptr)ᴋ8, (uintptr)buflen, (uintptr)ᴋ9, (uintptr)ᴋ10);
    System.GC.KeepAlive(ᴋ8);
    System.GC.KeepAlive(ᴋ9);
    System.GC.KeepAlive(ᴋ10);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ AdjustTokenPrivileges(Token token, bool disableAllPrivileges, ж<Tokenprivileges> newstate, uint32 buflen, ж<Tokenprivileges> prevstate, ж<uint32> returnlen) {
    error err = default;

    uint32 _p0 = default;
    if (disableAllPrivileges) {
        _p0 = 1;
    }
    var ᴋ11 = newstate;
    var ᴋ12 = prevstate;
    var ᴋ13 = returnlen;
        var (r1, _, e1) = syscall.SyscallN(procAdjustTokenPrivileges.Addr(), (uintptr)token, (uintptr)_p0, (uintptr)ᴋ11, (uintptr)buflen, (uintptr)ᴋ12, (uintptr)ᴋ13);
    System.GC.KeepAlive(ᴋ11);
    System.GC.KeepAlive(ᴋ12);
    System.GC.KeepAlive(ᴋ13);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ AllocateAndInitializeSid(ж<SidIdentifierAuthority> identAuth, byte subAuth, uint32 subAuth0, uint32 subAuth1, uint32 subAuth2, uint32 subAuth3, uint32 subAuth4, uint32 subAuth5, uint32 subAuth6, uint32 subAuth7, ж<ж<SID>> sid) {
    error err = default;

    var ᴋ14 = identAuth;
    var ᴋ15 = sid;
        var (r1, _, e1) = syscall.SyscallN(procAllocateAndInitializeSid.Addr(), (uintptr)ᴋ14, (uintptr)subAuth, (uintptr)subAuth0, (uintptr)subAuth1, (uintptr)subAuth2, (uintptr)subAuth3, (uintptr)subAuth4, (uintptr)subAuth5, (uintptr)subAuth6, (uintptr)subAuth7, (uintptr)ᴋ15);
    System.GC.KeepAlive(ᴋ14);
    System.GC.KeepAlive(ᴋ15);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*ret*/ buildSecurityDescriptor(ж<TRUSTEE> owner, ж<TRUSTEE> group, uint32 countAccessEntries, ж<EXPLICIT_ACCESS> accessEntries, uint32 countAuditEntries, ж<EXPLICIT_ACCESS> auditEntries, ж<SECURITY_DESCRIPTOR> oldSecurityDescriptor, ж<uint32> sizeNewSecurityDescriptor, ж<ж<SECURITY_DESCRIPTOR>> newSecurityDescriptor) {
    error ret = default;

    var ᴋ16 = owner;
    var ᴋ17 = group;
    var ᴋ18 = accessEntries;
    var ᴋ19 = auditEntries;
    var ᴋ20 = oldSecurityDescriptor;
    var ᴋ21 = sizeNewSecurityDescriptor;
    var ᴋ22 = newSecurityDescriptor;
        var (r0, _, _) = syscall.SyscallN(procBuildSecurityDescriptorW.Addr(), (uintptr)ᴋ16, (uintptr)ᴋ17, (uintptr)countAccessEntries, (uintptr)ᴋ18, (uintptr)countAuditEntries, (uintptr)ᴋ19, (uintptr)ᴋ20, (uintptr)ᴋ21, (uintptr)ᴋ22);
    System.GC.KeepAlive(ᴋ16);
    System.GC.KeepAlive(ᴋ17);
    System.GC.KeepAlive(ᴋ18);
    System.GC.KeepAlive(ᴋ19);
    System.GC.KeepAlive(ᴋ20);
    System.GC.KeepAlive(ᴋ21);
    System.GC.KeepAlive(ᴋ22);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*err*/ ChangeServiceConfig2(Handle service, uint32 infoLevel, ж<byte> info) {
    error err = default;

    var ᴋ23 = info;
        var (r1, _, e1) = syscall.SyscallN(procChangeServiceConfig2W.Addr(), (uintptr)service, (uintptr)infoLevel, (uintptr)ᴋ23);
    System.GC.KeepAlive(ᴋ23);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ChangeServiceConfig(Handle service, uint32 serviceType, uint32 startType, uint32 errorControl, ж<uint16> binaryPathName, ж<uint16> loadOrderGroup, ж<uint32> tagId, ж<uint16> dependencies, ж<uint16> serviceStartName, ж<uint16> password, ж<uint16> displayName) {
    error err = default;

    var ᴋ24 = binaryPathName;
    var ᴋ25 = loadOrderGroup;
    var ᴋ26 = tagId;
    var ᴋ27 = dependencies;
    var ᴋ28 = serviceStartName;
    var ᴋ29 = password;
    var ᴋ30 = displayName;
        var (r1, _, e1) = syscall.SyscallN(procChangeServiceConfigW.Addr(), (uintptr)service, (uintptr)serviceType, (uintptr)startType, (uintptr)errorControl, (uintptr)ᴋ24, (uintptr)ᴋ25, (uintptr)ᴋ26, (uintptr)ᴋ27, (uintptr)ᴋ28, (uintptr)ᴋ29, (uintptr)ᴋ30);
    System.GC.KeepAlive(ᴋ24);
    System.GC.KeepAlive(ᴋ25);
    System.GC.KeepAlive(ᴋ26);
    System.GC.KeepAlive(ᴋ27);
    System.GC.KeepAlive(ᴋ28);
    System.GC.KeepAlive(ᴋ29);
    System.GC.KeepAlive(ᴋ30);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ checkTokenMembership(Token tokenHandle, ж<SID> sidToCheck, ж<int32> isMember) {
    error err = default;

    var ᴋ31 = sidToCheck;
    var ᴋ32 = isMember;
        var (r1, _, e1) = syscall.SyscallN(procCheckTokenMembership.Addr(), (uintptr)tokenHandle, (uintptr)ᴋ31, (uintptr)ᴋ32);
    System.GC.KeepAlive(ᴋ31);
    System.GC.KeepAlive(ᴋ32);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CloseServiceHandle(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procCloseServiceHandle.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ControlService(Handle service, uint32 control, ж<SERVICE_STATUS> status) {
    error err = default;

    var ᴋ33 = status;
        var (r1, _, e1) = syscall.SyscallN(procControlService.Addr(), (uintptr)service, (uintptr)control, (uintptr)ᴋ33);
    System.GC.KeepAlive(ᴋ33);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ convertSecurityDescriptorToStringSecurityDescriptor(ж<SECURITY_DESCRIPTOR> sd, uint32 revision, SECURITY_INFORMATION securityInformation, ж<ж<uint16>> str, ж<uint32> strLen) {
    error err = default;

    var ᴋ34 = sd;
    var ᴋ35 = str;
    var ᴋ36 = strLen;
        var (r1, _, e1) = syscall.SyscallN(procConvertSecurityDescriptorToStringSecurityDescriptorW.Addr(), (uintptr)ᴋ34, (uintptr)revision, (uintptr)securityInformation, (uintptr)ᴋ35, (uintptr)ᴋ36);
    System.GC.KeepAlive(ᴋ34);
    System.GC.KeepAlive(ᴋ35);
    System.GC.KeepAlive(ᴋ36);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ConvertSidToStringSid(ж<SID> sid, ж<ж<uint16>> stringSid) {
    error err = default;

    var ᴋ37 = sid;
    var ᴋ38 = stringSid;
        var (r1, _, e1) = syscall.SyscallN(procConvertSidToStringSidW.Addr(), (uintptr)ᴋ37, (uintptr)ᴋ38);
    System.GC.KeepAlive(ᴋ37);
    System.GC.KeepAlive(ᴋ38);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ convertStringSecurityDescriptorToSecurityDescriptor(@string str, uint32 revision, ж<ж<SECURITY_DESCRIPTOR>> sd, ж<uint32> size) {
    error err = default;

    ж<uint16> _p0 = default;
    (_p0, err) = syscall.UTF16PtrFromString(str);
    if (err != nil) {
        return err;
    }
    return _convertStringSecurityDescriptorToSecurityDescriptor(_p0, revision, sd, size);
}

internal static error /*err*/ _convertStringSecurityDescriptorToSecurityDescriptor(ж<uint16> str, uint32 revision, ж<ж<SECURITY_DESCRIPTOR>> sd, ж<uint32> size) {
    error err = default;

    var ᴋ39 = str;
    var ᴋ40 = sd;
    var ᴋ41 = size;
        var (r1, _, e1) = syscall.SyscallN(procConvertStringSecurityDescriptorToSecurityDescriptorW.Addr(), (uintptr)ᴋ39, (uintptr)revision, (uintptr)ᴋ40, (uintptr)ᴋ41);
    System.GC.KeepAlive(ᴋ39);
    System.GC.KeepAlive(ᴋ40);
    System.GC.KeepAlive(ᴋ41);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ConvertStringSidToSid(ж<uint16> stringSid, ж<ж<SID>> sid) {
    error err = default;

    var ᴋ42 = stringSid;
    var ᴋ43 = sid;
        var (r1, _, e1) = syscall.SyscallN(procConvertStringSidToSidW.Addr(), (uintptr)ᴋ42, (uintptr)ᴋ43);
    System.GC.KeepAlive(ᴋ42);
    System.GC.KeepAlive(ᴋ43);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CopySid(uint32 destSidLen, ж<SID> destSid, ж<SID> srcSid) {
    error err = default;

    var ᴋ44 = destSid;
    var ᴋ45 = srcSid;
        var (r1, _, e1) = syscall.SyscallN(procCopySid.Addr(), (uintptr)destSidLen, (uintptr)ᴋ44, (uintptr)ᴋ45);
    System.GC.KeepAlive(ᴋ44);
    System.GC.KeepAlive(ᴋ45);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CreateProcessAsUser(Token token, ж<uint16> appName, ж<uint16> commandLine, ж<SecurityAttributes> procSecurity, ж<SecurityAttributes> threadSecurity, bool inheritHandles, uint32 creationFlags, ж<uint16> env, ж<uint16> currentDir, ж<StartupInfo> startupInfo, ж<ProcessInformation> outProcInfo) {
    error err = default;

    uint32 _p0 = default;
    if (inheritHandles) {
        _p0 = 1;
    }
    var ᴋ46 = appName;
    var ᴋ47 = commandLine;
    var ᴋ48 = procSecurity;
    var ᴋ49 = threadSecurity;
    var ᴋ50 = env;
    var ᴋ51 = currentDir;
    var ᴋ52 = startupInfo;
    var ᴋ53 = outProcInfo;
        var (r1, _, e1) = syscall.SyscallN(procCreateProcessAsUserW.Addr(), (uintptr)token, (uintptr)ᴋ46, (uintptr)ᴋ47, (uintptr)ᴋ48, (uintptr)ᴋ49, (uintptr)_p0, (uintptr)creationFlags, (uintptr)ᴋ50, (uintptr)ᴋ51, (uintptr)ᴋ52, (uintptr)ᴋ53);
    System.GC.KeepAlive(ᴋ46);
    System.GC.KeepAlive(ᴋ47);
    System.GC.KeepAlive(ᴋ48);
    System.GC.KeepAlive(ᴋ49);
    System.GC.KeepAlive(ᴋ50);
    System.GC.KeepAlive(ᴋ51);
    System.GC.KeepAlive(ᴋ52);
    System.GC.KeepAlive(ᴋ53);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle handle, error err) CreateService(Handle mgr, ж<uint16> serviceName, ж<uint16> displayName, uint32 access, uint32 srvType, uint32 startType, uint32 errCtl, ж<uint16> pathName, ж<uint16> loadOrderGroup, ж<uint32> tagId, ж<uint16> dependencies, ж<uint16> serviceStartName, ж<uint16> password) {
    Handle handle = default;
    error err = default;

    var ᴋ54 = serviceName;
    var ᴋ55 = displayName;
    var ᴋ56 = pathName;
    var ᴋ57 = loadOrderGroup;
    var ᴋ58 = tagId;
    var ᴋ59 = dependencies;
    var ᴋ60 = serviceStartName;
    var ᴋ61 = password;
        var (r0, _, e1) = syscall.SyscallN(procCreateServiceW.Addr(), (uintptr)mgr, (uintptr)ᴋ54, (uintptr)ᴋ55, (uintptr)access, (uintptr)srvType, (uintptr)startType, (uintptr)errCtl, (uintptr)ᴋ56, (uintptr)ᴋ57, (uintptr)ᴋ58, (uintptr)ᴋ59, (uintptr)ᴋ60, (uintptr)ᴋ61);
    System.GC.KeepAlive(ᴋ54);
    System.GC.KeepAlive(ᴋ55);
    System.GC.KeepAlive(ᴋ56);
    System.GC.KeepAlive(ᴋ57);
    System.GC.KeepAlive(ᴋ58);
    System.GC.KeepAlive(ᴋ59);
    System.GC.KeepAlive(ᴋ60);
    System.GC.KeepAlive(ᴋ61);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

internal static error /*err*/ createWellKnownSid(WELL_KNOWN_SID_TYPE sidType, ж<SID> domainSid, ж<SID> sid, ж<uint32> sizeSid) {
    error err = default;

    var ᴋ62 = domainSid;
    var ᴋ63 = sid;
    var ᴋ64 = sizeSid;
        var (r1, _, e1) = syscall.SyscallN(procCreateWellKnownSid.Addr(), (uintptr)sidType, (uintptr)ᴋ62, (uintptr)ᴋ63, (uintptr)ᴋ64);
    System.GC.KeepAlive(ᴋ62);
    System.GC.KeepAlive(ᴋ63);
    System.GC.KeepAlive(ᴋ64);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CryptAcquireContext(ж<Handle> provhandle, ж<uint16> container, ж<uint16> provider, uint32 provtype, uint32 flags) {
    error err = default;

    var ᴋ65 = provhandle;
    var ᴋ66 = container;
    var ᴋ67 = provider;
        var (r1, _, e1) = syscall.SyscallN(procCryptAcquireContextW.Addr(), (uintptr)ᴋ65, (uintptr)ᴋ66, (uintptr)ᴋ67, (uintptr)provtype, (uintptr)flags);
    System.GC.KeepAlive(ᴋ65);
    System.GC.KeepAlive(ᴋ66);
    System.GC.KeepAlive(ᴋ67);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CryptGenRandom(Handle provhandle, uint32 buflen, ж<byte> buf) {
    error err = default;

    var ᴋ68 = buf;
        var (r1, _, e1) = syscall.SyscallN(procCryptGenRandom.Addr(), (uintptr)provhandle, (uintptr)buflen, (uintptr)ᴋ68);
    System.GC.KeepAlive(ᴋ68);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CryptReleaseContext(Handle provhandle, uint32 flags) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procCryptReleaseContext.Addr(), (uintptr)provhandle, (uintptr)flags);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ DeleteService(Handle service) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procDeleteService.Addr(), (uintptr)service);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ DeregisterEventSource(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procDeregisterEventSource.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ DuplicateTokenEx(Token existingToken, uint32 desiredAccess, ж<SecurityAttributes> tokenAttributes, uint32 impersonationLevel, uint32 tokenType, ж<Token> newToken) {
    error err = default;

    var ᴋ69 = tokenAttributes;
    var ᴋ70 = newToken;
        var (r1, _, e1) = syscall.SyscallN(procDuplicateTokenEx.Addr(), (uintptr)existingToken, (uintptr)desiredAccess, (uintptr)ᴋ69, (uintptr)impersonationLevel, (uintptr)tokenType, (uintptr)ᴋ70);
    System.GC.KeepAlive(ᴋ69);
    System.GC.KeepAlive(ᴋ70);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ EnumDependentServices(Handle service, uint32 activityState, ж<ENUM_SERVICE_STATUS> services, uint32 buffSize, ж<uint32> bytesNeeded, ж<uint32> servicesReturned) {
    error err = default;

    var ᴋ71 = services;
    var ᴋ72 = bytesNeeded;
    var ᴋ73 = servicesReturned;
        var (r1, _, e1) = syscall.SyscallN(procEnumDependentServicesW.Addr(), (uintptr)service, (uintptr)activityState, (uintptr)ᴋ71, (uintptr)buffSize, (uintptr)ᴋ72, (uintptr)ᴋ73);
    System.GC.KeepAlive(ᴋ71);
    System.GC.KeepAlive(ᴋ72);
    System.GC.KeepAlive(ᴋ73);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ EnumServicesStatusEx(Handle mgr, uint32 infoLevel, uint32 serviceType, uint32 serviceState, ж<byte> services, uint32 bufSize, ж<uint32> bytesNeeded, ж<uint32> servicesReturned, ж<uint32> resumeHandle, ж<uint16> groupName) {
    error err = default;

    var ᴋ74 = services;
    var ᴋ75 = bytesNeeded;
    var ᴋ76 = servicesReturned;
    var ᴋ77 = resumeHandle;
    var ᴋ78 = groupName;
        var (r1, _, e1) = syscall.SyscallN(procEnumServicesStatusExW.Addr(), (uintptr)mgr, (uintptr)infoLevel, (uintptr)serviceType, (uintptr)serviceState, (uintptr)ᴋ74, (uintptr)bufSize, (uintptr)ᴋ75, (uintptr)ᴋ76, (uintptr)ᴋ77, (uintptr)ᴋ78);
    System.GC.KeepAlive(ᴋ74);
    System.GC.KeepAlive(ᴋ75);
    System.GC.KeepAlive(ᴋ76);
    System.GC.KeepAlive(ᴋ77);
    System.GC.KeepAlive(ᴋ78);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static bool /*isEqual*/ EqualSid(ж<SID> sid1, ж<SID> sid2) {
    bool isEqual = default;

    var ᴋ79 = sid1;
    var ᴋ80 = sid2;
        var (r0, _, _) = syscall.SyscallN(procEqualSid.Addr(), (uintptr)ᴋ79, (uintptr)ᴋ80);
    System.GC.KeepAlive(ᴋ79);
    System.GC.KeepAlive(ᴋ80);
    isEqual = r0 != 0;
    return isEqual;
}

public static error /*err*/ FreeSid(ж<SID> sid) {
    error err = default;

    var ᴋ81 = sid;
        var (r1, _, e1) = syscall.SyscallN(procFreeSid.Addr(), (uintptr)ᴋ81);
    System.GC.KeepAlive(ᴋ81);
    if (r1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetAce(ж<ACL> acl, uint32 aceIndex, ж<ж<ACCESS_ALLOWED_ACE>> pAce) {
    error err = default;

    var ᴋ82 = acl;
    var ᴋ83 = pAce;
        var (r1, _, e1) = syscall.SyscallN(procGetAce.Addr(), (uintptr)ᴋ82, (uintptr)aceIndex, (uintptr)ᴋ83);
    System.GC.KeepAlive(ᴋ82);
    System.GC.KeepAlive(ᴋ83);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static uint32 /*len*/ GetLengthSid(ж<SID> sid) {
    uint32 len = default;

    var ᴋ84 = sid;
        var (r0, _, _) = syscall.SyscallN(procGetLengthSid.Addr(), (uintptr)ᴋ84);
    System.GC.KeepAlive(ᴋ84);
    len = (uint32)r0;
    return len;
}

internal static error /*ret*/ getNamedSecurityInfo(@string objectName, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation, ж<ж<SID>> owner, ж<ж<SID>> group, ж<ж<ACL>> dacl, ж<ж<ACL>> sacl, ж<ж<SECURITY_DESCRIPTOR>> sd) {
    error ret = default;

    ж<uint16> _p0 = default;
    (_p0, ret) = syscall.UTF16PtrFromString(objectName);
    if (ret != nil) {
        return ret;
    }
    return _getNamedSecurityInfo(_p0, objectType, securityInformation, owner, group, dacl, sacl, sd);
}

internal static error /*ret*/ _getNamedSecurityInfo(ж<uint16> objectName, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation, ж<ж<SID>> owner, ж<ж<SID>> group, ж<ж<ACL>> dacl, ж<ж<ACL>> sacl, ж<ж<SECURITY_DESCRIPTOR>> sd) {
    error ret = default;

    var ᴋ85 = objectName;
    var ᴋ86 = owner;
    var ᴋ87 = group;
    var ᴋ88 = dacl;
    var ᴋ89 = sacl;
    var ᴋ90 = sd;
        var (r0, _, _) = syscall.SyscallN(procGetNamedSecurityInfoW.Addr(), (uintptr)ᴋ85, (uintptr)objectType, (uintptr)securityInformation, (uintptr)ᴋ86, (uintptr)ᴋ87, (uintptr)ᴋ88, (uintptr)ᴋ89, (uintptr)ᴋ90);
    System.GC.KeepAlive(ᴋ85);
    System.GC.KeepAlive(ᴋ86);
    System.GC.KeepAlive(ᴋ87);
    System.GC.KeepAlive(ᴋ88);
    System.GC.KeepAlive(ᴋ89);
    System.GC.KeepAlive(ᴋ90);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

internal static error /*err*/ getSecurityDescriptorControl(ж<SECURITY_DESCRIPTOR> sd, ж<SECURITY_DESCRIPTOR_CONTROL> control, ж<uint32> revision) {
    error err = default;

    var ᴋ91 = sd;
    var ᴋ92 = control;
    var ᴋ93 = revision;
        var (r1, _, e1) = syscall.SyscallN(procGetSecurityDescriptorControl.Addr(), (uintptr)ᴋ91, (uintptr)ᴋ92, (uintptr)ᴋ93);
    System.GC.KeepAlive(ᴋ91);
    System.GC.KeepAlive(ᴋ92);
    System.GC.KeepAlive(ᴋ93);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ getSecurityDescriptorDacl(ж<SECURITY_DESCRIPTOR> sd, ref bool daclPresent, ж<ж<ACL>> dacl, ref bool daclDefaulted) {
    error err = default;

    ref var _p0 = ref heap(new uint32(), out var Ꮡ_p0);
    if (daclPresent) {
        _p0 = 1;
    }
    ref var _p1 = ref heap(new uint32(), out var Ꮡ_p1);
    if (daclDefaulted) {
        _p1 = 1;
    }
    var ᴋ94 = sd;
    var ᴋ95 = Ꮡ_p0;
    var ᴋ96 = dacl;
    var ᴋ97 = Ꮡ_p1;
        var (r1, _, e1) = syscall.SyscallN(procGetSecurityDescriptorDacl.Addr(), (uintptr)ᴋ94, (uintptr)ᴋ95, (uintptr)ᴋ96, (uintptr)ᴋ97);
    System.GC.KeepAlive(ᴋ94);
    System.GC.KeepAlive(ᴋ95);
    System.GC.KeepAlive(ᴋ96);
    System.GC.KeepAlive(ᴋ97);
    daclPresent = _p0 != 0;
    daclDefaulted = _p1 != 0;
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ getSecurityDescriptorGroup(ж<SECURITY_DESCRIPTOR> sd, ж<ж<SID>> group, ref bool groupDefaulted) {
    error err = default;

    ref var _p0 = ref heap(new uint32(), out var Ꮡ_p0);
    if (groupDefaulted) {
        _p0 = 1;
    }
    var ᴋ98 = sd;
    var ᴋ99 = group;
    var ᴋ100 = Ꮡ_p0;
        var (r1, _, e1) = syscall.SyscallN(procGetSecurityDescriptorGroup.Addr(), (uintptr)ᴋ98, (uintptr)ᴋ99, (uintptr)ᴋ100);
    System.GC.KeepAlive(ᴋ98);
    System.GC.KeepAlive(ᴋ99);
    System.GC.KeepAlive(ᴋ100);
    groupDefaulted = _p0 != 0;
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static uint32 /*len*/ getSecurityDescriptorLength(ж<SECURITY_DESCRIPTOR> sd) {
    uint32 len = default;

    var ᴋ101 = sd;
        var (r0, _, _) = syscall.SyscallN(procGetSecurityDescriptorLength.Addr(), (uintptr)ᴋ101);
    System.GC.KeepAlive(ᴋ101);
    len = (uint32)r0;
    return len;
}

internal static error /*err*/ getSecurityDescriptorOwner(ж<SECURITY_DESCRIPTOR> sd, ж<ж<SID>> owner, ref bool ownerDefaulted) {
    error err = default;

    ref var _p0 = ref heap(new uint32(), out var Ꮡ_p0);
    if (ownerDefaulted) {
        _p0 = 1;
    }
    var ᴋ102 = sd;
    var ᴋ103 = owner;
    var ᴋ104 = Ꮡ_p0;
        var (r1, _, e1) = syscall.SyscallN(procGetSecurityDescriptorOwner.Addr(), (uintptr)ᴋ102, (uintptr)ᴋ103, (uintptr)ᴋ104);
    System.GC.KeepAlive(ᴋ102);
    System.GC.KeepAlive(ᴋ103);
    System.GC.KeepAlive(ᴋ104);
    ownerDefaulted = _p0 != 0;
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*ret*/ getSecurityDescriptorRMControl(ж<SECURITY_DESCRIPTOR> sd, ж<uint8> rmControl) {
    error ret = default;

    var ᴋ105 = sd;
    var ᴋ106 = rmControl;
        var (r0, _, _) = syscall.SyscallN(procGetSecurityDescriptorRMControl.Addr(), (uintptr)ᴋ105, (uintptr)ᴋ106);
    System.GC.KeepAlive(ᴋ105);
    System.GC.KeepAlive(ᴋ106);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

internal static error /*err*/ getSecurityDescriptorSacl(ж<SECURITY_DESCRIPTOR> sd, ref bool saclPresent, ж<ж<ACL>> sacl, ref bool saclDefaulted) {
    error err = default;

    ref var _p0 = ref heap(new uint32(), out var Ꮡ_p0);
    if (saclPresent) {
        _p0 = 1;
    }
    ref var _p1 = ref heap(new uint32(), out var Ꮡ_p1);
    if (saclDefaulted) {
        _p1 = 1;
    }
    var ᴋ107 = sd;
    var ᴋ108 = Ꮡ_p0;
    var ᴋ109 = sacl;
    var ᴋ110 = Ꮡ_p1;
        var (r1, _, e1) = syscall.SyscallN(procGetSecurityDescriptorSacl.Addr(), (uintptr)ᴋ107, (uintptr)ᴋ108, (uintptr)ᴋ109, (uintptr)ᴋ110);
    System.GC.KeepAlive(ᴋ107);
    System.GC.KeepAlive(ᴋ108);
    System.GC.KeepAlive(ᴋ109);
    System.GC.KeepAlive(ᴋ110);
    saclPresent = _p0 != 0;
    saclDefaulted = _p1 != 0;
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*ret*/ getSecurityInfo(Handle handle, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation, ж<ж<SID>> owner, ж<ж<SID>> group, ж<ж<ACL>> dacl, ж<ж<ACL>> sacl, ж<ж<SECURITY_DESCRIPTOR>> sd) {
    error ret = default;

    var ᴋ111 = owner;
    var ᴋ112 = group;
    var ᴋ113 = dacl;
    var ᴋ114 = sacl;
    var ᴋ115 = sd;
        var (r0, _, _) = syscall.SyscallN(procGetSecurityInfo.Addr(), (uintptr)handle, (uintptr)objectType, (uintptr)securityInformation, (uintptr)ᴋ111, (uintptr)ᴋ112, (uintptr)ᴋ113, (uintptr)ᴋ114, (uintptr)ᴋ115);
    System.GC.KeepAlive(ᴋ111);
    System.GC.KeepAlive(ᴋ112);
    System.GC.KeepAlive(ᴋ113);
    System.GC.KeepAlive(ᴋ114);
    System.GC.KeepAlive(ᴋ115);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

internal static ж<SidIdentifierAuthority> /*authority*/ getSidIdentifierAuthority(ж<SID> sid) {
    ж<SidIdentifierAuthority> authority = default;

    var ᴋ116 = sid;
        var (r0, _, _) = syscall.SyscallN(procGetSidIdentifierAuthority.Addr(), (uintptr)ᴋ116);
    System.GC.KeepAlive(ᴋ116);
    authority = @unsafe.Pointer.To<SidIdentifierAuthority>((@unsafe.Pointer)r0);
    return authority;
}

internal static ж<uint32> /*subAuthority*/ getSidSubAuthority(ж<SID> sid, uint32 index) {
    ж<uint32> subAuthority = default;

    var ᴋ117 = sid;
        var (r0, _, _) = syscall.SyscallN(procGetSidSubAuthority.Addr(), (uintptr)ᴋ117, (uintptr)index);
    System.GC.KeepAlive(ᴋ117);
    subAuthority = @unsafe.Pointer.To<uint32>((@unsafe.Pointer)r0);
    return subAuthority;
}

internal static ж<uint8> /*count*/ getSidSubAuthorityCount(ж<SID> sid) {
    ж<uint8> count = default;

    var ᴋ118 = sid;
        var (r0, _, _) = syscall.SyscallN(procGetSidSubAuthorityCount.Addr(), (uintptr)ᴋ118);
    System.GC.KeepAlive(ᴋ118);
    count = @unsafe.Pointer.To<uint8>((@unsafe.Pointer)r0);
    return count;
}

public static error /*err*/ GetTokenInformation(Token token, uint32 infoClass, ж<byte> info, uint32 infoLen, ж<uint32> returnedLen) {
    error err = default;

    var ᴋ119 = info;
    var ᴋ120 = returnedLen;
        var (r1, _, e1) = syscall.SyscallN(procGetTokenInformation.Addr(), (uintptr)token, (uintptr)infoClass, (uintptr)ᴋ119, (uintptr)infoLen, (uintptr)ᴋ120);
    System.GC.KeepAlive(ᴋ119);
    System.GC.KeepAlive(ᴋ120);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ImpersonateSelf(uint32 impersonationlevel) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procImpersonateSelf.Addr(), (uintptr)impersonationlevel);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ initializeSecurityDescriptor(ж<SECURITY_DESCRIPTOR> absoluteSD, uint32 revision) {
    error err = default;

    var ᴋ121 = absoluteSD;
        var (r1, _, e1) = syscall.SyscallN(procInitializeSecurityDescriptor.Addr(), (uintptr)ᴋ121, (uintptr)revision);
    System.GC.KeepAlive(ᴋ121);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ InitiateSystemShutdownEx(ж<uint16> machineName, ж<uint16> message, uint32 timeout, bool forceAppsClosed, bool rebootAfterShutdown, uint32 reason) {
    error err = default;

    uint32 _p0 = default;
    if (forceAppsClosed) {
        _p0 = 1;
    }
    uint32 _p1 = default;
    if (rebootAfterShutdown) {
        _p1 = 1;
    }
    var ᴋ122 = machineName;
    var ᴋ123 = message;
        var (r1, _, e1) = syscall.SyscallN(procInitiateSystemShutdownExW.Addr(), (uintptr)ᴋ122, (uintptr)ᴋ123, (uintptr)timeout, (uintptr)_p0, (uintptr)_p1, (uintptr)reason);
    System.GC.KeepAlive(ᴋ122);
    System.GC.KeepAlive(ᴋ123);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (bool ret, error err) isTokenRestricted(Token tokenHandle) {
    bool ret = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procIsTokenRestricted.Addr(), (uintptr)tokenHandle);
    ret = r0 != 0;
    if (!ret) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

internal static bool /*isValid*/ isValidSecurityDescriptor(ж<SECURITY_DESCRIPTOR> sd) {
    bool isValid = default;

    var ᴋ124 = sd;
        var (r0, _, _) = syscall.SyscallN(procIsValidSecurityDescriptor.Addr(), (uintptr)ᴋ124);
    System.GC.KeepAlive(ᴋ124);
    isValid = r0 != 0;
    return isValid;
}

internal static bool /*isValid*/ isValidSid(ж<SID> sid) {
    bool isValid = default;

    var ᴋ125 = sid;
        var (r0, _, _) = syscall.SyscallN(procIsValidSid.Addr(), (uintptr)ᴋ125);
    System.GC.KeepAlive(ᴋ125);
    isValid = r0 != 0;
    return isValid;
}

internal static bool /*isWellKnown*/ isWellKnownSid(ж<SID> sid, WELL_KNOWN_SID_TYPE sidType) {
    bool isWellKnown = default;

    var ᴋ126 = sid;
        var (r0, _, _) = syscall.SyscallN(procIsWellKnownSid.Addr(), (uintptr)ᴋ126, (uintptr)sidType);
    System.GC.KeepAlive(ᴋ126);
    isWellKnown = r0 != 0;
    return isWellKnown;
}

public static error /*err*/ LookupAccountName(ж<uint16> systemName, ж<uint16> accountName, ж<SID> sid, ж<uint32> sidLen, ж<uint16> refdDomainName, ж<uint32> refdDomainNameLen, ж<uint32> use) {
    error err = default;

    var ᴋ127 = systemName;
    var ᴋ128 = accountName;
    var ᴋ129 = sid;
    var ᴋ130 = sidLen;
    var ᴋ131 = refdDomainName;
    var ᴋ132 = refdDomainNameLen;
    var ᴋ133 = use;
        var (r1, _, e1) = syscall.SyscallN(procLookupAccountNameW.Addr(), (uintptr)ᴋ127, (uintptr)ᴋ128, (uintptr)ᴋ129, (uintptr)ᴋ130, (uintptr)ᴋ131, (uintptr)ᴋ132, (uintptr)ᴋ133);
    System.GC.KeepAlive(ᴋ127);
    System.GC.KeepAlive(ᴋ128);
    System.GC.KeepAlive(ᴋ129);
    System.GC.KeepAlive(ᴋ130);
    System.GC.KeepAlive(ᴋ131);
    System.GC.KeepAlive(ᴋ132);
    System.GC.KeepAlive(ᴋ133);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ LookupAccountSid(ж<uint16> systemName, ж<SID> sid, ж<uint16> name, ж<uint32> nameLen, ж<uint16> refdDomainName, ж<uint32> refdDomainNameLen, ж<uint32> use) {
    error err = default;

    var ᴋ134 = systemName;
    var ᴋ135 = sid;
    var ᴋ136 = name;
    var ᴋ137 = nameLen;
    var ᴋ138 = refdDomainName;
    var ᴋ139 = refdDomainNameLen;
    var ᴋ140 = use;
        var (r1, _, e1) = syscall.SyscallN(procLookupAccountSidW.Addr(), (uintptr)ᴋ134, (uintptr)ᴋ135, (uintptr)ᴋ136, (uintptr)ᴋ137, (uintptr)ᴋ138, (uintptr)ᴋ139, (uintptr)ᴋ140);
    System.GC.KeepAlive(ᴋ134);
    System.GC.KeepAlive(ᴋ135);
    System.GC.KeepAlive(ᴋ136);
    System.GC.KeepAlive(ᴋ137);
    System.GC.KeepAlive(ᴋ138);
    System.GC.KeepAlive(ᴋ139);
    System.GC.KeepAlive(ᴋ140);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ LookupPrivilegeValue(ж<uint16> systemname, ж<uint16> name, ж<LUID> luid) {
    error err = default;

    var ᴋ141 = systemname;
    var ᴋ142 = name;
    var ᴋ143 = luid;
        var (r1, _, e1) = syscall.SyscallN(procLookupPrivilegeValueW.Addr(), (uintptr)ᴋ141, (uintptr)ᴋ142, (uintptr)ᴋ143);
    System.GC.KeepAlive(ᴋ141);
    System.GC.KeepAlive(ᴋ142);
    System.GC.KeepAlive(ᴋ143);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ makeAbsoluteSD(ж<SECURITY_DESCRIPTOR> selfRelativeSD, ж<SECURITY_DESCRIPTOR> absoluteSD, ж<uint32> absoluteSDSize, ж<ACL> dacl, ж<uint32> daclSize, ж<ACL> sacl, ж<uint32> saclSize, ж<SID> owner, ж<uint32> ownerSize, ж<SID> group, ж<uint32> groupSize) {
    error err = default;

    var ᴋ144 = selfRelativeSD;
    var ᴋ145 = absoluteSD;
    var ᴋ146 = absoluteSDSize;
    var ᴋ147 = dacl;
    var ᴋ148 = daclSize;
    var ᴋ149 = sacl;
    var ᴋ150 = saclSize;
    var ᴋ151 = owner;
    var ᴋ152 = ownerSize;
    var ᴋ153 = group;
    var ᴋ154 = groupSize;
        var (r1, _, e1) = syscall.SyscallN(procMakeAbsoluteSD.Addr(), (uintptr)ᴋ144, (uintptr)ᴋ145, (uintptr)ᴋ146, (uintptr)ᴋ147, (uintptr)ᴋ148, (uintptr)ᴋ149, (uintptr)ᴋ150, (uintptr)ᴋ151, (uintptr)ᴋ152, (uintptr)ᴋ153, (uintptr)ᴋ154);
    System.GC.KeepAlive(ᴋ144);
    System.GC.KeepAlive(ᴋ145);
    System.GC.KeepAlive(ᴋ146);
    System.GC.KeepAlive(ᴋ147);
    System.GC.KeepAlive(ᴋ148);
    System.GC.KeepAlive(ᴋ149);
    System.GC.KeepAlive(ᴋ150);
    System.GC.KeepAlive(ᴋ151);
    System.GC.KeepAlive(ᴋ152);
    System.GC.KeepAlive(ᴋ153);
    System.GC.KeepAlive(ᴋ154);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ makeSelfRelativeSD(ж<SECURITY_DESCRIPTOR> absoluteSD, ж<SECURITY_DESCRIPTOR> selfRelativeSD, ж<uint32> selfRelativeSDSize) {
    error err = default;

    var ᴋ155 = absoluteSD;
    var ᴋ156 = selfRelativeSD;
    var ᴋ157 = selfRelativeSDSize;
        var (r1, _, e1) = syscall.SyscallN(procMakeSelfRelativeSD.Addr(), (uintptr)ᴋ155, (uintptr)ᴋ156, (uintptr)ᴋ157);
    System.GC.KeepAlive(ᴋ155);
    System.GC.KeepAlive(ᴋ156);
    System.GC.KeepAlive(ᴋ157);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*ret*/ NotifyServiceStatusChange(Handle service, uint32 notifyMask, ж<SERVICE_NOTIFY> notifier) {
    error ret = default;

    var ᴋ158 = notifier;
        var (r0, _, _) = syscall.SyscallN(procNotifyServiceStatusChangeW.Addr(), (uintptr)service, (uintptr)notifyMask, (uintptr)ᴋ158);
    System.GC.KeepAlive(ᴋ158);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*err*/ OpenProcessToken(Handle process, uint32 access, ж<Token> token) {
    error err = default;

    var ᴋ159 = token;
        var (r1, _, e1) = syscall.SyscallN(procOpenProcessToken.Addr(), (uintptr)process, (uintptr)access, (uintptr)ᴋ159);
    System.GC.KeepAlive(ᴋ159);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle handle, error err) OpenSCManager(ж<uint16> machineName, ж<uint16> databaseName, uint32 access) {
    Handle handle = default;
    error err = default;

    var ᴋ160 = machineName;
    var ᴋ161 = databaseName;
        var (r0, _, e1) = syscall.SyscallN(procOpenSCManagerW.Addr(), (uintptr)ᴋ160, (uintptr)ᴋ161, (uintptr)access);
    System.GC.KeepAlive(ᴋ160);
    System.GC.KeepAlive(ᴋ161);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) OpenService(Handle mgr, ж<uint16> serviceName, uint32 access) {
    Handle handle = default;
    error err = default;

    var ᴋ162 = serviceName;
        var (r0, _, e1) = syscall.SyscallN(procOpenServiceW.Addr(), (uintptr)mgr, (uintptr)ᴋ162, (uintptr)access);
    System.GC.KeepAlive(ᴋ162);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ OpenThreadToken(Handle thread, uint32 access, bool openAsSelf, ж<Token> token) {
    error err = default;

    uint32 _p0 = default;
    if (openAsSelf) {
        _p0 = 1;
    }
    var ᴋ163 = token;
        var (r1, _, e1) = syscall.SyscallN(procOpenThreadToken.Addr(), (uintptr)thread, (uintptr)access, (uintptr)_p0, (uintptr)ᴋ163);
    System.GC.KeepAlive(ᴋ163);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ QueryServiceConfig2(Handle service, uint32 infoLevel, ж<byte> buff, uint32 buffSize, ж<uint32> bytesNeeded) {
    error err = default;

    var ᴋ164 = buff;
    var ᴋ165 = bytesNeeded;
        var (r1, _, e1) = syscall.SyscallN(procQueryServiceConfig2W.Addr(), (uintptr)service, (uintptr)infoLevel, (uintptr)ᴋ164, (uintptr)buffSize, (uintptr)ᴋ165);
    System.GC.KeepAlive(ᴋ164);
    System.GC.KeepAlive(ᴋ165);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ QueryServiceConfig(Handle service, ж<QUERY_SERVICE_CONFIG> serviceConfig, uint32 bufSize, ж<uint32> bytesNeeded) {
    error err = default;

    var ᴋ166 = serviceConfig;
    var ᴋ167 = bytesNeeded;
        var (r1, _, e1) = syscall.SyscallN(procQueryServiceConfigW.Addr(), (uintptr)service, (uintptr)ᴋ166, (uintptr)bufSize, (uintptr)ᴋ167);
    System.GC.KeepAlive(ᴋ166);
    System.GC.KeepAlive(ᴋ167);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ QueryServiceDynamicInformation(Handle service, uint32 infoLevel, @unsafe.Pointer dynamicInfo) {
    error err = default;

    err = procQueryServiceDynamicInformation.Find();
    if (err != nil) {
        return err;
    }
    var ᴋ168 = dynamicInfo;
        var (r1, _, e1) = syscall.SyscallN(procQueryServiceDynamicInformation.Addr(), (uintptr)service, (uintptr)infoLevel, (uintptr)ᴋ168);
    System.GC.KeepAlive(ᴋ168);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ QueryServiceLockStatus(Handle mgr, ж<QUERY_SERVICE_LOCK_STATUS> lockStatus, uint32 bufSize, ж<uint32> bytesNeeded) {
    error err = default;

    var ᴋ169 = lockStatus;
    var ᴋ170 = bytesNeeded;
        var (r1, _, e1) = syscall.SyscallN(procQueryServiceLockStatusW.Addr(), (uintptr)mgr, (uintptr)ᴋ169, (uintptr)bufSize, (uintptr)ᴋ170);
    System.GC.KeepAlive(ᴋ169);
    System.GC.KeepAlive(ᴋ170);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ QueryServiceStatus(Handle service, ж<SERVICE_STATUS> status) {
    error err = default;

    var ᴋ171 = status;
        var (r1, _, e1) = syscall.SyscallN(procQueryServiceStatus.Addr(), (uintptr)service, (uintptr)ᴋ171);
    System.GC.KeepAlive(ᴋ171);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ QueryServiceStatusEx(Handle service, uint32 infoLevel, ж<byte> buff, uint32 buffSize, ж<uint32> bytesNeeded) {
    error err = default;

    var ᴋ172 = buff;
    var ᴋ173 = bytesNeeded;
        var (r1, _, e1) = syscall.SyscallN(procQueryServiceStatusEx.Addr(), (uintptr)service, (uintptr)infoLevel, (uintptr)ᴋ172, (uintptr)buffSize, (uintptr)ᴋ173);
    System.GC.KeepAlive(ᴋ172);
    System.GC.KeepAlive(ᴋ173);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*regerrno*/ RegCloseKey(Handle key) {
    error regerrno = default;

    var (r0, _, _) = syscall.SyscallN(procRegCloseKey.Addr(), (uintptr)key);
    if (r0 != 0) {
        regerrno = ((syscall.Errno)r0);
    }
    return regerrno;
}

public static error /*regerrno*/ RegEnumKeyEx(Handle key, uint32 index, ж<uint16> name, ж<uint32> nameLen, ж<uint32> reserved, ж<uint16> @class, ж<uint32> classLen, ж<Filetime> lastWriteTime) {
    error regerrno = default;

    var ᴋ174 = name;
    var ᴋ175 = nameLen;
    var ᴋ176 = reserved;
    var ᴋ177 = @class;
    var ᴋ178 = classLen;
    var ᴋ179 = lastWriteTime;
        var (r0, _, _) = syscall.SyscallN(procRegEnumKeyExW.Addr(), (uintptr)key, (uintptr)index, (uintptr)ᴋ174, (uintptr)ᴋ175, (uintptr)ᴋ176, (uintptr)ᴋ177, (uintptr)ᴋ178, (uintptr)ᴋ179);
    System.GC.KeepAlive(ᴋ174);
    System.GC.KeepAlive(ᴋ175);
    System.GC.KeepAlive(ᴋ176);
    System.GC.KeepAlive(ᴋ177);
    System.GC.KeepAlive(ᴋ178);
    System.GC.KeepAlive(ᴋ179);
    if (r0 != 0) {
        regerrno = ((syscall.Errno)r0);
    }
    return regerrno;
}

public static error /*regerrno*/ RegNotifyChangeKeyValue(Handle key, bool watchSubtree, uint32 notifyFilter, Handle @event, bool asynchronous) {
    error regerrno = default;

    uint32 _p0 = default;
    if (watchSubtree) {
        _p0 = 1;
    }
    uint32 _p1 = default;
    if (asynchronous) {
        _p1 = 1;
    }
    var (r0, _, _) = syscall.SyscallN(procRegNotifyChangeKeyValue.Addr(), (uintptr)key, (uintptr)_p0, (uintptr)notifyFilter, (uintptr)@event, (uintptr)_p1);
    if (r0 != 0) {
        regerrno = ((syscall.Errno)r0);
    }
    return regerrno;
}

public static error /*regerrno*/ RegOpenKeyEx(Handle key, ж<uint16> subkey, uint32 options, uint32 desiredAccess, ж<Handle> result) {
    error regerrno = default;

    var ᴋ180 = subkey;
    var ᴋ181 = result;
        var (r0, _, _) = syscall.SyscallN(procRegOpenKeyExW.Addr(), (uintptr)key, (uintptr)ᴋ180, (uintptr)options, (uintptr)desiredAccess, (uintptr)ᴋ181);
    System.GC.KeepAlive(ᴋ180);
    System.GC.KeepAlive(ᴋ181);
    if (r0 != 0) {
        regerrno = ((syscall.Errno)r0);
    }
    return regerrno;
}

public static error /*regerrno*/ RegQueryInfoKey(Handle key, ж<uint16> @class, ж<uint32> classLen, ж<uint32> reserved, ж<uint32> subkeysLen, ж<uint32> maxSubkeyLen, ж<uint32> maxClassLen, ж<uint32> valuesLen, ж<uint32> maxValueNameLen, ж<uint32> maxValueLen, ж<uint32> saLen, ж<Filetime> lastWriteTime) {
    error regerrno = default;

    var ᴋ182 = @class;
    var ᴋ183 = classLen;
    var ᴋ184 = reserved;
    var ᴋ185 = subkeysLen;
    var ᴋ186 = maxSubkeyLen;
    var ᴋ187 = maxClassLen;
    var ᴋ188 = valuesLen;
    var ᴋ189 = maxValueNameLen;
    var ᴋ190 = maxValueLen;
    var ᴋ191 = saLen;
    var ᴋ192 = lastWriteTime;
        var (r0, _, _) = syscall.SyscallN(procRegQueryInfoKeyW.Addr(), (uintptr)key, (uintptr)ᴋ182, (uintptr)ᴋ183, (uintptr)ᴋ184, (uintptr)ᴋ185, (uintptr)ᴋ186, (uintptr)ᴋ187, (uintptr)ᴋ188, (uintptr)ᴋ189, (uintptr)ᴋ190, (uintptr)ᴋ191, (uintptr)ᴋ192);
    System.GC.KeepAlive(ᴋ182);
    System.GC.KeepAlive(ᴋ183);
    System.GC.KeepAlive(ᴋ184);
    System.GC.KeepAlive(ᴋ185);
    System.GC.KeepAlive(ᴋ186);
    System.GC.KeepAlive(ᴋ187);
    System.GC.KeepAlive(ᴋ188);
    System.GC.KeepAlive(ᴋ189);
    System.GC.KeepAlive(ᴋ190);
    System.GC.KeepAlive(ᴋ191);
    System.GC.KeepAlive(ᴋ192);
    if (r0 != 0) {
        regerrno = ((syscall.Errno)r0);
    }
    return regerrno;
}

public static error /*regerrno*/ RegQueryValueEx(Handle key, ж<uint16> name, ж<uint32> reserved, ж<uint32> valtype, ж<byte> buf, ж<uint32> buflen) {
    error regerrno = default;

    var ᴋ193 = name;
    var ᴋ194 = reserved;
    var ᴋ195 = valtype;
    var ᴋ196 = buf;
    var ᴋ197 = buflen;
        var (r0, _, _) = syscall.SyscallN(procRegQueryValueExW.Addr(), (uintptr)key, (uintptr)ᴋ193, (uintptr)ᴋ194, (uintptr)ᴋ195, (uintptr)ᴋ196, (uintptr)ᴋ197);
    System.GC.KeepAlive(ᴋ193);
    System.GC.KeepAlive(ᴋ194);
    System.GC.KeepAlive(ᴋ195);
    System.GC.KeepAlive(ᴋ196);
    System.GC.KeepAlive(ᴋ197);
    if (r0 != 0) {
        regerrno = ((syscall.Errno)r0);
    }
    return regerrno;
}

public static (Handle handle, error err) RegisterEventSource(ж<uint16> uncServerName, ж<uint16> sourceName) {
    Handle handle = default;
    error err = default;

    var ᴋ198 = uncServerName;
    var ᴋ199 = sourceName;
        var (r0, _, e1) = syscall.SyscallN(procRegisterEventSourceW.Addr(), (uintptr)ᴋ198, (uintptr)ᴋ199);
    System.GC.KeepAlive(ᴋ198);
    System.GC.KeepAlive(ᴋ199);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) RegisterServiceCtrlHandlerEx(ж<uint16> serviceName, uintptr handlerProc, uintptr context) {
    Handle handle = default;
    error err = default;

    var ᴋ200 = serviceName;
        var (r0, _, e1) = syscall.SyscallN(procRegisterServiceCtrlHandlerExW.Addr(), (uintptr)ᴋ200, (uintptr)handlerProc, (uintptr)context);
    System.GC.KeepAlive(ᴋ200);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ ReportEvent(Handle log, uint16 etype, uint16 category, uint32 eventId, uintptr usrSId, uint16 numStrings, uint32 dataSize, ж<ж<uint16>> strings, ж<byte> rawData) {
    error err = default;

    var ᴋ201 = strings;
    var ᴋ202 = rawData;
        var (r1, _, e1) = syscall.SyscallN(procReportEventW.Addr(), (uintptr)log, (uintptr)etype, (uintptr)category, (uintptr)eventId, (uintptr)usrSId, (uintptr)numStrings, (uintptr)dataSize, (uintptr)ᴋ201, (uintptr)ᴋ202);
    System.GC.KeepAlive(ᴋ201);
    System.GC.KeepAlive(ᴋ202);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ RevertToSelf() {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procRevertToSelf.Addr());
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*ret*/ setEntriesInAcl(uint32 countExplicitEntries, ж<EXPLICIT_ACCESS> explicitEntries, ж<ACL> oldACL, ж<ж<ACL>> newACL) {
    error ret = default;

    var ᴋ203 = explicitEntries;
    var ᴋ204 = oldACL;
    var ᴋ205 = newACL;
        var (r0, _, _) = syscall.SyscallN(procSetEntriesInAclW.Addr(), (uintptr)countExplicitEntries, (uintptr)ᴋ203, (uintptr)ᴋ204, (uintptr)ᴋ205);
    System.GC.KeepAlive(ᴋ203);
    System.GC.KeepAlive(ᴋ204);
    System.GC.KeepAlive(ᴋ205);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*err*/ SetKernelObjectSecurity(Handle handle, SECURITY_INFORMATION securityInformation, ж<SECURITY_DESCRIPTOR> securityDescriptor) {
    error err = default;

    var ᴋ206 = securityDescriptor;
        var (r1, _, e1) = syscall.SyscallN(procSetKernelObjectSecurity.Addr(), (uintptr)handle, (uintptr)securityInformation, (uintptr)ᴋ206);
    System.GC.KeepAlive(ᴋ206);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*ret*/ SetNamedSecurityInfo(@string objectName, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation, ж<SID> owner, ж<SID> group, ж<ACL> dacl, ж<ACL> sacl) {
    error ret = default;

    ж<uint16> _p0 = default;
    (_p0, ret) = syscall.UTF16PtrFromString(objectName);
    if (ret != nil) {
        return ret;
    }
    return _SetNamedSecurityInfo(_p0, objectType, securityInformation, owner, group, dacl, sacl);
}

internal static error /*ret*/ _SetNamedSecurityInfo(ж<uint16> objectName, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation, ж<SID> owner, ж<SID> group, ж<ACL> dacl, ж<ACL> sacl) {
    error ret = default;

    var ᴋ207 = objectName;
    var ᴋ208 = owner;
    var ᴋ209 = group;
    var ᴋ210 = dacl;
    var ᴋ211 = sacl;
        var (r0, _, _) = syscall.SyscallN(procSetNamedSecurityInfoW.Addr(), (uintptr)ᴋ207, (uintptr)objectType, (uintptr)securityInformation, (uintptr)ᴋ208, (uintptr)ᴋ209, (uintptr)ᴋ210, (uintptr)ᴋ211);
    System.GC.KeepAlive(ᴋ207);
    System.GC.KeepAlive(ᴋ208);
    System.GC.KeepAlive(ᴋ209);
    System.GC.KeepAlive(ᴋ210);
    System.GC.KeepAlive(ᴋ211);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

internal static error /*err*/ setSecurityDescriptorControl(ж<SECURITY_DESCRIPTOR> sd, SECURITY_DESCRIPTOR_CONTROL controlBitsOfInterest, SECURITY_DESCRIPTOR_CONTROL controlBitsToSet) {
    error err = default;

    var ᴋ212 = sd;
        var (r1, _, e1) = syscall.SyscallN(procSetSecurityDescriptorControl.Addr(), (uintptr)ᴋ212, (uintptr)controlBitsOfInterest, (uintptr)controlBitsToSet);
    System.GC.KeepAlive(ᴋ212);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setSecurityDescriptorDacl(ж<SECURITY_DESCRIPTOR> sd, bool daclPresent, ж<ACL> dacl, bool daclDefaulted) {
    error err = default;

    uint32 _p0 = default;
    if (daclPresent) {
        _p0 = 1;
    }
    uint32 _p1 = default;
    if (daclDefaulted) {
        _p1 = 1;
    }
    var ᴋ213 = sd;
    var ᴋ214 = dacl;
        var (r1, _, e1) = syscall.SyscallN(procSetSecurityDescriptorDacl.Addr(), (uintptr)ᴋ213, (uintptr)_p0, (uintptr)ᴋ214, (uintptr)_p1);
    System.GC.KeepAlive(ᴋ213);
    System.GC.KeepAlive(ᴋ214);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setSecurityDescriptorGroup(ж<SECURITY_DESCRIPTOR> sd, ж<SID> group, bool groupDefaulted) {
    error err = default;

    uint32 _p0 = default;
    if (groupDefaulted) {
        _p0 = 1;
    }
    var ᴋ215 = sd;
    var ᴋ216 = group;
        var (r1, _, e1) = syscall.SyscallN(procSetSecurityDescriptorGroup.Addr(), (uintptr)ᴋ215, (uintptr)ᴋ216, (uintptr)_p0);
    System.GC.KeepAlive(ᴋ215);
    System.GC.KeepAlive(ᴋ216);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setSecurityDescriptorOwner(ж<SECURITY_DESCRIPTOR> sd, ж<SID> owner, bool ownerDefaulted) {
    error err = default;

    uint32 _p0 = default;
    if (ownerDefaulted) {
        _p0 = 1;
    }
    var ᴋ217 = sd;
    var ᴋ218 = owner;
        var (r1, _, e1) = syscall.SyscallN(procSetSecurityDescriptorOwner.Addr(), (uintptr)ᴋ217, (uintptr)ᴋ218, (uintptr)_p0);
    System.GC.KeepAlive(ᴋ217);
    System.GC.KeepAlive(ᴋ218);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static void setSecurityDescriptorRMControl(ж<SECURITY_DESCRIPTOR> sd, ж<uint8> rmControl) {
    var ᴋ219 = sd;
    var ᴋ220 = rmControl;
        syscall.SyscallN(procSetSecurityDescriptorRMControl.Addr(), (uintptr)ᴋ219, (uintptr)ᴋ220);
    System.GC.KeepAlive(ᴋ219);
    System.GC.KeepAlive(ᴋ220);
    return;
}

internal static error /*err*/ setSecurityDescriptorSacl(ж<SECURITY_DESCRIPTOR> sd, bool saclPresent, ж<ACL> sacl, bool saclDefaulted) {
    error err = default;

    uint32 _p0 = default;
    if (saclPresent) {
        _p0 = 1;
    }
    uint32 _p1 = default;
    if (saclDefaulted) {
        _p1 = 1;
    }
    var ᴋ221 = sd;
    var ᴋ222 = sacl;
        var (r1, _, e1) = syscall.SyscallN(procSetSecurityDescriptorSacl.Addr(), (uintptr)ᴋ221, (uintptr)_p0, (uintptr)ᴋ222, (uintptr)_p1);
    System.GC.KeepAlive(ᴋ221);
    System.GC.KeepAlive(ᴋ222);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*ret*/ SetSecurityInfo(Handle handle, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation, ж<SID> owner, ж<SID> group, ж<ACL> dacl, ж<ACL> sacl) {
    error ret = default;

    var ᴋ223 = owner;
    var ᴋ224 = group;
    var ᴋ225 = dacl;
    var ᴋ226 = sacl;
        var (r0, _, _) = syscall.SyscallN(procSetSecurityInfo.Addr(), (uintptr)handle, (uintptr)objectType, (uintptr)securityInformation, (uintptr)ᴋ223, (uintptr)ᴋ224, (uintptr)ᴋ225, (uintptr)ᴋ226);
    System.GC.KeepAlive(ᴋ223);
    System.GC.KeepAlive(ᴋ224);
    System.GC.KeepAlive(ᴋ225);
    System.GC.KeepAlive(ᴋ226);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*err*/ SetServiceStatus(Handle service, ж<SERVICE_STATUS> serviceStatus) {
    error err = default;

    var ᴋ227 = serviceStatus;
        var (r1, _, e1) = syscall.SyscallN(procSetServiceStatus.Addr(), (uintptr)service, (uintptr)ᴋ227);
    System.GC.KeepAlive(ᴋ227);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetThreadToken(ж<Handle> thread, Token token) {
    error err = default;

    var ᴋ228 = thread;
        var (r1, _, e1) = syscall.SyscallN(procSetThreadToken.Addr(), (uintptr)ᴋ228, (uintptr)token);
    System.GC.KeepAlive(ᴋ228);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetTokenInformation(Token token, uint32 infoClass, ж<byte> info, uint32 infoLen) {
    error err = default;

    var ᴋ229 = info;
        var (r1, _, e1) = syscall.SyscallN(procSetTokenInformation.Addr(), (uintptr)token, (uintptr)infoClass, (uintptr)ᴋ229, (uintptr)infoLen);
    System.GC.KeepAlive(ᴋ229);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ StartServiceCtrlDispatcher(ж<SERVICE_TABLE_ENTRY> serviceTable) {
    error err = default;

    var ᴋ230 = serviceTable;
        var (r1, _, e1) = syscall.SyscallN(procStartServiceCtrlDispatcherW.Addr(), (uintptr)ᴋ230);
    System.GC.KeepAlive(ᴋ230);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ StartService(Handle service, uint32 numArgs, ж<ж<uint16>> argVectors) {
    error err = default;

    var ᴋ231 = argVectors;
        var (r1, _, e1) = syscall.SyscallN(procStartServiceW.Addr(), (uintptr)service, (uintptr)numArgs, (uintptr)ᴋ231);
    System.GC.KeepAlive(ᴋ231);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CertAddCertificateContextToStore(Handle store, ж<CertContext> certContext, uint32 addDisposition, ж<ж<CertContext>> storeContext) {
    error err = default;

    var ᴋ232 = certContext;
    var ᴋ233 = storeContext;
        var (r1, _, e1) = syscall.SyscallN(procCertAddCertificateContextToStore.Addr(), (uintptr)store, (uintptr)ᴋ232, (uintptr)addDisposition, (uintptr)ᴋ233);
    System.GC.KeepAlive(ᴋ232);
    System.GC.KeepAlive(ᴋ233);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CertCloseStore(Handle store, uint32 flags) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procCertCloseStore.Addr(), (uintptr)store, (uintptr)flags);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (ж<CertContext> context, error err) CertCreateCertificateContext(uint32 certEncodingType, ж<byte> certEncoded, uint32 encodedLen) {
    ж<CertContext> context = default;
    error err = default;

    var ᴋ234 = certEncoded;
        var (r0, _, e1) = syscall.SyscallN(procCertCreateCertificateContext.Addr(), (uintptr)certEncodingType, (uintptr)ᴋ234, (uintptr)encodedLen);
    System.GC.KeepAlive(ᴋ234);
    context = @unsafe.Pointer.To<CertContext>((@unsafe.Pointer)r0);
    if (context == nil) {
        err = errnoErr(e1);
    }
    return (context, err);
}

public static error /*err*/ CertDeleteCertificateFromStore(ж<CertContext> certContext) {
    error err = default;

    var ᴋ235 = certContext;
        var (r1, _, e1) = syscall.SyscallN(procCertDeleteCertificateFromStore.Addr(), (uintptr)ᴋ235);
    System.GC.KeepAlive(ᴋ235);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static ж<CertContext> /*dupContext*/ CertDuplicateCertificateContext(ж<CertContext> certContext) {
    ж<CertContext> dupContext = default;

    var ᴋ236 = certContext;
        var (r0, _, _) = syscall.SyscallN(procCertDuplicateCertificateContext.Addr(), (uintptr)ᴋ236);
    System.GC.KeepAlive(ᴋ236);
    dupContext = @unsafe.Pointer.To<CertContext>((@unsafe.Pointer)r0);
    return dupContext;
}

public static (ж<CertContext> context, error err) CertEnumCertificatesInStore(Handle store, ж<CertContext> prevContext) {
    ж<CertContext> context = default;
    error err = default;

    var ᴋ237 = prevContext;
        var (r0, _, e1) = syscall.SyscallN(procCertEnumCertificatesInStore.Addr(), (uintptr)store, (uintptr)ᴋ237);
    System.GC.KeepAlive(ᴋ237);
    context = @unsafe.Pointer.To<CertContext>((@unsafe.Pointer)r0);
    if (context == nil) {
        err = errnoErr(e1);
    }
    return (context, err);
}

public static (ж<CertContext> cert, error err) CertFindCertificateInStore(Handle store, uint32 certEncodingType, uint32 findFlags, uint32 findType, @unsafe.Pointer findPara, ж<CertContext> prevCertContext) {
    ж<CertContext> cert = default;
    error err = default;

    var ᴋ238 = findPara;
    var ᴋ239 = prevCertContext;
        var (r0, _, e1) = syscall.SyscallN(procCertFindCertificateInStore.Addr(), (uintptr)store, (uintptr)certEncodingType, (uintptr)findFlags, (uintptr)findType, (uintptr)ᴋ238, (uintptr)ᴋ239);
    System.GC.KeepAlive(ᴋ238);
    System.GC.KeepAlive(ᴋ239);
    cert = @unsafe.Pointer.To<CertContext>((@unsafe.Pointer)r0);
    if (cert == nil) {
        err = errnoErr(e1);
    }
    return (cert, err);
}

public static (ж<CertChainContext> certchain, error err) CertFindChainInStore(Handle store, uint32 certEncodingType, uint32 findFlags, uint32 findType, @unsafe.Pointer findPara, ж<CertChainContext> prevChainContext) {
    ж<CertChainContext> certchain = default;
    error err = default;

    var ᴋ240 = findPara;
    var ᴋ241 = prevChainContext;
        var (r0, _, e1) = syscall.SyscallN(procCertFindChainInStore.Addr(), (uintptr)store, (uintptr)certEncodingType, (uintptr)findFlags, (uintptr)findType, (uintptr)ᴋ240, (uintptr)ᴋ241);
    System.GC.KeepAlive(ᴋ240);
    System.GC.KeepAlive(ᴋ241);
    certchain = @unsafe.Pointer.To<CertChainContext>((@unsafe.Pointer)r0);
    if (certchain == nil) {
        err = errnoErr(e1);
    }
    return (certchain, err);
}

public static ж<CertExtension> /*ret*/ CertFindExtension(ж<byte> objId, uint32 countExtensions, ж<CertExtension> extensions) {
    ж<CertExtension> ret = default;

    var ᴋ242 = objId;
    var ᴋ243 = extensions;
        var (r0, _, _) = syscall.SyscallN(procCertFindExtension.Addr(), (uintptr)ᴋ242, (uintptr)countExtensions, (uintptr)ᴋ243);
    System.GC.KeepAlive(ᴋ242);
    System.GC.KeepAlive(ᴋ243);
    ret = @unsafe.Pointer.To<CertExtension>((@unsafe.Pointer)r0);
    return ret;
}

public static void CertFreeCertificateChain(ж<CertChainContext> ctx) {
    var ᴋ244 = ctx;
        syscall.SyscallN(procCertFreeCertificateChain.Addr(), (uintptr)ᴋ244);
    System.GC.KeepAlive(ᴋ244);
    return;
}

public static error /*err*/ CertFreeCertificateContext(ж<CertContext> ctx) {
    error err = default;

    var ᴋ245 = ctx;
        var (r1, _, e1) = syscall.SyscallN(procCertFreeCertificateContext.Addr(), (uintptr)ᴋ245);
    System.GC.KeepAlive(ᴋ245);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CertGetCertificateChain(Handle engine, ж<CertContext> leaf, ж<Filetime> time, Handle additionalStore, ж<CertChainPara> para, uint32 flags, uintptr reserved, ж<ж<CertChainContext>> chainCtx) {
    error err = default;

    var ᴋ246 = leaf;
    var ᴋ247 = time;
    var ᴋ248 = para;
    var ᴋ249 = chainCtx;
        var (r1, _, e1) = syscall.SyscallN(procCertGetCertificateChain.Addr(), (uintptr)engine, (uintptr)ᴋ246, (uintptr)ᴋ247, (uintptr)additionalStore, (uintptr)ᴋ248, (uintptr)flags, (uintptr)reserved, (uintptr)ᴋ249);
    System.GC.KeepAlive(ᴋ246);
    System.GC.KeepAlive(ᴋ247);
    System.GC.KeepAlive(ᴋ248);
    System.GC.KeepAlive(ᴋ249);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static uint32 /*chars*/ CertGetNameString(ж<CertContext> certContext, uint32 nameType, uint32 flags, @unsafe.Pointer typePara, ж<uint16> name, uint32 size) {
    uint32 chars = default;

    var ᴋ250 = certContext;
    var ᴋ251 = typePara;
    var ᴋ252 = name;
        var (r0, _, _) = syscall.SyscallN(procCertGetNameStringW.Addr(), (uintptr)ᴋ250, (uintptr)nameType, (uintptr)flags, (uintptr)ᴋ251, (uintptr)ᴋ252, (uintptr)size);
    System.GC.KeepAlive(ᴋ250);
    System.GC.KeepAlive(ᴋ251);
    System.GC.KeepAlive(ᴋ252);
    chars = (uint32)r0;
    return chars;
}

public static (Handle handle, error err) CertOpenStore(uintptr storeProvider, uint32 msgAndCertEncodingType, uintptr cryptProv, uint32 flags, uintptr para) {
    Handle handle = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procCertOpenStore.Addr(), (uintptr)storeProvider, (uintptr)msgAndCertEncodingType, (uintptr)cryptProv, (uintptr)flags, (uintptr)para);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle store, error err) CertOpenSystemStore(Handle hprov, ж<uint16> name) {
    Handle store = default;
    error err = default;

    var ᴋ253 = name;
        var (r0, _, e1) = syscall.SyscallN(procCertOpenSystemStoreW.Addr(), (uintptr)hprov, (uintptr)ᴋ253);
    System.GC.KeepAlive(ᴋ253);
    store = (Handle)r0;
    if (store == 0) {
        err = errnoErr(e1);
    }
    return (store, err);
}

public static error /*err*/ CertVerifyCertificateChainPolicy(uintptr policyOID, ж<CertChainContext> chain, ж<CertChainPolicyPara> para, ж<CertChainPolicyStatus> status) {
    error err = default;

    var ᴋ254 = chain;
    var ᴋ255 = para;
    var ᴋ256 = status;
        var (r1, _, e1) = syscall.SyscallN(procCertVerifyCertificateChainPolicy.Addr(), (uintptr)policyOID, (uintptr)ᴋ254, (uintptr)ᴋ255, (uintptr)ᴋ256);
    System.GC.KeepAlive(ᴋ254);
    System.GC.KeepAlive(ᴋ255);
    System.GC.KeepAlive(ᴋ256);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CryptAcquireCertificatePrivateKey(ж<CertContext> cert, uint32 flags, @unsafe.Pointer parameters, ж<Handle> cryptProvOrNCryptKey, ж<uint32> keySpec, ж<bool> ᏑcallerFreeProvOrNCryptKey) {
    error err = default;
    ref var callerFreeProvOrNCryptKey = ref ᏑcallerFreeProvOrNCryptKey.DerefOrNull();

    ref var _p0 = ref heap(new uint32(), out var Ꮡ_p0);
    if (callerFreeProvOrNCryptKey) {
        _p0 = 1;
    }
    var ᴋ257 = cert;
    var ᴋ258 = parameters;
    var ᴋ259 = cryptProvOrNCryptKey;
    var ᴋ260 = keySpec;
    var ᴋ261 = Ꮡ_p0;
        var (r1, _, e1) = syscall.SyscallN(procCryptAcquireCertificatePrivateKey.Addr(), (uintptr)ᴋ257, (uintptr)flags, (uintptr)ᴋ258, (uintptr)ᴋ259, (uintptr)ᴋ260, (uintptr)ᴋ261);
    System.GC.KeepAlive(ᴋ257);
    System.GC.KeepAlive(ᴋ258);
    System.GC.KeepAlive(ᴋ259);
    System.GC.KeepAlive(ᴋ260);
    System.GC.KeepAlive(ᴋ261);
    callerFreeProvOrNCryptKey = _p0 != 0;
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CryptDecodeObject(uint32 encodingType, ж<byte> structType, ж<byte> encodedBytes, uint32 lenEncodedBytes, uint32 flags, @unsafe.Pointer decoded, ж<uint32> decodedLen) {
    error err = default;

    var ᴋ262 = structType;
    var ᴋ263 = encodedBytes;
    var ᴋ264 = decoded;
    var ᴋ265 = decodedLen;
        var (r1, _, e1) = syscall.SyscallN(procCryptDecodeObject.Addr(), (uintptr)encodingType, (uintptr)ᴋ262, (uintptr)ᴋ263, (uintptr)lenEncodedBytes, (uintptr)flags, (uintptr)ᴋ264, (uintptr)ᴋ265);
    System.GC.KeepAlive(ᴋ262);
    System.GC.KeepAlive(ᴋ263);
    System.GC.KeepAlive(ᴋ264);
    System.GC.KeepAlive(ᴋ265);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CryptProtectData(ж<DataBlob> dataIn, ж<uint16> name, ж<DataBlob> optionalEntropy, uintptr reserved, ж<CryptProtectPromptStruct> promptStruct, uint32 flags, ж<DataBlob> dataOut) {
    error err = default;

    var ᴋ266 = dataIn;
    var ᴋ267 = name;
    var ᴋ268 = optionalEntropy;
    var ᴋ269 = promptStruct;
    var ᴋ270 = dataOut;
        var (r1, _, e1) = syscall.SyscallN(procCryptProtectData.Addr(), (uintptr)ᴋ266, (uintptr)ᴋ267, (uintptr)ᴋ268, (uintptr)reserved, (uintptr)ᴋ269, (uintptr)flags, (uintptr)ᴋ270);
    System.GC.KeepAlive(ᴋ266);
    System.GC.KeepAlive(ᴋ267);
    System.GC.KeepAlive(ᴋ268);
    System.GC.KeepAlive(ᴋ269);
    System.GC.KeepAlive(ᴋ270);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CryptQueryObject(uint32 objectType, @unsafe.Pointer @object, uint32 expectedContentTypeFlags, uint32 expectedFormatTypeFlags, uint32 flags, ж<uint32> msgAndCertEncodingType, ж<uint32> contentType, ж<uint32> formatType, ж<Handle> certStore, ж<Handle> msg, ж<@unsafe.Pointer> context) {
    error err = default;

    var ᴋ271 = @object;
    var ᴋ272 = msgAndCertEncodingType;
    var ᴋ273 = contentType;
    var ᴋ274 = formatType;
    var ᴋ275 = certStore;
    var ᴋ276 = msg;
    var ᴋ277 = context;
        var (r1, _, e1) = syscall.SyscallN(procCryptQueryObject.Addr(), (uintptr)objectType, (uintptr)ᴋ271, (uintptr)expectedContentTypeFlags, (uintptr)expectedFormatTypeFlags, (uintptr)flags, (uintptr)ᴋ272, (uintptr)ᴋ273, (uintptr)ᴋ274, (uintptr)ᴋ275, (uintptr)ᴋ276, (uintptr)ᴋ277);
    System.GC.KeepAlive(ᴋ271);
    System.GC.KeepAlive(ᴋ272);
    System.GC.KeepAlive(ᴋ273);
    System.GC.KeepAlive(ᴋ274);
    System.GC.KeepAlive(ᴋ275);
    System.GC.KeepAlive(ᴋ276);
    System.GC.KeepAlive(ᴋ277);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CryptUnprotectData(ж<DataBlob> dataIn, ж<ж<uint16>> name, ж<DataBlob> optionalEntropy, uintptr reserved, ж<CryptProtectPromptStruct> promptStruct, uint32 flags, ж<DataBlob> dataOut) {
    error err = default;

    var ᴋ278 = dataIn;
    var ᴋ279 = name;
    var ᴋ280 = optionalEntropy;
    var ᴋ281 = promptStruct;
    var ᴋ282 = dataOut;
        var (r1, _, e1) = syscall.SyscallN(procCryptUnprotectData.Addr(), (uintptr)ᴋ278, (uintptr)ᴋ279, (uintptr)ᴋ280, (uintptr)reserved, (uintptr)ᴋ281, (uintptr)flags, (uintptr)ᴋ282);
    System.GC.KeepAlive(ᴋ278);
    System.GC.KeepAlive(ᴋ279);
    System.GC.KeepAlive(ᴋ280);
    System.GC.KeepAlive(ᴋ281);
    System.GC.KeepAlive(ᴋ282);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle store, error err) PFXImportCertStore(ж<CryptDataBlob> pfx, ж<uint16> password, uint32 flags) {
    Handle store = default;
    error err = default;

    var ᴋ283 = pfx;
    var ᴋ284 = password;
        var (r0, _, e1) = syscall.SyscallN(procPFXImportCertStore.Addr(), (uintptr)ᴋ283, (uintptr)ᴋ284, (uintptr)flags);
    System.GC.KeepAlive(ᴋ283);
    System.GC.KeepAlive(ᴋ284);
    store = (Handle)r0;
    if (store == 0) {
        err = errnoErr(e1);
    }
    return (store, err);
}

public static bool /*same*/ DnsNameCompare(ж<uint16> name1, ж<uint16> name2) {
    bool same = default;

    var ᴋ285 = name1;
    var ᴋ286 = name2;
        var (r0, _, _) = syscall.SyscallN(procDnsNameCompare_W.Addr(), (uintptr)ᴋ285, (uintptr)ᴋ286);
    System.GC.KeepAlive(ᴋ285);
    System.GC.KeepAlive(ᴋ286);
    same = r0 != 0;
    return same;
}

public static error /*status*/ DnsQuery(@string name, uint16 qtype, uint32 options, ж<byte> extra, ж<ж<DNSRecord>> qrs, ж<byte> pr) {
    error status = default;

    ж<uint16> _p0 = default;
    (_p0, status) = syscall.UTF16PtrFromString(name);
    if (status != nil) {
        return status;
    }
    return _DnsQuery(_p0, qtype, options, extra, qrs, pr);
}

internal static error /*status*/ _DnsQuery(ж<uint16> name, uint16 qtype, uint32 options, ж<byte> extra, ж<ж<DNSRecord>> qrs, ж<byte> pr) {
    error status = default;

    var ᴋ287 = name;
    var ᴋ288 = extra;
    var ᴋ289 = qrs;
    var ᴋ290 = pr;
        var (r0, _, _) = syscall.SyscallN(procDnsQuery_W.Addr(), (uintptr)ᴋ287, (uintptr)qtype, (uintptr)options, (uintptr)ᴋ288, (uintptr)ᴋ289, (uintptr)ᴋ290);
    System.GC.KeepAlive(ᴋ287);
    System.GC.KeepAlive(ᴋ288);
    System.GC.KeepAlive(ᴋ289);
    System.GC.KeepAlive(ᴋ290);
    if (r0 != 0) {
        status = ((syscall.Errno)r0);
    }
    return status;
}

public static void DnsRecordListFree(ж<DNSRecord> rl, uint32 freetype) {
    var ᴋ291 = rl;
        syscall.SyscallN(procDnsRecordListFree.Addr(), (uintptr)ᴋ291, (uintptr)freetype);
    System.GC.KeepAlive(ᴋ291);
    return;
}

public static error /*ret*/ DwmGetWindowAttribute(HWND hwnd, uint32 attribute, @unsafe.Pointer value, uint32 size) {
    error ret = default;

    var ᴋ292 = value;
        var (r0, _, _) = syscall.SyscallN(procDwmGetWindowAttribute.Addr(), (uintptr)hwnd, (uintptr)attribute, (uintptr)ᴋ292, (uintptr)size);
    System.GC.KeepAlive(ᴋ292);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*ret*/ DwmSetWindowAttribute(HWND hwnd, uint32 attribute, @unsafe.Pointer value, uint32 size) {
    error ret = default;

    var ᴋ293 = value;
        var (r0, _, _) = syscall.SyscallN(procDwmSetWindowAttribute.Addr(), (uintptr)hwnd, (uintptr)attribute, (uintptr)ᴋ293, (uintptr)size);
    System.GC.KeepAlive(ᴋ293);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*errcode*/ CancelMibChangeNotify2(Handle notificationHandle) {
    error errcode = default;

    var (r0, _, _) = syscall.SyscallN(procCancelMibChangeNotify2.Addr(), (uintptr)notificationHandle);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static void FreeMibTable(@unsafe.Pointer memory) {
    var ᴋ294 = memory;
        syscall.SyscallN(procFreeMibTable.Addr(), (uintptr)ᴋ294);
    System.GC.KeepAlive(ᴋ294);
    return;
}

public static error /*errcode*/ GetAdaptersAddresses(uint32 family, uint32 flags, uintptr reserved, ж<IpAdapterAddresses> adapterAddresses, ж<uint32> sizePointer) {
    error errcode = default;

    var ᴋ295 = adapterAddresses;
    var ᴋ296 = sizePointer;
        var (r0, _, _) = syscall.SyscallN(procGetAdaptersAddresses.Addr(), (uintptr)family, (uintptr)flags, (uintptr)reserved, (uintptr)ᴋ295, (uintptr)ᴋ296);
    System.GC.KeepAlive(ᴋ295);
    System.GC.KeepAlive(ᴋ296);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetAdaptersInfo(ж<IpAdapterInfo> ai, ж<uint32> ol) {
    error errcode = default;

    var ᴋ297 = ai;
    var ᴋ298 = ol;
        var (r0, _, _) = syscall.SyscallN(procGetAdaptersInfo.Addr(), (uintptr)ᴋ297, (uintptr)ᴋ298);
    System.GC.KeepAlive(ᴋ297);
    System.GC.KeepAlive(ᴋ298);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

internal static error /*errcode*/ getBestInterfaceEx(@unsafe.Pointer sockaddr, ж<uint32> pdwBestIfIndex) {
    error errcode = default;

    var ᴋ299 = sockaddr;
    var ᴋ300 = pdwBestIfIndex;
        var (r0, _, _) = syscall.SyscallN(procGetBestInterfaceEx.Addr(), (uintptr)ᴋ299, (uintptr)ᴋ300);
    System.GC.KeepAlive(ᴋ299);
    System.GC.KeepAlive(ᴋ300);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetIfEntry(ж<MibIfRow> pIfRow) {
    error errcode = default;

    var ᴋ301 = pIfRow;
        var (r0, _, _) = syscall.SyscallN(procGetIfEntry.Addr(), (uintptr)ᴋ301);
    System.GC.KeepAlive(ᴋ301);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetIfEntry2Ex(uint32 level, ж<MibIfRow2> row) {
    error errcode = default;

    var ᴋ302 = row;
        var (r0, _, _) = syscall.SyscallN(procGetIfEntry2Ex.Addr(), (uintptr)level, (uintptr)ᴋ302);
    System.GC.KeepAlive(ᴋ302);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetIfTable2Ex(uint32 level, ж<ж<MibIfTable2>> table) {
    error errcode = default;

    var ᴋ303 = table;
        var (r0, _, _) = syscall.SyscallN(procGetIfTable2Ex.Addr(), (uintptr)level, (uintptr)ᴋ303);
    System.GC.KeepAlive(ᴋ303);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetIpForwardEntry2(ж<MibIpForwardRow2> row) {
    error errcode = default;

    var ᴋ304 = row;
        var (r0, _, _) = syscall.SyscallN(procGetIpForwardEntry2.Addr(), (uintptr)ᴋ304);
    System.GC.KeepAlive(ᴋ304);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetIpForwardTable2(uint16 family, ж<ж<MibIpForwardTable2>> table) {
    error errcode = default;

    var ᴋ305 = table;
        var (r0, _, _) = syscall.SyscallN(procGetIpForwardTable2.Addr(), (uintptr)family, (uintptr)ᴋ305);
    System.GC.KeepAlive(ᴋ305);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetIpInterfaceEntry(ж<MibIpInterfaceRow> row) {
    error errcode = default;

    var ᴋ306 = row;
        var (r0, _, _) = syscall.SyscallN(procGetIpInterfaceEntry.Addr(), (uintptr)ᴋ306);
    System.GC.KeepAlive(ᴋ306);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetIpInterfaceTable(uint16 family, ж<ж<MibIpInterfaceTable>> table) {
    error errcode = default;

    var ᴋ307 = table;
        var (r0, _, _) = syscall.SyscallN(procGetIpInterfaceTable.Addr(), (uintptr)family, (uintptr)ᴋ307);
    System.GC.KeepAlive(ᴋ307);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetUnicastIpAddressEntry(ж<MibUnicastIpAddressRow> row) {
    error errcode = default;

    var ᴋ308 = row;
        var (r0, _, _) = syscall.SyscallN(procGetUnicastIpAddressEntry.Addr(), (uintptr)ᴋ308);
    System.GC.KeepAlive(ᴋ308);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ GetUnicastIpAddressTable(uint16 family, ж<ж<MibUnicastIpAddressTable>> table) {
    error errcode = default;

    var ᴋ309 = table;
        var (r0, _, _) = syscall.SyscallN(procGetUnicastIpAddressTable.Addr(), (uintptr)family, (uintptr)ᴋ309);
    System.GC.KeepAlive(ᴋ309);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ NotifyIpInterfaceChange(uint16 family, uintptr callback, @unsafe.Pointer callerContext, bool initialNotification, ж<Handle> notificationHandle) {
    error errcode = default;

    uint32 _p0 = default;
    if (initialNotification) {
        _p0 = 1;
    }
    var ᴋ310 = callerContext;
    var ᴋ311 = notificationHandle;
        var (r0, _, _) = syscall.SyscallN(procNotifyIpInterfaceChange.Addr(), (uintptr)family, (uintptr)callback, (uintptr)ᴋ310, (uintptr)_p0, (uintptr)ᴋ311);
    System.GC.KeepAlive(ᴋ310);
    System.GC.KeepAlive(ᴋ311);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ NotifyRouteChange2(uint16 family, uintptr callback, @unsafe.Pointer callerContext, bool initialNotification, ж<Handle> notificationHandle) {
    error errcode = default;

    uint32 _p0 = default;
    if (initialNotification) {
        _p0 = 1;
    }
    var ᴋ312 = callerContext;
    var ᴋ313 = notificationHandle;
        var (r0, _, _) = syscall.SyscallN(procNotifyRouteChange2.Addr(), (uintptr)family, (uintptr)callback, (uintptr)ᴋ312, (uintptr)_p0, (uintptr)ᴋ313);
    System.GC.KeepAlive(ᴋ312);
    System.GC.KeepAlive(ᴋ313);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static error /*errcode*/ NotifyUnicastIpAddressChange(uint16 family, uintptr callback, @unsafe.Pointer callerContext, bool initialNotification, ж<Handle> notificationHandle) {
    error errcode = default;

    uint32 _p0 = default;
    if (initialNotification) {
        _p0 = 1;
    }
    var ᴋ314 = callerContext;
    var ᴋ315 = notificationHandle;
        var (r0, _, _) = syscall.SyscallN(procNotifyUnicastIpAddressChange.Addr(), (uintptr)family, (uintptr)callback, (uintptr)ᴋ314, (uintptr)_p0, (uintptr)ᴋ315);
    System.GC.KeepAlive(ᴋ314);
    System.GC.KeepAlive(ᴋ315);
    if (r0 != 0) {
        errcode = ((syscall.Errno)r0);
    }
    return errcode;
}

public static (uintptr cookie, error err) AddDllDirectory(ж<uint16> path) {
    uintptr cookie = default;
    error err = default;

    var ᴋ316 = path;
        var (r0, _, e1) = syscall.SyscallN(procAddDllDirectory.Addr(), (uintptr)ᴋ316);
    System.GC.KeepAlive(ᴋ316);
    cookie = (uintptr)r0;
    if (cookie == 0) {
        err = errnoErr(e1);
    }
    return (cookie, err);
}

public static error /*err*/ AssignProcessToJobObject(Handle job, Handle process) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procAssignProcessToJobObject.Addr(), (uintptr)job, (uintptr)process);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CancelIo(Handle s) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procCancelIo.Addr(), (uintptr)s);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CancelIoEx(Handle s, ж<Overlapped> o) {
    error err = default;

    var ᴋ317 = o;
        var (r1, _, e1) = syscall.SyscallN(procCancelIoEx.Addr(), (uintptr)s, (uintptr)ᴋ317);
    System.GC.KeepAlive(ᴋ317);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ClearCommBreak(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procClearCommBreak.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ClearCommError(Handle handle, ж<uint32> lpErrors, ж<ComStat> lpStat) {
    error err = default;

    var ᴋ318 = lpErrors;
    var ᴋ319 = lpStat;
        var (r1, _, e1) = syscall.SyscallN(procClearCommError.Addr(), (uintptr)handle, (uintptr)ᴋ318, (uintptr)ᴋ319);
    System.GC.KeepAlive(ᴋ318);
    System.GC.KeepAlive(ᴋ319);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CloseHandle(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procCloseHandle.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static void ClosePseudoConsole(Handle console) {
    syscall.SyscallN(procClosePseudoConsole.Addr(), (uintptr)console);
    return;
}

public static error /*err*/ ConnectNamedPipe(Handle pipe, ж<Overlapped> overlapped) {
    error err = default;

    var ᴋ320 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procConnectNamedPipe.Addr(), (uintptr)pipe, (uintptr)ᴋ320);
    System.GC.KeepAlive(ᴋ320);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CreateDirectory(ж<uint16> path, ж<SecurityAttributes> sa) {
    error err = default;

    var ᴋ321 = path;
    var ᴋ322 = sa;
        var (r1, _, e1) = syscall.SyscallN(procCreateDirectoryW.Addr(), (uintptr)ᴋ321, (uintptr)ᴋ322);
    System.GC.KeepAlive(ᴋ321);
    System.GC.KeepAlive(ᴋ322);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle handle, error err) CreateEventEx(ж<SecurityAttributes> eventAttrs, ж<uint16> name, uint32 flags, uint32 desiredAccess) {
    Handle handle = default;
    error err = default;

    var ᴋ323 = eventAttrs;
    var ᴋ324 = name;
        var (r0, _, e1) = syscall.SyscallN(procCreateEventExW.Addr(), (uintptr)ᴋ323, (uintptr)ᴋ324, (uintptr)flags, (uintptr)desiredAccess);
    System.GC.KeepAlive(ᴋ323);
    System.GC.KeepAlive(ᴋ324);
    handle = (Handle)r0;
    if (handle == 0 || e1 == ERROR_ALREADY_EXISTS) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) CreateEvent(ж<SecurityAttributes> eventAttrs, uint32 manualReset, uint32 initialState, ж<uint16> name) {
    Handle handle = default;
    error err = default;

    var ᴋ325 = eventAttrs;
    var ᴋ326 = name;
        var (r0, _, e1) = syscall.SyscallN(procCreateEventW.Addr(), (uintptr)ᴋ325, (uintptr)manualReset, (uintptr)initialState, (uintptr)ᴋ326);
    System.GC.KeepAlive(ᴋ325);
    System.GC.KeepAlive(ᴋ326);
    handle = (Handle)r0;
    if (handle == 0 || e1 == ERROR_ALREADY_EXISTS) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) CreateFileMapping(Handle fhandle, ж<SecurityAttributes> sa, uint32 prot, uint32 maxSizeHigh, uint32 maxSizeLow, ж<uint16> name) {
    Handle handle = default;
    error err = default;

    var ᴋ327 = sa;
    var ᴋ328 = name;
        var (r0, _, e1) = syscall.SyscallN(procCreateFileMappingW.Addr(), (uintptr)fhandle, (uintptr)ᴋ327, (uintptr)prot, (uintptr)maxSizeHigh, (uintptr)maxSizeLow, (uintptr)ᴋ328);
    System.GC.KeepAlive(ᴋ327);
    System.GC.KeepAlive(ᴋ328);
    handle = (Handle)r0;
    if (handle == 0 || e1 == ERROR_ALREADY_EXISTS) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) CreateFile(ж<uint16> name, uint32 access, uint32 mode, ж<SecurityAttributes> sa, uint32 createmode, uint32 attrs, Handle templatefile) {
    Handle handle = default;
    error err = default;

    var ᴋ329 = name;
    var ᴋ330 = sa;
        var (r0, _, e1) = syscall.SyscallN(procCreateFileW.Addr(), (uintptr)ᴋ329, (uintptr)access, (uintptr)mode, (uintptr)ᴋ330, (uintptr)createmode, (uintptr)attrs, (uintptr)templatefile);
    System.GC.KeepAlive(ᴋ329);
    System.GC.KeepAlive(ᴋ330);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ CreateHardLink(ж<uint16> filename, ж<uint16> existingfilename, uintptr reserved) {
    error err = default;

    var ᴋ331 = filename;
    var ᴋ332 = existingfilename;
        var (r1, _, e1) = syscall.SyscallN(procCreateHardLinkW.Addr(), (uintptr)ᴋ331, (uintptr)ᴋ332, (uintptr)reserved);
    System.GC.KeepAlive(ᴋ331);
    System.GC.KeepAlive(ᴋ332);
    if ((uintptr)(r1 & 0xff) == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle handle, error err) CreateIoCompletionPort(Handle filehandle, Handle cphandle, uintptr key, uint32 threadcnt) {
    Handle handle = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procCreateIoCompletionPort.Addr(), (uintptr)filehandle, (uintptr)cphandle, (uintptr)key, (uintptr)threadcnt);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) CreateJobObject(ж<SecurityAttributes> jobAttr, ж<uint16> name) {
    Handle handle = default;
    error err = default;

    var ᴋ333 = jobAttr;
    var ᴋ334 = name;
        var (r0, _, e1) = syscall.SyscallN(procCreateJobObjectW.Addr(), (uintptr)ᴋ333, (uintptr)ᴋ334);
    System.GC.KeepAlive(ᴋ333);
    System.GC.KeepAlive(ᴋ334);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) CreateMutexEx(ж<SecurityAttributes> mutexAttrs, ж<uint16> name, uint32 flags, uint32 desiredAccess) {
    Handle handle = default;
    error err = default;

    var ᴋ335 = mutexAttrs;
    var ᴋ336 = name;
        var (r0, _, e1) = syscall.SyscallN(procCreateMutexExW.Addr(), (uintptr)ᴋ335, (uintptr)ᴋ336, (uintptr)flags, (uintptr)desiredAccess);
    System.GC.KeepAlive(ᴋ335);
    System.GC.KeepAlive(ᴋ336);
    handle = (Handle)r0;
    if (handle == 0 || e1 == ERROR_ALREADY_EXISTS) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) CreateMutex(ж<SecurityAttributes> mutexAttrs, bool initialOwner, ж<uint16> name) {
    Handle handle = default;
    error err = default;

    uint32 _p0 = default;
    if (initialOwner) {
        _p0 = 1;
    }
    var ᴋ337 = mutexAttrs;
    var ᴋ338 = name;
        var (r0, _, e1) = syscall.SyscallN(procCreateMutexW.Addr(), (uintptr)ᴋ337, (uintptr)_p0, (uintptr)ᴋ338);
    System.GC.KeepAlive(ᴋ337);
    System.GC.KeepAlive(ᴋ338);
    handle = (Handle)r0;
    if (handle == 0 || e1 == ERROR_ALREADY_EXISTS) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) CreateNamedPipe(ж<uint16> name, uint32 flags, uint32 pipeMode, uint32 maxInstances, uint32 outSize, uint32 inSize, uint32 defaultTimeout, ж<SecurityAttributes> sa) {
    Handle handle = default;
    error err = default;

    var ᴋ339 = name;
    var ᴋ340 = sa;
        var (r0, _, e1) = syscall.SyscallN(procCreateNamedPipeW.Addr(), (uintptr)ᴋ339, (uintptr)flags, (uintptr)pipeMode, (uintptr)maxInstances, (uintptr)outSize, (uintptr)inSize, (uintptr)defaultTimeout, (uintptr)ᴋ340);
    System.GC.KeepAlive(ᴋ339);
    System.GC.KeepAlive(ᴋ340);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ CreatePipe(ж<Handle> readhandle, ж<Handle> writehandle, ж<SecurityAttributes> sa, uint32 size) {
    error err = default;

    var ᴋ341 = readhandle;
    var ᴋ342 = writehandle;
    var ᴋ343 = sa;
        var (r1, _, e1) = syscall.SyscallN(procCreatePipe.Addr(), (uintptr)ᴋ341, (uintptr)ᴋ342, (uintptr)ᴋ343, (uintptr)size);
    System.GC.KeepAlive(ᴋ341);
    System.GC.KeepAlive(ᴋ342);
    System.GC.KeepAlive(ᴋ343);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CreateProcess(ж<uint16> appName, ж<uint16> commandLine, ж<SecurityAttributes> procSecurity, ж<SecurityAttributes> threadSecurity, bool inheritHandles, uint32 creationFlags, ж<uint16> env, ж<uint16> currentDir, ж<StartupInfo> startupInfo, ж<ProcessInformation> outProcInfo) {
    error err = default;

    uint32 _p0 = default;
    if (inheritHandles) {
        _p0 = 1;
    }
    var ᴋ344 = appName;
    var ᴋ345 = commandLine;
    var ᴋ346 = procSecurity;
    var ᴋ347 = threadSecurity;
    var ᴋ348 = env;
    var ᴋ349 = currentDir;
    var ᴋ350 = startupInfo;
    var ᴋ351 = outProcInfo;
        var (r1, _, e1) = syscall.SyscallN(procCreateProcessW.Addr(), (uintptr)ᴋ344, (uintptr)ᴋ345, (uintptr)ᴋ346, (uintptr)ᴋ347, (uintptr)_p0, (uintptr)creationFlags, (uintptr)ᴋ348, (uintptr)ᴋ349, (uintptr)ᴋ350, (uintptr)ᴋ351);
    System.GC.KeepAlive(ᴋ344);
    System.GC.KeepAlive(ᴋ345);
    System.GC.KeepAlive(ᴋ346);
    System.GC.KeepAlive(ᴋ347);
    System.GC.KeepAlive(ᴋ348);
    System.GC.KeepAlive(ᴋ349);
    System.GC.KeepAlive(ᴋ350);
    System.GC.KeepAlive(ᴋ351);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*hr*/ createPseudoConsole(uint32 size, Handle @in, Handle @out, uint32 flags, ж<Handle> pconsole) {
    error hr = default;

    var ᴋ352 = pconsole;
        var (r0, _, _) = syscall.SyscallN(procCreatePseudoConsole.Addr(), (uintptr)size, (uintptr)@in, (uintptr)@out, (uintptr)flags, (uintptr)ᴋ352);
    System.GC.KeepAlive(ᴋ352);
    if (r0 != 0) {
        hr = ((syscall.Errno)r0);
    }
    return hr;
}

public static error /*err*/ CreateSymbolicLink(ж<uint16> symlinkfilename, ж<uint16> targetfilename, uint32 flags) {
    error err = default;

    var ᴋ353 = symlinkfilename;
    var ᴋ354 = targetfilename;
        var (r1, _, e1) = syscall.SyscallN(procCreateSymbolicLinkW.Addr(), (uintptr)ᴋ353, (uintptr)ᴋ354, (uintptr)flags);
    System.GC.KeepAlive(ᴋ353);
    System.GC.KeepAlive(ᴋ354);
    if ((uintptr)(r1 & 0xff) == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle handle, error err) CreateToolhelp32Snapshot(uint32 flags, uint32 processId) {
    Handle handle = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procCreateToolhelp32Snapshot.Addr(), (uintptr)flags, (uintptr)processId);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ DefineDosDevice(uint32 flags, ж<uint16> deviceName, ж<uint16> targetPath) {
    error err = default;

    var ᴋ355 = deviceName;
    var ᴋ356 = targetPath;
        var (r1, _, e1) = syscall.SyscallN(procDefineDosDeviceW.Addr(), (uintptr)flags, (uintptr)ᴋ355, (uintptr)ᴋ356);
    System.GC.KeepAlive(ᴋ355);
    System.GC.KeepAlive(ᴋ356);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ DeleteFile(ж<uint16> path) {
    error err = default;

    var ᴋ357 = path;
        var (r1, _, e1) = syscall.SyscallN(procDeleteFileW.Addr(), (uintptr)ᴋ357);
    System.GC.KeepAlive(ᴋ357);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static void deleteProcThreadAttributeList(ж<ProcThreadAttributeList> attrlist) {
    var ᴋ358 = attrlist;
        syscall.SyscallN(procDeleteProcThreadAttributeList.Addr(), (uintptr)ᴋ358);
    System.GC.KeepAlive(ᴋ358);
    return;
}

public static error /*err*/ DeleteVolumeMountPoint(ж<uint16> volumeMountPoint) {
    error err = default;

    var ᴋ359 = volumeMountPoint;
        var (r1, _, e1) = syscall.SyscallN(procDeleteVolumeMountPointW.Addr(), (uintptr)ᴋ359);
    System.GC.KeepAlive(ᴋ359);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ DeviceIoControl(Handle handle, uint32 ioControlCode, ж<byte> inBuffer, uint32 inBufferSize, ж<byte> outBuffer, uint32 outBufferSize, ж<uint32> bytesReturned, ж<Overlapped> overlapped) {
    error err = default;

    var ᴋ360 = inBuffer;
    var ᴋ361 = outBuffer;
    var ᴋ362 = bytesReturned;
    var ᴋ363 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procDeviceIoControl.Addr(), (uintptr)handle, (uintptr)ioControlCode, (uintptr)ᴋ360, (uintptr)inBufferSize, (uintptr)ᴋ361, (uintptr)outBufferSize, (uintptr)ᴋ362, (uintptr)ᴋ363);
    System.GC.KeepAlive(ᴋ360);
    System.GC.KeepAlive(ᴋ361);
    System.GC.KeepAlive(ᴋ362);
    System.GC.KeepAlive(ᴋ363);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ DisconnectNamedPipe(Handle pipe) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procDisconnectNamedPipe.Addr(), (uintptr)pipe);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ DuplicateHandle(Handle hSourceProcessHandle, Handle hSourceHandle, Handle hTargetProcessHandle, ж<Handle> lpTargetHandle, uint32 dwDesiredAccess, bool bInheritHandle, uint32 dwOptions) {
    error err = default;

    uint32 _p0 = default;
    if (bInheritHandle) {
        _p0 = 1;
    }
    var ᴋ364 = lpTargetHandle;
        var (r1, _, e1) = syscall.SyscallN(procDuplicateHandle.Addr(), (uintptr)hSourceProcessHandle, (uintptr)hSourceHandle, (uintptr)hTargetProcessHandle, (uintptr)ᴋ364, (uintptr)dwDesiredAccess, (uintptr)_p0, (uintptr)dwOptions);
    System.GC.KeepAlive(ᴋ364);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ EscapeCommFunction(Handle handle, uint32 dwFunc) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procEscapeCommFunction.Addr(), (uintptr)handle, (uintptr)dwFunc);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static void ExitProcess(uint32 exitcode) {
    syscall.SyscallN(procExitProcess.Addr(), (uintptr)exitcode);
    return;
}

public static (uint32 n, error err) ExpandEnvironmentStrings(ж<uint16> src, ж<uint16> dst, uint32 size) {
    uint32 n = default;
    error err = default;

    var ᴋ365 = src;
    var ᴋ366 = dst;
        var (r0, _, e1) = syscall.SyscallN(procExpandEnvironmentStringsW.Addr(), (uintptr)ᴋ365, (uintptr)ᴋ366, (uintptr)size);
    System.GC.KeepAlive(ᴋ365);
    System.GC.KeepAlive(ᴋ366);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ FindClose(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procFindClose.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ FindCloseChangeNotification(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procFindCloseChangeNotification.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle handle, error err) FindFirstChangeNotification(@string path, bool watchSubtree, uint32 notifyFilter) {
    Handle handle = default;
    error err = default;

    ж<uint16> _p0 = default;
    (_p0, err) = syscall.UTF16PtrFromString(path);
    if (err != nil) {
        return (handle, err);
    }
    return _FindFirstChangeNotification(_p0, watchSubtree, notifyFilter);
}

internal static (Handle handle, error err) _FindFirstChangeNotification(ж<uint16> path, bool watchSubtree, uint32 notifyFilter) {
    Handle handle = default;
    error err = default;

    uint32 _p1 = default;
    if (watchSubtree) {
        _p1 = 1;
    }
    var ᴋ367 = path;
        var (r0, _, e1) = syscall.SyscallN(procFindFirstChangeNotificationW.Addr(), (uintptr)ᴋ367, (uintptr)_p1, (uintptr)notifyFilter);
    System.GC.KeepAlive(ᴋ367);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

internal static (Handle handle, error err) findFirstFile1(ж<uint16> name, ж<win32finddata1> data) {
    Handle handle = default;
    error err = default;

    var ᴋ368 = name;
    var ᴋ369 = data;
        var (r0, _, e1) = syscall.SyscallN(procFindFirstFileW.Addr(), (uintptr)ᴋ368, (uintptr)ᴋ369);
    System.GC.KeepAlive(ᴋ368);
    System.GC.KeepAlive(ᴋ369);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) FindFirstVolumeMountPoint(ж<uint16> rootPathName, ж<uint16> volumeMountPoint, uint32 bufferLength) {
    Handle handle = default;
    error err = default;

    var ᴋ370 = rootPathName;
    var ᴋ371 = volumeMountPoint;
        var (r0, _, e1) = syscall.SyscallN(procFindFirstVolumeMountPointW.Addr(), (uintptr)ᴋ370, (uintptr)ᴋ371, (uintptr)bufferLength);
    System.GC.KeepAlive(ᴋ370);
    System.GC.KeepAlive(ᴋ371);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) FindFirstVolume(ж<uint16> volumeName, uint32 bufferLength) {
    Handle handle = default;
    error err = default;

    var ᴋ372 = volumeName;
        var (r0, _, e1) = syscall.SyscallN(procFindFirstVolumeW.Addr(), (uintptr)ᴋ372, (uintptr)bufferLength);
    System.GC.KeepAlive(ᴋ372);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ FindNextChangeNotification(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procFindNextChangeNotification.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ findNextFile1(Handle handle, ж<win32finddata1> data) {
    error err = default;

    var ᴋ373 = data;
        var (r1, _, e1) = syscall.SyscallN(procFindNextFileW.Addr(), (uintptr)handle, (uintptr)ᴋ373);
    System.GC.KeepAlive(ᴋ373);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ FindNextVolumeMountPoint(Handle findVolumeMountPoint, ж<uint16> volumeMountPoint, uint32 bufferLength) {
    error err = default;

    var ᴋ374 = volumeMountPoint;
        var (r1, _, e1) = syscall.SyscallN(procFindNextVolumeMountPointW.Addr(), (uintptr)findVolumeMountPoint, (uintptr)ᴋ374, (uintptr)bufferLength);
    System.GC.KeepAlive(ᴋ374);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ FindNextVolume(Handle findVolume, ж<uint16> volumeName, uint32 bufferLength) {
    error err = default;

    var ᴋ375 = volumeName;
        var (r1, _, e1) = syscall.SyscallN(procFindNextVolumeW.Addr(), (uintptr)findVolume, (uintptr)ᴋ375, (uintptr)bufferLength);
    System.GC.KeepAlive(ᴋ375);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (Handle resInfo, error err) findResource(Handle module, uintptr name, uintptr resType) {
    Handle resInfo = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procFindResourceW.Addr(), (uintptr)module, (uintptr)name, (uintptr)resType);
    resInfo = (Handle)r0;
    if (resInfo == 0) {
        err = errnoErr(e1);
    }
    return (resInfo, err);
}

public static error /*err*/ FindVolumeClose(Handle findVolume) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procFindVolumeClose.Addr(), (uintptr)findVolume);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ FindVolumeMountPointClose(Handle findVolumeMountPoint) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procFindVolumeMountPointClose.Addr(), (uintptr)findVolumeMountPoint);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ FlushConsoleInputBuffer(Handle console) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procFlushConsoleInputBuffer.Addr(), (uintptr)console);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ FlushFileBuffers(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procFlushFileBuffers.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ FlushViewOfFile(uintptr addr, uintptr length) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procFlushViewOfFile.Addr(), (uintptr)addr, (uintptr)length);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 n, error err) FormatMessage(uint32 flags, uintptr msgsrc, uint32 msgid, uint32 langid, slice<uint16> buf, ж<byte> args) {
    uint32 n = default;
    error err = default;

    ж<uint16> _p0 = default;
    if (len(buf) > 0) {
        _p0 = Ꮡ(buf, 0);
    }
    var ᴋ376 = _p0;
    var ᴋ377 = args;
        var (r0, _, e1) = syscall.SyscallN(procFormatMessageW.Addr(), (uintptr)flags, (uintptr)msgsrc, (uintptr)msgid, (uintptr)langid, (uintptr)ᴋ376, (uintptr)len(buf), (uintptr)ᴋ377);
    System.GC.KeepAlive(ᴋ376);
    System.GC.KeepAlive(ᴋ377);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ FreeEnvironmentStrings(ж<uint16> envs) {
    error err = default;

    var ᴋ378 = envs;
        var (r1, _, e1) = syscall.SyscallN(procFreeEnvironmentStringsW.Addr(), (uintptr)ᴋ378);
    System.GC.KeepAlive(ᴋ378);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ FreeLibrary(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procFreeLibrary.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GenerateConsoleCtrlEvent(uint32 ctrlEvent, uint32 processGroupID) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procGenerateConsoleCtrlEvent.Addr(), (uintptr)ctrlEvent, (uintptr)processGroupID);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static uint32 /*acp*/ GetACP() {
    uint32 acp = default;

    var (r0, _, _) = syscall.SyscallN(procGetACP.Addr());
    acp = (uint32)r0;
    return acp;
}

public static uint32 /*ret*/ GetActiveProcessorCount(uint16 groupNumber) {
    uint32 ret = default;

    var (r0, _, _) = syscall.SyscallN(procGetActiveProcessorCount.Addr(), (uintptr)groupNumber);
    ret = (uint32)r0;
    return ret;
}

public static error /*err*/ GetCommModemStatus(Handle handle, ж<uint32> lpModemStat) {
    error err = default;

    var ᴋ379 = lpModemStat;
        var (r1, _, e1) = syscall.SyscallN(procGetCommModemStatus.Addr(), (uintptr)handle, (uintptr)ᴋ379);
    System.GC.KeepAlive(ᴋ379);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetCommState(Handle handle, ж<DCB> lpDCB) {
    error err = default;

    var ᴋ380 = lpDCB;
        var (r1, _, e1) = syscall.SyscallN(procGetCommState.Addr(), (uintptr)handle, (uintptr)ᴋ380);
    System.GC.KeepAlive(ᴋ380);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetCommTimeouts(Handle handle, ж<CommTimeouts> timeouts) {
    error err = default;

    var ᴋ381 = timeouts;
        var (r1, _, e1) = syscall.SyscallN(procGetCommTimeouts.Addr(), (uintptr)handle, (uintptr)ᴋ381);
    System.GC.KeepAlive(ᴋ381);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static ж<uint16> /*cmd*/ GetCommandLine() {
    ж<uint16> cmd = default;

    var (r0, _, _) = syscall.SyscallN(procGetCommandLineW.Addr());
    cmd = @unsafe.Pointer.To<uint16>((@unsafe.Pointer)r0);
    return cmd;
}

public static error /*err*/ GetComputerNameEx(uint32 nametype, ж<uint16> buf, ж<uint32> n) {
    error err = default;

    var ᴋ382 = buf;
    var ᴋ383 = n;
        var (r1, _, e1) = syscall.SyscallN(procGetComputerNameExW.Addr(), (uintptr)nametype, (uintptr)ᴋ382, (uintptr)ᴋ383);
    System.GC.KeepAlive(ᴋ382);
    System.GC.KeepAlive(ᴋ383);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetComputerName(ж<uint16> buf, ж<uint32> n) {
    error err = default;

    var ᴋ384 = buf;
    var ᴋ385 = n;
        var (r1, _, e1) = syscall.SyscallN(procGetComputerNameW.Addr(), (uintptr)ᴋ384, (uintptr)ᴋ385);
    System.GC.KeepAlive(ᴋ384);
    System.GC.KeepAlive(ᴋ385);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 cp, error err) GetConsoleCP() {
    uint32 cp = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procGetConsoleCP.Addr());
    cp = (uint32)r0;
    if (cp == 0) {
        err = errnoErr(e1);
    }
    return (cp, err);
}

public static error /*err*/ GetConsoleMode(Handle console, ж<uint32> mode) {
    error err = default;

    var ᴋ386 = mode;
        var (r1, _, e1) = syscall.SyscallN(procGetConsoleMode.Addr(), (uintptr)console, (uintptr)ᴋ386);
    System.GC.KeepAlive(ᴋ386);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 cp, error err) GetConsoleOutputCP() {
    uint32 cp = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procGetConsoleOutputCP.Addr());
    cp = (uint32)r0;
    if (cp == 0) {
        err = errnoErr(e1);
    }
    return (cp, err);
}

public static error /*err*/ GetConsoleScreenBufferInfo(Handle console, ж<ConsoleScreenBufferInfo> info) {
    error err = default;

    var ᴋ387 = info;
        var (r1, _, e1) = syscall.SyscallN(procGetConsoleScreenBufferInfo.Addr(), (uintptr)console, (uintptr)ᴋ387);
    System.GC.KeepAlive(ᴋ387);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 n, error err) GetCurrentDirectory(uint32 buflen, ж<uint16> buf) {
    uint32 n = default;
    error err = default;

    var ᴋ388 = buf;
        var (r0, _, e1) = syscall.SyscallN(procGetCurrentDirectoryW.Addr(), (uintptr)buflen, (uintptr)ᴋ388);
    System.GC.KeepAlive(ᴋ388);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static uint32 /*pid*/ GetCurrentProcessId() {
    uint32 pid = default;

    var (r0, _, _) = syscall.SyscallN(procGetCurrentProcessId.Addr());
    pid = (uint32)r0;
    return pid;
}

public static uint32 /*id*/ GetCurrentThreadId() {
    uint32 id = default;

    var (r0, _, _) = syscall.SyscallN(procGetCurrentThreadId.Addr());
    id = (uint32)r0;
    return id;
}

public static error /*err*/ GetDiskFreeSpaceEx(ж<uint16> directoryName, ж<uint64> freeBytesAvailableToCaller, ж<uint64> totalNumberOfBytes, ж<uint64> totalNumberOfFreeBytes) {
    error err = default;

    var ᴋ389 = directoryName;
    var ᴋ390 = freeBytesAvailableToCaller;
    var ᴋ391 = totalNumberOfBytes;
    var ᴋ392 = totalNumberOfFreeBytes;
        var (r1, _, e1) = syscall.SyscallN(procGetDiskFreeSpaceExW.Addr(), (uintptr)ᴋ389, (uintptr)ᴋ390, (uintptr)ᴋ391, (uintptr)ᴋ392);
    System.GC.KeepAlive(ᴋ389);
    System.GC.KeepAlive(ᴋ390);
    System.GC.KeepAlive(ᴋ391);
    System.GC.KeepAlive(ᴋ392);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static uint32 /*driveType*/ GetDriveType(ж<uint16> rootPathName) {
    uint32 driveType = default;

    var ᴋ393 = rootPathName;
        var (r0, _, _) = syscall.SyscallN(procGetDriveTypeW.Addr(), (uintptr)ᴋ393);
    System.GC.KeepAlive(ᴋ393);
    driveType = (uint32)r0;
    return driveType;
}

public static (ж<uint16> envs, error err) GetEnvironmentStrings() {
    ж<uint16> envs = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procGetEnvironmentStringsW.Addr());
    envs = @unsafe.Pointer.To<uint16>((@unsafe.Pointer)r0);
    if (envs == nil) {
        err = errnoErr(e1);
    }
    return (envs, err);
}

public static (uint32 n, error err) GetEnvironmentVariable(ж<uint16> name, ж<uint16> buffer, uint32 size) {
    uint32 n = default;
    error err = default;

    var ᴋ394 = name;
    var ᴋ395 = buffer;
        var (r0, _, e1) = syscall.SyscallN(procGetEnvironmentVariableW.Addr(), (uintptr)ᴋ394, (uintptr)ᴋ395, (uintptr)size);
    System.GC.KeepAlive(ᴋ394);
    System.GC.KeepAlive(ᴋ395);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ GetExitCodeProcess(Handle handle, ж<uint32> exitcode) {
    error err = default;

    var ᴋ396 = exitcode;
        var (r1, _, e1) = syscall.SyscallN(procGetExitCodeProcess.Addr(), (uintptr)handle, (uintptr)ᴋ396);
    System.GC.KeepAlive(ᴋ396);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetFileAttributesEx(ж<uint16> name, uint32 level, ж<byte> info) {
    error err = default;

    var ᴋ397 = name;
    var ᴋ398 = info;
        var (r1, _, e1) = syscall.SyscallN(procGetFileAttributesExW.Addr(), (uintptr)ᴋ397, (uintptr)level, (uintptr)ᴋ398);
    System.GC.KeepAlive(ᴋ397);
    System.GC.KeepAlive(ᴋ398);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 attrs, error err) GetFileAttributes(ж<uint16> name) {
    uint32 attrs = default;
    error err = default;

    var ᴋ399 = name;
        var (r0, _, e1) = syscall.SyscallN(procGetFileAttributesW.Addr(), (uintptr)ᴋ399);
    System.GC.KeepAlive(ᴋ399);
    attrs = (uint32)r0;
    if (attrs == INVALID_FILE_ATTRIBUTES) {
        err = errnoErr(e1);
    }
    return (attrs, err);
}

public static error /*err*/ GetFileInformationByHandle(Handle handle, ж<ByHandleFileInformation> data) {
    error err = default;

    var ᴋ400 = data;
        var (r1, _, e1) = syscall.SyscallN(procGetFileInformationByHandle.Addr(), (uintptr)handle, (uintptr)ᴋ400);
    System.GC.KeepAlive(ᴋ400);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetFileInformationByHandleEx(Handle handle, uint32 @class, ж<byte> outBuffer, uint32 outBufferLen) {
    error err = default;

    var ᴋ401 = outBuffer;
        var (r1, _, e1) = syscall.SyscallN(procGetFileInformationByHandleEx.Addr(), (uintptr)handle, (uintptr)@class, (uintptr)ᴋ401, (uintptr)outBufferLen);
    System.GC.KeepAlive(ᴋ401);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetFileTime(Handle handle, ж<Filetime> ctime, ж<Filetime> atime, ж<Filetime> wtime) {
    error err = default;

    var ᴋ402 = ctime;
    var ᴋ403 = atime;
    var ᴋ404 = wtime;
        var (r1, _, e1) = syscall.SyscallN(procGetFileTime.Addr(), (uintptr)handle, (uintptr)ᴋ402, (uintptr)ᴋ403, (uintptr)ᴋ404);
    System.GC.KeepAlive(ᴋ402);
    System.GC.KeepAlive(ᴋ403);
    System.GC.KeepAlive(ᴋ404);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 n, error err) GetFileType(Handle filehandle) {
    uint32 n = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procGetFileType.Addr(), (uintptr)filehandle);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (uint32 n, error err) GetFinalPathNameByHandle(Handle @file, ж<uint16> filePath, uint32 filePathSize, uint32 flags) {
    uint32 n = default;
    error err = default;

    var ᴋ405 = filePath;
        var (r0, _, e1) = syscall.SyscallN(procGetFinalPathNameByHandleW.Addr(), (uintptr)@file, (uintptr)ᴋ405, (uintptr)filePathSize, (uintptr)flags);
    System.GC.KeepAlive(ᴋ405);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (uint32 n, error err) GetFullPathName(ж<uint16> path, uint32 buflen, ж<uint16> buf, ж<ж<uint16>> fname) {
    uint32 n = default;
    error err = default;

    var ᴋ406 = path;
    var ᴋ407 = buf;
    var ᴋ408 = fname;
        var (r0, _, e1) = syscall.SyscallN(procGetFullPathNameW.Addr(), (uintptr)ᴋ406, (uintptr)buflen, (uintptr)ᴋ407, (uintptr)ᴋ408);
    System.GC.KeepAlive(ᴋ406);
    System.GC.KeepAlive(ᴋ407);
    System.GC.KeepAlive(ᴋ408);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static uintptr /*size*/ GetLargePageMinimum() {
    uintptr size = default;

    var (r0, _, _) = syscall.SyscallN(procGetLargePageMinimum.Addr());
    size = (uintptr)r0;
    return size;
}

public static error /*lasterr*/ GetLastError() {
    error lasterr = default;

    var (r0, _, _) = syscall.SyscallN(procGetLastError.Addr());
    if (r0 != 0) {
        lasterr = ((syscall.Errno)r0);
    }
    return lasterr;
}

public static (uint32 n, error err) GetLogicalDriveStrings(uint32 bufferLength, ж<uint16> buffer) {
    uint32 n = default;
    error err = default;

    var ᴋ409 = buffer;
        var (r0, _, e1) = syscall.SyscallN(procGetLogicalDriveStringsW.Addr(), (uintptr)bufferLength, (uintptr)ᴋ409);
    System.GC.KeepAlive(ᴋ409);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static (uint32 drivesBitMask, error err) GetLogicalDrives() {
    uint32 drivesBitMask = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procGetLogicalDrives.Addr());
    drivesBitMask = (uint32)r0;
    if (drivesBitMask == 0) {
        err = errnoErr(e1);
    }
    return (drivesBitMask, err);
}

public static (uint32 n, error err) GetLongPathName(ж<uint16> path, ж<uint16> buf, uint32 buflen) {
    uint32 n = default;
    error err = default;

    var ᴋ410 = path;
    var ᴋ411 = buf;
        var (r0, _, e1) = syscall.SyscallN(procGetLongPathNameW.Addr(), (uintptr)ᴋ410, (uintptr)ᴋ411, (uintptr)buflen);
    System.GC.KeepAlive(ᴋ410);
    System.GC.KeepAlive(ᴋ411);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static uint32 /*ret*/ GetMaximumProcessorCount(uint16 groupNumber) {
    uint32 ret = default;

    var (r0, _, _) = syscall.SyscallN(procGetMaximumProcessorCount.Addr(), (uintptr)groupNumber);
    ret = (uint32)r0;
    return ret;
}

public static (uint32 n, error err) GetModuleFileName(Handle module, ж<uint16> filename, uint32 size) {
    uint32 n = default;
    error err = default;

    var ᴋ412 = filename;
        var (r0, _, e1) = syscall.SyscallN(procGetModuleFileNameW.Addr(), (uintptr)module, (uintptr)ᴋ412, (uintptr)size);
    System.GC.KeepAlive(ᴋ412);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ GetModuleHandleEx(uint32 flags, ж<uint16> moduleName, ж<Handle> module) {
    error err = default;

    var ᴋ413 = moduleName;
    var ᴋ414 = module;
        var (r1, _, e1) = syscall.SyscallN(procGetModuleHandleExW.Addr(), (uintptr)flags, (uintptr)ᴋ413, (uintptr)ᴋ414);
    System.GC.KeepAlive(ᴋ413);
    System.GC.KeepAlive(ᴋ414);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetNamedPipeClientProcessId(Handle pipe, ж<uint32> clientProcessID) {
    error err = default;

    var ᴋ415 = clientProcessID;
        var (r1, _, e1) = syscall.SyscallN(procGetNamedPipeClientProcessId.Addr(), (uintptr)pipe, (uintptr)ᴋ415);
    System.GC.KeepAlive(ᴋ415);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetNamedPipeHandleState(Handle pipe, ж<uint32> state, ж<uint32> curInstances, ж<uint32> maxCollectionCount, ж<uint32> collectDataTimeout, ж<uint16> userName, uint32 maxUserNameSize) {
    error err = default;

    var ᴋ416 = state;
    var ᴋ417 = curInstances;
    var ᴋ418 = maxCollectionCount;
    var ᴋ419 = collectDataTimeout;
    var ᴋ420 = userName;
        var (r1, _, e1) = syscall.SyscallN(procGetNamedPipeHandleStateW.Addr(), (uintptr)pipe, (uintptr)ᴋ416, (uintptr)ᴋ417, (uintptr)ᴋ418, (uintptr)ᴋ419, (uintptr)ᴋ420, (uintptr)maxUserNameSize);
    System.GC.KeepAlive(ᴋ416);
    System.GC.KeepAlive(ᴋ417);
    System.GC.KeepAlive(ᴋ418);
    System.GC.KeepAlive(ᴋ419);
    System.GC.KeepAlive(ᴋ420);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetNamedPipeInfo(Handle pipe, ж<uint32> flags, ж<uint32> outSize, ж<uint32> inSize, ж<uint32> maxInstances) {
    error err = default;

    var ᴋ421 = flags;
    var ᴋ422 = outSize;
    var ᴋ423 = inSize;
    var ᴋ424 = maxInstances;
        var (r1, _, e1) = syscall.SyscallN(procGetNamedPipeInfo.Addr(), (uintptr)pipe, (uintptr)ᴋ421, (uintptr)ᴋ422, (uintptr)ᴋ423, (uintptr)ᴋ424);
    System.GC.KeepAlive(ᴋ421);
    System.GC.KeepAlive(ᴋ422);
    System.GC.KeepAlive(ᴋ423);
    System.GC.KeepAlive(ᴋ424);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetNamedPipeServerProcessId(Handle pipe, ж<uint32> serverProcessID) {
    error err = default;

    var ᴋ425 = serverProcessID;
        var (r1, _, e1) = syscall.SyscallN(procGetNamedPipeServerProcessId.Addr(), (uintptr)pipe, (uintptr)ᴋ425);
    System.GC.KeepAlive(ᴋ425);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetNumberOfConsoleInputEvents(Handle console, ж<uint32> numevents) {
    error err = default;

    var ᴋ426 = numevents;
        var (r1, _, e1) = syscall.SyscallN(procGetNumberOfConsoleInputEvents.Addr(), (uintptr)console, (uintptr)ᴋ426);
    System.GC.KeepAlive(ᴋ426);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetOverlappedResult(Handle handle, ж<Overlapped> overlapped, ж<uint32> done, bool wait) {
    error err = default;

    uint32 _p0 = default;
    if (wait) {
        _p0 = 1;
    }
    var ᴋ427 = overlapped;
    var ᴋ428 = done;
        var (r1, _, e1) = syscall.SyscallN(procGetOverlappedResult.Addr(), (uintptr)handle, (uintptr)ᴋ427, (uintptr)ᴋ428, (uintptr)_p0);
    System.GC.KeepAlive(ᴋ427);
    System.GC.KeepAlive(ᴋ428);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 ret, error err) GetPriorityClass(Handle process) {
    uint32 ret = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procGetPriorityClass.Addr(), (uintptr)process);
    ret = (uint32)r0;
    if (ret == 0) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

public static (uintptr proc, error err) GetProcAddress(Handle module, @string procname) {
    uintptr proc = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = syscall.BytePtrFromString(procname);
    if (err != nil) {
        return (proc, err);
    }
    return _GetProcAddress(module, _p0);
}

internal static (uintptr proc, error err) _GetProcAddress(Handle module, ж<byte> procname) {
    uintptr proc = default;
    error err = default;

    var ᴋ429 = procname;
        var (r0, _, e1) = syscall.SyscallN(procGetProcAddress.Addr(), (uintptr)module, (uintptr)ᴋ429);
    System.GC.KeepAlive(ᴋ429);
    proc = (uintptr)r0;
    if (proc == 0) {
        err = errnoErr(e1);
    }
    return (proc, err);
}

public static (uint32 id, error err) GetProcessId(Handle process) {
    uint32 id = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procGetProcessId.Addr(), (uintptr)process);
    id = (uint32)r0;
    if (id == 0) {
        err = errnoErr(e1);
    }
    return (id, err);
}

internal static error /*err*/ getProcessPreferredUILanguages(uint32 flags, ж<uint32> numLanguages, ж<uint16> buf, ж<uint32> bufSize) {
    error err = default;

    var ᴋ430 = numLanguages;
    var ᴋ431 = buf;
    var ᴋ432 = bufSize;
        var (r1, _, e1) = syscall.SyscallN(procGetProcessPreferredUILanguages.Addr(), (uintptr)flags, (uintptr)ᴋ430, (uintptr)ᴋ431, (uintptr)ᴋ432);
    System.GC.KeepAlive(ᴋ430);
    System.GC.KeepAlive(ᴋ431);
    System.GC.KeepAlive(ᴋ432);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetProcessShutdownParameters(ж<uint32> level, ж<uint32> flags) {
    error err = default;

    var ᴋ433 = level;
    var ᴋ434 = flags;
        var (r1, _, e1) = syscall.SyscallN(procGetProcessShutdownParameters.Addr(), (uintptr)ᴋ433, (uintptr)ᴋ434);
    System.GC.KeepAlive(ᴋ433);
    System.GC.KeepAlive(ᴋ434);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetProcessTimes(Handle handle, ж<Filetime> creationTime, ж<Filetime> exitTime, ж<Filetime> kernelTime, ж<Filetime> userTime) {
    error err = default;

    var ᴋ435 = creationTime;
    var ᴋ436 = exitTime;
    var ᴋ437 = kernelTime;
    var ᴋ438 = userTime;
        var (r1, _, e1) = syscall.SyscallN(procGetProcessTimes.Addr(), (uintptr)handle, (uintptr)ᴋ435, (uintptr)ᴋ436, (uintptr)ᴋ437, (uintptr)ᴋ438);
    System.GC.KeepAlive(ᴋ435);
    System.GC.KeepAlive(ᴋ436);
    System.GC.KeepAlive(ᴋ437);
    System.GC.KeepAlive(ᴋ438);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static void GetProcessWorkingSetSizeEx(Handle hProcess, ж<uintptr> lpMinimumWorkingSetSize, ж<uintptr> lpMaximumWorkingSetSize, ж<uint32> flags) {
    var ᴋ439 = lpMinimumWorkingSetSize;
    var ᴋ440 = lpMaximumWorkingSetSize;
    var ᴋ441 = flags;
        syscall.SyscallN(procGetProcessWorkingSetSizeEx.Addr(), (uintptr)hProcess, (uintptr)ᴋ439, (uintptr)ᴋ440, (uintptr)ᴋ441);
    System.GC.KeepAlive(ᴋ439);
    System.GC.KeepAlive(ᴋ440);
    System.GC.KeepAlive(ᴋ441);
    return;
}

public static error /*err*/ GetQueuedCompletionStatus(Handle cphandle, ж<uint32> qty, ж<uintptr> key, ж<ж<Overlapped>> overlapped, uint32 timeout) {
    error err = default;

    var ᴋ442 = qty;
    var ᴋ443 = key;
    var ᴋ444 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procGetQueuedCompletionStatus.Addr(), (uintptr)cphandle, (uintptr)ᴋ442, (uintptr)ᴋ443, (uintptr)ᴋ444, (uintptr)timeout);
    System.GC.KeepAlive(ᴋ442);
    System.GC.KeepAlive(ᴋ443);
    System.GC.KeepAlive(ᴋ444);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 n, error err) GetShortPathName(ж<uint16> longpath, ж<uint16> shortpath, uint32 buflen) {
    uint32 n = default;
    error err = default;

    var ᴋ445 = longpath;
    var ᴋ446 = shortpath;
        var (r0, _, e1) = syscall.SyscallN(procGetShortPathNameW.Addr(), (uintptr)ᴋ445, (uintptr)ᴋ446, (uintptr)buflen);
    System.GC.KeepAlive(ᴋ445);
    System.GC.KeepAlive(ᴋ446);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static void getStartupInfo(ж<StartupInfo> startupInfo) {
    var ᴋ447 = startupInfo;
        syscall.SyscallN(procGetStartupInfoW.Addr(), (uintptr)ᴋ447);
    System.GC.KeepAlive(ᴋ447);
    return;
}

public static (Handle handle, error err) GetStdHandle(uint32 stdhandle) {
    Handle handle = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procGetStdHandle.Addr(), (uintptr)stdhandle);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

internal static (uint32 len, error err) getSystemDirectory(ж<uint16> dir, uint32 dirLen) {
    uint32 len = default;
    error err = default;

    var ᴋ448 = dir;
        var (r0, _, e1) = syscall.SyscallN(procGetSystemDirectoryW.Addr(), (uintptr)ᴋ448, (uintptr)dirLen);
    System.GC.KeepAlive(ᴋ448);
    len = (uint32)r0;
    if (len == 0) {
        err = errnoErr(e1);
    }
    return (len, err);
}

internal static error /*err*/ getSystemPreferredUILanguages(uint32 flags, ж<uint32> numLanguages, ж<uint16> buf, ж<uint32> bufSize) {
    error err = default;

    var ᴋ449 = numLanguages;
    var ᴋ450 = buf;
    var ᴋ451 = bufSize;
        var (r1, _, e1) = syscall.SyscallN(procGetSystemPreferredUILanguages.Addr(), (uintptr)flags, (uintptr)ᴋ449, (uintptr)ᴋ450, (uintptr)ᴋ451);
    System.GC.KeepAlive(ᴋ449);
    System.GC.KeepAlive(ᴋ450);
    System.GC.KeepAlive(ᴋ451);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static void GetSystemTimeAsFileTime(ж<Filetime> time) {
    var ᴋ452 = time;
        syscall.SyscallN(procGetSystemTimeAsFileTime.Addr(), (uintptr)ᴋ452);
    System.GC.KeepAlive(ᴋ452);
    return;
}

public static void GetSystemTimePreciseAsFileTime(ж<Filetime> time) {
    var ᴋ453 = time;
        syscall.SyscallN(procGetSystemTimePreciseAsFileTime.Addr(), (uintptr)ᴋ453);
    System.GC.KeepAlive(ᴋ453);
    return;
}

internal static (uint32 len, error err) getSystemWindowsDirectory(ж<uint16> dir, uint32 dirLen) {
    uint32 len = default;
    error err = default;

    var ᴋ454 = dir;
        var (r0, _, e1) = syscall.SyscallN(procGetSystemWindowsDirectoryW.Addr(), (uintptr)ᴋ454, (uintptr)dirLen);
    System.GC.KeepAlive(ᴋ454);
    len = (uint32)r0;
    if (len == 0) {
        err = errnoErr(e1);
    }
    return (len, err);
}

public static (uint32 n, error err) GetTempPath(uint32 buflen, ж<uint16> buf) {
    uint32 n = default;
    error err = default;

    var ᴋ455 = buf;
        var (r0, _, e1) = syscall.SyscallN(procGetTempPathW.Addr(), (uintptr)buflen, (uintptr)ᴋ455);
    System.GC.KeepAlive(ᴋ455);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static error /*err*/ getThreadPreferredUILanguages(uint32 flags, ж<uint32> numLanguages, ж<uint16> buf, ж<uint32> bufSize) {
    error err = default;

    var ᴋ456 = numLanguages;
    var ᴋ457 = buf;
    var ᴋ458 = bufSize;
        var (r1, _, e1) = syscall.SyscallN(procGetThreadPreferredUILanguages.Addr(), (uintptr)flags, (uintptr)ᴋ456, (uintptr)ᴋ457, (uintptr)ᴋ458);
    System.GC.KeepAlive(ᴋ456);
    System.GC.KeepAlive(ᴋ457);
    System.GC.KeepAlive(ᴋ458);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static uint64 /*ms*/ getTickCount64() {
    uint64 ms = default;

    var (r0, _, _) = syscall.SyscallN(procGetTickCount64.Addr());
    ms = (uint64)r0;
    return ms;
}

public static (uint32 rc, error err) GetTimeZoneInformation(ж<Timezoneinformation> tzi) {
    uint32 rc = default;
    error err = default;

    var ᴋ459 = tzi;
        var (r0, _, e1) = syscall.SyscallN(procGetTimeZoneInformation.Addr(), (uintptr)ᴋ459);
    System.GC.KeepAlive(ᴋ459);
    rc = (uint32)r0;
    if (rc == 0xffffffffU) {
        err = errnoErr(e1);
    }
    return (rc, err);
}

internal static error /*err*/ getUserPreferredUILanguages(uint32 flags, ж<uint32> numLanguages, ж<uint16> buf, ж<uint32> bufSize) {
    error err = default;

    var ᴋ460 = numLanguages;
    var ᴋ461 = buf;
    var ᴋ462 = bufSize;
        var (r1, _, e1) = syscall.SyscallN(procGetUserPreferredUILanguages.Addr(), (uintptr)flags, (uintptr)ᴋ460, (uintptr)ᴋ461, (uintptr)ᴋ462);
    System.GC.KeepAlive(ᴋ460);
    System.GC.KeepAlive(ᴋ461);
    System.GC.KeepAlive(ᴋ462);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 ver, error err) GetVersion() {
    uint32 ver = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procGetVersion.Addr());
    ver = (uint32)r0;
    if (ver == 0) {
        err = errnoErr(e1);
    }
    return (ver, err);
}

public static error /*err*/ GetVolumeInformationByHandle(Handle @file, ж<uint16> volumeNameBuffer, uint32 volumeNameSize, ж<uint32> volumeNameSerialNumber, ж<uint32> maximumComponentLength, ж<uint32> fileSystemFlags, ж<uint16> fileSystemNameBuffer, uint32 fileSystemNameSize) {
    error err = default;

    var ᴋ463 = volumeNameBuffer;
    var ᴋ464 = volumeNameSerialNumber;
    var ᴋ465 = maximumComponentLength;
    var ᴋ466 = fileSystemFlags;
    var ᴋ467 = fileSystemNameBuffer;
        var (r1, _, e1) = syscall.SyscallN(procGetVolumeInformationByHandleW.Addr(), (uintptr)@file, (uintptr)ᴋ463, (uintptr)volumeNameSize, (uintptr)ᴋ464, (uintptr)ᴋ465, (uintptr)ᴋ466, (uintptr)ᴋ467, (uintptr)fileSystemNameSize);
    System.GC.KeepAlive(ᴋ463);
    System.GC.KeepAlive(ᴋ464);
    System.GC.KeepAlive(ᴋ465);
    System.GC.KeepAlive(ᴋ466);
    System.GC.KeepAlive(ᴋ467);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetVolumeInformation(ж<uint16> rootPathName, ж<uint16> volumeNameBuffer, uint32 volumeNameSize, ж<uint32> volumeNameSerialNumber, ж<uint32> maximumComponentLength, ж<uint32> fileSystemFlags, ж<uint16> fileSystemNameBuffer, uint32 fileSystemNameSize) {
    error err = default;

    var ᴋ468 = rootPathName;
    var ᴋ469 = volumeNameBuffer;
    var ᴋ470 = volumeNameSerialNumber;
    var ᴋ471 = maximumComponentLength;
    var ᴋ472 = fileSystemFlags;
    var ᴋ473 = fileSystemNameBuffer;
        var (r1, _, e1) = syscall.SyscallN(procGetVolumeInformationW.Addr(), (uintptr)ᴋ468, (uintptr)ᴋ469, (uintptr)volumeNameSize, (uintptr)ᴋ470, (uintptr)ᴋ471, (uintptr)ᴋ472, (uintptr)ᴋ473, (uintptr)fileSystemNameSize);
    System.GC.KeepAlive(ᴋ468);
    System.GC.KeepAlive(ᴋ469);
    System.GC.KeepAlive(ᴋ470);
    System.GC.KeepAlive(ᴋ471);
    System.GC.KeepAlive(ᴋ472);
    System.GC.KeepAlive(ᴋ473);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetVolumeNameForVolumeMountPoint(ж<uint16> volumeMountPoint, ж<uint16> volumeName, uint32 bufferlength) {
    error err = default;

    var ᴋ474 = volumeMountPoint;
    var ᴋ475 = volumeName;
        var (r1, _, e1) = syscall.SyscallN(procGetVolumeNameForVolumeMountPointW.Addr(), (uintptr)ᴋ474, (uintptr)ᴋ475, (uintptr)bufferlength);
    System.GC.KeepAlive(ᴋ474);
    System.GC.KeepAlive(ᴋ475);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetVolumePathName(ж<uint16> fileName, ж<uint16> volumePathName, uint32 bufferLength) {
    error err = default;

    var ᴋ476 = fileName;
    var ᴋ477 = volumePathName;
        var (r1, _, e1) = syscall.SyscallN(procGetVolumePathNameW.Addr(), (uintptr)ᴋ476, (uintptr)ᴋ477, (uintptr)bufferLength);
    System.GC.KeepAlive(ᴋ476);
    System.GC.KeepAlive(ᴋ477);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetVolumePathNamesForVolumeName(ж<uint16> volumeName, ж<uint16> volumePathNames, uint32 bufferLength, ж<uint32> returnLength) {
    error err = default;

    var ᴋ478 = volumeName;
    var ᴋ479 = volumePathNames;
    var ᴋ480 = returnLength;
        var (r1, _, e1) = syscall.SyscallN(procGetVolumePathNamesForVolumeNameW.Addr(), (uintptr)ᴋ478, (uintptr)ᴋ479, (uintptr)bufferLength, (uintptr)ᴋ480);
    System.GC.KeepAlive(ᴋ478);
    System.GC.KeepAlive(ᴋ479);
    System.GC.KeepAlive(ᴋ480);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (uint32 len, error err) getWindowsDirectory(ж<uint16> dir, uint32 dirLen) {
    uint32 len = default;
    error err = default;

    var ᴋ481 = dir;
        var (r0, _, e1) = syscall.SyscallN(procGetWindowsDirectoryW.Addr(), (uintptr)ᴋ481, (uintptr)dirLen);
    System.GC.KeepAlive(ᴋ481);
    len = (uint32)r0;
    if (len == 0) {
        err = errnoErr(e1);
    }
    return (len, err);
}

internal static error /*err*/ initializeProcThreadAttributeList(ж<ProcThreadAttributeList> attrlist, uint32 attrcount, uint32 flags, ж<uintptr> size) {
    error err = default;

    var ᴋ482 = attrlist;
    var ᴋ483 = size;
        var (r1, _, e1) = syscall.SyscallN(procInitializeProcThreadAttributeList.Addr(), (uintptr)ᴋ482, (uintptr)attrcount, (uintptr)flags, (uintptr)ᴋ483);
    System.GC.KeepAlive(ᴋ482);
    System.GC.KeepAlive(ᴋ483);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static bool /*ret*/ IsProcessorFeaturePresent(uint32 ProcessorFeature) {
    bool ret = default;

    var (r0, _, _) = syscall.SyscallN(procIsProcessorFeaturePresent.Addr(), (uintptr)ProcessorFeature);
    ret = r0 != 0;
    return ret;
}

public static error /*err*/ IsWow64Process(Handle handle, ж<bool> ᏑisWow64) {
    error err = default;
    ref var isWow64 = ref ᏑisWow64.DerefOrNull();

    ref var _p0 = ref heap(new uint32(), out var Ꮡ_p0);
    if (isWow64) {
        _p0 = 1;
    }
    var ᴋ484 = Ꮡ_p0;
        var (r1, _, e1) = syscall.SyscallN(procIsWow64Process.Addr(), (uintptr)handle, (uintptr)ᴋ484);
    System.GC.KeepAlive(ᴋ484);
    isWow64 = _p0 != 0;
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ IsWow64Process2(Handle handle, ж<uint16> processMachine, ж<uint16> nativeMachine) {
    error err = default;

    err = procIsWow64Process2.Find();
    if (err != nil) {
        return err;
    }
    var ᴋ485 = processMachine;
    var ᴋ486 = nativeMachine;
        var (r1, _, e1) = syscall.SyscallN(procIsWow64Process2.Addr(), (uintptr)handle, (uintptr)ᴋ485, (uintptr)ᴋ486);
    System.GC.KeepAlive(ᴋ485);
    System.GC.KeepAlive(ᴋ486);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle handle, error err) LoadLibraryEx(@string libname, Handle zero, uintptr flags) {
    Handle handle = default;
    error err = default;

    ж<uint16> _p0 = default;
    (_p0, err) = syscall.UTF16PtrFromString(libname);
    if (err != nil) {
        return (handle, err);
    }
    return _LoadLibraryEx(_p0, zero, flags);
}

internal static (Handle handle, error err) _LoadLibraryEx(ж<uint16> libname, Handle zero, uintptr flags) {
    Handle handle = default;
    error err = default;

    var ᴋ487 = libname;
        var (r0, _, e1) = syscall.SyscallN(procLoadLibraryExW.Addr(), (uintptr)ᴋ487, (uintptr)zero, (uintptr)flags);
    System.GC.KeepAlive(ᴋ487);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) LoadLibrary(@string libname) {
    Handle handle = default;
    error err = default;

    ж<uint16> _p0 = default;
    (_p0, err) = syscall.UTF16PtrFromString(libname);
    if (err != nil) {
        return (handle, err);
    }
    return _LoadLibrary(_p0);
}

internal static (Handle handle, error err) _LoadLibrary(ж<uint16> libname) {
    Handle handle = default;
    error err = default;

    var ᴋ488 = libname;
        var (r0, _, e1) = syscall.SyscallN(procLoadLibraryW.Addr(), (uintptr)ᴋ488);
    System.GC.KeepAlive(ᴋ488);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle resData, error err) LoadResource(Handle module, Handle resInfo) {
    Handle resData = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procLoadResource.Addr(), (uintptr)module, (uintptr)resInfo);
    resData = (Handle)r0;
    if (resData == 0) {
        err = errnoErr(e1);
    }
    return (resData, err);
}

public static (uintptr ptr, error err) LocalAlloc(uint32 flags, uint32 length) {
    uintptr ptr = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procLocalAlloc.Addr(), (uintptr)flags, (uintptr)length);
    ptr = (uintptr)r0;
    if (ptr == 0) {
        err = errnoErr(e1);
    }
    return (ptr, err);
}

public static (Handle handle, error err) LocalFree(Handle hmem) {
    Handle handle = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procLocalFree.Addr(), (uintptr)hmem);
    handle = (Handle)r0;
    if (handle != 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ LockFileEx(Handle @file, uint32 flags, uint32 reserved, uint32 bytesLow, uint32 bytesHigh, ж<Overlapped> overlapped) {
    error err = default;

    var ᴋ489 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procLockFileEx.Addr(), (uintptr)@file, (uintptr)flags, (uintptr)reserved, (uintptr)bytesLow, (uintptr)bytesHigh, (uintptr)ᴋ489);
    System.GC.KeepAlive(ᴋ489);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uintptr addr, error err) LockResource(Handle resData) {
    uintptr addr = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procLockResource.Addr(), (uintptr)resData);
    addr = (uintptr)r0;
    if (addr == 0) {
        err = errnoErr(e1);
    }
    return (addr, err);
}

public static (uintptr addr, error err) MapViewOfFile(Handle handle, uint32 access, uint32 offsetHigh, uint32 offsetLow, uintptr length) {
    uintptr addr = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procMapViewOfFile.Addr(), (uintptr)handle, (uintptr)access, (uintptr)offsetHigh, (uintptr)offsetLow, (uintptr)length);
    addr = (uintptr)r0;
    if (addr == 0) {
        err = errnoErr(e1);
    }
    return (addr, err);
}

public static error /*err*/ Module32First(Handle snapshot, ж<ModuleEntry32> moduleEntry) {
    error err = default;

    var ᴋ490 = moduleEntry;
        var (r1, _, e1) = syscall.SyscallN(procModule32FirstW.Addr(), (uintptr)snapshot, (uintptr)ᴋ490);
    System.GC.KeepAlive(ᴋ490);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Module32Next(Handle snapshot, ж<ModuleEntry32> moduleEntry) {
    error err = default;

    var ᴋ491 = moduleEntry;
        var (r1, _, e1) = syscall.SyscallN(procModule32NextW.Addr(), (uintptr)snapshot, (uintptr)ᴋ491);
    System.GC.KeepAlive(ᴋ491);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ MoveFileEx(ж<uint16> from, ж<uint16> to, uint32 flags) {
    error err = default;

    var ᴋ492 = from;
    var ᴋ493 = to;
        var (r1, _, e1) = syscall.SyscallN(procMoveFileExW.Addr(), (uintptr)ᴋ492, (uintptr)ᴋ493, (uintptr)flags);
    System.GC.KeepAlive(ᴋ492);
    System.GC.KeepAlive(ᴋ493);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ MoveFile(ж<uint16> from, ж<uint16> to) {
    error err = default;

    var ᴋ494 = from;
    var ᴋ495 = to;
        var (r1, _, e1) = syscall.SyscallN(procMoveFileW.Addr(), (uintptr)ᴋ494, (uintptr)ᴋ495);
    System.GC.KeepAlive(ᴋ494);
    System.GC.KeepAlive(ᴋ495);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (int32 nwrite, error err) MultiByteToWideChar(uint32 codePage, uint32 dwFlags, ж<byte> str, int32 nstr, ж<uint16> wchar, int32 nwchar) {
    int32 nwrite = default;
    error err = default;

    var ᴋ496 = str;
    var ᴋ497 = wchar;
        var (r0, _, e1) = syscall.SyscallN(procMultiByteToWideChar.Addr(), (uintptr)codePage, (uintptr)dwFlags, (uintptr)ᴋ496, (uintptr)nstr, (uintptr)ᴋ497, (uintptr)nwchar);
    System.GC.KeepAlive(ᴋ496);
    System.GC.KeepAlive(ᴋ497);
    nwrite = (int32)r0;
    if (nwrite == 0) {
        err = errnoErr(e1);
    }
    return (nwrite, err);
}

public static (Handle handle, error err) OpenEvent(uint32 desiredAccess, bool inheritHandle, ж<uint16> name) {
    Handle handle = default;
    error err = default;

    uint32 _p0 = default;
    if (inheritHandle) {
        _p0 = 1;
    }
    var ᴋ498 = name;
        var (r0, _, e1) = syscall.SyscallN(procOpenEventW.Addr(), (uintptr)desiredAccess, (uintptr)_p0, (uintptr)ᴋ498);
    System.GC.KeepAlive(ᴋ498);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) OpenMutex(uint32 desiredAccess, bool inheritHandle, ж<uint16> name) {
    Handle handle = default;
    error err = default;

    uint32 _p0 = default;
    if (inheritHandle) {
        _p0 = 1;
    }
    var ᴋ499 = name;
        var (r0, _, e1) = syscall.SyscallN(procOpenMutexW.Addr(), (uintptr)desiredAccess, (uintptr)_p0, (uintptr)ᴋ499);
    System.GC.KeepAlive(ᴋ499);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) OpenProcess(uint32 desiredAccess, bool inheritHandle, uint32 processId) {
    Handle handle = default;
    error err = default;

    uint32 _p0 = default;
    if (inheritHandle) {
        _p0 = 1;
    }
    var (r0, _, e1) = syscall.SyscallN(procOpenProcess.Addr(), (uintptr)desiredAccess, (uintptr)_p0, (uintptr)processId);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static (Handle handle, error err) OpenThread(uint32 desiredAccess, bool inheritHandle, uint32 threadId) {
    Handle handle = default;
    error err = default;

    uint32 _p0 = default;
    if (inheritHandle) {
        _p0 = 1;
    }
    var (r0, _, e1) = syscall.SyscallN(procOpenThread.Addr(), (uintptr)desiredAccess, (uintptr)_p0, (uintptr)threadId);
    handle = (Handle)r0;
    if (handle == 0) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ PostQueuedCompletionStatus(Handle cphandle, uint32 qty, uintptr key, ж<Overlapped> overlapped) {
    error err = default;

    var ᴋ500 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procPostQueuedCompletionStatus.Addr(), (uintptr)cphandle, (uintptr)qty, (uintptr)key, (uintptr)ᴋ500);
    System.GC.KeepAlive(ᴋ500);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Process32First(Handle snapshot, ж<ProcessEntry32> procEntry) {
    error err = default;

    var ᴋ501 = procEntry;
        var (r1, _, e1) = syscall.SyscallN(procProcess32FirstW.Addr(), (uintptr)snapshot, (uintptr)ᴋ501);
    System.GC.KeepAlive(ᴋ501);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Process32Next(Handle snapshot, ж<ProcessEntry32> procEntry) {
    error err = default;

    var ᴋ502 = procEntry;
        var (r1, _, e1) = syscall.SyscallN(procProcess32NextW.Addr(), (uintptr)snapshot, (uintptr)ᴋ502);
    System.GC.KeepAlive(ᴋ502);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ProcessIdToSessionId(uint32 pid, ж<uint32> sessionid) {
    error err = default;

    var ᴋ503 = sessionid;
        var (r1, _, e1) = syscall.SyscallN(procProcessIdToSessionId.Addr(), (uintptr)pid, (uintptr)ᴋ503);
    System.GC.KeepAlive(ᴋ503);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ PulseEvent(Handle @event) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procPulseEvent.Addr(), (uintptr)@event);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ PurgeComm(Handle handle, uint32 dwFlags) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procPurgeComm.Addr(), (uintptr)handle, (uintptr)dwFlags);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 n, error err) QueryDosDevice(ж<uint16> deviceName, ж<uint16> targetPath, uint32 max) {
    uint32 n = default;
    error err = default;

    var ᴋ504 = deviceName;
    var ᴋ505 = targetPath;
        var (r0, _, e1) = syscall.SyscallN(procQueryDosDeviceW.Addr(), (uintptr)ᴋ504, (uintptr)ᴋ505, (uintptr)max);
    System.GC.KeepAlive(ᴋ504);
    System.GC.KeepAlive(ᴋ505);
    n = (uint32)r0;
    if (n == 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ QueryFullProcessImageName(Handle proc, uint32 flags, ж<uint16> exeName, ж<uint32> size) {
    error err = default;

    var ᴋ506 = exeName;
    var ᴋ507 = size;
        var (r1, _, e1) = syscall.SyscallN(procQueryFullProcessImageNameW.Addr(), (uintptr)proc, (uintptr)flags, (uintptr)ᴋ506, (uintptr)ᴋ507);
    System.GC.KeepAlive(ᴋ506);
    System.GC.KeepAlive(ᴋ507);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ QueryInformationJobObject(Handle job, int32 JobObjectInformationClass, uintptr JobObjectInformation, uint32 JobObjectInformationLength, ж<uint32> retlen) {
    error err = default;

    var ᴋ508 = retlen;
        var (r1, _, e1) = syscall.SyscallN(procQueryInformationJobObject.Addr(), (uintptr)job, (uintptr)JobObjectInformationClass, (uintptr)JobObjectInformation, (uintptr)JobObjectInformationLength, (uintptr)ᴋ508);
    System.GC.KeepAlive(ᴋ508);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ReadConsole(Handle console, ж<uint16> buf, uint32 toread, ж<uint32> read, ж<byte> inputControl) {
    error err = default;

    var ᴋ509 = buf;
    var ᴋ510 = read;
    var ᴋ511 = inputControl;
        var (r1, _, e1) = syscall.SyscallN(procReadConsoleW.Addr(), (uintptr)console, (uintptr)ᴋ509, (uintptr)toread, (uintptr)ᴋ510, (uintptr)ᴋ511);
    System.GC.KeepAlive(ᴋ509);
    System.GC.KeepAlive(ᴋ510);
    System.GC.KeepAlive(ᴋ511);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ReadDirectoryChanges(Handle handle, ж<byte> buf, uint32 buflen, bool watchSubTree, uint32 mask, ж<uint32> retlen, ж<Overlapped> overlapped, uintptr completionRoutine) {
    error err = default;

    uint32 _p0 = default;
    if (watchSubTree) {
        _p0 = 1;
    }
    var ᴋ512 = buf;
    var ᴋ513 = retlen;
    var ᴋ514 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procReadDirectoryChangesW.Addr(), (uintptr)handle, (uintptr)ᴋ512, (uintptr)buflen, (uintptr)_p0, (uintptr)mask, (uintptr)ᴋ513, (uintptr)ᴋ514, (uintptr)completionRoutine);
    System.GC.KeepAlive(ᴋ512);
    System.GC.KeepAlive(ᴋ513);
    System.GC.KeepAlive(ᴋ514);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ readFile(Handle handle, slice<byte> buf, ж<uint32> done, ж<Overlapped> overlapped) {
    error err = default;

    ж<byte> _p0 = default;
    if (len(buf) > 0) {
        _p0 = Ꮡ(buf, 0);
    }
    var ᴋ515 = _p0;
    var ᴋ516 = done;
    var ᴋ517 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procReadFile.Addr(), (uintptr)handle, (uintptr)ᴋ515, (uintptr)len(buf), (uintptr)ᴋ516, (uintptr)ᴋ517);
    System.GC.KeepAlive(ᴋ515);
    System.GC.KeepAlive(ᴋ516);
    System.GC.KeepAlive(ᴋ517);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ReadProcessMemory(Handle process, uintptr baseAddress, ж<byte> buffer, uintptr size, ж<uintptr> numberOfBytesRead) {
    error err = default;

    var ᴋ518 = buffer;
    var ᴋ519 = numberOfBytesRead;
        var (r1, _, e1) = syscall.SyscallN(procReadProcessMemory.Addr(), (uintptr)process, (uintptr)baseAddress, (uintptr)ᴋ518, (uintptr)size, (uintptr)ᴋ519);
    System.GC.KeepAlive(ᴋ518);
    System.GC.KeepAlive(ᴋ519);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ReleaseMutex(Handle mutex) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procReleaseMutex.Addr(), (uintptr)mutex);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ RemoveDirectory(ж<uint16> path) {
    error err = default;

    var ᴋ520 = path;
        var (r1, _, e1) = syscall.SyscallN(procRemoveDirectoryW.Addr(), (uintptr)ᴋ520);
    System.GC.KeepAlive(ᴋ520);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ RemoveDllDirectory(uintptr cookie) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procRemoveDllDirectory.Addr(), (uintptr)cookie);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ResetEvent(Handle @event) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procResetEvent.Addr(), (uintptr)@event);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*hr*/ resizePseudoConsole(Handle pconsole, uint32 size) {
    error hr = default;

    var (r0, _, _) = syscall.SyscallN(procResizePseudoConsole.Addr(), (uintptr)pconsole, (uintptr)size);
    if (r0 != 0) {
        hr = ((syscall.Errno)r0);
    }
    return hr;
}

public static (uint32 ret, error err) ResumeThread(Handle thread) {
    uint32 ret = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procResumeThread.Addr(), (uintptr)thread);
    ret = (uint32)r0;
    if (ret == 0xffffffffU) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

public static error /*err*/ SetCommBreak(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetCommBreak.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetCommMask(Handle handle, uint32 dwEvtMask) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetCommMask.Addr(), (uintptr)handle, (uintptr)dwEvtMask);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetCommState(Handle handle, ж<DCB> lpDCB) {
    error err = default;

    var ᴋ521 = lpDCB;
        var (r1, _, e1) = syscall.SyscallN(procSetCommState.Addr(), (uintptr)handle, (uintptr)ᴋ521);
    System.GC.KeepAlive(ᴋ521);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetCommTimeouts(Handle handle, ж<CommTimeouts> timeouts) {
    error err = default;

    var ᴋ522 = timeouts;
        var (r1, _, e1) = syscall.SyscallN(procSetCommTimeouts.Addr(), (uintptr)handle, (uintptr)ᴋ522);
    System.GC.KeepAlive(ᴋ522);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetConsoleCP(uint32 cp) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetConsoleCP.Addr(), (uintptr)cp);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setConsoleCursorPosition(Handle console, uint32 position) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetConsoleCursorPosition.Addr(), (uintptr)console, (uintptr)position);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetConsoleMode(Handle console, uint32 mode) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetConsoleMode.Addr(), (uintptr)console, (uintptr)mode);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetConsoleOutputCP(uint32 cp) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetConsoleOutputCP.Addr(), (uintptr)cp);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetCurrentDirectory(ж<uint16> path) {
    error err = default;

    var ᴋ523 = path;
        var (r1, _, e1) = syscall.SyscallN(procSetCurrentDirectoryW.Addr(), (uintptr)ᴋ523);
    System.GC.KeepAlive(ᴋ523);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetDefaultDllDirectories(uint32 directoryFlags) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetDefaultDllDirectories.Addr(), (uintptr)directoryFlags);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetDllDirectory(@string path) {
    error err = default;

    ж<uint16> _p0 = default;
    (_p0, err) = syscall.UTF16PtrFromString(path);
    if (err != nil) {
        return err;
    }
    return _SetDllDirectory(_p0);
}

internal static error /*err*/ _SetDllDirectory(ж<uint16> path) {
    error err = default;

    var ᴋ524 = path;
        var (r1, _, e1) = syscall.SyscallN(procSetDllDirectoryW.Addr(), (uintptr)ᴋ524);
    System.GC.KeepAlive(ᴋ524);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetEndOfFile(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetEndOfFile.Addr(), (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetEnvironmentVariable(ж<uint16> name, ж<uint16> value) {
    error err = default;

    var ᴋ525 = name;
    var ᴋ526 = value;
        var (r1, _, e1) = syscall.SyscallN(procSetEnvironmentVariableW.Addr(), (uintptr)ᴋ525, (uintptr)ᴋ526);
    System.GC.KeepAlive(ᴋ525);
    System.GC.KeepAlive(ᴋ526);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static uint32 /*ret*/ SetErrorMode(uint32 mode) {
    uint32 ret = default;

    var (r0, _, _) = syscall.SyscallN(procSetErrorMode.Addr(), (uintptr)mode);
    ret = (uint32)r0;
    return ret;
}

public static error /*err*/ SetEvent(Handle @event) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetEvent.Addr(), (uintptr)@event);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetFileAttributes(ж<uint16> name, uint32 attrs) {
    error err = default;

    var ᴋ527 = name;
        var (r1, _, e1) = syscall.SyscallN(procSetFileAttributesW.Addr(), (uintptr)ᴋ527, (uintptr)attrs);
    System.GC.KeepAlive(ᴋ527);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetFileCompletionNotificationModes(Handle handle, uint8 flags) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetFileCompletionNotificationModes.Addr(), (uintptr)handle, (uintptr)flags);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetFileInformationByHandle(Handle handle, uint32 @class, ж<byte> inBuffer, uint32 inBufferLen) {
    error err = default;

    var ᴋ528 = inBuffer;
        var (r1, _, e1) = syscall.SyscallN(procSetFileInformationByHandle.Addr(), (uintptr)handle, (uintptr)@class, (uintptr)ᴋ528, (uintptr)inBufferLen);
    System.GC.KeepAlive(ᴋ528);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 newlowoffset, error err) SetFilePointer(Handle handle, int32 lowoffset, ж<int32> highoffsetptr, uint32 whence) {
    uint32 newlowoffset = default;
    error err = default;

    var ᴋ529 = highoffsetptr;
        var (r0, _, e1) = syscall.SyscallN(procSetFilePointer.Addr(), (uintptr)handle, (uintptr)lowoffset, (uintptr)ᴋ529, (uintptr)whence);
    System.GC.KeepAlive(ᴋ529);
    newlowoffset = (uint32)r0;
    if (newlowoffset == 0xffffffffU) {
        err = errnoErr(e1);
    }
    return (newlowoffset, err);
}

public static error /*err*/ SetFileTime(Handle handle, ж<Filetime> ctime, ж<Filetime> atime, ж<Filetime> wtime) {
    error err = default;

    var ᴋ530 = ctime;
    var ᴋ531 = atime;
    var ᴋ532 = wtime;
        var (r1, _, e1) = syscall.SyscallN(procSetFileTime.Addr(), (uintptr)handle, (uintptr)ᴋ530, (uintptr)ᴋ531, (uintptr)ᴋ532);
    System.GC.KeepAlive(ᴋ530);
    System.GC.KeepAlive(ᴋ531);
    System.GC.KeepAlive(ᴋ532);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetFileValidData(Handle handle, int64 validDataLength) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetFileValidData.Addr(), (uintptr)handle, (uintptr)validDataLength);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetHandleInformation(Handle handle, uint32 mask, uint32 flags) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetHandleInformation.Addr(), (uintptr)handle, (uintptr)mask, (uintptr)flags);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (nint ret, error err) SetInformationJobObject(Handle job, uint32 JobObjectInformationClass, uintptr JobObjectInformation, uint32 JobObjectInformationLength) {
    nint ret = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procSetInformationJobObject.Addr(), (uintptr)job, (uintptr)JobObjectInformationClass, (uintptr)JobObjectInformation, (uintptr)JobObjectInformationLength);
    ret = (nint)r0;
    if (ret == 0) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

public static error /*err*/ SetNamedPipeHandleState(Handle pipe, ж<uint32> state, ж<uint32> maxCollectionCount, ж<uint32> collectDataTimeout) {
    error err = default;

    var ᴋ533 = state;
    var ᴋ534 = maxCollectionCount;
    var ᴋ535 = collectDataTimeout;
        var (r1, _, e1) = syscall.SyscallN(procSetNamedPipeHandleState.Addr(), (uintptr)pipe, (uintptr)ᴋ533, (uintptr)ᴋ534, (uintptr)ᴋ535);
    System.GC.KeepAlive(ᴋ533);
    System.GC.KeepAlive(ᴋ534);
    System.GC.KeepAlive(ᴋ535);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetPriorityClass(Handle process, uint32 priorityClass) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetPriorityClass.Addr(), (uintptr)process, (uintptr)priorityClass);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetProcessPriorityBoost(Handle process, bool disable) {
    error err = default;

    uint32 _p0 = default;
    if (disable) {
        _p0 = 1;
    }
    var (r1, _, e1) = syscall.SyscallN(procSetProcessPriorityBoost.Addr(), (uintptr)process, (uintptr)_p0);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetProcessShutdownParameters(uint32 level, uint32 flags) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetProcessShutdownParameters.Addr(), (uintptr)level, (uintptr)flags);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetProcessWorkingSetSizeEx(Handle hProcess, uintptr dwMinimumWorkingSetSize, uintptr dwMaximumWorkingSetSize, uint32 flags) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetProcessWorkingSetSizeEx.Addr(), (uintptr)hProcess, (uintptr)dwMinimumWorkingSetSize, (uintptr)dwMaximumWorkingSetSize, (uintptr)flags);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetStdHandle(uint32 stdhandle, Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetStdHandle.Addr(), (uintptr)stdhandle, (uintptr)handle);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetVolumeLabel(ж<uint16> rootPathName, ж<uint16> volumeName) {
    error err = default;

    var ᴋ536 = rootPathName;
    var ᴋ537 = volumeName;
        var (r1, _, e1) = syscall.SyscallN(procSetVolumeLabelW.Addr(), (uintptr)ᴋ536, (uintptr)ᴋ537);
    System.GC.KeepAlive(ᴋ536);
    System.GC.KeepAlive(ᴋ537);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetVolumeMountPoint(ж<uint16> volumeMountPoint, ж<uint16> volumeName) {
    error err = default;

    var ᴋ538 = volumeMountPoint;
    var ᴋ539 = volumeName;
        var (r1, _, e1) = syscall.SyscallN(procSetVolumeMountPointW.Addr(), (uintptr)ᴋ538, (uintptr)ᴋ539);
    System.GC.KeepAlive(ᴋ538);
    System.GC.KeepAlive(ᴋ539);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetupComm(Handle handle, uint32 dwInQueue, uint32 dwOutQueue) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetupComm.Addr(), (uintptr)handle, (uintptr)dwInQueue, (uintptr)dwOutQueue);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 size, error err) SizeofResource(Handle module, Handle resInfo) {
    uint32 size = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procSizeofResource.Addr(), (uintptr)module, (uintptr)resInfo);
    size = (uint32)r0;
    if (size == 0) {
        err = errnoErr(e1);
    }
    return (size, err);
}

public static uint32 /*ret*/ SleepEx(uint32 milliseconds, bool alertable) {
    uint32 ret = default;

    uint32 _p0 = default;
    if (alertable) {
        _p0 = 1;
    }
    var (r0, _, _) = syscall.SyscallN(procSleepEx.Addr(), (uintptr)milliseconds, (uintptr)_p0);
    ret = (uint32)r0;
    return ret;
}

public static error /*err*/ TerminateJobObject(Handle job, uint32 exitCode) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procTerminateJobObject.Addr(), (uintptr)job, (uintptr)exitCode);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ TerminateProcess(Handle handle, uint32 exitcode) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procTerminateProcess.Addr(), (uintptr)handle, (uintptr)exitcode);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Thread32First(Handle snapshot, ж<ThreadEntry32> threadEntry) {
    error err = default;

    var ᴋ540 = threadEntry;
        var (r1, _, e1) = syscall.SyscallN(procThread32First.Addr(), (uintptr)snapshot, (uintptr)ᴋ540);
    System.GC.KeepAlive(ᴋ540);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Thread32Next(Handle snapshot, ж<ThreadEntry32> threadEntry) {
    error err = default;

    var ᴋ541 = threadEntry;
        var (r1, _, e1) = syscall.SyscallN(procThread32Next.Addr(), (uintptr)snapshot, (uintptr)ᴋ541);
    System.GC.KeepAlive(ᴋ541);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ UnlockFileEx(Handle @file, uint32 reserved, uint32 bytesLow, uint32 bytesHigh, ж<Overlapped> overlapped) {
    error err = default;

    var ᴋ542 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procUnlockFileEx.Addr(), (uintptr)@file, (uintptr)reserved, (uintptr)bytesLow, (uintptr)bytesHigh, (uintptr)ᴋ542);
    System.GC.KeepAlive(ᴋ542);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ UnmapViewOfFile(uintptr addr) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procUnmapViewOfFile.Addr(), (uintptr)addr);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ updateProcThreadAttribute(ж<ProcThreadAttributeList> attrlist, uint32 flags, uintptr attr, @unsafe.Pointer value, uintptr size, @unsafe.Pointer prevvalue, ж<uintptr> returnedsize) {
    error err = default;

    var ᴋ543 = attrlist;
    var ᴋ544 = value;
    var ᴋ545 = prevvalue;
    var ᴋ546 = returnedsize;
        var (r1, _, e1) = syscall.SyscallN(procUpdateProcThreadAttribute.Addr(), (uintptr)ᴋ543, (uintptr)flags, (uintptr)attr, (uintptr)ᴋ544, (uintptr)size, (uintptr)ᴋ545, (uintptr)ᴋ546);
    System.GC.KeepAlive(ᴋ543);
    System.GC.KeepAlive(ᴋ544);
    System.GC.KeepAlive(ᴋ545);
    System.GC.KeepAlive(ᴋ546);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uintptr value, error err) VirtualAlloc(uintptr address, uintptr size, uint32 alloctype, uint32 protect) {
    uintptr value = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procVirtualAlloc.Addr(), (uintptr)address, (uintptr)size, (uintptr)alloctype, (uintptr)protect);
    value = (uintptr)r0;
    if (value == 0) {
        err = errnoErr(e1);
    }
    return (value, err);
}

public static error /*err*/ VirtualFree(uintptr address, uintptr size, uint32 freetype) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procVirtualFree.Addr(), (uintptr)address, (uintptr)size, (uintptr)freetype);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ VirtualLock(uintptr addr, uintptr length) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procVirtualLock.Addr(), (uintptr)addr, (uintptr)length);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ VirtualProtect(uintptr address, uintptr size, uint32 newprotect, ж<uint32> oldprotect) {
    error err = default;

    var ᴋ547 = oldprotect;
        var (r1, _, e1) = syscall.SyscallN(procVirtualProtect.Addr(), (uintptr)address, (uintptr)size, (uintptr)newprotect, (uintptr)ᴋ547);
    System.GC.KeepAlive(ᴋ547);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ VirtualProtectEx(Handle process, uintptr address, uintptr size, uint32 newProtect, ж<uint32> oldProtect) {
    error err = default;

    var ᴋ548 = oldProtect;
        var (r1, _, e1) = syscall.SyscallN(procVirtualProtectEx.Addr(), (uintptr)process, (uintptr)address, (uintptr)size, (uintptr)newProtect, (uintptr)ᴋ548);
    System.GC.KeepAlive(ᴋ548);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ VirtualQuery(uintptr address, ж<MemoryBasicInformation> buffer, uintptr length) {
    error err = default;

    var ᴋ549 = buffer;
        var (r1, _, e1) = syscall.SyscallN(procVirtualQuery.Addr(), (uintptr)address, (uintptr)ᴋ549, (uintptr)length);
    System.GC.KeepAlive(ᴋ549);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ VirtualQueryEx(Handle process, uintptr address, ж<MemoryBasicInformation> buffer, uintptr length) {
    error err = default;

    var ᴋ550 = buffer;
        var (r1, _, e1) = syscall.SyscallN(procVirtualQueryEx.Addr(), (uintptr)process, (uintptr)address, (uintptr)ᴋ550, (uintptr)length);
    System.GC.KeepAlive(ᴋ550);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ VirtualUnlock(uintptr addr, uintptr length) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procVirtualUnlock.Addr(), (uintptr)addr, (uintptr)length);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static uint32 /*sessionID*/ WTSGetActiveConsoleSessionId() {
    uint32 sessionID = default;

    var (r0, _, _) = syscall.SyscallN(procWTSGetActiveConsoleSessionId.Addr());
    sessionID = (uint32)r0;
    return sessionID;
}

public static error /*err*/ WaitCommEvent(Handle handle, ж<uint32> lpEvtMask, ж<Overlapped> lpOverlapped) {
    error err = default;

    var ᴋ551 = lpEvtMask;
    var ᴋ552 = lpOverlapped;
        var (r1, _, e1) = syscall.SyscallN(procWaitCommEvent.Addr(), (uintptr)handle, (uintptr)ᴋ551, (uintptr)ᴋ552);
    System.GC.KeepAlive(ᴋ551);
    System.GC.KeepAlive(ᴋ552);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (uint32 @event, error err) waitForMultipleObjects(uint32 count, uintptr handles, bool waitAll, uint32 waitMilliseconds) {
    uint32 @event = default;
    error err = default;

    uint32 _p0 = default;
    if (waitAll) {
        _p0 = 1;
    }
    var (r0, _, e1) = syscall.SyscallN(procWaitForMultipleObjects.Addr(), (uintptr)count, (uintptr)handles, (uintptr)_p0, (uintptr)waitMilliseconds);
    @event = (uint32)r0;
    if (@event == 0xffffffffU) {
        err = errnoErr(e1);
    }
    return (@event, err);
}

public static (uint32 @event, error err) WaitForSingleObject(Handle handle, uint32 waitMilliseconds) {
    uint32 @event = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procWaitForSingleObject.Addr(), (uintptr)handle, (uintptr)waitMilliseconds);
    @event = (uint32)r0;
    if (@event == 0xffffffffU) {
        err = errnoErr(e1);
    }
    return (@event, err);
}

public static error /*err*/ WriteConsole(Handle console, ж<uint16> buf, uint32 towrite, ж<uint32> written, ж<byte> reserved) {
    error err = default;

    var ᴋ553 = buf;
    var ᴋ554 = written;
    var ᴋ555 = reserved;
        var (r1, _, e1) = syscall.SyscallN(procWriteConsoleW.Addr(), (uintptr)console, (uintptr)ᴋ553, (uintptr)towrite, (uintptr)ᴋ554, (uintptr)ᴋ555);
    System.GC.KeepAlive(ᴋ553);
    System.GC.KeepAlive(ᴋ554);
    System.GC.KeepAlive(ᴋ555);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ writeFile(Handle handle, slice<byte> buf, ж<uint32> done, ж<Overlapped> overlapped) {
    error err = default;

    ж<byte> _p0 = default;
    if (len(buf) > 0) {
        _p0 = Ꮡ(buf, 0);
    }
    var ᴋ556 = _p0;
    var ᴋ557 = done;
    var ᴋ558 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procWriteFile.Addr(), (uintptr)handle, (uintptr)ᴋ556, (uintptr)len(buf), (uintptr)ᴋ557, (uintptr)ᴋ558);
    System.GC.KeepAlive(ᴋ556);
    System.GC.KeepAlive(ᴋ557);
    System.GC.KeepAlive(ᴋ558);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WriteProcessMemory(Handle process, uintptr baseAddress, ж<byte> buffer, uintptr size, ж<uintptr> numberOfBytesWritten) {
    error err = default;

    var ᴋ559 = buffer;
    var ᴋ560 = numberOfBytesWritten;
        var (r1, _, e1) = syscall.SyscallN(procWriteProcessMemory.Addr(), (uintptr)process, (uintptr)baseAddress, (uintptr)ᴋ559, (uintptr)size, (uintptr)ᴋ560);
    System.GC.KeepAlive(ᴋ559);
    System.GC.KeepAlive(ᴋ560);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ AcceptEx(Handle ls, Handle @as, ж<byte> buf, uint32 rxdatalen, uint32 laddrlen, uint32 raddrlen, ж<uint32> recvd, ж<Overlapped> overlapped) {
    error err = default;

    var ᴋ561 = buf;
    var ᴋ562 = recvd;
    var ᴋ563 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procAcceptEx.Addr(), (uintptr)ls, (uintptr)@as, (uintptr)ᴋ561, (uintptr)rxdatalen, (uintptr)laddrlen, (uintptr)raddrlen, (uintptr)ᴋ562, (uintptr)ᴋ563);
    System.GC.KeepAlive(ᴋ561);
    System.GC.KeepAlive(ᴋ562);
    System.GC.KeepAlive(ᴋ563);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static void GetAcceptExSockaddrs(ж<byte> buf, uint32 rxdatalen, uint32 laddrlen, uint32 raddrlen, ж<ж<RawSockaddrAny>> lrsa, ж<int32> lrsalen, ж<ж<RawSockaddrAny>> rrsa, ж<int32> rrsalen) {
    var ᴋ564 = buf;
    var ᴋ565 = lrsa;
    var ᴋ566 = lrsalen;
    var ᴋ567 = rrsa;
    var ᴋ568 = rrsalen;
        syscall.SyscallN(procGetAcceptExSockaddrs.Addr(), (uintptr)ᴋ564, (uintptr)rxdatalen, (uintptr)laddrlen, (uintptr)raddrlen, (uintptr)ᴋ565, (uintptr)ᴋ566, (uintptr)ᴋ567, (uintptr)ᴋ568);
    System.GC.KeepAlive(ᴋ564);
    System.GC.KeepAlive(ᴋ565);
    System.GC.KeepAlive(ᴋ566);
    System.GC.KeepAlive(ᴋ567);
    System.GC.KeepAlive(ᴋ568);
    return;
}

public static error /*err*/ TransmitFile(Handle s, Handle handle, uint32 bytesToWrite, uint32 bytsPerSend, ж<Overlapped> overlapped, ж<TransmitFileBuffers> transmitFileBuf, uint32 flags) {
    error err = default;

    var ᴋ569 = overlapped;
    var ᴋ570 = transmitFileBuf;
        var (r1, _, e1) = syscall.SyscallN(procTransmitFile.Addr(), (uintptr)s, (uintptr)handle, (uintptr)bytesToWrite, (uintptr)bytsPerSend, (uintptr)ᴋ569, (uintptr)ᴋ570, (uintptr)flags);
    System.GC.KeepAlive(ᴋ569);
    System.GC.KeepAlive(ᴋ570);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*neterr*/ NetApiBufferFree(ж<byte> buf) {
    error neterr = default;

    var ᴋ571 = buf;
        var (r0, _, _) = syscall.SyscallN(procNetApiBufferFree.Addr(), (uintptr)ᴋ571);
    System.GC.KeepAlive(ᴋ571);
    if (r0 != 0) {
        neterr = ((syscall.Errno)r0);
    }
    return neterr;
}

public static error /*neterr*/ NetGetJoinInformation(ж<uint16> server, ж<ж<uint16>> name, ж<uint32> bufType) {
    error neterr = default;

    var ᴋ572 = server;
    var ᴋ573 = name;
    var ᴋ574 = bufType;
        var (r0, _, _) = syscall.SyscallN(procNetGetJoinInformation.Addr(), (uintptr)ᴋ572, (uintptr)ᴋ573, (uintptr)ᴋ574);
    System.GC.KeepAlive(ᴋ572);
    System.GC.KeepAlive(ᴋ573);
    System.GC.KeepAlive(ᴋ574);
    if (r0 != 0) {
        neterr = ((syscall.Errno)r0);
    }
    return neterr;
}

public static error /*neterr*/ NetUserEnum(ж<uint16> serverName, uint32 level, uint32 filter, ж<ж<byte>> buf, uint32 prefMaxLen, ж<uint32> entriesRead, ж<uint32> totalEntries, ж<uint32> resumeHandle) {
    error neterr = default;

    var ᴋ575 = serverName;
    var ᴋ576 = buf;
    var ᴋ577 = entriesRead;
    var ᴋ578 = totalEntries;
    var ᴋ579 = resumeHandle;
        var (r0, _, _) = syscall.SyscallN(procNetUserEnum.Addr(), (uintptr)ᴋ575, (uintptr)level, (uintptr)filter, (uintptr)ᴋ576, (uintptr)prefMaxLen, (uintptr)ᴋ577, (uintptr)ᴋ578, (uintptr)ᴋ579);
    System.GC.KeepAlive(ᴋ575);
    System.GC.KeepAlive(ᴋ576);
    System.GC.KeepAlive(ᴋ577);
    System.GC.KeepAlive(ᴋ578);
    System.GC.KeepAlive(ᴋ579);
    if (r0 != 0) {
        neterr = ((syscall.Errno)r0);
    }
    return neterr;
}

public static error /*neterr*/ NetUserGetInfo(ж<uint16> serverName, ж<uint16> userName, uint32 level, ж<ж<byte>> buf) {
    error neterr = default;

    var ᴋ580 = serverName;
    var ᴋ581 = userName;
    var ᴋ582 = buf;
        var (r0, _, _) = syscall.SyscallN(procNetUserGetInfo.Addr(), (uintptr)ᴋ580, (uintptr)ᴋ581, (uintptr)level, (uintptr)ᴋ582);
    System.GC.KeepAlive(ᴋ580);
    System.GC.KeepAlive(ᴋ581);
    System.GC.KeepAlive(ᴋ582);
    if (r0 != 0) {
        neterr = ((syscall.Errno)r0);
    }
    return neterr;
}

public static error /*ntstatus*/ NtCreateFile(ж<Handle> handle, uint32 access, ж<OBJECT_ATTRIBUTES> oa, ж<IO_STATUS_BLOCK> iosb, ж<int64> allocationSize, uint32 attributes, uint32 share, uint32 disposition, uint32 options, uintptr eabuffer, uint32 ealength) {
    error ntstatus = default;

    var ᴋ583 = handle;
    var ᴋ584 = oa;
    var ᴋ585 = iosb;
    var ᴋ586 = allocationSize;
        var (r0, _, _) = syscall.SyscallN(procNtCreateFile.Addr(), (uintptr)ᴋ583, (uintptr)access, (uintptr)ᴋ584, (uintptr)ᴋ585, (uintptr)ᴋ586, (uintptr)attributes, (uintptr)share, (uintptr)disposition, (uintptr)options, (uintptr)eabuffer, (uintptr)ealength);
    System.GC.KeepAlive(ᴋ583);
    System.GC.KeepAlive(ᴋ584);
    System.GC.KeepAlive(ᴋ585);
    System.GC.KeepAlive(ᴋ586);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ NtCreateNamedPipeFile(ж<Handle> pipe, uint32 access, ж<OBJECT_ATTRIBUTES> oa, ж<IO_STATUS_BLOCK> iosb, uint32 share, uint32 disposition, uint32 options, uint32 typ, uint32 readMode, uint32 completionMode, uint32 maxInstances, uint32 inboundQuota, uint32 outputQuota, ж<int64> timeout) {
    error ntstatus = default;

    var ᴋ587 = pipe;
    var ᴋ588 = oa;
    var ᴋ589 = iosb;
    var ᴋ590 = timeout;
        var (r0, _, _) = syscall.SyscallN(procNtCreateNamedPipeFile.Addr(), (uintptr)ᴋ587, (uintptr)access, (uintptr)ᴋ588, (uintptr)ᴋ589, (uintptr)share, (uintptr)disposition, (uintptr)options, (uintptr)typ, (uintptr)readMode, (uintptr)completionMode, (uintptr)maxInstances, (uintptr)inboundQuota, (uintptr)outputQuota, (uintptr)ᴋ590);
    System.GC.KeepAlive(ᴋ587);
    System.GC.KeepAlive(ᴋ588);
    System.GC.KeepAlive(ᴋ589);
    System.GC.KeepAlive(ᴋ590);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ NtQueryEaFile(Handle handle, ж<IO_STATUS_BLOCK> iosb, ж<byte> outBuffer, uint32 outBufferLen, bool returnSingleEntry, ж<byte> eaList, uint32 eaListLen, ж<uint32> eaIndex, bool restartScan) {
    error ntstatus = default;

    uint32 _p0 = default;
    if (returnSingleEntry) {
        _p0 = 1;
    }
    uint32 _p1 = default;
    if (restartScan) {
        _p1 = 1;
    }
    var ᴋ591 = iosb;
    var ᴋ592 = outBuffer;
    var ᴋ593 = eaList;
    var ᴋ594 = eaIndex;
        var (r0, _, _) = syscall.SyscallN(procNtQueryEaFile.Addr(), (uintptr)handle, (uintptr)ᴋ591, (uintptr)ᴋ592, (uintptr)outBufferLen, (uintptr)_p0, (uintptr)ᴋ593, (uintptr)eaListLen, (uintptr)ᴋ594, (uintptr)_p1);
    System.GC.KeepAlive(ᴋ591);
    System.GC.KeepAlive(ᴋ592);
    System.GC.KeepAlive(ᴋ593);
    System.GC.KeepAlive(ᴋ594);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ NtQueryInformationFile(Handle handle, ж<IO_STATUS_BLOCK> iosb, ж<byte> outBuffer, uint32 outBufferLen, uint32 @class) {
    error ntstatus = default;

    var ᴋ595 = iosb;
    var ᴋ596 = outBuffer;
        var (r0, _, _) = syscall.SyscallN(procNtQueryInformationFile.Addr(), (uintptr)handle, (uintptr)ᴋ595, (uintptr)ᴋ596, (uintptr)outBufferLen, (uintptr)@class);
    System.GC.KeepAlive(ᴋ595);
    System.GC.KeepAlive(ᴋ596);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ NtQueryInformationProcess(Handle proc, int32 procInfoClass, @unsafe.Pointer procInfo, uint32 procInfoLen, ж<uint32> retLen) {
    error ntstatus = default;

    var ᴋ597 = procInfo;
    var ᴋ598 = retLen;
        var (r0, _, _) = syscall.SyscallN(procNtQueryInformationProcess.Addr(), (uintptr)proc, (uintptr)procInfoClass, (uintptr)ᴋ597, (uintptr)procInfoLen, (uintptr)ᴋ598);
    System.GC.KeepAlive(ᴋ597);
    System.GC.KeepAlive(ᴋ598);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ NtQuerySystemInformation(int32 sysInfoClass, @unsafe.Pointer sysInfo, uint32 sysInfoLen, ж<uint32> retLen) {
    error ntstatus = default;

    var ᴋ599 = sysInfo;
    var ᴋ600 = retLen;
        var (r0, _, _) = syscall.SyscallN(procNtQuerySystemInformation.Addr(), (uintptr)sysInfoClass, (uintptr)ᴋ599, (uintptr)sysInfoLen, (uintptr)ᴋ600);
    System.GC.KeepAlive(ᴋ599);
    System.GC.KeepAlive(ᴋ600);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ NtSetEaFile(Handle handle, ж<IO_STATUS_BLOCK> iosb, ж<byte> inBuffer, uint32 inBufferLen) {
    error ntstatus = default;

    var ᴋ601 = iosb;
    var ᴋ602 = inBuffer;
        var (r0, _, _) = syscall.SyscallN(procNtSetEaFile.Addr(), (uintptr)handle, (uintptr)ᴋ601, (uintptr)ᴋ602, (uintptr)inBufferLen);
    System.GC.KeepAlive(ᴋ601);
    System.GC.KeepAlive(ᴋ602);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ NtSetInformationFile(Handle handle, ж<IO_STATUS_BLOCK> iosb, ж<byte> inBuffer, uint32 inBufferLen, uint32 @class) {
    error ntstatus = default;

    var ᴋ603 = iosb;
    var ᴋ604 = inBuffer;
        var (r0, _, _) = syscall.SyscallN(procNtSetInformationFile.Addr(), (uintptr)handle, (uintptr)ᴋ603, (uintptr)ᴋ604, (uintptr)inBufferLen, (uintptr)@class);
    System.GC.KeepAlive(ᴋ603);
    System.GC.KeepAlive(ᴋ604);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ NtSetInformationProcess(Handle proc, int32 procInfoClass, @unsafe.Pointer procInfo, uint32 procInfoLen) {
    error ntstatus = default;

    var ᴋ605 = procInfo;
        var (r0, _, _) = syscall.SyscallN(procNtSetInformationProcess.Addr(), (uintptr)proc, (uintptr)procInfoClass, (uintptr)ᴋ605, (uintptr)procInfoLen);
    System.GC.KeepAlive(ᴋ605);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ NtSetSystemInformation(int32 sysInfoClass, @unsafe.Pointer sysInfo, uint32 sysInfoLen) {
    error ntstatus = default;

    var ᴋ606 = sysInfo;
        var (r0, _, _) = syscall.SyscallN(procNtSetSystemInformation.Addr(), (uintptr)sysInfoClass, (uintptr)ᴋ606, (uintptr)sysInfoLen);
    System.GC.KeepAlive(ᴋ606);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static bool /*ret*/ RtlAddFunctionTable(ж<RUNTIME_FUNCTION> functionTable, uint32 entryCount, uintptr baseAddress) {
    bool ret = default;

    var ᴋ607 = functionTable;
        var (r0, _, _) = syscall.SyscallN(procRtlAddFunctionTable.Addr(), (uintptr)ᴋ607, (uintptr)entryCount, (uintptr)baseAddress);
    System.GC.KeepAlive(ᴋ607);
    ret = r0 != 0;
    return ret;
}

public static error /*ntstatus*/ RtlDefaultNpAcl(ж<ж<ACL>> acl) {
    error ntstatus = default;

    var ᴋ608 = acl;
        var (r0, _, _) = syscall.SyscallN(procRtlDefaultNpAcl.Addr(), (uintptr)ᴋ608);
    System.GC.KeepAlive(ᴋ608);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static bool /*ret*/ RtlDeleteFunctionTable(ж<RUNTIME_FUNCTION> functionTable) {
    bool ret = default;

    var ᴋ609 = functionTable;
        var (r0, _, _) = syscall.SyscallN(procRtlDeleteFunctionTable.Addr(), (uintptr)ᴋ609);
    System.GC.KeepAlive(ᴋ609);
    ret = r0 != 0;
    return ret;
}

public static error /*ntstatus*/ RtlDosPathNameToNtPathName(ж<uint16> dosName, ж<NTUnicodeString> ntName, ж<uint16> ntFileNamePart, ж<RTL_RELATIVE_NAME> relativeName) {
    error ntstatus = default;

    var ᴋ610 = dosName;
    var ᴋ611 = ntName;
    var ᴋ612 = ntFileNamePart;
    var ᴋ613 = relativeName;
        var (r0, _, _) = syscall.SyscallN(procRtlDosPathNameToNtPathName_U_WithStatus.Addr(), (uintptr)ᴋ610, (uintptr)ᴋ611, (uintptr)ᴋ612, (uintptr)ᴋ613);
    System.GC.KeepAlive(ᴋ610);
    System.GC.KeepAlive(ᴋ611);
    System.GC.KeepAlive(ᴋ612);
    System.GC.KeepAlive(ᴋ613);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static error /*ntstatus*/ RtlDosPathNameToRelativeNtPathName(ж<uint16> dosName, ж<NTUnicodeString> ntName, ж<uint16> ntFileNamePart, ж<RTL_RELATIVE_NAME> relativeName) {
    error ntstatus = default;

    var ᴋ614 = dosName;
    var ᴋ615 = ntName;
    var ᴋ616 = ntFileNamePart;
    var ᴋ617 = relativeName;
        var (r0, _, _) = syscall.SyscallN(procRtlDosPathNameToRelativeNtPathName_U_WithStatus.Addr(), (uintptr)ᴋ614, (uintptr)ᴋ615, (uintptr)ᴋ616, (uintptr)ᴋ617);
    System.GC.KeepAlive(ᴋ614);
    System.GC.KeepAlive(ᴋ615);
    System.GC.KeepAlive(ᴋ616);
    System.GC.KeepAlive(ᴋ617);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static ж<PEB> /*peb*/ RtlGetCurrentPeb() {
    ж<PEB> peb = default;

    var (r0, _, _) = syscall.SyscallN(procRtlGetCurrentPeb.Addr());
    peb = @unsafe.Pointer.To<PEB>((@unsafe.Pointer)r0);
    return peb;
}

internal static void rtlGetNtVersionNumbers(ж<uint32> majorVersion, ж<uint32> minorVersion, ж<uint32> buildNumber) {
    var ᴋ618 = majorVersion;
    var ᴋ619 = minorVersion;
    var ᴋ620 = buildNumber;
        syscall.SyscallN(procRtlGetNtVersionNumbers.Addr(), (uintptr)ᴋ618, (uintptr)ᴋ619, (uintptr)ᴋ620);
    System.GC.KeepAlive(ᴋ618);
    System.GC.KeepAlive(ᴋ619);
    System.GC.KeepAlive(ᴋ620);
    return;
}

internal static error /*ntstatus*/ rtlGetVersion(ж<OsVersionInfoEx> info) {
    error ntstatus = default;

    var ᴋ621 = info;
        var (r0, _, _) = syscall.SyscallN(procRtlGetVersion.Addr(), (uintptr)ᴋ621);
    System.GC.KeepAlive(ᴋ621);
    if (r0 != 0) {
        ntstatus = ((NTStatus)(uint32)r0);
    }
    return ntstatus;
}

public static void RtlInitString(ж<NTString> destinationString, ж<byte> sourceString) {
    var ᴋ622 = destinationString;
    var ᴋ623 = sourceString;
        syscall.SyscallN(procRtlInitString.Addr(), (uintptr)ᴋ622, (uintptr)ᴋ623);
    System.GC.KeepAlive(ᴋ622);
    System.GC.KeepAlive(ᴋ623);
    return;
}

public static void RtlInitUnicodeString(ж<NTUnicodeString> destinationString, ж<uint16> sourceString) {
    var ᴋ624 = destinationString;
    var ᴋ625 = sourceString;
        syscall.SyscallN(procRtlInitUnicodeString.Addr(), (uintptr)ᴋ624, (uintptr)ᴋ625);
    System.GC.KeepAlive(ᴋ624);
    System.GC.KeepAlive(ᴋ625);
    return;
}

internal static syscall.Errno /*ret*/ rtlNtStatusToDosErrorNoTeb(NTStatus ntstatus) {
    syscall.Errno ret = default;

    var (r0, _, _) = syscall.SyscallN(procRtlNtStatusToDosErrorNoTeb.Addr(), (uintptr)ntstatus);
    ret = (syscall.Errno)r0;
    return ret;
}

internal static error /*ret*/ clsidFromString(ж<uint16> lpsz, ж<GUID> pclsid) {
    error ret = default;

    var ᴋ626 = lpsz;
    var ᴋ627 = pclsid;
        var (r0, _, _) = syscall.SyscallN(procCLSIDFromString.Addr(), (uintptr)ᴋ626, (uintptr)ᴋ627);
    System.GC.KeepAlive(ᴋ626);
    System.GC.KeepAlive(ᴋ627);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

internal static error /*ret*/ coCreateGuid(ж<GUID> pguid) {
    error ret = default;

    var ᴋ628 = pguid;
        var (r0, _, _) = syscall.SyscallN(procCoCreateGuid.Addr(), (uintptr)ᴋ628);
    System.GC.KeepAlive(ᴋ628);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*ret*/ CoGetObject(ж<uint16> name, ж<BIND_OPTS3> bindOpts, ж<GUID> guid, ж<ж<uintptr>> functionTable) {
    error ret = default;

    var ᴋ629 = name;
    var ᴋ630 = bindOpts;
    var ᴋ631 = guid;
    var ᴋ632 = functionTable;
        var (r0, _, _) = syscall.SyscallN(procCoGetObject.Addr(), (uintptr)ᴋ629, (uintptr)ᴋ630, (uintptr)ᴋ631, (uintptr)ᴋ632);
    System.GC.KeepAlive(ᴋ629);
    System.GC.KeepAlive(ᴋ630);
    System.GC.KeepAlive(ᴋ631);
    System.GC.KeepAlive(ᴋ632);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*ret*/ CoInitializeEx(uintptr reserved, uint32 coInit) {
    error ret = default;

    var (r0, _, _) = syscall.SyscallN(procCoInitializeEx.Addr(), (uintptr)reserved, (uintptr)coInit);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static void CoTaskMemFree(@unsafe.Pointer address) {
    var ᴋ633 = address;
        syscall.SyscallN(procCoTaskMemFree.Addr(), (uintptr)ᴋ633);
    System.GC.KeepAlive(ᴋ633);
    return;
}

public static void CoUninitialize() {
    syscall.SyscallN(procCoUninitialize.Addr());
    return;
}

internal static int32 /*chars*/ stringFromGUID2(ж<GUID> rguid, ж<uint16> lpsz, int32 cchMax) {
    int32 chars = default;

    var ᴋ634 = rguid;
    var ᴋ635 = lpsz;
        var (r0, _, _) = syscall.SyscallN(procStringFromGUID2.Addr(), (uintptr)ᴋ634, (uintptr)ᴋ635, (uintptr)cchMax);
    System.GC.KeepAlive(ᴋ634);
    System.GC.KeepAlive(ᴋ635);
    chars = (int32)r0;
    return chars;
}

public static error /*err*/ EnumProcessModules(Handle process, ж<Handle> module, uint32 cb, ж<uint32> cbNeeded) {
    error err = default;

    var ᴋ636 = module;
    var ᴋ637 = cbNeeded;
        var (r1, _, e1) = syscall.SyscallN(procEnumProcessModules.Addr(), (uintptr)process, (uintptr)ᴋ636, (uintptr)cb, (uintptr)ᴋ637);
    System.GC.KeepAlive(ᴋ636);
    System.GC.KeepAlive(ᴋ637);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ EnumProcessModulesEx(Handle process, ж<Handle> module, uint32 cb, ж<uint32> cbNeeded, uint32 filterFlag) {
    error err = default;

    var ᴋ638 = module;
    var ᴋ639 = cbNeeded;
        var (r1, _, e1) = syscall.SyscallN(procEnumProcessModulesEx.Addr(), (uintptr)process, (uintptr)ᴋ638, (uintptr)cb, (uintptr)ᴋ639, (uintptr)filterFlag);
    System.GC.KeepAlive(ᴋ638);
    System.GC.KeepAlive(ᴋ639);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ enumProcesses(ж<uint32> processIds, uint32 nSize, ж<uint32> bytesReturned) {
    error err = default;

    var ᴋ640 = processIds;
    var ᴋ641 = bytesReturned;
        var (r1, _, e1) = syscall.SyscallN(procEnumProcesses.Addr(), (uintptr)ᴋ640, (uintptr)nSize, (uintptr)ᴋ641);
    System.GC.KeepAlive(ᴋ640);
    System.GC.KeepAlive(ᴋ641);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetModuleBaseName(Handle process, Handle module, ж<uint16> baseName, uint32 size) {
    error err = default;

    var ᴋ642 = baseName;
        var (r1, _, e1) = syscall.SyscallN(procGetModuleBaseNameW.Addr(), (uintptr)process, (uintptr)module, (uintptr)ᴋ642, (uintptr)size);
    System.GC.KeepAlive(ᴋ642);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetModuleFileNameEx(Handle process, Handle module, ж<uint16> filename, uint32 size) {
    error err = default;

    var ᴋ643 = filename;
        var (r1, _, e1) = syscall.SyscallN(procGetModuleFileNameExW.Addr(), (uintptr)process, (uintptr)module, (uintptr)ᴋ643, (uintptr)size);
    System.GC.KeepAlive(ᴋ643);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetModuleInformation(Handle process, Handle module, ж<ModuleInfo> modinfo, uint32 cb) {
    error err = default;

    var ᴋ644 = modinfo;
        var (r1, _, e1) = syscall.SyscallN(procGetModuleInformation.Addr(), (uintptr)process, (uintptr)module, (uintptr)ᴋ644, (uintptr)cb);
    System.GC.KeepAlive(ᴋ644);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ QueryWorkingSetEx(Handle process, uintptr pv, uint32 cb) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procQueryWorkingSetEx.Addr(), (uintptr)process, (uintptr)pv, (uintptr)cb);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*ret*/ SubscribeServiceChangeNotifications(Handle service, uint32 eventType, uintptr callback, uintptr callbackCtx, ж<uintptr> subscription) {
    error ret = default;

    ret = procSubscribeServiceChangeNotifications.Find();
    if (ret != nil) {
        return ret;
    }
    var ᴋ645 = subscription;
        var (r0, _, _) = syscall.SyscallN(procSubscribeServiceChangeNotifications.Addr(), (uintptr)service, (uintptr)eventType, (uintptr)callback, (uintptr)callbackCtx, (uintptr)ᴋ645);
    System.GC.KeepAlive(ᴋ645);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*err*/ UnsubscribeServiceChangeNotifications(uintptr subscription) {
    error err = default;

    err = procUnsubscribeServiceChangeNotifications.Find();
    if (err != nil) {
        return err;
    }
    syscall.SyscallN(procUnsubscribeServiceChangeNotifications.Addr(), (uintptr)subscription);
    return err;
}

public static error /*err*/ GetUserNameEx(uint32 nameFormat, ж<uint16> nameBuffre, ж<uint32> nSize) {
    error err = default;

    var ᴋ646 = nameBuffre;
    var ᴋ647 = nSize;
        var (r1, _, e1) = syscall.SyscallN(procGetUserNameExW.Addr(), (uintptr)nameFormat, (uintptr)ᴋ646, (uintptr)ᴋ647);
    System.GC.KeepAlive(ᴋ646);
    System.GC.KeepAlive(ᴋ647);
    if ((uintptr)(r1 & 0xff) == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ TranslateName(ж<uint16> accName, uint32 accNameFormat, uint32 desiredNameFormat, ж<uint16> translatedName, ж<uint32> nSize) {
    error err = default;

    var ᴋ648 = accName;
    var ᴋ649 = translatedName;
    var ᴋ650 = nSize;
        var (r1, _, e1) = syscall.SyscallN(procTranslateNameW.Addr(), (uintptr)ᴋ648, (uintptr)accNameFormat, (uintptr)desiredNameFormat, (uintptr)ᴋ649, (uintptr)ᴋ650);
    System.GC.KeepAlive(ᴋ648);
    System.GC.KeepAlive(ᴋ649);
    System.GC.KeepAlive(ᴋ650);
    if ((uintptr)(r1 & 0xff) == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetupDiBuildDriverInfoList(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDIT driverType) {
    error err = default;

    var ᴋ651 = deviceInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiBuildDriverInfoList.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ651, (uintptr)driverType);
    System.GC.KeepAlive(ᴋ651);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetupDiCallClassInstaller(DI_FUNCTION installFunction, DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    error err = default;

    var ᴋ652 = deviceInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiCallClassInstaller.Addr(), (uintptr)installFunction, (uintptr)deviceInfoSet, (uintptr)ᴋ652);
    System.GC.KeepAlive(ᴋ652);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetupDiCancelDriverInfoSearch(DevInfo deviceInfoSet) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetupDiCancelDriverInfoSearch.Addr(), (uintptr)deviceInfoSet);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiClassGuidsFromNameEx(ж<uint16> className, ж<GUID> classGuidList, uint32 classGuidListSize, ж<uint32> requiredSize, ж<uint16> machineName, uintptr reserved) {
    error err = default;

    var ᴋ653 = className;
    var ᴋ654 = classGuidList;
    var ᴋ655 = requiredSize;
    var ᴋ656 = machineName;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiClassGuidsFromNameExW.Addr(), (uintptr)ᴋ653, (uintptr)ᴋ654, (uintptr)classGuidListSize, (uintptr)ᴋ655, (uintptr)ᴋ656, (uintptr)reserved);
    System.GC.KeepAlive(ᴋ653);
    System.GC.KeepAlive(ᴋ654);
    System.GC.KeepAlive(ᴋ655);
    System.GC.KeepAlive(ᴋ656);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiClassNameFromGuidEx(ж<GUID> classGUID, ж<uint16> className, uint32 classNameSize, ж<uint32> requiredSize, ж<uint16> machineName, uintptr reserved) {
    error err = default;

    var ᴋ657 = classGUID;
    var ᴋ658 = className;
    var ᴋ659 = requiredSize;
    var ᴋ660 = machineName;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiClassNameFromGuidExW.Addr(), (uintptr)ᴋ657, (uintptr)ᴋ658, (uintptr)classNameSize, (uintptr)ᴋ659, (uintptr)ᴋ660, (uintptr)reserved);
    System.GC.KeepAlive(ᴋ657);
    System.GC.KeepAlive(ᴋ658);
    System.GC.KeepAlive(ᴋ659);
    System.GC.KeepAlive(ᴋ660);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (DevInfo handle, error err) setupDiCreateDeviceInfoListEx(ж<GUID> classGUID, uintptr hwndParent, ж<uint16> machineName, uintptr reserved) {
    DevInfo handle = default;
    error err = default;

    var ᴋ661 = classGUID;
    var ᴋ662 = machineName;
        var (r0, _, e1) = syscall.SyscallN(procSetupDiCreateDeviceInfoListExW.Addr(), (uintptr)ᴋ661, (uintptr)hwndParent, (uintptr)ᴋ662, (uintptr)reserved);
    System.GC.KeepAlive(ᴋ661);
    System.GC.KeepAlive(ᴋ662);
    handle = (DevInfo)r0;
    if (handle == ((DevInfo)(uintptr)InvalidHandle)) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

internal static error /*err*/ setupDiCreateDeviceInfo(DevInfo deviceInfoSet, ж<uint16> DeviceName, ж<GUID> classGUID, ж<uint16> DeviceDescription, uintptr hwndParent, DICD CreationFlags, ж<DevInfoData> deviceInfoData) {
    error err = default;

    var ᴋ663 = DeviceName;
    var ᴋ664 = classGUID;
    var ᴋ665 = DeviceDescription;
    var ᴋ666 = deviceInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiCreateDeviceInfoW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ663, (uintptr)ᴋ664, (uintptr)ᴋ665, (uintptr)hwndParent, (uintptr)CreationFlags, (uintptr)ᴋ666);
    System.GC.KeepAlive(ᴋ663);
    System.GC.KeepAlive(ᴋ664);
    System.GC.KeepAlive(ᴋ665);
    System.GC.KeepAlive(ᴋ666);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetupDiDestroyDeviceInfoList(DevInfo deviceInfoSet) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procSetupDiDestroyDeviceInfoList.Addr(), (uintptr)deviceInfoSet);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetupDiDestroyDriverInfoList(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDIT driverType) {
    error err = default;

    var ᴋ667 = deviceInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiDestroyDriverInfoList.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ667, (uintptr)driverType);
    System.GC.KeepAlive(ᴋ667);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiEnumDeviceInfo(DevInfo deviceInfoSet, uint32 memberIndex, ж<DevInfoData> deviceInfoData) {
    error err = default;

    var ᴋ668 = deviceInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiEnumDeviceInfo.Addr(), (uintptr)deviceInfoSet, (uintptr)memberIndex, (uintptr)ᴋ668);
    System.GC.KeepAlive(ᴋ668);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiEnumDriverInfo(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDIT driverType, uint32 memberIndex, ж<DrvInfoData> driverInfoData) {
    error err = default;

    var ᴋ669 = deviceInfoData;
    var ᴋ670 = driverInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiEnumDriverInfoW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ669, (uintptr)driverType, (uintptr)memberIndex, (uintptr)ᴋ670);
    System.GC.KeepAlive(ᴋ669);
    System.GC.KeepAlive(ᴋ670);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (DevInfo handle, error err) setupDiGetClassDevsEx(ж<GUID> classGUID, ж<uint16> Enumerator, uintptr hwndParent, DIGCF Flags, DevInfo deviceInfoSet, ж<uint16> machineName, uintptr reserved) {
    DevInfo handle = default;
    error err = default;

    var ᴋ671 = classGUID;
    var ᴋ672 = Enumerator;
    var ᴋ673 = machineName;
        var (r0, _, e1) = syscall.SyscallN(procSetupDiGetClassDevsExW.Addr(), (uintptr)ᴋ671, (uintptr)ᴋ672, (uintptr)hwndParent, (uintptr)Flags, (uintptr)deviceInfoSet, (uintptr)ᴋ673, (uintptr)reserved);
    System.GC.KeepAlive(ᴋ671);
    System.GC.KeepAlive(ᴋ672);
    System.GC.KeepAlive(ᴋ673);
    handle = (DevInfo)r0;
    if (handle == ((DevInfo)(uintptr)InvalidHandle)) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ SetupDiGetClassInstallParams(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<ClassInstallHeader> classInstallParams, uint32 classInstallParamsSize, ж<uint32> requiredSize) {
    error err = default;

    var ᴋ674 = deviceInfoData;
    var ᴋ675 = classInstallParams;
    var ᴋ676 = requiredSize;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiGetClassInstallParamsW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ674, (uintptr)ᴋ675, (uintptr)classInstallParamsSize, (uintptr)ᴋ676);
    System.GC.KeepAlive(ᴋ674);
    System.GC.KeepAlive(ᴋ675);
    System.GC.KeepAlive(ᴋ676);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiGetDeviceInfoListDetail(DevInfo deviceInfoSet, ж<DevInfoListDetailData> deviceInfoSetDetailData) {
    error err = default;

    var ᴋ677 = deviceInfoSetDetailData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiGetDeviceInfoListDetailW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ677);
    System.GC.KeepAlive(ᴋ677);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiGetDeviceInstallParams(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DevInstallParams> deviceInstallParams) {
    error err = default;

    var ᴋ678 = deviceInfoData;
    var ᴋ679 = deviceInstallParams;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiGetDeviceInstallParamsW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ678, (uintptr)ᴋ679);
    System.GC.KeepAlive(ᴋ678);
    System.GC.KeepAlive(ᴋ679);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiGetDeviceInstanceId(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<uint16> instanceId, uint32 instanceIdSize, ж<uint32> instanceIdRequiredSize) {
    error err = default;

    var ᴋ680 = deviceInfoData;
    var ᴋ681 = instanceId;
    var ᴋ682 = instanceIdRequiredSize;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiGetDeviceInstanceIdW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ680, (uintptr)ᴋ681, (uintptr)instanceIdSize, (uintptr)ᴋ682);
    System.GC.KeepAlive(ᴋ680);
    System.GC.KeepAlive(ᴋ681);
    System.GC.KeepAlive(ᴋ682);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiGetDeviceProperty(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DEVPROPKEY> propertyKey, ж<DEVPROPTYPE> propertyType, ж<byte> propertyBuffer, uint32 propertyBufferSize, ж<uint32> requiredSize, uint32 flags) {
    error err = default;

    var ᴋ683 = deviceInfoData;
    var ᴋ684 = propertyKey;
    var ᴋ685 = propertyType;
    var ᴋ686 = propertyBuffer;
    var ᴋ687 = requiredSize;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiGetDevicePropertyW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ683, (uintptr)ᴋ684, (uintptr)ᴋ685, (uintptr)ᴋ686, (uintptr)propertyBufferSize, (uintptr)ᴋ687, (uintptr)flags);
    System.GC.KeepAlive(ᴋ683);
    System.GC.KeepAlive(ᴋ684);
    System.GC.KeepAlive(ᴋ685);
    System.GC.KeepAlive(ᴋ686);
    System.GC.KeepAlive(ᴋ687);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiGetDeviceRegistryProperty(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDRP property, ж<uint32> propertyRegDataType, ж<byte> propertyBuffer, uint32 propertyBufferSize, ж<uint32> requiredSize) {
    error err = default;

    var ᴋ688 = deviceInfoData;
    var ᴋ689 = propertyRegDataType;
    var ᴋ690 = propertyBuffer;
    var ᴋ691 = requiredSize;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiGetDeviceRegistryPropertyW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ688, (uintptr)property, (uintptr)ᴋ689, (uintptr)ᴋ690, (uintptr)propertyBufferSize, (uintptr)ᴋ691);
    System.GC.KeepAlive(ᴋ688);
    System.GC.KeepAlive(ᴋ689);
    System.GC.KeepAlive(ᴋ690);
    System.GC.KeepAlive(ᴋ691);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiGetDriverInfoDetail(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DrvInfoData> driverInfoData, ж<DrvInfoDetailData> driverInfoDetailData, uint32 driverInfoDetailDataSize, ж<uint32> requiredSize) {
    error err = default;

    var ᴋ692 = deviceInfoData;
    var ᴋ693 = driverInfoData;
    var ᴋ694 = driverInfoDetailData;
    var ᴋ695 = requiredSize;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiGetDriverInfoDetailW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ692, (uintptr)ᴋ693, (uintptr)ᴋ694, (uintptr)driverInfoDetailDataSize, (uintptr)ᴋ695);
    System.GC.KeepAlive(ᴋ692);
    System.GC.KeepAlive(ᴋ693);
    System.GC.KeepAlive(ᴋ694);
    System.GC.KeepAlive(ᴋ695);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiGetSelectedDevice(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    error err = default;

    var ᴋ696 = deviceInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiGetSelectedDevice.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ696);
    System.GC.KeepAlive(ᴋ696);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiGetSelectedDriver(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DrvInfoData> driverInfoData) {
    error err = default;

    var ᴋ697 = deviceInfoData;
    var ᴋ698 = driverInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiGetSelectedDriverW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ697, (uintptr)ᴋ698);
    System.GC.KeepAlive(ᴋ697);
    System.GC.KeepAlive(ᴋ698);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle key, error err) SetupDiOpenDevRegKey(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, DICS_FLAG Scope, uint32 HwProfile, DIREG KeyType, uint32 samDesired) {
    Handle key = default;
    error err = default;

    var ᴋ699 = deviceInfoData;
        var (r0, _, e1) = syscall.SyscallN(procSetupDiOpenDevRegKey.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ699, (uintptr)Scope, (uintptr)HwProfile, (uintptr)KeyType, (uintptr)samDesired);
    System.GC.KeepAlive(ᴋ699);
    key = (Handle)r0;
    if (key == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (key, err);
}

public static error /*err*/ SetupDiSetClassInstallParams(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<ClassInstallHeader> classInstallParams, uint32 classInstallParamsSize) {
    error err = default;

    var ᴋ700 = deviceInfoData;
    var ᴋ701 = classInstallParams;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiSetClassInstallParamsW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ700, (uintptr)ᴋ701, (uintptr)classInstallParamsSize);
    System.GC.KeepAlive(ᴋ700);
    System.GC.KeepAlive(ᴋ701);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetupDiSetDeviceInstallParams(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DevInstallParams> deviceInstallParams) {
    error err = default;

    var ᴋ702 = deviceInfoData;
    var ᴋ703 = deviceInstallParams;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiSetDeviceInstallParamsW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ702, (uintptr)ᴋ703);
    System.GC.KeepAlive(ᴋ702);
    System.GC.KeepAlive(ᴋ703);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupDiSetDeviceRegistryProperty(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDRP property, ж<byte> propertyBuffer, uint32 propertyBufferSize) {
    error err = default;

    var ᴋ704 = deviceInfoData;
    var ᴋ705 = propertyBuffer;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiSetDeviceRegistryPropertyW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ704, (uintptr)property, (uintptr)ᴋ705, (uintptr)propertyBufferSize);
    System.GC.KeepAlive(ᴋ704);
    System.GC.KeepAlive(ᴋ705);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetupDiSetSelectedDevice(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    error err = default;

    var ᴋ706 = deviceInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiSetSelectedDevice.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ706);
    System.GC.KeepAlive(ᴋ706);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ SetupDiSetSelectedDriver(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DrvInfoData> driverInfoData) {
    error err = default;

    var ᴋ707 = deviceInfoData;
    var ᴋ708 = driverInfoData;
        var (r1, _, e1) = syscall.SyscallN(procSetupDiSetSelectedDriverW.Addr(), (uintptr)deviceInfoSet, (uintptr)ᴋ707, (uintptr)ᴋ708);
    System.GC.KeepAlive(ᴋ707);
    System.GC.KeepAlive(ᴋ708);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ setupUninstallOEMInf(ж<uint16> infFileName, SUOI flags, uintptr reserved) {
    error err = default;

    var ᴋ709 = infFileName;
        var (r1, _, e1) = syscall.SyscallN(procSetupUninstallOEMInfW.Addr(), (uintptr)ᴋ709, (uintptr)flags, (uintptr)reserved);
    System.GC.KeepAlive(ᴋ709);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (ж<ж<uint16>> argv, error err) commandLineToArgv(ж<uint16> cmd, ж<int32> argc) {
    ж<ж<uint16>> argv = default;
    error err = default;

    var ᴋ710 = cmd;
    var ᴋ711 = argc;
        var (r0, _, e1) = syscall.SyscallN(procCommandLineToArgvW.Addr(), (uintptr)ᴋ710, (uintptr)ᴋ711);
    System.GC.KeepAlive(ᴋ710);
    System.GC.KeepAlive(ᴋ711);
    argv = @unsafe.Pointer.To<ж<uint16>>((@unsafe.Pointer)r0);
    if (argv == nil) {
        err = errnoErr(e1);
    }
    return (argv, err);
}

internal static error /*ret*/ shGetKnownFolderPath(ж<KNOWNFOLDERID> id, uint32 flags, Token token, ж<ж<uint16>> path) {
    error ret = default;

    var ᴋ712 = id;
    var ᴋ713 = path;
        var (r0, _, _) = syscall.SyscallN(procSHGetKnownFolderPath.Addr(), (uintptr)ᴋ712, (uintptr)flags, (uintptr)token, (uintptr)ᴋ713);
    System.GC.KeepAlive(ᴋ712);
    System.GC.KeepAlive(ᴋ713);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static error /*err*/ ShellExecute(Handle hwnd, ж<uint16> verb, ж<uint16> @file, ж<uint16> args, ж<uint16> cwd, int32 showCmd) {
    error err = default;

    var ᴋ714 = verb;
    var ᴋ715 = @file;
    var ᴋ716 = args;
    var ᴋ717 = cwd;
        var (r1, _, e1) = syscall.SyscallN(procShellExecuteW.Addr(), (uintptr)hwnd, (uintptr)ᴋ714, (uintptr)ᴋ715, (uintptr)ᴋ716, (uintptr)ᴋ717, (uintptr)showCmd);
    System.GC.KeepAlive(ᴋ714);
    System.GC.KeepAlive(ᴋ715);
    System.GC.KeepAlive(ᴋ716);
    System.GC.KeepAlive(ᴋ717);
    if (r1 <= 32) {
        err = errnoErr(e1);
    }
    return err;
}

public static void EnumChildWindows(HWND hwnd, uintptr enumFunc, @unsafe.Pointer param) {
    var ᴋ718 = param;
        syscall.SyscallN(procEnumChildWindows.Addr(), (uintptr)hwnd, (uintptr)enumFunc, (uintptr)ᴋ718);
    System.GC.KeepAlive(ᴋ718);
    return;
}

public static error /*err*/ EnumWindows(uintptr enumFunc, @unsafe.Pointer param) {
    error err = default;

    var ᴋ719 = param;
        var (r1, _, e1) = syscall.SyscallN(procEnumWindows.Addr(), (uintptr)enumFunc, (uintptr)ᴋ719);
    System.GC.KeepAlive(ᴋ719);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ ExitWindowsEx(uint32 flags, uint32 reason) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procExitWindowsEx.Addr(), (uintptr)flags, (uintptr)reason);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (int32 copied, error err) GetClassName(HWND hwnd, ж<uint16> className, int32 maxCount) {
    int32 copied = default;
    error err = default;

    var ᴋ720 = className;
        var (r0, _, e1) = syscall.SyscallN(procGetClassNameW.Addr(), (uintptr)hwnd, (uintptr)ᴋ720, (uintptr)maxCount);
    System.GC.KeepAlive(ᴋ720);
    copied = (int32)r0;
    if (copied == 0) {
        err = errnoErr(e1);
    }
    return (copied, err);
}

public static HWND /*hwnd*/ GetDesktopWindow() {
    HWND hwnd = default;

    var (r0, _, _) = syscall.SyscallN(procGetDesktopWindow.Addr());
    hwnd = (HWND)r0;
    return hwnd;
}

public static HWND /*hwnd*/ GetForegroundWindow() {
    HWND hwnd = default;

    var (r0, _, _) = syscall.SyscallN(procGetForegroundWindow.Addr());
    hwnd = (HWND)r0;
    return hwnd;
}

public static error /*err*/ GetGUIThreadInfo(uint32 thread, ж<GUIThreadInfo> info) {
    error err = default;

    var ᴋ721 = info;
        var (r1, _, e1) = syscall.SyscallN(procGetGUIThreadInfo.Addr(), (uintptr)thread, (uintptr)ᴋ721);
    System.GC.KeepAlive(ᴋ721);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static Handle /*hkl*/ GetKeyboardLayout(uint32 tid) {
    Handle hkl = default;

    var (r0, _, _) = syscall.SyscallN(procGetKeyboardLayout.Addr(), (uintptr)tid);
    hkl = (Handle)r0;
    return hkl;
}

public static HWND /*shellWindow*/ GetShellWindow() {
    HWND shellWindow = default;

    var (r0, _, _) = syscall.SyscallN(procGetShellWindow.Addr());
    shellWindow = (HWND)r0;
    return shellWindow;
}

public static (uint32 tid, error err) GetWindowThreadProcessId(HWND hwnd, ж<uint32> pid) {
    uint32 tid = default;
    error err = default;

    var ᴋ722 = pid;
        var (r0, _, e1) = syscall.SyscallN(procGetWindowThreadProcessId.Addr(), (uintptr)hwnd, (uintptr)ᴋ722);
    System.GC.KeepAlive(ᴋ722);
    tid = (uint32)r0;
    if (tid == 0) {
        err = errnoErr(e1);
    }
    return (tid, err);
}

public static bool /*isWindow*/ IsWindow(HWND hwnd) {
    bool isWindow = default;

    var (r0, _, _) = syscall.SyscallN(procIsWindow.Addr(), (uintptr)hwnd);
    isWindow = r0 != 0;
    return isWindow;
}

public static bool /*isUnicode*/ IsWindowUnicode(HWND hwnd) {
    bool isUnicode = default;

    var (r0, _, _) = syscall.SyscallN(procIsWindowUnicode.Addr(), (uintptr)hwnd);
    isUnicode = r0 != 0;
    return isUnicode;
}

public static bool /*isVisible*/ IsWindowVisible(HWND hwnd) {
    bool isVisible = default;

    var (r0, _, _) = syscall.SyscallN(procIsWindowVisible.Addr(), (uintptr)hwnd);
    isVisible = r0 != 0;
    return isVisible;
}

public static (Handle hkl, error err) LoadKeyboardLayout(ж<uint16> name, uint32 flags) {
    Handle hkl = default;
    error err = default;

    var ᴋ723 = name;
        var (r0, _, e1) = syscall.SyscallN(procLoadKeyboardLayoutW.Addr(), (uintptr)ᴋ723, (uintptr)flags);
    System.GC.KeepAlive(ᴋ723);
    hkl = (Handle)r0;
    if (hkl == 0) {
        err = errnoErr(e1);
    }
    return (hkl, err);
}

public static (int32 ret, error err) MessageBox(HWND hwnd, ж<uint16> text, ж<uint16> caption, uint32 boxtype) {
    int32 ret = default;
    error err = default;

    var ᴋ724 = text;
    var ᴋ725 = caption;
        var (r0, _, e1) = syscall.SyscallN(procMessageBoxW.Addr(), (uintptr)hwnd, (uintptr)ᴋ724, (uintptr)ᴋ725, (uintptr)boxtype);
    System.GC.KeepAlive(ᴋ724);
    System.GC.KeepAlive(ᴋ725);
    ret = (int32)r0;
    if (ret == 0) {
        err = errnoErr(e1);
    }
    return (ret, err);
}

public static int32 /*ret*/ ToUnicodeEx(uint32 vkey, uint32 scancode, ж<byte> keystate, ж<uint16> pwszBuff, int32 cchBuff, uint32 flags, Handle hkl) {
    int32 ret = default;

    var ᴋ726 = keystate;
    var ᴋ727 = pwszBuff;
        var (r0, _, _) = syscall.SyscallN(procToUnicodeEx.Addr(), (uintptr)vkey, (uintptr)scancode, (uintptr)ᴋ726, (uintptr)ᴋ727, (uintptr)cchBuff, (uintptr)flags, (uintptr)hkl);
    System.GC.KeepAlive(ᴋ726);
    System.GC.KeepAlive(ᴋ727);
    ret = (int32)r0;
    return ret;
}

public static error /*err*/ UnloadKeyboardLayout(Handle hkl) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procUnloadKeyboardLayout.Addr(), (uintptr)hkl);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ CreateEnvironmentBlock(ж<ж<uint16>> block, Token token, bool inheritExisting) {
    error err = default;

    uint32 _p0 = default;
    if (inheritExisting) {
        _p0 = 1;
    }
    var ᴋ728 = block;
        var (r1, _, e1) = syscall.SyscallN(procCreateEnvironmentBlock.Addr(), (uintptr)ᴋ728, (uintptr)token, (uintptr)_p0);
    System.GC.KeepAlive(ᴋ728);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ DestroyEnvironmentBlock(ж<uint16> block) {
    error err = default;

    var ᴋ729 = block;
        var (r1, _, e1) = syscall.SyscallN(procDestroyEnvironmentBlock.Addr(), (uintptr)ᴋ729);
    System.GC.KeepAlive(ᴋ729);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ GetUserProfileDirectory(Token t, ж<uint16> dir, ж<uint32> dirLen) {
    error err = default;

    var ᴋ730 = dir;
    var ᴋ731 = dirLen;
        var (r1, _, e1) = syscall.SyscallN(procGetUserProfileDirectoryW.Addr(), (uintptr)t, (uintptr)ᴋ730, (uintptr)ᴋ731);
    System.GC.KeepAlive(ᴋ730);
    System.GC.KeepAlive(ᴋ731);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (uint32 bufSize, error err) GetFileVersionInfoSize(@string filename, ж<Handle> zeroHandle) {
    uint32 bufSize = default;
    error err = default;

    ж<uint16> _p0 = default;
    (_p0, err) = syscall.UTF16PtrFromString(filename);
    if (err != nil) {
        return (bufSize, err);
    }
    return _GetFileVersionInfoSize(_p0, zeroHandle);
}

internal static (uint32 bufSize, error err) _GetFileVersionInfoSize(ж<uint16> filename, ж<Handle> zeroHandle) {
    uint32 bufSize = default;
    error err = default;

    var ᴋ732 = filename;
    var ᴋ733 = zeroHandle;
        var (r0, _, e1) = syscall.SyscallN(procGetFileVersionInfoSizeW.Addr(), (uintptr)ᴋ732, (uintptr)ᴋ733);
    System.GC.KeepAlive(ᴋ732);
    System.GC.KeepAlive(ᴋ733);
    bufSize = (uint32)r0;
    if (bufSize == 0) {
        err = errnoErr(e1);
    }
    return (bufSize, err);
}

public static error /*err*/ GetFileVersionInfo(@string filename, uint32 handle, uint32 bufSize, @unsafe.Pointer buffer) {
    error err = default;

    ж<uint16> _p0 = default;
    (_p0, err) = syscall.UTF16PtrFromString(filename);
    if (err != nil) {
        return err;
    }
    return _GetFileVersionInfo(_p0, handle, bufSize, buffer);
}

internal static error /*err*/ _GetFileVersionInfo(ж<uint16> filename, uint32 handle, uint32 bufSize, @unsafe.Pointer buffer) {
    error err = default;

    var ᴋ734 = filename;
    var ᴋ735 = buffer;
        var (r1, _, e1) = syscall.SyscallN(procGetFileVersionInfoW.Addr(), (uintptr)ᴋ734, (uintptr)handle, (uintptr)bufSize, (uintptr)ᴋ735);
    System.GC.KeepAlive(ᴋ734);
    System.GC.KeepAlive(ᴋ735);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ VerQueryValue(@unsafe.Pointer block, @string subBlock, @unsafe.Pointer pointerToBufferPointer, ж<uint32> bufSize) {
    error err = default;

    ж<uint16> _p0 = default;
    (_p0, err) = syscall.UTF16PtrFromString(subBlock);
    if (err != nil) {
        return err;
    }
    return _VerQueryValue(block, _p0, pointerToBufferPointer, bufSize);
}

internal static error /*err*/ _VerQueryValue(@unsafe.Pointer block, ж<uint16> subBlock, @unsafe.Pointer pointerToBufferPointer, ж<uint32> bufSize) {
    error err = default;

    var ᴋ736 = block;
    var ᴋ737 = subBlock;
    var ᴋ738 = pointerToBufferPointer;
    var ᴋ739 = bufSize;
        var (r1, _, e1) = syscall.SyscallN(procVerQueryValueW.Addr(), (uintptr)ᴋ736, (uintptr)ᴋ737, (uintptr)ᴋ738, (uintptr)ᴋ739);
    System.GC.KeepAlive(ᴋ736);
    System.GC.KeepAlive(ᴋ737);
    System.GC.KeepAlive(ᴋ738);
    System.GC.KeepAlive(ᴋ739);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ TimeBeginPeriod(uint32 period) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(proctimeBeginPeriod.Addr(), (uintptr)period);
    if (r1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ TimeEndPeriod(uint32 period) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(proctimeEndPeriod.Addr(), (uintptr)period);
    if (r1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*ret*/ WinVerifyTrustEx(HWND hwnd, ж<GUID> actionId, ж<WinTrustData> data) {
    error ret = default;

    var ᴋ740 = actionId;
    var ᴋ741 = data;
        var (r0, _, _) = syscall.SyscallN(procWinVerifyTrustEx.Addr(), (uintptr)hwnd, (uintptr)ᴋ740, (uintptr)ᴋ741);
    System.GC.KeepAlive(ᴋ740);
    System.GC.KeepAlive(ᴋ741);
    if (r0 != 0) {
        ret = ((syscall.Errno)r0);
    }
    return ret;
}

public static void FreeAddrInfoW(ж<AddrinfoW> addrinfo) {
    var ᴋ742 = addrinfo;
        syscall.SyscallN(procFreeAddrInfoW.Addr(), (uintptr)ᴋ742);
    System.GC.KeepAlive(ᴋ742);
    return;
}

public static error /*sockerr*/ GetAddrInfoW(ж<uint16> nodename, ж<uint16> servicename, ж<AddrinfoW> hints, ж<ж<AddrinfoW>> result) {
    error sockerr = default;

    var ᴋ743 = nodename;
    var ᴋ744 = servicename;
    var ᴋ745 = hints;
    var ᴋ746 = result;
        var (r0, _, _) = syscall.SyscallN(procGetAddrInfoW.Addr(), (uintptr)ᴋ743, (uintptr)ᴋ744, (uintptr)ᴋ745, (uintptr)ᴋ746);
    System.GC.KeepAlive(ᴋ743);
    System.GC.KeepAlive(ᴋ744);
    System.GC.KeepAlive(ᴋ745);
    System.GC.KeepAlive(ᴋ746);
    if (r0 != 0) {
        sockerr = ((syscall.Errno)r0);
    }
    return sockerr;
}

public static error /*err*/ WSACleanup() {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procWSACleanup.Addr());
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WSADuplicateSocket(Handle s, uint32 processID, ж<WSAProtocolInfo> info) {
    error err = default;

    var ᴋ747 = info;
        var (r1, _, e1) = syscall.SyscallN(procWSADuplicateSocketW.Addr(), (uintptr)s, (uintptr)processID, (uintptr)ᴋ747);
    System.GC.KeepAlive(ᴋ747);
    if (r1 != 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static (int32 n, error err) WSAEnumProtocols(ж<int32> protocols, ж<WSAProtocolInfo> protocolBuffer, ж<uint32> bufferLength) {
    int32 n = default;
    error err = default;

    var ᴋ748 = protocols;
    var ᴋ749 = protocolBuffer;
    var ᴋ750 = bufferLength;
        var (r0, _, e1) = syscall.SyscallN(procWSAEnumProtocolsW.Addr(), (uintptr)ᴋ748, (uintptr)ᴋ749, (uintptr)ᴋ750);
    System.GC.KeepAlive(ᴋ748);
    System.GC.KeepAlive(ᴋ749);
    System.GC.KeepAlive(ᴋ750);
    n = (int32)r0;
    if (n == -1) {
        err = errnoErr(e1);
    }
    return (n, err);
}

public static error /*err*/ WSAGetOverlappedResult(Handle h, ж<Overlapped> o, ж<uint32> bytes, bool wait, ж<uint32> flags) {
    error err = default;

    uint32 _p0 = default;
    if (wait) {
        _p0 = 1;
    }
    var ᴋ751 = o;
    var ᴋ752 = bytes;
    var ᴋ753 = flags;
        var (r1, _, e1) = syscall.SyscallN(procWSAGetOverlappedResult.Addr(), (uintptr)h, (uintptr)ᴋ751, (uintptr)ᴋ752, (uintptr)_p0, (uintptr)ᴋ753);
    System.GC.KeepAlive(ᴋ751);
    System.GC.KeepAlive(ᴋ752);
    System.GC.KeepAlive(ᴋ753);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WSAIoctl(Handle s, uint32 iocc, ж<byte> inbuf, uint32 cbif, ж<byte> outbuf, uint32 cbob, ж<uint32> cbbr, ж<Overlapped> overlapped, uintptr completionRoutine) {
    error err = default;

    var ᴋ754 = inbuf;
    var ᴋ755 = outbuf;
    var ᴋ756 = cbbr;
    var ᴋ757 = overlapped;
        var (r1, _, e1) = syscall.SyscallN(procWSAIoctl.Addr(), (uintptr)s, (uintptr)iocc, (uintptr)ᴋ754, (uintptr)cbif, (uintptr)ᴋ755, (uintptr)cbob, (uintptr)ᴋ756, (uintptr)ᴋ757, (uintptr)completionRoutine);
    System.GC.KeepAlive(ᴋ754);
    System.GC.KeepAlive(ᴋ755);
    System.GC.KeepAlive(ᴋ756);
    System.GC.KeepAlive(ᴋ757);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WSALookupServiceBegin(ж<WSAQUERYSET> querySet, uint32 flags, ж<Handle> handle) {
    error err = default;

    var ᴋ758 = querySet;
    var ᴋ759 = handle;
        var (r1, _, e1) = syscall.SyscallN(procWSALookupServiceBeginW.Addr(), (uintptr)ᴋ758, (uintptr)flags, (uintptr)ᴋ759);
    System.GC.KeepAlive(ᴋ758);
    System.GC.KeepAlive(ᴋ759);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WSALookupServiceEnd(Handle handle) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procWSALookupServiceEnd.Addr(), (uintptr)handle);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WSALookupServiceNext(Handle handle, uint32 flags, ж<int32> size, ж<WSAQUERYSET> querySet) {
    error err = default;

    var ᴋ760 = size;
    var ᴋ761 = querySet;
        var (r1, _, e1) = syscall.SyscallN(procWSALookupServiceNextW.Addr(), (uintptr)handle, (uintptr)flags, (uintptr)ᴋ760, (uintptr)ᴋ761);
    System.GC.KeepAlive(ᴋ760);
    System.GC.KeepAlive(ᴋ761);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WSARecv(Handle s, ж<WSABuf> bufs, uint32 bufcnt, ж<uint32> recvd, ж<uint32> flags, ж<Overlapped> overlapped, ж<byte> croutine) {
    error err = default;

    var ᴋ762 = bufs;
    var ᴋ763 = recvd;
    var ᴋ764 = flags;
    var ᴋ765 = overlapped;
    var ᴋ766 = croutine;
        var (r1, _, e1) = syscall.SyscallN(procWSARecv.Addr(), (uintptr)s, (uintptr)ᴋ762, (uintptr)bufcnt, (uintptr)ᴋ763, (uintptr)ᴋ764, (uintptr)ᴋ765, (uintptr)ᴋ766);
    System.GC.KeepAlive(ᴋ762);
    System.GC.KeepAlive(ᴋ763);
    System.GC.KeepAlive(ᴋ764);
    System.GC.KeepAlive(ᴋ765);
    System.GC.KeepAlive(ᴋ766);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WSARecvFrom(Handle s, ж<WSABuf> bufs, uint32 bufcnt, ж<uint32> recvd, ж<uint32> flags, ж<RawSockaddrAny> from, ж<int32> fromlen, ж<Overlapped> overlapped, ж<byte> croutine) {
    error err = default;

    var ᴋ767 = bufs;
    var ᴋ768 = recvd;
    var ᴋ769 = flags;
    var ᴋ770 = from;
    var ᴋ771 = fromlen;
    var ᴋ772 = overlapped;
    var ᴋ773 = croutine;
        var (r1, _, e1) = syscall.SyscallN(procWSARecvFrom.Addr(), (uintptr)s, (uintptr)ᴋ767, (uintptr)bufcnt, (uintptr)ᴋ768, (uintptr)ᴋ769, (uintptr)ᴋ770, (uintptr)ᴋ771, (uintptr)ᴋ772, (uintptr)ᴋ773);
    System.GC.KeepAlive(ᴋ767);
    System.GC.KeepAlive(ᴋ768);
    System.GC.KeepAlive(ᴋ769);
    System.GC.KeepAlive(ᴋ770);
    System.GC.KeepAlive(ᴋ771);
    System.GC.KeepAlive(ᴋ772);
    System.GC.KeepAlive(ᴋ773);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WSASend(Handle s, ж<WSABuf> bufs, uint32 bufcnt, ж<uint32> sent, uint32 flags, ж<Overlapped> overlapped, ж<byte> croutine) {
    error err = default;

    var ᴋ774 = bufs;
    var ᴋ775 = sent;
    var ᴋ776 = overlapped;
    var ᴋ777 = croutine;
        var (r1, _, e1) = syscall.SyscallN(procWSASend.Addr(), (uintptr)s, (uintptr)ᴋ774, (uintptr)bufcnt, (uintptr)ᴋ775, (uintptr)flags, (uintptr)ᴋ776, (uintptr)ᴋ777);
    System.GC.KeepAlive(ᴋ774);
    System.GC.KeepAlive(ᴋ775);
    System.GC.KeepAlive(ᴋ776);
    System.GC.KeepAlive(ᴋ777);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ WSASendTo(Handle s, ж<WSABuf> bufs, uint32 bufcnt, ж<uint32> sent, uint32 flags, ж<RawSockaddrAny> to, int32 tolen, ж<Overlapped> overlapped, ж<byte> croutine) {
    error err = default;

    var ᴋ778 = bufs;
    var ᴋ779 = sent;
    var ᴋ780 = to;
    var ᴋ781 = overlapped;
    var ᴋ782 = croutine;
        var (r1, _, e1) = syscall.SyscallN(procWSASendTo.Addr(), (uintptr)s, (uintptr)ᴋ778, (uintptr)bufcnt, (uintptr)ᴋ779, (uintptr)flags, (uintptr)ᴋ780, (uintptr)tolen, (uintptr)ᴋ781, (uintptr)ᴋ782);
    System.GC.KeepAlive(ᴋ778);
    System.GC.KeepAlive(ᴋ779);
    System.GC.KeepAlive(ᴋ780);
    System.GC.KeepAlive(ᴋ781);
    System.GC.KeepAlive(ᴋ782);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static (Handle handle, error err) WSASocket(int32 af, int32 typ, int32 protocol, ж<WSAProtocolInfo> protoInfo, uint32 group, uint32 flags) {
    Handle handle = default;
    error err = default;

    var ᴋ783 = protoInfo;
        var (r0, _, e1) = syscall.SyscallN(procWSASocketW.Addr(), (uintptr)af, (uintptr)typ, (uintptr)protocol, (uintptr)ᴋ783, (uintptr)group, (uintptr)flags);
    System.GC.KeepAlive(ᴋ783);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*sockerr*/ WSAStartup(uint32 verreq, ж<WSAData> data) {
    error sockerr = default;

    var ᴋ784 = data;
        var (r0, _, _) = syscall.SyscallN(procWSAStartup.Addr(), (uintptr)verreq, (uintptr)ᴋ784);
    System.GC.KeepAlive(ᴋ784);
    if (r0 != 0) {
        sockerr = ((syscall.Errno)r0);
    }
    return sockerr;
}

internal static error /*err*/ bind(Handle s, @unsafe.Pointer name, int32 namelen) {
    error err = default;

    var ᴋ785 = name;
        var (r1, _, e1) = syscall.SyscallN(procbind.Addr(), (uintptr)s, (uintptr)ᴋ785, (uintptr)namelen);
    System.GC.KeepAlive(ᴋ785);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Closesocket(Handle s) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procclosesocket.Addr(), (uintptr)s);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ connect(Handle s, @unsafe.Pointer name, int32 namelen) {
    error err = default;

    var ᴋ786 = name;
        var (r1, _, e1) = syscall.SyscallN(procconnect.Addr(), (uintptr)s, (uintptr)ᴋ786, (uintptr)namelen);
    System.GC.KeepAlive(ᴋ786);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static (ж<Hostent> h, error err) GetHostByName(@string name) {
    ж<Hostent> h = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = syscall.BytePtrFromString(name);
    if (err != nil) {
        return (h, err);
    }
    return _GetHostByName(_p0);
}

internal static (ж<Hostent> h, error err) _GetHostByName(ж<byte> name) {
    ж<Hostent> h = default;
    error err = default;

    var ᴋ787 = name;
        var (r0, _, e1) = syscall.SyscallN(procgethostbyname.Addr(), (uintptr)ᴋ787);
    System.GC.KeepAlive(ᴋ787);
    h = @unsafe.Pointer.To<Hostent>((@unsafe.Pointer)r0);
    if (h == nil) {
        err = errnoErr(e1);
    }
    return (h, err);
}

internal static error /*err*/ getpeername(Handle s, ж<RawSockaddrAny> rsa, ж<int32> addrlen) {
    error err = default;

    var ᴋ788 = rsa;
    var ᴋ789 = addrlen;
        var (r1, _, e1) = syscall.SyscallN(procgetpeername.Addr(), (uintptr)s, (uintptr)ᴋ788, (uintptr)ᴋ789);
    System.GC.KeepAlive(ᴋ788);
    System.GC.KeepAlive(ᴋ789);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static (ж<Protoent> p, error err) GetProtoByName(@string name) {
    ж<Protoent> p = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = syscall.BytePtrFromString(name);
    if (err != nil) {
        return (p, err);
    }
    return _GetProtoByName(_p0);
}

internal static (ж<Protoent> p, error err) _GetProtoByName(ж<byte> name) {
    ж<Protoent> p = default;
    error err = default;

    var ᴋ790 = name;
        var (r0, _, e1) = syscall.SyscallN(procgetprotobyname.Addr(), (uintptr)ᴋ790);
    System.GC.KeepAlive(ᴋ790);
    p = @unsafe.Pointer.To<Protoent>((@unsafe.Pointer)r0);
    if (p == nil) {
        err = errnoErr(e1);
    }
    return (p, err);
}

public static (ж<Servent> s, error err) GetServByName(@string name, @string proto) {
    ж<Servent> s = default;
    error err = default;

    ж<byte> _p0 = default;
    (_p0, err) = syscall.BytePtrFromString(name);
    if (err != nil) {
        return (s, err);
    }
    ж<byte> _p1 = default;
    (_p1, err) = syscall.BytePtrFromString(proto);
    if (err != nil) {
        return (s, err);
    }
    return _GetServByName(_p0, _p1);
}

internal static (ж<Servent> s, error err) _GetServByName(ж<byte> name, ж<byte> proto) {
    ж<Servent> s = default;
    error err = default;

    var ᴋ791 = name;
    var ᴋ792 = proto;
        var (r0, _, e1) = syscall.SyscallN(procgetservbyname.Addr(), (uintptr)ᴋ791, (uintptr)ᴋ792);
    System.GC.KeepAlive(ᴋ791);
    System.GC.KeepAlive(ᴋ792);
    s = @unsafe.Pointer.To<Servent>((@unsafe.Pointer)r0);
    if (s == nil) {
        err = errnoErr(e1);
    }
    return (s, err);
}

internal static error /*err*/ getsockname(Handle s, ж<RawSockaddrAny> rsa, ж<int32> addrlen) {
    error err = default;

    var ᴋ793 = rsa;
    var ᴋ794 = addrlen;
        var (r1, _, e1) = syscall.SyscallN(procgetsockname.Addr(), (uintptr)s, (uintptr)ᴋ793, (uintptr)ᴋ794);
    System.GC.KeepAlive(ᴋ793);
    System.GC.KeepAlive(ᴋ794);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Getsockopt(Handle s, int32 level, int32 optname, ж<byte> optval, ж<int32> optlen) {
    error err = default;

    var ᴋ795 = optval;
    var ᴋ796 = optlen;
        var (r1, _, e1) = syscall.SyscallN(procgetsockopt.Addr(), (uintptr)s, (uintptr)level, (uintptr)optname, (uintptr)ᴋ795, (uintptr)ᴋ796);
    System.GC.KeepAlive(ᴋ795);
    System.GC.KeepAlive(ᴋ796);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ listen(Handle s, int32 backlog) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(proclisten.Addr(), (uintptr)s, (uintptr)backlog);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static uint16 /*u*/ Ntohs(uint16 netshort) {
    uint16 u = default;

    var (r0, _, _) = syscall.SyscallN(procntohs.Addr(), (uintptr)netshort);
    u = (uint16)r0;
    return u;
}

internal static (int32 n, error err) recvfrom(Handle s, slice<byte> buf, int32 flags, ж<RawSockaddrAny> from, ж<int32> fromlen) {
    int32 n = default;
    error err = default;

    ж<byte> _p0 = default;
    if (len(buf) > 0) {
        _p0 = Ꮡ(buf, 0);
    }
    var ᴋ797 = _p0;
    var ᴋ798 = from;
    var ᴋ799 = fromlen;
        var (r0, _, e1) = syscall.SyscallN(procrecvfrom.Addr(), (uintptr)s, (uintptr)ᴋ797, (uintptr)len(buf), (uintptr)flags, (uintptr)ᴋ798, (uintptr)ᴋ799);
    System.GC.KeepAlive(ᴋ797);
    System.GC.KeepAlive(ᴋ798);
    System.GC.KeepAlive(ᴋ799);
    n = (int32)r0;
    if (n == -1) {
        err = errnoErr(e1);
    }
    return (n, err);
}

internal static error /*err*/ sendto(Handle s, slice<byte> buf, int32 flags, @unsafe.Pointer to, int32 tolen) {
    error err = default;

    ж<byte> _p0 = default;
    if (len(buf) > 0) {
        _p0 = Ꮡ(buf, 0);
    }
    var ᴋ800 = _p0;
    var ᴋ801 = to;
        var (r1, _, e1) = syscall.SyscallN(procsendto.Addr(), (uintptr)s, (uintptr)ᴋ800, (uintptr)len(buf), (uintptr)flags, (uintptr)ᴋ801, (uintptr)tolen);
    System.GC.KeepAlive(ᴋ800);
    System.GC.KeepAlive(ᴋ801);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

public static error /*err*/ Setsockopt(Handle s, int32 level, int32 optname, ж<byte> optval, int32 optlen) {
    error err = default;

    var ᴋ802 = optval;
        var (r1, _, e1) = syscall.SyscallN(procsetsockopt.Addr(), (uintptr)s, (uintptr)level, (uintptr)optname, (uintptr)ᴋ802, (uintptr)optlen);
    System.GC.KeepAlive(ᴋ802);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

internal static error /*err*/ shutdown(Handle s, int32 how) {
    error err = default;

    var (r1, _, e1) = syscall.SyscallN(procshutdown.Addr(), (uintptr)s, (uintptr)how);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return err;
}

internal static (Handle handle, error err) socket(int32 af, int32 typ, int32 protocol) {
    Handle handle = default;
    error err = default;

    var (r0, _, e1) = syscall.SyscallN(procsocket.Addr(), (uintptr)af, (uintptr)typ, (uintptr)protocol);
    handle = (Handle)r0;
    if (handle == InvalidHandle) {
        err = errnoErr(e1);
    }
    return (handle, err);
}

public static error /*err*/ WTSEnumerateSessions(Handle handle, uint32 reserved, uint32 version, ж<ж<WTS_SESSION_INFO>> sessions, ж<uint32> count) {
    error err = default;

    var ᴋ803 = sessions;
    var ᴋ804 = count;
        var (r1, _, e1) = syscall.SyscallN(procWTSEnumerateSessionsW.Addr(), (uintptr)handle, (uintptr)reserved, (uintptr)version, (uintptr)ᴋ803, (uintptr)ᴋ804);
    System.GC.KeepAlive(ᴋ803);
    System.GC.KeepAlive(ᴋ804);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

public static void WTSFreeMemory(uintptr ptr) {
    syscall.SyscallN(procWTSFreeMemory.Addr(), (uintptr)ptr);
    return;
}

public static error /*err*/ WTSQueryUserToken(uint32 session, ж<Token> token) {
    error err = default;

    var ᴋ805 = token;
        var (r1, _, e1) = syscall.SyscallN(procWTSQueryUserToken.Addr(), (uintptr)session, (uintptr)ᴋ805);
    System.GC.KeepAlive(ᴋ805);
    if (r1 == 0) {
        err = errnoErr(e1);
    }
    return err;
}

} // end windows_package
