//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplicationFinal
{
    using System;
    using System.Collections.Generic;
    
    public partial class file
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public file()
        {
            this.user_share_file = new HashSet<user_share_file>();
            this.user = new HashSet<user>();
        }
        public int id { get; set; }
        public Nullable<int> type { get; set; }
        public string url { get; set; }
        public Nullable<System.DateTime> time { get; set; }
        public Nullable<int> download_times { get; set; }
        public Nullable<int> cost { get; set; }
        public Nullable<double> size { get; set; }
        public string name { get; set; }
        public Nullable<int> permission { get; set; }
        public Nullable<int> status { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<user_share_file> user_share_file { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<user> user { get; set; }
    }
}
