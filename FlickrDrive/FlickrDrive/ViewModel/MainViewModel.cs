using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FlickrDrive.Annotations;
using GalaSoft.MvvmLight.Command;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FlickrDrive.ViewModel
{
    public class MainViewModel:INotifyPropertyChanged
    {
        private readonly Alive _alive;
        private RelayCommand _openFolderCommand;
        private RelayCommand _synchronizeCommand;
        private RelayCommand _stopCommand;
        private RelayCommand _refreshCommand;
        private RelayCommand _changeRootCommand;
        private RelayCommand _logoutCommand;
        public Alive Alive { get { return _alive; } }

        public MainViewModel(Alive alive)
        {
            _alive = alive;
            _alive.Initialize();
            _alive.PropertyChanged += OnAliveOnPropertyChanged;
        }

        private void OnAliveOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Alive.IsLoggedIn))
            {
                OnPropertyChanged(nameof(LoginString));
            }
        }

        public string LoginString
        {
            get
            {
                string toReturn = "Login";
                if (_alive.IsLoggedIn)
                {
                    toReturn = $"Logout ({_alive.UserName})";
                }
                return toReturn;
            }
        }
        public RelayCommand OpenFolderCommand {
            get
        {
            return _openFolderCommand ?? (_openFolderCommand = new RelayCommand(OpenFolder));
        } }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh));
            }
        }

        private void Refresh()
        {
            _alive.UpdateMeta();
        }

        private void OpenFolder()
        {
            Process.Start(_alive.Root);
        }


        public RelayCommand SynchronizeCommand
        {
            get
            {
                return _synchronizeCommand ?? (_synchronizeCommand = new RelayCommand(Synchronize));
            }
        }

        public RelayCommand LoginLogoutCommand
        {
            get
            {
                return _logoutCommand ?? (_logoutCommand = new RelayCommand(LoginLogout));
            }
        }

        private void LoginLogout()
        {
            if (_alive.IsLoggedIn)
            {
                _alive.Logout();
            }
            else
            {
                _alive.Initialize();
            }
        }

        public RelayCommand StopCommand
        {
            get
            {
                return _stopCommand ?? (_stopCommand = new RelayCommand(Stop));
            }
        }   

        private void Stop()
        {
            _alive.Stop();
        }

        public RelayCommand ChangeRootCommand
        {
            get
            {
                return _changeRootCommand ?? (_changeRootCommand = new RelayCommand(ChangeRoot));
            }
        }

        private void ChangeRoot()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _alive.Root = dialog.FileName;
            }
        }

        private void Synchronize()
        {
            _alive.Synchronize();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}