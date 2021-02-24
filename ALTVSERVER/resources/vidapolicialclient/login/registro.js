function registrarSubmit() {
    if (!$('#user').val() || !$('#email').val() || !$('#password').val() || !$('#password2').val()) {
        mostrarErro("Verifique se todos os campos foram preenchidos corretamente!");
        return;
    }

    alt.emit("registrarUsuario", $('#user').val(), $('#email').val(), $('#password').val(), $('#password2').val());
}

function voltarLogin() {
    alt.emit("voltarLogin");
}

function mostrarErro(erro) {
    if (erro != "")  {
        $('#erro').html(erro);
        $('#erro').css('display', 'block');
    }
}

if('alt' in window)
    alt.on('mostrarErro', mostrarErro);