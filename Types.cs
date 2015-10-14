using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter.Types
{
    class EcmaValue
    {
        public TypeTag tag;
        public object value;
        public EcmaValue(TypeTag tag, object value)
        {

        }
    }
    public enum TypeTag
    {
        /*  Языковые типы   */
        UNDEFINED,
        NULL,
        BOOLEAN,
        STRING,
        NUMBER,
        OBJECT,

        /* Типы спецификации */
        PROPERTY_DESCRIPTOR
    }
    public abstract class Type
    {
        public readonly TypeTag typeTag;
        protected Type(TypeTag typeTag)
        {
            this.typeTag = typeTag;
        }
    }
    public class UndefinedType : Type
    {
        private static UndefinedType undefinedValue;
        private UndefinedType()
            : base(TypeTag.UNDEFINED)
        { }
        public static UndefinedType UndefinedValue
        {
            get
            {
                if (undefinedValue == null)
                {
                    return new UndefinedType();
                }
                return undefinedValue;
            }
        }
    }
    public class NullType : Type
    {
        private static NullType instance; // singleton

        private NullType()
            : base(TypeTag.NULL)
        { }

        public static NullType Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NullType();
                }
                return instance;
            }
        }
    }
    public class BooleanType : Type
    {
        private static BooleanType trueValue;
        private static BooleanType falseValue;

        private bool value;
        private BooleanType(bool value)
            : base(TypeTag.BOOLEAN)
        {
            this.value = value;
        }
        public bool Value
        {
            get { return value; }
            set { this.value = value; }
        }
        public static BooleanType TrueValue
        {
            get
            {
                if (trueValue == null)
                {
                    return new BooleanType(true);
                }
                return trueValue;
            }
        }
        public static BooleanType FalseValue
        {
            get
            {
                if (falseValue == null)
                {
                    return new BooleanType(false);
                }
                return falseValue;
            }
        }
    }
    public class StringType : Type
    {
        private string value;
        public StringType(string value)
            : base(TypeTag.STRING)
        {
            this.value = value;
        }
        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }
    }
    public class NumberType : Type
    {
        private double value;
        //private bool posInfinity;
        //private bool negInfinity;
        //private bool NaN;

        public NumberType(double value)
            : base(TypeTag.NUMBER)
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
            {
                this.value = value;
            }
        }
    }
    public class ObjectType : Type
    {
        private Dictionary<string, DataDescriptor> namedDataProperties;   // ассоциирует имя со значением и набором булевых атрибутов
        private Dictionary<string, AcessorDescriptor> namedAccessorProperties; // ассоциирует имя с одной или двумя функциями доступа и с набором булевых атрибутов
        
        /*  Внутренние свойства каждого объекта */
        private Type prototype;     // Прототип данного объекта [Object/NULL]
        private StringType _class;      // Классифицаия объектов
        private BooleanType extensible;    // Если true, к объекту могут быть добавлены собственные свойства
        /* ------------------------------------*/

        public ObjectType()
            : base(TypeTag.OBJECT)
        {
            namedDataProperties = new Dictionary<string, DataDescriptor>();
            namedAccessorProperties = new Dictionary<string, AcessorDescriptor>();
        }

        /* Внутренние методы каждого объекта */
        private Type getOwnProperty(string propertyName) // возвращает дескриптор свойства
        {

            if (!namedDataProperties.ContainsKey(propertyName) || !namedAccessorProperties.ContainsKey(propertyName))
                return UndefinedType.UndefinedValue;
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
        /*
         * Полностью заполненный дескриптор свойств является либо дескриптором свойств-аксессоров,
         * либо дескриптором свойств данных, при этом все его поля соответствуют атрибутам свойства,
         * описанных либо в Таблице 5, либо в Таблице 6 пункта 8.6.1.
         */
        public Type GetProperty(string propertyName) // возвращает полностью заполненный дескриптор свойства
        {
            Type prop = getOwnProperty(propertyName);
            if (!(prop.typeTag == TypeTag.UNDEFINED))
            {
                return prop;
            }
            else
            {
                Type proto = this.prototype;
                if (proto.typeTag == TypeTag.NULL) return UndefinedType.UndefinedValue;
                return ((ObjectType)proto).getOwnProperty(propertyName);
            }
        }
        private Type get(string propertyName)   // возвращает значение именнованного свойства
        {
            Type desc = this.GetProperty(propertyName);
            if (desc.typeTag == TypeTag.UNDEFINED) return UndefinedType.UndefinedValue;
            if (desc is DataDescriptor)
            {
                return ((DataDescriptor)desc).Value;
            }
            else//if (desc is AcessorDescriptor)
            {
                Type getter = ((AcessorDescriptor)desc).Get;
                if (getter.typeTag == TypeTag.UNDEFINED) return UndefinedType.UndefinedValue;
                return null; // TODO: вызвать внутренний метод CALL для getter передавая О в качестве значения this и непередавая никаких аргументов
            }
        }

        // QUESTION: как реализовывать?
        // canPut - Возвращает булево значение, означающее возможность выполнения операции [[Put]] с именем свойства propertyName.
        private BooleanType canPut(string propertyName)
        {
            Type desc = getOwnProperty(propertyName);
            if (!(desc.typeTag == TypeTag.UNDEFINED))
            {
                if (desc is AcessorDescriptor)
                {
                    if (((AcessorDescriptor)desc).Set.typeTag == TypeTag.UNDEFINED)
                    {
                        return BooleanType.FalseValue;
                    }
                    else
                    {
                        return BooleanType.TrueValue;
                    }
                }
                else
                {
                    if (((DataDescriptor)desc).Writable)
                    {
                        return BooleanType.TrueValue;
                    }
                    else
                    {
                        return BooleanType.FalseValue;
                    }
                }
            }
            Type proto = prototype;
            if (proto.typeTag == TypeTag.NULL) return extensible;
            Type inherited = ((ObjectType)proto).GetProperty(propertyName);
            if (inherited.typeTag == TypeTag.UNDEFINED) return extensible;
            if (inherited is AcessorDescriptor)
            {
                if (((AcessorDescriptor)inherited).Set.typeTag == TypeTag.UNDEFINED) return BooleanType.FalseValue;
                else return BooleanType.TrueValue;
            }
            else
            {
                if (extensible.Value == false)
                {
                    return BooleanType.FalseValue;
                }
                else
                {
                    if (((DataDescriptor)inherited).Writable)
                    {
                        return BooleanType.TrueValue;
                    }
                    else
                    {
                        return BooleanType.FalseValue;
                    }
                }
            }
        }

        // TODO: 8.12.5 реализовать
        // put - Присваивает значению второго параметра заданное именованное свойство.
        // Обработка отказов контролируется с помощью флага.
        private BooleanType put(string propertyName, Type val, bool needThrow)
        {
            if (canPut(propertyName).Value == false)
            {
                if (needThrow)
                {
                    throw new Exception("TypeError");
                }
                else
                {
                    return canPut(propertyName);
                }
            }
            Type ownDesc = getOwnProperty(propertyName);
            if (ownDesc is DataDescriptor)
            {

            }
            return null;
        }
        

        // Возвращает булево значение, которое указывает, есть ли уже у данного объекта свойство с таким именем.
        private BooleanType hasProperty(string propertyName)
        {
            Type desc = GetProperty(propertyName);
            if (desc.typeTag == TypeTag.UNDEFINED) return BooleanType.FalseValue;
            return BooleanType.TrueValue;
        }
        
        // TODO: 8.12.7 реализовать
        // Удаляет из объекта заданное именованное собственное свойство. Обработка отказов контролируется с помощью флага.
        private BooleanType delete(string propertyName, bool flag)
        {
            //return new BooleanType(namedDataProperties.Remove(propertyName));
            return null;
        }

        // TODO: 8.12.8
        private Type defaultValue(string hint)
        {
            return null;
        }
        // TODO: 8.12.9
        private BooleanType defineOwnProperty(string propertyName, PropertyDescriptorType descriptor, bool needThrow)
        {
            // TODO: 8.12.9 
            return null;
        }
        /* ------------------------------------*/
    }
    
    public abstract class PropertyDescriptorType : Type
    {
        private bool enumerable;    // Если true, данное свойство можно перечислить с помощью for-in
        private bool configurable;  // Если false, нельзя будет удалить данное свойство, измнеить его, сделав свойством-аксессором
                                    // или изменить его атрибуты (кроме Value)
        protected PropertyDescriptorType()
            : base(TypeTag.PROPERTY_DESCRIPTOR)
        {
            enumerable = false;
            configurable = false;
        }
        public bool Enumerable
        {
            get { return enumerable; }
            set { this.enumerable = value; }
        }
        public bool Configurable
        {
            get { return configurable; }
            set { this.enumerable = value; }
        }
    }
    public class DataDescriptor : PropertyDescriptorType
    {
        private Type value;             // Значение, извлеченное посредством чтения свойства
        private bool writable;          // Если false, нельзя взять значение value с помощью Put

        public DataDescriptor()
        {
            value = UndefinedType.UndefinedValue;
            writable = false;
        }

        public Type Value
        {
            get
            { return value; }
            set
            { this.value = value; }
        }
        public bool Writable
        {
            get { return writable; }
            set { this.writable = value; }
        }
    }
    public class AcessorDescriptor : PropertyDescriptorType
    {
        private Type get;   // undefined или object
        private Type set;   // undefined или object

        public AcessorDescriptor()
        {
            get = UndefinedType.UndefinedValue;
            set = UndefinedType.UndefinedValue;
        }
        public Type Get
        {
            get { return get; }
            set { this.get = value; }
        }
        public Type Set
        {
            get { return set; }
            set { this.set = value; }
        }
    }

 

}
