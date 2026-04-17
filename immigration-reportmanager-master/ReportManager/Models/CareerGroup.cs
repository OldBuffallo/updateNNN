using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager.Models
{
    class CareerGroup : INotifyPropertyChanged
    {
        private static readonly string IDCGProperty = "IDCG";
        private static readonly string CareerGroupNameProperty = "CareerGroupName";
        private int _idCG;
        private string _careerGroupName;

        #region set get method
        public int IDCG
        {
            get
            {
                return _idCG;
            }
            set
            {
                this.SetValue(ref _idCG, value, IDCGProperty);
                OnPropertyChanged("IDCG");
            }
        }
        public string CareerGroupName
        {
            get
            {
                return _careerGroupName;
            }
            set
            {
                this.SetValue(ref _careerGroupName, value, CareerGroupNameProperty);
                OnPropertyChanged("CareerGroupName");
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
