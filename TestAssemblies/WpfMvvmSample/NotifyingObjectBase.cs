using System.ComponentModel;
using WpfMvvmSample.Annotations;

namespace WpfMvvmSample
{
    public class NotifyingObjectBase: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public bool SetProperty<T>(ref T backingField, T value, string propertyName)
        {
            bool shouldChange = !Equals(backingField, value);
            if (shouldChange)
            {
                backingField = value;
                OnPropertyChanged(propertyName);
            }
            return shouldChange;
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}