// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.OnionFruit.Services.LocationDb.Exceptions
{
    public class FileVersionException : Exception
    {
        public FileVersionException(int version)
            : base($"The provided file is not currently supported (version {version})")
        {
        }
    }
}
