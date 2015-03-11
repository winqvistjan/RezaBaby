using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezaBaby
{
    /// <summary>
    /// Class to be used for holding the file-blobname mapping.
    /// </summary>
    public class FileBlobNameMapper
    {
        public FileBlobNameMapper(string blobName, string filePath)
        {
            BlobName = blobName;
            FilePath = filePath;
        }

        public string BlobName { get; set; }

        public string FilePath { get; set; }
    }
}