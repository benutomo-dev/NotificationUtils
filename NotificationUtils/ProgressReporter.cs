using System;

namespace NotificationUtils
{
    public class ProgressReporter
    {
        public event Action<Exception> UnhandledException;

        public event Action ProgressDegreeChanged;

        public double MinProgressDegree => root.MinProgressDegree;

        public double? ProgressDegree => root.ProgressDegree;


        public ProgressToken Token => root.Token;

        ProgressNode root;
        public ProgressReporter()
        {
            root = new ProgressNode(0);
            root.ProgressDegreeChanged += DirectNotifyProgressDegreeChanged;
        }

        private void DirectNotifyProgressDegreeChanged()
        {
            try
            {
                ProgressDegreeChanged?.Invoke();
            }
            catch (Exception ex)
            {
                UnhandledException?.Invoke(ex);
            }
        }
    }
}
