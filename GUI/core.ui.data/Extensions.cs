using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using core.ui.data;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Linq;

namespace System
{
    public static class ObjectId
    {
        public static long GetId(this object o)
        {
            long rc = 0;

            if (o != null)
            {
                List<Item> references = null;
                _Lookup.TryGetValue(RuntimeHelpers.GetHashCode(o), out references);

                if (references == null)
                {
                    references = new List<Item>();
                    _Lookup.Add(RuntimeHelpers.GetHashCode(o), references);
                }

                Item item = references.FirstOrDefault(i => i.Reference.Target == o);

                if (item == null)
                {
                    item = new Item() { Reference = new WeakReference(o), Id = ++_Id };
                    references.Add(item);
                }

                rc = item.Id;
            }

            return rc;
        }

        private static long _Id = 0;
        private static Dictionary<int, List<Item>> _Lookup = new Dictionary<int, List<Item>>();

        private class Item
        {
            public WeakReference Reference
            {
                get;
                set;
            }

            public long Id
            {
                get;
                set;
            }
        }
    }

    public static class MorePropertyPath
    {
        public static PropertyPath GetPropertyPath<T>(this object source, Expression<Func<T>> property)
        {
            PropertyPath rc = new PropertyPath(property.GetPropertyName());

            return rc;
        }
    }

    public static class MorePropertyInfo
    {
        public static string GetPropertyName<T>(this Expression<Func<T>> property)
        {
            PropertyInfo propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;

            if (propertyInfo == null)
            {
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
            }

            return propertyInfo.Name;
        }
    }

    public static class MoreFrameworkElement
    {
        public static bool IsInDesignMode(this UIElement source)
        {
            return DesignerProperties.GetIsInDesignMode(source);
        }

        public static bool IsVisible(this FrameworkElement source)
        {
            Visibility visibility = (Visibility) source.GetValue(FrameworkElement.VisibilityProperty);

            return visibility == Visibility.Visible;
        }

        public static void Subscribe(this FrameworkElement source, object o)
        {
            Mediator.Subscribe(source, o);
        }

        public static void Publish<T>(this FrameworkElement fe, T m) where T : Message
        {
            Mediator.Publish<T>(fe, m);
        }
    }

    public static class MoreVisualTreeHelper
    {
        public static DependencyObject Find(this DependencyObject source, string name)
        {
            DependencyObject rc = null;

            if(source != null)
            {
                int count = VisualTreeHelper.GetChildrenCount(source);

                for (int i = 0; i < count && rc == null; i++)
                {
                    DependencyObject d = VisualTreeHelper.GetChild(source, i);
                    FrameworkElement fe = d as FrameworkElement;

                    if (fe != null)
                    {
                        if (fe.Name == name)
                        {
                            rc = fe;
                        }
                        else
                        {
                            rc = fe.Find(name);
                        }
                    }
                    else
                    {
                        rc = d.Find(name);
                    }
                }
            }

            return rc;
        }

        public static DependencyObject Find(this DependencyObject source, Type parent)
        {
            DependencyObject rc = null;
            DependencyObject root = source;

            while (root != null)
            {
                Type t = root.GetType();

                if (t.GetTypeInfo().IsSubclassOf(parent) || t == parent)
                {
                    rc = root;
                    break;
                }

                root = VisualTreeHelper.GetParent(root);
            }

            return rc;
        }
    }

    public static class MoreType
    {
        public static bool ImplementsMessageSink(this Type t)
        {
            MessageSinkInfo msi = MessageSinkInfo.GetMessageSinkInfo(t);

            return msi.Interfaces.Count > 0;
        }

        public static IEnumerable<Type> GetMessageSinks(this Type t)
        {
            MessageSinkInfo msi = MessageSinkInfo.GetMessageSinkInfo(t);

            return msi.Interfaces;
        }

        public static bool IsMessageSink(this Type t)
        {
            bool rc = false;

            if (t != null)
            {
                if (t.GetTypeInfo().IsGenericType)
                {
                    if (typeof(IMessageSink<>).IsAssignableFrom(t.GetGenericTypeDefinition()))
                    {
                        rc = true;
                    }
                }
            }

            return rc;
        }

        public static Type GetMessageType(this Type t)
        {
            Type rc = null;

            if (t != null)
            {
                if (t.IsMessageSink())
                {
                    if (t.GetTypeInfo().IsGenericType)
                    {
                        rc = t.GenericTypeArguments[0];
                    }
                }
            }

            return rc;
        }

