import { KantabMessage } from "./KantabMessage.js";

export class HelloMessage extends KantabMessage{
    toBytes(){
        return new Uint8Array([0x02, 0x00]);
    }
} 