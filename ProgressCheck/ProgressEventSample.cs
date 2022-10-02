using NotificationUtils;

namespace ProgressCheck
{
    public static class ProgressEventSampleKeys
    {
        public const string ProcessName = nameof(ProcessName);
    }

    enum ProgressEventSample
    {
        [MessageDataDefinition(ProgressEventSampleKeys.ProcessName)]
        BeginChildProcess,

        [MessageDataDefinition(ProgressEventSampleKeys.ProcessName)]
        EndChildProcess,

        CompletedProcess,
    }
}
