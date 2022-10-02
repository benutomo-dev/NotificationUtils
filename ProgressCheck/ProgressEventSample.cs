using NotificationUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
