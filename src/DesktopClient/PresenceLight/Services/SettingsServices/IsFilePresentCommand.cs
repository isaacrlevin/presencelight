using MediatR;
using System;

namespace PresenceLight.Services
{
    public class IsFilePresentCommand : IRequest<bool>
    {
    }
}