        public static bool IsMessage(this Type t)
        {
            bool rc = false;

            if (t != null)
            {
                Type bt = t.GetTypeInfo().BaseType;

                if (bt != null)
                {
                    if (typeof(Message).IsAssignableFrom(bt))
                    {
                        rc = true;
                    }
                }
            }

            return rc;
        }

        public static Type FindType(this string source)
        {
            Type rc = null;

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (TypeInfo t in a.DefinedTypes)
                {
                    if (t.Name == source)
                    {
                        rc = t.AsType();
                        break;
                    }
                }

                if (rc != null)
                {
                    break;
                }
            }

            return rc;
        }

        public static List<Type> FindSubclasses(this Type source)
        {
            List<Type> rc = new List<Type>();

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (TypeInfo t in a.DefinedTypes)
                {
                    if (t.IsSubclassOf(source))
                    {
                        rc.Add(t.AsType());
                    }
                }
            }

            return rc;
        }
    }

    public static class MoreList
    {
        public static bool AddDistinct<T>(this List<T> source, T item)
        {
            bool rc = false;

            if (!source.Contains(item))
            {
                rc = true;
                source.Add(item);
            }

            return rc;
        }

        public static void RemoveRange<T>(this List<T> source, List<T> children)
        {
            foreach (T child in children)
            {
                source.Remove(child);
            }
        }
    }

    public static class MoreString
    {
        public static ViewContract ToViewContract(this string source)
        {
            ViewContract rc = null;

            rc = new ViewContract() { TypeName = source };

            return rc;
        }

        public static ViewModelBase ToViewModel(this string source)
        {
            ViewModelBase rc = null;
            Type t = source.FindType();

            rc = Activator.CreateInstance(t) as ViewModelBase;

            return rc;
        }

        static bool In(this char source, UnicodeCategory[] categories)
        {
            bool rc = false;

            var category = Char.GetUnicodeCategory(source);

            foreach (UnicodeCategory current in categories)
            {
                if (current == category)
                {
                    rc = true;
                    break;
                }
            }

            return rc;
        }

        static bool In(this string source, UnicodeCategory[] categories)
        {
            bool rc = true;

            foreach (char c in source.ToCharArray())
            {
                if (!c.In(categories))
                {
                    rc = false;
                    break;
                }
            }

            return rc;
        }

        public static bool IsValidIdentifier(this string source)
        {
            bool rc = false;

            UnicodeCategory[] letter = { 
                                           UnicodeCategory.UppercaseLetter, 
                                           UnicodeCategory.LowercaseLetter, 
                                           UnicodeCategory.TitlecaseLetter, 
                                           UnicodeCategory.ModifierLetter, 
                                           UnicodeCategory.OtherLetter 
                                       };

            char underscore = '_';

            UnicodeCategory[] parts = { 
                                        UnicodeCategory.UppercaseLetter, 
                                        UnicodeCategory.LowercaseLetter, 
                                        UnicodeCategory.TitlecaseLetter, 
                                        UnicodeCategory.ModifierLetter, 
                                        UnicodeCategory.OtherLetter,
                                        UnicodeCategory.LetterNumber, 
                                        UnicodeCategory.NonSpacingMark, 
                                        UnicodeCategory.SpacingCombiningMark, 
                                        UnicodeCategory.DecimalDigitNumber, 
                                        UnicodeCategory.ConnectorPunctuation, 
                                        UnicodeCategory.Format
                                    };

            if(!string.IsNullOrEmpty(source))
            {
                if (source[0] == underscore || source[0].In(letter))
                {
                    if (source.In(parts))
                    {
                        rc = true;
                    }
                }
            }

            return rc;
        }
    }

    public static class MorePropertyChangedEventArgs
    { 
        public static bool HasPropertyChanged<T>(this PropertyChangedEventArgs source, Expression<Func<T>> property)
        {
            bool rc = false;

            rc = string.IsNullOrEmpty(source.PropertyName) || source.PropertyName == property.GetPropertyName();

            return rc;
        }

        public static bool HasPropertyChanged(this PropertyChangedEventArgs source, PropertyInfo property)
        {
            bool rc = false;

            rc = string.IsNullOrEmpty(source.PropertyName) || source.PropertyName == property.Name;

            return rc;
        }
    }

    public static class MoreObservableCollection
    {
        public static void AddRange<T>(this ObservableCollection<T> source, List<T> items)
        {
            foreach (T item in items)
            {
                source.Add(item);
            }
        }
    }
}
