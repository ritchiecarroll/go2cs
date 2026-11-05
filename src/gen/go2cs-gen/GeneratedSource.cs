// GeneratedSource.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Text;
using Microsoft.CodeAnalysis;

namespace go2cs;

/// <summary>
/// The one door every generator's file goes through: <c>AddGeneratedSource</c> adds it with the whitespace half of the
/// project's C# style applied (docs/PLAN-generated-code-format.md, classes D to G). Line endings are CRLF, no line ends
/// in a space or tab, no two blank lines are adjacent, the file neither starts nor ends with a blank line, and it ends
/// with exactly one line break.
/// </summary>
/// <remarks>
/// The templates stay as they are written: a raw-string line that only holds interpolations, or a blank line
/// indented past its closing quotes, leaves a whitespace-only line, and this pass removes it in one place rather than
/// contorting every template around it. The pass is lexical, so it could change a program only where a line break
/// falls inside a string literal, which takes a verbatim or raw literal. A text that contains either opener is added
/// unchanged, and GenTests' format guard names such a file if it leaves the style.
/// </remarks>
public static class GeneratedSource
{
    /// <summary>
    /// Adds <paramref name="source"/> as <paramref name="hintName"/>, normalized by <see cref="Normalize"/>.
    /// </summary>
    public static void AddGeneratedSource(this GeneratorExecutionContext context, string hintName, string source) =>
        context.AddSource(hintName, Normalize(source));

    /// <summary>
    /// Adds <paramref name="source"/> as <paramref name="hintName"/>, normalized by <see cref="Normalize"/>.
    /// </summary>
    public static void AddGeneratedSource(this SourceProductionContext context, string hintName, string source) =>
        context.AddSource(hintName, Normalize(source));

    /// <summary>
    /// <paramref name="source"/> with CRLF line endings, no trailing whitespace, no run of blank lines, no leading or
    /// trailing blank line, and one final line break; unchanged when it holds a verbatim or raw string literal.
    /// </summary>
    public static string Normalize(string source)
    {
        if (source.Contains("@\"") || source.Contains("@$\"") || source.Contains("\"\"\""))
            return source;

        string[] lines = source.Replace("\r\n", "\n").Split('\n');
        StringBuilder result = new(source.Length + 64);
        bool pendingBlank = false;

        foreach (string line in lines)
        {
            string trimmed = line.TrimEnd(' ', '\t', '\r');

            if (trimmed.Length == 0)
            {
                pendingBlank = result.Length > 0;
                continue;
            }

            if (pendingBlank)
                result.Append("\r\n");

            result.Append(trimmed).Append("\r\n");
            pendingBlank = false;
        }

        return result.ToString();
    }
}
