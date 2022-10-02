using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

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
