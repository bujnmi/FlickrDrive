using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FlickrDrive.Annotations;
using GalaSoft.MvvmLight.Command;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FlickrDrive.ViewModel
{
    public class MainViewModel:INotifyPropertyChanged
    {
        private RelayCommand _openFolderCommand;
        private RelayCommand _synchronizeCommand;
        private RelayCommand _stopCommand;
        private RelayCommand _refreshCommand;
        private RelayCommand _changeRootCommand;
        private RelayCommand _logoutCommand;
        private RelayCommand _reorderSetsCommand;
        public FlickrAlive flickrAlive { get; }

        public MainViewModel(FlickrAlive flickrAlive)
        {
            this.flickrAlive = flickrAlive;
            this.flickrAlive.Initialize();
            this.flickrAlive.PropertyChanged += OnAliveOnPropertyChanged;
        }

        private void OnAliveOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(flickrAlive.IsLoggedIn))
            {
                OnPropertyChanged(nameof(LoginString));
            }
        }

        public string LoginString
        {
            get
            {
                string toReturn = "Login";
                if (flickrAlive.IsLoggedIn)
                {
                    toReturn = $"Logout ({flickrAlive.UserName})";
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
            Task.Run(() =>
            {
                flickrAlive.UpdateMeta();
            });
        }

        private void OpenFolder()
        {
            Process.Start(flickrAlive.Root);
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
            if (flickrAlive.IsLoggedIn)
            {
                flickrAlive.Logout();
            }
            else
            {
                flickrAlive.Initialize();
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
            flickrAlive.Stop();
        }

        public RelayCommand ChangeRootCommand
        {
            get
            {
                return _changeRootCommand ?? (_changeRootCommand = new RelayCommand(ChangeRoot));
            }
        }

        public RelayCommand ReorderSetsCommand
        {
            get
            {
                return _reorderSetsCommand ?? (_reorderSetsCommand = new RelayCommand(ReorderSets));
            }
        }

        private void ChangeRoot()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                flickrAlive.Root = dialog.FileName;
            }
        }

        private void Synchronize()
        {
            flickrAlive.Synchronize();
        }

        private void ReorderSets()
        {
            flickrAlive.ReorderSets();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}