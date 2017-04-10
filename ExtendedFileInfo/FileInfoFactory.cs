using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtendedFileInfos
{
    public static class FileInfoFactory
    {
        public static IFileInfo Create(string filename)
        {
            if(filename.Length < 248)
            {
                return new FileInfoFacade(filename);
            }
            else
            {
                return new ExtendedFileInfo(filename);
            }
        }
    }
}
