using System;

namespace NotificationUtils
{
    public class DelegateMessageListener<MessageIdEnumType> : IMessageListener<MessageIdEnumType> where MessageIdEnumType:struct
    {
        Action<MessageData<MessageIdEnumType>> OnMessageDelegate { get; }

        Func<MessageIdEnumType, bool> OnMessagePreviewDelegate { get; }

        public DelegateMessageListener(Action<MessageData<MessageIdEnumType>> onMessageDelegate): this(null, onMessageDelegate)
        {
        }

        public DelegateMessageListener(Func<MessageIdEnumType, bool> onMessagePreviewDelegate, Action<MessageData<MessageIdEnumType>> onMessageDelegate)
        {
            OnMessagePreviewDelegate = onMessagePreviewDelegate;
            OnMessageDelegate = onMessageDelegate;
        }

        public void OnMessage(MessageData<MessageIdEnumType> messageContext)
        {
            OnMessageDelegate?.Invoke(messageContext);
        }

        public bool OnMessagePreview(MessageIdEnumType message)
        {
            return OnMessagePreviewDelegate?.Invoke(message) ?? true;
        }
    }
}
