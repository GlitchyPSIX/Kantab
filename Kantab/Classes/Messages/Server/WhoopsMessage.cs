namespace Kantab.Classes.Messages.Server; 

public class WhoopsMessage : KantabMessage {
    public override byte[] ToBytes()
    {
        return new byte[] { 0xFE, 00 };
    }
}