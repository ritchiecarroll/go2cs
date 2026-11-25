// ClaimableWrapperMembers.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;

namespace go2cs.Templates.InheritedType;

/// <summary>
/// The ASCII members each inherited-type template declares for its own use that a Go method of the same name takes away
/// (R7): read by GenTests' NamedSliceArrayClaimedMemberTests, which fails on a generated public member that is neither on
/// its template's list nor renamed by the converter (identifierNaming.go <c>reserved</c>).
/// </summary>
public static class ClaimableWrapperMembers
{
    /// <summary>The named-slice wrapper's claimable members (ISliceTypeTemplate).</summary>
    public static IReadOnlyCollection<string> Slice => ISliceTypeTemplate.ClaimableMembers;

    /// <summary>The named-array wrapper's claimable members (IArrayTypeTemplate).</summary>
    public static IReadOnlyCollection<string> Array => IArrayTypeTemplate.ClaimableMembers;

    /// <summary>The claimable members of a type defined over a named array (IArrayViewTypeTemplate).</summary>
    public static IReadOnlyCollection<string> ArrayView => IArrayViewTypeTemplate.ClaimableMembers;

    /// <summary>The named-channel wrapper's claimable members (IChannelTypeTemplate).</summary>
    public static IReadOnlyCollection<string> Channel => IChannelTypeTemplate.ClaimableMembers;

    /// <summary>The named-map wrapper's claimable members (IMapTypeTemplate).</summary>
    public static IReadOnlyCollection<string> Map => IMapTypeTemplate.ClaimableMembers;

    /// <summary>The named-numeric wrapper's claimable members (NumericTypeTemplate).</summary>
    public static IReadOnlyCollection<string> Numeric => NumericTypeTemplate.ClaimableMembers;

    /// <summary>The named-string wrapper's claimable members (InheritedTypeTemplate's string surface).</summary>
    public static IReadOnlyCollection<string> String => InheritedTypeTemplate.StringClaimableMembers;

    /// <summary>The named-pointer wrapper's claimable members, claimed by the type's own name only (PointerTypeTemplate).</summary>
    public static IReadOnlyCollection<string> Pointer => PointerTypeTemplate.ClaimableMembers;
}
