using AltV.Net.Data;
using AltV.Net.Enums;
using System.Collections.Generic;

namespace VidaPolicial
{
    public class Situacao
    {
        public VehicleModel VeiculoFugitivo { get; set; }
        public Posicao PosicaoFugitivo { get; set; }
        public List<Posicao> PosicoesPoliciais { get; set; }
     
        public class Posicao
        {
            public Posicao(Position position, Rotation rotation)
            {
                Position = position;
                Rotation = rotation;
            }

            public Posicao()
            {

            }

            public Position Position { get; set; }
            public Rotation Rotation { get; set; }
        }
    }
}