using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace NotificationUtils
{
    public class MessageData<MessageIdEnumType> : MessageData, IFormattable where MessageIdEnumType : struct
    {
        Dictionary<string, string>? stringCache;

        string? toStringCache;

        MessageTraits<MessageIdEnumType>.MessageTrait messageTraitsData;

        public MessageIdEnumType Message { get; }

        public MessageData(MessageIdEnumType message, ImmutableDictionary<string, object>? context): base(context)
        {
            if (!MessageTraits<MessageIdEnumType>.MessageTraitDict.TryGetValue(message, out messageTraitsData))
            {
                throw new ArgumentException(nameof(message));
            }

#if DEBUG
            var missingContextKeys = messageTraitsData.ContextKeyTraitDict?.Keys.Where(v => !this.ContainsKey(v)).ToList();
            Trace.Assert(missingContextKeys?.Count == 0, $"{typeof(MessageIdEnumType).Name}で定義されているキーが不足しているメッセージを作成しようとしています。{string.Join(",", missingContextKeys)}");
#endif

            Message = message;
        }


        public string this[string key]
        {
            get
            {
                if (keyValuePairs is null)
                {
                    return string.Empty;
                }

                if (stringCache != null && stringCache.TryGetValue(key, out var cachedValue))
                {
                    return cachedValue;
                }

                string stringValue;

                if (keyValuePairs.TryGetValue(key, out var value))
                {
                    if (messageTraitsData.ContextKeyTraitDict is not null && messageTraitsData.ContextKeyTraitDict.TryGetValue(key, out var trait) && trait.CustumConverter is TypeConverter compelTypeConverter)
                    {
                        stringValue = compelTypeConverter.ConvertToString(value) ?? string.Empty;
                    }
                    else if (value is string)
                    {
                        stringValue = (string)value;
                    }
                    else
                    {
                        var valueType = value.GetType();
                        var converter = TypeDescriptor.GetConverter(valueType);

                        if (converter?.CanConvertTo(typeof(string)) ?? false)
                        {
                            stringValue = converter.ConvertToString(value) ?? string.Empty;
                        }
                        else
                        {
                            stringValue = value?.ToString() ?? string.Empty;
                        }
                    }
                }
                else
                {
                    stringValue = string.Empty;
                }

                WithCreate(ref stringCache).Add(key, stringValue);
                return stringValue;
            }
        }

        public override string ToString() => ToString(default(string), default(IFormatProvider));

        public string ToString(string? format) => ToString(format, default(IFormatProvider));

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (format is null)
            {
                if (keyValuePairs is null)
                {
                    return $"{Message.ToString()}{{Empty}}";
                }

                if (toStringCache is null)
                {
                    var valueListQuery = keyValuePairs.Keys
                                    .Select(v =>
                                    {
                                        int orderPriority;
                                        bool visible;
                                        if (messageTraitsData.ContextKeyTraitDict is not null && messageTraitsData.ContextKeyTraitDict.TryGetValue(v, out var traits))
                                        {
                                            orderPriority = traits.OrderPriority;
                                            visible = traits.Visible;
                                        }
                                        else
                                        {
                                            orderPriority = MessageDataDefinitionAttribute.DefautOrderPriority;
                                            visible = false;
                                        }

                                        return (name:v, orderPriority, visible);
                                    })
                                    .Where(v => v.visible)
                                    .OrderBy(v => v.orderPriority)
                                    .ThenBy(v => v.name)
                                    .Select(v => $"{v.name}:{this[v.name]}");

                    toStringCache = $"{Message.ToString()}{{{string.Join(",", valueListQuery)}}}";
                }
                return toStringCache;
            }

            if (format[format.Length - 1] == '{')
            {
                throw new FormatException("閉じられていない'{'が存在します。");
            }

            var openOrClose = new[] { '{', '}' };
            var formatingOrClose = new[] { ':', '}' };

            StringBuilder stringBuilder = new StringBuilder(1024);

            int current = 0;

            while (current < format.Length)
            {
                var braseIndex = format.IndexOfAny(openOrClose, current);

                if (braseIndex < 0)
                {
                    stringBuilder.Append(format, current, format.Length - current);
                    break;
                }
                else if (format[braseIndex] == format[braseIndex + 1])
                {
                    stringBuilder.Append(format[braseIndex]);
                    current += 2;
                    continue;
                }
                else if (format[braseIndex] == '}')
                {
                    throw new FormatException("'{'に対応しない'}'が存在します。");
                }
                else
                {
                    var fmtOrCloseIndex = format.IndexOfAny(formatingOrClose, current + 1);

                    if (fmtOrCloseIndex < 0)
                    {
                        throw new FormatException("閉じられていない'{'が存在します。");
                    }
                    else
                    {
                        if (braseIndex - current > 0)
                        {
                            stringBuilder.Append(format, current, braseIndex - current);
                        }

                        string contextKey = format.Substring(braseIndex + 1, fmtOrCloseIndex - braseIndex - 1);
                        if (format[fmtOrCloseIndex] == ':')
                        {
                            var closeIndex = format.IndexOf('}', fmtOrCloseIndex + 1);

                            if (closeIndex < 0)
                            {
                                throw new FormatException("閉じられていない'{'が存在します。");
                            }
                            else
                            {
                                string valueFormat = format.Substring(fmtOrCloseIndex + 1, closeIndex - fmtOrCloseIndex - 1);
                                if (TryGetValue(contextKey, out var value))
                                {
                                    if (messageTraitsData.ContextKeyTraitDict is not null && messageTraitsData.ContextKeyTraitDict.TryGetValue(contextKey, out var trait) && trait.CustumFormatter is ICustomFormatter customFormatter)
                                    {
                                        stringBuilder.Append(customFormatter.Format(valueFormat, value, formatProvider) ?? string.Empty);
                                    }
                                    else if (value is IFormattable formattable)
                                    {
                                        stringBuilder.Append(formattable.ToString(valueFormat, formatProvider) ?? string.Empty);
                                    }
                                    else
                                    {
                                        stringBuilder.Append(this[contextKey]);
                                    }
                                }
                            }

                            current = closeIndex + 1;
                        }
                        else
                        {
                            if (!(formatProvider is null) && TryGetValue(contextKey, out var value) && value is IFormattable formattable)
                            {
                                stringBuilder.Append(formattable.ToString(null, formatProvider) ?? string.Empty);
                            }
                            else
                            {
                                stringBuilder.Append(this[contextKey]);
                            }

                            current = fmtOrCloseIndex + 1;
                        }

                        continue;
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }

    public abstract class MessageData
    {
        internal protected ImmutableDictionary<string, object>? keyValuePairs;

        protected MessageData(ImmutableDictionary<string, object>? context)
        {
            this.keyValuePairs = context;
        }

        public static MessageData<MessageIdEnumType> Empty<MessageIdEnumType>(MessageIdEnumType message) where MessageIdEnumType : struct
        {
            return new MessageData<MessageIdEnumType>(message, null);
        }


        public bool ContainsKey(string key) => keyValuePairs?.ContainsKey(key) ?? false;

        public bool TryGetValue(string key,[NotNullWhen(true)] out object? value)
        {
            if (keyValuePairs is null)
            {
                value = default;
                return false;
            }

            return keyValuePairs.TryGetValue(key, out value);
        }
        public bool TryGetValueAs<T>(string key, [NotNullWhen(true)] out T? value)
        {
            if (keyValuePairs is null || !keyValuePairs.TryGetValue(key, out var objectValue) || !(objectValue is T))
            {
                value = default;
                return false;
            }

            value = (T)objectValue;
            return true;
        }

        public T GetValueAs<T>(string key)
        {
            if (keyValuePairs is null || !keyValuePairs.TryGetValue(key, out var objectValue))
            {
                throw new KeyNotFoundException();
            }

            // 型違いはInvalidCastException
            return (T)objectValue;
        }

        public T GetValueAs<T>(string key, T defaultValue)
        {
            if (keyValuePairs is null || !keyValuePairs.TryGetValue(key, out var objectValue) || !(objectValue is T))
            {
                return defaultValue;
            }

            return (T)objectValue;
        }

        public T GetValueAs<T>(string key, Func<T> defaultValueFunc)
        {
            if (defaultValueFunc is null)
            {
                throw new ArgumentNullException(nameof(defaultValueFunc));
            }

            if (keyValuePairs is null || !keyValuePairs.TryGetValue(key, out var objectValue) || !(objectValue is T))
            {
                return defaultValueFunc.Invoke();
            }

            return (T)objectValue;
        }


        internal protected static ImmutableDictionary<TKey, TVal>.Builder WithCreate<TKey, TVal>(ref ImmutableDictionary<TKey, TVal>.Builder? member)
        {
            if (member is null) member = ImmutableDictionary.CreateBuilder<TKey, TVal>();
            return member;
        }
        internal protected static Dictionary<TKey, TVal> WithCreate<TKey, TVal>(ref Dictionary<TKey, TVal>? member)
        {
            if (member is null) member = new Dictionary<TKey, TVal>();
            return member;
        }

        public class Builder : IDictionary<string, object>
        {
            ImmutableDictionary<string, object>.Builder? keyValuePairs;

            public object this[string key]
            {
                get
                {
                    if (keyValuePairs is null) throw new KeyNotFoundException();
                    return keyValuePairs[key];
                }
                set => WithCreate(ref keyValuePairs)[key] = value;
            }

            public ICollection<string> Keys => ((IDictionary<string, object>?)keyValuePairs)?.Keys ?? Array.Empty<string>();

            public ICollection<object> Values => ((IDictionary<string, object>?)keyValuePairs)?.Values ?? Array.Empty<object>();

            public int Count => keyValuePairs?.Count ?? 0;

            bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

            public void Add(string key, object value) => WithCreate(ref keyValuePairs).Add(key, value);

            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => WithCreate(ref keyValuePairs).Add(item);

            public void Clear() => keyValuePairs = null;

            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => keyValuePairs?.Contains(item) ?? false;

            public bool ContainsKey(string key) => keyValuePairs?.ContainsKey(key) ?? false;

            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                var localKeyValuePairs = keyValuePairs;
                if (localKeyValuePairs != null)
                {
                    ((IDictionary<string, object>)localKeyValuePairs).CopyTo(array, arrayIndex);
                }
            }

            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => keyValuePairs?.GetEnumerator() ?? Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();

            public bool Remove(string key)
            {
                return keyValuePairs?.Remove(key) ?? false;
            }

            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
            {
                return keyValuePairs?.Remove(item) ?? false;
            }

            public bool TryGetValue(string key, out object value)
            {
                var localKeyValuePairs = keyValuePairs;
                if (localKeyValuePairs is null)
                {
                    value = null!;
                    return false;
                }
                else
                {
                    return localKeyValuePairs.TryGetValue(key, out value);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => keyValuePairs?.GetEnumerator() ?? Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();

            public MessageData<MessageIdEnumType> Build<MessageIdEnumType>(MessageIdEnumType messageId) where MessageIdEnumType : struct
            {
                return new MessageData<MessageIdEnumType>(messageId, keyValuePairs?.ToImmutable() ?? ImmutableDictionary<string, object>.Empty);
            }
        }
    }
}
