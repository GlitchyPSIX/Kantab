import { dataViewToKantabMessage } from "../ws/helpers/kantabMessageHelpers.js";
import { HelloMessage } from "../ws/messages/HelloMessage.js";
import { PenInformationMessage } from "../ws/messages/PenInformationMessage.js";
import { Point } from "pixi.js";
import { PingMessage } from "../ws/messages/PingMessage.js";
import { KantabMessage } from "../ws/messages/KantabMessage.js";
import { EndOfTheLineMessage } from "../ws/messages/EndOfTheLine.js";
import { GoodbyeMessage } from "../ws/messages/GoodbyeMessage.js";
import { ConsiderRefreshMessage } from "../ws/messages/ConsiderRefreshMessage.js";
import { WhoopsMessage } from "../ws/messages/WhoopsMessage.js";
export class KantabClient {

    /**
     * @type {WebSocket}
     */
    #websocket;

    /**
     * @function
     * @param {PenInformationMessage} penInfo
     */
    positionChangeReceived = (penInfo) => {};

    cachedPenInfo =  {
        /**
         * @type {Point}
         */
        position: new Point(),
    
        /**
         * @type {number}
         */
        pressure: 0,

        /**
         * @type {number}
         */
        tiltCurrent: 0,

        /**
         * @type {number}
         */
        tilt: 0,

        /**
         * @type {boolean}
         */
        extended: false
    }

    constructor(websocketUrl = "ws://" + location.host) {
        this.#websocket = new WebSocket(websocketUrl, 'kantab-v1');
        this.#websocket.binaryType = "arraybuffer";
        this.#websocket.addEventListener("message", (msgEv) => this.processKantabMessage(new DataView(msgEv.data)));
    }

    /**
     * 
     * @param {KantabMessage} msg 
     */
    sendKantabMessage(msg){
        this.#websocket.send(msg.toBytes());
    }
    
    /**
     * 
     * @param {boolean} sendMessage 
     * @param {boolean} errored 
     * @param {boolean} refreshing 
     */
    closeServer(sendMessage, errored = false, refreshing = false){
        if (sendMessage){
            let eotlMsg = new EndOfTheLineMessage()
            eotlMsg.errored = errored;
            eotlMsg.refreshing = refreshing;
            this.sendKantabMessage(eotlMsg);

            // let the server send a goodbye instead of yeeting
            if (!(errored||refreshing)) return;
        }

        this.#websocket.close();
    }

    /**
     * @param {DataView} buf 
     */
    processKantabMessage(buf) {
        let recvMessage = dataViewToKantabMessage(buf);

        switch (recvMessage.constructor) {
            case HelloMessage: {
                this.#websocket.send(new HelloMessage().toBytes());
                break;
            }
            case PingMessage: {
                this.#websocket.send(new PingMessage().toBytes()); // naming is funny because it's technically pong but it's the same
                break;
            }
            case PenInformationMessage: {
                this.cachedPenInfo.position = recvMessage.position;
                this.cachedPenInfo.pressure = Math.min(recvMessage.pressure, 1);
                if (isNaN(this.cachedPenInfo.tilt)){
                    this.cachedPenInfo.tilt = 0;
                }

                // this tilt is temporary; in a further call down the chain it will be lerp smoothed
                this.cachedPenInfo.tiltCurrent = recvMessage.tilt;

                this.cachedPenInfo.extended = recvMessage.extended;
                this.positionChangeReceived(recvMessage);
                break;
            }
            case ConsiderRefreshMessage: {
                this.closeServer(true, false, true);
                location.reload();
                break;
            }
            case WhoopsMessage: {
                console.error("Something's wrong! Server rejected a recent message.");
                break;
            }
            case GoodbyeMessage: {
                this.closeServer(false);
                if (recvMessage.misbehaving){
                    alert("Kantab Server disconnected this client because it was misbehaving.");
                }
                break;
            }
        }
    }
}