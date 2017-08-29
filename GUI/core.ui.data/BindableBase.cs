using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace core.ui.data
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void NotifyPropertyChanged(PropertyInfo property)
        {
            NotifyPropertyChanged(property.Name);
        }

        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            NotifyPropertyChanged(property.GetPropertyName());
        }

        public void NotifyAllPropertiesChanged()
        {
            NotifyPropertyChanged(string.Empty);
        }
    }
}
