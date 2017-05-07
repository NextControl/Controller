using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ManiaNextControl.FileManagement
{
    public class CFileIO
    {
        /// <summary>
        /// No pathfolder
        /// </summary>
        public static CFileIO Default = new CFileIO() { pathFolder = "" };
        public string pathFolder { internal set; get; }

        /// <summary>
        /// Read a file asynchronously
        /// </summary>
        /// <param name="filePath">folderPath + filePath</param>
        /// <returns></returns>
        public async Task<string> ReadTextAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(pathFolder + filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();

                byte[] buffer = new byte[0x1000];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string text = Encoding.UTF8.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Write into a file asynchronously
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task WriteTextAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(pathFolder + filePath,
                FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }
    }
}
