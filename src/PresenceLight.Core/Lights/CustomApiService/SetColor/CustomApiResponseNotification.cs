using MediatR;

namespace PresenceLight.Core.CustomApiServices
{
    public class CustomApiResponseNotification : INotification
    {
        public CustomApiResponse Respone { get; }

        public CustomApiResponseNotification(CustomApiResponse response)
        {
            Respone = response;
        }
    }
}
