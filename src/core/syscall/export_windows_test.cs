// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;
using static go.syscall_package;

partial class syscall_internal_test_package {

internal static Func<uint32, (ж<global::go.syscall_package.procThreadAttributeListContainer>, error)> NewProcThreadAttributeList;
internal static void initᴛNewProcThreadAttributeList() { NewProcThreadAttributeList = newProcThreadAttributeList; }

internal static Func<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>, uint32, uintptr, @unsafe.Pointer, uintptr, @unsafe.Pointer, ж<uintptr>, error> UpdateProcThreadAttribute;
internal static void initᴛUpdateProcThreadAttribute() { UpdateProcThreadAttribute = updateProcThreadAttribute; }

internal static Action<ж<global::go.syscall_package._PROC_THREAD_ATTRIBUTE_LIST>> DeleteProcThreadAttributeList;
internal static void initᴛDeleteProcThreadAttributeList() { DeleteProcThreadAttributeList = deleteProcThreadAttributeList; }

public static UntypedInt PROC_THREAD_ATTRIBUTE_HANDLE_LIST => /* _PROC_THREAD_ATTRIBUTE_HANDLE_LIST */ 131074;

public static Func<@string, slice<uint16>, slice<uint16>> EncodeWTF16 = encodeWTF16;

public static Func<slice<uint16>, slice<byte>, slice<byte>> DecodeWTF16 = decodeWTF16;

public static Func<slice<@string>, slice<@string>> EnvSorted = envSorted;

} // end syscall_internal_test_package
