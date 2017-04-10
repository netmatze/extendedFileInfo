using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtendedFileInfos
{
    public class FileInfoFacade : IFileInfo
    {
        private string filename;
        private FileInfo fileInfo;

        public FileInfoFacade(string filename)
        {
            this.filename = filename;
            this.fileInfo = new FileInfo(filename);
        }

        public FileInfoFacade(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            this.filename = fileInfo.FullName;
        }

        public bool Exists()
        {
            return fileInfo.Exists;
        }

        public FileStream Open(FileMode fileMode, FileAccess fileAccess)
        {
            return fileInfo.Open(fileMode, fileAccess);
        }

        public bool IsReadOnly
        {
            get
            {
                return fileInfo.IsReadOnly;
            }            
        }

        public long Length
        {
            get { return fileInfo.Length; }
        }

        public string Name
        {
            get { return fileInfo.Name; }
        }

        public DateTime CreationTime
        {
            get
            {
                return fileInfo.CreationTime;
            }
        }

        public string Extension
        {
            get { return fileInfo.Extension; }
        }

        public string FullName
        {
            get { return fileInfo.FullName; }
        }

        public IFileInfo CopyTo(string destFileName)
        {
            var fileInfoCopy = fileInfo.CopyTo(destFileName);
            IFileInfo ifileInfoCopy = new FileInfoFacade(fileInfoCopy);
            return ifileInfoCopy;
        }

        public IFileInfo CopyTo(string destFileName, bool overwrite)
        {
            var fileInfoCopy = fileInfo.CopyTo(destFileName, overwrite);
            IFileInfo ifileInfoCopy = new FileInfoFacade(fileInfoCopy);
            return ifileInfoCopy;
        }

        public void MoveTo(string destFileName)
        {
            fileInfo.MoveTo(destFileName);
        }

        public void Delete()
        {
            fileInfo.Delete();
        }

        public void SetAttributes(FileAttributes fileAttributes)
        {
            File.SetAttributes(fileInfo.FullName, fileAttributes);
        }

        public FileAttributes GetAttributes()
        {
            return File.GetAttributes(fileInfo.FullName);
        }
    }
}
