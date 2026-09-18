using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public void LooksBinaryDetectsNulAndNull()
        {
            Assert.IsTrue(ProjectDirectoryTextFiles.LooksBinary(null));
            Assert.IsFalse(ProjectDirectoryTextFiles.LooksBinary(Array.Empty<byte>()));
            Assert.IsTrue(ProjectDirectoryTextFiles.LooksBinary(new byte[] { 0 }));
            Assert.IsFalse(ProjectDirectoryTextFiles.LooksBinary(new byte[] { 65, 66 }));

            var underLimit = new byte[400];
            for (var i = 0; i < underLimit.Length; i++)
            {
                underLimit[i] = 1;
            }

            Assert.IsFalse(ProjectDirectoryTextFiles.LooksBinary(underLimit));

            var atLimit = new byte[512];
            for (var i = 0; i < atLimit.Length; i++)
            {
                atLimit[i] = 1;
            }

            Assert.IsFalse(ProjectDirectoryTextFiles.LooksBinary(atLimit));

            var overLimit = new byte[513];
            for (var i = 0; i < overLimit.Length; i++)
            {
                overLimit[i] = 1;
            }

            overLimit[512] = 0;
            Assert.IsFalse(ProjectDirectoryTextFiles.LooksBinary(overLimit));
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
        public void TryGetDirectoryEntriesReturnsFalseWhenMissing()
        {
            string[] files;
            string[] directories;
            Assert.IsFalse(ProjectDirectoryTextFiles.TryGetDirectoryEntries(
                Path.Combine(Path.GetTempPath(), "coree-analyzers-missing-" + Guid.NewGuid().ToString("N")),
                out files,
                out directories));
            Assert.AreEqual(0, files.Length);
            Assert.AreEqual(0, directories.Length);
        }

        [TestMethod]
        public void TryReadTextReturnsNullForMissingAndBinary()
        {
            Assert.IsNull(ProjectDirectoryTextFiles.TryReadText(
                Path.Combine(Path.GetTempPath(), "coree-analyzers-missing-" + Guid.NewGuid().ToString("N") + ".txt")));

            var dir = CreateTempDirectory();
            try
            {
                Assert.IsNull(ProjectDirectoryTextFiles.TryReadText(dir));

                var binary = Path.Combine(dir, "blob.bin");
                File.WriteAllBytes(binary, new byte[] { 0, 1, 2 });
                Assert.IsNull(ProjectDirectoryTextFiles.TryReadText(binary));

                var textPath = Path.Combine(dir, "notes.md");
                File.WriteAllText(textPath, "hello");
                Assert.AreEqual("hello", ProjectDirectoryTextFiles.TryReadText(textPath).ToString());

                string[] files;
                string[] directories;
                Assert.IsFalse(ProjectDirectoryTextFiles.TryGetDirectoryEntries(textPath, out files, out directories));
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void EnumerateFilesSkipsBuildFoldersAndMissingRoots()
        {
            Assert.AreEqual(0, ProjectDirectoryTextFiles.EnumerateFiles(null).Count());
            Assert.AreEqual(0, ProjectDirectoryTextFiles.EnumerateFiles("  ").Count());
            Assert.AreEqual(
                0,
                ProjectDirectoryTextFiles.EnumerateFiles(
                    Path.Combine(Path.GetTempPath(), "coree-analyzers-missing-" + Guid.NewGuid().ToString("N"))).Count());

            var dir = CreateTempDirectory();
            try
            {
                File.WriteAllText(Path.Combine(dir, "root.md"), "root");
                Directory.CreateDirectory(Path.Combine(dir, "docs"));
                File.WriteAllText(Path.Combine(dir, "docs", "nested.md"), "nested");
                WriteIgnored("bin");
                WriteIgnored("obj");
                WriteIgnored(".git");
                WriteIgnored(".vs");

                var found = ProjectDirectoryTextFiles.EnumerateFiles(dir)
                    .Select(path => Path.GetFileName(path))
                    .ToList();
                CollectionAssert.AreEquivalent(new[] { "root.md", "nested.md" }, found);

                void WriteIgnored(string name)
                {
                    var ignored = Path.Combine(dir, name);
                    Directory.CreateDirectory(ignored);
                    File.WriteAllText(Path.Combine(ignored, "skip.md"), "skip");
                }
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void TryGetDirectoryEntriesListsFilesAndSubdirectories()
        {
            var dir = CreateTempDirectory();
            try
            {
                File.WriteAllText(Path.Combine(dir, "a.txt"), "a");
                Directory.CreateDirectory(Path.Combine(dir, "sub"));
                string[] files;
                string[] directories;
                Assert.IsTrue(ProjectDirectoryTextFiles.TryGetDirectoryEntries(dir, out files, out directories));
                Assert.AreEqual(1, files.Length);
                Assert.AreEqual(1, directories.Length);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        private static string CreateTempDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), "coree-analyzers-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
