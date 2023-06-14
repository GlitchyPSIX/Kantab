namespace Kantab.Classes.Messages.Server; 

public class GoodbyeMessage : KantabMessage {
    public bool Misbehaved { get; }

    public GoodbyeMessage(bool misbehaved) {
        Misbehaved = misbehaved;
    }

    public override byte[] ToBytes()
    {
        return new byte[] { 0xFF, (byte)(Misbehaved ? 1 : 0) };
    }
}