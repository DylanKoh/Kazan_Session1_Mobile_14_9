using System;

namespace Kazan_Session1_Mobile_14_9
{
    public class GlobalClass
    {
        public class CustomList
        {
            public int AssetID { get; set; }
            public string AssetName { get; set; }
            public string DepartmentName { get; set; }
            public string AssetSN { get; set; }
            public DateTime? WarrantyDate { get; set; }
            public string AssetGroup { get; set; }
        }
        public partial class Department
        {
            public long ID { get; set; }
            public string Name { get; set; }
        }
        public partial class AssetGroup
        {
            public long ID { get; set; }
            public string Name { get; set; }
        }

    }
}
