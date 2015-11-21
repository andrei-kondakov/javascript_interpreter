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

        public static void EnterInGlobalCode()
        {
            // 15.1
            GlobalObject = new ObjectType();
            //GlobalObject.DefineOwnProperty("undefined", new DataDescriptor(UndefinedType.Value, false, false, false), false);
            Function = new ObjectType();
            ObjectPrototype = new ObjectType();
            //ObjectPrototype.Put("__proto__", NullType.Value, false);
            //ObjectPrototype.Put("__extensible__", new BooleanType(true), false);
              
        }

        public static bool SameValue(object x, object y)
        {
            if (!x.GetType().Equals(y.GetType())) return false; // QUESTION TODO TEST
            if (x.Equals(EcmaTypes.UNDEFINED)) return true;
            if (x.Equals(EcmaTypes.NULL)) return true;
            if (x is NumberType)
            {
                return ((NumberType)x).Value == ((NumberType)y).Value;
            }
            if (x is string)
            {
                return x.Equals(y);
            }
 
            if (IsBooleanType(x))
            {
                if ((x.Equals(EcmaTypes.TRUE) && y.Equals(EcmaTypes.TRUE))
                    || (x.Equals(EcmaTypes.FALSE) && y.Equals(EcmaTypes.FALSE)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // TODO: отнсоятся к одному и то му же объекту
                return x.GetType().Equals(y.GetType());
            }
        }
        // ------------------------ Работа со ссылками --------------------------------//
        public static object GetBase(Reference v)
        {
            return v.BaseValue;
        }
        public static string GetReferenceName(Reference v)
        {
            return v.ReferenceName;
        }
        public static bool IsStrictReference(Reference v)
        {
            return v.StrictReference;
        }
        public static bool HasPrimitiveBase(Reference v)
        {
            var baseValue = GetBase(v);
            return IsBooleanType(baseValue) || baseValue is string || baseValue is NumberType;
        }
        public static bool IsPropertyReference(Reference v)
        {
            return GetBase(v) is ObjectType || EcmaScript.HasPrimitiveBase(v);
        }
        public static bool IsUnresolvableReference(Reference v)
        {
            return GetBase(v).Equals(EcmaTypes.UNDEFINED);
        }
        public static object GetValue(object v)
        {
            if (!(v is Reference)) return v;
            Reference val = (Reference)v;
            var baseValue = GetBase(val);
            if (IsUnresolvableReference(val)) throw new Exception("ReferenceError");
            if (IsPropertyReference(val))
            {
                throw new NotImplementedException();
                if (HasPrimitiveBase(val) == false)
                {
                    // TODO http://es5.javascript.ru/x8.html#x8.7.1
                    
                }
                else
                {

                }
            }
            else
            {
                
            }
            return null;
        }
        // ------------------------ Работа с дексрипторами --------------------------------//
        public static bool IsDataDescriptor(object desc)
        {
            if (desc.Equals(EcmaTypes.UNDEFINED))
            {
                return false;
            }
            PropertyDescriptorType propDesc = (PropertyDescriptorType)desc;
            if (propDesc.Attributes["value"] == null && propDesc.Attributes["writable"] == null)
            {
                return false;
            }
            return true;
        }
        public static bool IsAcessorDescriptor(object desc)
        {
            if (desc.Equals(EcmaTypes.UNDEFINED))
            {
                return false;
            }
            PropertyDescriptorType propDesc = (PropertyDescriptorType)desc;
            if (propDesc.Attributes["get"] == null && propDesc.Attributes["set"] == null)
            {
                return false;
            }
            return true;
        }
        public static bool IsGenericDescriptor(object desc)
        {
            if (desc.Equals(EcmaTypes.UNDEFINED))
            {
                return false;
            }
            PropertyDescriptorType propDesc = (PropertyDescriptorType)desc;
            if (EcmaScript.IsDataDescriptor(propDesc)==false
                && EcmaScript.IsAcessorDescriptor(propDesc)==false)
            {
                return true;
            }
            return false;
        }
        public static bool IsBooleanType(object value)
        {
            return value.Equals(EcmaTypes.TRUE) || value.Equals(EcmaTypes.FALSE);
        }
        // ------------------------ Работа с лексическим окружением --------------------------------//
        public static LexicalEnvironment NewDeclarativeEnvironment(LexicalEnvironment env)
        {
            return new LexicalEnvironment(new DeclarativeEnviromentRecord(), env);
        }
        public static LexicalEnvironment NewObjectEnvironment(ObjectType obj, LexicalEnvironment env)
        {
            return new LexicalEnvironment(new ObjectEnviromentRecord(obj), env);
        }
    }
    

}
