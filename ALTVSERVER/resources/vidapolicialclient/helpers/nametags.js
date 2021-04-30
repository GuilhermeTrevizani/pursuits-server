import alt from 'alt-client';
import * as native from 'natives';

let drawDistance = 20;
let showNametags = true;
let interval;

alt.onServer('nametags:Config', handleConfig);

function handleConfig(_showNametags) {
    showNametags = _showNametags;

    if (!showNametags) {
        if (interval) {
            alt.clearInterval(interval);
            interval = null;
        }
        return;
    }

    interval = alt.setInterval(drawNametags, 0);
}

async function drawNametags() {
    for (let i = 0, n = alt.Player.all.length; i < n; i++) {
        let player = alt.Player.all[i];
        if (!player.valid)
            continue;

        if (player.scriptID === alt.Player.local.scriptID) 
            continue;

        const name = player.getSyncedMeta('nametag');
        if (!name) 
            continue;

        if (!native.hasEntityClearLosToEntity(alt.Player.local.scriptID, player.scriptID, 17))
            continue;

        let dist = distance2d(player.pos, alt.Player.local.pos);
        if (dist > drawDistance)
            continue;

        const isChatting = player.getMeta('chatting');
        const pos = { ...native.getPedBoneCoords(player.scriptID, 12844, 0, 0, 0) };
        pos.z += player.vehicle ? 1 : 0.50;

        let scale = 1 - (0.8 * dist) / drawDistance;
        //let fontSize = 0.4 * scale;
        let fontSize = 0.4;

        const entity = player.vehicle ? player.vehicle.scriptID : player.scriptID;
        const vector = native.getEntityVelocity(entity);
        const frameTime = native.getFrameTime();

        // Names
        native.setDrawOrigin(
            pos.x + vector.x * frameTime,
            pos.y + vector.y * frameTime,
            pos.z + vector.z * frameTime,
            0
        );
        native.beginTextCommandDisplayText('STRING');
        native.setTextFont(4);
        native.setTextScale(fontSize, fontSize);
        native.setTextProportional(true);
        native.setTextCentre(true);
        native.setTextColour(255, 255, 255, 255);
        native.setTextOutline();
        native.addTextComponentSubstringPlayerName(isChatting ? `${name}~r~*` : `${name}`);
        native.endTextCommandDisplayText(0, 0);

        const lineHeight = native.getTextScaleHeight(fontSize, 4);
        
        drawBarBackground(100, lineHeight, scale, 0.75, 139, 0, 0, 255);
        drawBar(native.getEntityHealth(player.scriptID) - 100, lineHeight, scale, 0.75, 255, 0, 0, 255); //  - 100

        if (native.getPedArmour(player.scriptID) > 0) {
            drawBarBackground(100, lineHeight, scale, 0.25, 140, 140, 140, 255);
            drawBar(native.getPedArmour(player.scriptID), lineHeight, scale, 0.25, 255, 255, 255, 255); 
        }

        native.clearDrawOrigin();
    }
}

/**
 * @param  {alt.Vector3} vector1
 * @param  {alt.Vector3} vector2
 */
function distance2d(vector1, vector2) {
    return Math.sqrt(Math.pow(vector1.x - vector2.x, 2) + Math.pow(vector1.y - vector2.y, 2) + Math.pow(vector1.z - vector2.z, 2));
}

function drawBar(value, lineHeight, scale, position, r, g, b, a) {
    const healthWidth = value * 0.0005 * scale;
    native.drawRect(
        (healthWidth - 100 * 0.0005 * scale) / 2,
        lineHeight + position * lineHeight,
        healthWidth,
        lineHeight / 4,
        r,
        g,
        b,
        a
    );
}

function drawBarBackground(value, lineHeight, scale, position, r, g, b, a) {
    const width = value * 0.0005 * scale;
    native.drawRect(0, lineHeight + position * lineHeight, width + 0.002, lineHeight / 3 + 0.002, 0, 0, 0, 255);
    native.drawRect(0, lineHeight + position * lineHeight, width, lineHeight / 3, r, g, b, a);
}
