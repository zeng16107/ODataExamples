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
    
    public partial class Order
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Order()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
        }
    
        public int id { get; set; }
        public int customer_id { get; set; }
        public string order_number { get; set; }
        public int order_status { get; set; }
        public string inserted_by { private get; set; }
        public System.DateTime inserted_datetime { private get; set; }
        public string last_updated_by { private get; set; }
        public System.DateTime last_updated_datetime { private get; set; }
    
        public virtual Customer Customer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
