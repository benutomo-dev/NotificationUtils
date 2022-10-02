using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationUtils
{
    public interface IMessageListener
    {
    }
    public interface IMessageListener<MessageIdEnumType> : IMessageListener where MessageIdEnumType: struct
    {
        bool OnMessagePreview(MessageIdEnumType message);

        void OnMessage(MessageData<MessageIdEnumType> messageContext);
    }
}
