using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NotificationUtils
{
    public class ProgressNode : IProgresSource
    {
        public event Action WeightChanged;

        public event Action ProgressDegreeChanged;

        private object lockObject = new object();

        private bool needsUpdateProgressDegree;

        private List<IProgresSource> progressSources;

        public ProgressToken Token => new ProgressToken(this);

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

        private double _MinProgressDegree;
        public double MinProgressDegree
        {
            get
            {
                RefreshProgressDegree(out var minProgressDegree, out _);
                Debug.Assert(minProgressDegree >= 0);
                return minProgressDegree;
            }
        }

        private double? _ProgressDegree;
        public double ?ProgressDegree
        {
            get
            {
                RefreshProgressDegree(out _, out var progressDegree);
                Debug.Assert(!progressDegree.HasValue || progressDegree.Value >= 0);
                return progressDegree;
            }
        }

        private void RefreshProgressDegree(out double minProgressDegree, out double? progressDegree)
        {
            minProgressDegree = _MinProgressDegree;
            progressDegree = _ProgressDegree;

            if (!needsUpdateProgressDegree)
            {
                return;
            }

            lock (lockObject)
            {
                if (!needsUpdateProgressDegree)
                {
                    minProgressDegree = _MinProgressDegree;
                    progressDegree = _ProgressDegree;
                    return;
                }

                if (progressSources is null || progressSources.Count == 0)
                {
                    minProgressDegree = 0;
                    progressDegree = 0;
                }
                else
                {
                    double totalWeight = progressSources?.Sum(v => v.Weight) ?? 0;

                    if (totalWeight == 0)
                    {
                        Debug.Assert(progressSources?.All(v => v.Weight == 0) ?? true);
                        totalWeight = 1;
                    }

                    (minProgressDegree, progressDegree) = progressSources?.Aggregate((minProgressDegree: 0.0, progressDegree: (double?)0.0), (accumlate, soruce) =>
                    {
                        var weightRate = soruce.Weight / totalWeight;
                        var accumMinProgress = accumlate.minProgressDegree + weightRate * soruce.MinProgressDegree;

                        var sourceProgressDegree = soruce.ProgressDegree;

                        if (accumlate.progressDegree.HasValue && sourceProgressDegree.HasValue)
                        {
                            return (accumMinProgress, accumlate.progressDegree + weightRate * sourceProgressDegree);
                        }
                        else
                        {
                            return (accumMinProgress, null);
                        }
                    }) ?? (0.0, 0.0);

                    Debug.Assert(minProgressDegree >= 0);
                    Debug.Assert(!progressDegree.HasValue || progressDegree.Value >= 0);

                    _MinProgressDegree = minProgressDegree;
                    _ProgressDegree = progressDegree;
                }

                needsUpdateProgressDegree = false;
            }
        }

        internal ProgressNode CreateBranchedTree(int weight)
        {
            var progressNode = new ProgressNode(weight);

            AddProgressSource(progressNode);

            return progressNode;
        }

        internal ProgressLeaf CreateLeaf(int weight = 1, int max = 1, int notificationStep = 1)
        {
            var progressReceptor = new ProgressLeaf(weight, 0, max, notificationStep);

            AddProgressSource(progressReceptor);

            return progressReceptor;
        }

        private void AddProgressSource(IProgresSource source)
        {
            source.ProgressDegreeChanged += () =>
            {
                needsUpdateProgressDegree = true;
                ProgressDegreeChanged?.Invoke();
            };

            lock (lockObject)
            {
                // ProgressSourcesはProgressDegreeの計算と排他的に変更する
                progressSources = progressSources ?? new List<IProgresSource>();
                progressSources.Add(source);
            }

            // 進捗の計算源を変更したので既存の進捗値は無効化
            needsUpdateProgressDegree = true;
            ProgressDegreeChanged?.Invoke();
        }

        internal ProgressNode(int weight)
        {
            if (weight < 0) throw new ArgumentException();

            _Weight = weight;
            _MinProgressDegree = 0;
            _ProgressDegree = 0;
            needsUpdateProgressDegree = false;
        }
    }
}
