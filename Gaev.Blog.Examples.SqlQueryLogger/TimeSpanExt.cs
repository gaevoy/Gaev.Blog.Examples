using System;

namespace Gaev.Blog.Examples
{
    public static class TimeSpanExt
    {
        public static TimeSpan Milliseconds(this int val) => TimeSpan.FromMilliseconds(val);
    }
}