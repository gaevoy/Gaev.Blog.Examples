using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Gaev.Blog.Examples.PiiManagement.EfCore;

public class PiiStringConverter : ValueConverter<PiiString, string>
{
    private static readonly Expression<Func<PiiString, string>> Serialize
        = v => PiiScope.Serializer.ToString(v);

    private static readonly Expression<Func<string, PiiString>> Deserialize
        = v => PiiScope.Serializer.FromString(v);

    public PiiStringConverter() : base(Serialize, Deserialize)
    {
    }
}