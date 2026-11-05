// GeneratedWhitespace.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;

namespace go2cs.Tests;

/// <summary>
/// Reads generated C# for the whitespace half of the project's style (docs/coding-style.md rules 2 and 7,
/// src/.editorconfig's <c>insert_final_newline</c>, and the CRLF every go2cs-gen writer uses): no LF without its CR, no
/// line ending in a space or tab, no two blank lines in a row, no blank first line, and exactly one final line break.
/// </summary>
/// <remarks>
/// The scout's census (docs/PLAN-generated-code-format.md, classes D to G) found each of these in generated files:
/// whitespace-only lines from raw-string templates in 862 files, blank runs in 633, no final newline in 292, and LF
/// from <c>StringBuilder.AppendLine</c> mixed into CRLF text in 698 on Linux.
/// </remarks>
internal static class GeneratedWhitespace
{
    /// <summary>
    /// Every whitespace finding in <paramref name="text"/>, as <c>kind line</c> with a 1-based line.
    /// </summary>
    public static List<string> Findings(string text)
    {
        List<string> findings = [];
        int line = 1, lineStart = 0;
        bool previousBlank = false;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != '\n')
                continue;

            bool crlf = i > lineStart && text[i - 1] == '\r';
            string content = text.Substring(lineStart, i - lineStart - (crlf ? 1 : 0));
            bool blank = content.Length == 0;

            if (!crlf)
                findings.Add($"lf {line}");

            if (content.Length > 0 && (content[^1] == ' ' || content[^1] == '\t'))
                findings.Add($"trailing {line}");

            if (blank && line == 1)
                findings.Add("leading blank 1");
            else if (blank && previousBlank)
                findings.Add($"blank run {line}");

            previousBlank = blank || content.Trim().Length == 0;
            lineStart = i + 1;
            line++;
        }

        if (lineStart < text.Length)
            findings.Add($"no final newline {line}");
        else if (previousBlank && line > 1)
            findings.Add($"trailing blank {line - 1}");

        return findings;
    }
}
