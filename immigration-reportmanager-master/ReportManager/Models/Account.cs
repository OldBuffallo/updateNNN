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
    public class Account : INotifyPropertyChanged
    {
        private static readonly string IDUserProperty = "IDUser";
        private static readonly string UsernameProperty = "Username";
        private static readonly string NameProperty = "Name";
        private static readonly string PasswordProperty = "Password";
        private static readonly string PermissionProperty = "Permission";
        private static readonly string PermissionStringProperty = "PermissionString";
        private static readonly string PermissionIndexProperty = "PermissionIndex";
        private int _idUser;
        private string _username;
        private string _name;
        private string _password;
        private int _permission;
        private string _permissionString;
        private int _permissionIndex;

        public Account()
        {

        }
        public Account(int idUser, string username, string name, string password = "", int permission = 0)
        {
            IDUser = idUser;
            Username = username;
            Name = name;
            Password = password;
            Permission = permission;
        }
        public int IDUser
        {
            get
            {
                return _idUser;
            }
            set
            {
                this.SetValue(ref _idUser, value, IDUserProperty);
                OnPropertyChanged("IDUser");
            }
        }
        public string Username
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_username))
                {
                    return _username.Trim();
                }
                return "";
            }
            set
            {
                this.SetValue(ref _username, value, UsernameProperty);
                OnPropertyChanged("Username");
            }
        }
        public string Name
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_name))
                {
                    return MethodHandler.ToTitleCase(_name);
                }
                return "";
            }
            set
            {
                this.SetValue(ref _name, value, NameProperty);
                OnPropertyChanged("Name");
            }
        }
        public string Password
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_password))
                {
                    return _password.Trim();
                }
                return "";
            }
            set
            {
                this.SetValue(ref _password, value, PasswordProperty);
                OnPropertyChanged("Password");
            }
        }
        public int Permission
        {
            get
            {
                return _permission;
            }
            set
            {
                this.SetValue(ref _permission, value, PermissionProperty);
                OnPropertyChanged("Permission");
            }
        }
        public string PermissionString
        {
            get
            {
                switch (Permission)
                {
                    case 1:
                        return "Admin";
                    default:
                        return "User";
                }
            }
            set
            {
                this.SetValue(ref _permissionString, value, PermissionStringProperty);
                OnPropertyChanged("PermissionString");
            }
        }
        public int PermissionIndex
        {
            get
            {
                return (Permission > 0) ? (Permission - 1) : 0;
            }
            set
            {
                Permission = value + 1;
                this.SetValue(ref _permissionIndex, value, PermissionIndexProperty);
                OnPropertyChanged("PermissionIndex");
            }
        }

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
    }
}
