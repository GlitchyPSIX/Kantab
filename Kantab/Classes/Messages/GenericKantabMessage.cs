namespace Kantab.Classes.Messages; 

public class GenericKantabMessage:KantabMessage {
    private byte[] innerBytes;
    public GenericKantabMessage(byte[] bytes) : base(bytes) {
        innerBytes = bytes;
    }

    public override byte[] ToBytes() {
        return innerBytes;
    }
}