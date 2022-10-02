using NotificationUtils;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgressCheck
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            MessagingConfiguration.SetMessageDataValidationByAssemblyDebuggableAttribute(Assembly.GetEntryAssembly());

            InitializeComponent();

            Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 1000;

            var cancellationTokenSource = new CancellationTokenSource();

            cancelButton.Click += (_, __) => cancellationTokenSource.Cancel();

            var progressReporter = new ProgressReporter();
            var processingMessanger = new MessagingPlatform(new DelegateMessageListener<ProgressEventSample>(context =>
            {
                label1?.Invoke(new Action(() =>
                {
                    switch (context.Message)
                    {
                        case ProgressEventSample.BeginChildProcess:
                            label1.Text = context.ToString("{ProcessName}の処理中です。");
                            break;
                        case ProgressEventSample.EndChildProcess:
                            label1.Text = context.ToString("{ProcessName}の処理が完了しました。");
                            break;
                        case ProgressEventSample.CompletedProcess:
                            label1.Text = "全ての処理が完了しました。";
                            break;
                    }
                }));
            }));

            var progressObserver = Observable.FromEvent(h => progressReporter.ProgressDegreeChanged += h, h => progressReporter.ProgressDegreeChanged -= h)
                                    .Sample(TimeSpan.FromMilliseconds(500))
                                    .Select(v => progressReporter.ProgressDegree)
                                    .DistinctUntilChanged()
                                    .ObserveOn(this)
                                    .Subscribe(progressDegree =>
                                    {
                                        if (progressDegree.HasValue)
                                        {
                                            if (progressBar1.Style != ProgressBarStyle.Continuous)
                                            {
                                                progressBar1.Style = ProgressBarStyle.Continuous;
                                                progressBar1.Value = progressBar1.Maximum; // 進捗バーの位置が即時反映させるために、先に大きな値を設定しておく
                                            }
                                            progressBar1.Value = (int)(progressDegree * 1000);
                                        }
                                        else
                                        {
                                            if (progressBar1.Style != ProgressBarStyle.Marquee)
                                            {
                                                progressBar1.Style = ProgressBarStyle.Marquee;
                                            }
                                        }
                                    });


            using (progressObserver)
            {
                try
                {
                    var rootTokens = new NotificationTokens(progressReporter.Token, processingMessanger.Token, cancellationTokenSource.Token);

                    var tokens1 = rootTokens.CreateProgressBranchedTokens(1);
                    var tokens2 = rootTokens.CreateProgressBranchedTokens(1);

                    await Task.Run(() => DummyProc(tokens1), cancellationTokenSource.Token);
                    await Task.Run(() => DummyProc(tokens2), cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    label1.Text = "処理がキャンセルされました。";
                    progressBar1.Value = progressBar1.Maximum;
                }
            }
        }

        private static async Task DummyProc(NotificationTokens tokens)
        {
            ProgressToken progressToken = tokens.Progress;
            MessagingToken eventMessanger = tokens.Messaging;
            CancellationToken cancellationToken = tokens.Cancellation;

            var leaf1 = progressToken.CreateLeaf(weight: 1, max: 200, notificationStep: 10);
            var branch1 = progressToken.CreateBranchedToken(weight: 1);
            var leaf2 = branch1.CreateLeaf(weight: 1, max: 20, notificationStep: 1);
            var leaf3 = branch1.CreateLeaf(weight: 1, max: 20, notificationStep: 1);

            var messageBodySource = new MessageData.Builder();

            messageBodySource["ProcessName"] = "Leaf1";

            eventMessanger.Send(ProgressEventSample.BeginChildProcess, messageBodySource);
            for (int i = 0; i < leaf1.Max; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(30).ConfigureAwait(false);
                leaf1.Current++;
            }
            leaf1.Complete();
            eventMessanger.Send(ProgressEventSample.EndChildProcess, messageBodySource);

            await Task.Delay(1000).ConfigureAwait(false);

            messageBodySource["ProcessName"] = "Leaf2";

            eventMessanger.Send(ProgressEventSample.BeginChildProcess, messageBodySource);
            for (int i = 0; i < leaf2.Max; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(300).ConfigureAwait(false);
                leaf2.Current++;

                if (i == leaf2.Max / 2)
                {
                    leaf2.IsAmbiguousProgress = true;
                }
            }
            leaf2.Complete();
            eventMessanger.Send(ProgressEventSample.EndChildProcess, messageBodySource);


            await Task.Delay(1000).ConfigureAwait(false);

            messageBodySource["ProcessName"] = "Leaf3";

            eventMessanger.Send(ProgressEventSample.BeginChildProcess, messageBodySource);
            for (int i = 0; i < leaf3.Max; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(300).ConfigureAwait(false);
                leaf3.Current++;
            }
            leaf3.Complete();

            eventMessanger.Send(ProgressEventSample.EndChildProcess, messageBodySource);

            eventMessanger.Send(ProgressEventSample.CompletedProcess);
        }
    }
}
