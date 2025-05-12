import { KantabMessage } from "./KantabMessage.js";

export class GoodbyeMessage extends KantabMessage{
    /**
     * @type {boolean}
     */
    misbehaving;

    /**
     * @param {DataView} dataview 
     */
    constructor(dataview){
        super(dataview);
        this.misbehaving = dataview.getUint8(1) == 1;
    }

    toBytes(){
        // not implemented; just consumer
        return new Uint8Array();
    }
} 