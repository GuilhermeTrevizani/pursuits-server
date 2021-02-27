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

    public enum TipoPosicaoHelicoptero
    {
        LosSantos = 1,
        SandyShores = 2,
        PaletoBay = 3,
    }

    public enum ModeloVeiculo
    {
        PoliceSlick,
        PoliceOld,
        PScout,
        BeachP,
        Polmerit2,
        Police42,
        PolSpeedo,
        PolRiot,
        LSPDB,
    }
}