namespace NSMedieval.Tools
{
    using System;
    using System.Collections;
    using System.IO;
    using UnityEngine;

    /// <summary>
    ///     Methods for working with file paths.
    ///     (checking if a path exists. Creating directories in case path does not exist.)
    /// </summary>
    public static class FilePathUtils
    {
        /// <summary>
        ///     Checks if the given path (or the path of the given filename) exists.
        ///     If the path does not exist, the missing directories will be created.
        /// </summary>
        /// <param name="fullPathToCheck"></param>
        public static void CheckAndCreatePath(string fullPathToCheck)
        {
            string fullPath = Path.GetFullPath(fullPathToCheck);

            // Don't do anything if the given file or path exists.
            if (File.Exists(fullPath) || Directory.Exists(fullPath))
            {
                return;
            }

            // Try to create the missing directories.
            CreateMissingDirectories(fullPath);
        }

        /// <summary>
        ///     Create the missing directories of the given path.
        ///     For example if te given path is "C:/data/pictures/landscapes", and the directory "pictures" does not exist in
        ///     C:/data, then both "pictures" and "landscapes" directories will be created.
        /// </summary>
        private static void CreateMissingDirectories(string fullPath)
        {
            Debug.Log($"Creating missing directories in path: {fullPath}");

            // Extension == it must be a file
            DirectoryInfo dir = new(fullPath);
            if (dir.Extension.Length > 0)
            {
                dir = dir.Parent;
            }

            // Push all directories into a stack.
            Stack directories = new();
            directories.Push(dir);
            while (dir.Parent != null)
            {
                DirectoryInfo parent = dir.Parent;
                directories.Push(parent);
                dir = parent;
            }

            // Check all directories from the stack, and try to create the missing ones.
            while (directories.Count > 0)
            {
                DirectoryInfo directory = (DirectoryInfo)directories.Pop();
                if (directory.Exists)
                {
                    continue;
                }

                Debug.Log($"Creating missing directory: {directory.FullName}");
                try
                {
                    Directory.CreateDirectory(directory.FullName);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create missing directory: {e}");
                    throw;
                }
            }
        }
    }
}