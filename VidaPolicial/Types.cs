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
        Nenhum = 0,

        Helper = 1,

        Administrator = 2,

        Manager = 1337,
    }

    public enum TipoLog
    {
        Staff = 1,
        Morte = 2,
    }
}