//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RezaBaby
{
    using System;
    using System.Collections.Generic;
    
    public partial class AlbumMedia
    {
        public int ID { get; set; }
        public int AlbumId { get; set; }
        public string FileName { get; set; }
        public string URL { get; set; }
        public string MimeType { get; set; }
        public string Orientation { get; set; }
    
        public virtual Album Album { get; set; }
    }
}