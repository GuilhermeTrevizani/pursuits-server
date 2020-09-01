using System;

namespace VidaPolicial.Entities
{
    public class Log
    {
        public long Codigo { get; set; }
        public DateTime Data { get; set; }
        public TipoLog Tipo { get; set; }
        public string Descricao { get; set; }
        public int UsuarioOrigem { get; set; }
        public int UsuarioDestino { get; set; }
        public long SocialClubOrigem { get; set; }
        public long SocialClubDestino { get; set; }
        public string IPOrigem { get; set; }
        public string IPDestino { get; set; }
        public long HardwareIdHashOrigem { get; set; }
        public long HardwareIdExHashOrigem { get; set; }
        public long HardwareIdHashDestino { get; set; }
        public long HardwareIdExHashDestino { get; set; }
    }
}