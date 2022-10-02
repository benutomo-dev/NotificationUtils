using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationUtils
{
    public class ProgressLeaf : IProgresSource
    {
        public event Action WeightChanged;

        public event Action ProgressDegreeChanged;

        public double MinProgressDegree => Max == 0 ? 1 : (double)Current / Max;

        public double? ProgressDegree => _IsAmbiguousProgress ? (double?)null : MinProgressDegree;

        private int _Weight;
        public int Weight
        {
            get => _Weight;
            set
            {
                int normalizedValue = Math.Max(0, value);
                if (_Weight == normalizedValue) return;
                _Weight = normalizedValue;
                WeightChanged?.Invoke();
            }
        }


        private int _NotificationStep;
        public int NotificationStep
        {
            get => _NotificationStep;
            set
            {
                _NotificationStep = Math.Max(1, value);
            }
        }

        public bool _IsAmbiguousProgress;
        public bool IsAmbiguousProgress
        {
            get => _IsAmbiguousProgress;
            set
            {
                if (_IsAmbiguousProgress == value)
                {
                    return;
                }

                _IsAmbiguousProgress = value;
                ProgressDegreeChanged?.Invoke();
            }
        }

        private int _Current;
        public int Current
        {
            get => _Current;
            set
            {
                int normalizedValue = Math.Max(0, Math.Min(Max, value));

                int old = _Current;

                if (_Current == normalizedValue) return;

                _Current = normalizedValue;

                // 曖昧状態にしている場合は更新を通知しない
                if (_IsAmbiguousProgress) return;

                // _NotificationStepで設定される刻みの範囲内を超える変動があったか最大値に達した場合に限ってProgressDegreeの変動を通知する
                if (normalizedValue == Max || (old / _NotificationStep) != (normalizedValue / _NotificationStep))
                {
                    ProgressDegreeChanged?.Invoke();
                }
            }
        }

        private int _Max;
        public int Max
        {
            get => _Max;
            set
            {
                int normalizedMax = Math.Max(0, value);

                if (_Max == normalizedMax) return;

                _Max = normalizedMax;
                _Current = Math.Min(Current, normalizedMax);

                ProgressDegreeChanged?.Invoke();
            }
        }


        public ProgressLeaf(int weight, int current, int max, int notificationStep)
        {
            if (weight < 0) throw new ArgumentException();
            if (max < 0) throw new ArgumentException();
            if (current < 0) throw new ArgumentException();
            if (current > max) throw new ArgumentException();
            if (notificationStep < 1) throw new ArgumentException();

            _Weight = weight;
            _Current = current;
            _Max = max;
            _NotificationStep = notificationStep;
        }


        public void Complete()
        {
            Current = Max;
            IsAmbiguousProgress = false;
        }
    }
}
