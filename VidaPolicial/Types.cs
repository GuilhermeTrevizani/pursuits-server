namespace VidaPolicial
{
    public enum TipoMensagem
    {
        Nenhum,
        Erro,
        Sucesso,
        Titulo,
        Punicao,
    }

    public enum TipoStaff
    {
        Jogador = 0,

        Ajudante = 1,

        Administrador = 2,

        Diretor = 1337,
    }

    public enum TipoLog
    {
        Staff = 1,
        Morte = 2,
    }
}