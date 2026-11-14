// GoManualConversionAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Mark a file as having been manually converted from Go to C#.
/// </summary>
/// <remarks>
/// <para>
/// This attribute should be applied to files that have been manually converted from Go to C#.
/// When encountered, it indicates that the file should not be automatically converted again.
/// </para>
/// <para>
/// Note that the module-level scope is used as a pseudo file-level marker for this attribute.
/// </para>
/// </remarks>
// Note: This attribute uses 'module' scope, which technically applies at the compiled module
// level. Since typical builds produce only a single module from multiple source files, this
// attribute acts effectively as a file-level marker to indicate this file has been manually
// converted from Go to C#. Multiple instances across different source files within the same
// module are allowed by the compiler without conflict, serving the intended purpose. The go2cs
// conversion tool detects this attribute by scanning the target source code file directly, thus
// identifying when to skip re-conversion. Note that actual module-level metadata stored in the
// assembly will likely serialize only a single instance of this attribute, even when multiple
// instances are defined across various files. As a result, reflection-based detection of this
// attribute only confirms that at least one file in the module was manually converted, not
// identifying individual files.
[AttributeUsage(AttributeTargets.Module, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoManualConversionAttribute : Attribute
{
}
