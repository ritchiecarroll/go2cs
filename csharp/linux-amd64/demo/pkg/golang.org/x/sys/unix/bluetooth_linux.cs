// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class unix_package {

public static UntypedInt BTPROTO_L2CAP => 0;
public static UntypedInt BTPROTO_HCI => 1;
public static UntypedInt BTPROTO_SCO => 2;
public static UntypedInt BTPROTO_RFCOMM => 3;
public static UntypedInt BTPROTO_BNEP => 4;
public static UntypedInt BTPROTO_CMTP => 5;
public static UntypedInt BTPROTO_HIDP => 6;
public static UntypedInt BTPROTO_AVDTP => 7;

public static UntypedInt HCI_CHANNEL_RAW => 0;
public static UntypedInt HCI_CHANNEL_USER => 1;
public static UntypedInt HCI_CHANNEL_MONITOR => 2;
public static UntypedInt HCI_CHANNEL_CONTROL => 3;
public static UntypedInt HCI_CHANNEL_LOGGING => 4;

public static UntypedInt SOL_BLUETOOTH => 0x112;
public static UntypedInt SOL_HCI => 0x0;
public static UntypedInt SOL_L2CAP => 0x6;
public static UntypedInt SOL_RFCOMM => 0x12;
public static UntypedInt SOL_SCO => 0x11;

} // end unix_package
