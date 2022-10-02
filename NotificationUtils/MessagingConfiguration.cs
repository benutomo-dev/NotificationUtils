using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace NotificationUtils
{
    public static class MessagingConfiguration
    {
        public static bool EnableMessageDataValidation { get; set; }

        static MessagingConfiguration()
        {
            SetMessageDataValidationByAssemblyDebuggableAttribute(Assembly.GetEntryAssembly());
        }

        public static void SetMessageDataValidationByAssemblyDebuggableAttribute(Assembly assembly)
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(DebuggableAttribute), true);
            if (attributes == null || attributes.Length == 0)
            {
                EnableMessageDataValidation = false;
                return;
            }

            foreach (DebuggableAttribute attribute in attributes)
            {
                if (attribute.IsJITTrackingEnabled)
                {
                    EnableMessageDataValidation = true;
                    return;
                }
            }

            EnableMessageDataValidation = false;
            return;
        }
    }
}
