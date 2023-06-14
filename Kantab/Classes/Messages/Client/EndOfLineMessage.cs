namespace Kantab.Classes.Messages.Client;

public class EndOfLineMessage : KantabMessage
{
    public bool Ungraceful { get; }

    public EndOfLineMessage(bool ungraceful) {
        Ungraceful = ungraceful;
    }

    public override byte[] ToBytes()
    {
        return new byte[] { 0xFF, (byte)(Ungraceful ? 1 : 0) };
    }
}