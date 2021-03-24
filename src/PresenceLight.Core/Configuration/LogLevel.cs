using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresenceLight.Core.Configuration
{
    public class LogLevel
    {
        public string Label { get; private set; }
        public static List<LogLevel> GetAll()
        {
            return new List<LogLevel> {
                new LogLevel{
                     Label = "None"
                },
                new LogLevel{
                     Label = "Info"
                },
                new LogLevel{
                     Label = "Error"
                }
            };
        }
    }
}
