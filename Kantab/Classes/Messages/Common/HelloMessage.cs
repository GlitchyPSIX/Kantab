namespace Kantab.Classes.Messages.Common;

public class HelloMessage : KantabMessage
{
    public override byte[] ToBytes()
    {
        return new byte[] { 02, 00 };
    }
}