// GoTestMatchingConsoleOutputAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Mark a package as suitable for comparing console output of Go and converted C# during testing.
/// </summary>
/// <remarks>
/// This attribute should be applied to converted packages that have deterministic console output.
/// In these cases, the output of the Go package should match the output of the converted C# code.
/// Applying this attribute enables automated testing of the console output to ensure consistency
/// between Go and C# for improved regression testing.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoTestMatchingConsoleOutputAttribute : Attribute
{
}
