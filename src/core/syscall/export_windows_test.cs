// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;
using static go.syscall_package;

partial class syscall_internal_test_package {

internal static readonly ж<Func<uint32, (ж<global::go.syscall_package.procThreadAttributeListContainer>, error)>> ᏑNewProcThreadAttributeList = new StaticBox<Func<uint32, (ж<global::go.syscall_package.procThreadAttributeListContainer>, error)>>(default(Func<uint32, (ж<global::go.syscall_package.procThreadAttributeListContainer>, error)>));
internal static ref Func<uint32, (ж<global::go.syscall_package.procThreadAttributeListContainer>, error)> NewProcThreadAttributeList => ref ((StaticBox<Func<uint32, (ж<global::go.syscall_package.procThreadAttributeListContainer>, error)>>)ᏑNewProcThreadAttributeList).ValueSlot;
internal static void initᴛNewProcThreadAttributeList() { NewProcThreadAttributeList = newProcThreadAttributeList; }

internal static readonly ж<Func<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>, uint32, uintptr, @unsafe.Pointer, uintptr, @unsafe.Pointer, ж<uintptr>, error>> ᏑUpdateProcThreadAttribute = new StaticBox<Func<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>, uint32, uintptr, @unsafe.Pointer, uintptr, @unsafe.Pointer, ж<uintptr>, error>>(default(Func<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>, uint32, uintptr, @unsafe.Pointer, uintptr, @unsafe.Pointer, ж<uintptr>, error>));
internal static ref Func<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>, uint32, uintptr, @unsafe.Pointer, uintptr, @unsafe.Pointer, ж<uintptr>, error> UpdateProcThreadAttribute => ref ((StaticBox<Func<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>, uint32, uintptr, @unsafe.Pointer, uintptr, @unsafe.Pointer, ж<uintptr>, error>>)ᏑUpdateProcThreadAttribute).ValueSlot;
internal static void initᴛUpdateProcThreadAttribute() { UpdateProcThreadAttribute = updateProcThreadAttribute; }

internal static readonly ж<Action<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>>> ᏑDeleteProcThreadAttributeList = new StaticBox<Action<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>>>(default(Action<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>>));
internal static ref Action<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>> DeleteProcThreadAttributeList => ref ((StaticBox<Action<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>>>)ᏑDeleteProcThreadAttributeList).ValueSlot;
internal static void initᴛDeleteProcThreadAttributeList() { DeleteProcThreadAttributeList = deleteProcThreadAttributeList; }

public static UntypedInt PROC_THREAD_ATTRIBUTE_HANDLE_LIST => /* _PROC_THREAD_ATTRIBUTE_HANDLE_LIST */ 131074;

public static readonly ж<Func<@string, slice<uint16>, slice<uint16>>> ᏑEncodeWTF16 = new StaticBox<Func<@string, slice<uint16>, slice<uint16>>>(encodeWTF16);
public static ref Func<@string, slice<uint16>, slice<uint16>> EncodeWTF16 => ref ((StaticBox<Func<@string, slice<uint16>, slice<uint16>>>)ᏑEncodeWTF16).ValueSlot;

public static readonly ж<Func<slice<uint16>, slice<byte>, slice<byte>>> ᏑDecodeWTF16 = new StaticBox<Func<slice<uint16>, slice<byte>, slice<byte>>>(decodeWTF16);
public static ref Func<slice<uint16>, slice<byte>, slice<byte>> DecodeWTF16 => ref ((StaticBox<Func<slice<uint16>, slice<byte>, slice<byte>>>)ᏑDecodeWTF16).ValueSlot;

public static readonly ж<Func<slice<@string>, slice<@string>>> ᏑEnvSorted = new StaticBox<Func<slice<@string>, slice<@string>>>(envSorted);
public static ref Func<slice<@string>, slice<@string>> EnvSorted => ref ((StaticBox<Func<slice<@string>, slice<@string>>>)ᏑEnvSorted).ValueSlot;

} // end syscall_internal_test_package
