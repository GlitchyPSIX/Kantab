import { KantabMessage } from "./KantabMessage.js";

export class EndOfTheLineMessage extends KantabMessage{
    /**
     * @type {boolean}
     */
    errored;

    /**
     * @type {boolean}
     */
    refreshing;

    toBytes(){
        return new Uint8Array([0xEE, (errored) ? 2 : ((refreshing) ? 1 : 0)]);
    }
} 