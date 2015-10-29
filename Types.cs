using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace JavaScriptInterpreter.Types
{
    public abstract class EcmaType
    { }

    #region Language_types
    public class UndefinedType : EcmaType
    {
        private static UndefinedType value;
        private UndefinedType()
        { }
        public static UndefinedType Value
        {
            get
            {
                if (value == null)
                {
                    return new UndefinedType();
                }
                return value;
            }
        }
    }
    public class NullType : EcmaType
    {
        private static NullType value; // singleton
        private NullType()
        { }
        public static NullType Value
        {
            get
            {
                if (value == null)
                {
                    value = new NullType();
                }
                return value;
            }
        }
    }
    public class BooleanType : EcmaType
    {
        private bool value;
        public BooleanType(bool value)
        {
            this.value = value;
        }
        public bool Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
    public class StringType : EcmaType
    {
        private string value;
        public StringType(string value)
        {
            this.value = value;
        }
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
    public class NumberType : EcmaType
    {
        private double value;
        //private bool posInfinity;
        //private bool negInfinity;
        //private bool NaN;

        public NumberType(double value)
        {
            this.value = value;
        }
        public double Value
        {
            get
            {
                // TODO:    подумать над реализацией специальных значений 
                //          положительная бесконечность, отриц. бесконечность,
                //          Not a Number (NaN)

                //if (!(posInfinity || negInfinity || NaN))
                //{
                return value;
                //}
                //return null;
            }
            set
            { this.value = value; }
        }
    }
    public class ObjectType : EcmaType
    {
        // QUESTION m.b. Dicitipnary<string, PropertyDescriptroType> ??!??
        private Dictionary<string, DataDescriptor> namedDataProperties;   // ассоциирует имя со значением и набором булевых атрибутов
        private Dictionary<string, AcessorDescriptor> namedAccessorProperties; // ассоциирует имя с одной или двумя функциями доступа и с набором булевых атрибутов
        
        /*  Внутренние свойства каждого объекта */
        /*private EcmaType prototype;     // Прототип данного объекта [Object/NULL]
        private string __class__;          // Классифицаия объектов
        private bool extensible; // Если true, к объекту могут быть добавлены собственные свойства*/
        /* ------------------------------------*/

        public ObjectType()
        {
            namedDataProperties = new Dictionary<string, DataDescriptor>();
            namedAccessorProperties = new Dictionary<string, AcessorDescriptor>();
        }

        /* Внутренние методы каждого объекта */

        /// <summary>
        /// Возвращает дескриптор именнованного свойства данного объекта
        /// или undefined (в случае отсутствия)
        /// </summary>
        /// <param name="propertyName">Название свойства</param>
        /// <returns></returns>
        private EcmaType getOwnProperty(string propertyName) // возвращает дескриптор свойства
        {
            if (!namedDataProperties.ContainsKey(propertyName) || !namedAccessorProperties.ContainsKey(propertyName))
                return UndefinedType.Value;
            PropertyDescriptorType property, result;
            if (namedAccessorProperties.ContainsKey(propertyName))
            {
                property = namedDataProperties[propertyName];
                result = new DataDescriptor();
                ((DataDescriptor)result).Value=((DataDescriptor)property).Value;
                ((DataDescriptor)result).Writable = ((DataDescriptor)property).Writable;
            }
            else
            {
                property = namedAccessorProperties[propertyName];
                result = new AcessorDescriptor();
                ((AcessorDescriptor)result).Get = ((AcessorDescriptor)property).Get;
                ((AcessorDescriptor)result).Set = ((AcessorDescriptor)property).Set;
            }
            result.Enumerable = property.Enumerable;
            result.Configurable = property.Configurable;
            return result;
        }
        /// <summary>
        /// Возвращает полностью заполненный дескриптор свойства данного объекта
        /// или undefined (в случае отсутствия)
        /// </summary>
        /// <param name="propertyName">Название свойства</param>
        /// <returns></returns>
        public EcmaType GetProperty(string propertyName) 
        {
            EcmaType prop = getOwnProperty(propertyName);
            if (!(prop is UndefinedType))
            {
                return prop;
            }
            else
            {
                EcmaType proto = this.get("__proto__");
                if (proto is NullType) return UndefinedType.Value;
                return ((ObjectType)proto).GetProperty(propertyName);
            }
        }

        // get - возвращает значение именнованного свойства
        /// <summary>
        /// Возвращает значение свойства
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns></returns>
        private EcmaType get(string propertyName)   
        {
            EcmaType desc = this.GetProperty(propertyName);
            if (desc is UndefinedType) return UndefinedType.Value;
            if (desc is DataDescriptor)
            {
                return (EcmaType)((DataDescriptor)desc).Value;
            }
            else//if (desc is AcessorDescriptor)
            {
                EcmaType getter = (EcmaType)((AcessorDescriptor)desc).Get;
                if (getter is UndefinedType) return UndefinedType.Value;
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
            EcmaType desc = getOwnProperty(propertyName);
            if (!(desc is UndefinedType))
            {
                if (desc is AcessorDescriptor)
                {
                    if (((AcessorDescriptor)desc).Set is UndefinedType) return false;
                    else return true;
                }
                else
                {
                    return (bool)((DataDescriptor)desc).Writable;
                }
            }
            EcmaType proto = this.get("__proto__");
            if (proto is NullType) return ((BooleanType)this.get("__extensible__")).Value;
            EcmaType inherited = ((ObjectType)proto).GetProperty(propertyName);
            if (inherited is UndefinedType) return ((BooleanType)this.get("__extensible__")).Value;
            if (inherited is AcessorDescriptor)
            {
                if (((AcessorDescriptor)inherited).Set is UndefinedType) return false;
                else return true;
            }
            else
            {
                if (((BooleanType)this.get("__extensible__")).Value == false) return false;
                else return (bool)((DataDescriptor)inherited).Writable;
            }
            
        }

        /// <summary>
        /// Присваивает значение именнованному свойству
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="value">Значение</param>
        /// <param name="needThrow">Обработка отказов</param>
        /// <returns>Результат</returns>

        public bool Put(string propertyName, EcmaType value, bool needThrow)
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
            EcmaType ownDesc = getOwnProperty(propertyName);
            bool res = false;
            if (ownDesc is DataDescriptor)
            {
                DataDescriptor valueDesc = new DataDescriptor();
                res = DefineOwnProperty(propertyName, valueDesc, needThrow);
            }
            return res;
        }
        

        /// <summary>
        /// Есть ли у данного объекта свойство с таким именем
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns></returns>
        private bool hasProperty(string propertyName)
        {
            EcmaType desc = GetProperty(propertyName);
            if (desc is UndefinedType) return false;
            return true;
        }
        
       
        /// <summary>
        /// Удаляет из объекта указанное именнованное свойство
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="needThrow">Обработка отказов</param>
        /// <returns></returns>
        private bool delete(string propertyName, bool needThrow)
        {
            EcmaType desc = GetProperty(propertyName);
            if (desc is UndefinedType) return true;
            if ((bool)((PropertyDescriptorType)desc).Configurable)
            {
                if (desc is DataDescriptor) namedDataProperties.Remove(propertyName);
                else namedAccessorProperties.Remove(propertyName);
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
        public EcmaType DefaultValue(string hint)
        {
            // TODO результат вызова 8.12.8
            return null;
        }
        

        private bool isGenericDescriptor(EcmaType desc)
        {
            if (desc is UndefinedType) return false;
            if (!(desc is AcessorDescriptor) && !(desc is DataDescriptor)) return true;
            return false;
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
        public bool DefineOwnProperty(string propertyName, PropertyDescriptorType descriptor, bool needThrow)
        {
            EcmaType current = getOwnProperty(propertyName);
            if (current is UndefinedType && ((BooleanType)this.get("__extensible__")).Value == false)
            {
                if (needThrow) throw new Exception("TypeError");
                return false;
            }
            if (current is UndefinedType && ((BooleanType)this.get("__extensible__")).Value)
            {
                if (isGenericDescriptor(descriptor) || descriptor is DataDescriptor)
                {
                    DataDescriptor dataDescriptor = new DataDescriptor();
                    dataDescriptor.Value = ((DataDescriptor)descriptor).Value;
                    dataDescriptor.Writable = ((DataDescriptor)descriptor).Value;
                    dataDescriptor.Configurable = descriptor.Configurable;
                    dataDescriptor.Enumerable = descriptor.Enumerable;
                    //var configurable = ((DataDescriptor)descriptor).Configurable;
                    //var writable = ((DataDescriptor)descriptor).Writable;
                    //var value = ((DataDescriptor)descriptor).Value;
                    //var enumerable = ((DataDescriptor)descriptor).Enumerable;
                    //if (configurable==null) { dataDescriptor.SetDefaultValue("configurable"); }
                    //else { dataDescriptor.Configurable = configurable; }
                    //if (writable==null) { dataDescriptor.SetDefaultValue("writable"); }
                    //else dataDescriptor.Writable = writable;
                    //if (value==null) { dataDescriptor.SetDefaultValue("value"); }
                    //else dataDescriptor.Value = value;
                    //if (enumerable == null) { dataDescriptor.SetDefaultValue("enumerable"); }
                    //else dataDescriptor.Enumerable = enumerable;
                    namedDataProperties.Add(propertyName, dataDescriptor);
                }
                else
                {
                    AcessorDescriptor acessorDescriptor = new AcessorDescriptor();
                    acessorDescriptor.Get = ((AcessorDescriptor)descriptor).Get;
                    acessorDescriptor.Set = ((AcessorDescriptor)descriptor).Set;
                    acessorDescriptor.Configurable = descriptor.Configurable;
                    acessorDescriptor.Enumerable = descriptor.Enumerable;
                    //var configurable = ((AcessorDescriptor)descriptor).Configurable;
                    //var enumerable = ((AcessorDescriptor)descriptor).Enumerable;
                    //var get = ((AcessorDescriptor)descriptor).Get;
                    //var set = ((AcessorDescriptor)descriptor).Set;
                    //if (configurable==null) { acessorDescriptor.SetDefaultValue("configurable"); }
                    //else acessorDescriptor.Configurable = configurable;
                    //if (enumerable==null) { acessorDescriptor.SetDefaultValue("enumerable"); }
                    //else acessorDescriptor.Enumerable = enumerable;
                    //if (get==null) { acessorDescriptor.SetDefaultValue("get"); }
                    //else acessorDescriptor.Get = get;
                    //if (set==null) { acessorDescriptor.SetDefaultValue("set"); }
                    //else acessorDescriptor.Set = set;
                    namedAccessorProperties.Add(propertyName, acessorDescriptor);
                }
                return true;
            }
            if (descriptor.Attributes.Count == 0) return true;
            // same - true, если все поля в descriptor также встречаются в current,
            // и значение каждого поля в Desc оказывается таким же, 
            // что и у соответствующего поля в current (TODO? при сравнении с помощью алгоритма SameValue (9.12).)
            // TEST! Срочно проверить будет ли работать
            bool same = descriptor.Attributes.Cast<DictionaryEntry>()
                .Union(((PropertyDescriptorType)current).Attributes.Cast<DictionaryEntry>()).Count() == descriptor.Attributes.Count;
            if (same) return true;
            if ((bool)((PropertyDescriptorType)current).Configurable == false)
            {
                if ((bool)descriptor.Configurable == true)
                {
                    if (needThrow) throw new Exception("TypeError");
                    return false;
                }
                // Булево отрицание = просто неравенство двух значений?
                if (descriptor.Enumerable != null && ((PropertyDescriptorType)current).Enumerable != descriptor.Enumerable)
                {
                    if (needThrow) throw new Exception("TypeError");
                    return false;
                }
            }
            if (!isGenericDescriptor(descriptor))
            {
                if (current is DataDescriptor != descriptor is DataDescriptor)
                {
                    if ((bool)((PropertyDescriptorType)current).Configurable == false)
                    {
                        if (needThrow) throw new Exception("TypeError");
                        return false;
                    }
                    if (current is DataDescriptor)
                    {
                        DataDescriptor oldDataDescriptor = namedDataProperties[propertyName];
                        namedDataProperties.Remove(propertyName);
                        AcessorDescriptor newAcessorDescriptor = new AcessorDescriptor();
                        newAcessorDescriptor.SetDefaultValue("get");
                        newAcessorDescriptor.SetDefaultValue("set");
                        newAcessorDescriptor.Configurable = oldDataDescriptor.Configurable;
                        newAcessorDescriptor.Enumerable = oldDataDescriptor.Enumerable;
                    }
                    else
                    {
                        AcessorDescriptor oldAcessorDescriptor = namedAccessorProperties[propertyName];
                        namedAccessorProperties.Remove(propertyName);
                        DataDescriptor newDataDescriptor = new DataDescriptor();
                        newDataDescriptor.SetDefaultValue("value");
                        newDataDescriptor.SetDefaultValue("writable");
                        newDataDescriptor.Configurable = oldAcessorDescriptor.Configurable;
                        newDataDescriptor.Enumerable = oldAcessorDescriptor.Enumerable;
                    }
                }
                else
                {
                    if (current is DataDescriptor && descriptor is DataDescriptor)
                    {
                        if ((bool)((DataDescriptor)current).Configurable == false)
                        {
                            if ((bool)((DataDescriptor)current).Writable == false
                                && (bool)((DataDescriptor)descriptor).Writable == true)
                            {
                                if (needThrow) throw new Exception("TypeError");
                                return false;
                            }
                            if ((bool)((DataDescriptor)current).Writable == false)
                            {
                                // SameValue or Equal??
                                // CHECK!!!
                                if (descriptor.Attributes["value"] != null
                                    && EcmaScript.SameValue((EcmaType)descriptor.Attributes["value"], (EcmaType)((DataDescriptor)current).Attributes["value"]) == false)
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
                        if ((bool)((AcessorDescriptor)current).Configurable == false)
                        {
                            if (descriptor.Attributes["set"] != null
                                    && EcmaScript.SameValue((EcmaType)descriptor.Attributes["set"], (EcmaType)((AcessorDescriptor)current).Attributes["set"]) == false)
                            {
                                if (needThrow) throw new Exception("TypeError");
                                return false;
                            }
                            if (descriptor.Attributes["get"] != null
                                    && EcmaScript.SameValue((EcmaType)descriptor.Attributes["get"], (EcmaType)((AcessorDescriptor)current).Attributes["get"]) == false)
                            {
                                if (needThrow) throw new Exception("TypeError");
                                return false;
                            }
                        }
                    }
                    PropertyDescriptorType prop;
                    if (namedDataProperties.ContainsKey(propertyName)) prop = namedDataProperties[propertyName];
                    else prop = namedAccessorProperties[propertyName];
                    descriptor.Attributes = (Hashtable)prop.Attributes.Clone(); // TEST!
                    return true;
                }
            }
            //if (descriptor.Configurable==null && descriptor.Enumerable==null)
            //{
            //    if (descriptor is DataDescriptor
            //        && !((DataDescriptor)descriptor).Value.HasValue && !((DataDescriptor)descriptor).Writable.HasValue)
            //    {
            //        return true;
            //    }
            //    if (descriptor is AcessorDescriptor
            //        && !((AcessorDescriptor)descriptor).Get.HasValue && !((AcessorDescriptor)descriptor).Set.HasValue)
            //    {
            //        return true;
            //    }
            //}
            //if (descriptor.Configurable.Value == ((PropertyDescriptorType)current).Configurable.Value
            //    && descriptor.Enumerable.Value == ((PropertyDescriptorType)current).Enumerable.Value)
            //{
            //    if (descriptor is DataDescriptor)
            //    {
            //        if (EcmaScript.SameValue(((DataDescriptor)descriptor).Value.Value, ((DataDescriptor)current).Value.Value)
            //            && ((DataDescriptor)descriptor).Writable == ((DataDescriptor)current).Writable)
            //        {
            //            return true;
            //        }
            //    }
            //    else
            //    {
            //        if (EcmaScript.SameValue(((AcessorDescriptor)descriptor).Get.Value, ((AcessorDescriptor)current).Get.Value)
            //            && EcmaScript.SameValue(((AcessorDescriptor)descriptor).Set.Value, ((AcessorDescriptor)current).Set.Value))
            //        {
            //            return true;
            //        }
            //    }
            //}
            //if (((PropertyDescriptorType)current).Configurable.Value == false)
            //{
            //    if (descriptor.Configurable == true) reject(needThrow);
                
            //}
            return true;
        }
        /* ------------------------------------*/
    }
    #endregion

    #region Specification_types
    public abstract class PropertyDescriptorType : EcmaType
    {
        public Hashtable Attributes;
        //private bool? enumerable;    // Если true, данное свойство можно перечислить с помощью for-in
        //private bool? configurable;  // Если false, нельзя будет удалить данное свойство, измнеить его, сделав свойством-аксессором
        //                            // или изменить его атрибуты (кроме Value)
        
        protected PropertyDescriptorType()
        {
            Attributes = new Hashtable();
        }

        public object Enumerable
        {
            get { return Attributes["enumerable"]; }
            set
            {
                if (value != null)
                {
                    if (value is bool)
                    {
                        Attributes["enumerable"] = value;
                    }
                    else { throw new Exception("Error: Enumerable should be bool value"); }
                }
                else
                {
                    SetDefaultValue("enumerable");
                }
            }
        }
        public object Configurable
        {
            get { return Attributes["configurable"]; }
            set
            {
                if (value != null)
                {
                    if (value is bool)
                    {
                        if (value != null) Attributes["configurable"] = value;
                    }
                    else { throw new Exception("Error: Configurable should be bool value"); }
                }
                else
                {
                    SetDefaultValue("configurable");
                }
            }
        }
        public void SetDefaultValue(string attribute)
        {
            switch (attribute)
            {
                case "enumerable":
                    {
                        Attributes["enumerable"] = false;
                        break;
                    }
                case "configurable":
                    {
                        Attributes["configurable"] = false;
                        break;
                    }
                case "value":
                    {
                        Attributes["value"] = UndefinedType.Value;
                        break;
                    }
                case "writable":
                    {
                        Attributes["writable"] = false;
                        break;
                    }
                case "get":
                    {
                        Attributes["get"] = UndefinedType.Value;
                        break;
                    }
                case "set":
                    {
                        Attributes["set"] = UndefinedType.Value;
                        break;
                    }
                default:
                    throw new Exception("Error: Have not a default value for attribute: " + attribute);
            }
        }
    }
    public class DataDescriptor : PropertyDescriptorType
    {
        public DataDescriptor()
            : base()
        { }
        public DataDescriptor(EcmaType value, bool writable, bool enumerable, bool configurable)
            : base()
        {
            this.Value = value;
            this.Writable = writable;
            this.Enumerable = enumerable;
            this.Configurable = configurable;
        }
        public object Value
        {
            get
            {
                return Attributes["value"];
            }
            set
            {
                // QUESTION EcmaType OR?? Будет ли работать с наследниками
                if (value != null)
                {
                    if (value is EcmaType) { Attributes["value"] = value; }
                    else { throw new Exception("Error: Value should be instance of ECMAtype"); }
                }
                else
                {
                    SetDefaultValue("value");
                }
            }
        }
        public object Writable
        {
            get
            {
                return Attributes["writable"];
            }
            set
            {
                if (value != null)
                {
                    if (value is bool) { Attributes["writable"] = value; }
                    else { throw new Exception("Error: Writable should be bool value"); }
                }
                else
                {
                    SetDefaultValue("writable");
                }
            }
        }


        
    }
    public class AcessorDescriptor : PropertyDescriptorType
    {
        public AcessorDescriptor()
            : base()
        { }
        public object Get
        {
            get
            {
                return Attributes["get"];
            }
            set
            {
                // QUESTION EcmaType OR?? Будет ли работать с наследниками
                if (value != null)
                {
                    if (value is EcmaType) { Attributes["get"] = value; }
                    else { throw new Exception("Error: Get should be instance of ECMAtype"); }
                }
                else
                {
                    SetDefaultValue("get");
                }
            }
        }
        public object Set
        {
            get
            {
                return Attributes["set"];
            }
            set
            {
                // QUESTION EcmaType OR?? Будет ли работать с наследниками
                if (value != null)
                {
                    if (value is EcmaType) { Attributes["set"] = value; }
                    else { throw new Exception("Error: Set should be instance of ECMAtype"); }
                }
                else
                {
                    SetDefaultValue("set");
                }
            }
        }
    }
    public class Reference : EcmaType
    {
        private EcmaType baseValue;     // база Undefined, BooleanType, StringType, NumberType
        private string referenceName;   // имя ссылки
        private bool strictReference;   // строгая ссылка

        public EcmaType BaseValue { get; set; }
        public string ReferenceName { get; set; }
        public bool StrictReference { get; set; }
    }
    //public abstract class EnvironmentRecord
    //{
    //    /// <summary>
    //    /// Определяет, имеет ли запись окружения привязку к индентификатору
    //    /// </summary>
    //    /// <param name="name">Текст индентификатора</param>
    //    /// <returns></returns>
    //    protected abstract bool hasBinding(string name)
    //    {
    //        return false;
    //    }
    //    /// <summary>
    //    /// Создает в записи окружения новую изменяемую привязку.
    //    /// </summary>
    //    /// <param name="name">Привязанное имя</param>
    //    /// <param name="delete">Привязка в последствии может быть удалена</param>
    //    /// <returns></returns>
    //    protected abstract bool createMutableBinding(string name, bool delete)
    //    {
    //        return false;
    //    }
        
    //}
    //public class DeclarativeEnviromentRecord : EnvironmentRecord
    //{
        
    //}
    //public class ObjectEnviromentRecord : EnvironmentRecord
    //{

    //}
    //public class LexicalEnvironment : EcmaType
    //{
    //    private EnvironmentRecord record;
    //    private LexicalEnvironment externalLexicalEnvironment; // ссылка! на внешнее лексическое окружение
    //}

    
}
    #endregion
