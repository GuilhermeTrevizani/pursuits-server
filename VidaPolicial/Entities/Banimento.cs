﻿using System;

namespace VidaPolicial.Entities
{
    public class Banimento
    {
        public int Codigo { get; set; }
        public DateTime Data { get; set; }
        public DateTime? Expiracao { get; set; }
        public int Usuario { get; set; }
        public long SocialClub { get; set; }
        public long HardwareIdHash { get; set; }
        public long HardwareIdExHash { get; set; }
        public string Motivo { get; set; }
        public int UsuarioStaff { get; set; }
    }
}