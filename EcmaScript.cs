using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JavaScriptInterpreter.Types;

namespace JavaScriptInterpreter
{
    public static class EcmaScript
    {
        public static ObjectType GlobalObject;
        public static ObjectType Function;
        public static ObjectType ObjectPrototype; // объект прототип

        public static void Start()
        {
            // 15.1
            GlobalObject = new ObjectType();
            GlobalObject.DefineOwnProperty("undefined", new DataDescriptor(UndefinedType.Value, false, false, false), false);
            Function = new ObjectType();
            ObjectPrototype = new ObjectType();
            ObjectPrototype.Put("__proto__", NullType.Value, false);
            ObjectPrototype.Put("__extensible__", new BooleanType(true), false);
              
        }

        public static bool SameValue(EcmaType x, EcmaType y)
        {
            if (!x.GetType().Equals(y.GetType())) return false;
            if (x is UndefinedType) return true;
            if (x is NullType) return true;
            if (x is NumberType)
            {
                return ((NumberType)x).Value == ((NumberType)y).Value;
            }
            if (x is StringType)
            {
                return ((StringType)x).Value.Equals(((StringType)y).Value);
            }
            if (x is BooleanType)
            {
                return ((BooleanType)x).Value == ((BooleanType)y).Value;
            }
            return x.GetType().Equals(y.GetType());
        }
        // ------------------------ Работа со ссылками --------------------------------//
        public static EcmaType GetBase(EcmaType v)
        {
            return ((Reference)v).BaseValue;
        }
        public static string GetReferenceName(EcmaType v)
        {
            return ((Reference)v).ReferenceName;
        }
        public static bool IsStrictReference(EcmaType v)
        {
            return ((Reference)v).StrictReference;
        }
        public static bool HasPrimitiveBase(EcmaType v)
        {
            EcmaType baseValue = EcmaScript.GetBase(v);
            return baseValue is BooleanType || baseValue is StringType || baseValue is NumberType;
        }
        public static bool IsPropertyReference(EcmaType v)
        {
            return EcmaScript.GetBase(v) is ObjectType || EcmaScript.HasPrimitiveBase(v);
        }
        public static bool IsUnresolvableReference(EcmaType v)
        {
            return EcmaScript.GetBase(v) is UndefinedType;
        }
        //public EcmaType GetValue(EcmaType v)
        //{
        //    if (!(v is Reference)) return v;
        //    EcmaType baseValue = EcmaScript.GetBase(v);
        //    if (EcmaScript.IsUnresolvableReference(v)) throw new Exception("ReferenceError");
        //    //TODO!
        //}

    }
    
}
