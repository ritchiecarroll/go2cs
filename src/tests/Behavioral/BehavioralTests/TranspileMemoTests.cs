// TranspileMemoTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

// Four test classes ask TranspileProject for every fixture: Transpile FORCED, then Compile, Target and Output unforced.
// The unforced calls skip only when every .cs is newer than its .go and than the converter. Since the converter writes
// an unchanged source byte-compared (incremental .cs writes), an unchanged emission keeps its .cs OLDER than a rebuilt
// converter, so all three unforced calls re-ran the converter for every fixture, on every run after a converter rebuild:
// measured 2026-10-06 at 554 s per pass of the corpus, three passes per full run.
[TestClass]
public class TranspileMemoTests : BehavioralTestBase
{
    // A single-package fixture whose committed .cs is what the converter emits today.
    private const string Fixture = "ZeroValueStructVar";

    [ClassInitialize]
    public static void Initialize(TestContext context) => Init(context);

    [TestMethod]
    public void AnUnchangedEmissionIsTranspiledOncePerProcess()
    {
        string projPath = Path.GetFullPath($"{TestRootPath}{Fixture}");

        // Settle the emission first, so the call measured below rewrites nothing.
        TranspileProject(Fixture, true);

        // The state a converter rebuild leaves: every .cs of an unchanged emission older than the converter.
        string[] sources = Directory.GetFiles(projPath, "*.cs", SearchOption.AllDirectories)
            .Where(fileName => !fileName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                               !fileName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();
        DateTime[] times = sources.Select(File.GetLastWriteTimeUtc).ToArray();
        DateTime older = File.GetLastWriteTimeUtc(go2cs).AddHours(-1);

        try
        {
            foreach (string source in sources)
                File.SetLastWriteTimeUtc(source, older);

            int before = ConverterRuns(Fixture);

            // Transpile: FORCED, so it runs although this process already transpiled the fixture above.
            TranspileProject(Fixture, true);

            Assert.AreEqual(1, ConverterRuns(Fixture) - before, "a FORCED call must still run the converter");

            TranspileProject(Fixture);          // Compile
            TranspileProject(Fixture);          // Target
            TranspileProject(Fixture);          // Output

            Assert.AreEqual(1, ConverterRuns(Fixture) - before,
                "one process ran the converter more than once over an unchanged emission: every test class after Transpile re-transpiled it");
        }
        finally
        {
            for (int i = 0; i < sources.Length; i++)
                File.SetLastWriteTimeUtc(sources[i], times[i]);
        }
    }
}
