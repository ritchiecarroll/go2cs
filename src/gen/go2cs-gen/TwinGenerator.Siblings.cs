// TwinGenerator.Siblings.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace go2cs;

// M:N SPIKE (R5b, B4). Spike code: it stays on the spike branch and never lands.
// How the twin generator gets to see what its sibling generators add, and what each way costs.

/// <summary>
/// The twin generator under a name that sorts after every sibling's: the compiler runs one assembly's generators in
/// the order of their full type names, so this one finds every sibling's output in the memo and runs none of them
/// a second time. It works only for a project built with <c>GoTwinsSiblings=last</c>; its files then go to this
/// type's directory.
/// </summary>
[Generator]
public sealed class ZTwinGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context) =>
        TwinGenerator.Execute(context, typeof(ZTwinGenerator));
}

public sealed partial class TwinGenerator
{
    /// <summary>
    /// The sibling generators' output of the running compilation, kept as they add it. Every sibling's file goes
    /// through <c>AddGeneratedSource</c>, and the compiler runs the generators of one assembly one after the other
    /// over the same <see cref="Compilation"/>, in the order of their full type names, so the twin generator can
    /// read here what the siblings that sort before it produced. A sibling that sorts after it has not run yet:
    /// those alone run a second time, nested. Off unless the project sets <c>GoTwinsSiblings=memo</c> (this
    /// generator under its own name, the late siblings nested) or <c>GoTwinsSiblings=last</c> (the work moves to
    /// <see cref="ZTwinGenerator"/>, and nothing runs twice).
    /// </summary>
    internal static class SiblingMemo
    {
        private static readonly ConditionalWeakTable<Compilation, List<KeyValuePair<string, string>>> s_sources = new();

        [ThreadStatic]
        private static bool t_nested;

        internal static bool Nested
        {
            get => t_nested;
            set => t_nested = value;
        }

        internal static bool On(AnalyzerConfigOptions options) =>
            options.TryGetValue("build_property.GoTwins", out string? twins) && string.Equals(twins, "true", StringComparison.OrdinalIgnoreCase) &&
            options.TryGetValue("build_property.GoTwinsSiblings", out string? siblings) &&
            (string.Equals(siblings, "memo", StringComparison.OrdinalIgnoreCase) || string.Equals(siblings, "last", StringComparison.OrdinalIgnoreCase));

        internal static bool Last(AnalyzerConfigOptions options) =>
            options.TryGetValue("build_property.GoTwinsSiblings", out string? siblings) && string.Equals(siblings, "last", StringComparison.OrdinalIgnoreCase);

        // Returns the source it was given, so the one door keeps its one statement.
        internal static string Record(GeneratorExecutionContext context, string hintName, string source)
        {
            if (t_nested || !On(context.AnalyzerConfigOptions.GlobalOptions))
                return source;

            List<KeyValuePair<string, string>> sources = s_sources.GetOrCreateValue(context.Compilation);

            lock (sources)
                sources.Add(new KeyValuePair<string, string>(hintName, source));

            return source;
        }

        internal static KeyValuePair<string, string>[]? Take(Compilation compilation)
        {
            if (!s_sources.TryGetValue(compilation, out List<KeyValuePair<string, string>>? sources))
                return null;

            lock (sources)
                return sources.ToArray();
        }
    }

    internal sealed partial class Run
    {
        private Stopwatch m_watch = null!;
        private long m_siblingMs, m_analysisMs;
        private string m_siblings = "nested";
        private int m_siblingSources, m_memoSources, m_nestedGenerators;

        /// <summary>The generator type this run works for: the siblings that sort after it have not run yet.</summary>
        internal Type Self = typeof(TwinGenerator);

        private bool AddSiblingsFromMemo()
        {
            if (!SiblingMemo.On(Context.AnalyzerConfigOptions.GlobalOptions) || SiblingMemo.Take(Context.Compilation) is not { Length: > 0 } sources)
                return false;

            // The siblings that sort after this generator have added nothing yet.
            string self = Self.FullName!;
            Type[] late = SiblingTypes().Where(type => StringComparer.OrdinalIgnoreCase.Compare(type.FullName, self) > 0).ToArray();

            if (late.Length > 0)
                RunNested(late);
            else
                m_augmented = Context.Compilation;

            CSharpParseOptions options = (CSharpParseOptions)Context.ParseOptions;

            m_augmented = m_augmented.AddSyntaxTrees(sources.Select(source =>
                CSharpSyntaxTree.ParseText(SourceText.From(source.Value, Encoding.UTF8), options, source.Key, Context.CancellationToken)));

            m_siblings = Self == typeof(TwinGenerator) ? "memo" : "last";
            m_memoSources = sources.Length;
            m_siblingSources += sources.Length;

            return true;
        }

        private void AddSiblingsNested() =>
            RunNested(SiblingTypes());

        private static Type[] SiblingTypes() =>
            typeof(TwinGenerator).Assembly.GetTypes()
                .Where(type => type != typeof(TwinGenerator) && type != typeof(ZTwinGenerator) && !type.IsAbstract && typeof(ISourceGenerator).IsAssignableFrom(type) &&
                               type.GetCustomAttribute<GeneratorAttribute>() is not null)
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToArray();

        private void RunNested(Type[] types)
        {
            ISourceGenerator[] siblings = types.Select(type => (ISourceGenerator)Activator.CreateInstance(type)).ToArray();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(siblings, Context.AdditionalFiles, (CSharpParseOptions)Context.ParseOptions, Context.AnalyzerConfigOptions);

            SiblingMemo.Nested = true;

            try
            {
                driver = driver.RunGeneratorsAndUpdateCompilation(Context.Compilation, out m_augmented, out _, Context.CancellationToken);
            }
            finally
            {
                SiblingMemo.Nested = false;
            }

            m_nestedGenerators = siblings.Length;
            m_siblingSources = driver.GetRunResult().GeneratedTrees.Length;
        }

        // The count is the control between the two ways: equal counts and equal twin files say the memo and the
        // late siblings together gave what the full nested run produces. The times are written only on request,
        // since they differ from build to build.
        private void ReportSiblings(StringBuilder report)
        {
            report.Append("// siblings=").Append(m_siblings).Append(" sibling-sources=").Append(m_siblingSources)
                .Append(" memo-sources=").Append(m_memoSources).Append(" nested-generators=").Append(m_nestedGenerators).Append("\r\n");

            if (!Context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GoTwinsTiming", out string? timing) ||
                !string.Equals(timing, "true", StringComparison.OrdinalIgnoreCase))
                return;

            report.Append("// timing sibling-ms=").Append(m_siblingMs).Append(" analysis-ms=").Append(m_analysisMs)
                .Append(" emit-ms=").Append(m_watch.ElapsedMilliseconds - m_siblingMs - m_analysisMs).Append("\r\n");
        }
    }
}
