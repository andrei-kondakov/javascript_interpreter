using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using JavaScriptInterpreter;

namespace ES
{
    public static class EcmaTypes
    {
        public static ES.Boolean Equal(LanguageType lval, LanguageType rval)
        {
            if (lval.GetType().Equals(rval.GetType()))
            {
                if (lval is ES.Undefined)
                {
                    return new ES.Boolean(true);
                }
                if (lval is ES.Null)
                {
                    return new ES.Boolean(true);
                }
                if (lval is ES.Number)
                {
                    var x = lval as ES.Number;
                    var y = rval as ES.Number;
                    if (double.IsNaN(x.Value))
                    {
                        return new ES.Boolean(false);
                    }
                    if (double.IsNaN(y.Value))
                    {
                        return new ES.Boolean(false);
                    }
                    if (x.Value == y.Value)
                    {
                        return new ES.Boolean(true);
                    }
                    return new ES.Boolean(false);
                }
                if (lval is ES.String)
                {
                    var x = lval as ES.String;
                    var y = rval as ES.String;
                    return new ES.Boolean(x.Value.Equals(y.Value));
                }
                if (lval is ES.Boolean)
                {
                    var x = lval as ES.Boolean;
                    var y = rval as ES.Boolean;
                    return new ES.Boolean(x.Value == y.Value);
                }
                if (lval is ES.Object)
                {
                    throw new NotImplementedException();
                }
            }
            if (lval is ES.Null && rval is ES.Undefined) return new ES.Boolean(true);
            if (lval is ES.Undefined && rval is ES.Null) return new ES.Boolean(true);
            if (lval is ES.Number && rval is ES.String)
            {
                return EcmaTypes.Equal(lval, ES.Convert.ToNumber(rval));
            }
            if (lval is ES.String && rval is ES.Number)
            {
                return EcmaTypes.Equal(ES.Convert.ToNumber(lval), rval);
            }
            if (lval is ES.Boolean)
            {
                return EcmaTypes.Equal(ES.Convert.ToNumber(lval), rval);
            }
            if (rval is ES.Boolean)
            {
                return EcmaTypes.Equal(lval, ES.Convert.ToNumber(rval));
            }
            if ((lval is ES.String) || (lval is ES.Number) && rval is Object)
            {
                return EcmaTypes.Equal(lval, ES.Convert.ToPrimitive(rval, null));
            }
            if (lval is ES.Object && (rval is ES.String || rval is ES.Number))
            {
                return EcmaTypes.Equal(ES.Convert.ToPrimitive(lval, null), rval);
            }
            return new ES.Boolean(false);
        }
        public static ES.LanguageType Less(ES.LanguageType lval, ES.LanguageType rval, bool leftFirst)
        {
            ES.LanguageType x, y;
            if (leftFirst == true)
            {
                x = ES.Convert.ToPrimitive(lval, "Number");
                y = ES.Convert.ToPrimitive(rval, "Number");
            }
            else
            {
                y = ES.Convert.ToPrimitive(rval, "Number");
                x = ES.Convert.ToPrimitive(lval, "Number");
            }
            if (!(x is ES.String && y is ES.String))
            {
                var nx = ES.Convert.ToNumber(x);
                var ny = ES.Convert.ToNumber(y);
                if (double.IsNaN(nx.Value)) return ES.Undefined.Value;
                if (double.IsNaN(ny.Value)) return ES.Undefined.Value;
                if (nx.Value == ny.Value) return new ES.Boolean(false);
                return new ES.Boolean(nx.Value < ny.Value);
            }
            throw new NotImplementedException();
            //var sx = ES.Convert.ToString(x);
            //var sy = ES.Convert.ToString(y);
            //return new ES.Boolean(sx.Value < sy.Value);
        }
        
        //public static bool SameValue(object x, object y)
        //{
        //    if (!x.GetType().Equals(y.GetType())) return false; // QUESTION TODO TEST
        //    if (x.Equals(Undefined.Value)) return true;
        //    if (x.Equals(ES.Null.Value)) return true;
        //    if (x is ES.Number)
        //    {
        //        return ((Number)x).Value == ((Number)y).Value;
        //    }
        //    if (x is string)
        //    {
        //        return x.Equals(y);
        //    }

