using Kantab.Enums;

namespace Kantab.Classes.Messages.Common; 

public class CapabilitiesMessage : KantabMessage {
    public ClientFeatures Features { get; }

    public CapabilitiesMessage(ClientFeatures features) {
        Features = features;
    }

    public CapabilitiesMessage(byte bytes) : base(bytes) {
        Features = (ClientFeatures)bytes;
    }

    public override byte[] ToBytes() {
        return new byte[] {0x03, (byte) Features};
    }
}