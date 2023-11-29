namespace TramlineFive.Common.Messages
{
    public class SubscribeMessage
    {
        public string lineName;
        public string stopCode;

        public SubscribeMessage(string lineName, string stopCode)
        {
            this.lineName = lineName;
            this.stopCode = stopCode;
        }
    }
}