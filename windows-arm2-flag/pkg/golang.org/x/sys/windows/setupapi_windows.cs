// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using encoding;
using static windows_package;

partial class windows_package {

public static Errno ERROR_EXPECTED_SECTION_NAME => /* 0x20000000 | 0xC0000000 | 0 */ unchecked((Errno)3758096384);
public static Errno ERROR_BAD_SECTION_NAME_LINE => /* 0x20000000 | 0xC0000000 | 1 */ unchecked((Errno)3758096385);
public static Errno ERROR_SECTION_NAME_TOO_LONG => /* 0x20000000 | 0xC0000000 | 2 */ unchecked((Errno)3758096386);
public static Errno ERROR_GENERAL_SYNTAX => /* 0x20000000 | 0xC0000000 | 3 */ unchecked((Errno)3758096387);
public static Errno ERROR_WRONG_INF_STYLE => /* 0x20000000 | 0xC0000000 | 0x100 */ unchecked((Errno)3758096640);
public static Errno ERROR_SECTION_NOT_FOUND => /* 0x20000000 | 0xC0000000 | 0x101 */ unchecked((Errno)3758096641);
public static Errno ERROR_LINE_NOT_FOUND => /* 0x20000000 | 0xC0000000 | 0x102 */ unchecked((Errno)3758096642);
public static Errno ERROR_NO_BACKUP => /* 0x20000000 | 0xC0000000 | 0x103 */ unchecked((Errno)3758096643);
public static Errno ERROR_NO_ASSOCIATED_CLASS => /* 0x20000000 | 0xC0000000 | 0x200 */ unchecked((Errno)3758096896);
public static Errno ERROR_CLASS_MISMATCH => /* 0x20000000 | 0xC0000000 | 0x201 */ unchecked((Errno)3758096897);
public static Errno ERROR_DUPLICATE_FOUND => /* 0x20000000 | 0xC0000000 | 0x202 */ unchecked((Errno)3758096898);
public static Errno ERROR_NO_DRIVER_SELECTED => /* 0x20000000 | 0xC0000000 | 0x203 */ unchecked((Errno)3758096899);
public static Errno ERROR_KEY_DOES_NOT_EXIST => /* 0x20000000 | 0xC0000000 | 0x204 */ unchecked((Errno)3758096900);
public static Errno ERROR_INVALID_DEVINST_NAME => /* 0x20000000 | 0xC0000000 | 0x205 */ unchecked((Errno)3758096901);
public static Errno ERROR_INVALID_CLASS => /* 0x20000000 | 0xC0000000 | 0x206 */ unchecked((Errno)3758096902);
public static Errno ERROR_DEVINST_ALREADY_EXISTS => /* 0x20000000 | 0xC0000000 | 0x207 */ unchecked((Errno)3758096903);
public static Errno ERROR_DEVINFO_NOT_REGISTERED => /* 0x20000000 | 0xC0000000 | 0x208 */ unchecked((Errno)3758096904);
public static Errno ERROR_INVALID_REG_PROPERTY => /* 0x20000000 | 0xC0000000 | 0x209 */ unchecked((Errno)3758096905);
public static Errno ERROR_NO_INF => /* 0x20000000 | 0xC0000000 | 0x20A */ unchecked((Errno)3758096906);
public static Errno ERROR_NO_SUCH_DEVINST => /* 0x20000000 | 0xC0000000 | 0x20B */ unchecked((Errno)3758096907);
public static Errno ERROR_CANT_LOAD_CLASS_ICON => /* 0x20000000 | 0xC0000000 | 0x20C */ unchecked((Errno)3758096908);
public static Errno ERROR_INVALID_CLASS_INSTALLER => /* 0x20000000 | 0xC0000000 | 0x20D */ unchecked((Errno)3758096909);
public static Errno ERROR_DI_DO_DEFAULT => /* 0x20000000 | 0xC0000000 | 0x20E */ unchecked((Errno)3758096910);
public static Errno ERROR_DI_NOFILECOPY => /* 0x20000000 | 0xC0000000 | 0x20F */ unchecked((Errno)3758096911);
public static Errno ERROR_INVALID_HWPROFILE => /* 0x20000000 | 0xC0000000 | 0x210 */ unchecked((Errno)3758096912);
public static Errno ERROR_NO_DEVICE_SELECTED => /* 0x20000000 | 0xC0000000 | 0x211 */ unchecked((Errno)3758096913);
public static Errno ERROR_DEVINFO_LIST_LOCKED => /* 0x20000000 | 0xC0000000 | 0x212 */ unchecked((Errno)3758096914);
public static Errno ERROR_DEVINFO_DATA_LOCKED => /* 0x20000000 | 0xC0000000 | 0x213 */ unchecked((Errno)3758096915);
public static Errno ERROR_DI_BAD_PATH => /* 0x20000000 | 0xC0000000 | 0x214 */ unchecked((Errno)3758096916);
public static Errno ERROR_NO_CLASSINSTALL_PARAMS => /* 0x20000000 | 0xC0000000 | 0x215 */ unchecked((Errno)3758096917);
public static Errno ERROR_FILEQUEUE_LOCKED => /* 0x20000000 | 0xC0000000 | 0x216 */ unchecked((Errno)3758096918);
public static Errno ERROR_BAD_SERVICE_INSTALLSECT => /* 0x20000000 | 0xC0000000 | 0x217 */ unchecked((Errno)3758096919);
public static Errno ERROR_NO_CLASS_DRIVER_LIST => /* 0x20000000 | 0xC0000000 | 0x218 */ unchecked((Errno)3758096920);
public static Errno ERROR_NO_ASSOCIATED_SERVICE => /* 0x20000000 | 0xC0000000 | 0x219 */ unchecked((Errno)3758096921);
public static Errno ERROR_NO_DEFAULT_DEVICE_INTERFACE => /* 0x20000000 | 0xC0000000 | 0x21A */ unchecked((Errno)3758096922);
public static Errno ERROR_DEVICE_INTERFACE_ACTIVE => /* 0x20000000 | 0xC0000000 | 0x21B */ unchecked((Errno)3758096923);
public static Errno ERROR_DEVICE_INTERFACE_REMOVED => /* 0x20000000 | 0xC0000000 | 0x21C */ unchecked((Errno)3758096924);
public static Errno ERROR_BAD_INTERFACE_INSTALLSECT => /* 0x20000000 | 0xC0000000 | 0x21D */ unchecked((Errno)3758096925);
public static Errno ERROR_NO_SUCH_INTERFACE_CLASS => /* 0x20000000 | 0xC0000000 | 0x21E */ unchecked((Errno)3758096926);
public static Errno ERROR_INVALID_REFERENCE_STRING => /* 0x20000000 | 0xC0000000 | 0x21F */ unchecked((Errno)3758096927);
public static Errno ERROR_INVALID_MACHINENAME => /* 0x20000000 | 0xC0000000 | 0x220 */ unchecked((Errno)3758096928);
public static Errno ERROR_REMOTE_COMM_FAILURE => /* 0x20000000 | 0xC0000000 | 0x221 */ unchecked((Errno)3758096929);
public static Errno ERROR_MACHINE_UNAVAILABLE => /* 0x20000000 | 0xC0000000 | 0x222 */ unchecked((Errno)3758096930);
public static Errno ERROR_NO_CONFIGMGR_SERVICES => /* 0x20000000 | 0xC0000000 | 0x223 */ unchecked((Errno)3758096931);
public static Errno ERROR_INVALID_PROPPAGE_PROVIDER => /* 0x20000000 | 0xC0000000 | 0x224 */ unchecked((Errno)3758096932);
public static Errno ERROR_NO_SUCH_DEVICE_INTERFACE => /* 0x20000000 | 0xC0000000 | 0x225 */ unchecked((Errno)3758096933);
public static Errno ERROR_DI_POSTPROCESSING_REQUIRED => /* 0x20000000 | 0xC0000000 | 0x226 */ unchecked((Errno)3758096934);
public static Errno ERROR_INVALID_COINSTALLER => /* 0x20000000 | 0xC0000000 | 0x227 */ unchecked((Errno)3758096935);
public static Errno ERROR_NO_COMPAT_DRIVERS => /* 0x20000000 | 0xC0000000 | 0x228 */ unchecked((Errno)3758096936);
public static Errno ERROR_NO_DEVICE_ICON => /* 0x20000000 | 0xC0000000 | 0x229 */ unchecked((Errno)3758096937);
public static Errno ERROR_INVALID_INF_LOGCONFIG => /* 0x20000000 | 0xC0000000 | 0x22A */ unchecked((Errno)3758096938);
public static Errno ERROR_DI_DONT_INSTALL => /* 0x20000000 | 0xC0000000 | 0x22B */ unchecked((Errno)3758096939);
public static Errno ERROR_INVALID_FILTER_DRIVER => /* 0x20000000 | 0xC0000000 | 0x22C */ unchecked((Errno)3758096940);
public static Errno ERROR_NON_WINDOWS_NT_DRIVER => /* 0x20000000 | 0xC0000000 | 0x22D */ unchecked((Errno)3758096941);
public static Errno ERROR_NON_WINDOWS_DRIVER => /* 0x20000000 | 0xC0000000 | 0x22E */ unchecked((Errno)3758096942);
public static Errno ERROR_NO_CATALOG_FOR_OEM_INF => /* 0x20000000 | 0xC0000000 | 0x22F */ unchecked((Errno)3758096943);
public static Errno ERROR_DEVINSTALL_QUEUE_NONNATIVE => /* 0x20000000 | 0xC0000000 | 0x230 */ unchecked((Errno)3758096944);
public static Errno ERROR_NOT_DISABLEABLE => /* 0x20000000 | 0xC0000000 | 0x231 */ unchecked((Errno)3758096945);
public static Errno ERROR_CANT_REMOVE_DEVINST => /* 0x20000000 | 0xC0000000 | 0x232 */ unchecked((Errno)3758096946);
public static Errno ERROR_INVALID_TARGET => /* 0x20000000 | 0xC0000000 | 0x233 */ unchecked((Errno)3758096947);
public static Errno ERROR_DRIVER_NONNATIVE => /* 0x20000000 | 0xC0000000 | 0x234 */ unchecked((Errno)3758096948);
public static Errno ERROR_IN_WOW64 => /* 0x20000000 | 0xC0000000 | 0x235 */ unchecked((Errno)3758096949);
public static Errno ERROR_SET_SYSTEM_RESTORE_POINT => /* 0x20000000 | 0xC0000000 | 0x236 */ unchecked((Errno)3758096950);
public static Errno ERROR_SCE_DISABLED => /* 0x20000000 | 0xC0000000 | 0x238 */ unchecked((Errno)3758096952);
public static Errno ERROR_UNKNOWN_EXCEPTION => /* 0x20000000 | 0xC0000000 | 0x239 */ unchecked((Errno)3758096953);
public static Errno ERROR_PNP_REGISTRY_ERROR => /* 0x20000000 | 0xC0000000 | 0x23A */ unchecked((Errno)3758096954);
public static Errno ERROR_REMOTE_REQUEST_UNSUPPORTED => /* 0x20000000 | 0xC0000000 | 0x23B */ unchecked((Errno)3758096955);
public static Errno ERROR_NOT_AN_INSTALLED_OEM_INF => /* 0x20000000 | 0xC0000000 | 0x23C */ unchecked((Errno)3758096956);
public static Errno ERROR_INF_IN_USE_BY_DEVICES => /* 0x20000000 | 0xC0000000 | 0x23D */ unchecked((Errno)3758096957);
public static Errno ERROR_DI_FUNCTION_OBSOLETE => /* 0x20000000 | 0xC0000000 | 0x23E */ unchecked((Errno)3758096958);
public static Errno ERROR_NO_AUTHENTICODE_CATALOG => /* 0x20000000 | 0xC0000000 | 0x23F */ unchecked((Errno)3758096959);
public static Errno ERROR_AUTHENTICODE_DISALLOWED => /* 0x20000000 | 0xC0000000 | 0x240 */ unchecked((Errno)3758096960);
public static Errno ERROR_AUTHENTICODE_TRUSTED_PUBLISHER => /* 0x20000000 | 0xC0000000 | 0x241 */ unchecked((Errno)3758096961);
public static Errno ERROR_AUTHENTICODE_TRUST_NOT_ESTABLISHED => /* 0x20000000 | 0xC0000000 | 0x242 */ unchecked((Errno)3758096962);
public static Errno ERROR_AUTHENTICODE_PUBLISHER_NOT_TRUSTED => /* 0x20000000 | 0xC0000000 | 0x243 */ unchecked((Errno)3758096963);
public static Errno ERROR_SIGNATURE_OSATTRIBUTE_MISMATCH => /* 0x20000000 | 0xC0000000 | 0x244 */ unchecked((Errno)3758096964);
public static Errno ERROR_ONLY_VALIDATE_VIA_AUTHENTICODE => /* 0x20000000 | 0xC0000000 | 0x245 */ unchecked((Errno)3758096965);
public static Errno ERROR_DEVICE_INSTALLER_NOT_READY => /* 0x20000000 | 0xC0000000 | 0x246 */ unchecked((Errno)3758096966);
public static Errno ERROR_DRIVER_STORE_ADD_FAILED => /* 0x20000000 | 0xC0000000 | 0x247 */ unchecked((Errno)3758096967);
public static Errno ERROR_DEVICE_INSTALL_BLOCKED => /* 0x20000000 | 0xC0000000 | 0x248 */ unchecked((Errno)3758096968);
public static Errno ERROR_DRIVER_INSTALL_BLOCKED => /* 0x20000000 | 0xC0000000 | 0x249 */ unchecked((Errno)3758096969);
public static Errno ERROR_WRONG_INF_TYPE => /* 0x20000000 | 0xC0000000 | 0x24A */ unchecked((Errno)3758096970);
public static Errno ERROR_FILE_HASH_NOT_IN_CATALOG => /* 0x20000000 | 0xC0000000 | 0x24B */ unchecked((Errno)3758096971);
public static Errno ERROR_DRIVER_STORE_DELETE_FAILED => /* 0x20000000 | 0xC0000000 | 0x24C */ unchecked((Errno)3758096972);
public static Errno ERROR_UNRECOVERABLE_STACK_OVERFLOW => /* 0x20000000 | 0xC0000000 | 0x300 */ unchecked((Errno)3758097152);
public static Errno EXCEPTION_SPAPI_UNRECOVERABLE_STACK_OVERFLOW => /* ERROR_UNRECOVERABLE_STACK_OVERFLOW */ unchecked((Errno)3758097152);
public static Errno ERROR_NO_DEFAULT_INTERFACE_DEVICE => /* ERROR_NO_DEFAULT_DEVICE_INTERFACE */ unchecked((Errno)3758096922);
public static Errno ERROR_INTERFACE_DEVICE_ACTIVE => /* ERROR_DEVICE_INTERFACE_ACTIVE */ unchecked((Errno)3758096923);
public static Errno ERROR_INTERFACE_DEVICE_REMOVED => /* ERROR_DEVICE_INTERFACE_REMOVED */ unchecked((Errno)3758096924);
public static Errno ERROR_NO_SUCH_INTERFACE_DEVICE => /* ERROR_NO_SUCH_DEVICE_INTERFACE */ unchecked((Errno)3758096933);

public static UntypedInt MAX_DEVICE_ID_LEN => 200;
public static UntypedInt MAX_DEVNODE_ID_LEN => /* MAX_DEVICE_ID_LEN */ 200;
public static UntypedInt MAX_GUID_STRING_LEN => 39;
public static UntypedInt MAX_CLASS_NAME_LEN => 32;
public static UntypedInt MAX_PROFILE_LEN => 80;
public static UntypedInt MAX_CONFIG_VALUE => 9999;
public static UntypedInt MAX_INSTANCE_VALUE => 9999;
public static UntypedInt CONFIGMG_VERSION => 0x0400;

public static UntypedInt LINE_LEN => 256;
public static UntypedInt MAX_INF_STRING_LENGTH => 4096;
public static UntypedInt MAX_INF_SECTION_NAME_LENGTH => 255;
public static UntypedInt MAX_TITLE_LEN => 60;
public static UntypedInt MAX_INSTRUCTION_LEN => 256;
public static UntypedInt MAX_LABEL_LEN => 30;
public static UntypedInt MAX_SERVICE_NAME_LEN => 256;
public static UntypedInt MAX_SUBTITLE_LEN => 256;

public static UntypedInt SP_MAX_MACHINENAME_LENGTH => /* MAX_PATH + 3 */ 263;

partial struct HSPFILEQ /*num:uintptr*/;

partial struct DevInfo /*num:uintptr*/;

partial struct DEVINST /*num:uint32*/;

[global::go.golib.GoInlineLayout(Size = 32, Align = 8), StructLayout(LayoutKind.Explicit, Size = 32)] partial struct DevInfoData {
    [FieldOffset(0)] internal uint32 size;
    [FieldOffset(4)] public GUID ClassGUID;
    [FieldOffset(20)] public DEVINST DevInst;
    [FieldOffset(24)] internal uintptr _;
}

partial struct DevInfoListDetailData {
    internal uint32 size;
    public GUID ClassGUID;
    public Handle RemoteMachineHandle;
    internal array<uint16> remoteMachineName = new(SP_MAX_MACHINENAME_LENGTH);
}

} partial class windowsᴛDevInfoListDetailData {

internal static uint32 unsafeSizeOf(this ref DevInfoListDetailData _) {
    if (/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8 == 4) {
        return (uint32)(/* unsafe.Offsetof(DevInfoListDetailData{}.remoteMachineName) */ (uintptr)32 + /* unsafe.Sizeof(DevInfoListDetailData{}.remoteMachineName) */ (uintptr)526);
    }
    return (uint32)/* unsafe.Sizeof(DevInfoListDetailData{}) */ (uintptr)560;
}

public static @string RemoteMachineName(this ref DevInfoListDetailData data) {
    return UTF16ToString(data.remoteMachineName[..]);
}

public static error SetRemoteMachineName(this ref DevInfoListDetailData data, @string remoteMachineName) {
    var (str, err) = UTF16FromString(remoteMachineName);
    if (err != nil) {
        return err;
    }
    copy(data.remoteMachineName[..], str);
    return default;
}

} partial class windows_package {

partial struct DI_FUNCTION /*num:uint32*/;

public static DI_FUNCTION DIF_SELECTDEVICE => 0x00000001;
public static DI_FUNCTION DIF_INSTALLDEVICE => 0x00000002;
public static DI_FUNCTION DIF_ASSIGNRESOURCES => 0x00000003;
public static DI_FUNCTION DIF_PROPERTIES => 0x00000004;
public static DI_FUNCTION DIF_REMOVE => 0x00000005;
public static DI_FUNCTION DIF_FIRSTTIMESETUP => 0x00000006;
public static DI_FUNCTION DIF_FOUNDDEVICE => 0x00000007;
public static DI_FUNCTION DIF_SELECTCLASSDRIVERS => 0x00000008;
public static DI_FUNCTION DIF_VALIDATECLASSDRIVERS => 0x00000009;
public static DI_FUNCTION DIF_INSTALLCLASSDRIVERS => 0x0000000A;
public static DI_FUNCTION DIF_CALCDISKSPACE => 0x0000000B;
public static DI_FUNCTION DIF_DESTROYPRIVATEDATA => 0x0000000C;
public static DI_FUNCTION DIF_VALIDATEDRIVER => 0x0000000D;
public static DI_FUNCTION DIF_DETECT => 0x0000000F;
public static DI_FUNCTION DIF_INSTALLWIZARD => 0x00000010;
public static DI_FUNCTION DIF_DESTROYWIZARDDATA => 0x00000011;
public static DI_FUNCTION DIF_PROPERTYCHANGE => 0x00000012;
public static DI_FUNCTION DIF_ENABLECLASS => 0x00000013;
public static DI_FUNCTION DIF_DETECTVERIFY => 0x00000014;
public static DI_FUNCTION DIF_INSTALLDEVICEFILES => 0x00000015;
public static DI_FUNCTION DIF_UNREMOVE => 0x00000016;
public static DI_FUNCTION DIF_SELECTBESTCOMPATDRV => 0x00000017;
public static DI_FUNCTION DIF_ALLOW_INSTALL => 0x00000018;
public static DI_FUNCTION DIF_REGISTERDEVICE => 0x00000019;
public static DI_FUNCTION DIF_NEWDEVICEWIZARD_PRESELECT => 0x0000001A;
public static DI_FUNCTION DIF_NEWDEVICEWIZARD_SELECT => 0x0000001B;
public static DI_FUNCTION DIF_NEWDEVICEWIZARD_PREANALYZE => 0x0000001C;
public static DI_FUNCTION DIF_NEWDEVICEWIZARD_POSTANALYZE => 0x0000001D;
public static DI_FUNCTION DIF_NEWDEVICEWIZARD_FINISHINSTALL => 0x0000001E;
public static DI_FUNCTION DIF_INSTALLINTERFACES => 0x00000020;
public static DI_FUNCTION DIF_DETECTCANCEL => 0x00000021;
public static DI_FUNCTION DIF_REGISTER_COINSTALLERS => 0x00000022;
public static DI_FUNCTION DIF_ADDPROPERTYPAGE_ADVANCED => 0x00000023;
public static DI_FUNCTION DIF_ADDPROPERTYPAGE_BASIC => 0x00000024;
public static DI_FUNCTION DIF_TROUBLESHOOTER => 0x00000026;
public static DI_FUNCTION DIF_POWERMESSAGEWAKE => 0x00000027;
public static DI_FUNCTION DIF_ADDREMOTEPROPERTYPAGE_ADVANCED => 0x00000028;
public static DI_FUNCTION DIF_UPDATEDRIVER_UI => 0x00000029;
public static DI_FUNCTION DIF_FINISHINSTALL_ACTION => 0x0000002A;

partial struct DevInstallParams {
    internal uint32 size;
    public DI_FLAGS Flags;
    public DI_FLAGSEX FlagsEx;
    internal uintptr hwndParent;
    public uintptr InstallMsgHandler;
    public uintptr InstallMsgHandlerContext;
    public HSPFILEQ FileQueue;
    internal uintptr _;
    internal uint32 __;
    internal array<uint16> driverPath = new(MAX_PATH);
}

} partial class windowsᴛDevInstallParams {

public static @string DriverPath(this ref DevInstallParams @params) {
    return UTF16ToString(@params.driverPath[..]);
}

public static error SetDriverPath(this ref DevInstallParams @params, @string driverPath) {
    var (str, err) = UTF16FromString(driverPath);
    if (err != nil) {
        return err;
    }
    copy(@params.driverPath[..], str);
    return default;
}

} partial class windows_package {

partial struct DI_FLAGS /*num:uint32*/;

public static DI_FLAGS DI_SHOWOEM => 0x00000001;
public static DI_FLAGS DI_SHOWCOMPAT => 0x00000002;
public static DI_FLAGS DI_SHOWCLASS => 0x00000004;
public static DI_FLAGS DI_SHOWALL => 0x00000007;
public static DI_FLAGS DI_NOVCP => 0x00000008;
public static DI_FLAGS DI_DIDCOMPAT => 0x00000010;
public static DI_FLAGS DI_DIDCLASS => 0x00000020;
public static DI_FLAGS DI_AUTOASSIGNRES => 0x00000040;
public static DI_FLAGS DI_NEEDRESTART => 0x00000080;
public static DI_FLAGS DI_NEEDREBOOT => 0x00000100;
public static DI_FLAGS DI_NOBROWSE => 0x00000200;
public static DI_FLAGS DI_MULTMFGS => 0x00000400;
public static DI_FLAGS DI_DISABLED => 0x00000800;
public static DI_FLAGS DI_GENERALPAGE_ADDED => 0x00001000;
public static DI_FLAGS DI_RESOURCEPAGE_ADDED => 0x00002000;
public static DI_FLAGS DI_PROPERTIES_CHANGE => 0x00004000;
public static DI_FLAGS DI_INF_IS_SORTED => 0x00008000;
public static DI_FLAGS DI_ENUMSINGLEINF => 0x00010000;
public static DI_FLAGS DI_DONOTCALLCONFIGMG => 0x00020000;
public static DI_FLAGS DI_INSTALLDISABLED => 0x00040000;
public static DI_FLAGS DI_COMPAT_FROM_CLASS => 0x00080000;
public static DI_FLAGS DI_CLASSINSTALLPARAMS => 0x00100000;
public static DI_FLAGS DI_NODI_DEFAULTACTION => 0x00200000;
public static DI_FLAGS DI_QUIETINSTALL => 0x00800000;
public static DI_FLAGS DI_NOFILECOPY => 0x01000000;
public static DI_FLAGS DI_FORCECOPY => 0x02000000;
public static DI_FLAGS DI_DRIVERPAGE_ADDED => 0x04000000;
public static DI_FLAGS DI_USECI_SELECTSTRINGS => 0x08000000;
public static DI_FLAGS DI_OVERRIDE_INFFLAGS => 0x10000000;
public static DI_FLAGS DI_PROPS_NOCHANGEUSAGE => 0x20000000;
public static DI_FLAGS DI_NOSELECTICONS => 0x40000000;
public static DI_FLAGS DI_NOWRITE_IDS => 0x80000000;

partial struct DI_FLAGSEX /*num:uint32*/;

public static DI_FLAGSEX DI_FLAGSEX_CI_FAILED => 0x00000004;
public static DI_FLAGSEX DI_FLAGSEX_FINISHINSTALL_ACTION => 0x00000008;
public static DI_FLAGSEX DI_FLAGSEX_DIDINFOLIST => 0x00000010;
public static DI_FLAGSEX DI_FLAGSEX_DIDCOMPATINFO => 0x00000020;
public static DI_FLAGSEX DI_FLAGSEX_FILTERCLASSES => 0x00000040;
public static DI_FLAGSEX DI_FLAGSEX_SETFAILEDINSTALL => 0x00000080;
public static DI_FLAGSEX DI_FLAGSEX_DEVICECHANGE => 0x00000100;
public static DI_FLAGSEX DI_FLAGSEX_ALWAYSWRITEIDS => 0x00000200;
public static DI_FLAGSEX DI_FLAGSEX_PROPCHANGE_PENDING => 0x00000400;
public static DI_FLAGSEX DI_FLAGSEX_ALLOWEXCLUDEDDRVS => 0x00000800;
public static DI_FLAGSEX DI_FLAGSEX_NOUIONQUERYREMOVE => 0x00001000;
public static DI_FLAGSEX DI_FLAGSEX_USECLASSFORCOMPAT => 0x00002000;
public static DI_FLAGSEX DI_FLAGSEX_NO_DRVREG_MODIFY => 0x00008000;
public static DI_FLAGSEX DI_FLAGSEX_IN_SYSTEM_SETUP => 0x00010000;
public static DI_FLAGSEX DI_FLAGSEX_INET_DRIVER => 0x00020000;
public static DI_FLAGSEX DI_FLAGSEX_APPENDDRIVERLIST => 0x00040000;
public static DI_FLAGSEX DI_FLAGSEX_PREINSTALLBACKUP => 0x00080000;
public static DI_FLAGSEX DI_FLAGSEX_BACKUPONREPLACE => 0x00100000;
public static DI_FLAGSEX DI_FLAGSEX_DRIVERLIST_FROM_URL => 0x00200000;
public static DI_FLAGSEX DI_FLAGSEX_EXCLUDE_OLD_INET_DRIVERS => 0x00800000;
public static DI_FLAGSEX DI_FLAGSEX_POWERPAGE_ADDED => 0x01000000;
public static DI_FLAGSEX DI_FLAGSEX_FILTERSIMILARDRIVERS => 0x02000000;
public static DI_FLAGSEX DI_FLAGSEX_INSTALLEDDRIVER => 0x04000000;
public static DI_FLAGSEX DI_FLAGSEX_NO_CLASSLIST_NODE_MERGE => 0x08000000;
public static DI_FLAGSEX DI_FLAGSEX_ALTPLATFORM_DRVSEARCH => 0x10000000;
public static DI_FLAGSEX DI_FLAGSEX_RESTART_DEVICE_ONLY => 0x20000000;
public static DI_FLAGSEX DI_FLAGSEX_RECURSIVESEARCH => 0x40000000;
public static DI_FLAGSEX DI_FLAGSEX_SEARCH_PUBLISHED_INFS => 0x80000000;

partial struct ClassInstallHeader {
    internal uint32 size;
    public DI_FUNCTION InstallFunction;
}

public static ж<ClassInstallHeader> MakeClassInstallHeader(DI_FUNCTION installFunction) {
    var hdr = Ꮡ(new ClassInstallHeader(InstallFunction: installFunction));
    hdr.size = (uint32)/* unsafe.Sizeof(*hdr) */ (uintptr)8;
    return hdr;
}

partial struct DICS_STATE /*num:uint32*/;

public static DICS_STATE DICS_ENABLE => 0x00000001;
public static DICS_STATE DICS_DISABLE => 0x00000002;
public static DICS_STATE DICS_PROPCHANGE => 0x00000003;
public static DICS_STATE DICS_START => 0x00000004;
public static DICS_STATE DICS_STOP => 0x00000005;

partial struct DICS_FLAG /*num:uint32*/;

public static DICS_FLAG DICS_FLAG_GLOBAL => 0x00000001;
public static DICS_FLAG DICS_FLAG_CONFIGSPECIFIC => 0x00000002;
public static DICS_FLAG DICS_FLAG_CONFIGGENERAL => 0x00000004;

partial struct PropChangeParams {
    public ClassInstallHeader ClassInstallHeader;
    public DICS_STATE StateChange;
    public DICS_FLAG Scope;
    public uint32 HwProfile;
}

partial struct DI_REMOVEDEVICE /*num:uint32*/;

public static DI_REMOVEDEVICE DI_REMOVEDEVICE_GLOBAL => 0x00000001;
public static DI_REMOVEDEVICE DI_REMOVEDEVICE_CONFIGSPECIFIC => 0x00000002;

partial struct RemoveDeviceParams {
    public ClassInstallHeader ClassInstallHeader;
    public DI_REMOVEDEVICE Scope;
    public uint32 HwProfile;
}

partial struct DrvInfoData {
    internal uint32 size;
    public uint32 DriverType;
    internal uintptr _;
    internal array<uint16> description = new(LINE_LEN);
    internal array<uint16> mfgName = new(LINE_LEN);
    internal array<uint16> providerName = new(LINE_LEN);
    public Filetime DriverDate;
    public uint64 DriverVersion;
}

} partial class windowsᴛDrvInfoData {

public static @string Description(this ref DrvInfoData data) {
    return UTF16ToString(data.description[..]);
}

public static error SetDescription(this ref DrvInfoData data, @string description) {
    var (str, err) = UTF16FromString(description);
    if (err != nil) {
        return err;
    }
    copy(data.description[..], str);
    return default;
}

public static @string MfgName(this ref DrvInfoData data) {
    return UTF16ToString(data.mfgName[..]);
}

public static error SetMfgName(this ref DrvInfoData data, @string mfgName) {
    var (str, err) = UTF16FromString(mfgName);
    if (err != nil) {
        return err;
    }
    copy(data.mfgName[..], str);
    return default;
}

public static @string ProviderName(this ref DrvInfoData data) {
    return UTF16ToString(data.providerName[..]);
}

public static error SetProviderName(this ref DrvInfoData data, @string providerName) {
    var (str, err) = UTF16FromString(providerName);
    if (err != nil) {
        return err;
    }
    copy(data.providerName[..], str);
    return default;
}

public static bool IsNewer(this ref DrvInfoData data, Filetime driverDate, uint64 driverVersion) {
    if (data.DriverDate.HighDateTime > driverDate.HighDateTime) {
        return true;
    }
    if (data.DriverDate.HighDateTime < driverDate.HighDateTime) {
        return false;
    }

    if (data.DriverDate.LowDateTime > driverDate.LowDateTime) {
        return true;
    }
    if (data.DriverDate.LowDateTime < driverDate.LowDateTime) {
        return false;
    }

    if (data.DriverVersion > driverVersion) {
        return true;
    }
    if (data.DriverVersion < driverVersion) {
        return false;
    }

    return false;
}

} partial class windows_package {

partial struct DrvInfoDetailData {
    internal uint32 size;
    public Filetime InfDate;
    internal uint32 compatIDsOffset;
    internal uint32 compatIDsLength;
    internal uintptr _;
    internal array<uint16> sectionName = new(LINE_LEN);
    internal array<uint16> infFileName = new(MAX_PATH);
    internal array<uint16> drvDescription = new(LINE_LEN);
    internal array<uint16> hardwareID = new(1);
}

} partial class windowsᴛDrvInfoDetailData {

internal static uint32 unsafeSizeOf(this ref DrvInfoDetailData _) {
    if (/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8 == 4) {
        return (uint32)(/* unsafe.Offsetof(DrvInfoDetailData{}.hardwareID) */ (uintptr)1576 + /* unsafe.Sizeof(DrvInfoDetailData{}.hardwareID) */ (uintptr)2);
    }
    return (uint32)/* unsafe.Sizeof(DrvInfoDetailData{}) */ (uintptr)1584;
}

public static @string SectionName(this ref DrvInfoDetailData data) {
    return UTF16ToString(data.sectionName[..]);
}

public static @string InfFileName(this ref DrvInfoDetailData data) {
    return UTF16ToString(data.infFileName[..]);
}

public static @string DrvDescription(this ref DrvInfoDetailData data) {
    return UTF16ToString(data.drvDescription[..]);
}

public static @string HardwareID(this ref DrvInfoDetailData data) {
    if (data.compatIDsOffset > 1) {
        var bufW = data.getBuf();
        return UTF16ToString(bufW.slice(0, wcslen(bufW)));
    }

    return "";
}

public static slice<@string> CompatIDs(this ref DrvInfoDetailData data) {
    var a = new slice<@string>(0);

    if (data.compatIDsLength > 0) {
        var bufW = data.getBuf();
        bufW = bufW.slice((nint)(data.compatIDsOffset), (nint)(data.compatIDsOffset + data.compatIDsLength));
        for (nint i = 0; i < len(bufW); ) {
            nint j = i + wcslen(bufW.slice(i));
            if (i < j) {
                a = append(a, UTF16ToString(bufW.slice(i, j)));
            }
            i = j + 1;
        }
    }

    return a;
}

} partial class windows_package {

internal partial struct getBuf_sl /*dyn*/ {
    internal ж<uint16> addr;
    internal nint len;
    internal nint cap;
}

} partial class windowsᴛDrvInfoDetailData {

internal static slice<uint16> getBuf(this ref DrvInfoDetailData data) {
    var len = (data.size - (uint32)/* unsafe.Offsetof(data.hardwareID) */ (uintptr)1576) / 2;
    ref var sl = ref heap<getBuf_sl>(new getBuf_sl(Ꮡ(data.hardwareID, 0), (nint)len, (nint)len), out var Ꮡsl);
    return ~Ꮡsl.Reinterpret<getBuf_sl, slice<uint16>>();
}

public static bool IsCompatible(this ref DrvInfoDetailData data, @string hwid) {
    @string hwidLC = strings.ToLower(hwid);
    if (strings.ToLower(data.HardwareID()) == hwidLC) {
        return true;
    }
    var a = data.CompatIDs();
    foreach (var i in range(len(a))) {
        if (strings.ToLower(a[i]) == hwidLC) {
            return true;
        }
    }

    return false;
}

} partial class windows_package {

partial struct DICD /*num:uint32*/;

public static DICD DICD_GENERATE_ID => 0x00000001;
public static DICD DICD_INHERIT_CLASSDRVS => 0x00000002;

partial struct SUOI /*num:uint32*/;

public static SUOI SUOI_FORCEDELETE => 0x0001;

partial struct SPDIT /*num:uint32*/;

public static SPDIT SPDIT_NODRIVER => 0x00000000;
public static SPDIT SPDIT_CLASSDRIVER => 0x00000001;
public static SPDIT SPDIT_COMPATDRIVER => 0x00000002;

partial struct DIGCF /*num:uint32*/;

public static DIGCF DIGCF_DEFAULT => 0x00000001;
public static DIGCF DIGCF_PRESENT => 0x00000002;
public static DIGCF DIGCF_ALLCLASSES => 0x00000004;
public static DIGCF DIGCF_PROFILE => 0x00000008;
public static DIGCF DIGCF_DEVICEINTERFACE => 0x00000010;

partial struct DIREG /*num:uint32*/;

public static DIREG DIREG_DEV => 0x00000001;
public static DIREG DIREG_DRV => 0x00000002;
public static DIREG DIREG_BOTH => 0x00000004;

partial struct SPDRP /*num:uint32*/;

public static SPDRP SPDRP_DEVICEDESC => 0x00000000;
public static SPDRP SPDRP_HARDWAREID => 0x00000001;
public static SPDRP SPDRP_COMPATIBLEIDS => 0x00000002;
public static SPDRP SPDRP_SERVICE => 0x00000004;
public static SPDRP SPDRP_CLASS => 0x00000007;
public static SPDRP SPDRP_CLASSGUID => 0x00000008;
public static SPDRP SPDRP_DRIVER => 0x00000009;
public static SPDRP SPDRP_CONFIGFLAGS => 0x0000000A;
public static SPDRP SPDRP_MFG => 0x0000000B;
public static SPDRP SPDRP_FRIENDLYNAME => 0x0000000C;
public static SPDRP SPDRP_LOCATION_INFORMATION => 0x0000000D;
public static SPDRP SPDRP_PHYSICAL_DEVICE_OBJECT_NAME => 0x0000000E;
public static SPDRP SPDRP_CAPABILITIES => 0x0000000F;
public static SPDRP SPDRP_UI_NUMBER => 0x00000010;
public static SPDRP SPDRP_UPPERFILTERS => 0x00000011;
public static SPDRP SPDRP_LOWERFILTERS => 0x00000012;
public static SPDRP SPDRP_BUSTYPEGUID => 0x00000013;
public static SPDRP SPDRP_LEGACYBUSTYPE => 0x00000014;
public static SPDRP SPDRP_BUSNUMBER => 0x00000015;
public static SPDRP SPDRP_ENUMERATOR_NAME => 0x00000016;
public static SPDRP SPDRP_SECURITY => 0x00000017;
public static SPDRP SPDRP_SECURITY_SDS => 0x00000018;
public static SPDRP SPDRP_DEVTYPE => 0x00000019;
public static SPDRP SPDRP_EXCLUSIVE => 0x0000001A;
public static SPDRP SPDRP_CHARACTERISTICS => 0x0000001B;
public static SPDRP SPDRP_ADDRESS => 0x0000001C;
public static SPDRP SPDRP_UI_NUMBER_DESC_FORMAT => 0x0000001D;
public static SPDRP SPDRP_DEVICE_POWER_DATA => 0x0000001E;
public static SPDRP SPDRP_REMOVAL_POLICY => 0x0000001F;
public static SPDRP SPDRP_REMOVAL_POLICY_HW_DEFAULT => 0x00000020;
public static SPDRP SPDRP_REMOVAL_POLICY_OVERRIDE => 0x00000021;
public static SPDRP SPDRP_INSTALL_STATE => 0x00000022;
public static SPDRP SPDRP_LOCATION_PATHS => 0x00000023;
public static SPDRP SPDRP_BASE_CONTAINERID => 0x00000024;
public static SPDRP SPDRP_MAXIMUM_PROPERTY => 0x00000025;

partial struct DEVPROPTYPE /*num:uint32*/;

public static DEVPROPTYPE DEVPROP_TYPEMOD_ARRAY => 0x00001000;
public static DEVPROPTYPE DEVPROP_TYPEMOD_LIST => 0x00002000;
public static DEVPROPTYPE DEVPROP_TYPE_EMPTY => 0x00000000;
public static DEVPROPTYPE DEVPROP_TYPE_NULL => 0x00000001;
public static DEVPROPTYPE DEVPROP_TYPE_SBYTE => 0x00000002;
public static DEVPROPTYPE DEVPROP_TYPE_BYTE => 0x00000003;
public static DEVPROPTYPE DEVPROP_TYPE_INT16 => 0x00000004;
public static DEVPROPTYPE DEVPROP_TYPE_UINT16 => 0x00000005;
public static DEVPROPTYPE DEVPROP_TYPE_INT32 => 0x00000006;
public static DEVPROPTYPE DEVPROP_TYPE_UINT32 => 0x00000007;
public static DEVPROPTYPE DEVPROP_TYPE_INT64 => 0x00000008;
public static DEVPROPTYPE DEVPROP_TYPE_UINT64 => 0x00000009;
public static DEVPROPTYPE DEVPROP_TYPE_FLOAT => 0x0000000A;
public static DEVPROPTYPE DEVPROP_TYPE_DOUBLE => 0x0000000B;
public static DEVPROPTYPE DEVPROP_TYPE_DECIMAL => 0x0000000C;
public static DEVPROPTYPE DEVPROP_TYPE_GUID => 0x0000000D;
public static DEVPROPTYPE DEVPROP_TYPE_CURRENCY => 0x0000000E;
public static DEVPROPTYPE DEVPROP_TYPE_DATE => 0x0000000F;
public static DEVPROPTYPE DEVPROP_TYPE_FILETIME => 0x00000010;
public static DEVPROPTYPE DEVPROP_TYPE_BOOLEAN => 0x00000011;
public static DEVPROPTYPE DEVPROP_TYPE_STRING => 0x00000012;
public static DEVPROPTYPE DEVPROP_TYPE_STRING_LIST => /* DEVPROP_TYPE_STRING | DEVPROP_TYPEMOD_LIST */ 8210;
public static DEVPROPTYPE DEVPROP_TYPE_SECURITY_DESCRIPTOR => 0x00000013;
public static DEVPROPTYPE DEVPROP_TYPE_SECURITY_DESCRIPTOR_STRING => 0x00000014;
public static DEVPROPTYPE DEVPROP_TYPE_DEVPROPKEY => 0x00000015;
public static DEVPROPTYPE DEVPROP_TYPE_DEVPROPTYPE => 0x00000016;
public static DEVPROPTYPE DEVPROP_TYPE_BINARY => /* DEVPROP_TYPE_BYTE | DEVPROP_TYPEMOD_ARRAY */ 4099;
public static DEVPROPTYPE DEVPROP_TYPE_ERROR => 0x00000017;
public static DEVPROPTYPE DEVPROP_TYPE_NTSTATUS => 0x00000018;
public static DEVPROPTYPE DEVPROP_TYPE_STRING_INDIRECT => 0x00000019;
public static DEVPROPTYPE MAX_DEVPROP_TYPE => 0x00000019;
public static DEVPROPTYPE MAX_DEVPROP_TYPEMOD => 0x00002000;
public static DEVPROPTYPE DEVPROP_MASK_TYPE => 0x00000FFF;
public static DEVPROPTYPE DEVPROP_MASK_TYPEMOD => 0x0000F000;

partial struct DEVPROPGUID /*GUID*/;

partial struct DEVPROPID /*num:uint32*/;

public static DEVPROPID DEVPROPID_FIRST_USABLE => 2;

[global::go.golib.GoInlineLayout(Size = 20, Align = 4), StructLayout(LayoutKind.Explicit, Size = 20)] partial struct DEVPROPKEY {
    [FieldOffset(0)] public DEVPROPGUID FmtID;
    [FieldOffset(16)] public DEVPROPID PID;
}

partial struct CONFIGRET /*num:uint32*/;

} partial class windowsᴛCONFIGRET {

public static @string Error(this CONFIGRET ret) {
    var (win32Error, ok) = ret.Unwrap()._<Errno>(ᐧ); if (ok) {
        return fmt.Sprintf("%s (CfgMgr error: 0x%08x)"u8, win32Error.Error(), (uint32)ret);
    }
    return fmt.Sprintf("CfgMgr error: 0x%08x"u8, (uint32)ret);
}

public static Errno Win32Error(this CONFIGRET ret, Errno defaultError) {
    return cm_MapCrToWin32Err(ret, defaultError);
}

public static error Unwrap(this CONFIGRET ret) {
    Errno noMatch = /* Errno(^uintptr(0)) */ unchecked((Errno)18446744073709551615);
    var win32Error = ret.Win32Error(noMatch);
    if (win32Error == noMatch) {
        return default;
    }
    return win32Error;
}

} partial class windows_package {

public static CONFIGRET CR_SUCCESS => 0x00000000;
public static CONFIGRET CR_DEFAULT => 0x00000001;
public static CONFIGRET CR_OUT_OF_MEMORY => 0x00000002;
public static CONFIGRET CR_INVALID_POINTER => 0x00000003;
public static CONFIGRET CR_INVALID_FLAG => 0x00000004;
public static CONFIGRET CR_INVALID_DEVNODE => 0x00000005;
public static CONFIGRET CR_INVALID_DEVINST => /* CR_INVALID_DEVNODE */ 5;
public static CONFIGRET CR_INVALID_RES_DES => 0x00000006;
public static CONFIGRET CR_INVALID_LOG_CONF => 0x00000007;
public static CONFIGRET CR_INVALID_ARBITRATOR => 0x00000008;
public static CONFIGRET CR_INVALID_NODELIST => 0x00000009;
public static CONFIGRET CR_DEVNODE_HAS_REQS => 0x0000000A;
public static CONFIGRET CR_DEVINST_HAS_REQS => /* CR_DEVNODE_HAS_REQS */ 10;
public static CONFIGRET CR_INVALID_RESOURCEID => 0x0000000B;
public static CONFIGRET CR_DLVXD_NOT_FOUND => 0x0000000C;
public static CONFIGRET CR_NO_SUCH_DEVNODE => 0x0000000D;
public static CONFIGRET CR_NO_SUCH_DEVINST => /* CR_NO_SUCH_DEVNODE */ 13;
public static CONFIGRET CR_NO_MORE_LOG_CONF => 0x0000000E;
public static CONFIGRET CR_NO_MORE_RES_DES => 0x0000000F;
public static CONFIGRET CR_ALREADY_SUCH_DEVNODE => 0x00000010;
public static CONFIGRET CR_ALREADY_SUCH_DEVINST => /* CR_ALREADY_SUCH_DEVNODE */ 16;
public static CONFIGRET CR_INVALID_RANGE_LIST => 0x00000011;
public static CONFIGRET CR_INVALID_RANGE => 0x00000012;
public static CONFIGRET CR_FAILURE => 0x00000013;
public static CONFIGRET CR_NO_SUCH_LOGICAL_DEV => 0x00000014;
public static CONFIGRET CR_CREATE_BLOCKED => 0x00000015;
public static CONFIGRET CR_NOT_SYSTEM_VM => 0x00000016;
public static CONFIGRET CR_REMOVE_VETOED => 0x00000017;
public static CONFIGRET CR_APM_VETOED => 0x00000018;
public static CONFIGRET CR_INVALID_LOAD_TYPE => 0x00000019;
public static CONFIGRET CR_BUFFER_SMALL => 0x0000001A;
public static CONFIGRET CR_NO_ARBITRATOR => 0x0000001B;
public static CONFIGRET CR_NO_REGISTRY_HANDLE => 0x0000001C;
public static CONFIGRET CR_REGISTRY_ERROR => 0x0000001D;
public static CONFIGRET CR_INVALID_DEVICE_ID => 0x0000001E;
public static CONFIGRET CR_INVALID_DATA => 0x0000001F;
public static CONFIGRET CR_INVALID_API => 0x00000020;
public static CONFIGRET CR_DEVLOADER_NOT_READY => 0x00000021;
public static CONFIGRET CR_NEED_RESTART => 0x00000022;
public static CONFIGRET CR_NO_MORE_HW_PROFILES => 0x00000023;
public static CONFIGRET CR_DEVICE_NOT_THERE => 0x00000024;
public static CONFIGRET CR_NO_SUCH_VALUE => 0x00000025;
public static CONFIGRET CR_WRONG_TYPE => 0x00000026;
public static CONFIGRET CR_INVALID_PRIORITY => 0x00000027;
public static CONFIGRET CR_NOT_DISABLEABLE => 0x00000028;
public static CONFIGRET CR_FREE_RESOURCES => 0x00000029;
public static CONFIGRET CR_QUERY_VETOED => 0x0000002A;
public static CONFIGRET CR_CANT_SHARE_IRQ => 0x0000002B;
public static CONFIGRET CR_NO_DEPENDENT => 0x0000002C;
public static CONFIGRET CR_SAME_RESOURCES => 0x0000002D;
public static CONFIGRET CR_NO_SUCH_REGISTRY_KEY => 0x0000002E;
public static CONFIGRET CR_INVALID_MACHINENAME => 0x0000002F;
public static CONFIGRET CR_REMOTE_COMM_FAILURE => 0x00000030;
public static CONFIGRET CR_MACHINE_UNAVAILABLE => 0x00000031;
public static CONFIGRET CR_NO_CM_SERVICES => 0x00000032;
public static CONFIGRET CR_ACCESS_DENIED => 0x00000033;
public static CONFIGRET CR_CALL_NOT_IMPLEMENTED => 0x00000034;
public static CONFIGRET CR_INVALID_PROPERTY => 0x00000035;
public static CONFIGRET CR_DEVICE_INTERFACE_ACTIVE => 0x00000036;
public static CONFIGRET CR_NO_SUCH_DEVICE_INTERFACE => 0x00000037;
public static CONFIGRET CR_INVALID_REFERENCE_STRING => 0x00000038;
public static CONFIGRET CR_INVALID_CONFLICT_LIST => 0x00000039;
public static CONFIGRET CR_INVALID_INDEX => 0x0000003A;
public static CONFIGRET CR_INVALID_STRUCTURE_SIZE => 0x0000003B;
public static CONFIGRET NUM_CR_RESULTS => 0x0000003C;

public static UntypedInt CM_GET_DEVICE_INTERFACE_LIST_PRESENT => 0;
public static UntypedInt CM_GET_DEVICE_INTERFACE_LIST_ALL_DEVICES => 1;

public static UntypedInt DN_ROOT_ENUMERATED => 0x00000001;
public static UntypedInt DN_DRIVER_LOADED => 0x00000002;
public static UntypedInt DN_ENUM_LOADED => 0x00000004;
public static UntypedInt DN_STARTED => 0x00000008;
public static UntypedInt DN_MANUAL => 0x00000010;
public static UntypedInt DN_NEED_TO_ENUM => 0x00000020;
public static UntypedInt DN_NOT_FIRST_TIME => 0x00000040;
public static UntypedInt DN_HARDWARE_ENUM => 0x00000080;
public static UntypedInt DN_LIAR => 0x00000100;
public static UntypedInt DN_HAS_MARK => 0x00000200;
public static UntypedInt DN_HAS_PROBLEM => 0x00000400;
public static UntypedInt DN_FILTERED => 0x00000800;
public static UntypedInt DN_MOVED => 0x00001000;
public static UntypedInt DN_DISABLEABLE => 0x00002000;
public static UntypedInt DN_REMOVABLE => 0x00004000;
public static UntypedInt DN_PRIVATE_PROBLEM => 0x00008000;
public static UntypedInt DN_MF_PARENT => 0x00010000;
public static UntypedInt DN_MF_CHILD => 0x00020000;
public static UntypedInt DN_WILL_BE_REMOVED => 0x00040000;
public static UntypedInt DN_NOT_FIRST_TIMEE => 0x00080000;
public static UntypedInt DN_STOP_FREE_RES => 0x00100000;
public static UntypedInt DN_REBAL_CANDIDATE => 0x00200000;
public static UntypedInt DN_BAD_PARTIAL => 0x00400000;
public static UntypedInt DN_NT_ENUMERATOR => 0x00800000;
public static UntypedInt DN_NT_DRIVER => 0x01000000;
public static UntypedInt DN_NEEDS_LOCKING => 0x02000000;
public static UntypedInt DN_ARM_WAKEUP => 0x04000000;
public static UntypedInt DN_APM_ENUMERATOR => 0x08000000;
public static UntypedInt DN_APM_DRIVER => 0x10000000;
public static UntypedInt DN_SILENT_INSTALL => 0x20000000;
public static UntypedInt DN_NO_SHOW_IN_DM => 0x40000000;
public static UntypedInt DN_BOOT_LOG_PROB => 0x80000000;
public static UntypedInt DN_NEED_RESTART => /* DN_LIAR */ 256;
public static UntypedInt DN_DRIVER_BLOCKED => /* DN_NOT_FIRST_TIME */ 64;
public static UntypedInt DN_LEGACY_DRIVER => /* DN_MOVED */ 4096;
public static UntypedInt DN_CHILD_WITH_INVALID_ID => /* DN_HAS_MARK */ 512;
public static UntypedInt DN_DEVICE_DISCONNECTED => /* DN_NEEDS_LOCKING */ 33554432;
public static UntypedInt DN_QUERY_REMOVE_PENDING => /* DN_MF_PARENT */ 65536;
public static UntypedInt DN_QUERY_REMOVE_ACTIVE => /* DN_MF_CHILD */ 131072;
public static UntypedInt DN_CHANGEABLE_FLAGS => /* DN_NOT_FIRST_TIME | DN_HARDWARE_ENUM | DN_HAS_MARK | DN_DISABLEABLE | DN_REMOVABLE | DN_MF_CHILD | DN_MF_PARENT | DN_NOT_FIRST_TIMEE | DN_STOP_FREE_RES | DN_REBAL_CANDIDATE | DN_NT_ENUMERATOR | DN_NT_DRIVER | DN_SILENT_INSTALL | DN_NO_SHOW_IN_DM */ 1639670464;

public static (DevInfo deviceInfoSet, error err) SetupDiCreateDeviceInfoListEx(ж<GUID> classGUID, uintptr hwndParent, @string machineName) {
    DevInfo deviceInfoSet = default;
    error err = default;

    ж<uint16> machineNameUTF16 = default;
    if (machineName != ""u8) {
        (machineNameUTF16, err) = UTF16PtrFromString(machineName);
        if (err != nil) {
            return (deviceInfoSet, err);
        }
    }
    return setupDiCreateDeviceInfoListEx(classGUID, hwndParent, machineNameUTF16, 0);
}

public static (ж<DevInfoListDetailData> deviceInfoSetDetailData, error err) SetupDiGetDeviceInfoListDetail(DevInfo deviceInfoSet) {
    var data = Ꮡ(new DevInfoListDetailData());
    data.size = data.unsafeSizeOf();

    return (data, setupDiGetDeviceInfoListDetail(deviceInfoSet, data));
}

} partial class windowsᴛDevInfo {

public static (ж<DevInfoListDetailData>, error) DeviceInfoListDetail(this DevInfo deviceInfoSet) {
    return SetupDiGetDeviceInfoListDetail(deviceInfoSet);
}

} partial class windows_package {

public static (ж<DevInfoData> deviceInfoData, error err) SetupDiCreateDeviceInfo(DevInfo deviceInfoSet, @string deviceName, ж<GUID> classGUID, @string deviceDescription, uintptr hwndParent, DICD creationFlags) {
    ж<DevInfoData> deviceInfoData = default;
    error err = default;

    (var deviceNameUTF16, err) = UTF16PtrFromString(deviceName);
    if (err != nil) {
        return (deviceInfoData, err);
    }

    ж<uint16> deviceDescriptionUTF16 = default;
    if (deviceDescription != ""u8) {
        (deviceDescriptionUTF16, err) = UTF16PtrFromString(deviceDescription);
        if (err != nil) {
            return (deviceInfoData, err);
        }
    }

    var data = Ꮡ(new DevInfoData());
    data.size = (uint32)/* unsafe.Sizeof(*data) */ (uintptr)32;

    return (data, setupDiCreateDeviceInfo(deviceInfoSet, deviceNameUTF16, classGUID, deviceDescriptionUTF16, hwndParent, creationFlags, data));
}

} partial class windowsᴛDevInfo {

public static (ж<DevInfoData>, error) CreateDeviceInfo(this DevInfo deviceInfoSet, @string deviceName, ж<GUID> classGUID, @string deviceDescription, uintptr hwndParent, DICD creationFlags) {
    return SetupDiCreateDeviceInfo(deviceInfoSet, deviceName, classGUID, deviceDescription, hwndParent, creationFlags);
}

} partial class windows_package {

public static (ж<DevInfoData>, error) SetupDiEnumDeviceInfo(DevInfo deviceInfoSet, nint memberIndex) {
    var data = Ꮡ(new DevInfoData());
    data.size = (uint32)/* unsafe.Sizeof(*data) */ (uintptr)32;

    return (data, setupDiEnumDeviceInfo(deviceInfoSet, (uint32)memberIndex, data));
}

} partial class windowsᴛDevInfo {

public static (ж<DevInfoData>, error) EnumDeviceInfo(this DevInfo deviceInfoSet, nint memberIndex) {
    return SetupDiEnumDeviceInfo(deviceInfoSet, memberIndex);
}

public static error Close(this DevInfo deviceInfoSet) {
    return SetupDiDestroyDeviceInfoList(deviceInfoSet);
}

public static error BuildDriverInfoList(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDIT driverType) {
    return SetupDiBuildDriverInfoList(deviceInfoSet, deviceInfoData, driverType);
}

public static error CancelDriverInfoSearch(this DevInfo deviceInfoSet) {
    return SetupDiCancelDriverInfoSearch(deviceInfoSet);
}

} partial class windows_package {

public static (ж<DrvInfoData>, error) SetupDiEnumDriverInfo(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDIT driverType, nint memberIndex) {
    var data = Ꮡ(new DrvInfoData());
    data.size = (uint32)/* unsafe.Sizeof(*data) */ (uintptr)1568;

    return (data, setupDiEnumDriverInfo(deviceInfoSet, deviceInfoData, driverType, (uint32)memberIndex, data));
}

} partial class windowsᴛDevInfo {

public static (ж<DrvInfoData>, error) EnumDriverInfo(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDIT driverType, nint memberIndex) {
    return SetupDiEnumDriverInfo(deviceInfoSet, deviceInfoData, driverType, memberIndex);
}

} partial class windows_package {

public static (ж<DrvInfoData>, error) SetupDiGetSelectedDriver(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    var data = Ꮡ(new DrvInfoData());
    data.size = (uint32)/* unsafe.Sizeof(*data) */ (uintptr)1568;

    return (data, setupDiGetSelectedDriver(deviceInfoSet, deviceInfoData, data));
}

} partial class windowsᴛDevInfo {

public static (ж<DrvInfoData>, error) SelectedDriver(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    return SetupDiGetSelectedDriver(deviceInfoSet, deviceInfoData);
}

public static error SetSelectedDriver(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DrvInfoData> driverInfoData) {
    return SetupDiSetSelectedDriver(deviceInfoSet, deviceInfoData, driverInfoData);
}

} partial class windows_package {

public static (ж<DrvInfoDetailData>, error) SetupDiGetDriverInfoDetail(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DrvInfoData> driverInfoData) {
    ref var reqSize = ref heap<uint32>((uint32)2048, out var ᏑreqSize);
    for (;;) {
        var buf = new slice<byte>((nint)(reqSize));
        var data = Ꮡ(buf, 0).Reinterpret<byte, DrvInfoDetailData>();
        data.size = data.unsafeSizeOf();
        var err = setupDiGetDriverInfoDetail(deviceInfoSet, deviceInfoData, driverInfoData, data, (uint32)len(buf), ᏑreqSize);
        if (AreEqual(err, ERROR_INSUFFICIENT_BUFFER)) {
            continue;
        }
        if (err != nil) {
            return (default, err);
        }
        data.size = reqSize;
        return (data, default);
    }
}

} partial class windowsᴛDevInfo {

public static (ж<DrvInfoDetailData>, error) DriverInfoDetail(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DrvInfoData> driverInfoData) {
    return SetupDiGetDriverInfoDetail(deviceInfoSet, deviceInfoData, driverInfoData);
}

public static error DestroyDriverInfoList(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDIT driverType) {
    return SetupDiDestroyDriverInfoList(deviceInfoSet, deviceInfoData, driverType);
}

} partial class windows_package {

public static (DevInfo handle, error err) SetupDiGetClassDevsEx(ж<GUID> classGUID, @string enumerator, uintptr hwndParent, DIGCF flags, DevInfo deviceInfoSet, @string machineName) {
    DevInfo handle = default;
    error err = default;

    ж<uint16> enumeratorUTF16 = default;
    if (enumerator != ""u8) {
        (enumeratorUTF16, err) = UTF16PtrFromString(enumerator);
        if (err != nil) {
            return (handle, err);
        }
    }
    ж<uint16> machineNameUTF16 = default;
    if (machineName != ""u8) {
        (machineNameUTF16, err) = UTF16PtrFromString(machineName);
        if (err != nil) {
            return (handle, err);
        }
    }
    return setupDiGetClassDevsEx(classGUID, enumeratorUTF16, hwndParent, flags, deviceInfoSet, machineNameUTF16, 0);
}

} partial class windowsᴛDevInfo {

public static error CallClassInstaller(this DevInfo deviceInfoSet, DI_FUNCTION installFunction, ж<DevInfoData> deviceInfoData) {
    return SetupDiCallClassInstaller(installFunction, deviceInfoSet, deviceInfoData);
}

public static (Handle, error) OpenDevRegKey(this DevInfo deviceInfoSet, ж<DevInfoData> DeviceInfoData, DICS_FLAG Scope, uint32 HwProfile, DIREG KeyType, uint32 samDesired) {
    return SetupDiOpenDevRegKey(deviceInfoSet, DeviceInfoData, Scope, HwProfile, KeyType, samDesired);
}

} partial class windows_package {

public static (any value, error err) SetupDiGetDeviceProperty(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DEVPROPKEY> propertyKey) {
    any value = default;
    error err = default;

    ref var reqSize = ref heap<uint32>((uint32)256, out var ᏑreqSize);
    for (;;) {
        ref var dataType = ref heap(new DEVPROPTYPE(), out var ᏑdataType);
        var buf = new slice<byte>((nint)(reqSize));
        err = setupDiGetDeviceProperty(deviceInfoSet, deviceInfoData, propertyKey, ᏑdataType, Ꮡ(buf, 0), (uint32)len(buf), ᏑreqSize, 0);
        if (AreEqual(err, ERROR_INSUFFICIENT_BUFFER)) {
            continue;
        }
        if (err != nil) {
            return (value, err);
        }
        switch (dataType) {
        case var ᴛ when ᴛ == DEVPROP_TYPE_STRING: {
            @string ret = UTF16ToString(bufToUTF16(buf));
            runtime.KeepAlive(buf);
            return (ret, default);
        }}

        return (default, errors.New("unimplemented property type"));
    }
}

public static (any value, error err) SetupDiGetDeviceRegistryProperty(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDRP property) {
    any value = default;
    error err = default;

    ref var reqSize = ref heap<uint32>((uint32)256, out var ᏑreqSize);
    for (;;) {
        ref var dataType = ref heap(new uint32(), out var ᏑdataType);
        var buf = new slice<byte>((nint)(reqSize));
        err = setupDiGetDeviceRegistryProperty(deviceInfoSet, deviceInfoData, property, ᏑdataType, Ꮡ(buf, 0), (uint32)len(buf), ᏑreqSize);
        if (AreEqual(err, ERROR_INSUFFICIENT_BUFFER)) {
            continue;
        }
        if (err != nil) {
            return (value, err);
        }
        return getRegistryValue(buf.slice(0, (nint)(reqSize)), dataType);
    }
}

internal static (any, error) getRegistryValue(slice<byte> buf, uint32 dataType) {
    switch (dataType) {
    case var ᴛ when ᴛ == REG_SZ: {
        @string ret = UTF16ToString(bufToUTF16(buf));
        runtime.KeepAlive(buf);
        return (ret, default);
    }
    case var ᴛ when ᴛ == REG_EXPAND_SZ: {
        @string value = UTF16ToString(bufToUTF16(buf));
        if (value == ""u8) {
            return ((@string)""u8, default);
        }
        var (p, err) = syscall.UTF16PtrFromString(value);
        if (err != nil) {
            return ((@string)""u8, err);
        }
        var ret = new slice<uint16>(100);
        for (;;) {
            var (n, errΔ1) = ExpandEnvironmentStrings(p, Ꮡ(ret, 0), (uint32)len(ret));
            if (errΔ1 != nil) {
                return ((@string)""u8, errΔ1);
            }
            if (n <= (uint32)len(ret)) {
                return (UTF16ToString(ret.slice(0, (nint)(n))), default);
            }
            ret = new slice<uint16>((nint)(n));
        }
        break;
    }
    case var ᴛ when ᴛ == REG_BINARY: {
        return (buf, default);
    }
    case var ᴛ when ᴛ == REG_DWORD_LITTLE_ENDIAN: {
        return (binary.LittleEndian.Uint32(buf), default);
    }
    case var ᴛ when ᴛ == REG_DWORD_BIG_ENDIAN: {
        return (binary.BigEndian.Uint32(buf), default);
    }
    case var ᴛ when ᴛ == REG_MULTI_SZ: {
        var bufW = bufToUTF16(buf);
        var a = slice<@string>([]);
        for (nint i = 0; i < len(bufW); ) {
            nint j = i + wcslen(bufW.slice(i));
            if (i < j) {
                a = append(a, UTF16ToString(bufW.slice(i, j)));
            }
            i = j + 1;
        }
        runtime.KeepAlive(buf);
        return (a, default);
    }
    case var ᴛ when ᴛ == REG_QWORD_LITTLE_ENDIAN: {
        return (binary.LittleEndian.Uint64(buf), default);
    }
    default: {
        return (default, fmt.Errorf("Unsupported registry value type: %v"u8, dataType));
    }}

}

internal partial struct bufToUTF16_sl /*dyn*/ {
    internal ж<uint16> addr;
    internal nint len;
    internal nint cap;
}

internal static slice<uint16> bufToUTF16(slice<byte> buf) {
    ref var sl = ref heap<bufToUTF16_sl>(new bufToUTF16_sl(Ꮡ(buf, 0).Reinterpret<byte, uint16>(), len(buf) / 2, cap(buf) / 2), out var Ꮡsl);
    return ~Ꮡsl.Reinterpret<bufToUTF16_sl, slice<uint16>>();
}

internal partial struct utf16ToBuf_sl /*dyn*/ {
    internal ж<byte> addr;
    internal nint len;
    internal nint cap;
}

internal static slice<byte> utf16ToBuf(slice<uint16> buf) {
    ref var sl = ref heap<utf16ToBuf_sl>(new utf16ToBuf_sl(Ꮡ(buf, 0).Reinterpret<uint16, byte>(), len(buf) * 2, cap(buf) * 2), out var Ꮡsl);
    return ~Ꮡsl.Reinterpret<utf16ToBuf_sl, slice<byte>>();
}

internal static nint wcslen(slice<uint16> str) {
    for (nint i = 0; i < len(str); i++) {
        if (str[i] == 0) {
            return i;
        }
    }
    return len(str);
}

} partial class windowsᴛDevInfo {

public static (any, error) DeviceRegistryProperty(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDRP property) {
    return SetupDiGetDeviceRegistryProperty(deviceInfoSet, deviceInfoData, property);
}

} partial class windows_package {

public static error SetupDiSetDeviceRegistryProperty(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDRP property, slice<byte> propertyBuffers) {
    return setupDiSetDeviceRegistryProperty(deviceInfoSet, deviceInfoData, property, Ꮡ(propertyBuffers, 0), (uint32)len(propertyBuffers));
}

} partial class windowsᴛDevInfo {

public static error SetDeviceRegistryProperty(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDRP property, slice<byte> propertyBuffers) {
    return SetupDiSetDeviceRegistryProperty(deviceInfoSet, deviceInfoData, property, propertyBuffers);
}

public static error SetDeviceRegistryPropertyString(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, SPDRP property, @string str) {
    var str16ᴷ = new stackAppend<uint16>();

    var (str16, err) = UTF16FromString(str);
    if (err != nil) {
        return err;
    }
    err = SetupDiSetDeviceRegistryProperty(deviceInfoSet, deviceInfoData, property, utf16ToBuf(append(str16ᴷ, str16, (uint16)0)));
    runtime.KeepAlive(str16);
    return err;
}

} partial class windows_package {

public static (ж<DevInstallParams>, error) SetupDiGetDeviceInstallParams(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    var @params = Ꮡ(new DevInstallParams());
    @params.size = (uint32)/* unsafe.Sizeof(*params) */ (uintptr)584;

    return (@params, setupDiGetDeviceInstallParams(deviceInfoSet, deviceInfoData, @params));
}

} partial class windowsᴛDevInfo {

public static (ж<DevInstallParams>, error) DeviceInstallParams(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    return SetupDiGetDeviceInstallParams(deviceInfoSet, deviceInfoData);
}

} partial class windows_package {

public static (@string, error) SetupDiGetDeviceInstanceId(DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    ref var reqSize = ref heap<uint32>((uint32)1024, out var ᏑreqSize);
    for (;;) {
        var buf = new slice<uint16>((nint)(reqSize));
        var err = setupDiGetDeviceInstanceId(deviceInfoSet, deviceInfoData, Ꮡ(buf, 0), (uint32)len(buf), ᏑreqSize);
        if (AreEqual(err, ERROR_INSUFFICIENT_BUFFER)) {
            continue;
        }
        if (err != nil) {
            return ("", err);
        }
        return (UTF16ToString(buf), default);
    }
}

} partial class windowsᴛDevInfo {

public static (@string, error) DeviceInstanceID(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    return SetupDiGetDeviceInstanceId(deviceInfoSet, deviceInfoData);
}

public static error ClassInstallParams(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<ClassInstallHeader> classInstallParams, uint32 classInstallParamsSize, ж<uint32> requiredSize) {
    return SetupDiGetClassInstallParams(deviceInfoSet, deviceInfoData, classInstallParams, classInstallParamsSize, requiredSize);
}

public static error SetDeviceInstallParams(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<DevInstallParams> deviceInstallParams) {
    return SetupDiSetDeviceInstallParams(deviceInfoSet, deviceInfoData, deviceInstallParams);
}

public static error SetClassInstallParams(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData, ж<ClassInstallHeader> classInstallParams, uint32 classInstallParamsSize) {
    return SetupDiSetClassInstallParams(deviceInfoSet, deviceInfoData, classInstallParams, classInstallParamsSize);
}

} partial class windows_package {

public static (@string className, error err) SetupDiClassNameFromGuidEx(ж<GUID> classGUID, @string machineName) {
    @string className = default;
    error err = default;

    ref var classNameUTF16 = ref heap(new array<uint16>(32), out var ᏑclassNameUTF16);

    ж<uint16> machineNameUTF16 = default;
    if (machineName != ""u8) {
        (machineNameUTF16, err) = UTF16PtrFromString(machineName);
        if (err != nil) {
            return (className, err);
        }
    }

    err = setupDiClassNameFromGuidEx(classGUID, ᏑclassNameUTF16.at<uint16>(0), MAX_CLASS_NAME_LEN, nil, machineNameUTF16, 0);
    if (err != nil) {
        return (className, err);
    }

    className = UTF16ToString(classNameUTF16[..]);
    return (className, err);
}

public static (slice<GUID>, error) SetupDiClassGuidsFromNameEx(@string className, @string machineName) {
    var (classNameUTF16, err) = UTF16PtrFromString(className);
    if (err != nil) {
        return (default, err);
    }

    ж<uint16> machineNameUTF16 = default;
    if (machineName != ""u8) {
        (machineNameUTF16, err) = UTF16PtrFromString(machineName);
        if (err != nil) {
            return (default, err);
        }
    }

    ref var reqSize = ref heap<uint32>((uint32)4, out var ᏑreqSize);
    for (;;) {
        var buf = new slice<GUID>((nint)(reqSize), () => new());
        err = setupDiClassGuidsFromNameEx(classNameUTF16, Ꮡ(buf, 0), (uint32)len(buf), ᏑreqSize, machineNameUTF16, 0);
        if (AreEqual(err, ERROR_INSUFFICIENT_BUFFER)) {
            continue;
        }
        if (err != nil) {
            return (default, err);
        }
        return (buf.slice(0, (nint)(reqSize)), default);
    }
}

public static (ж<DevInfoData>, error) SetupDiGetSelectedDevice(DevInfo deviceInfoSet) {
    var data = Ꮡ(new DevInfoData());
    data.size = (uint32)/* unsafe.Sizeof(*data) */ (uintptr)32;

    return (data, setupDiGetSelectedDevice(deviceInfoSet, data));
}

} partial class windowsᴛDevInfo {

public static (ж<DevInfoData>, error) SelectedDevice(this DevInfo deviceInfoSet) {
    return SetupDiGetSelectedDevice(deviceInfoSet);
}

public static error SetSelectedDevice(this DevInfo deviceInfoSet, ж<DevInfoData> deviceInfoData) {
    return SetupDiSetSelectedDevice(deviceInfoSet, deviceInfoData);
}

} partial class windows_package {

public static error SetupUninstallOEMInf(@string infFileName, SUOI flags) {
    var (infFileName16, err) = UTF16PtrFromString(infFileName);
    if (err != nil) {
        return err;
    }
    return setupUninstallOEMInf(infFileName16, flags, 0);
}

public static (slice<@string>, error) CM_Get_Device_Interface_List(@string deviceID, ж<GUID> interfaceClass, uint32 flags) {
    var (deviceID16, err) = UTF16PtrFromString(deviceID);
    if (err != nil) {
        return (default, err);
    }
    slice<uint16> buf = default;
    ref var buflen = ref heap(new uint32(), out var Ꮡbuflen);
    for (;;) {
        {
            var ret = cm_Get_Device_Interface_List_Size(Ꮡbuflen, interfaceClass, deviceID16, flags); if (ret != CR_SUCCESS) {
                return (default, ret);
            }
        }
        buf = new slice<uint16>((nint)(buflen));
        {
            var ret = cm_Get_Device_Interface_List(interfaceClass, deviceID16, Ꮡ(buf, 0), buflen, flags); if (ret == CR_SUCCESS) {
                break;
            } else if (ret != CR_BUFFER_SMALL) {
                return (default, ret);
            }
        }
    }
    slice<@string> interfaces = default;
    for (nint i = 0; i < len(buf); ) {
        nint j = i + wcslen(buf.slice(i));
        if (i < j) {
            interfaces = append(interfaces, UTF16ToString(buf.slice(i, j)));
        }
        i = j + 1;
    }
    if (interfaces == nil) {
        return (default, ERROR_NO_SUCH_DEVICE_INTERFACE);
    }
    return (interfaces, default);
}

public static error CM_Get_DevNode_Status(ж<uint32> status, ж<uint32> problemNumber, DEVINST devInst, uint32 flags) {
    var ret = cm_Get_DevNode_Status(status, problemNumber, devInst, flags);
    if (ret == CR_SUCCESS) {
        return default;
    }
    return ret;
}

} // end windows_package
