﻿using MediatR;
using Q42.HueApi;
using System.Collections.Generic;

namespace PresenceLight.Core.RemoteHueServices
{
    public class GetLightsCommand : IRequest<IEnumerable<Light>>
    {
    }
}
