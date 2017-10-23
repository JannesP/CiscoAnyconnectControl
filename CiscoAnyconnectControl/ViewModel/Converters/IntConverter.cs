using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.ViewModel.Converters
{
    class IntConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var parsedInt = 0;
            if (value is string)
            {
                if (!int.TryParse((string)value, out parsedInt))
                {
                    throw new NotSupportedException();
                }

            }
            else if (value is int)
            {
                parsedInt = (int)value;
            }
            return parsedInt;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || sourceType == typeof(int);
        }
    }
}
