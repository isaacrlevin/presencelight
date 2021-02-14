using MediatR;
using System.IO;

namespace PresenceLight.Core.GraphServices
{
    public class GetPhotoCommand : IRequest<Stream>
    {
    }
}
