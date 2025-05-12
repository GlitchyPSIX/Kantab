import { KantabMessage } from "./KantabMessage.js";

export class ConsiderRefreshMessage extends KantabMessage{
    toBytes(){
        // not implemented; just consumer
        return new Uint8Array();
    }
} 