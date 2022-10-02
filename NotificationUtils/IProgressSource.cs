using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
