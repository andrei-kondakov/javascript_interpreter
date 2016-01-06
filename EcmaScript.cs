﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JavaScriptInterpreter.Types;

namespace JavaScriptInterpreter
{
    public static class EcmaScript
    {
        public static Stack<ExecutionContext> ExecutionContexts;
        public static void EnterInGlobalCode()
        {
            ObjectType globalObject = new ObjectType();
            globalObject.InternalProperties["prototype"] = "object";
            globalObject.InternalProperties["class"] = "global_object";
            globalObject.DefineOwnProperty("global", new PropertyDescriptorType(globalObject, false, false, false), true);
            LexicalEnvironment globalEnvironment = NewObjectEnvironment(globalObject, null);
            ExecutionContext globalExceutionContext = new ExecutionContext(globalEnvironment, globalObject);
            ExecutionContexts.Push(globalExceutionContext);
            //ThisBinding = globalObject;
            //http://es5.javascript.ru/x10.html#outer-environment-reference
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
                ((EnvironmentRecord)baseValue).GetBindingValue(GetReferenceName(val), IsStrictReference(val));
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
        public static LexicalEnvironment NewDeclarativeEnvironment(LexicalEnvironment outer)
        {
            return new LexicalEnvironment(new DeclarativeEnviromentRecord(), outer);
        }
        public static LexicalEnvironment NewObjectEnvironment(ObjectType obj, LexicalEnvironment outer)
        {
            return new LexicalEnvironment(new ObjectEnviromentRecord(obj), outer);
        }
        
    }
    public struct ExecutionContext
    {
        public LexicalEnvironment Environment;
        object ThisBinding;
        public ExecutionContext(LexicalEnvironment env, object thisBinding)
        {
            this.Environment = env;
            this.ThisBinding = thisBinding;
        }
    }
}
