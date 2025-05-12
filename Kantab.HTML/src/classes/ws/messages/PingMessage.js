import { KantabMessage } from "./KantabMessage.js";

export class PingMessage extends KantabMessage{
    toBytes(){
        return new Uint8Array([0x01, 0x00]);
    }
} 