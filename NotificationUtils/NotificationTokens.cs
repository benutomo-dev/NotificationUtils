using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NotificationUtils
{
    public class NotificationTokens
    {
        public ProgressToken Progress { get; }
        public MessagingToken Messaging { get; }
        public CancellationToken Cancellation { get; }

        public static NotificationTokens Empty { get; } = new NotificationTokens(ProgressToken.None, MessagingToken.None, CancellationToken.None);

        public NotificationTokens(ProgressToken progress, MessagingToken messaging, CancellationToken cancellation)
        {
            Progress = progress;
            Messaging = messaging;
            Cancellation = cancellation;
        }

        public NotificationTokens CreateProgressBranchedTokens(int weight) => new NotificationTokens(Progress.CreateBranchedToken(weight), Messaging, Cancellation);

        public static implicit operator NotificationTokens(ProgressToken progress) => new NotificationTokens(progress, MessagingToken.None, CancellationToken.None);
        public static implicit operator NotificationTokens(MessagingToken messaging) => new NotificationTokens(ProgressToken.None, messaging, CancellationToken.None);
        public static implicit operator NotificationTokens(CancellationToken cancellation) => new NotificationTokens(ProgressToken.None, MessagingToken.None, cancellation);
    }
}
