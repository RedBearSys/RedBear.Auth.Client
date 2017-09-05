using System.IO;

namespace RedBear.Auth.Client
{
    public interface IFileReader
    {
        TextReader Reader { get; }
        TextReader Open(string filename);
        void Close();
    }
}
