namespace Kantab.Classes.Messages.Client;

public class EndOfLineMessage : KantabMessage
{
    public EndOfLineReason Reason { get; private set; }

    public EndOfLineMessage(EndOfLineReason reason) {
        Reason = reason;
    }

    public override byte[] ToBytes()
    {
        return new byte[] { 0xFF, (byte)Reason };
    }
}

public enum EndOfLineReason {
    NORMAL,
    REFRESH,
    ERROR
}