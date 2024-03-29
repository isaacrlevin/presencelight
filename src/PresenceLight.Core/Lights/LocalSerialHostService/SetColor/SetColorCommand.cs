﻿using MediatR;
using System;

namespace PresenceLight.Core.LocalSerialHostServices
{
    public class SetColorCommand : IRequest<string>
    {
        public string Availability { get; set; }
        public string Activity { get; set; }
    }
}
