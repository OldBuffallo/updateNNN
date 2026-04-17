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
    public class Investment : INotifyPropertyChanged
    {
        private static readonly string IDInvestmentProperty = "IDInvestment";
        private static readonly string NameProperty = "Name";
        private static readonly string NationalityProperty = "Nationality";
        private static readonly string AmountOfMoneyProperty = "AmountOfMoney";
        private static readonly string IDCompanyProperty = "IDCompany";
        private static readonly string PassportProperty = "Passport";
        private int _idInvestment;
        private string _name;
        private string _nataionality;
        private decimal _amountOfMoney;
        private int _idCompany;
        private string _passport;

        #region set get method
        public Investment(){}
        public Investment(Investment investment)
        {
            _idInvestment = investment.IDInvestment;
            _name = investment.Name;
            _nataionality = investment.Nationality;
            _amountOfMoney = investment.AmountOfMoney;
            _idCompany = investment.IDCompany;
            _passport = investment.Passport;
        }
        public int IDInvestment
        {
            get
            {
                return _idInvestment;
            }
            set
            {
                this.SetValue(ref _idInvestment, value, IDInvestmentProperty);
                OnPropertyChanged("IDInvestment");
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
        public string Nationality
        {
            get
            {
                return _nataionality;
            }
            set
            {
                this.SetValue(ref _nataionality, value, NationalityProperty);
                OnPropertyChanged("Nationality");
            }
        }
        public decimal AmountOfMoney
        {
            get
            {
                return _amountOfMoney;
            }
            set
            {
                this.SetValue(ref _amountOfMoney, value, AmountOfMoneyProperty);
                OnPropertyChanged("AmountOfMoney");
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

        public string Passport
        {
            get
            {
                return _passport;
            }
            set
            {
                this.SetValue(ref _passport, value, PassportProperty);
                OnPropertyChanged("Passport");
            }
        }
        #endregion

        override
        public string ToString()
        {
            string str = "";
            if (!string.IsNullOrEmpty(Name))
            {
                str += Name + "\n";
            }
            str += Nationality + " : " + MoneyConvert.ConvertMoneyToString(AmountOfMoney + "");
            return str.Trim();
        }

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
