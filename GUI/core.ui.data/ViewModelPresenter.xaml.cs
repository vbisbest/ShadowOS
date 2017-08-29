using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;

namespace core.ui.data
{
    public sealed partial class ViewModelPresenter : UserControl
    {
        public ViewModelPresenter()
        {
            this.InitializeComponent();
        }

        #region ViewModel
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(object), typeof(ViewModelPresenter), new PropertyMetadata(null, (s, ea) =>
        {
            (s as ViewModelPresenter).UpdateView();
        }));

        public ViewModelBase ViewModel
        {
            get
            {
                return (ViewModelBase)this.GetValue(ViewModelProperty);
            }

            set
            {
                this.SetValue(ViewModelProperty, value);
            }
        }
        #endregion

        #region Template
        public static DependencyProperty DataTemplateProperty = DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(ViewModelPresenter), new PropertyMetadata(null, (s, ea) =>
        {
        }));

        public DataTemplate DataTemplate
        {
            get
            {
                return (DataTemplate) this.GetValue(DataTemplateProperty);
            }
            set
            {
                this.SetValue(DataTemplateProperty, value);
            }

        }
        #endregion

        void UpdateView()
        {
            if (ViewModel != null)
            {
                if (ViewModel.View != null)
                {
                    root.Child = ViewModel.View;
                }
                else if (DataTemplate != null)
                {
                    FrameworkElement fe = DataTemplate.LoadContent() as FrameworkElement;
                    ViewModelBase.SetGlue(fe, true);
                    fe.DataContext = ViewModel;

                    root.Child = fe;
                }
            }
        }
    }
}
