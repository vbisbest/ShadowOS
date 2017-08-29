using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace core.ui.data
{
    public delegate void ValidationEventHandler(ValidationEventArgs ea);

    public enum ValidationStatus
    {
        None,
        Error,
        Warning,
        Information
    }

    public class ValidationSource : BindableBase, IDisposable
    {
        public ValidationSource()
        {
        }

        public bool IsValid
        {
            get
            {
                return !HasError();
            }
        }

        public bool IsValidating
        {
            get
            {
                return _IsValidating;
            }

            set
            {
                _IsValidating = value;
                NotifyPropertyChanged(() => IsValidating);
            }
        }
        bool _IsValidating = false;

        public bool HasError()
        {
            bool rc = false;

            foreach (ValidationInfo vi in ValidationInfo)
            {
                if (vi.HasError)
                {
                    rc = true;
                    break;
                }
            }

            return rc;
        }

        public bool HasWarning()
        {
            bool rc = false;

            foreach (ValidationInfo vi in ValidationInfo)
            {
                if (vi.HasWarning)
                {
                    rc = true;
                    break;
                }
            }

            return rc;
        }

        public bool HasInformation()
        {
            bool rc = false;

            foreach (ValidationInfo vi in ValidationInfo)
            {
                if (vi.HasInformation)
                {
                    rc = true;
                    break;
                }
            }

            return rc;
        }

        public void SetAllError(string message)
        {
            foreach (ValidationInfo vi in ValidationInfo)
            {
                vi.ErrorMessage = message;
            }
        }

        public void SetAllWarning(string message)
        {
            foreach (ValidationInfo vi in ValidationInfo)
            {
                vi.WarningMessage = message;
            }
        }

        public void SetAllInformation(string message)
        {
            foreach (ValidationInfo vi in ValidationInfo)
            {
                vi.InformationMessage = message;
            }
        }

        public void SetError<T>(Expression<Func<T>> property, string message)
        {
            ValidationInfo vi = this[property.GetPropertyName()];
            vi.ErrorMessage = message;
        }

        public void SetWarning<T>(Expression<Func<T>> property, string message)
        {
            ValidationInfo vi = this[property.GetPropertyName()];
            vi.WarningMessage = message;
        }

        public void SetInformation<T>(Expression<Func<T>> property, string message)
        {
            ValidationInfo vi = this[property.GetPropertyName()];
            vi.InformationMessage = message;
        }

        public void ClearAllError()
        {
            foreach (ValidationInfo vi in ValidationInfo)
            {
                vi.ErrorMessage = null;
            }
        }

        public void ClearAllWarning()
        {
            foreach (ValidationInfo vi in ValidationInfo)
            {
                vi.WarningMessage = null;
            }
        }

        public void ClearAllInformation()
        {
            foreach (ValidationInfo vi in ValidationInfo)
            {
                vi.InformationMessage = null;
            }
        }

        public void ClearAll()
        {
            foreach (ValidationInfo vi in ValidationInfo)
            {
                vi.ErrorMessage = null;
                vi.WarningMessage = null;
                vi.InformationMessage = null;
            }
        }

        public void ClearError<T>(Expression<Func<T>> property)
        {
            ValidationInfo vi = this[property.GetPropertyName()];
            vi.ErrorMessage = null;
        }

        public void ClearWarning<T>(Expression<Func<T>> property)
        {
            ValidationInfo vi = this[property.GetPropertyName()];
            vi.WarningMessage = null;
        }

        public void ClearInformation<T>(Expression<Func<T>> property)
        {
            ValidationInfo vi = this[property.GetPropertyName()];
            vi.InformationMessage = null;
        }

        public void Validate()
        {
            ValidationEventArgs ea = new ValidationEventArgs(this);

            if (Validating != null)
            {
                IsValidating = true;
                Validating(ea);
                IsValidating = false;
            }

            NotifyPropertyChanged(() => ValidationInfo);
        }
        

        public event ValidationEventHandler Validating;

        internal ValidationInfo GetValidationInfo<T>(Expression<Func<T>> property)
        {
            return GetValidationInfo(property.GetPropertyName());
        }

        ValidationInfo GetValidationInfo(string propertyName)
        {
            ValidationInfo rc = null;

            rc = Get(propertyName);

            if (rc == null)
            {
                rc = new ValidationInfo(propertyName);
                rc.PropertyChanged += ValidationInfo_PropertyChanged;
                _ValidationInfo.Add(rc);
            }

            return rc;
        }

        private ValidationInfo Get(string propertyName)
        {
            ValidationInfo rc = null;

            foreach (ValidationInfo vi in _ValidationInfo)
            {
                if (vi.PropertyName == propertyName)
                {
                    rc = vi;
                    break;
                }
            }

            return rc;
        }

        internal ValidationInfo this[string propertyName]
        {
            get
            {
                return GetValidationInfo(propertyName);
            }
        }

        internal ObservableCollection<ValidationInfo> ValidationInfo
        {
            get
            {
                return _ValidationInfo;
            }
        }
        ObservableCollection<ValidationInfo> _ValidationInfo = new ObservableCollection<ValidationInfo>();

        void ValidationInfo_PropertyChanged(object sender, PropertyChangedEventArgs ea)
        {
            if (!IsValidating)
            {
                ValidationInfo vi = sender as ValidationInfo;
                if (vi != null)
                {
                    if (ea.HasPropertyChanged(() => vi.ErrorMessage))
                    {
                        NotifyPropertyChanged(() => ValidationInfo);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_ValidationInfo != null)
            {
                foreach (ValidationInfo vi in _ValidationInfo)
                {
                    vi.Dispose();
                }

                _ValidationInfo.Clear();

                _ValidationInfo = null;
            }
        }
    }

    [DebuggerDisplay("{PropertyName}")]
    public class ValidationInfo : BindableBase, IDisposable
    {
        internal ValidationInfo(string propertyName)
        {
            PropertyName = propertyName;
        }

        public bool HasStatus
        {
            get
            {
                return (AggregateStatus.Max() != ValidationStatus.None);
            }
        }

        public bool IsValid
        {
            get
            {
                return HasError;
            }
        }

        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(ErrorMessage);
            }
        }

        public bool HasWarning
        {
            get
            {
                return !string.IsNullOrEmpty(WarningMessage);
            }
        }

        public bool HasInformation
        {
            get
            {
                return !string.IsNullOrEmpty(InformationMessage);
            }
        }

        public string GetMessage(ValidationStatus status)
        {
            string rc = null;

            if (status == ValidationStatus.Error)
            {
                rc = ErrorMessage;
            }

            if (status == ValidationStatus.Warning)
            {
                rc = WarningMessage;
            }

            if (status == ValidationStatus.Information)
            {
                rc = InformationMessage;
            }

            return rc;
        }

        public List<ValidationStatus> AggregateStatus
        {
            get
            {
                List<ValidationStatus> rc = new List<ValidationStatus>();

                if (HasError)
                {
                    rc.Add(ValidationStatus.Error);
                }

                if (HasWarning)
                {
                    rc.Add(ValidationStatus.Warning);
                }

                if (HasInformation)
                {
                    rc.Add(ValidationStatus.Information);
                }

                return rc;
            }
        }

        void NotifyAggregateStatusChange(ValidationStatus status)
        {
            if(!AggregateStatus.SameAs(_PreviousStatus))
            {
                NotifyPropertyChanged(() => AggregateStatus);
                NotifyPropertyChanged(() => HasStatus);
                _PreviousStatus = AggregateStatus;
            }
            else if (AggregateStatus.Contains(status))
            {
                NotifyPropertyChanged(() => AggregateStatus);
                NotifyPropertyChanged(() => HasStatus);
            }
        }
        List<ValidationStatus> _PreviousStatus = new List<ValidationStatus>();

        public string InformationMessage
        {
            get
            {
                return _InformationMessage;
            }
            set
            {
                if (_InformationMessage != value)
                {
                    _InformationMessage = value;

                    NotifyPropertyChanged(() => InformationMessage);
                    NotifyPropertyChanged(() => HasInformation);
                    NotifyAggregateStatusChange(ValidationStatus.Information);
                }
            }
        }
        private string _InformationMessage = string.Empty;

        public string WarningMessage
        {
            get
            {
                return _WarningMessage;
            }
            set
            {
                if (_WarningMessage != value)
                {
                    _WarningMessage = value;

                    NotifyPropertyChanged(() => WarningMessage);
                    NotifyPropertyChanged(() => HasWarning);
                    NotifyAggregateStatusChange(ValidationStatus.Warning);
                }
            }
        }
        private string _WarningMessage = string.Empty;

        public string ErrorMessage
        {
            get
            {
                return _ErrorMessage;
            }
            set
            {
                if (_ErrorMessage != value)
                {
                    _ErrorMessage = value;

                    NotifyPropertyChanged(() => ErrorMessage);
                    NotifyPropertyChanged(() => HasError);
                    NotifyPropertyChanged(() => IsValid);
                    NotifyAggregateStatusChange(ValidationStatus.Error);
                }
            }
        }
        private string _ErrorMessage = string.Empty;

        public string PropertyName
        {
            get
            {
                return _PropertyName;
            }

            private set
            {
                if (!value.IsValidIdentifier())
                {
                    throw new ArgumentException(string.Format("PropertyName: Name must be a valid C# identifier. '{0}'", value));
                }

                _PropertyName = value;
            }
        }
        private string _PropertyName;

        public void Dispose()
        {
        }
    }

    public class ValidationEventArgs : EventArgs
    {
        public ValidationEventArgs(ValidationSource source)
        {
            _Source = source;
        }

        public ValidationSource Source
        {
            get
            {
                return _Source;
            }
        }
        private ValidationSource _Source = null;
    }

    [ContentProperty("Content")]
    public class Validator : ValidatorBase
    {
        public Validator() : base("Validator:Control")
        {
            Margin = new Thickness(0);
        }

        #region Content
        public static DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(Validator), new PropertyMetadata(null));

        public object Content
        {
            get
            {
                return (object) GetValue(ContentProperty);
            }
            set
            {
                SetValue(ContentProperty, value);
            }
        }
        #endregion

        #region ValidationBrush
        public static DependencyProperty ValidationBrushProperty = DependencyProperty.Register("ValidationBrush", typeof(Brush), typeof(Validator), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public Brush ValidationBrush
        {
            get
            {
                return (Brush) GetValue(ValidationBrushProperty);
            }
            private set
            {
                SetValue(ValidationBrushProperty, value);
            }
        }
        #endregion

        public override void Update()
        {
            string tt = string.Empty;
            List<ValidationStatus> aggregate = AggregateStatus;
            ValidationStatus vs = _Status.Matches(aggregate).Max();

            if (vs != ValidationStatus.None)
            {
                foreach (ValidationInfo vi in _ValidationInfo)
                {
                    if (vi.AggregateStatus.Contains(vs))
                    {
                        tt = string.IsNullOrEmpty(tt) ? vi.GetMessage(vs) : string.Format("{0}\r\n{1}", tt, vi.GetMessage(vs));
                    }
                }
            }

            ToolTip = string.IsNullOrEmpty(tt) ? (string)null : tt;

            if (vs != ValidationStatus.None)
            {
                switch (vs)
                {
                    case ValidationStatus.Error:
                        ValidationBrush = new SolidColorBrush(Colors.Red);
                        break;

                    case ValidationStatus.Warning:
                        ValidationBrush = new SolidColorBrush(Colors.Gold);
                        break;

                    case ValidationStatus.Information:
                        ValidationBrush = new SolidColorBrush(Colors.Blue);
                        break;
                }
            }
            else if (vs == ValidationStatus.None)
            {
                ValidationBrush = new SolidColorBrush(Colors.Transparent);
            }
            else if (string.IsNullOrEmpty(tt))
            {
                ValidationBrush = new SolidColorBrush(Colors.Transparent);
            }

            if (string.IsNullOrEmpty(tt))
            {
                Replace();
            }
            else
            {
                if (!_Exchanged)
                {
                    Exchange();
                }
            }
        }

        List<ValidationStatus> AggregateStatus
        {
            get
            {
                List<ValidationStatus> rc = new List<ValidationStatus>();

                foreach (ValidationInfo vi in _ValidationInfo)
                {
                    foreach (ValidationStatus vs in vi.AggregateStatus)
                    {
                        rc.AddDistinct(vs);
                    }
                }

                return rc;
            }
        }

        #region Margin
        void Exchange()
        {
            if(Content != null)
            {
                if(Content is FrameworkElement)
                {
                    FrameworkElement fe = Content as FrameworkElement;

                    if(fe != null)
                    {
                        Thickness swap;

                        swap = Margin;
                        Margin = fe.Margin;
                        fe.Margin = swap;
                        _Exchanged = !_Exchanged;
                    }
                }
            }
        }
        bool _Exchanged = false;

        void Replace()
        {
            if (_Exchanged)
            {
                Exchange();
            }
        }
        #endregion

        protected override void ValidationSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }

    public class ValidatorDetail : ValidatorBase
    {
        public ValidatorDetail() : base("ValidatorDetail:Control")
        {
            this.SetValue(StatusMessagesProperty, new ObservableCollection<ValidationStatusWrapper>());
        }

        #region Max
        public static DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(bool), typeof(ValidatorDetail), new PropertyMetadata(false, (s, ea) =>
        {
            ValidatorDetail v = s as ValidatorDetail;

            if (v != null)
            {
                v.UpdateBindings();
            }
        }));

        public bool Max
        {
            get
            {
                return (bool) GetValue(MaxProperty);
            }

            set
            {
                SetValue(MaxProperty, value);
            }
        }
        #endregion

        #region Group
        public static DependencyProperty GroupProperty = DependencyProperty.Register("Group", typeof(bool), typeof(ValidatorDetail), new PropertyMetadata(false, (s, ea) =>
        {
            ValidatorDetail v = s as ValidatorDetail;

            if (v != null)
            {
                v.UpdateBindings();
            }
        }));

        public bool Group
        {
            get
            {
                return (bool)GetValue(GroupProperty);
            }

            set
            {
                SetValue(GroupProperty, value);
            }
        }
        #endregion

        List<ValidationStatus> AggregateStatus
        {
            get
            {
                List<ValidationStatus> rc = new List<ValidationStatus>();

                foreach (ValidationInfo vi in _ValidationInfo)
                {
                    foreach (ValidationStatus vs in vi.AggregateStatus)
                    {
                        rc.AddDistinct(vs);
                    }
                }

                return rc;
            }
        }

        ValidationStatus MaxStatus
        {
            get
            {
                ValidationStatus rc = _Status.Matches(AggregateStatus).Max();

                return rc;
            }
        }

        public override void Update()
        {
            List<ValidationStatusWrapper> sms = new List<ValidationStatusWrapper>();

            ValidationStatus max = MaxStatus;

            ValidationStatusWrapper e = new ValidationStatusWrapper() { Status = ValidationStatus.Error };
            ValidationStatusWrapper w = new ValidationStatusWrapper() { Status = ValidationStatus.Warning };
            ValidationStatusWrapper i = new ValidationStatusWrapper() { Status = ValidationStatus.Information };

            foreach (ValidationInfo vi in _ValidationInfo)
            {
                if (vi.HasError && (!Max || (Max && max == ValidationStatus.Error)))
                {
                    if (_Status.Contains(ValidationStatus.Error))
                    {
                        if (Group)
                        {
                            e.AppendMessage(vi.ErrorMessage);
                        }
                        else
                        {
                            sms.Add(new ValidationStatusWrapper() { Status = ValidationStatus.Error, Message = vi.ErrorMessage });
                        }
                    }
                }

                if (vi.HasWarning && (!Max || (Max && max == ValidationStatus.Warning)))
                {
                    if (_Status.Contains(ValidationStatus.Warning))
                    {
                        if (Group)
                        {
                            w.AppendMessage(vi.WarningMessage);
                        }
                        else
                        {
                            sms.Add(new ValidationStatusWrapper() { Status = ValidationStatus.Warning, Message = vi.WarningMessage });
                        }
                    }
                }

                if (vi.HasInformation && (!Max || (Max && max == ValidationStatus.Information)))
                {
                    if (_Status.Contains(ValidationStatus.Information))
                    {
                        if (Group)
                        {
                            i.AppendMessage(vi.InformationMessage);
                        }
                        else
                        {
                            sms.Add(new ValidationStatusWrapper() { Status = ValidationStatus.Information, Message = vi.InformationMessage });
                        }
                    }
                }
            }

            if (Group)
            {
                if (e.HasMessage)
                {
                    sms.Add(e);
                }

                if (w.HasMessage)
                {
                    sms.Add(w);
                }

                if (i.HasMessage)
                {
                    sms.Add(i);
                }
            }

            StatusMessages.Clear();
            StatusMessages.AddRange(sms.OrderBy(wrapper => wrapper.Status).ToList());
        }

        public static DependencyProperty StatusMessagesProperty = DependencyProperty.Register("StatusMessages", typeof(ObservableCollection<ValidationStatusWrapper>), typeof(ValidatorDetail), new PropertyMetadata(null));
        public ObservableCollection<ValidationStatusWrapper> StatusMessages
        {
            get
            {
                return (ObservableCollection<ValidationStatusWrapper>) this.GetValue(StatusMessagesProperty);
            }
        }

        protected override void ValidationSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }

    [DebuggerDisplay("{Message}")]
    public class ValidationStatusWrapper : BindableBase
    {
        public ValidationStatusWrapper()
        {
        }

        public ValidationStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                NotifyPropertyChanged(() => Status);
            }
        }
        private ValidationStatus _Status = ValidationStatus.None;

        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                _Message = value;
                NotifyPropertyChanged(() => Message);
                NotifyPropertyChanged(() => HasMessage);
            }
        }
        private string _Message = string.Empty;

        public void AppendMessage(string message)
        {
            Message = string.IsNullOrEmpty(Message) ? message : string.Format("{0}\r\n{1}", Message, message);
        }

        public bool HasMessage
        {
            get
            {
                return !string.IsNullOrEmpty(Message);
            }
        }
    }

    public class ValidationStatusDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Error
        {
            get;
            set;
        }

        public DataTemplate Warning
        {
            get;
            set;
        }

        public DataTemplate Information
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate rc = null;

            ValidationStatusWrapper wrapper = item as ValidationStatusWrapper;

            if (wrapper != null)
            {
                if (wrapper.Status == ValidationStatus.Error)
                {
                    rc = Error;
                }

                if (wrapper.Status == ValidationStatus.Warning)
                {
                    rc = Warning;
                }

                if (wrapper.Status == ValidationStatus.Information)
                {
                    rc = Information;
                }
            }

            return rc;
        }
    }

    public class ValidatorPopup : ValidatorBase
    {
        public ValidatorPopup() : base("ValidatorPopup:Control")
        {
        }

        public override void Update()
        {
        }

        protected override void ValidationSource_PropertyChanged(object sender, PropertyChangedEventArgs ea)
        {
            if(ea.HasPropertyChanged(() => ValidationSource.IsValidating) && ValidationSource.IsValidating == false)
            {
                string message = string.Empty;

                List<ValidationStatus> aggregate = AggregateStatus;
                ValidationStatus vs = _Status.Matches(aggregate).Max();

                if (vs != ValidationStatus.None)
                {
                    if (_Dialog == null)
                    {
                        ValidatorDetail detail = new ValidatorDetail();

                        detail.SetBinding(ValidatorDetail.ValidationSourceProperty, new Binding() { Source = this, Path = this.GetPropertyPath(() => ValidationSource) });
                        detail.SetBinding(ValidatorDetail.PropertyNameProperty, new Binding() { Source = this, Path = this.GetPropertyPath(() => PropertyName) });
                        detail.SetBinding(ValidatorDetail.StatusProperty, new Binding() { Source = this, Path = this.GetPropertyPath(() => Status) });

                        detail.Group = true;
                        detail.Max = true;

                        _Dialog = new ValidatorDialog(detail);

                        _Dialog.Closing += (s, e) =>
                        {
                            _Dialog = null;
                        };

                        _Dialog.Owner = Window;
                        _Dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        _Dialog.Show();
                    }
                }
            }
        }

        ValidatorDialog _Dialog = null;

        List<ValidationStatus> AggregateStatus
        {
            get
            {
                List<ValidationStatus> rc = new List<ValidationStatus>();

                foreach (ValidationInfo vi in _ValidationInfo)
                {
                    foreach (ValidationStatus vs in vi.AggregateStatus)
                    {
                        rc.AddDistinct(vs);
                    }
                }

                return rc;
            }
        }

        public Window Window
        {
            get
            {
                return this.Find(typeof(Window)) as Window;
            }
        }
    }

    public abstract class ValidatorBase : Control, IDisposable
    {
        public ValidatorBase(string garbage)
        {
            this.Publish(new Garbage(this, garbage));
            Status = "e";
        }

        #region ValidationSource
        public static DependencyProperty ValidationSourceProperty = DependencyProperty.Register("ValidationSource", typeof(ValidationSource), typeof(ValidatorBase), new PropertyMetadata(null, (s, ea) =>
        {
            ValidatorBase v = s as ValidatorBase;

            if (v != null)
            {
                if (ea.OldValue != null)
                {
                    ValidationSource vs = ea.OldValue as ValidationSource;

                    if (vs != null)
                    {
                        vs.ValidationInfo.CollectionChanged -= v.ValidationInfo_CollectionChanged;
                        vs.PropertyChanged -= v.ValidationSource_PropertyChanged;
                    }
                }

                if (ea.NewValue != null)
                {
                    ValidationSource vs = ea.NewValue as ValidationSource;

                    if (vs != null)
                    {
                        vs.PropertyChanged -= v.ValidationSource_PropertyChanged;
                        vs.PropertyChanged += v.ValidationSource_PropertyChanged;
                    }
                }

                v.UpdateBindings();
            }
        }));

        protected abstract void ValidationSource_PropertyChanged(object sender, PropertyChangedEventArgs e);

        public ValidationSource ValidationSource
        {
            get
            {
                return (ValidationSource) GetValue(ValidationSourceProperty);
            }
            set
            {
                SetValue(ValidationSourceProperty, value);
            }
        }

        void InitializeBindings()
        {
            DisposeBindings();
            _ValidationInfo = new List<ValidationInfo>();
        }

        void DisposeBindings()
        {
            if (_ValidationInfo != null)
            {
                foreach (ValidationInfo vi in _ValidationInfo)
                {
                    vi.PropertyChanged -= ValidationInfo_PropertyChanged;
                }

                _ValidationInfo.Clear();

                _ValidationInfo = null;
            }
        }

        protected void UpdateBindings()
        {
            InitializeBindings();

            if (ValidationSource != null && !string.IsNullOrEmpty(PropertyName) && !string.IsNullOrEmpty(Status))
            {
                bool star = _PropertyNames.Contains("*");

                if (star)
                {
                    ValidationSource.ValidationInfo.CollectionChanged -= ValidationInfo_CollectionChanged;
                    ValidationSource.ValidationInfo.CollectionChanged += ValidationInfo_CollectionChanged;

                    foreach (ValidationInfo vi in ValidationSource.ValidationInfo)
                    {
                        _ValidationInfo.AddDistinct(vi);
                        vi.PropertyChanged += ValidationInfo_PropertyChanged;
                    }
                }
                else
                {
                    foreach (string propertyName in _PropertyNames)
                    {
                        ValidationInfo vi = ValidationSource[propertyName];

                        _ValidationInfo.AddDistinct(vi);
                        vi.PropertyChanged -= ValidationInfo_PropertyChanged;
                        vi.PropertyChanged += ValidationInfo_PropertyChanged;
                    }
                }
            }
        }

        void ValidationInfo_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs ea)
        {
            foreach (ValidationInfo vi in ea.NewItems)
            {
                _ValidationInfo.AddDistinct(vi);
                vi.PropertyChanged += ValidationInfo_PropertyChanged;
            }
        }

        void ValidationInfo_PropertyChanged(object s, PropertyChangedEventArgs ea)
        {
            ValidationInfo vi = s as ValidationInfo;
            if (ea.HasPropertyChanged(() => vi.AggregateStatus))
            {
                Update();
            }
        }

        protected List<ValidationInfo> _ValidationInfo = null;
        #endregion

        #region PropertyName
        public static DependencyProperty PropertyNameProperty = DependencyProperty.Register("PropertyName", typeof(string), typeof(ValidatorBase), new PropertyMetadata(null, (s, ea) =>
        {
            ValidatorBase v = s as ValidatorBase;

            if (v != null)
            {
                v.UpdateProperties();
                v.UpdateBindings();
            }
        }));

        public string PropertyName
        {
            get
            {
                return (string)GetValue(PropertyNameProperty);
            }
            set
            {
                SetValue(PropertyNameProperty, value);
            }
        }

        void InitializePropertyNames()
        {
            DisposePropertyNames();
            _PropertyNames = new List<string>();
        }

        void DisposePropertyNames()
        {
            if (_PropertyNames != null)
            {
                _PropertyNames.Clear();

                _PropertyNames = null;
            }
        }

        void UpdateProperties()
        {
            InitializePropertyNames();

            if (!string.IsNullOrEmpty(PropertyName))
            {
                foreach (string pn in PropertyName.Split(','))
                {
                    string name = pn.Trim();

                    if (!string.IsNullOrEmpty(name))
                    {
                        _PropertyNames.AddDistinct(name);
                    }
                }
            }
        }

        List<string> _PropertyNames = null;
        #endregion

        #region Status
        public static DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(string), typeof(ValidatorBase), new PropertyMetadata(null, (s, ea) =>
        {
            ValidatorBase v = s as ValidatorBase;

            if (v != null)
            {
                v.UpdateStatus();
                v.UpdateBindings();
            }
        }));

        public string Status
        {
            get
            {
                return (string)GetValue(StatusProperty);
            }
            set
            {
                SetValue(StatusProperty, value);
            }
        }

        void InitializeStatus()
        {
            DisposeStatus();
            _Status = new List<ValidationStatus>();
        }

        void DisposeStatus()
        {
            if (_Status != null)
            {
                _Status.Clear();

                _Status = null;
            }
        }

        void UpdateStatus()
        {
            InitializeStatus();

            if (!string.IsNullOrEmpty(Status))
            {
                foreach (string s in Status.Split(','))
                {
                    string status = s.Trim();

                    if (!string.IsNullOrEmpty(status))
                    {
                        int i = Array.IndexOf(_StatusStrings, status.ToLower());

                        if (i != -1)
                        {
                            _Status.AddDistinct(_StatusValues[i]);
                        }
                        else if (status == "*")
                        {
                            _Status.AddDistinct(ValidationStatus.Error);
                            _Status.AddDistinct(ValidationStatus.Warning);
                            _Status.AddDistinct(ValidationStatus.Information);

                            break;
                        }
                        else
                        {
                            throw new ArgumentException(string.Format("Status name must be a valid in the range of {{error, err, e, warning, warn, w, information, info, i, *}}: '{0}'", status));
                        }
                    }
                }
            }
        }

        protected List<ValidationStatus> _Status = null;

        static ValidationStatus[] _StatusValues = { ValidationStatus.Error, ValidationStatus.Error, ValidationStatus.Error, ValidationStatus.Warning, ValidationStatus.Warning, ValidationStatus.Warning, ValidationStatus.Information, ValidationStatus.Information, ValidationStatus.Information };
        static string[] _StatusStrings = { "error", "err", "e", "warning", "warn", "w", "information", "info", "i" };
        #endregion

        public abstract void Update();

        public virtual void Dispose()
        {
            if (ValidationSource != null)
            {
                ValidationSource.ValidationInfo.CollectionChanged -= ValidationInfo_CollectionChanged;
            }

            DisposeBindings();
            DisposePropertyNames();
            DisposeStatus();
        }
    }

    internal static class MoreValidationInfo
    {
        public static ValidationStatus Max(this List<ValidationStatus> source)
        {
            ValidationStatus rc = ValidationStatus.None;

            foreach (ValidationStatus vs in source)
            {
                if (vs == ValidationStatus.Error)
                {
                    rc = vs;
                    break;
                }
                else if (vs == ValidationStatus.Warning)
                {
                    rc = vs;
                }
                else if (vs == ValidationStatus.Information)
                {
                    if (rc == ValidationStatus.None)
                    {
                        rc = vs;
                    }
                }
            }

            return rc;
        }

        public static List<ValidationStatus> Matches(this List<ValidationStatus> source, List<ValidationStatus> target)
        {
            List<ValidationStatus> rc = new List<ValidationStatus>();

            foreach (ValidationStatus vs in source)
            {
                if (target.Contains(vs))
                {
                    rc.AddDistinct(vs);
                }
            }

            return rc;
        }

        public static bool SameAs(this List<ValidationStatus> source, List<ValidationStatus> target)
        {
            bool rc = true;

            foreach (ValidationStatus vs in source)
            {
                if (!target.Contains(vs))
                {
                    rc = false;
                    break;
                }
            }

            if (rc)
            {
                foreach (ValidationStatus vs in target)
                {
                    if (!source.Contains(vs))
                    {
                        rc = false;
                        break;
                    }
                }
            }

            return rc;
        }
    }
}
