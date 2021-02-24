function entrar() {
    if (!$('#user').val() || !$('#password').val()) {
        mostrarErro("Verifique se todos os campos foram preenchidos corretamente!");
        return;
    }

    alt.emit('entrarUsuario', $('#user').val(), $('#password').val());
}

function registrar() {
    alt.emit('registrarUsuario');
}

function showLogin(usuario) {
    $('#user').val(usuario);
    if (usuario != "")
    {
        $('#password').focus();
        $('#btn-registrar').hide();
    }
}

function mostrarErro(erro) {
    if (erro != "") {
        $('#erro').html(erro);
        $('#erro').css('display', 'block');
    }
}

if('alt' in window) {
    alt.on('showLogin', showLogin);
    alt.on('mostrarErro', mostrarErro);
}