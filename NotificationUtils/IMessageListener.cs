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
