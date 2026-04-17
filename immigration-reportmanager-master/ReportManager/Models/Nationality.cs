using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager.Models
{
    public class Nationality : INotifyPropertyChanged
    {
        private static readonly string IDNationalityProperty = "IDNationality";
        private static readonly string NationalityCodeProperty = "NationalityCode";
        private static readonly string NationalityNameProperty = "NationalityName";
        private static readonly string LineNumberProperty = "LineNumber";
        private int _idNationality;
        private string _nationalityCode;
        private string _nationalityName;
        private int _lineNumber;

        #region set get method
        public Nationality() { }
        public Nationality(int id, string code, string name)
        {
            _idNationality = id;
            _nationalityCode = code;
            _nationalityName = name;
        }
        public Nationality(Nationality nationality)
        {
            _idNationality = nationality.IDNationality;
            _nationalityCode = nationality.NationalityCode;
            _nationalityName = nationality.NationalityName;
            _lineNumber = nationality.LineNumber;
        }
        public int IDNationality
        {
            get
            {
                return _idNationality;
            }
            set
            {
                this.SetValue(ref _idNationality, value, IDNationalityProperty);
            }
        }
        public string NationalityCode
        {
            get
            {
                return _nationalityCode;
            }
            set
            {
                this.SetValue(ref _nationalityCode, value, NationalityCodeProperty);
            }
        }
        public string NationalityName
        {
            get
            {
                return _nationalityName;
            }
            set
            {
                this.SetValue(ref _nationalityName, value, NationalityNameProperty);
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
