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
    class AddEmailViewModel : INotifyPropertyChanged
    {
        public static readonly string AddEmailProperty = "AddEmail";
        public static readonly string ListEmailsProperty = "ListEmails";
        private Email _addEmail;
        private Email _oldEmail;
        private bool _isEdit;
        private ObservableCollection<Email> _listEmails;
        private ObservableCollection<Email> _listUpdateEmails;
        private AddEmailWindow _window;
        private ICommand _btnAddEmailCommand;
        private ICommand _btnDeleteEmailCommand;
        private ICommand _btnExitCommand;

        #region set get method
        public AddEmailViewModel(AddEmailWindow window, ObservableCollection<Email> listEmails, ObservableCollection<Email> listUpdateEmails = null)
        {
            _window = window;
            ListEmails = listEmails;
            _listUpdateEmails = listUpdateEmails;
            _addEmail = new Email();
            _isEdit = false;
        }
        public AddEmailViewModel(AddEmailWindow window, Email selectMail, ObservableCollection<Email> listEmails, ObservableCollection<Email> listUpdateEmails = null)
        {
            _window = window;
            ListEmails = listEmails;
            _listUpdateEmails = listUpdateEmails;
            _addEmail = new Email(selectMail);
            _oldEmail = selectMail;
            _isEdit = true;
        }
        public Email AddEmail
        {
            get
            {
                return _addEmail;
            }
            set
            {
                this.SetValue(ref _addEmail, value, AddEmailProperty);
                OnPropertyChanged("AddEmail");
            }
        }
        public ObservableCollection<Email> ListEmails
        {
            get
            {
                return _listEmails;
            }
            set
            {
                this.SetValue(ref _listEmails, value, ListEmailsProperty);
                OnPropertyChanged("ListEmails");
            }
        }
        #endregion

        #region method ICommand
        public ICommand BtnAddEmailCommand
        {
            get
            {
                return _btnAddEmailCommand ?? (_btnAddEmailCommand = new CommandHandler(BtnAddEmail, true));
            }
        }
        private void BtnAddEmail()
        {
            if (string.IsNullOrEmpty(AddEmail.Name))
            {
                MessageBox.Show("không được để trống tên");
                return;
            }
            if (string.IsNullOrEmpty(AddEmail.Mail))
            {
                MessageBox.Show("không được để trống email");
                return;
            }
            bool removeListDisplay = false;
            bool removeListUpdate = false;
            if (_isEdit)
            {
                // check change
                if (_oldEmail.Mail.Equals(AddEmail.Mail) && _oldEmail.Name.Equals(AddEmail.Name))
                {
                    MessageBox.Show("Không thay đổi thông tin email");
                    return;
                }
                // remove info list display and list update
                removeListDisplay = ListEmails.Remove(_oldEmail);
                removeListUpdate = _listUpdateEmails.Remove(_oldEmail);
            }
            // check email exist in list
            int i = 0;
            for(; i < ListEmails.Count; i++)
            {
                if(ListEmails[i].Name.ToUpper().Equals(AddEmail.Name.ToUpper())
                    && ListEmails[i].Mail.ToUpper().Equals(AddEmail.Mail.ToUpper()))
                {
                    MessageBox.Show("Đã tồn tại địa chỉ email này \n vui lòng kiểm tra lại");
                    break;
                }
            }
            if(i == ListEmails.Count)
            {
                //email is not yet
                ListEmails.Add(AddEmail);
                if (_listUpdateEmails != null)
                {
                    // current screen is edit info
                    // delete old info and add new info
                    if(AddEmail.IDEmail != 0)
                    {
                        _listUpdateEmails.Add(_oldEmail);
                        AddEmail.IDEmail = 0;
                    }
                    _listUpdateEmails.Add(AddEmail);
                } 
            } else
            {
                // restore data list
                if (removeListDisplay)
                {
                    ListEmails.Add(_oldEmail);
                }
                if (removeListUpdate)
                {
                    _listUpdateEmails.Add(_oldEmail);
                }
            }
            BtnExitCommand.Execute(true);
        }

        public ICommand BtnDeleteEmailCommand
        {
            get
            {
                return _btnDeleteEmailCommand ?? (_btnDeleteEmailCommand = new CommandHandler(DeleteEmail, true));
            }
        }
        private void DeleteEmail()
        {
            if (_listUpdateEmails != null)
            {
                int i = 0;
                for (; i < _listUpdateEmails.Count; i++)
                {
                    if (_listUpdateEmails[i].Name.ToUpper().Equals(_oldEmail.Name.ToUpper()) && _listUpdateEmails[i].Mail.ToUpper().Equals(_oldEmail.Mail.ToUpper()) && _listUpdateEmails[i].IDEmail == _oldEmail.IDEmail)
                    {
                        break;
                    }
                }
                if (i == _listUpdateEmails.Count && _oldEmail.IDEmail != 0)
                {
                    _listUpdateEmails.Add(_oldEmail);
                }
                else if (i < _listUpdateEmails.Count && _oldEmail.IDEmail == 0)
                {
                    _listUpdateEmails.Remove(_oldEmail);
                }
            }
            ListEmails.Remove(_oldEmail);
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
