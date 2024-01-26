using System;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

namespace Gaev.Blog.ExceptionRethrow;

public class ExceptionRethrowTests
{
    [Test]
    public void Stacktrace_should_point_to_exception_line()
    {
        try
        {
            throw new Exception(); // The stacktrace should point here
        }
        catch (Exception)
        {
            throw;
        }
    }

    [Test]
    public void Stacktrace_should_point_to_exception_line_of_validate_method()
    {
        try
        {
            Validate();
        }
        catch (Exception)
        {
            throw;
        }
    }

    [Test]
    public void Stacktrace_should_point_to_exception_line_with_net_framework_fix()
    {
        try
        {
            throw new Exception(); // The stacktrace should point here
        }
        catch (Exception ex)
        {
            ExceptionDispatchInfo.Capture(ex).Throw();
            throw;
        }
    }

    private static void Validate()
    {
        throw new Exception(); // The stacktrace should point here
    }
}
