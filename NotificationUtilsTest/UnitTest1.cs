using NotificationUtils;
using Xunit;

namespace NotificationUtilsTest
{
    public class UnitTest1
    {
        enum MessagesId
        {
            [MessageDataDefinition("Value")]
            [MessageDataDefinition("Value2", Visible =false)]
            MessageA,
        }

        [Fact]
        public void Test1()
        {
            int recvCount = 0;
            MessageData<MessagesId> recvContext = null;

            var listener = new DelegateMessageListener<MessagesId>(recv =>
            {
                recvCount++;
                recvContext = recv;
            });

            var messanger = new MessagingPlatform(listener);

            var context = new MessageData.Builder()
            {
                ["Value"] = "aaa",
                ["Value2"] = "aaa",
            };

            messanger.Token.Send(MessagesId.MessageA, context);

            Assert.Equal(1, recvCount);
            Assert.Equal(MessagesId.MessageA, recvContext.Message);
            Assert.Equal("MessageA{Value:aaa}", recvContext.ToString());
            Assert.Equal("Recved Value2=aaa", recvContext.ToString("Recved Value2={Value2}"));

            var progress = new ProgressReporter();
            SomeMethod(progress.Token);
        }

        void SomeMethod(ProgressToken progress)
        {
            var someMethod1 = progress.CreateBranchedToken(1);
            var someMethod2 = progress.CreateBranchedToken(1);
            var someProcess = progress.CreateLeaf();

            SomeMethod2(someMethod1);
            SomeMethod2(someMethod2);
            someProcess.Complete();
        }

        void SomeMethod2(ProgressToken progress)
        {
            var someProcess = progress.CreateLeaf();
            someProcess.Complete();
        }
    }
}
