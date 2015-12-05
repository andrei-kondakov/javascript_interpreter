using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ES;

namespace ES
{
    // ---------------------------- Преобразование типов ---------------------------//
    public static class Convert
    {
        public static ES.LanguageType ToPrimitive(ES.LanguageType input, string preferredType)
        {
            if (input is ES.Object)
            {
                throw new NotImplementedException();
                //return ((ES.Object)input).DefaultValue(preferredType);
            }
            else
            {
                return input;
            }
        }
        //public static BooleanType ToBoolean(EcmaType x)
        //{
        //    if (x is UndefinedType) return new BooleanType(false);
        //    if (x is NullType) return new BooleanType(true);
        //    if (x is BooleanType) return (BooleanType)x;
        //    if (x is NumberType)
        //    {
        //        // TODO if +0, -0, NaN = false
        //        return new BooleanType(System.Convert.ToBoolean(((NumberType)x).Value));
        //    }
        //    if (x is StringType)
        //    {
        //        if (((StringType)x).Value.Length == 0) return new BooleanType(false);
        //        else return new BooleanType(true);
        //    }
        //    if (x is ObjectType) return new BooleanType(true);
        //    return new BooleanType(false);
        //}
        public static ES.Number ToNumber(ES.LanguageType x)
        {

            if (x is ES.Undefined) return new ES.Number(Double.NaN);// QUESTION/TODO: NaN???!??
            if (x is ES.Null) return new ES.Number(0);
            if (x is ES.Boolean)
            {
                bool val = ((ES.Boolean)x).Value;
                return new ES.Number(System.Convert.ToDouble(val));
            }
            if (x is ES.Number) return (ES.Number)x;
            if (x is ES.String)
            {
                string str = ((ES.String)x).Value;
                double number = double.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
                return new ES.Number(number);
            }
            if (x is Object)
            {
                throw new NotImplementedException();
                //EcmaType primValue = ToPrimitive(x, "Number");
                //return ToNumber(primValue);
            }
            return null;
        }
        //public static StringType ToString(EcmaType x)
        //{
        //    if (x is UndefinedType) return new StringType("undefined");
        //    if (x is NullType) return new StringType("null");
        //    if (x is BooleanType)
        //    {
        //        if (((BooleanType)x).Value) return new StringType("true");
        //        else return new StringType("false");
        //    }
        //    if (x is NumberType)
        //    {
        //        // TODO NaN, +0, -0
        //        double number = ((NumberType)x).Value;
        //        return new StringType(number.ToString());
        //    }
        //    if (x is ObjectType)
        //    {
        //        EcmaType primValue = ToPrimitive(x, "String");
        //        return ToString(primValue);
        //    }
        //    return null;
        //}
        // TODO IsCallable
        // ToObject
    }

}
