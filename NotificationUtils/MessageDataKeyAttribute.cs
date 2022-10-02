using System;

namespace NotificationUtils
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class MessageDataDefinitionAttribute : Attribute
    {
        public static readonly int DefautOrderPriority = 1000;

        public string KeyName { get; }

        public string CustumConverter { get; }

        public string CustumFormatter { get; }

        public bool Visible { get; set; }

        public int OrderPriority { get; set; }

        public MessageDataDefinitionAttribute(string keyName) : this(keyName, default(string), default(string))
        {
        }
        public MessageDataDefinitionAttribute(string keyName, Type custumStringConverter = null, Type custumFormatter = null) : this(keyName, custumStringConverter?.AssemblyQualifiedName, custumFormatter?.AssemblyQualifiedName)
        {
        }
        public MessageDataDefinitionAttribute(string contextKey, string custumStringConverter = null, string custumFormatter = null)
        {
            KeyName = contextKey;
            CustumConverter = custumStringConverter;
            CustumFormatter = custumFormatter;
            Visible = true;
            OrderPriority = DefautOrderPriority;
        }
    }
}
