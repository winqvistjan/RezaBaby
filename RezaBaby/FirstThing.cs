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
    
    public partial class FirstThing
    {
        public FirstThing()
        {
            this.FirstThingDetails = new HashSet<FirstThingDetail>();
        }
    
        public int ID { get; set; }
        public string What { get; set; }
        public System.DateTime When { get; set; }
        public string Where { get; set; }
    
        public virtual ICollection<FirstThingDetail> FirstThingDetails { get; set; }
    }
}
