using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using static System.Runtime.CompilerServices.MethodImplOptions;

// ReSharper disable RedundantCatchClause

namespace Gaev.Blog.ExceptionRethrow;

public class ExceptionRethrowTests
{
    [Test]
    public void Stacktrace_should_point_to_exception_line()
    {
        try
        {
            throw new Exception();
        }
        catch (Exception)
        {
            throw;
        }
    }

    [Test]
    public void Stacktrace_should_point_to_exception_line_of_validate_method()
    {
        [MethodImpl(NoInlining)]
        void Validate()
        {
            throw new Exception();
        }

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
    public void Stacktrace_should_point_to_exception_line_of_inlined_validate_method()
    {
        [MethodImpl(AggressiveInlining)]
        void Validate()
        {
            throw new Exception();
        }

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
            throw new Exception();
        }
        catch (Exception ex)
        {
            ExceptionDispatchInfo.Capture(ex).Throw();
            throw;
        }
    }
}
