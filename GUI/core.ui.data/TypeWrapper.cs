using System;
using System.Collections.Generic;
using System.Text;

namespace core.ui.data
{
    public class TypeWrapper : IDisposable
    {
        public string TypeName
        {
            get
            {
                return _TypeName;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _TypeName = value;
                    _Type = _TypeName.FindType();

                    if (_Type == null)
                    {
                        throw new ArgumentException("Invalid type reference. You may want to check your spelling.", "TypeName");
                    }
                }
                else
                {
                    _TypeName = string.Empty;
                    _Type = null;
                }

            }
        }
        private string _TypeName = string.Empty;

        protected Type _Type = null;

        public string FullName
        {
            get
            {
                string rc = string.Empty;

                if (_Type != null)
                {
                    rc = _Type.FullName;
                }

                return rc;
            }
        }

        public override string ToString()
        {
            return FullName;
        }

        public void Dispose()
        {
            _TypeName = null;
            _Type = null;
        }
    }
}
