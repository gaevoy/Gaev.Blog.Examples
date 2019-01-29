using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable CheckNamespace
#pragma warning disable 649

public class Uploader
{
    private readonly DriveService _googleApi;

    public async Task UploadFolder(string path)
    {
        await Task.WhenAll(new DirectoryInfo(path)
            .EnumerateFiles()
            .Select(file => UploadFile(file.FullName))
        );
    }

    public async Task UploadFile(string path)
    {
        using (var content = File.OpenRead(path))
            await _googleApi.UploadFile(path, content);
    }
}

public class ThrottledSomething
{
    private readonly SemaphoreSlim _throttler = new SemaphoreSlim( /*degreeOfParallelism:*/ 2);

    public async Task Throttle()
    {
        await _throttler.WaitAsync();
        try
        {
            // calling a method to throttle
        }
        finally
        {
            _throttler.Release();
        }
    }
}

public class ThrottledUploader
{
    private readonly DriveService _googleApi;
    private readonly SemaphoreSlim _throttler = new SemaphoreSlim( /*degreeOfParallelism:*/ 2);

    public async Task UploadFolder(string path)
    {
        await Task.WhenAll(new DirectoryInfo(path)
            .EnumerateFiles()
            .Select(file => UploadFile(file.FullName))
        );
    }

    public async Task UploadFile(string path)
    {
        await _throttler.WaitAsync();
        try
        {
            using (var content = File.OpenRead(path))
                await _googleApi.UploadFile(path, content);
        }
        finally
        {
            _throttler.Release();
        }
    }
}

public class ThrottledUploaderRefactored
{
    private readonly DriveService _googleApi;
    private readonly SemaphoreSlim _throttler = new SemaphoreSlim( /*degreeOfParallelism:*/ 2);

    public async Task UploadFolder(string path)
    {
        await Task.WhenAll(new DirectoryInfo(path)
            .EnumerateFiles()
            .Select(file => UploadFile(file.FullName))
        );
    }

    public async Task UploadFile(string path)
    {
        using (_throttler.Throttle())
        using (var content = File.OpenRead(path))
            await _googleApi.UploadFile(path, content);
    }
}