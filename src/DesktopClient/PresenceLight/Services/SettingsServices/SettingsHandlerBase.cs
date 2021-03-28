
using Microsoft.Extensions.Options;

using PresenceLight.Core;

using System;
using System.Reflection;

namespace PresenceLight.Services
{
    internal abstract class SettingsHandlerBase
    {
     
        protected internal static BaseConfig Config
        {
            get;set;
        }
        protected internal static BaseConfig Options
        {
            get; set;
        }

        static object lockObj = new();
        public SettingsHandlerBase( )
        {
          
        }
        protected internal static void SyncOptions()
        {
            PropertyInfo[] properties = typeof(BaseConfig).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object? value = property?.GetValue(Config);

                if (property?.PropertyType == typeof(string) && value != null && string.IsNullOrEmpty(value.ToString()))
                {
                    if (property.Name != "LogInfo" && property.Name != "LogError")
                    {
                        property.SetValue(Options, $"{value}".Trim());
                    }
                }
                else
                {
                    if (property?.Name != "LogInfo" && property?.Name != "LogError")
                    {
                        property?.SetValue(Options, value);
                    }
                }
            }
        }
    }
}
