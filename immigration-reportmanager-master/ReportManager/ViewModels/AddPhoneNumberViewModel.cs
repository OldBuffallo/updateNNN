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
    class AddPhoneNumberViewModel : INotifyPropertyChanged
    {
        public static readonly string AddPhoneNumberProperty = "AddPhoneNumber";
        public static readonly string ListPhoneNumbersProperty = "ListPhoneNumbers";
        private PhoneNumber _addPhoneNumber;
        private PhoneNumber _oldPhoneNumber;
        private bool _isEdit;
        private ObservableCollection<PhoneNumber> _listPhoneNumbers;
        private ObservableCollection<PhoneNumber> _listUpdatePhoneNumbers;
        private PhoneNumberWindow _window;
        private ICommand _btnAddPhoneNumberCommand;
        private ICommand _btnDeletePhoneNumberCommand;
        private ICommand _btnExitCommand;

        #region set get method
        public AddPhoneNumberViewModel(PhoneNumberWindow window, ObservableCollection<PhoneNumber> listPhoneNumbers, ObservableCollection<PhoneNumber> listUpdatePhoneNumbers = null)
        {
            _window = window;
            ListPhoneNumbers = listPhoneNumbers;
            _listUpdatePhoneNumbers = listUpdatePhoneNumbers;
            _addPhoneNumber = new PhoneNumber();
            _isEdit = false;
        }
        public AddPhoneNumberViewModel(PhoneNumberWindow window, PhoneNumber selectPhone, ObservableCollection<PhoneNumber> listPhoneNumbers, ObservableCollection<PhoneNumber> listUpdatePhoneNumbers = null)
        {
            _window = window;
            ListPhoneNumbers = listPhoneNumbers;
            _listUpdatePhoneNumbers = listUpdatePhoneNumbers;
            _addPhoneNumber = new PhoneNumber(selectPhone);
            _oldPhoneNumber = selectPhone;
            _isEdit = true;
        }
        public PhoneNumber AddPhoneNumber
        {
            get
            {
                return _addPhoneNumber;
            }
            set
            {
                this.SetValue(ref _addPhoneNumber, value, AddPhoneNumberProperty);
                OnPropertyChanged("AddPhoneNumber");
            }
        }
        public ObservableCollection<PhoneNumber> ListPhoneNumbers
        {
            get
            {
                return _listPhoneNumbers;
            }
            set
            {
                this.SetValue(ref _listPhoneNumbers, value, ListPhoneNumbersProperty);
                OnPropertyChanged("ListPhoneNumbers");
            }
        }
        #endregion

        #region method ICommand
        public ICommand BtnAddPhoneNumberCommand
        {
            get
            {
                return _btnAddPhoneNumberCommand ?? (_btnAddPhoneNumberCommand = new CommandHandler(BtnPhoneNumber, true));
            }
        }
        private void BtnPhoneNumber()
        {
            if (string.IsNullOrEmpty(AddPhoneNumber.Phone))
            {
                MessageBox.Show("không được để trống sô điện thoại");
                return;
            }
            if (string.IsNullOrEmpty(AddPhoneNumber.Name))
            {
                MessageBox.Show("không được để trống tên");
                return;
            }
            bool removeListDisplay = false;
            bool removeListUpdate = false;
            if (_isEdit)
            {
                // check change
                if (_oldPhoneNumber.Phone.Equals(AddPhoneNumber.Phone) && _oldPhoneNumber.Name.Equals(AddPhoneNumber.Name))
                {
                    MessageBox.Show("Không thay đổi thông tin điện thoại");
                    return;
                }
                // remove info list display and list update
                removeListDisplay = ListPhoneNumbers.Remove(_oldPhoneNumber);
                removeListUpdate = _listUpdatePhoneNumbers.Remove(_oldPhoneNumber);
            }
            // check exist phone number in list
            int i = 0;
            for(; i < ListPhoneNumbers.Count; i++)
            {
                if(ListPhoneNumbers[i].Name.ToUpper().Equals(AddPhoneNumber.Name.ToUpper())
                    && ListPhoneNumbers[i].Phone.ToUpper().Equals(AddPhoneNumber.Phone.ToUpper()))
                {
                    MessageBox.Show("Đã tồn tại thông tin số điện thoại này \n vui lòng kiểm tra lại");
                    break;
                }
            }
            if(i == ListPhoneNumbers.Count)
            {
                // phone number is not yet
                ListPhoneNumbers.Add(AddPhoneNumber);
                // current screen is edit info
                if(_listUpdatePhoneNumbers != null)
                {
                    if(AddPhoneNumber.IDPhoneNumber != 0)
                    {
                        _listUpdatePhoneNumbers.Add(_oldPhoneNumber);
                        AddPhoneNumber.IDPhoneNumber = 0;
                    }
                    _listUpdatePhoneNumbers.Add(AddPhoneNumber);
                }
            }
            else
            {
                // restore data list
                if (removeListDisplay)
                {
                    ListPhoneNumbers.Add(_oldPhoneNumber);
                }
                if (removeListUpdate)
                {
                    _listUpdatePhoneNumbers.Add(_oldPhoneNumber);
                }
            }
            BtnExitCommand.Execute(true);
        }

        public ICommand BtnDeletePhoneNumberCommand
        {
            get
            {
                return _btnDeletePhoneNumberCommand ?? (_btnDeletePhoneNumberCommand = new CommandHandler(DeletePhoneNumber, true));
            }
        }
        private void DeletePhoneNumber()
        {
            if (_oldPhoneNumber != null)
            {
                if (_listUpdatePhoneNumbers != null)
                {
                    int i = 0;
                    for (; i < _listUpdatePhoneNumbers.Count; i++)
                    {
                        if (_listUpdatePhoneNumbers[i].Name.ToUpper().Equals(_oldPhoneNumber.Name.ToUpper()) && _listUpdatePhoneNumbers[i].Phone.ToUpper().Equals(_oldPhoneNumber.Phone.ToUpper()) && _listUpdatePhoneNumbers[i].IDPhoneNumber == _oldPhoneNumber.IDPhoneNumber)
                        {
                            break;
                        }
                    }
                    if (i == _listUpdatePhoneNumbers.Count && _oldPhoneNumber.IDPhoneNumber != 0)
                    {
                        _listUpdatePhoneNumbers.Add(_oldPhoneNumber);
                    }
                    else if (i < _listUpdatePhoneNumbers.Count && _oldPhoneNumber.IDPhoneNumber == 0)
                    {
                        _listUpdatePhoneNumbers.Remove(_oldPhoneNumber);
                    }
                }
                ListPhoneNumbers.Remove(_oldPhoneNumber);
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