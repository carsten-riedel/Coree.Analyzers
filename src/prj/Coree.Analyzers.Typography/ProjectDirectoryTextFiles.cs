#nullable disable
using System;
using System.Collections.Generic;
using System.IO;

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

            bool bin = name.Equals("bin", StringComparison.OrdinalIgnoreCase);
            bool obj = name.Equals("obj", StringComparison.OrdinalIgnoreCase);
            bool git = name.Equals(".git", StringComparison.OrdinalIgnoreCase);
            bool vs = name.Equals(".vs", StringComparison.OrdinalIgnoreCase);
            return bin || obj || git || vs;
        }

        internal static bool HasIgnoredDirectorySegment(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            var normalized = relativePath.Replace('\\', '/');
            var start = 0;
            for (var i = 0; i <= normalized.Length; i++)
            {
                if (i != normalized.Length && normalized[i] != '/')
                {
                    continue;
                }

                var length = i - start;
                if (length > 0 && IsIgnoredDirectoryName(normalized.Substring(start, length)))
                {
                    return true;
                }

                start = i + 1;
            }

            return false;
        }

        internal static bool ContainsNul(SourceText text)
        {
            if (text == null)
            {
                return false;
            }

            var limit = text.Length < 512 ? text.Length : 512;
            for (var i = 0; i < limit; i++)
            {
                if (text[i] == '\0')
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

        internal static bool IsAlreadyCompiled(HashSet<string> compiledPaths, string path)
        {
            if (compiledPaths == null || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string fullPath = NormalizeFullPath(path);
            return compiledPaths.Contains(fullPath);
        }
    }
}
