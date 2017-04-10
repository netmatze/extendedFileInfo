using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ExtendedFileInfos
{
    public class ExtendedFileInfo : IFileInfo, IDisposable
    {
        private readonly string PREFIX = @"\\?\";
        private string filename;
        private string name;
        private long length;
        private string extention;
        private bool exists;
        private DateTime creationTime;
        private bool readOnly;

        private SafeFileHandle fileHandle;

        public ExtendedFileInfo(string filename)
        {
            this.filename = filename;
            SetAttributes();
        }

        private void SetAttributes()
        {
            var prefixFullname = PREFIX + filename;
            SafeFileHandle fileHandle = ExternalCalls.CreateFile(prefixFullname,
                ExternalCalls.ExtFileAccess.GenericRead, ExternalCalls.ExtFileShare.Read, IntPtr.Zero,
                ExternalCalls.ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            if (fileHandle != null && !fileHandle.IsInvalid)
            {
                using (var fileStream = new FileStream(fileHandle, FileAccess.Read))
                {
                    if (fileStream.Length > 0)
                        exists = true;
                    length = fileStream.Length;
                }
                name = filename.Split('\\').Last();
                extention = filename.Split('.').Last();
                IntPtr ptr = fileHandle.DangerousGetHandle();
                var ftCreationTime = new FILETIME();
                var ftLastAccessTime = new FILETIME();
                var ftLastWriteTime = new FILETIME();
                ExternalCalls.GetFileTime(ptr, ref ftCreationTime, ref ftLastAccessTime, ref ftLastWriteTime);
                var creationTime = DateTime.FromFileTimeUtc((((long)ftCreationTime.dwHighDateTime) << 32) | ((uint)ftCreationTime.dwLowDateTime));
                var lastAccessTime = DateTime.FromFileTimeUtc((((long)ftLastAccessTime.dwHighDateTime) << 32) | ((uint)ftLastAccessTime.dwLowDateTime));
                var lastWriteTime = DateTime.FromFileTimeUtc((((long)ftLastWriteTime.dwHighDateTime) << 32) | ((uint)ftLastWriteTime.dwLowDateTime));
                this.creationTime = creationTime;
                if (!fileHandle.IsClosed)
                    fileHandle.Close();
                if ((ExternalCalls.GetFileAttributes(prefixFullname) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    this.readOnly = true;
                }
            }
        }

        public bool Exists()
        {
            return exists;
        }

        public FileStream Open(FileMode fileMode, FileAccess fileAccess)
        {
            var prefixFullname = PREFIX + filename;
            if (fileMode == FileMode.Open)
            {
                if (fileAccess == FileAccess.Write)
                {
                    fileHandle = ExternalCalls.CreateFile(prefixFullname,
                       ExternalCalls.ExtFileAccess.GenericWrite, ExternalCalls.ExtFileShare.Write, IntPtr.Zero,
                       ExternalCalls.ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
                    return new FileStream(fileHandle, fileAccess);
                }
                else
                {
                    fileHandle = ExternalCalls.CreateFile(prefixFullname,
                        ExternalCalls.ExtFileAccess.GenericRead, ExternalCalls.ExtFileShare.Read, IntPtr.Zero,
                        ExternalCalls.ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
                    return new FileStream(fileHandle, fileAccess);
                }
            }
            else if(fileMode == FileMode.Create)
            {
                if (fileAccess == FileAccess.Write)
                {
                    fileHandle = ExternalCalls.CreateFile(prefixFullname,
                       ExternalCalls.ExtFileAccess.GenericWrite, ExternalCalls.ExtFileShare.Write, IntPtr.Zero,
                       ExternalCalls.ECreationDisposition.CreateAlways, 0, IntPtr.Zero);
                    return new FileStream(fileHandle, fileAccess);
                }
                else
                {
                    fileHandle = ExternalCalls.CreateFile(prefixFullname,
                        ExternalCalls.ExtFileAccess.GenericRead, ExternalCalls.ExtFileShare.Read, IntPtr.Zero,
                        ExternalCalls.ECreationDisposition.CreateAlways, 0, IntPtr.Zero);
                    return new FileStream(fileHandle, fileAccess);
                }
            }
            else if(fileMode == FileMode.OpenOrCreate)
            {
                fileHandle = ExternalCalls.CreateFile(prefixFullname,
                    ExternalCalls.ExtFileAccess.GenericWrite, ExternalCalls.ExtFileShare.Write, IntPtr.Zero,
                    ExternalCalls.ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
                if(fileHandle == null)
                {
                    fileHandle = ExternalCalls.CreateFile(prefixFullname,
                        ExternalCalls.ExtFileAccess.GenericWrite, ExternalCalls.ExtFileShare.Write, IntPtr.Zero,
                        ExternalCalls.ECreationDisposition.New, 0, IntPtr.Zero);
                }
                return new FileStream(fileHandle, fileAccess);
            }
            return null;
        }

        public bool IsReadOnly
        {
            get
            {
                return readOnly;
            }            
        }

        public long Length
        {
            get
            {
                return length;
            }
        }

        public string Name
        {
            get { return name; }
        }        

        public DateTime CreationTime
        {
            get
            {
                return DateTime.Now;
            }
        }

        public string Extension
        {
            get { return extention; }
        }

        public string FullName
        {
            get { return filename; }
        }

        public IFileInfo CopyTo(string destFileName, bool overwrite)
        {
            var prefixFullname = PREFIX + filename;
            var prefixFullnameCopy = PREFIX + destFileName;
            var fileHandleFrom = ExternalCalls.CreateFile(prefixFullname,
                    ExternalCalls.ExtFileAccess.GenericRead, ExternalCalls.ExtFileShare.Read, IntPtr.Zero,
                    ExternalCalls.ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            byte[] buffer;
            using (var fileStream = new FileStream(fileHandleFrom, FileAccess.Read))
            {
                buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
            }
            if (!fileHandleFrom.IsClosed)
                fileHandleFrom.Close();
            var fileHandleTo = ExternalCalls.CreateFile(prefixFullnameCopy,
                   ExternalCalls.ExtFileAccess.GenericWrite, ExternalCalls.ExtFileShare.Write, IntPtr.Zero,
                   ExternalCalls.ECreationDisposition.CreateAlways, 0, IntPtr.Zero);
            using (var fileStream = new FileStream(fileHandleTo, FileAccess.Write))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
            if (!fileHandleTo.IsClosed)
                fileHandleTo.Close();
            IFileInfo ifileInfo = new ExtendedFileInfo(destFileName);
            return ifileInfo;
        }

        public IFileInfo CopyTo(string destFileName)
        {
            var prefixFullname = PREFIX + filename;
            var prefixFullnameCopy = PREFIX + destFileName;
            var fileHandleFrom = ExternalCalls.CreateFile(prefixFullname,
                    ExternalCalls.ExtFileAccess.GenericRead, ExternalCalls.ExtFileShare.Read, IntPtr.Zero,
                    ExternalCalls.ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            byte[] buffer;
            using (var fileStream = new FileStream(fileHandleFrom, FileAccess.Read))
            {
                buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
            }
            if (!fileHandleFrom.IsClosed)
                fileHandleFrom.Close();
            var fileHandleTo = ExternalCalls.CreateFile(prefixFullnameCopy,
                   ExternalCalls.ExtFileAccess.GenericWrite, ExternalCalls.ExtFileShare.Write, IntPtr.Zero,
                   ExternalCalls.ECreationDisposition.CreateAlways, 0, IntPtr.Zero);
            using (var fileStream = new FileStream(fileHandleTo, FileAccess.Write))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
            if (!fileHandleTo.IsClosed)
                fileHandleTo.Close();
            IFileInfo ifileInfo = new ExtendedFileInfo(destFileName);
            return ifileInfo;
        }

        public void MoveTo(string destFileName)
        {
            var prefixFullname = PREFIX + filename;
            var prefixFullnameCopy = PREFIX + destFileName;
            var fileHandleFrom = ExternalCalls.CreateFile(prefixFullname,
                    ExternalCalls.ExtFileAccess.GenericRead, ExternalCalls.ExtFileShare.Read, IntPtr.Zero,
                    ExternalCalls.ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            byte[] buffer;
            using (var fileStream = new FileStream(fileHandleFrom, FileAccess.Read))
            {
                buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
            }
            fileHandleFrom.Close();
            var fileHandleTo = ExternalCalls.CreateFile(prefixFullnameCopy,
                   ExternalCalls.ExtFileAccess.GenericWrite, ExternalCalls.ExtFileShare.Write, IntPtr.Zero,
                   ExternalCalls.ECreationDisposition.CreateAlways, 0, IntPtr.Zero);
            using (var fileStream = new FileStream(fileHandleTo, FileAccess.Write))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
            fileHandleTo.Close();
            ExternalCalls.DeleteFile(prefixFullname);            
        }

        public void Delete()
        {
            var prefixFullname = PREFIX + filename;
            ExternalCalls.DeleteFile(prefixFullname);
        }

        public void SetAttributes(FileAttributes fileAttributes)
        {
            var prefixFullname = PREFIX + filename;
            ExternalCalls.SetFileAttributes(prefixFullname, fileAttributes);
        }

        public FileAttributes GetAttributes()
        {
            var prefixFullname = PREFIX + filename;
            return ExternalCalls.GetFileAttributes(prefixFullname);
        }

        public void Dispose()
        {
            if(fileHandle != null)
            {
                if (!fileHandle.IsClosed)
                    fileHandle.Close();
            }
        }
    }
}
