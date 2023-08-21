using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jacobs.DD.AbstractObjects
{
    public class ObservableObjects : INotifyPropertyChanged
    {
        //Setup INotifyPropertyChanged here
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
