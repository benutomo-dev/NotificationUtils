using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationUtils
{
    class MessageTraits<MessageIdEnumType> where MessageIdEnumType : struct
    {
        public class ContextKeyTrait
        {
            public string ContextKey { get; }
            public TypeConverter CustumConverter { get; }
            public ICustomFormatter CustumFormatter { get; }

            public bool Visible { get; }

            public int OrderPriority { get; }

            public ContextKeyTrait(MessageDataDefinitionAttribute contextKeysAttribute)
            {
                ContextKey = contextKeysAttribute.KeyName;
                CustumConverter = contextKeysAttribute.CustumConverter is null ? null : (TypeConverter)Activator.CreateInstance(Type.GetType(contextKeysAttribute.CustumConverter));
                CustumFormatter = contextKeysAttribute.CustumFormatter is null ? null : (ICustomFormatter)Activator.CreateInstance(Type.GetType(contextKeysAttribute.CustumFormatter));
                Visible = contextKeysAttribute.Visible;
                OrderPriority = contextKeysAttribute.OrderPriority;
            }
        }

        public class MessageTrait
        {
            public Dictionary<string, ContextKeyTrait> ContextKeyTraitDict { get; set; }
        }

        public static Dictionary<MessageIdEnumType, MessageTrait> MessageTraitDict { get; } = new Dictionary<MessageIdEnumType, MessageTrait>();

        static MessageTraits()
        {
            if (!typeof(MessageIdEnumType).IsEnum)
            {
                throw new NotSupportedException("メッセージは列挙型で定義する必要があります。");
            }

            foreach (var messageName in Enum.GetNames(typeof(MessageIdEnumType)))
            {
                if (!Enum.TryParse(messageName, out MessageIdEnumType value))
                {
                    throw new Exception("列挙型のフィールド名から値を得られませんでした。");
                }

                if (MessageTraitDict.ContainsKey(value))
                {
                    throw new NotSupportedException();
                }

                var fieldInfo = typeof(MessageIdEnumType).GetField(messageName);

                var ContextKeyTraits = fieldInfo.GetCustomAttributes(typeof(MessageDataDefinitionAttribute), false)
                                                .OfType<MessageDataDefinitionAttribute>()
                                                .Select(v => new ContextKeyTrait(v))
                                                .ToLookup(v => v.ContextKey);

                // 複数の属性の間においてキー名の重複は認めない
                var dupContextKeyNames = ContextKeyTraits.Where(v => v.Count() > 1).Select(v => v.Key).ToList();
                if (dupContextKeyNames.Count > 0)
                {
                    throw new Exception($"{typeof(MessageIdEnumType).Name}の{messageName}に{nameof(MessageDataDefinitionAttribute.KeyName)}が重複している{nameof(MessageDataDefinitionAttribute)}が含まれています。{string.Join(",", dupContextKeyNames)}");
                }

                MessageTraitDict[value] = new MessageTrait
                {
                    ContextKeyTraitDict = ContextKeyTraits.SelectMany(v => v).ToDictionary(v => v.ContextKey, v => v),
                };
            }
        }
    }
}
