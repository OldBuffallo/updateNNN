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
    public class Ward : INotifyPropertyChanged
    {
        private static readonly string IDWardProperty = "IDWard";
        private static readonly string WardNameProperty = "WardName";
        private static readonly string LineNumberProperty = "LineNumber";
        private int _idWard;
        private string _wardName;
        private int _lineNumber;

        #region set get method
        public Ward()
        {
        }
        public Ward(Ward dis)
        {
            IDWard = dis.IDWard;
            WardName = dis.WardName;
        }

        public int IDWard
        {
            get
            {
                return _idWard;
            }
            set
            {
                this.SetValue(ref _idWard, value, IDWardProperty);
            }
        }
        public string WardName
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_wardName))
                {
                    return MethodHandler.ToTitleCase(_wardName.Trim());
                }
                return "";
            }
            set
            {
                this.SetValue(ref _wardName, value, WardNameProperty);
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
            }
        }
        #endregion

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
