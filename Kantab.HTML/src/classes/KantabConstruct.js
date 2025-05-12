/**
 * @typedef KantabRange
 * @property {number} min,
 * @property {number} max
 * */

/**
 * @typedef KantabVec2
 * @property {number} x,
 * @property {number} y
 * */

/**
 * @typedef KantabTint
 * @property {string} colorFrom,
 * @property {string?} colorTo,
 * @property {KantabRange} range
 * */

/**
 * @typedef KantabQuad
 * @property {KantabVec2} topLeft,
 * @property {KantabVec2} topRight,
 * @property {KantabVec2} bottomLeft,
 * @property {KantabVec2} bottomRight,
 * */

/**
 * @typedef KantabConstructLayer
 * @property {string} name Name of the layer
 * @property {string} mainTexturePath Path to the layer's texture
 * @property {KantabVec2} offset Layer offset in pixels from construct's 0, 0
 * @property {KantabVec2} pivot Layer pivot point in pixels from layer texture's 0, 0
 * @property {KantabVec2} scale Layer scale in X/Y
 * @property {{type: "general"|"pressure", mult: number, range: KantabRange?}?} rotation Rotation mode and limits
 * @property {KantabTint?} tint Layer tint based on pressure
 * @property {KantabRange?} pressureRange Pressure range at which the layer is visible
 * @property {boolean} behind Is this layer in the backdrop? Only makes an effect in Static layers.
 */

/**
 * @typedef KantabReactive
 * @property {{offset: KantabVec2}?} lifted Position change based on pen state Lifted
 * @property {{scaleX: KantabRange, scaleY: KantabRange}?} xy Scale change based on XY coordinate from 0 to 1
 */

/**
 * @typedef KantabConstructModel
 * @property {KantabConstructLayer[]} staticLayers Array for each static layer of the Construct, rendered in front of the dynamic layers
 * @property {KantabConstructLayer[]} layers Array for each layer of the Construct
 * @property {"none" | "kantab-sim" | "kantab-strict" | "spud"} tiltStyle The tilt style of the Construct.
 * @property {{x: number, y: number}} baseOffset The base offset of the Construct relative to the origin
 * @property {KantabReactive?} reactive Position/Scale changes based on pen states
 * @property {KantabQuad?} confineQuad Four points to confine the Construct to (i.e. Small construct tablet area)
 * @property {boolean} relativeConfine Whether the Quad confinement works on the absolute coordinates rather than the relative coordinates
 */

import { Assets, Color, Container, Point, Sprite } from "pixi.js";
import { clamp, invLerp, lerp } from "./ws/helpers/mathHelpers.js";
import { rgb2hsv } from "./ws/helpers/colorHelpers.js";

export class KantabConstruct {
    /**
     * @type {Map<string, Container>}
     */
    constructLayers = new Map();

    /**
     * @type {Map<string, Container>}
     */
    staticLayers = new Map();

    /**
     * @type {Map<string, {type: "general"|"pressure", mult: number, range: KantabRange?}>}
     */
    #constructLayerRotations = new Map();

    /**
     * @type {Map<string, KantabTint>}
     */
    #constructLayerTints = new Map();

    /**
     * @type {Map<string, {min: number, max: number}>}
     */
    #constructLayerPressureVisTresholds = new Map();

    tiltStyle;

    /**
     * @type {KantabReactive}
     */
    reactive = {
        lifted: {
            offset: { x: 0, y: 0 }
        },
        xy: {
            scaleX: { min: 1, max: 1 },
            scaleY: { min: 1, max: 1 }
        }
    };

    /**
     * @type {Container}
     */
    dynLayerConstructContainer; // actually holds the Container below

    /**
     * @type {Container}
     */
    constructContainer;

    /**
     * @type {Container}
     */
    staticLayerConstructContainer;

    /**
     * @type {Container}
     */
    staticBackgroundLayerConstructContainer;

    /**
     * @type {KantabQuad}
     */
    confineQuad;

    /**
     * @type {boolean}
     */
    relativeConfine = false;

