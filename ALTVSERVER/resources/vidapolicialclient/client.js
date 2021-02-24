import * as alt from 'alt';
import * as native from 'natives';
import { showCursor } from '/helpers/cursor.js';
import { activateChat, toggleInput } from '/chat/client.mjs';
import { drawText2d } from '/helpers/text.js';
import * as nametags from '/helpers/nametags.js';
import * as systemsInteriors from '/helpers/interiors.js';
import { createBlip, removeBlip } from '/helpers/blip.js';
import * as NativeUI from '/nativeui/nativeui.js';

let view, playersView, intervalSpec = null;
let player = alt.Player.local;
let playersViewLoading, isMetric;
let areaName, zoneName, tempoRestante = '';

native.pauseClock(true);
native.displayHud(false);
native.displayRadar(false);
native.disablePlayerVehicleRewards(player.scriptID);
native.setPlayerHealthRechargeMultiplier(player.scriptID, 0.0);
alt.emit('load:Interiors');
player.setMeta('chatting', false);

native.setFocusPosAndVel(-436.0717, 1039.26, 372.1287, 0, 0, 0);
let cam = native.createCamWithParams('DEFAULT_SCRIPTED_CAMERA', -436.0717, 1039.26, 372.1287, 0, 0, 0, 60, true, 0);
native.pointCamAtCoord(cam, 3.063985, 0.0, -170.8151);
native.setCamActive(cam, true);
native.renderScriptCams(true, false, 0, true, false);

alt.setInterval(() => {
    if (!player.hasSyncedMeta('nametag'))
        return;

    tempoRestante = '';
    if (player.getSyncedMeta('tempo') != ''){
        let inicio = new Date(player.getSyncedMeta('tempo'));
        let fim = new Date(inicio.getTime() + 7*60000);
        var diffInSeconds = Math.abs(fim - new Date()) / 1000;
        var minutes = ('0' + Math.floor(diffInSeconds / 60 % 60).toString()).slice(-2);
        var seconds = ('0' + Math.floor(diffInSeconds % 60).toString()).slice(-2);
        tempoRestante = `Tempo Restante: ~y~${minutes}:${seconds}`;
    }

    let obj = native.getStreetNameAtCoord(player.pos.x, player.pos.y, player.pos.z, 0, 0);
    areaName = native.getStreetNameFromHashKey(obj[1]);

    zoneName = native.getStreetNameFromHashKey(obj[2]);
    if (zoneName != '')
        zoneName += ', ';

    zoneName += native.getLabelText(native.getNameOfZone(player.pos.x, player.pos.y, player.pos.z));
    isMetric = native.getProfileSetting(227) == 1;

    alt.emitServer('AtualizarInformacoes', areaName, zoneName);
}, 800);

alt.everyTick(() => {
    drawText2d('GTA V Pursuits ~y~by TR3V1Z4', 0.8, 0.92, 0.5, 4, 255, 255, 255, 180, true, true, 2);

    if (!player.hasSyncedMeta('nametag'))
        return;

    native.hideHudComponentThisFrame(6);
    native.hideHudComponentThisFrame(7);
    native.hideHudComponentThisFrame(8);
    native.hideHudComponentThisFrame(9);
    
    native.setPedDiesInWater(player.scriptID, true);
    native.setPedMaxTimeInWater(player.scriptID, 10);

    native.setPedConfigFlag(player.scriptID, 429, true); // Do not start engine automatically 
    native.setPedConfigFlag(player.scriptID, 241, true); // PED_FLAG_DISABLE_STOPPING_VEH_ENGINE

    native.restorePlayerStamina(player.scriptID, 100);
    native.setPedSuffersCriticalHits(player.scriptID, false);

    native.disableControlAction(0, 140, true); // Disable weapon knockout
    if (native.isPlayerFreeAiming(player)) {
        native.disableControlAction(0, 141, true);
        native.disableControlAction(0, 142, true);
    }

    if(player.getSyncedMeta('congelar')) 
        native.disableControlAction(0, 75, true);

    let currentWeapon = native.getCurrentPedWeapon(player.scriptID, false);
    if ((!player.getSyncedMeta('podeatirar') && currentWeapon[1] != 911657153)
        || (player.getSyncedMeta('podeatirar') && currentWeapon[1] == 911657153))
        {
            native.disablePlayerFiring(player.scriptID, true);
            native.disableControlAction(0, 92, true);
        }

    if(native.isPedShooting(player.scriptID) && !player.getSyncedMeta('atirou')) 
        alt.emitServer('Atirou');

    if (areaName != '' && zoneName != '')
    {
        drawText2d(areaName, 0.16, 0.92, 0.5, 4, 254, 189, 12, 200, true, true, 1);
        drawText2d(zoneName, 0.16, 0.95, 0.4, 4, 255, 255, 255, 200, true, true, 1);
    }

    if (player.vehicle) {
        drawText2d(`${(native.getEntitySpeed(player.vehicle.scriptID) * (isMetric ? 3.6 : 2.236936)).toFixed(0)} ${(isMetric) ? 'KM/H' : 'MPH'}`, 0.16, 0.90,
            0.4, 4, 255, 255, 255, 200, true, true, 1);
    }

    if (tempoRestante != '')
        drawText2d(tempoRestante, 0.8, 0.89, 0.4, 4, 255, 255, 255, 200, true, true, 2);
});

