using System.ComponentModel;
using System.Runtime.CompilerServices;
using FlickrDrive.Annotations;

namespace FlickrDrive
{
    public class BaseNotifyObject:INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
}