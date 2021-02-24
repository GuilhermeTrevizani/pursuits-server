import * as native from 'natives';

let blips = [];

export function createBlip(codigo, sprite, label, color, pos, display = 2) {
    if (!sprite) 
        return;

    let blip = native.addBlipForCoord(pos.x, pos.y, pos.z);
    native.setBlipSprite(blip, sprite);
    native.setBlipColour(blip, color);
    native.beginTextCommandSetBlipName('STRING');
    native.addTextComponentSubstringPlayerName(label);
    native.endTextCommandSetBlipName(blip);
    native.setBlipDisplay(blip, display);

    blips.push({
        codigo,
        blip
    });
}

export function removeBlip(codigo) {
    var x = blips.findIndex(x => x.codigo === codigo);
    if (x === -1)
        return;

    if (blips[x].blip != null) {
        native.removeBlip(blips[x].blip);
        blips[x].blip = null;
    }

    blips.splice(x, 1);
}