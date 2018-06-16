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
    
    public partial class OrderDetail
    {
        public int id { get; set; }
        public int order_id { get; set; }
        public int product_id { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public string inserted_by { private get; set; }
        public System.DateTime inserted_datetime { private get; set; }
        public string last_updated_by { private get; set; }
        public System.DateTime last_updated_datetime { private get; set; }
    
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
