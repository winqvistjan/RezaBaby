using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezaBaby
{
    public class BlobOperationStatus
    {
        public string Name { get; set; }

        public Uri BlobUri { get; set; }

        public OperationStatus OperationStatus { get; set; }

        public Exception ExceptionDetails { get; set; }
    }

    public enum OperationStatus
    {
        Failed, Succeded
    }
}