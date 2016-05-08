using System;

namespace NTestController
{
    public class Platform : IPlatform
    {
        #region Members from IPlatform

        /// <see cref="IPlatform.OS"/>
        public string OS { get; set; }

        /// <see cref="IPlatform.Version"/>
        public string Version { get; set; }

        /// <see cref="IPlatform.CPU"/>
        public string CPU { get; set; }

        #endregion Members from IPlatform
    }
}