alt.on('keydown', (key) => {
    if (!player.hasSyncedMeta('nametag') || player.getMeta('chatting'))
        return;

    if (key == 113) { // F2
        if (playersViewLoading)
            return;

        playersViewLoading = true;
        if (playersView != null) {
            playersView.destroy();
            playersView = null;
            toggleView(false);
            playersViewLoading = false;
            return;
        }

        alt.emitServer('ListarPlayers');
    } else if (key == 66) { // B
        alt.emitServer('AtivarDesativarGPS');
    } else if (key == 76) { // L
        alt.emitServer('TrancarDestrancarVeiculo');
    } else if (key == 72) { // H
        alt.emitServer('Algemar');
    } else if (key == 114) { // F3
        alt.log('F3 keydown');

        /*let obj = native.createObject(alt.hash('P_ld_stinger_s'), player.pos.x, player.pos.y, player.pos.z, true, true, true);
        let netId = native.networkGetNetworkIdFromEntity(obj);
        native.setNetworkIdExistsOnAllMachines(netId, true);
        native.setNetworkIdCanMigrate(netId, false);
        native.setEntityHeading(obj, native.getEntityHeading(player.scriptID));
        native.placeObjectOnGroundProperly(obj);*/
    }
});

alt.onServer('Server:ListarPlayers', (nomeServidor, players, qtdStaffers) => {
    if (playersView != null)
        playersView.destroy();

    playersView = new alt.WebView('http://resource/players/players.html');
    playersView.on('load', () => {
        playersView.emit('showView', nomeServidor, players, qtdStaffers);
        playersViewLoading = false;
    });
    playersView.focus();
    toggleView(true);
});

alt.onServer('Server:BaseHTML', (html) => {
    if (view != null)
        view.destroy();
    view = new alt.WebView('http://resource/login/base.html');
    view.on('load', () => {
        view.emit('showHTML', html);
    });
    view.on('closeView', () => {
        if (view != null)
            view.destroy();
        view = null;
        toggleView(false);
    });
    view.focus();
    toggleView(true);
});

alt.onServer('Server:Login', (usuario) => {
    serverLogin(usuario);
});
function serverLogin(usuario) {
    if (view == null)
        view = new alt.WebView('http://resource/login/login.html');

    view.on('load', () => {
        view.emit('showLogin', usuario);
    });

    view.focus();

    view.on('entrarUsuario', (usuario, senha) => {
        alt.emitServer('EntrarUsuario', usuario, senha);
    });
    view.on('registrarUsuario', () => {
        view.destroy();
        view = new alt.WebView('http://resource/login/registro.html');  
        view.focus();
    
        view.on('voltarLogin', () => {
            view.destroy();
            view = null;
            serverLogin('');
        });
        view.on('registrarUsuario', (usuario, email, senha, senha2) => {
            alt.emitServer('RegistrarUsuario', usuario, email, senha, senha2);
        });
    });
    toggleView(true);
}

alt.onServer('Server:ConfirmarLogin', () => {
    if (view != null) {
        view.destroy();
        view = null;
    }

    native.destroyAllCams(true);
    native.renderScriptCams(false, false, 0, false, false);
    native.clearFocus();
    native.displayHud(true);
    native.displayRadar(true);
    activateChat(true);
    toggleView(false);
});

alt.onServer('Server:MostrarErro', (erro) => {
    view.emit('mostrarErro', erro);
});

alt.onServer('Server:MostrarSucesso', (mensagem) => {
    view.emit('mostrarSucesso', mensagem);
});

alt.onServer('setPedIntoVehicle', (veh, seatIndex, freeze = false) => {
    let interval = alt.setInterval(() => { 
        native.setPedIntoVehicle(player.scriptID, veh.scriptID, seatIndex); 
        if (player.vehicle == veh) {
            alt.clearInterval(interval);
            native.setVehicleEngineOn(veh.scriptID, true, true, true); 
            if (freeze)
                toggleGameControls(false, veh);
        }
    }, 61);
});