        //    if (IsBooleanType(x))
        //    {
        //        if ((x.Equals(ES.Boolean.True) && y.Equals(ES.Boolean.True))
        //            || (x.Equals(ES.Boolean.False) && y.Equals(ES.Boolean.False)))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        // TODO: отнсоятся к одному и то му же объекту
        //        return x.GetType().Equals(y.GetType());
        //    }
        //}
        //public static bool IsBooleanType(object value)
        //{
        //    return value.Equals(ES.Boolean.True) || value.Equals(ES.Boolean.False);
        //}
        
    }
    public abstract class Type { }


    #region Language_types
    public abstract class LanguageType : Type
    { }
    public class Undefined : LanguageType
    {
        static Undefined value = null;
        private Undefined() { }
        public static Undefined Value
        {
            get
            {
                if (value == null)
                {
                    value = new Undefined();
                }
                return value;
            }
        }
        public override string ToString()
        {
            return "undefined";
        }
    }
    public class Null : LanguageType
    {
        private static Null value = null;
        private Null() { }
        public static Null Value
        {
            get
            {
                if (value == null)
                {
                    value = new Null();
                }
                return value;
            }
        }
    }
    public class Boolean : LanguageType
    {
        public bool Value;

        public Boolean (bool value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

    }
        public class Number : LanguageType
    {
        public double Value;
        //private bool posInfinity;
        //private bool negInfinity;
        //private bool NaN;
        // TODO:    подумать над реализацией специальных значений 
        //          положительная бесконечность, отриц. бесконечность,
        //          Not a Number (NaN)
        public Number(double value)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
    public class String : LanguageType
    {
        public string Value;
        public String(string value)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return "'" + Value + "'";
        }
    }
    public class Object : LanguageType
    {
        // QUESTION m.b. Dicitipnary<string, PropertyDescriptroType> ??!??
        private Dictionary<string, PropertyDescriptor> namedProperties;   // ассоциирует имя со значением и набором булевых атрибутов
        public Dictionary<string, object> InternalProperties;      // внутренние свойства
        /*  Внутренние свойства каждого объекта */
        /*private EcmaType prototype;     // Прототип данного объекта [Object/NULL]
        private string __class__;          // Классифицаия объектов
        private bool extensible; // Если true, к объекту могут быть добавлены собственные свойства*/
        /* ------------------------------------*/

        public Object()
        {
            namedProperties = new Dictionary<string, PropertyDescriptor>();
            InternalProperties = new Dictionary<string, object>();
            InternalProperties["extensible"] = true;
            InternalProperties["class"] = "Object";
        }
        public override string ToString()
        {
            string result = base.ToString();
            result += "{";
            foreach (KeyValuePair<string, PropertyDescriptor> kvp in namedProperties)
            {
                result += kvp.Key + ": " + kvp.Value + ", ";
            }
            result = result.Remove(result.Length - 2);
            result += "}";
            return result;
        }

        /* Внутренние методы каждого объекта */

        /// <summary>
        /// Возвращает дескриптор именнованного свойства данного объекта
        /// или undefined (в случае отсутствия)
        /// </summary>
        /// <param name="propertyName">Название свойства</param>
        /// <returns>UNDEFINED/PROPERTY_DESCRIPTOR</returns>
        public Type GetOwnProperty(string propertyName) // возвращает дескриптор свойства
        {
            if (!namedProperties.ContainsKey(propertyName))
                return Undefined.Value;
            PropertyDescriptor property, result;
            property = namedProperties[propertyName];
            result = new PropertyDescriptor();
            if (PropertyDescriptor.IsDataDescriptor(property))
            {
                result.Attributes["value"] = property.Attributes["value"];
                result.Attributes["writable"] = property.Attributes["writable"];
            }
            else
            {
                result.Attributes["get"] = property.Attributes["get"];
                result.Attributes["set"] = property.Attributes["set"];
            }
            result.Attributes["enumerable"] = property.Attributes["enumerable"];
            result.Attributes["configurable"] = property.Attributes["configurable"];
            return result;
        }
        /// <summary>
        /// Возвращает полностью заполненный дескриптор свойства данного объекта
        /// или undefined (в случае отсутствия)
        /// </summary>
        /// <param name="propertyName">Название свойства</param>
        /// <returns>Undefined/PropertyDescriptorType</returns>
        public Type GetProperty(string propertyName) 
        {
            var property = GetOwnProperty(propertyName);
            if (!(property is Undefined))
            {
                return property;
            }
            else
            {
                var prototype = InternalProperties["prototype"];
                if (property is Undefined)
                {
                    return Undefined.Value; 
                }
                if (prototype.Equals(ES.Null.Value))
                {
                    return Undefined.Value;
                }
                return ((ES.Object)prototype).GetProperty(propertyName);
            }
        }

