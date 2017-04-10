using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendedFileInfos;
using System.IO;

namespace ExtendedFileInfoTests
{
    [TestClass]
    public class ExtendedFileInfoTest
    {
        [TestMethod]
        public void ExtendedFileInfoTestShortFilePath()
        {
            var shortFilePath = @"C:\FilePath\ShortPath\Textdocument.txt";
            var extendedFileInfo = FileInfoFactory.Create(shortFilePath);
            using(var fileStream = extendedFileInfo.Open(FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
            }
        }

        [TestMethod]
        public void ExtendedFileInfoTestLongFilePath()
        {
            var longFilePath =
                @"C:\FilePath\LooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooPath\Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog\Textdocument.txt";
            var extendedFileInfo = FileInfoFactory.Create(longFilePath);
            using (var fileStream = extendedFileInfo.Open(FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
            }
        }
    }
}
