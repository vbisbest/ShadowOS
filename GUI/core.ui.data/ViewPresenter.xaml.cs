using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Reflection;
using System.Windows.Controls;
using System.Windows;

namespace core.ui.data
{
    public sealed partial class ViewPresenter : UserControl
    {
        public ViewPresenter()
        {
            this.InitializeComponent();
        }

        #region Contract
        public static DependencyProperty ContractProperty = DependencyProperty.Register("Contract", typeof(ViewContract), typeof(ViewPresenter), new PropertyMetadata(null, (s, ea) =>
        {
            (s as ViewPresenter).UpdateView();
        }));

        public ViewContract Contract
        {
            get
            {
                return (ViewContract) this.GetValue(ContractProperty);
            }

            set
            {
                this.SetValue(ContractProperty, value);
            }
        }
        #endregion

        void UpdateView()
        {
            if (Contract != null)
            {
                root.Child = Contract.View;
            }
        }
    }

    public class ViewContract : TypeWrapper, IDisposable
    {
        public Control View
        {
            get
            {
                if (_View == null)
                {
                    if (_Type != null)
                    {
                        _View = Activator.CreateInstance(_Type) as Control;
                    }
                }

                return _View;
            }
        }
        private Control _View = null;

        void IDisposable.Dispose()
        {
            _View = null;
        }
    }
}