    clearConstruct() {
        if (!this.dynLayerConstructContainer) {
            this.dynLayerConstructContainer = new Container();
        }
        else {
            this.dynLayerConstructContainer.destroy({ children: true });
            this.dynLayerConstructContainer = new Container();
            this.dynLayerConstructContainer.sortableChildren = true;
        }

        if (!this.constructContainer) {
            this.constructContainer = new Container();
        }
        else {
            this.constructContainer.destroy({ children: true });
            this.constructContainer = new Container();
            this.constructContainer.sortableChildren = true;
        }

        if (!this.staticLayerConstructContainer) {
            this.staticLayerConstructContainer = new Container();
            this.staticLayerConstructContainer.zIndex = 1;
        }
        else {
            this.staticLayerConstructContainer.destroy({ children: true });
            this.staticLayerConstructContainer = new Container();
            this.staticLayerConstructContainer.sortableChildren = true;
        }

        if (!this.staticBackgroundLayerConstructContainer) {
            this.staticBackgroundLayerConstructContainer = new Container();
            this.staticBackgroundLayerConstructContainer.zIndex = 1;
        }
        else {
            this.staticBackgroundLayerConstructContainer.destroy({ children: true });
            this.staticBackgroundLayerConstructContainer = new Container();
            this.staticBackgroundLayerConstructContainer.sortableChildren = true;
        }


        this.dynLayerConstructContainer.addChild(this.constructContainer);
        this.#constructLayerRotations.clear();
        this.#constructLayerPressureVisTresholds.clear();
        this.reactive.lifted.offset = { x: 0, y: 0 };
        this.tiltStyle = "none";
        this.constructLayers.clear();

    }

    /**
     * 
     * @param {KantabConstructModel} construct 
     */
    async buildConstruct(construct, fsBase = "constructs") {
        this.clearConstruct();
        for (let i = 0; i < construct.layers?.length ?? 0; i++) {
            let layerInfo = construct.layers[i];
            let resolvedPath = new URL(layerInfo.mainTexturePath, location.href.split("?")[0]+fsBase);
            let layerTexture = await Assets.load(resolvedPath.href.replace(location.href.split("?")[0], ""));
            let layerSprite = new Sprite(layerTexture);

            layerSprite.pivot = new Point(layerInfo.pivot.x, layerInfo.pivot.y);
            layerSprite.position = new Point(layerInfo.offset.x, layerInfo.offset.y);
            layerSprite.zIndex = construct.layers.length - i;

            this.constructLayers.set(layerInfo.name, layerSprite);
            this.constructContainer.addChild(layerSprite);

            if (layerInfo.scale) {
                layerSprite.scale = (layerInfo.scale.x, layerInfo.scale.y);
            }

            if (layerInfo.rotation) {
                if (!["general", "pressure"].includes(layerInfo.rotation.type)) continue;

                this.#constructLayerRotations.set(layerInfo.name, layerInfo.rotation);
            }

            if (layerInfo.pressureRange) {
                this.#constructLayerPressureVisTresholds.set(layerInfo.name, layerInfo.pressureRange);
            }

            if (layerInfo.tint) {
                // If no target color then no dynamism is necessary, just straight up tint
                if (!layerInfo.tint.colorTo) {
                    layerSprite.tint = new Color(layerInfo.colorFrom);
                }
                else {
                    this.#constructLayerTints.set(layerInfo.name, layerInfo.tint);
                }
            }
        }

        // static layers
        for (let i = 0; i < construct.staticLayers?.length ?? 0; i++) {
            let layerInfo = construct.staticLayers[i];
            let resolvedPath = new URL(layerInfo.mainTexturePath, location.href.split("?")[0]+fsBase);
            let layerTexture = await Assets.load(resolvedPath.href.replace(location.href.split("?")[0], ""));
            let layerSprite = new Sprite(layerTexture);

            layerSprite.pivot = new Point(layerInfo.pivot.x, layerInfo.pivot.y);
            layerSprite.position = new Point(layerInfo.offset.x, layerInfo.offset.y);
            layerSprite.zIndex = construct.layers.length - i;

            this.staticLayers.set(layerInfo.name, layerSprite);
            if (layerInfo.behind){
                this.staticBackgroundLayerConstructContainer.addChild(layerSprite);
            }
            else {
                this.staticLayerConstructContainer.addChild(layerSprite);
            }
            

            if (layerInfo.scale) {
                layerSprite.scale = (layerInfo.scale.x, layerInfo.scale.y);
            }

            if (layerInfo.rotation) {
                if (!["general", "pressure"].includes(layerInfo.rotation.type)) continue;

                this.#constructLayerRotations.set(layerInfo.name, layerInfo.rotation);
            }

            if (layerInfo.pressureRange) {
                this.#constructLayerPressureVisTresholds.set(layerInfo.name, layerInfo.pressureRange);
            }

            if (layerInfo.tint) {
                // If no target color then no dynamism is necessary, just straight up tint
                if (!layerInfo.tint.colorTo) {
                    layerSprite.tint = new Color(layerInfo.colorFrom);
                }
                else {
                    this.#constructLayerTints.set(layerInfo.name, layerInfo.tint);
                }
            }
        }


        if (construct.reactive) {
            if (construct.reactive.lifted){
                this.reactive.lifted = construct.reactive.lifted;
            }
            if (construct.reactive.xy){
                this.reactive.xy = construct.reactive.xy;
            }
        }

        if (construct.confineQuad){
            this.confineQuad = construct.confineQuad;
            if (construct.relativeConfine === true){
                this.relativeConfine = construct.relativeConfine;
            }
        }


        this.constructContainer.pivot = new Point(construct.baseOffset.x, construct.baseOffset.y);
        this.tiltStyle = construct.tiltStyle;
    }

