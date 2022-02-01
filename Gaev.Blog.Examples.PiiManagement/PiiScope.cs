using System;
using System.Threading;

namespace Gaev.Blog.Examples.PiiManagement;

public sealed class PiiScope : IDisposable
{
    public static IPiiSerializer GlobalSerializer = new PiiSerializers.PlainText();
    private static readonly AsyncLocal<PiiScope> CurrentScope = new AsyncLocal<PiiScope>();
    private readonly IPiiSerializer _serializer;
    public static IPiiSerializer Serializer => CurrentScope.Value?._serializer ?? GlobalSerializer;

    public PiiScope(IPiiSerializer serializer)
    {
        _serializer = serializer;
        CurrentScope.Value = this;
    }

    public void Dispose()
    {
        CurrentScope.Value = null;
    }
}