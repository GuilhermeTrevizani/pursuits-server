using AltV.Net.Enums;
using System.Collections.Generic;
using VidaPolicial.Entities;

namespace VidaPolicial
{
    public static class Global
    {
        public static string CorSucesso { get; set; } = "#6EB469";
        public static string CorAmarelo { get; set; } = "#FEBD0C";
        public static string CorErro { get; set; } = "#FF6A4D";
        public static string NomeServidor { get; set; }
        public static int MaxPlayers { get; set; }
        public static string ConnectionString { get; set; }
        public static WeatherType Weather { get; set; }
        public static Parametro Parametros { get; set; }
        public static List<Usuario> Usuarios { get; set; }
        public static List<Situacao> Situacoes { get; set; }
        public static List<Perseguicao> Perseguicoes { get; set; }
    }
}