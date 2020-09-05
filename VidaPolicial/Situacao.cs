using AltV.Net.Data;
using System.Collections.Generic;

namespace VidaPolicial
{
    public class Situacao
    {
        public Posicao PosicaoFugitivo { get; set; }
        public List<Posicao> PosicoesPoliciais { get; set; }
        public TipoPosicaoHelicoptero PosicaoHelicoptero { get; set; } = TipoPosicaoHelicoptero.LosSantos;

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