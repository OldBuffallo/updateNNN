using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager.Models
{
    public class Field : INotifyPropertyChanged
    {
        private static readonly string IDFieldProperty = "IDField";
        private static readonly string FieldNameProperty = "FieldName";
        private static readonly string DescriptionProperty = "Description";
        private int _idField;
        private string _fieldName;
        private string _description;

        #region set get method
        public int IDField
        {
            get
            {
                return _idField;
            }
            set
            {
                this.SetValue(ref _idField, value, IDFieldProperty);
                OnPropertyChanged("IDField");
            }
        }
        public string FieldName
        {
            get
            {
                return _fieldName;
            }
            set
            {
                this.SetValue(ref _fieldName, value, FieldNameProperty);
                OnPropertyChanged("FieldName");
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                this.SetValue(ref _description, value, DescriptionProperty);
                OnPropertyChanged("Description");
            }
        }
        #endregion

        #region Protected Methods
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool SetValue<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