    /**
     * 
     * @param {PenInformation} penInfo 
     */
    applyPenInfo(penInfo) {
        if (!penInfo.position || isNaN(penInfo.tilt) || isNaN(penInfo.pressure)) return;

        this.#doRotation(penInfo);
        this.#doPressureTresholds(penInfo);
        this.#doTint(penInfo);
    }

    /**
     * 
     * @param {PenInformation} penInfo 
     */
    #doRotation(penInfo) {
        switch (this.tiltStyle) {
            case "none":
                {
                    this.dynLayerConstructContainer.angle = 0;
                    break;
                }
            case "kantab-sim": {
                if (penInfo.extended) {
                    this.dynLayerConstructContainer.angle = penInfo.tilt;
                }
                else {
                    this.dynLayerConstructContainer.angle = (Math.max(penInfo.position.y, 0) * -15) + (Math.max(penInfo.position.x, 0) * 8);
                }
                break;
            }
            case "kantab-strict": {
                if (penInfo.extended) {
                    this.dynLayerConstructContainer.angle = penInfo.tilt;
                }
                else {
                    this.dynLayerConstructContainer.angle = 0;
                }
                break;
            }
            case "spud": {
                this.dynLayerConstructContainer.angle = (Math.max(penInfo.position.y, 0) * -60) + (Math.max(penInfo.position.x, 0) * 40);
                break;
            }
        }

        if (penInfo.pressure > 0) {
            this.dynLayerConstructContainer.position = new Point(this.reactive.lifted.offset.x, this.reactive.lifted.offset.y);
        }
        else {
            this.dynLayerConstructContainer.position = new Point(0, 0);
        }

        // Per layer rotation
        this.#constructLayerRotations.forEach((rot, key) => {
            let container = this.constructLayers.get(key);

            switch (rot.type) {
                case "general":
                    if (!rot.range) {
                        container.angle = this.dynLayerConstructContainer.angle * rot.mult;
                    }
                    else {
                        container.angle = clamp(this.dynLayerConstructContainer.angle * rot.mult, rot.range.max, rot.range.min);
                    }
                    break;
                case "pressure":
                    if (!rot.range) {
                        container.angle = lerp(0, 360, penInfo.pressure) * rot.mult;
                    }
                    else {
                        container.angle = lerp(rot.range.min, rot.range.max, penInfo.pressure * rot.mult);
                    }
                    break;
            }

        });
    }

    /**
     * Performs pressure visibility checks per-layer
     * @param {PenInformation} penInfo 
     */
    #doPressureTresholds(penInfo) {
        this.#constructLayerPressureVisTresholds.forEach((range, key) => {
            let container = this.constructLayers.get(key);
            container.renderable = (penInfo.pressure < range.max) && (penInfo.pressure >= range.min);
        });
    }

    /**
     * Performs tinting per layer
     * @param {PenInformation} penInfo 
     */
    #doTint(penInfo) {
        this.#constructLayerTints.forEach((tint, key) => {
            if (!tint.colorTo) return;
            // TODO: optimize this by caching with the layer tint the color objects so only clamping is needed
            let colorA = new Color(tint.colorFrom);
            let colorB = new Color(tint.colorTo);
            let clampedPressure = clamp(penInfo.pressure, tint.range.max, tint.range.min);

            // gives 0-1 when pressure is within the confines of range, clamped returns min when below min
            let progWithinRange = invLerp(tint.range.min, tint.range.max, clampedPressure);

            let hsvA = rgb2hsv(colorA.red, colorA.green, colorA.blue);
            let hsvB = rgb2hsv(colorB.red, colorB.green, colorB.blue);
            let hsvL = [lerp(hsvA[0], hsvB[0], progWithinRange), lerp(hsvA[1], hsvB[1], progWithinRange), lerp(hsvA[2], hsvB[2], progWithinRange)];
            let colorLerp = new Color({h: hsvL[0], s: hsvL[1], v: hsvL[2], a: 1});

            let container = this.constructLayers.get(key);
            container.tint = colorLerp;
        });
    }
}