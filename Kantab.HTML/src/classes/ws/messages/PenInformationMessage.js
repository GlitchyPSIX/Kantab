import { Point } from "pixi.js";
import { KantabMessage } from "./KantabMessage.js";

export class PenInformationMessage extends KantabMessage{
    /**
     * @type {Point}
     */
    position;

    /**
     * @type {Number}
     */
    tilt;

    /**
     * @type {Number}
     */
    pressure;

    /**
     * @type {Boolean}
     */
    extended;

    /**
     * @param {DataView} dataview 
     */
    constructor(dataview){
        super(dataview);
        this.extended = dataview.getUint8(1) == 1;
        // always li-endian, intel arch and arm use it like so
        this.position = new Point(dataview.getFloat32(2, true), dataview.getFloat32(6, true))
        
        if (this.extended){
            this.pressure = dataview.getFloat32(10, true)
            this.tilt = dataview.getFloat32(14, true)
        }
        else {
            // simple press, but a good enough press to actually actuate
            // or mouse. idk you do you
            this.pressure = dataview.getUint8(10) > 0 ? 0.25 : 0;
            this.tilt = 0;
        }
    }

    toBytes(){
        // not implemented; just consumer
        return new Uint8Array();
    }
} 