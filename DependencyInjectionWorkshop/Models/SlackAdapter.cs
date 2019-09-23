using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void SendLogFailedMessage(string message);
    }

    public class SlackAdapter : INotification
    {
        public void SendLogFailedMessage(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}