        // get - возвращает значение именнованного свойства
        /// <summary>
        /// Возвращает значение свойства
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns></returns>
        public LanguageType Get(string propertyName)   
        {
            var desc = GetProperty(propertyName);
            if (desc is Undefined) return Undefined.Value;
            if (PropertyDescriptor.IsDataDescriptor(desc))
            {
                return (LanguageType)((PropertyDescriptor)desc).Attributes["value"];
            }
            else //if (EcmaScript.IsAcessorDescriptor(desc))
            {
                throw new NotImplementedException("Get for acessor descriptor");
                //var getter = ((PropertyDescriptor)desc).Attributes["get"];
                //if (getter is Undefined) return Undefined.Value;
                //return null; // TODO: вызвать внутренний метод CALL для getter передавая О в качестве значения this и непередавая никаких аргументов
            }
        }

        // canPut - Возвращает булево значение, означающее возможность выполнения операции Put с именем свойства propertyName.
        /// <summary>
        /// Проверяет можно ли выполнить операцию Put с аргументом propertyName
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns>true/false</returns>
        public bool CanPut(string propertyName)
        {
            var desc = GetOwnProperty(propertyName);
            if (!(desc is Undefined))
            {
                if (PropertyDescriptor.IsAcessorDescriptor(desc))
                {
                    if (((PropertyDescriptor)desc).Attributes["set"] is Undefined)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return (bool)((PropertyDescriptor)desc).Attributes["writable"];
                }
            }
            var prototype = InternalProperties["prototype"];
            if (prototype is ES.Null)
            {
                return (bool)InternalProperties["extensible"];
            }
            var inherited = ((ES.Object)prototype).GetProperty(propertyName);
            if (inherited is Undefined)
            {
                return (bool)InternalProperties["extensible"];
            }
            if (PropertyDescriptor.IsAcessorDescriptor(inherited))
            {
                throw new NotImplementedException("Can put for acessor descriptor");
                //if (((PropertyDescriptor)inherited).Attributes["set"] is Undefined)
                //{
                //    return false;
                //}
                //else
                //{
                //    return true;
                //}
            }
            else
            {
                if ((bool)InternalProperties["extensible"] == false)
                {
                    return false;
                }
                else
                {
                    return (bool)((PropertyDescriptor)inherited).Attributes["writable"];
                }
            }
        }

        /// <summary>
        /// Присваивает значение именнованному свойству
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="value">Значение</param>
        /// <param name="needThrow">Обработка отказов</param>
        /// <returns>Результат</returns>

