import { Application } from "pixi.js";
import { KantabHandFullscreen } from "../src/classes/kantabHandFullscreen.js";
import { KantabClient } from "../src/classes/client/kantabClient.js";
import { expDecay } from "../src/classes/ws/helpers/mathHelpers.js";

let deltaTime = 0;
let prevTimestamp = performance.now();

/**
 * 
 * @param {string} constructName 
 * @returns 
 */
const initView = async (constructName) => {
    const pixiApp = new Application();

    await pixiApp.init({ resizeTo: document.querySelector("body"), backgroundAlpha: 0 });
    document.body.appendChild(pixiApp.canvas);

    globalThis.__PIXI_APP__ = pixiApp;

    let bigHandRenderer = new KantabHandFullscreen(pixiApp.stage);


    let kantabSettings;
    let kantabConstructMeta;


    let devMode = false;

    // Developer mode Construct and info to be used when developing independent of the Kantab Server (vite run)
    if (devMode) {
        kantabSettings = JSON.parse('{"region":{"TopLeft":{},"BottomRight":{},"Size":{},"Empty":false},"penProvider":0,"fetchRate":13,"port":7329,"autostart":false,"construct":"naz","scale":0.1}');
        kantabConstructMeta = JSON.parse('{"Id":"naz","name":"Sample Dev Construct","author":"GlitchyPSI","description":"A messy test construct.","fsbase":"/constructs/naz/","constructs":{"large":"large.json","small":null}}');
    }
    else {
        kantabSettings = await (await fetch("/settings")).json();
        kantabConstructMeta = await (await fetch("/settings/construct")).json();
    }

    if (!kantabConstructMeta.fsbase) { console.error("No Kantab construct was selected!"); return; }
    let constructInfo = await (await fetch(`${kantabConstructMeta.fsbase}${kantabConstructMeta.constructs[constructName]}`)).json();

    bigHandRenderer.fsBase = kantabConstructMeta.fsbase;
    await bigHandRenderer.initialize(constructInfo, { scale: new URLSearchParams(window.location.search).get("s") ?? kantabSettings.scale ?? 1 });

    bigHandRenderer.heightGetter = () => pixiApp.screen.height;
    bigHandRenderer.widthGetter = () => pixiApp.screen.width;
    console.log(`Loading Construct: ${kantabConstructMeta.name}`);
    let kantabClient = new KantabClient("ws://localhost:" + kantabSettings.port);

    const updateHandPosition = (timestamp) => {

        deltaTime = (timestamp - prevTimestamp) / 1000;
        prevTimestamp = timestamp;

        // do lerp smoothing

        // thank you Freya Holmér
        kantabClient.cachedPenInfo.tilt = expDecay(kantabClient.cachedPenInfo.tilt, kantabClient.cachedPenInfo.tiltCurrent, 20, deltaTime);

        bigHandRenderer.moveTo(kantabClient.cachedPenInfo.position?.x ?? 0, kantabClient.cachedPenInfo.position?.y ?? 0);
        bigHandRenderer.setConstructState(kantabClient.cachedPenInfo);
        requestAnimationFrame(updateHandPosition);
    }

    requestAnimationFrame(updateHandPosition);
};

export { initView };