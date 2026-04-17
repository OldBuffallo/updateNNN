using ReportManager.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager.Models
{
    public class PhoneNumber : INotifyPropertyChanged
    {
        private static readonly string IDPhoneNumberProperty = "IDPhoneNumber";
        private static readonly string NameProperty = "Name";
        private static readonly string PhoneProperty = "Phone";
        private static readonly string IDCompanyProperty = "IDCompany";
        private int _idPhoneNumber;
        private string _name;
        private string _phone;
        private int _idCompany;

        #region set get method
        public PhoneNumber()
        {

        }
        public PhoneNumber(PhoneNumber phoneNumber)
        {
            IDPhoneNumber = phoneNumber.IDPhoneNumber;
            Name = phoneNumber.Name;
            Phone = phoneNumber.Phone;
            IDCompany = phoneNumber.IDCompany;
        }
        public int IDPhoneNumber
        {
            get
            {
                return _idPhoneNumber;
            }
            set
            {
                this.SetValue(ref _idPhoneNumber, value, IDPhoneNumberProperty);
                OnPropertyChanged("IDPhoneNumber");
            }
        }
        public string Name
        {
            get
            {
                return MethodHandler.ToTitleCase(_name);
            }
            set
            {
                this.SetValue(ref _name, value, NameProperty);
                OnPropertyChanged("Name");
            }
        }
        public string Phone
        {
            get
            {
                return _phone;
            }
            set
            {
                this.SetValue(ref _phone, value, PhoneProperty);
                OnPropertyChanged("Phone");
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
        #endregion

        override
        public string ToString()
        {
            string str = "";
            if (!string.IsNullOrEmpty(Name))
            {
                str += Name + ": ";
            }
            str += Phone;
            return str.Trim();
        }

        #region protected method
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
