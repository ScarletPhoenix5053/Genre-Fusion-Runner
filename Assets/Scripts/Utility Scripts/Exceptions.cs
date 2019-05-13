using UnityEngine;
using System;

public class InvalidSignException : Exception
{
    public InvalidSignException()
    {
    }
    public InvalidSignException(string message)
        : base(message)
    {
    }
    public InvalidSignException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
public class InvalidCsvException : Exception
{
    public InvalidCsvException()
    {
    }
    public InvalidCsvException(string message)
        : base(message)
    {
    }
    public InvalidCsvException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
public class NoActiveWallException : Exception
{
    public NoActiveWallException()
    {
    }
    public NoActiveWallException(string message)
        : base(message)
    {
    }
    public NoActiveWallException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
public class LineOutOfRangeException : Exception
{
    public LineOutOfRangeException()
    {
    }
    public LineOutOfRangeException(string message)
        : base(message)
    {
    }
    public LineOutOfRangeException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
public class BadBufferSizeException : Exception
{
    public BadBufferSizeException()
    {
    }
    public BadBufferSizeException(string message)
        : base(message)
    {
    }
    public BadBufferSizeException(string message, Exception inner)
        : base(message, inner)
    {
    }
}