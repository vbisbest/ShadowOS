using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace core.ui.data
{
    public class ViewModelWrapper : IDisposable
    {
        public ViewModelWrapper(ViewModelBase source, ViewModelBase wrapper)
        {
            Source = source;
            Wrapper = wrapper;
        }

        public ViewModelBase Wrapper
        {
            get
            {
                return _Wrapper;
            }
            set
            {
                if (_Wrapper != null)
                {
                    _Wrapper = value;
                    _Properties.AddRange(_Wrapper.GetType().GetProperties(BindingFlags.Public));
                }
            }
        }
        private ViewModelBase _Wrapper = null;
        private List<PropertyInfo> _Properties = new List<PropertyInfo>();

        public ViewModelBase Source
        {
            get
            {
                return _Source;
            }
            private set
            {
                _Source = value;

                if (_Source != null)
                {
                    _Source.PropertyChanged += _Source_PropertyChanged;
                    _Source.NotifyAllPropertiesChanged();
                }
            }
        }
        private ViewModelBase _Source = null;        

        void _Source_PropertyChanged(object sender, PropertyChangedEventArgs ea)
        {
            if (ea != null)
            {
                foreach(PropertyInfo pi in _Properties)
                {
                    if (ea.HasPropertyChanged(pi))
                    {
                        _Wrapper.NotifyPropertyChanged(pi);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_Wrapper != null)
            {
                _Wrapper = null;
            }

            if (_Source != null)
            {
                _Source.PropertyChanged -= _Source_PropertyChanged;
                _Source = null;
            }

            if (_Properties != null)
            {
                _Properties.Clear();
                _Properties = null;
            }
        }
    }
}
