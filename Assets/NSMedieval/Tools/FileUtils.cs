namespace NSMedieval.Tools
{
    using System;
    using System.IO;
    using System.Threading;
    using UnityEngine;

    public static class FileUtils
    {
        public delegate T SafeReadDataFromFile<T>(string path);

        public delegate void SafeWriteDataToFile<T>(string path, T data);

        private const int MaxTries = 5;
        private const int WaitFixed = 25;
        private const int WaitAdd = 20;

        public static void SafeFileOperation(Action action)
        {
            int iterations = MaxTries;
            int wait = WaitFixed;
            while (iterations-- >= 0)
            {
                try
                {
                    action();
                    return;
                }
                catch (IOException e)
                {
                    if (e.Message.StartsWith("Sharing violation"))
                    {
                        Debug.Log($"SafeFileOperation<{action.Method.Name}>: Retrying in {wait} ms");
                        Thread.Sleep(wait);
                        wait += WaitAdd;
                    }
                    else
                    {
                        throw;
                    }

                    if (iterations == 0)
                    {
                        Debug.Log($"SafeFileOperation<{action.Method.Name}>: Failed after {MaxTries} retries. Throwing exception.");
                        throw;
                    }
                }
            }
        }

        private static T SafeFileOperation<T>(string path, SafeReadDataFromFile<T> reader)
        {
            string fn = Path.GetFileName(path);
            int iterations = MaxTries;
            int wait = WaitFixed;
            while (iterations-- > 0)
            {
                try
                {
                    T dataRead = reader(path);
                    return dataRead;
                }
                catch (Exception e)
                {
                    if (e is IOException || e is UnauthorizedAccessException || e.Message.StartsWith("Sharing violation"))
                    {
                        Debug.Log($"SafeFileOperation<{typeof(T)}>: Retrying reading file {fn} in {wait} ms");
                        Thread.Sleep(wait);
                        wait += WaitAdd;
                    }
                    else
                    {
                        Thread.Sleep(wait);
                    }

                    if (iterations == 0)
                    {
                        Debug.LogWarning($"SafeFileOperation<{typeof(T)}>: Cannot read file {fn}  after {MaxTries} retries. Throwing exception.");
                        throw;
                    }
                }
            }

            return default;
        }

        private static void SafeWriteFileOperation<T>(string path, SafeWriteDataToFile<T> writer, T data)
        {
            string fn = Path.GetFileName(path);
            int iterations = MaxTries;
            int wait = WaitFixed;
            while (iterations-- > 0)
            {
                try
                {
                    writer(path, data);
                    Debug.Log($"SafeWriteFileOperation<{typeof(T)}>: success writing {fn}");
                    return;
                }
                catch (IOException e)
                {
                    if (e.Message.StartsWith("Sharing violation"))
                    {
                        Debug.Log($"SafeWriteFileOperation<{typeof(T)}>: Retrying writing file {fn} in {wait} ms");
                        Thread.Sleep(wait);
                        wait += WaitAdd;
                    }
                    else
                    {
                        throw;
                    }

                    if (iterations == 0)
                    {
                        Debug.Log($"SafeWriteFileOperation<{typeof(T)}>: Cannot write file {fn}  after {MaxTries} retries. Throwing exception.");
                        throw;
                    }
                }
            }
        }

        private static void WriteMemStreamToFile(string path, MemoryStream memoryStream)
        {
            using (FileStream fs = File.Create(path))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.CopyTo(fs);
            }
        }

        #region Public Methods for Reading

        public static byte[] SafeReadAllBytes(string path)
        {
            return SafeFileOperation(path, File.ReadAllBytes);
        }

        public static string SafeReadAllText(string path)
        {
            return SafeFileOperation(path, File.ReadAllText);
        }

        public static string[] SafeReadAllLines(string path)
        {
            return SafeFileOperation(path, File.ReadAllLines);
        }

        #endregion

        #region Public Methods for Writing

        public static void SafeWriteAllBytes(string path, byte[] data)
        {
            SafeWriteFileOperation(path, File.WriteAllBytes, data);
        }

        public static void SafeWriteAllText(string path, string data)
        {
            SafeWriteFileOperation(path, File.WriteAllText, data);
        }

        public static void SafeWriteAllLines(string path, string[] data)
        {
            SafeWriteFileOperation(path, File.WriteAllLines, data);
        }

        public static void SafeWriteMemoryStream(string zipFilename, MemoryStream ms)
        {
            SafeWriteFileOperation(zipFilename, WriteMemStreamToFile, ms);
        }

        #endregion
    }
}