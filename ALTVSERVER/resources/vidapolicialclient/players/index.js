function showView(nomeServidor, x, qtdStaffers) {
    let players = JSON.parse(x);
    $('#title').text(`${nomeServidor} • Jogadores Online (${players.length})`);
    $('#bottom').text(`Equipe Administrativa Online: ${qtdStaffers}`);
    $("#onlineplayers").html('');
    players.forEach(function(p) {
	    $("#onlineplayers").append(`<tr><td>${p.ID}</td><td>${p.Nome}</td><td>${p.Level}</td><td ${(p.Cor != "" ? `style='color:${p.Cor}'` : "")}>${p.Tipo}</td><td>${p.Ping}</td></tr>`);
    });
}

if('alt' in window)
    alt.on('showView', showView);