﻿using System;
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
        public static ES.String ToString(ES.LanguageType x)
        {
            if (x is ES.Undefined)
            {
                return new ES.String("undefined");
            }
            if (x is ES.Null)
            {
                return new ES.String("null");
            }
            if (x is ES.Boolean)
            {
                return new ES.String(((ES.Boolean)x).Value.ToString());
            }
            if (x is ES.Number)
            {
                var number = x as ES.Number;
                if (double.IsNaN(number.Value))
                {
                    return new ES.String("NaN");
                }
                else
                {
                    return new ES.String(number.Value.ToString());
                }
            }
            if (x is ES.String)
            {
                return x as ES.String;
            }
            if (x is ES.Object)
            {
                throw new NotImplementedException();
            }
            return null;
        }
        public static ES.Boolean ToBoolean(ES.LanguageType x)
        {
            if (x is ES.Undefined)
            {
                return new ES.Boolean(false);
            }
            if (x is ES.Null)
            {
                return new ES.Boolean(false);
            }
            if (x is ES.Boolean)
            {
                return x as ES.Boolean;
            }
            if (x is ES.Number)
            {
                var number = x as ES.Number;
                return new ES.Boolean(System.Convert.ToBoolean(number.Value));
            }
            if (x is ES.String)
            {
                var str = x as ES.String;
                if (str.Value.Length > 0)
                {
                    return new ES.Boolean(true);
                }
                else
                {
                    return new ES.Boolean(false);
                }
            }
            if (x is ES.Object)
            {
                return new ES.Boolean(true);
            }
            return null;
        }
        // TODO IsCallable
        // ToObject
    }

}
