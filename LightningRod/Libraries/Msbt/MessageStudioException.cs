namespace LightningRod.Libraries.Msbt;

public sealed class MessageStudioException : Exception
{
    public MessageStudioException(string message) : base(message)
    {
    }

    public MessageStudioException(string message, Exception inner) : base(message, inner)
    {
    }
}
