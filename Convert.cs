using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ES;

namespace JavaScriptInterpreter
{
    // ---------------------------- Преобразование типов ---------------------------//
    public static class Convert
    {
        public static object ToPrimitive(object input, string preferredType)
        {
            if (input is ES.Object)
            {
                return ((ES.Object)input).DefaultValue(preferredType);
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
        //public static NumberType ToNumber(EcmaType x)
        //{

        //    if (x is UndefinedType) return null;// QUESTION/TODO: NaN???!??
        //    if (x is NullType) return new NumberType(0);
        //    if (x is BooleanType)
        //    {
        //        if (((BooleanType)x).Value) return new NumberType(1);
        //        else return new NumberType(0);
        //    }
        //    if (x is NumberType) return (NumberType)x;
        //    if (x is StringType)
        //    {
        //        string str = ((StringType)x).Value;
        //        double number = System.Convert.ToDouble(str);
        //        return new NumberType(number);
        //    }
        //    if (x is Object)
        //    {
        //        EcmaType primValue = ToPrimitive(x, "Number");
        //        return ToNumber(primValue);
        //    }
        //    return null;
        //}
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
