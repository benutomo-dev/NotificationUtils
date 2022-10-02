using System;

namespace NotificationUtils
{
    public interface IProgresSource
    {
        event Action WeightChanged;

        event Action ProgressDegreeChanged;

        int Weight { get; }

        double MinProgressDegree { get; }

        double? ProgressDegree { get; }
    }
}
