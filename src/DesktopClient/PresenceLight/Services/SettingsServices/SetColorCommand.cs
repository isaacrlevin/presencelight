using MediatR;
using System.Threading.Tasks;

namespace PresenceLight.Services 
{
    public class SetColorCommand : IRequest<Unit>
    {
        public string Color { get; internal set; }
        public string Activity { get; internal set; }
    }
}
