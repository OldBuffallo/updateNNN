using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager.Models
{
    public class Attach : INotifyPropertyChanged
    {
        private static readonly string IDAttachProperty = "IDAttach";
        private static readonly string IDCompanyProperty = "IDCompany";
        private static readonly string TypeProperty = "Type";
        private static readonly string NameProperty = "Name";
        private static readonly string FolderProperty = "Folder";
        private static readonly string DateCreatedProperty = "DateCreated";
        private static readonly string DateModifiedProperty = "DateModified";
        private static readonly string LineNumberProperty = "LineNumber";
        private int _idAttach;
        private int _idCompany;
        private int _type;
        private string _name;
        private string _folder;
        private string _dateCreated;
        private string _dateModified;
        private int _lineNumber;

        #region set get method
        public Attach()
        {

        }
        public Attach(Attach attach)
        {
            _idAttach= attach._idAttach;
            _idCompany= attach._idCompany;
            _type= attach._type;
            _name= attach._name;
            _folder= attach._folder;
            _dateCreated= attach._dateCreated;
            _dateModified= attach._dateModified;
        }
        public int IDAttach
        {
            get
            {
                return _idAttach;
            }
            set
            {
                this.SetValue(ref _idAttach, value, IDAttachProperty);
                OnPropertyChanged("IDAttach");
            }
        }
        public int IDCompany
        {
            get
            {
                return _idCompany;
            }
            set
            {
                this.SetValue(ref _idCompany, value, IDCompanyProperty);
                OnPropertyChanged("IDCompany");
            }
        }
        public int Type
        {
            get
            {
                return _type;
            }
            set
            {
                this.SetValue(ref _type, value, TypeProperty);
                OnPropertyChanged("Type");
            }
        }
        public string TypeString
        {
            get
            {
                switch(Type)
                {
                    case 0:
                        return "BC_NNN";
                    case 1:
                        return "HSPN";
                    default:
                        return "";
                }
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                this.SetValue(ref _name, value, NameProperty);
                OnPropertyChanged("Name");
            }
        }
        public string Folder
        {
            get
            {
                return _folder;
            }
            set
            {
                this.SetValue(ref _folder, value, FolderProperty);
                OnPropertyChanged("Folder");
            }
        }
        public string DateCreated
        {
            get
            {
                return _dateCreated;
            }
            set
            {
                this.SetValue(ref _dateCreated, value, DateCreatedProperty);
                OnPropertyChanged("DateCreated");
            }
        }
        public string DateModified
        {
            get
            {
                return _dateModified;
            }
            set
            {
                this.SetValue(ref _dateModified, value, DateModifiedProperty);
                OnPropertyChanged("DateModified");
            }
        }
        public int LineNumber
        {
            get
            {
                return _lineNumber;
            }
            set
            {
                this.SetValue(ref _lineNumber, value, LineNumberProperty);
                OnPropertyChanged("LineNumber");
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