        public bool Put(string propertyName, LanguageType value, bool needThrow)
        {
            bool possibleToPut = CanPut(propertyName);
            if (!possibleToPut)
            {
                if (needThrow)
                {
                    throw new Exception("TypeError");
                }
                else
                {
                    return possibleToPut;
                }
            }
            var ownDesc = GetOwnProperty(propertyName);
            if (PropertyDescriptor.IsDataDescriptor(ownDesc))
            {
                PropertyDescriptor valueDesc = new PropertyDescriptor();
                valueDesc.Attributes["value"] = value;
                return DefineOwnProperty(propertyName, valueDesc, needThrow);
            }
            var desc = GetProperty(propertyName);
            if (PropertyDescriptor.IsAcessorDescriptor(desc))
            {
                throw new NotImplementedException("Put for acessor descrptor");
                //var setter = ((PropertyDescriptor)desc).Attributes["set"];
                //// QUESTION: почему setter не может быть undefined?
                //// TODO 8.12.5 5 пункт
                //return false; // NOT IMPLEMENTED!!
            }
            else
            {
                PropertyDescriptor newDesc = new PropertyDescriptor();
                newDesc.Attributes["value"] = value;
                newDesc.Attributes["writable"] = true;
                newDesc.Attributes["enumerable"] = true;
                newDesc.Attributes["configurable"] = true;
                return DefineOwnProperty(propertyName, newDesc, needThrow);
            }
        }

        /// <summary>
        /// Есть ли у данного объекта свойство с таким именем
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns></returns>
        public bool HasProperty(string propertyName)
        {
            var desc = GetProperty(propertyName);
            if (desc is Undefined) return false;
            return true;
        }
       
