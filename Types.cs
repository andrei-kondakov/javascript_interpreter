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
        public static bool SameValue(object x, object y)
        {
            if (!x.GetType().Equals(y.GetType())) return false; // QUESTION TODO TEST
            if (x.Equals(Undefined.Value)) return true;
            if (x.Equals(ES.Null.Value)) return true;
            if (x is ES.Number)
            {
                return ((Number)x).Value == ((Number)y).Value;
            }
            if (x is string)
            {
                return x.Equals(y);
            }

            if (IsBooleanType(x))
            {
                if ((x.Equals(ES.Boolean.True) && y.Equals(ES.Boolean.True))
                    || (x.Equals(ES.Boolean.False) && y.Equals(ES.Boolean.False)))
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
        public static bool IsBooleanType(object value)
        {
            return value.Equals(ES.Boolean.True) || value.Equals(ES.Boolean.False);
        }
        
    }
    #region Language_types
    public abstract class LanguageType
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
        public static Null Value = null;
    }
    public class Boolean : LanguageType
    {
        public static bool True = true;
        public static bool False = false;
    }
    public enum DescriptorType
    {
        DATA,
        ACCESSOR,
        UNKNOWN
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
    }
    public class String : LanguageType
    {
        public string Value;
        public String(string value)
        {
            this.Value = value;
        }
    }
    public class Object : LanguageType
    {
        // QUESTION m.b. Dicitipnary<string, PropertyDescriptroType> ??!??
        private Dictionary<string, PropertyDescriptor> namedProperties;   // ассоциирует имя со значением и набором булевых атрибутов
        private Dictionary<string, object> internalProperties;      // внутренние свойства
        /*  Внутренние свойства каждого объекта */
        /*private EcmaType prototype;     // Прототип данного объекта [Object/NULL]
        private string __class__;          // Классифицаия объектов
        private bool extensible; // Если true, к объекту могут быть добавлены собственные свойства*/
        /* ------------------------------------*/

        public Object()
        {
            namedProperties = new Dictionary<string, PropertyDescriptor>();
            internalProperties = new Dictionary<string, object>();
            internalProperties["extensible"] = true;
        }

        public Dictionary<string, object> InternalProperties
        {
            get
            {
                return internalProperties;
            }
        }

        /* Внутренние методы каждого объекта */

        /// <summary>
        /// Возвращает дескриптор именнованного свойства данного объекта
        /// или undefined (в случае отсутствия)
        /// </summary>
        /// <param name="propertyName">Название свойства</param>
        /// <returns>UNDEFINED/PROPERTY_DESCRIPTOR</returns>
        private object getOwnProperty(string propertyName) // возвращает дескриптор свойства
        {
            if (!namedProperties.ContainsKey(propertyName))
                return Undefined.Value;
            PropertyDescriptor property, result;
            property = namedProperties[propertyName];
            if (PropertyDescriptor.IsDataDescriptor(property))
            {
                result = new PropertyDescriptor(DescriptorType.DATA);
                result.Attributes["value"] = property.Attributes["value"];
                result.Attributes["writable"] = property.Attributes["writable"];
            }
            else
            {
                result = new PropertyDescriptor(DescriptorType.ACCESSOR);
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
        public object GetProperty(string propertyName) 
        {
            var property = getOwnProperty(propertyName);
            if (!(property is Undefined))
            {
                return property;
            }
            else
            {
                var prototype = internalProperties["prototype"];
                if (property is Undefined)
                {
                    return Undefined.Value; 
                }
                // QUESTION?
                return ((ES.Object)prototype).GetProperty(propertyName);
            }
        }

        // get - возвращает значение именнованного свойства
        /// <summary>
        /// Возвращает значение свойства
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns></returns>
        public object Get(string propertyName)   
        {
            var desc = GetProperty(propertyName);
            if (desc is Undefined) return Undefined.Value;
            if (PropertyDescriptor.IsDataDescriptor(desc))
            {
                return ((PropertyDescriptor)desc).Attributes["value"];
            }
            else //if (EcmaScript.IsAcessorDescriptor(desc))
            {
                var getter = ((PropertyDescriptor)desc).Attributes["get"];
                if (getter is Undefined) return Undefined.Value;
                return null; // TODO: вызвать внутренний метод CALL для getter передавая О в качестве значения this и непередавая никаких аргументов
            }
        }

        // canPut - Возвращает булево значение, означающее возможность выполнения операции Put с именем свойства propertyName.
        /// <summary>
        /// Проверяет можно ли выполнить операцию Put с аргументом propertyName
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns>true/false</returns>
        private bool canPut(string propertyName)
        {
            var desc = getOwnProperty(propertyName);
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
            var prototype = internalProperties["prototype"];
            if (prototype.Equals(ES.Null.Value))
            {
                return (bool)internalProperties["extensible"];
            }
            var inherited = ((ES.Object)prototype).GetProperty(propertyName);
            if (inherited is Undefined)
            {
                return (bool)internalProperties["extensible"];
            }
            if (PropertyDescriptor.IsAcessorDescriptor(inherited))
            {
                if (((PropertyDescriptor)inherited).Attributes["set"] is Undefined)
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
                if ((bool)internalProperties["extensible"] == false)
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

        public bool Put(string propertyName, object value, bool needThrow)
        {
            bool possibleToPut = canPut(propertyName);
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
            var ownDesc = getOwnProperty(propertyName);
            if (PropertyDescriptor.IsDataDescriptor(ownDesc))
            {
                PropertyDescriptor valueDesc = new PropertyDescriptor(DescriptorType.DATA);
                valueDesc.Attributes["value"] = value;
                return DefineOwnProperty(propertyName, valueDesc, needThrow);
            }
            var desc = GetProperty(propertyName);
            if (PropertyDescriptor.IsAcessorDescriptor(desc))
            {
                var setter = ((PropertyDescriptor)desc).Attributes["set"];
                // QUESTION: почему setter не может быть undefined?
                // TODO 8.12.5 5 пункт
                return false; // NOT IMPLEMENTED!!
            }
            else
            {
                PropertyDescriptor newDesc = new PropertyDescriptor(DescriptorType.DATA);
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
            var desc = getOwnProperty(propertyName);
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
            var current = getOwnProperty(propertyName);
            if (current is Undefined && (bool)internalProperties["extensible"] == false)
            {
                if (needThrow) throw new Exception("TypeError");
                return false;
            }
            if (current is Undefined && (bool)internalProperties["extensible"])
            {
                PropertyDescriptor newDescriptor;
                var enumerable = descriptor.Attributes["enumerable"];
                var configurable = descriptor.Attributes["configurable"];
                if (PropertyDescriptor.IsGenericDescriptor(descriptor) || PropertyDescriptor.IsDataDescriptor(descriptor))
                {
                    newDescriptor = new PropertyDescriptor(DescriptorType.DATA);
                    var value = descriptor.Attributes["value"];
                    var writable = descriptor.Attributes["writable"];
                    if (value != null) newDescriptor.Attributes["value"] = value;
                    if (writable != null) newDescriptor.Attributes["writable"] = writable;
                }
                else
                {
                    newDescriptor = new PropertyDescriptor(DescriptorType.ACCESSOR);
                    PropertyDescriptor acessorDescriptor = new PropertyDescriptor(DescriptorType.ACCESSOR);
                    var get = descriptor.Attributes["get"];
                    var set = descriptor.Attributes["set"];
                    if (get != null) newDescriptor.Attributes["get"] = get;
                    if (set != null) newDescriptor.Attributes["set"] = set;
                }
                if (enumerable != null) newDescriptor.Attributes["enumerable"] = enumerable;
                if (configurable != null) newDescriptor.Attributes["configurable"] = configurable;
                namedProperties.Add(propertyName, newDescriptor);
                return true;
            }
            if (descriptor.Attributes.Count == 0) return true;
            // same - true, если все поля в descriptor также встречаются в current,
            // и значение каждого поля в Desc оказывается таким же, 
            // что и у соответствующего поля в current (TODO? при сравнении с помощью алгоритма SameValue (9.12).)
            // QUESTION TODO TEST! Срочно проверить будет ли работать
            bool same = descriptor.Attributes.Cast<DictionaryEntry>()
                .Union(((PropertyDescriptor)current).Attributes.Cast<DictionaryEntry>()).Count() == descriptor.Attributes.Count;
            if (same) return true;
            if ((bool)((PropertyDescriptor)current).Attributes["configurable"] == false)
            {
                if ((bool)descriptor.Attributes["configurable"])
                {
                    if (needThrow) throw new Exception("TypeError");
                    return false;
                }
                // Булево отрицание = просто неравенство двух значений?
                if (descriptor.Attributes["enumerable"] != null 
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
                    // QUESTION TEST
                    PropertyDescriptor oldDescriptor = (PropertyDescriptor)GetProperty(propertyName);
                    PropertyDescriptor newDescriptor;
                    Delete(propertyName, needThrow); // QUESTION TEST
                    if (PropertyDescriptor.IsDataDescriptor(current))
                    {
                        newDescriptor = new PropertyDescriptor(DescriptorType.ACCESSOR);
                    }
                    else
                    {
                        newDescriptor = new PropertyDescriptor(DescriptorType.DATA);
                    }
                    newDescriptor.Attributes["configurable"] = oldDescriptor.Attributes["configurable"];
                    newDescriptor.Attributes["enumerable"] = oldDescriptor.Attributes["enumerable"];
                }
                else
                {
                    if (PropertyDescriptor.IsDataDescriptor(current) && PropertyDescriptor.IsDataDescriptor(descriptor))
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
                                // SameValue or Equal??
                                // TODO !!!
                                if (descriptor.Attributes["value"] != null
                                    && EcmaTypes.SameValue(descriptor.Attributes["value"], ((PropertyDescriptor)current).Attributes["value"]) == false)
                                {
                                    if (needThrow) throw new Exception("TypeError");
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            // QUESTION 10b
                            // return true?
                        }
                    }
                    else
                    {
                        if ((bool)((PropertyDescriptor)current).Attributes["configurable"] == false)
                        {
                            if (descriptor.Attributes["set"] != null
                                    && EcmaTypes.SameValue(descriptor.Attributes["set"], ((PropertyDescriptor)current).Attributes["set"]) == false)
                            {
                                if (needThrow) throw new Exception("TypeError");
                                return false;
                            }
                            if (descriptor.Attributes["get"] != null
                                    && EcmaTypes.SameValue(descriptor.Attributes["get"], ((PropertyDescriptor)current).Attributes["get"]) == false)
                            {
                                if (needThrow) throw new Exception("TypeError");
                                return false;
                            }
                        }
                    }
                }
            }
            PropertyDescriptor prop = namedProperties[propertyName];
            descriptor.Attributes = (Hashtable)prop.Attributes.Clone(); // TEST!
            return true;
        }
        /* ------------------------------------*/
    }
    #endregion

    #region Specification_types
    public abstract class SpecificationType { }
    public class PropertyDescriptor : SpecificationType
    {
        public Hashtable Attributes;

        public PropertyDescriptor(DescriptorType type)
        {
            Attributes = new Hashtable();

            switch (type)
            {
                case DescriptorType.DATA:
                    {
                        SetDefaultValue("value");
                        SetDefaultValue("writable");
                        SetDefaultValue("enumerable");
                        SetDefaultValue("configurable");
                        break;
                    }
                case DescriptorType.ACCESSOR:
                    {
                        SetDefaultValue("get");
                        SetDefaultValue("set");
                        SetDefaultValue("enumerable");
                        SetDefaultValue("configurable");
                        break;
                    }
                default:
                    //throw new Exception("Unknown type of property descriptor (use \"data\" or \"acessor\"");
                    {
                        break;
                    }
            }   
        }
        public PropertyDescriptor(object value, bool writable, bool enumerable, bool configurable)
        {
            Attributes = new Hashtable();
            Attributes["value"] = value;
            Attributes["writable"] = writable;
            Attributes["enumerable"] = enumerable;
            Attributes["configurable"] = configurable;
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
        // ------------------------ Работа с дексрипторами --------------------------------//
        public static bool IsDataDescriptor(object desc)
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
        public static bool IsAcessorDescriptor(object desc)
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
        public static bool IsGenericDescriptor(object desc)
        {
            if (desc is Undefined)
            {
                return false;
            }
            PropertyDescriptor propDesc = (PropertyDescriptor)desc;
            if (PropertyDescriptor.IsDataDescriptor(propDesc) == false
                && PropertyDescriptor.IsAcessorDescriptor(propDesc) == false)
            {
                return true;
            }
            return false;
        }
        
    }
    public class Reference : SpecificationType
    {
        private object baseValue;     // база Undefined, BooleanType, StringType, NumberType, или EnvironmentRecord
        private string referenceName;   // имя ссылки
        private bool strictReference;   // строгая ссылка

        public Reference(object baseValue, string referenceName, bool strictReference)
        {
            this.baseValue = baseValue;
            this.referenceName = referenceName;
            this.strictReference = strictReference;
        }

        //public object BaseValue { get; set; }
        //public string ReferenceName { get; set; }
        //public bool StrictReference { get; set; }
        // ------------------------ Работа со ссылками --------------------------------//
        public object GetBase()
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
            return EcmaTypes.IsBooleanType(baseValue) || baseValue is ES.String || baseValue is ES.Number;
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
        protected object value;
        public Binding(object value)
        {
           this.value = value;
        }
        public object Value { get; set; }
    }
    public class MutableBinding : Binding
    {
        protected bool canBeDeleted;

        public MutableBinding(object value, bool canBeDeleted)
            : base(value)
        {
            this.canBeDeleted = canBeDeleted;
        }
        public bool CanBeDeleted { get; set; }
    }
    public class ImmutableBinding : Binding
    {
        protected bool initialized;
        public ImmutableBinding(object value)
            : base(value)
        {
            this.initialized = false;
        }
        public bool Initialized { get; set; }
    }
    public abstract class EnvironmentRecord : SpecificationType
    {
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
        public abstract void SetMutableBinding(string name, object value, bool strict);

        /// <summary>
        /// Возврщает из записи окружения значение уже существующей привязки.
        /// </summary>
        /// <param name="name">Имя привязки</param>
        /// <param name="strict">Ссылка в сторогом режиме</param>
        public abstract object GetBindingValue(string name, bool strict);

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
    public class DeclarativeEnviromentRecord : EnvironmentRecord
    {
        //protected HashSet<Reference> references; // привязки для индентификаторов в области видимости
        protected Dictionary<string, Binding> bindings;
        public DeclarativeEnviromentRecord()
        {
            bindings = new Dictionary<string, Binding>();
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

        public override void SetMutableBinding(string name, object value, bool strict)
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

        public override object GetBindingValue(string name, bool strict)
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

        public void InitializeImmutableBinding(string name, object value)
        {
            Debug.Assert(bindings.ContainsKey(name) && bindings[name] is ImmutableBinding && ((ImmutableBinding)bindings[name]).Initialized == false);
            ImmutableBinding immutableBinding = (ImmutableBinding)bindings[name];
            immutableBinding.Value = value;
            immutableBinding.Initialized = true;

        }
    }
    public class ObjectEnviromentRecord : EnvironmentRecord
    {
        private ES.Object bindingObject; // объект привязки
        private bool provideThis;
        public ObjectEnviromentRecord(ES.Object bindingObject)
        {
            this.bindingObject = bindingObject;
            this.provideThis = false;
        }

        public ES.Object BindingObject
        {
            get
            {
                return bindingObject;
            }
        }

        public override bool HasBinding(string name)
        {
            return bindingObject.HasProperty(name);
        }

        public override void CreateMutableBinding(string name, bool delete)
        {
            Debug.Assert(!bindingObject.HasProperty(name));
            bindingObject.DefineOwnProperty(name, new PropertyDescriptor(Undefined.Value, true, true, delete), true);
        }

        public override void SetMutableBinding(string name, object value, bool strict)
        {
            bindingObject.Put(name, value, strict);
        }

        public override object GetBindingValue(string name, bool strict)
        {
            bool value = bindingObject.HasProperty(name);
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
            return bindingObject.Get(name);
        }

        public override bool DeleteBinding(string name)
        {
            return bindingObject.Delete(name, false);
        }

        public override object ImplicitThisValue()
        {
            if (provideThis)
            {
                return bindingObject;
            }
            else
            {
                return Undefined.Value;
            }
        }
    }
    public class LexicalEnvironment : SpecificationType
    {
        private EnvironmentRecord environmentRecord;
        private LexicalEnvironment outer; // ссылка! на внешнее лексическое окружение
        public LexicalEnvironment(EnvironmentRecord environmentRecord, LexicalEnvironment externalLexicalEnvironment)
        {
            this.environmentRecord = environmentRecord;
            this.outer = externalLexicalEnvironment;
        }
        public EnvironmentRecord EnvironmentRecord
        {
            get
            {
                return environmentRecord;
            }
        }
        public LexicalEnvironment Outer
        {
            get
            {
                return outer;
            }
        }
        // ------------------------ Работа с лексическим окружением --------------------------------//
        public static LexicalEnvironment NewDeclarativeEnvironment(LexicalEnvironment outer)
        {
            return new LexicalEnvironment(new DeclarativeEnviromentRecord(), outer);
        }
        public static LexicalEnvironment NewObjectEnvironment(ES.Object obj, LexicalEnvironment outer)
        {
            return new LexicalEnvironment(new ObjectEnviromentRecord(obj), outer);
        }
        public static object GetIdentifierReference(LexicalEnvironment lex, string name, bool strict)
        {
            if (lex == null)
            {
                return new Reference(Undefined.Value, name, strict);
            }
            bool exists = lex.environmentRecord.HasBinding(name);
            if (exists)
            {
                return new Reference(lex.EnvironmentRecord, name, strict);
            }
            else
            {
                return GetIdentifierReference(lex.Outer, name, strict);
            }
        }
    }

    
}
    #endregion
