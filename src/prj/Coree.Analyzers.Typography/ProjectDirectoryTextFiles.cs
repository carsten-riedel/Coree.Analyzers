#nullable disable
#pragma warning disable RS1035 // Walks the consuming project directory when include globs are set.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Text;

namespace Coree.Analyzers.Typography
{
    internal static class ProjectDirectoryTextFiles
    {
        internal static bool IsIgnoredDirectoryName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.Equals("bin", StringComparison.OrdinalIgnoreCase)
                || name.Equals("obj", StringComparison.OrdinalIgnoreCase)
                || name.Equals(".git", StringComparison.OrdinalIgnoreCase)
                || name.Equals(".vs", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool LooksBinary(byte[] bytes)
        {
            if (bytes == null)
            {
                return true;
            }

            var limit = bytes.Length < 512 ? bytes.Length : 512;
            for (var i = 0; i < limit; i++)
            {
                if (bytes[i] == 0)
                {
                    return true;
                }
            }

            return false;
        }

        internal static string NormalizeFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            try
            {
                return Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return path;
            }
        }

        internal static void AddSeenPath(HashSet<string> seen, string path)
        {
            if (seen == null || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            seen.Add(NormalizeFullPath(path));
        }

        internal static bool TryGetDirectoryEntries(string directory, out string[] files, out string[] directories)
        {
            files = Array.Empty<string>();
            directories = Array.Empty<string>();
            try
            {
                files = Directory.GetFiles(directory);
                directories = Directory.GetDirectories(directory);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static SourceText TryReadText(string path)
        {
            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception)
            {
                return null;
            }

            if (LooksBinary(bytes))
            {
                return null;
            }

            using (var stream = new MemoryStream(bytes))
            {
                return SourceText.From(stream, Encoding.UTF8);
            }
        }

        internal static IEnumerable<string> EnumerateFiles(string projectDirectory)
        {
            if (string.IsNullOrWhiteSpace(projectDirectory))
            {
                yield break;
            }

            var stack = new Stack<string>();
            stack.Push(projectDirectory);
            while (stack.Count > 0)
            {
                var directory = stack.Pop();
                string[] files;
                string[] directories;
                if (!TryGetDirectoryEntries(directory, out files, out directories))
                {
                    continue;
                }

                for (var i = 0; i < files.Length; i++)
                {
                    yield return files[i];
                }

                for (var i = 0; i < directories.Length; i++)
                {
                    if (IsIgnoredDirectoryName(Path.GetFileName(directories[i])))
                    {
                        continue;
                    }

                    stack.Push(directories[i]);
                }
            }
        }
    }
}
