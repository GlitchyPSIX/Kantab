import { ConsiderRefreshMessage } from "../messages/ConsiderRefreshMessage.js";
import { GoodbyeMessage } from "../messages/GoodbyeMessage.js";
import { HelloMessage } from "../messages/HelloMessage.js";
import { PenInformationMessage } from "../messages/PenInformationMessage.js";
import { PingMessage } from "../messages/PingMessage.js";
import { WhoopsMessage } from "../messages/WhoopsMessage.js";

/**
 * @returns {KantabMessage}
 * @param {DataView} byteArr 
 */
export function dataViewToKantabMessage(byteArr) {
    switch(byteArr.getUint8(0)){
        case 0x01:{
            return new PingMessage();
        }
        case 0x02:{
            return new HelloMessage();
        }
        case 0x04:{
            return new PenInformationMessage(byteArr);
        }
        case 0x04:{
            return new PenInformationMessage(byteArr);
        }
        case 0xFD:{
            return new ConsiderRefreshMessage();
        }
        case 0xFE:{
            return new WhoopsMessage();
        }
        case 0xFF:{
            return new GoodbyeMessage(byteArr);
        }
    }
}