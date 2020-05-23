using System;
using iTextSharp.text.pdf;

namespace RosterApiLambda.Extensions
{
    public static class AcroFieldsExtensions
    {
        public static void SetField(this AcroFields acroFields, string name, int value)
        {
            acroFields.SetField(name, Convert.ToString(value));
        }
    }
}
