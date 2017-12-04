using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VSL.FileTransfer
{
    public abstract class FTSocket
    {
        internal abstract void ReceiveHeader(FTEventArgs e);
        internal abstract void ReceiveFile(FTEventArgs e);
        internal abstract void SendHeader(FTEventArgs e);
        /// <summary>
        /// Sends only the raw file data.
        /// </summary>
        /// <param name="e">Work Item</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FileNotFoundException"/>
        internal protected virtual void SendFile(FTEventArgs e)
        {
            if (e.Path != null)
                throw new ArgumentNullException("e.Path", "You must specify the path of the file");
            if (!File.Exists(e.Path))
                throw new FileNotFoundException("You can only send existing files", e.Path);
            e.FileStream = new FileStream(e.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

        }
        public abstract void Cancel(FTEventArgs e);
        #region util
        /// <summary>
        /// Writes all meta data to a file at the specified path. The file will stay in its directory but will be renamed to its original name which will be returned.
        /// </summary>
        /// <param name="fi">Meta data that will be applied except the directory.</param>
        /// <param name="path">Current path of the file.</param>
        /// <returns>The new path of the file.</returns>
        public static string ApplyMetaData(FileInfo fi, string path)
        {
            FileInfo current = new FileInfo(path);
            string directory = current.DirectoryName;
            if (File.Exists(Path.Combine(directory, fi.Name)))
            {
                long count = 2;
                while (true)
                {
                    string newpath = Path.Combine(directory, fi.Name + " (" + count + ")");
                    if (File.Exists(newpath))
                        count++;
                    else
                    {
                        current.MoveTo(newpath);
                        break;
                    }
                }
            }
            fi.Attributes = current.Attributes;
            fi.CreationTime = current.CreationTime;
            fi.LastAccessTime = current.LastAccessTime;
            fi.LastWriteTime = current.LastWriteTime;
            return fi.FullName;
        }
        #endregion
    }
}