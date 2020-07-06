using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileContentFinder.Models
{
    static class FileFinder
    {
        /// <summary>
        /// Finds all files in <paramref name="path"/> that contain the <paramref name="query"/> parameter
        /// </summary>
        /// <param name="path">The path in which the files should be searched in</param>
        /// <param name="query">The test string that the files must contain</param>
        /// <returns>Returns a list of filenames in the directory</returns>
        public static async IAsyncEnumerable<string> FindNext(string path, string query, bool recursive, bool useRegex)
        {
            byte[] queryBytes = Encoding.UTF8.GetBytes(query);
            Stack<string> dirs = new Stack<string>();
            dirs.Push(path);

            do
            {
                string dir = dirs.Pop();

                string[] subdirs = Directory.GetDirectories(dir);
                foreach (var subdir in subdirs)
                    dirs.Push(subdir);

                string[] subjects = Directory.GetFiles(dir);

                if (query.Length == 0)
                    break;

                for (int i = 0; i < subjects.Length; i++)
                {
                    bool matches = false;

                    try
                    {
                        using (var stream = File.OpenRead(subjects[i]))
                        {
                            byte[] bytes = new byte[Math.Min(stream.Length, 8 * 1000000)]; // Read 8mb max
                            byte[] prevBytes = new byte[bytes.Length];

                            for (int j = 0; stream.Position < stream.Length; j++)
                            {
                                // Read the file piece after piece in max 8mb pieces
                                await stream.ReadAsync(bytes, 0, bytes.Length);

                                if (useRegex)
                                {
                                    if (Regex.IsMatch(Encoding.UTF8.GetString(bytes) + Encoding.UTF8.GetString(prevBytes), query))
                                    {
                                        matches = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (ByteArraysContain(bytes, prevBytes, queryBytes))
                                    {
                                        matches = true;
                                        break;
                                    }
                                }

                                prevBytes = bytes;
                            }
                        }

                    }
                    catch (UnauthorizedAccessException e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (PathTooLongException e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("An unknown exception occured. Please contact the developer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    if (matches)
                    {
                        yield return Path.GetRelativePath(path, subjects[i]);
                    }
                }
            }
            while (dirs.Count > 0 && recursive);
        }

        /// <summary>
        /// Checks if two byte arrays combined contain a certain byte array
        /// </summary>
        /// <param name="arr1">first array</param>
        /// <param name="arr2">second array</param>
        /// <param name="value">search value</param>
        /// <returns>True if both arrays combined contain <paramref name="value"/>, otherwise false</returns>
        private static bool ByteArraysContain(byte[] arr1, byte[] arr2, byte[] value)
        {
            int matches = 0;

            for (int i = 0; i < (arr1.Length + arr2.Length); i++)
            {
                if (i < arr1.Length)
                {
                    if (arr1[i] == value[matches])
                        matches++;
                    else
                        matches = 0;
                }
                else
                {
                    if (arr2[i - arr1.Length] == value[matches])
                        matches++;
                    else
                        matches = 0;
                }

                if (matches == value.Length)
                    return true;
            }

            return false;
        }
    }
}
