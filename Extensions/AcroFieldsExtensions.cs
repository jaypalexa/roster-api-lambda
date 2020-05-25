using System;
using iTextSharp.text.pdf;

namespace RosterApiLambda.Extensions
{
    public static class AcroFieldsExtensions
    {
        public static void SetField(this AcroFields acroFields, string name, object value)
        {
            acroFields.SetField(name, Convert.ToString(value));
        }
    }
}
