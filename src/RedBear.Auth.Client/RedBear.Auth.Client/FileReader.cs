using System;
using System.IO;

namespace RedBear.Auth.Client
{
    public class FileReader : IDisposable, IFileReader
    {
        public TextReader Reader { get; private set; }

        public TextReader Open(string filename)
        {
            Reader = File.OpenText(filename);
            return Reader;
        }

        public void Close()
        {
            Reader?.Close();
        }

        public void Dispose()
        {
            Reader?.Dispose();
        }
    }
}
