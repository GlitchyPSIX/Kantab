namespace Kantab.Classes.Messages.Server; 

public class ConsiderRefreshMessage : KantabMessage {

    public override byte[] ToBytes()
    {
        return new byte[] { 0xFD, 00 };
    }
}