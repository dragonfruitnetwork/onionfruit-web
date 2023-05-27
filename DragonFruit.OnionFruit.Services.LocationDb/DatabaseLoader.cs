// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;
using DragonFruit.OnionFruit.Services.LocationDb.Exceptions;
using DragonFruit.OnionFruit.Services.LocationDb.V1;

namespace DragonFruit.OnionFruit.Services.LocationDb
{
    public static class DatabaseLoader
    {
        private const string MagicName = "LOCDBXX";

        public static unsafe ILocationDatabase LoadFromFile(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 0x1000, FileOptions.RandomAccess);
            var file = MemoryMappedFile.CreateFromFile(fileStream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);

            try
            {
                GlobalDatabaseHeader magicValue;

                using (var headerView = file.CreateViewAccessor(0, sizeof(GlobalDatabaseHeader), MemoryMappedFileAccess.Read))
                {
                    headerView.Read(0, out magicValue);
                }

                if (Encoding.ASCII.GetString(magicValue.magic, GlobalDatabaseHeader.MagicByteLength) != MagicName)
                {
                    throw new FileFormatException("The file does not use the supported format");
                }

                switch (magicValue.version)
                {
                    case 1:
                        return new DatabaseV1(file);

                    default:
                        throw new FileVersionException(magicValue.version);
                }
            }
            catch
            {
                // dispose before throwing
                file.Dispose();

                throw;
            }
        }
    }
}
