using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coree.Analyzers.Typography.Tests
{
    [TestClass]
    public class ProjectDirectoryTextFilesTests
    {
        [TestMethod]
        public void IsIgnoredDirectoryNameRecognizesBuildAndVcsFolders()
        {
            Assert.IsFalse(ProjectDirectoryTextFiles.IsIgnoredDirectoryName(null));
            Assert.IsFalse(ProjectDirectoryTextFiles.IsIgnoredDirectoryName(string.Empty));
            Assert.IsFalse(ProjectDirectoryTextFiles.IsIgnoredDirectoryName("src"));
            Assert.IsTrue(ProjectDirectoryTextFiles.IsIgnoredDirectoryName("bin"));
            Assert.IsTrue(ProjectDirectoryTextFiles.IsIgnoredDirectoryName("OBJ"));
            Assert.IsTrue(ProjectDirectoryTextFiles.IsIgnoredDirectoryName(".git"));
            Assert.IsTrue(ProjectDirectoryTextFiles.IsIgnoredDirectoryName(".vs"));
        }

        [TestMethod]
        public void HasIgnoredDirectorySegmentDetectsBinObjGitAndVs()
        {
            Assert.IsFalse(ProjectDirectoryTextFiles.HasIgnoredDirectorySegment(null));
            Assert.IsFalse(ProjectDirectoryTextFiles.HasIgnoredDirectorySegment("  "));
            Assert.IsFalse(ProjectDirectoryTextFiles.HasIgnoredDirectorySegment("docs/notes.md"));
            Assert.IsTrue(ProjectDirectoryTextFiles.HasIgnoredDirectorySegment("bin/out.txt"));
            Assert.IsTrue(ProjectDirectoryTextFiles.HasIgnoredDirectorySegment(@"obj\Debug\x.txt"));
            Assert.IsTrue(ProjectDirectoryTextFiles.HasIgnoredDirectorySegment(".git/config"));
            Assert.IsTrue(ProjectDirectoryTextFiles.HasIgnoredDirectorySegment("src/.vs/slnx"));
            Assert.IsFalse(ProjectDirectoryTextFiles.HasIgnoredDirectorySegment("docs//notes.md"));
        }

        [TestMethod]
        public void ContainsNulReadsOnlyTheFirst512Characters()
        {
            Assert.IsFalse(ProjectDirectoryTextFiles.ContainsNul(null));
            Assert.IsFalse(ProjectDirectoryTextFiles.ContainsNul(SourceText.From(string.Empty)));
            Assert.IsTrue(ProjectDirectoryTextFiles.ContainsNul(SourceText.From("\0a")));
            Assert.IsFalse(ProjectDirectoryTextFiles.ContainsNul(SourceText.From("ab")));

            var underLimit = new string('a', 400);
            Assert.IsFalse(ProjectDirectoryTextFiles.ContainsNul(SourceText.From(underLimit)));

            var atLimit = new string('a', 512);
            Assert.IsFalse(ProjectDirectoryTextFiles.ContainsNul(SourceText.From(atLimit)));

            var overLimit = new string('a', 513) + "\0";
            Assert.IsFalse(ProjectDirectoryTextFiles.ContainsNul(SourceText.From(overLimit)));
        }

        [TestMethod]
        public void NormalizeFullPathKeepsBlankAndInvalidValues()
        {
            Assert.AreEqual(null, ProjectDirectoryTextFiles.NormalizeFullPath(null));
            Assert.AreEqual("  ", ProjectDirectoryTextFiles.NormalizeFullPath("  "));
            Assert.AreEqual("foo\0bar", ProjectDirectoryTextFiles.NormalizeFullPath("foo\0bar"));
            StringAssert.Contains(
                ProjectDirectoryTextFiles.NormalizeFullPath("notes.md"),
                "notes.md");
        }

        [TestMethod]
        public void AddSeenPathIgnoresNullSetAndBlankPath()
        {
            ProjectDirectoryTextFiles.AddSeenPath(null, @"C:\proj\a.txt");
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ProjectDirectoryTextFiles.AddSeenPath(seen, null);
            ProjectDirectoryTextFiles.AddSeenPath(seen, "  ");
            Assert.AreEqual(0, seen.Count);
            ProjectDirectoryTextFiles.AddSeenPath(seen, "notes.md");
            Assert.AreEqual(1, seen.Count);
        }

        [TestMethod]
        public void IsAlreadyCompiledMatchesNormalizedPaths()
        {
            Assert.IsFalse(ProjectDirectoryTextFiles.IsAlreadyCompiled(null, @"C:\proj\A.cs"));
            var compiled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Assert.IsFalse(ProjectDirectoryTextFiles.IsAlreadyCompiled(compiled, null));
            Assert.IsFalse(ProjectDirectoryTextFiles.IsAlreadyCompiled(compiled, "  "));
            ProjectDirectoryTextFiles.AddSeenPath(compiled, @"C:\proj\A.cs");
            Assert.IsTrue(ProjectDirectoryTextFiles.IsAlreadyCompiled(compiled, @"C:\proj\A.cs"));
            Assert.IsFalse(ProjectDirectoryTextFiles.IsAlreadyCompiled(compiled, @"C:\proj\B.cs"));
        }
    }
}
