using System;
using System.Runtime.Serialization;

[Serializable]
public class RobotsEntityNotFoundException : Exception
{
    public RobotsEntityNotFoundException()
    {
    }

    public RobotsEntityNotFoundException(Guid id)
        : base($"A robots entry could not be found with the id of '{id}'")
    {
    }

    public RobotsEntityNotFoundException(string message)
        : base(message)
    {
    }

    public RobotsEntityNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected RobotsEntityNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}