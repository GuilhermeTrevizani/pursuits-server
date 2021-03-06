﻿using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace VidaPolicial.Entities
{
    public class Usuario
    {
        public int Codigo { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public DateTime DataRegistro { get; set; } = DateTime.Now;
        public long SocialClubRegistro { get; set; } = 0;
        public string IPRegistro { get; set; } = string.Empty;
        public DateTime DataUltimoAcesso { get; set; } = DateTime.Now;
        public long SocialClubUltimoAcesso { get; set; } = 0;
        public string IPUltimoAcesso { get; set; } = string.Empty;
        public TipoStaff Staff { get; set; } = 0;
        public int Level { get; set; } = 1;
        public string Veiculo { get; set; } = VehicleModel.Police2.ToString();
        public long Skin { get; set; } = (long)PedModel.Cop01SMY;
        public long HardwareIdHashRegistro { get; set; }
        public long HardwareIdExHashRegistro { get; set; }
        public long HardwareIdHashUltimoAcesso { get; set; }
        public long HardwareIdExHashUltimoAcesso { get; set; }
        public bool TimeStamp { get; set; } = true;
        public bool Helicoptero { get; set; } = false;
        public DateTime? DataTerminoVIP { get; set; } = null;
        public byte Pintura { get; set; } = 0;

        [NotMapped]
        public IPlayer Player { get; set; }

        [NotMapped]
        public int ID { get; set; }

        [NotMapped]
        public bool Policial { get; set; } = false;

        [NotMapped]
        public IVehicle VeiculoPerseguicao { get; set; }

        [NotMapped]
        public bool GPS { get; set; } = false;

        [NotMapped]
        public string Cor
        {
            get
            {
                var cor = string.Empty;
                if (Staff == TipoStaff.Ajudante)
                    cor = "#1abc9c";
                else if (Staff == TipoStaff.Administrador)
                    cor = "#3498db";
                else if (Staff == TipoStaff.Diretor)
                    cor = "#e81e61";
                else if (DataTerminoVIP.HasValue)
                    cor = "#f47fff";
                return cor;
            }
        }

        [NotMapped]
        public string AreaName { get; set; }

        [NotMapped]
        public string ZoneName { get; set; }

        [NotMapped]
        public bool ArenaDM { get; set; }

        [NotMapped]
        public Position? PosicaoSpec { get; set; }
    }
}