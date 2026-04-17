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
    public class Email : INotifyPropertyChanged
    {
        private static readonly string IDEmailProperty = "IDEmail";
        private static readonly string NameProperty = "Name";
        private static readonly string MailProperty = "Mail";
        private static readonly string IDCompanyProperty = "IDCompany";
        private int _idEmail;
        private string _name;
        private string _mail;
        private int _idCompany;

        #region set get method
        public Email()
        {

        }
        public Email(Email email)
        {
            IDEmail = email.IDEmail;
            Name = email.Name;
            Mail = email.Mail;
            IDCompany = email.IDCompany;
        }
        public int IDEmail
        {
            get
            {
                return _idEmail;
            }
            set
            {
                this.SetValue(ref _idEmail, value, IDEmailProperty);
                OnPropertyChanged("IDEmail");
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
        public string Mail
        {
            get
            {
                return _mail;
            }
            set
            {
                this.SetValue(ref _mail, value, MailProperty);
                OnPropertyChanged("Mail");
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
            str += Mail;
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
