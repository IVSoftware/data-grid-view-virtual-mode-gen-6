using System;

namespace data_grid_view_virtual_mode
{
    internal class MyClass
    {
        public MyClass() { }
        public MyClass(string description) 
        {
            Description = description;
        }
        public string Description { get; set; }
        public string ID => _id;
        string _id
            = Guid
            .NewGuid()
            .ToString()
            .Trim(new char[] { '{', '}' });
        public bool CheckMe { get; set; }
    }
}