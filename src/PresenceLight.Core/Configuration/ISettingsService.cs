using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PresenceLight.Core;

namespace PresenceLight.Core
{
    /// <summary>
    /// Represents a service for managing application settings.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Loads the application settings from a file.
        /// </summary>
        /// <returns>The loaded settings, or null if the file is not found.</returns>
        public Task<BaseConfig?> LoadSettings();

        /// <summary>
        /// Saves the application settings to a file.
        /// </summary>
        /// <param name="data">The settings to save.</param>
        /// <returns>True if the settings are successfully saved, false otherwise.</returns>
        public Task<bool> SaveSettings(BaseConfig data);

        /// <summary>
        /// Deletes the application settings file.
        /// </summary>
        /// <returns>True if the settings file is successfully deleted, false otherwise.</returns>
        public Task<bool> DeleteSettings();

        /// <summary>
        /// Checks if the application settings file is present.
        /// </summary>
        /// <returns>True if the settings file is present, false otherwise.</returns>
        public Task<bool> IsFilePresent();

        /// <summary>
        /// Gets the location of the application settings file.
        /// </summary>
        /// <returns>The file location.</returns>
        public string GetSettingsFileLocation();
    }
}
