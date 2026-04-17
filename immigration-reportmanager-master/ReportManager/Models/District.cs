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
    public class District : INotifyPropertyChanged
    {
        private static readonly string IDDistrictProperty = "IDDistrict";
        private static readonly string DisTrictNameProperty = "DisTrictName";
        private static readonly string LineNumberProperty = "LineNumber";
        private int _idDistrict;
        private string _disTrictName;
        private int _lineNumber;

        #region set get method
        public District()
        {
        }
        public District(District dis)
        {
            IDDistrict = dis.IDDistrict;
            DisTrictName = dis.DisTrictName;
        }

        public int IDDistrict
        {
            get
            {
                return _idDistrict;
            }
            set
            {
                this.SetValue(ref _idDistrict, value, IDDistrictProperty);
            }
        }
        public string DisTrictName
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_disTrictName))
                {
                    return MethodHandler.ToTitleCase(_disTrictName.Trim());
                }
                return "";
            }
            set
            {
                this.SetValue(ref _disTrictName, value, DisTrictNameProperty);
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
