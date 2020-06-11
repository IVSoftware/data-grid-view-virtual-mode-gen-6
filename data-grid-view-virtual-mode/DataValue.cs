using System;
using System.ComponentModel;

namespace data_grid_view_virtual_mode
{
    internal class DataValue
    {
        public DataValue() { }
        public DataValue(string description) 
        {
            Description = description;
        }
        public string Description 
        {
            get => _description;
            set
            {
                if(value != _description)
                {
                    _description = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Description)));
                }
            }
        }
        string _description = null;
        public string ID => _id;
        
        public bool CheckMe
        {
            get => _checkMe;
            set
            {
                if(value != _checkMe)
                {
                    _checkMe = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(CheckMe)));
                }
            }
        }
        bool _checkMe = false;

        string _id
            = Guid
            .NewGuid()
            .ToString()
            .Trim(new char[] { '{', '}' });

        public static event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}