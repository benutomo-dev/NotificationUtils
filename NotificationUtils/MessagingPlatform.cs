using System;
using System.Diagnostics;
using System.Linq;

namespace NotificationUtils
{
    public class MessagingPlatform
    {
        IMessageListener[] messageListeners;

        public MessagingToken Token => new MessagingToken(this);

        public MessagingPlatform(params IMessageListener[] messageListeners)
        {
            this.messageListeners = messageListeners;
        }

        internal void Send<MessageIdEnumType>(MessageIdEnumType messageId, MessageData.Builder? dataSource) where MessageIdEnumType : struct
        {
            if (messageListeners is null)
            {
                return;
            }

            if (MessagingConfiguration.EnableMessageDataValidation)
            {
                if (MessageTraits<MessageIdEnumType>.MessageTraitDict.TryGetValue(messageId, out var traitsData))
                {
                    var missingContextKeys = traitsData.ContextKeyTraitDict?.Keys.Where(v => dataSource is null || !dataSource.ContainsKey(v)).ToList();
                    Trace.Assert(missingContextKeys?.Count == 0);
                }
            }

            MessageData<MessageIdEnumType>? messageData = null;

            foreach (var typelessListener in messageListeners)
            {
                if (!(typelessListener is IMessageListener<MessageIdEnumType> listener))
                {
                    continue;
                }

                try
                {
                    if (listener.OnMessagePreview(messageId))
                    {
                        messageData = messageData ?? dataSource?.Build(messageId) ?? MessageData.Empty(messageId);

                        listener.OnMessage(messageData);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }
}
