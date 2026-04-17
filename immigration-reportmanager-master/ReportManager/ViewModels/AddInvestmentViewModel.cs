using ReportManager.Bases;
using ReportManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReportManager.ViewModels
{
    class AddInvestmentViewModel : INotifyPropertyChanged
    {
        public static readonly string AddInvestmentProperty = "AddInvestment";
        public static readonly string ListInvestmentsProperty = "ListInvestment";
        public static readonly string ListnationalitiesProperty = "Listnationalities";
        public static readonly string InputNationalityProperty = "InputNationality";
        private Investment _addInvestment;
        private Investment _oldInvestment;
        private Nationality _inputNationality;
        private bool _isEdit;
        private ObservableCollection<Investment> _listInvestments;
        private ObservableCollection<Investment> _listUpdateInvestments;
        private ObservableCollection<Nationality> _listnationalities;
        private InvestmentWindow _window;
        private ICommand _btnAddInvestmentCommand;
        private ICommand _btnDeleteInvestmentCommand;
        private ICommand _btnExitCommand;

        #region set get method
        public AddInvestmentViewModel(InvestmentWindow window, Investment selectInvestment, ObservableCollection<Investment> listInvestments, ObservableCollection<Investment> listUpdateInvestments = null)
        {
            _window = window;
            ListInvestments = listInvestments;
            _listUpdateInvestments = listUpdateInvestments;
            _addInvestment = new Investment(selectInvestment);
            _oldInvestment = selectInvestment;
            _listnationalities = MethodHandler.getNationalities();
            _listnationalities.Insert(0, new Nationality(0, "", ""));
            if (_listnationalities != null && _listnationalities.Count > 0)
            {
                foreach(Nationality national in _listnationalities)
                {
                    if (national.NationalityName == AddInvestment.Nationality)
                    {
                        InputNationality = national;
                        break;
                    }
                }
            }
            _isEdit = true;
        }
        public Investment AddInvestment
        {
            get
            {
                return _addInvestment;
            }
            set
            {
                this.SetValue(ref _addInvestment, value, AddInvestmentProperty);
            }
        }
        public ObservableCollection<Investment> ListInvestments
        {
            get
            {
                return _listInvestments;
            }
            set
            {
                this.SetValue(ref _listInvestments, value, ListInvestmentsProperty);
            }
        }
        public ObservableCollection<Nationality> Listnationalities
        {
            get
            {
                return _listnationalities;
            }
            set
            {
                this.SetValue(ref _listnationalities, value, ListnationalitiesProperty);
            }
        }
        public Nationality InputNationality
        {
            get
            {
                return _inputNationality;
            }
            set
            {
                this.SetValue(ref _inputNationality, value, InputNationalityProperty);
                AddInvestment.Nationality = _inputNationality.NationalityName;
            }
        }
        #endregion

        #region method ICommand
        public ICommand BtnAddInvestmentCommand
        {
            get
            {
                return _btnAddInvestmentCommand ?? (_btnAddInvestmentCommand = new CommandHandler(BtnAddInvestment, true));
            }
        }
        private void BtnAddInvestment()
        {
            if (string.IsNullOrWhiteSpace(AddInvestment.Name))
            {
                MessageBox.Show("không cho phép trống chủ sở hữu");
                return;
            }
            if (string.IsNullOrWhiteSpace(AddInvestment.Nationality))
            {
                MessageBox.Show("chọn một quốc tịch");
                return;
            }
            if (string.IsNullOrWhiteSpace(AddInvestment.Passport))
            {
                MessageBox.Show("không cho phép trống số hộ chiếu");
                return;
            }
            if (!(AddInvestment.AmountOfMoney > 0))
            {
                MessageBox.Show("vui lòng nhập vốn đầu tư");
                AddInvestment.AmountOfMoney = 0;
                return;
            }
            bool removeListDisplay = false;
            bool removeListUpdate = false;
            if (_isEdit)
            {
                // check change
                if (_oldInvestment.Name.Equals(AddInvestment.Name) && _oldInvestment.Nationality.Equals(AddInvestment.Nationality) && _oldInvestment.Passport.Equals(AddInvestment.Passport) && _oldInvestment.AmountOfMoney == AddInvestment.AmountOfMoney)
                {
                    MessageBox.Show("Không thay đổi thông tin email");
                    return;
                }
                // remove info list display and list update
                removeListDisplay = ListInvestments.Remove(_oldInvestment);
                removeListUpdate = _listUpdateInvestments.Remove(_oldInvestment);
            }
            // check exist invest in list
            int i = 0;
            for (; i < ListInvestments.Count; i++)
            {
                if (ListInvestments[i].Name.ToUpper().Equals(AddInvestment.Name.ToUpper())
                    && ListInvestments[i].Nationality.ToUpper().Equals(AddInvestment.Nationality.ToUpper())
                    && ListInvestments[i].Passport.ToUpper().Equals(AddInvestment.Passport.ToUpper())
                    && ListInvestments[i].AmountOfMoney == AddInvestment.AmountOfMoney)
                {
                    MessageBox.Show("vốn đầu tư đã tồn tại \n vui lòng kiểm tra lại");
                    break;
                }
            }
            if (i == ListInvestments.Count)
            {
                // invest is not yet
                ListInvestments.Add(AddInvestment);
                
                if (_listUpdateInvestments != null)
                {
                    // current screen is edit
                    // delete old info and add new info
                    if(AddInvestment.IDInvestment != 0)
                    {
                        _listUpdateInvestments.Add(_oldInvestment);
                        AddInvestment.IDInvestment = 0;
                    }
                    _listUpdateInvestments.Add(AddInvestment);
                }
            }
            else
            {
                // restore data list
                if (removeListDisplay)
                {
                    ListInvestments.Add(_oldInvestment);
                }
                if (removeListUpdate)
                {
                    _listUpdateInvestments.Add(_oldInvestment);
                }
            }
            BtnExitCommand.Execute(true);
        }

        public ICommand BtnDeleteInvestmentCommand
        {
            get
            {
                return _btnDeleteInvestmentCommand ?? (_btnDeleteInvestmentCommand = new CommandHandler(DeleteInvestment, true));
            }
        }
        private void DeleteInvestment()
        {
            if (_oldInvestment != null)
            {
                if (_listUpdateInvestments != null)
                {
                    int i = 0;
                    for (; i < _listUpdateInvestments.Count; i++)
                    {
                        if (_listUpdateInvestments[i].Name.ToUpper().Equals(_oldInvestment.Name.ToUpper()) && _listUpdateInvestments[i].Nationality.ToUpper().Equals(_oldInvestment.Nationality.ToUpper())
                            && _listUpdateInvestments[i].Passport.ToUpper().Equals(_oldInvestment.Passport.ToUpper())
                            && _listUpdateInvestments[i].AmountOfMoney == _oldInvestment.AmountOfMoney && _listUpdateInvestments[i].IDInvestment == _oldInvestment.IDInvestment)
                        {
                            break;
                        }
                    }
                    if (i == _listUpdateInvestments.Count && _oldInvestment.IDInvestment != 0)
                    {
                        _listUpdateInvestments.Add(_oldInvestment);
                    }
                    else if (i < _listUpdateInvestments.Count && _oldInvestment.IDInvestment == 0)
                    {
                        _listUpdateInvestments.Remove(_oldInvestment);
                    }
                }
                ListInvestments.Remove(_oldInvestment);
            }
            BtnExitCommand.Execute(true);
        }

        public ICommand BtnExitCommand
        {
            get
            {
                return _btnExitCommand ?? (_btnExitCommand = new CommandHandler(Exit, true));
            }
        }
        private void Exit()
        {
            _window.Close();
        }
        #endregion

        #region Protected Methods
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
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
