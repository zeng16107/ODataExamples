//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ODataExamples.Repository.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductOverview
    {
        public int id { get; set; }
        public Nullable<int> product_id { get; set; }
        public string overview_description { get; set; }
        public string inserted_by { get; set; }
        public System.DateTime inserted_datetime { get; set; }
        public string last_updated_by { get; set; }
        public System.DateTime last_updated_datetime { get; set; }
    
        public virtual Product Product { get; set; }
    }
}