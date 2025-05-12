import { Point, Container, Sprite } from "pixi.js";
import { KantabConstruct } from "./KantabConstruct.js";
import { clamp, lerp } from "./ws/helpers/mathHelpers.js";
import { calculateHomography } from "simple-homography";
import { multiply as mathMult } from 'mathjs';

class KantabHandFullscreen {
    /** @type {Container} */
    #baseContainer = null;

    /** @type {Container} */
    // Contains the part of the construct with the dynamic layers
    #dynContainer = null;

    // Contains the part of the construct with the static layers in the foreground
    /** @type {Container} */
    #staContainerFg = null;

    // Contains the part of the construct with the static layers in the background
    /** @type {Container} */
    #staContainerBg = null;

    /** @type {KantabConstruct} */
    #construct = null;

    /**
     * @returns {Number}
     */
    widthGetter = () => 0;

    /**
     * @returns {Number}
     */
    heightGetter = () => 0;

    scale = 1;

    fsBase = "/constructs"

    /**
    * @param {Container} pxStage 
    */
    constructor(pxStage) {
        this.#baseContainer = pxStage.addChild(new Container());
        this.#baseContainer.position.set(0, 0);
        this.#baseContainer.label = "Kantab Hand"
        this.#baseContainer.sortableChildren = true;
    }

    async initialize(construct, settings) {
        this.#construct = new KantabConstruct();
        await this.#construct.buildConstruct(construct, this.fsBase);
        this.#staContainerFg = this.#baseContainer.addChild(new Container());
        this.#staContainerFg.addChild(this.#construct.staticLayerConstructContainer);
        this.#staContainerFg.label = "Foreground Static";
        this.#staContainerFg.zIndex = 1;
        this.#dynContainer = this.#baseContainer.addChild(new Container());
        this.#dynContainer.addChild(this.#construct.dynLayerConstructContainer);
        this.#dynContainer.label = "Dynamic";
        this.#dynContainer.zIndex = 0;
        this.#staContainerBg = this.#baseContainer.addChild(new Container());
        this.#staContainerBg.label = "Background Static";
        this.#staContainerBg.addChild(this.#construct.staticBackgroundLayerConstructContainer);
        this.#staContainerBg.zIndex = -1;
        if (settings) {
            this.scale = parseFloat(settings.scale);
            this.#dynContainer.scale = this.scale;
            this.#staContainerFg.scale = this.scale;
            this.#staContainerBg.scale = this.scale;
        }
    }

    /**
     * 
     * @param {number} relX 
     * @param {number} relY 
     */
    moveTo(relX, relY) {
        let inputX = relX;
        let inputY = relY;

        if (this.#construct.confineQuad && this.#construct.relativeConfine){
            let target = this.#construct.confineQuad;
            let homography = calculateHomography(
                [0,0],
                [target.topLeft.x, target.topLeft.y],
                [1, 0],
                [target.topRight.x, target.topRight.y],
                [1, 1],
                [target.bottomRight.x, target.bottomRight.y],
                [0, 1],
                [target.bottomLeft.x, target.bottomLeft.y]
            );

            let source = [relX, relY, 1];
            /**
             * @type {import("mathjs").Matrix}
             */
            // JS does not play nice with math.js's matrix index accessor, that might be a job for TS
            // so we're using set and get explicitly
            let result = mathMult(homography, source);

            result.set([0], result.get([0])/result.get([2]));
            result.set([1], result.get([1])/result.get([2]));
            result.set([2], 1);

            relX = result.get([0]);
            relY = result.get([1]);
        }

        if (inputX < -0.15 || inputX > 1.15 || inputY < -0.15 || inputY > 1.15){
            this.#dynContainer.visible = false;
        }
        else {
            this.#dynContainer.visible = true;
        }

        let winWidth = this.widthGetter();
        let winHeight = this.heightGetter();

        let absX = relX * winWidth;
        let absY = relY * winHeight;

        // TODO: extract homography calcs into function to avoid repetition
        if (this.#construct.confineQuad && !this.#construct.relativeConfine){
            let target = this.#construct.confineQuad;
            let homography = calculateHomography(
                [0,0],
                [target.topLeft.x, target.topLeft.y],
                [winWidth, 0],
                [target.topRight.x, target.topRight.y],
                [winWidth, winHeight],
                [target.bottomRight.x, target.bottomRight.y],
                [0, winHeight],
                [target.bottomLeft.x, target.bottomLeft.y]
            );

            let source = [absX, absY, 1];
            /**
             * @type {import("mathjs").Matrix}
             */
            // JS does not play nice with math.js's matrix index accessor, that might be a job for TS
            // so we're using set and get explicitly
            let result = mathMult(homography, source);

            result.set([0], result.get([0])/result.get([2]));
            result.set([1], result.get([1])/result.get([2]));
            result.set([2], 1);

            absX = result.get([0]) * this.scale;
            absY = result.get([1]) * this.scale;
        }

        this.#dynContainer.position.set(absX, absY);

        // scale based on XY position
        if (this.#construct.reactive?.xy){
            let scaleX = lerp(this.#construct.reactive.xy.scaleX.min, this.#construct.reactive.xy.scaleX.max,  clamp(inputX, 1, 0) );
            let scaleY = lerp(this.#construct.reactive.xy.scaleY.min, this.#construct.reactive.xy.scaleY.max, clamp(inputY, 1, 0));
            this.setReactiveScale(scaleX * scaleY);
        }
    }

    setScale(scale) {
        this.scale = scale;
        this.#dynContainer.scale = scale;
        this.#staContainerFg.scale = scale;
        this.#staContainerBg.scale = scale;
    }

    setReactiveScale(scale) {
        this.#dynContainer.scale = this.scale * scale;
    }

    /** @param {import("../jsdoc.types.js").PenInformation} penInfo */
    setConstructState(penInfo) {
        this.#construct.applyPenInfo(penInfo);
    }
}

export { KantabHandFullscreen }