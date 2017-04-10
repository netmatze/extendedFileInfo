
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtendedFileInfos
{
    public interface IFileInfo
    {
        bool Exists();
        bool IsReadOnly { get; }        
        long Length { get; }        
        string Name { get; }
        FileStream Open(FileMode fileMode, FileAccess fileAccess);
        IFileInfo CopyTo(string destFileName);        
        IFileInfo CopyTo(string destFileName, bool overwrite);
        void MoveTo(string destFileName);
        void Delete();
        void SetAttributes(FileAttributes fileAttributes);
        FileAttributes GetAttributes();
        DateTime CreationTime { get; }                
        string Extension { get; }
        string FullName { get; }
    }
}
