namespace NotificationUtils
{
    public struct MessagingToken
    {
        MessagingPlatform MessagingPlatform { get; }

        public static MessagingToken None { get; } = new MessagingToken();

        internal MessagingToken(MessagingPlatform messagingPlatform)
        {
            MessagingPlatform = messagingPlatform;
        }

        public void Send<MessageIdEnumType>(MessageIdEnumType messageId, MessageData.Builder dataSource) where MessageIdEnumType : struct
        {
            MessagingPlatform?.Send(messageId, dataSource);
        }

        public void Send<MessageIdEnumType>(MessageIdEnumType messageId) where MessageIdEnumType : struct
        {
            MessagingPlatform?.Send(messageId, null);
        }
    }
}
