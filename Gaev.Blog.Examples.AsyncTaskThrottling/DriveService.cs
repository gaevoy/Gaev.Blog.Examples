using System.IO;
using System.Threading.Tasks;

public interface DriveService
{
    Task UploadFile(string name, Stream content);
}