alt.onServer('alt:log', (msg) => {
    alt.log(msg);
});

alt.onServer('blip:create', (codigo, tipo, nome, cor, entity) => {
    createBlip(codigo, tipo, nome, cor, entity);
});

alt.onServer('blip:remove', (codigo) => {
    removeBlip(codigo);
});

alt.onServer('AbrirSelecionarVeiculo', () => {
    activateChat(false);
    const ui = new NativeUI.Menu('Seu Veículo', 'Selecione seu veículo', new NativeUI.Point(25, 25));
    ui.AddItem(new NativeUI.UIMenuItem('VAPID Police Cruiser', 'POLICE'));
    ui.AddItem(new NativeUI.UIMenuItem('Bravado Police Interceptor', 'POLICE2'));
    ui.AddItem(new NativeUI.UIMenuItem('VAPID Unmarked Police Cruiser', 'POLICE4'));
    ui.AddItem(new NativeUI.UIMenuItem('Buffallo Unmarked Police Cruiser', 'FBI'));
    ui.AddItem(new NativeUI.UIMenuItem('DECLASSE Police Unmarked', 'FBI2'));
    ui.AddItem(new NativeUI.UIMenuItem('VAPID Sheriff Cruiser', 'SHERIFF'));
    ui.AddItem(new NativeUI.UIMenuItem('DECLASSE Sheriff', 'SHERIFF2'));
    ui.AddItem(new NativeUI.UIMenuItem('DECLASSE North Yankton State Patrol', 'POLICEOLD1'));
    ui.AddItem(new NativeUI.UIMenuItem('Albany Esperanto North Yankton State Patrol', 'POLICEOLD2'));
    ui.AddItem(new NativeUI.UIMenuItem('DECLASSE Park Ranger', 'PRANGER'));
    ui.AddItem(new NativeUI.UIMenuItem('Police Motorcycle', 'POLICEB'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~VAPID Police Interceptor', 'POLICE3'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~VAPID Police Cruiser Slicktop', 'POLICESLICK'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~VAPID Old Police Cruiser', 'POLICEOLD'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~VAPID Police Scout', 'PSCOUT'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~VAPID Police Contender', 'BEACHP'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~DECLASSE Merit Police Unmarked', 'POLMERIT2'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~BRAVADO Unmarked Police Cruiser', 'POLICE42'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~VAPID Police Speedo', 'POLSPEEDO'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~Police RIOT', 'POLRIOT'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~LSPD Motorcycle', 'LSPDB'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~VAPID Police Cruiser v2', 'PULICE'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~Bravado Police Interceptor v2', 'PULICE2'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~VAPID Police Interceptor v2', 'PULICE3'));
    ui.AddItem(new NativeUI.UIMenuItem('~q~[VIP] ~s~Stanier Unmarked Police Cruiser', 'PULICE4'));
    
    ui.ItemSelect.on(item => {
        if (item instanceof NativeUI.UIMenuItem) {
            alt.emitServer('SelecionarVeiculo', item.Text, item.Description);
            ui.Close();
        }
    });

    ui.MenuClose.on(() => {
        activateChat(true);
    });

    ui.Open();
});

alt.onServer('AbrirSelecionarSkin', () => {
    activateChat(false);
    const ui = new NativeUI.Menu('Sua Skin', 'Selecione sua skin', new NativeUI.Point(25, 25));
    ui.AddItem(new NativeUI.UIMenuItem('Policial homem', 'Cop01SMY'));
    ui.AddItem(new NativeUI.UIMenuItem('Policial mulher', 'Cop01FMY'));
    ui.AddItem(new NativeUI.UIMenuItem('Sheriff homem', 'Sheriff01SMY'));
    ui.AddItem(new NativeUI.UIMenuItem('Sheriff mulher', 'Sheriff01SFY'));
    ui.AddItem(new NativeUI.UIMenuItem('Segurança homem', 'Security01SMM'));
    ui.AddItem(new NativeUI.UIMenuItem('Policial homem de jaqueta', 'SnowCop01SMM'));
    ui.AddItem(new NativeUI.UIMenuItem('SWAT homem', 'SWAT01SMY'));
    ui.AddItem(new NativeUI.UIMenuItem('Guarda de trânsito homem', 'TrafficWarden'));
    ui.AddItem(new NativeUI.UIMenuItem('FBI homem', 'FIBSec01'));
    ui.AddItem(new NativeUI.UIMenuItem('CIA homem', 'CIASec01SMM'));
    
    ui.ItemSelect.on(item => {
        if (item instanceof NativeUI.UIMenuItem) {
            alt.emitServer('SelecionarSkin', item.Text, item.Description);
            ui.Close();
        }
    });

    ui.MenuClose.on(() => {
        activateChat(true);
    });

    ui.Open();
});

alt.onServer('toggleGameControls', toggleGameControls);
function toggleGameControls(toggle, veh = null) {
    native.freezeEntityPosition(player.scriptID, !toggle);
    if (veh != null)
        native.freezeEntityPosition(veh.scriptID, !toggle);
    else if (player.vehicle)
        native.freezeEntityPosition(player.vehicle.scriptID, !toggle);
}

alt.onServer('vehicle:setVehicleEngineOn', (vehicle, state) => {
    native.setVehicleEngineOn(vehicle.scriptID, state, true, true);
});

alt.onServer('setPlayerCanDoDriveBy', (toggle) => {
    native.setPlayerCanDoDriveBy(player.scriptID, toggle);
});

alt.onServer('displayAdvancedNotification', displayAdvancedNotification);
function displayAdvancedNotification(message, title = '', subtitle = '', notifImage = null, iconType = 0, backgroundColor = null, durationMult = 1) {
    native.beginTextCommandThefeedPost('STRING');
    native.addTextComponentSubstringPlayerName(message);
    if (backgroundColor != null) 
        native.thefeedSetNextPostBackgroundColor(backgroundColor);
    if (notifImage != null) 
        native.endTextCommandThefeedPostMessagetextTu(notifImage, notifImage, false, iconType, title, subtitle, durationMult);
    return native.endTextCommandThefeedPostTicker(false, true);
}

alt.onServer('AbrirSelecionarPinturaArmas', () => {
    activateChat(false);
    const ui = new NativeUI.Menu('Pintura das Armas', 'Selecione sua pintura', new NativeUI.Point(25, 25));
    ui.AddItem(new NativeUI.UIMenuItem('0 - Normal', ''));
    ui.AddItem(new NativeUI.UIMenuItem('1 - Verde', ''));
    ui.AddItem(new NativeUI.UIMenuItem('2 - Dourada', ''));
    ui.AddItem(new NativeUI.UIMenuItem('3 - Rosa', ''));
    ui.AddItem(new NativeUI.UIMenuItem('4 - Exército', ''));
    ui.AddItem(new NativeUI.UIMenuItem('5 - LSPD', ''));
    ui.AddItem(new NativeUI.UIMenuItem('6 - Laranja', ''));
    ui.AddItem(new NativeUI.UIMenuItem('7 - Platina', ''));
    
    ui.ItemSelect.on(item => {
        if (item instanceof NativeUI.UIMenuItem) {
            alt.emitServer('SelecionarPinturaArmas', item.Text);
            ui.Close();
        }
    });

    ui.MenuClose.on(() => {
        activateChat(true);
    });

    ui.Open();
});

alt.onServer('SpectatePlayer', (target) => {
    native.freezeEntityPosition(player.scriptID, true);
    native.destroyAllCams(true);
    native.renderScriptCams(false, false, 0, false, false);

    if (intervalSpec != null)
        alt.clearInterval(intervalSpec);

    intervalSpec = alt.setInterval(() => { 
        native.setEntityVisible(player.scriptID, false, false);
        native.setEntityInvincible(player.scriptID, true);
        if (target.scriptID != 0) {
            alt.clearInterval(intervalSpec);
            native.attachEntityToEntity(player.scriptID, target.scriptID, 0, 0.0, 0.0, 5.0, 0.0, 0.0, 0.0, true, false, false, false, 0, false);
            let cam = native.createCamWithParams('DEFAULT_SCRIPTED_CAMERA', target.pos.x, target.pos.y, target.pos.z, 0, 0, 0, 60);
            native.setCamActive(cam, true);
            native.renderScriptCams(true, false, 0, true, false);
            native.setCamAffectsAiming(cam, false);
            native.attachCamToEntity(cam, target.scriptID, 0, -8.0, 5.0, true); 
            native.pointCamAtEntity(cam, target.scriptID, 0.0, 0.0, 0.0, true);
            intervalSpec = null;
        }
    }, 61);
});

alt.onServer('UnspectatePlayer', () => {
    if (intervalSpec != null)
        alt.clearInterval(intervalSpec);

    native.destroyAllCams(true);
    native.renderScriptCams(false, false, 0, false, false);
    native.detachEntity(player.scriptID, true, true);
    native.freezeEntityPosition(player.scriptID, false);
    native.setEntityVisible(player.scriptID, true, true);
    native.setEntityInvincible(player.scriptID, false);
});

function toggleView(show) {
    showCursor(show);
    alt.toggleGameControls(!show);
    toggleInput(!show);
}