        /// <summary>
        /// Удаляет из объекта указанное именнованное свойство
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="needThrow">Обработка отказов</param>
        /// <returns></returns>
        public bool Delete(string propertyName, bool needThrow)
        {
            var desc = GetOwnProperty(propertyName);
            if (desc is Undefined) return true;
            if ((bool)((PropertyDescriptor)desc).Attributes["configurable"])
            {
                namedProperties.Remove(propertyName);
                return true;
            }
            else
            {
                if (needThrow) throw new Exception("TypeError");
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="hint">Подсказка</param>
        /// <returns>Значение по умолчанию для данного объекта</returns>
        public object DefaultValue(string hint)
        {
            // TODO результат вызова 8.12.8
            return null;
        }



        // TODO: 8.12.9
        // defineOwnProperty - алгоритм содержит шаги, проверяющие различны поля Desc Property Descriptor 
        // на наличие определенных значений. Проверяемые таким образом поля не обязательно должны существовать в Desc.
        // Если поле отсутствует, его значение считается false.
        /// <summary>
        /// Создает или изменяет именнованное собственное свойство так, чтобы оно имело состояние описанное
        /// дискриптором свойства.
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="descriptor">Дескриптор свойства</param>
        /// <param name="needThrow">Обработка отказов</param>
        /// <returns></returns>
        public bool DefineOwnProperty(string propertyName, PropertyDescriptor descriptor, bool needThrow)
        {
            Type current = GetOwnProperty(propertyName);
            bool extensible = (bool)InternalProperties["extensible"];
            if (current is Undefined && extensible == false)
            {
                if (needThrow) throw new Exception("TypeError");
                return false;
            }
            if (current is Undefined && extensible)
            {
                PropertyDescriptor newDescriptor;
                if (PropertyDescriptor.IsGenericDescriptor(descriptor) || PropertyDescriptor.IsDataDescriptor(descriptor))
                {
                    newDescriptor = new PropertyDescriptor();
                    var value = descriptor.Attributes["value"];
                    var writable = descriptor.Attributes["writable"];
                    if (value != null)
                    {
                        newDescriptor.Attributes["value"] = value;
                    }
                    else
                    {
                        newDescriptor.SetDefaultValue("value");
                    }
                    if (writable != null)
                    {
                        newDescriptor.Attributes["writable"] = writable;
                    }
                    else
                    {
                        newDescriptor.SetDefaultValue("writable");
                    }
                }
                else
                {
                    throw new NotImplementedException("DefineOwnProperty for acessors descriptor");
                    //newDescriptor = new PropertyDescriptor(DescriptorType.ACCESSOR);
                    //PropertyDescriptor acessorDescriptor = new PropertyDescriptor(DescriptorType.ACCESSOR);
                    //var get = descriptor.Attributes["get"];
                    //var set = descriptor.Attributes["set"];
                    //if (get != null) newDescriptor.Attributes["get"] = get;
                    //if (set != null) newDescriptor.Attributes["set"] = set;
                }
                var enumerable = descriptor.Attributes["enumerable"];
                var configurable = descriptor.Attributes["configurable"];
                if (enumerable != null)
                {
                    newDescriptor.Attributes["enumerable"] = enumerable;
                }
                else
                {
                    newDescriptor.SetDefaultValue("enumerable");
                }
                if (configurable != null)
                {
                    newDescriptor.Attributes["configurable"] = configurable;
                }
                else
                {
                    newDescriptor.SetDefaultValue("configurable");
                }
                namedProperties.Add(propertyName, newDescriptor);
                return true;
            }
            if (descriptor.Attributes.Count == 0)
            {
                return true;
            }
            // same - true, если все поля в descriptor также встречаются в current,
            // и значение каждого поля в Desc оказывается таким же, 
            // что и у соответствующего поля в current (TODO? при сравнении с помощью алгоритма SameValue (9.12).)
            // QUESTION TODO TEST! Срочно проверить будет ли работать
            bool same = descriptor.Attributes.Cast<DictionaryEntry>()
                .Union(((PropertyDescriptor)current).Attributes.Cast<DictionaryEntry>()).Count() == descriptor.Attributes.Count;
            if (same) return true;
            if ((bool)((PropertyDescriptor)current).Attributes["configurable"] == false)
            {
                if (descriptor.Attributes.ContainsKey("configurable") 
                    && (bool)descriptor.Attributes["configurable"])
                {
                    if (needThrow) throw new Exception("TypeError");
                    return false;
                }
                // Булево отрицание = просто неравенство двух значений?
                if (descriptor.Attributes.ContainsKey("enumerable") 
                    && ((PropertyDescriptor)current).Attributes["enumerable"] != descriptor.Attributes["enumerable"])
                {
                    if (needThrow) throw new Exception("TypeError");
                    return false;
                }
            }
            if (!PropertyDescriptor.IsGenericDescriptor(descriptor))
            {
                if (PropertyDescriptor.IsDataDescriptor(current) != PropertyDescriptor.IsDataDescriptor(descriptor))
                {
                    if ((bool)((PropertyDescriptor)current).Attributes["configurable"] == false)
                    {
                        if (needThrow) throw new Exception("TypeError");
                        return false;
                    }
                    throw new NotImplementedException("Transfer from DataDescrpitor to Acessor descriptor");
                    //// QUESTION TEST
                    //PropertyDescriptor oldDescriptor = (PropertyDescriptor)GetProperty(propertyName);
                    //PropertyDescriptor newDescriptor;
                    //Delete(propertyName, needThrow); // QUESTION TEST
                    //if (PropertyDescriptor.IsDataDescriptor(current))
                    //{
                    //    newDescriptor = new PropertyDescriptor(DescriptorType.ACCESSOR);
                    //}
                    //else
                    //{
                    //    newDescriptor = new PropertyDescriptor(DescriptorType.DATA);
                    //}
                    //newDescriptor.Attributes["configurable"] = oldDescriptor.Attributes["configurable"];
                    //newDescriptor.Attributes["enumerable"] = oldDescriptor.Attributes["enumerable"];
                }
                else if (PropertyDescriptor.IsDataDescriptor(current) && PropertyDescriptor.IsDataDescriptor(descriptor))
                {
                    if ((bool)((PropertyDescriptor)current).Attributes["configurable"] == false)
                    {
                        if ((bool)((PropertyDescriptor)current).Attributes["writable"] == false
                            && (bool)((PropertyDescriptor)descriptor).Attributes["writable"] == true)
                        {
                            if (needThrow) throw new Exception("TypeError");
                            return false;
                        }
                        if ((bool)((PropertyDescriptor)current).Attributes["writable"] == false)
                        {
                            throw new NotImplementedException("TEST SAME VALUE!");
                            //// SameValue or Equal??
                            //// TODO !!!
                            //if (descriptor.Attributes["value"] != null
                            //    && EcmaTypes.SameValue(descriptor.Attributes["value"], ((PropertyDescriptor)current).Attributes["value"]) == false)
                            //{
                            //    if (needThrow) throw new Exception("TypeError");
                            //    return false;
                            //}
                        }
                    }
                }
                else if (PropertyDescriptor.IsAcessorDescriptor(current) && PropertyDescriptor.IsAcessorDescriptor(descriptor))
                {
                    throw new NotImplementedException("DefineOwnProperty for acessors descriptor");
                    //if ((bool)((PropertyDescriptor)current).Attributes["configurable"] == false)
                    //{
                    //    if (descriptor.Attributes["set"] != null
                    //            && EcmaTypes.SameValue(descriptor.Attributes["set"], ((PropertyDescriptor)current).Attributes["set"]) == false)
                    //    {
                    //        if (needThrow) throw new Exception("TypeError");
                    //        return false;
                    //    }
                    //    if (descriptor.Attributes["get"] != null
                    //            && EcmaTypes.SameValue(descriptor.Attributes["get"], ((PropertyDescriptor)current).Attributes["get"]) == false)
                    //    {
                    //        if (needThrow) throw new Exception("TypeError");
                    //        return false;
                    //    }
                    //}
                }
            }

            PropertyDescriptor prop = namedProperties[propertyName];
            foreach(string key in descriptor.Attributes.Keys)
            {
                prop.Attributes[key] = descriptor.Attributes[key];
            }
            return true;
        }
        /* ------------------------------------*/
    }
    #endregion

    #region Specification_types
    public abstract class SpecificationType : Type { }
    public class PropertyDescriptor : SpecificationType
    {
        public Hashtable Attributes;

        public PropertyDescriptor()
        {
            Attributes = new Hashtable();
        }
        public void SetDefaultValue(string attribute)
        {
            switch (attribute)
            {
                case "enumerable":              // Если true, данное свойство можно перечислить с помощью for-in
                    {
                        Attributes["enumerable"] = false;
                        break;
                    }
                case "configurable":            // Если false, нельзя будет удалить данное свойство, измнеить его, сделав свойством-аксессором или изменить его атрибуты (кроме Value)
                    {
                        Attributes["configurable"] = false;
                        break;
                    }
                case "value":
                    {
                        Attributes["value"] = Undefined.Value;
                        break;
                    }
                case "writable":
                    {
                        Attributes["writable"] = false;
                        break;
                    }
                case "get":
                    {
                        Attributes["get"] = Undefined.Value;
                        break;
                    }
                case "set":
                    {
                        Attributes["set"] = Undefined.Value;
                        break;
                    }
                default:
                    throw new Exception("Error: Have not a default value for attribute: " + attribute);
            }
        }
        public override string ToString()
        {
            object value = Attributes["value"];
            if (Attributes["value"] != null)
            {
                if (value is ES.Object)
                {
                    return base.ToString();
                }
                else
                {
                    return value.ToString();
                }
                
            }
            return base.ToString();
        }
        // ------------------------ Работа с дексрипторами --------------------------------//
        public static bool IsDataDescriptor(Type desc)
        {
            if (desc is Undefined)
            {
                return false;
            }
            PropertyDescriptor propDesc = (PropertyDescriptor)desc;
            if (propDesc.Attributes["value"] == null && propDesc.Attributes["writable"] == null)
            {
                return false;
            }
            return true;
        }
        public static bool IsAcessorDescriptor(Type desc)
        {
            if (desc is Undefined)
            {
                return false;
            }
            PropertyDescriptor propDesc = (PropertyDescriptor)desc;
            if (propDesc.Attributes["get"] == null && propDesc.Attributes["set"] == null)
            {
                return false;
            }
            return true;
        }
        public static bool IsGenericDescriptor(Type desc)
        {
            if (desc is Undefined)
            {
                return false;
            }
            PropertyDescriptor propDesc = (PropertyDescriptor)desc;
            if (IsDataDescriptor(propDesc) == false
                && IsAcessorDescriptor(propDesc) == false)
            {
                return true;
            }
            return false;
        }
        
    }
    public class Reference : SpecificationType
    {
        private Type baseValue;     // база Undefined, BooleanType, StringType, NumberType, или EnvironmentRecord
        private string referenceName;   // имя ссылки
        private bool strictReference;   // строгая ссылка

        public Reference(Type baseValue, string referenceName, bool strictReference)
        {
            this.baseValue = baseValue;
            this.referenceName = referenceName;
            this.strictReference = strictReference;
        }
        public override string ToString()
        {
            var value = baseValue as LanguageType;
            if (value != null)
            {
                return ES.Convert.ToPrimitive(value, null).ToString();
            }
            else
            {
                var objectEnvironment = baseValue as ObjectEnviroment;
                if (objectEnvironment != null)
                {
                    return objectEnvironment.BindingObject.Get(referenceName).ToString();
                }
                return base.ToString();
            }
        }

        //public object BaseValue { get; set; }
        //public string ReferenceName { get; set; }
        //public bool StrictReference { get; set; }
        // ------------------------ Работа со ссылками --------------------------------//
        public Type GetBase()
        {
            return baseValue;
        }
        public string GetReferenceName()
        {
            return referenceName;
        }
        public bool IsStrictReference()
        {
            return strictReference;
        }
        public bool HasPrimitiveBase()
        {
            return baseValue is ES.Boolean || baseValue is ES.String || baseValue is ES.Number;
        }
        public bool IsPropertyReference()
        {
            return this.GetBase() is ES.Object || this.HasPrimitiveBase();
        }
        public bool IsUnresolvableReference()
        {
            return this.GetBase() is Undefined;
        }
    }
    public abstract class Binding : SpecificationType
    {
        public LanguageType Value;
        public Binding(LanguageType value)
        {
           this.Value = value;
        }
    }
    public class MutableBinding : Binding
    {
        protected bool canBeDeleted;

        public MutableBinding(LanguageType value, bool canBeDeleted)
            : base(value)
        {
            this.canBeDeleted = canBeDeleted;
        }
        public bool CanBeDeleted { get; set; }
    }
    public class ImmutableBinding : Binding
    {
        protected bool initialized;
        public ImmutableBinding(LanguageType value)
            : base(value)
        {
            this.initialized = false;
        }
        public bool Initialized { get; set; }
    }
    public abstract class EnvironmentRecord : SpecificationType
    {
        /// <summary>
        /// Ссылка на внешнее окружение
        /// </summary>
        public EnvironmentRecord Outer;
        /// <summary>
        /// Определяет, имеет ли запись окружения привязку к индентификатору
        /// </summary>
        /// <param name="name">Текст индентификатора</param>
        /// <returns></returns>
        public abstract bool HasBinding(string name);
        
        /// <summary>
        /// Создает в записи окружения новую изменяемую привязку.
        /// </summary>
        /// <param name="name">Привязанное имя</param>
        /// <param name="delete">Привязка в последствии может быть удалена</param>
        /// <returns></returns>
        public abstract void CreateMutableBinding(string name, bool delete);
        
        /// <summary>
        /// Присваивает значение уже существующей в записи окружения изменяемой привязки
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="value">Значение</param>
        /// <param name="s">Ссылка в сторогом режиме</param>
        public abstract void SetMutableBinding(string name, LanguageType value, bool strict);

        /// <summary>
        /// Возврщает из записи окружения значение уже существующей привязки.
        /// </summary>
        /// <param name="name">Имя привязки</param>
        /// <param name="strict">Ссылка в сторогом режиме</param>
        public abstract LanguageType GetBindingValue(string name, bool strict);

        /// <summary>
        /// Удаляет привязку из записи окружения
        /// </summary>
        /// <param name="name">Имя</param>
        public abstract bool DeleteBinding(string name);

        /// <summary>
        /// Возвращает значение, которое будет использоваться в качестве значения this при вызове 
        /// объектов функции, получаемых в качестве значений привязки из этой записи окружения
        /// </summary>
        public abstract object ImplicitThisValue();

    }
    public class DeclarativeEnviroment : EnvironmentRecord
    {
        //protected HashSet<Reference> references; // привязки для индентификаторов в области видимости
        protected Dictionary<string, Binding> bindings;
        public DeclarativeEnviroment(EnvironmentRecord outer)
        {
            bindings = new Dictionary<string, Binding>();
            this.Outer = outer;
        }
        public override bool HasBinding(string name)
        {
            return bindings.ContainsKey(name);
        }

        public override void CreateMutableBinding(string name, bool canBeDeleted)
        {
            Debug.Assert(bindings.ContainsKey(name));
            MutableBinding mutableBinding = new MutableBinding(Undefined.Value, canBeDeleted);
            bindings[name] = mutableBinding;
        }

        public override void SetMutableBinding(string name, LanguageType value, bool strict)
        {
            Debug.Assert(!bindings.ContainsKey(name));
            Binding binding = bindings[name];
            if (binding is MutableBinding)
            {
                ((MutableBinding)binding).Value = value;
            }
            else
            {
                if (strict == true)
                {
                    throw new Exception("TypeError");
                }
            }
        }

        public override LanguageType GetBindingValue(string name, bool strict)
        {
            Debug.Assert(!bindings.ContainsKey(name));
            Binding binding = bindings[name];
            if (binding is ImmutableBinding && binding.Value is Undefined)
            {
                if (strict == false)
                {
                    return Undefined.Value;
                }
                else
                {
                    throw new Exception("ReferenceError");
                }
            }
            else
            {
                return bindings[name].Value;
            }
            
        }

        public override bool DeleteBinding(string name)
        {
            if (!bindings.ContainsKey(name))
            {
                return true;
            }
            else
            {
                Binding binding = bindings[name];
                if (binding is MutableBinding)
                {
                    if (((MutableBinding)binding).CanBeDeleted == false)
                    {
                        return false;
                    }
                    else
                    {
                        bindings.Remove(name);
                        return true;
                    }
                }
                else
                {
                    Debug.Assert(true);
                    return false;
                }
            }
        }

        public override object ImplicitThisValue()
        {
            return Undefined.Value;
        }

        public void CreateImmutableBinding(string name, bool canBeDeleted)
        {
            Debug.Assert(bindings.ContainsKey(name));
            ImmutableBinding immutableBinding = new ImmutableBinding(Undefined.Value);
            bindings[name] = immutableBinding;
        }

        public void InitializeImmutableBinding(string name, LanguageType value)
        {
            Debug.Assert(bindings.ContainsKey(name) && bindings[name] is ImmutableBinding && ((ImmutableBinding)bindings[name]).Initialized == false);
            ImmutableBinding immutableBinding = (ImmutableBinding)bindings[name];
            immutableBinding.Value = value;
            immutableBinding.Initialized = true;

        }
    }
    public class ObjectEnviroment : EnvironmentRecord
    {
        public ES.Object BindingObject; // объект привязки
        public bool ProvideThis = true;
        public ObjectEnviroment(ES.Object bindingObject, EnvironmentRecord outer)
        {
            this.BindingObject = bindingObject;
            this.Outer = outer;
        }
        public override bool HasBinding(string name)
        {
            return BindingObject.HasProperty(name);
        }

        public override void CreateMutableBinding(string name, bool delete)
        {
            Debug.Assert(!BindingObject.HasProperty(name));
            PropertyDescriptor desc = new PropertyDescriptor();
            desc.Attributes["value"] = Undefined.Value;
            desc.Attributes["writable"] = true;
            desc.Attributes["enumerable"] = true;
            desc.Attributes["configurable"] = delete;
            BindingObject.DefineOwnProperty(name, desc, true);
        }

        public override void SetMutableBinding(string name, LanguageType value, bool strict)
        {
            BindingObject.Put(name, value, strict);
        }

        public override LanguageType GetBindingValue(string name, bool strict)
        {
            bool value = BindingObject.HasProperty(name);
            if (value == false)
            {
                if (strict == false)
                {
                    return Undefined.Value;
                }
                else
                {
                    throw new Exception("ReferenceError");
                }
            }
            return BindingObject.Get(name);
        }

        public override bool DeleteBinding(string name)
        {
            return BindingObject.Delete(name, false);
        }

        public override object ImplicitThisValue()
        {
            if (ProvideThis)
            {
                return BindingObject;
            }
            else
            {
                return Undefined.Value;
            }
        }
    }
}
    #endregion
