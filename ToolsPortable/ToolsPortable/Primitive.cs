using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    [DataContract]
    public abstract class Primitive
    {
        public long Id { get; set; }
    }

    [DataContract]
    public class StringDb : Primitive, IEquatable<string>, IEquatable<StringDb>, IComparable<string>, IComparable<StringDb>
    {
        public StringDb()
        {
            //must have a parameterless constructor for Entity
        }

        public StringDb(string value)
        {
            Value = value;
        }

        [DataMember]
        public string Value { get; set; }

        public bool Equals(string other)
        {
            return Value.Equals(other);
        }

        public bool Equals(StringDb other)
        {
            return Equals(other.Value);
        }

        public int CompareTo(string other)
        {
            return Value.CompareTo(other);
        }

        public int CompareTo(StringDb other)
        {
            return CompareTo(other.Value);
        }
    }

    [DataContract]
    public class LongDb : Primitive, IEquatable<long>, IEquatable<LongDb>, IComparable<long>, IComparable<LongDb>
    {
        public LongDb() { }

        public LongDb(long value)
        {
            Value = value;
        }

        [DataMember]
        public long Value { get; set; }

        public bool Equals(long other)
        {
            return Value == other;
        }

        public bool Equals(LongDb other)
        {
            return Equals(other.Value);
        }

        public int CompareTo(long other)
        {
            return Value.CompareTo(other);
        }

        public int CompareTo(LongDb other)
        {
            return CompareTo(other.Value);
        }
    }

    public class DictionaryLongLong : DictionaryOfItems<long, long>
    {
        public DictionaryLongLong(object owner, string propertyNameOfRawData) : base(owner, propertyNameOfRawData) { }
        public DictionaryLongLong(object owner, string propertyNameOfRawData, IEnumerable<KeyValuePair<long, long>> other) : base(owner, propertyNameOfRawData, other) { }

        protected override IParser<long> getKeyParser()
        {
            return LongParser.Parser;
        }

        protected override IParser<long> getValueParser()
        {
            return LongParser.Parser;
        }
    }

    public class RawProvider
    {
        public RawProvider()
        {
            Raw = "";
        }

        public string Raw { get; set; }
    }

    public abstract class DictionaryOfItems<T, K> : IEnumerable<KeyValuePair<T, K>>
    {
        private object _owner;
        private PropertyInfo _property;

        public DictionaryOfItems(object owner, string propertyNameOfRawData)
        {
            _owner = owner;
            _property = owner.GetType().GetRuntimeProperty(propertyNameOfRawData);
        }

        public DictionaryOfItems(object owner, string propertyNameOfRawData, IEnumerable<KeyValuePair<T, K>> other) : this(owner, propertyNameOfRawData)
        {
            IEnumerator<KeyValuePair<T, K>> i = other.GetEnumerator();
            while (i.MoveNext())
                Add(i.Current);
        }

        public string RawData
        {
            get { return _property.GetValue(_owner, null) as string; }
            set { _property.SetValue(_owner, value, null); }
        }

        protected Dictionary<T, K> _cachedDictionary;
        protected virtual Dictionary<T, K> dictionary
        {
            get
            {
                if (_cachedDictionary == null)
                    _cachedDictionary = DictionaryAdapter.FromString(RawData, getKeyParser(), getValueParser(), keySeparator, valueSeparator);

                return _cachedDictionary;
            }
        }

        protected abstract IParser<T> getKeyParser();
        protected abstract IParser<K> getValueParser();

        protected virtual string keySeparator
        {
            get { return DictionaryAdapter.KEY_SEPARATOR; }
        }

        protected virtual string valueSeparator
        {
            get { return DictionaryAdapter.VALUE_SEPARATOR; }
        }

        private void append(T key, K value)
        {
            if (RawData.Length != 0)
                RawData += valueSeparator;

            RawData += getKeyParser().ToString(key) + keySeparator + getValueParser().ToString(value);
        }

        private void reset()
        {
            RawData = DictionaryAdapter.ToString(dictionary, getKeyParser(), getValueParser());
        }

        /// <summary>
        /// Returns a NEW dictionary of the current items.
        /// </summary>
        /// <returns></returns>
        public Dictionary<T, K> ToDictionary()
        {
            return new Dictionary<T, K>(dictionary);
        }

        public void Add(T key, K value)
        {
            bool canAppend = !ContainsKey(key); //if we're simply adding it to the end

            dictionary[key] = value;

            if (canAppend)
                append(key, value);
            else
                reset();
        }

        public void AddRange(IEnumerable<KeyValuePair<T, K>> other)
        {
            IEnumerator<KeyValuePair<T, K>> i = other.GetEnumerator();
            while (i.MoveNext())
                Add(i.Current);
        }

        public bool ContainsKey(T key)
        {
            return dictionary.ContainsKey(key);
        }

        public ICollection<T> Keys
        {
            get { return dictionary.Keys; }
        }

        public bool Remove(T key)
        {
            bool answer = dictionary.Remove(key);
            reset();
            return answer;
        }

        /// <summary>
        /// Throws exception if key not found
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public K Get(T key)
        {
            return dictionary[key];
        }

        public bool TryGetValue(T key, out K value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public ICollection<K> Values
        {
            get { return dictionary.Values; }
        }

        public void Add(KeyValuePair<T, K> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            RawData = "";
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<T, K> item)
        {
            return dictionary.Contains(item);
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<T, K> item)
        {
            return Remove(item.Key);
        }

        public Dictionary<T, K>.Enumerator GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator<KeyValuePair<T, K>> IEnumerable<KeyValuePair<T, K>>.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }

    public class OldDateTimeParser : IParser<DateTime>
    {
        public static OldDateTimeParser Parser = new OldDateTimeParser();

        public DateTime Parse(string str)
        {
            return DateTime.Parse(str);
        }

        public string ToString(DateTime value)
        {
            return value.ToString();
        }
    }

    public class DateTimeParser : IParser<DateTime>
    {
        public static DateTimeParser Parser = new DateTimeParser();

        public DateTime Parse(string str)
        {
            return DateTime.Parse(str, CultureInfo.InvariantCulture);
        }

        public string ToString(DateTime value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class DictionaryLongDate : DictionaryOfItems<long, DateTime>
    {
        public DictionaryLongDate(object owner, string propertyNameOfRawData) : base(owner, propertyNameOfRawData) { }
        public DictionaryLongDate(object owner, string propertyNameOfRawData, IEnumerable<KeyValuePair<long, DateTime>> other) : base(owner, propertyNameOfRawData, other) { }

        protected override IParser<long> getKeyParser()
        {
            return LongParser.Parser;
        }

        protected override IParser<DateTime> getValueParser()
        {
            return DateTimeParser.Parser;
        }

        protected override Dictionary<long, DateTime> dictionary
        {
            get
            {
                //support updating user data from CurrentCulture to CultureInvariant
                try
                {
                    return base.dictionary;
                }

                catch
                {
                    _cachedDictionary = DictionaryAdapter.FromString(RawData, getKeyParser(), OldDateTimeParser.Parser, keySeparator, valueSeparator);

                    return _cachedDictionary;
                }
            }
        }
    }

    public class ByteParser : IParser<byte>
    {
        public static readonly ByteParser Parser = new ByteParser();

        public byte Parse(string str)
        {
            return byte.Parse(str, CultureInfo.InvariantCulture);
        }

        public string ToString(byte value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class ListOfBytes : ListOfItems<byte>
    {
        public ListOfBytes(object owner, string propertyNameOfRawData) : base(owner, propertyNameOfRawData) { }
        public ListOfBytes(object owner, string propertyNameOfRawData, IEnumerable<byte> other) : base(owner, propertyNameOfRawData, other) { }

        protected override IParser<byte> getParser()
        {
            return ByteParser.Parser;
        }
    }

    public abstract class ListOfItems<T> : IEnumerable<T>
    {
        private object _owner;
        private PropertyInfo _property;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner">The class creating this list</param>
        /// <param name="propertyNameOfRawData">The property name of the RawData string inside the class you specified before.</param>
        public ListOfItems(object owner, string propertyNameOfRawData)
        {
            _owner = owner;
            _property = owner.GetType().GetRuntimeProperty(propertyNameOfRawData);
        }

        public ListOfItems(object owner, string propertyNameOfRawData, IEnumerable<T> list) : this(owner, propertyNameOfRawData)
        {
            _cachedList = new List<T>(list);
            resetData();
        }

        public string RawData
        {
            get { return _property.GetValue(_owner, null) as string; }
            set { _property.SetValue(_owner, value, null); }
        }

        protected List<T> _cachedList;
        protected List<T> list
        {
            get
            {
                if (_cachedList == null)
                    _cachedList = ListAdapter.FromString<T>(RawData, getParser());

                return _cachedList;
            }
        }

        protected abstract IParser<T> getParser();

        private void resetData()
        {
            RawData = ListAdapter.ToString(list, getParser());
        }

        public T Get(int index)
        {
            return list[index];
        }

        public void Set(int index, T item)
        {
            list[index] = item;
            resetData();
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        /// <summary>
        /// Inefficient.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public virtual void Insert(int index, T item)
        {
            list.Insert(index, item);
            resetData();
        }

        /// <summary>
        /// Inefficient
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
            resetData();
        }

        /// <summary>
        /// Efficient!
        /// </summary>
        /// <param name="item"></param>
        public virtual void Add(T item)
        {
            list.Add(item);

            if (RawData.Length != 0)
                RawData += ListAdapter.SEPARATOR;

            RawData += getParser().ToString(item);
        }

        public void AddRange(ListOfItems<T> other)
        {
            list.Capacity += other.Count;

            for (int i = 0; i < other.Count; i++)
                Add(other.Get(i));
        }

        /// <summary>
        /// Efficient!
        /// </summary>
        public void Clear()
        {
            RawData = "";
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Returns a NEW list of the items
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            return new List<T>(list);
        }

        /// <summary>
        /// Inefficient
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            bool answer = list.Remove(item);
            resetData();
            return answer;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }

    public abstract class SetOfItems<T> : ListOfItems<T>
    {
        public SetOfItems(object owner, string propertyNameOfRawData) : base(owner, propertyNameOfRawData) { }
        public SetOfItems(object owner, string propertyNameOfRawData, IEnumerable<T> items) : base(owner, propertyNameOfRawData)
        {
            IEnumerator<T> i = items.GetEnumerator();
            while (i.MoveNext())
                Add(i.Current);
        }

        public override void Add(T item)
        {
            if (!Contains(item))
                base.Add(item);
        }

        public override void Insert(int index, T item)
        {
            if (!Contains(item))
                base.Insert(index, item);
        }
    }

    class LongParser : IParser<long>
    {
        public static LongParser Parser = new LongParser();

        public long Parse(string str)
        {
            return long.Parse(str, CultureInfo.InvariantCulture);
        }

        public string ToString(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class ListOfLongs : ListOfItems<long>
    {
        public ListOfLongs(object owner, string propertyNameOfRawData) : base(owner, propertyNameOfRawData) { }
        public ListOfLongs(object owner, string propertyNameOfRawData, IEnumerable<long> items) : base(owner, propertyNameOfRawData, items) { }

        protected override IParser<long> getParser()
        {
            return LongParser.Parser;
        }
    }

    public class SetOfLongs : SetOfItems<long>
    {
        public SetOfLongs(object owner, string propertyNameOfRawData) : base(owner, propertyNameOfRawData) { }
        public SetOfLongs(object owner, string propertyNameOfRawData, IEnumerable<long> items) : base(owner, propertyNameOfRawData, items) { }

        protected override IParser<long> getParser()
        {
            return LongParser.Parser;
        }
    }

    public class StringParser : IParser<string>
    {
        public static StringParser Parser = new StringParser();

        public string Parse(string str)
        {
            return str;
        }

        public string ToString(string value)
        {
            return value;
        }
    }

    public class ListOfStrings : ListOfItems<string>
    {
        public ListOfStrings(object owner, string propertyNameOfRawData) : base(owner, propertyNameOfRawData) { }
        public ListOfStrings(object owner, string propertyNameOfRawData, IEnumerable<string> items) : base(owner, propertyNameOfRawData, items) { }

        protected override IParser<string> getParser()
        {
            return StringParser.Parser;
        }
    }

    public interface IParser<T>
    {
        string ToString(T value);

        T Parse(string str);
    }

    public class ListAdapter
    {
        public static readonly char SEPARATOR = ',';

        public static string ToString(IEnumerable list, string split)
        {
            if (list == null)
                return null;

            StringBuilder answer = new StringBuilder();
            IEnumerator i = list.GetEnumerator();

            if (i.MoveNext())
            {
                answer.Append(i.Current.ToString());

                while (i.MoveNext())
                    answer.Append(split).Append(i.Current.ToString());
            }

            return answer.ToString();
        }

        public static string ToString<T>(IEnumerable<T> list, IParser<T> parser)
        {
            return ToString(list, parser, SEPARATOR);
        }

        public static string ToString<T>(IEnumerable<T> list, IParser<T> parser, params char[] split)
        {
            if (list == null)
                return null;

            StringBuilder answer = new StringBuilder();
            IEnumerator<T> i = list.GetEnumerator();
            while (i.MoveNext())
                answer.Append(parser.ToString(i.Current)).Append(new string(split));

            return answer.ToString().TrimEnd(split);
        }

        public static List<long> FromLongs(string rawData)
        {
            return FromLongs(rawData, SEPARATOR);
        }

        public static List<T> FromString<T>(string rawData, IParser<T> parser)
        {
            return FromString<T>(rawData, parser, SEPARATOR);
        }

        public static List<T> FromString<T>(string rawData, IParser<T> parser, params char[] split)
        {
            if (rawData == null)
                return null;

            if (rawData.Length == 0)
                return new List<T>();

            string[] array = rawData.Split(split);

            List<T> answer = new List<T>();

            for (int i = 0; i < array.Length; i++)
                answer.Add(parser.Parse(array[i]));

            return answer;
        }

        public static List<long> FromLongs(string rawData, params char[] split)
        {
            if (rawData == null)
                return null;

            if (rawData.Length == 0)
                return new List<long>();

            string[] array = rawData.Split(split);

            List<long> answer = new List<long>();

            for (int i = 0; i < array.Length; i++)
                answer.Add(long.Parse(array[i]));

            return answer;
        }
    }

    public class DictionaryAdapter
    {
        public static readonly string KEY_SEPARATOR = "=";
        public static readonly string VALUE_SEPARATOR = "&";

        public static string ToString<T, K>(IEnumerable<KeyValuePair<T, K>> dictionary, IParser<T> keyParser, IParser<K> valueParser)
        {
            return ToString(dictionary, keyParser, valueParser, KEY_SEPARATOR, VALUE_SEPARATOR);
        }

        public static string ToString<T, K>(IEnumerable<KeyValuePair<T, K>> dictionary, IParser<T> keyParser, IParser<K> valueParser, string keySeparator, string valueSeparator)
        {
            if (dictionary == null)
                return null;

            StringBuilder answer = new StringBuilder();
            IEnumerator<KeyValuePair<T, K>> i = dictionary.GetEnumerator();
            while (i.MoveNext())
                answer.Append(keyParser.ToString(i.Current.Key)).Append(keySeparator).Append(valueParser.ToString(i.Current.Value)).Append(valueSeparator);

            return answer.ToString().TrimEnd(valueSeparator.ToCharArray());
        }

        public static Dictionary<T, K> FromString<T, K>(string str, IParser<T> keyParser, IParser<K> valueParser)
        {
            return FromString(str, keyParser, valueParser, KEY_SEPARATOR, VALUE_SEPARATOR);
        }

        public static Dictionary<T, K> FromString<T, K>(string str, IParser<T> keyParser, IParser<K> valueParser, string keySeparator, string valueSeparator)
        {
            if (str == null)
                return null;

            if (str.Length == 0)
                return new Dictionary<T, K>();

            StringBuilder buildingKey = new StringBuilder();
            StringBuilder buildingValue = null;

            int keySeparatorIndex = 0;
            int valueSeparatorIndex = 0;

            StringBuilder tempKeySeparator = new StringBuilder();
            StringBuilder tempValueSeparator = new StringBuilder();

            Dictionary<T, K> answer = new Dictionary<T, K>();

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (buildingValue == null)
                {
                    if (c == keySeparator[keySeparatorIndex])
                    {
                        keySeparatorIndex++;

                        if (keySeparatorIndex >= keySeparator.Length)
                        {
                            keySeparatorIndex = 0;
                            tempKeySeparator.Remove(0, tempKeySeparator.Length);

                            buildingValue = new StringBuilder();
                        }
                    }

                    else
                    {
                        keySeparatorIndex = 0;

                        buildingKey.Append(tempKeySeparator).Append(c);

                        tempKeySeparator.Remove(0, tempKeySeparator.Length);
                    }
                }

                else
                {
                    if (c == valueSeparator[valueSeparatorIndex])
                    {
                        valueSeparatorIndex++;

                        if (valueSeparatorIndex >= valueSeparator.Length)
                        {
                            valueSeparatorIndex = 0;
                            tempValueSeparator.Remove(0, tempValueSeparator.Length);

                            answer[keyParser.Parse(buildingKey.ToString())] = valueParser.Parse(buildingValue.ToString());

                            buildingValue = null;
                            buildingKey.Remove(0, buildingKey.Length);
                        }
                    }

                    else
                    {
                        valueSeparatorIndex = 0;

                        buildingValue.Append(tempValueSeparator).Append(c);

                        tempValueSeparator.Remove(0, tempValueSeparator.Length);
                    }
                }
            }

            if (buildingValue != null)
                answer[keyParser.Parse(buildingKey.ToString())] = valueParser.Parse(buildingValue.ToString());

            return answer;
        }
    }
}
