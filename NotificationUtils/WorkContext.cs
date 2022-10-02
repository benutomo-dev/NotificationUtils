namespace NotificationUtils
{
    public abstract class WorkContext
    {
        public static WorkContext Default { get; } = new EmptyContext();

        private class EmptyContext : WorkContext
        {
        }
    }
}
