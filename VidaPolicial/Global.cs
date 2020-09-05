using AltV.Net.Data;
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
        public static Parametro Parametros { get; set; }
        public static List<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public static List<Perseguicao> Perseguicoes { get; set; } = new List<Perseguicao>();

        public static List<Situacao> Situacoes { get; set; } = new List<Situacao>()
        {
            new Situacao()
                {
                    PosicaoFugitivo = new Situacao.Posicao(new Position(-104.07033f, -1503.0593f, 33.188965f), new Rotation(0f, 0.015625f, 2.453125f)),
                    PosicoesPoliciais = new List<Situacao.Posicao>()
                    {
                        new Situacao.Posicao(new Position(-55.331867f, -1461.2307f, 31.554565f), new Rotation(0f, 0.046875f, 1.71875f)),
                        new Situacao.Posicao(new Position(-11.446152f, -1458.277f, 30.071777f), new Rotation(0f, 0.03125f, 1.609375f)),
                        new Situacao.Posicao(new Position(-29.986813f, -1502.9275f, 30.206543f), new Rotation(0.03125f, 0f, 0.90625f)),
                        new Situacao.Posicao( new Position(63.6f, -1387.4901f, 28.926025f), new Rotation(-0.015625f, 0f, 0.046875f)),
                        new Situacao.Posicao(new Position(26.10989f, -1357.2924f, 28.808105f), new Rotation(0f, 0.046875f, 1.546875f)),
                        new Situacao.Posicao(new Position(-58.074726f, -1357.3055f, 28.858643f), new Rotation(0f, 0.046875f, 1.515625f)),
                        new Situacao.Posicao(new Position(-146.47913f, -1373.3539f, 28.993408f), new Rotation(0f, 0.0625f, 2.125f)),
                        new Situacao.Posicao(new Position(-201.87692f, -1407.0198f, 30.745728f), new Rotation(0f, 0.046875f, 2.0625f)),
                        new Situacao.Posicao(new Position(-176.14944f, -1512.8044f, 32.835205f), new Rotation(-0.109375f, 0f, -0.703125f)),
                    },
                },
                new Situacao()
                {
                    PosicaoFugitivo = new Situacao.Posicao(new Position(227.65714f, 200.42638f, 104.901855f),new Rotation(0.015625f, 0f, 1.21875f)),
                    PosicoesPoliciais = new List<Situacao.Posicao>()
                    {
                        new Situacao.Posicao(new Position(173.81538f, 132.56703f, 97.67322f),new Rotation(-0.21875f, 0.046875f, 2.765625f)),
                        new Situacao.Posicao(new Position(188.91429f, 101.670334f, 92.028564f),new Rotation(-0.015625f, -0.1875f, 1.265625f)),
                        new Situacao.Posicao(new Position(233.35385f, -46.443954f, 69.09595f),new Rotation(-0.03125f, 0f, 2.734375f)),
                        new Situacao.Posicao(new Position(342.58023f, 31.793407f, 88.01831f), new Rotation(0.125f, -0.15625f, 1.15625f)),
                        new Situacao.Posicao(new Position(358.9978f, 110.69011f, 102.32385f), new Rotation(0.046875f, 0.046875f, -0.328125f)),
                        new Situacao.Posicao(new Position(293.1165f, 75.0989f, 93.93262f),new Rotation(0f, 0f, 2.796875f)),
                        new Situacao.Posicao(new Position(537.9824f, 87.797806f, 95.76929f),new Rotation(0.03125f, 0.046875f, 1.203125f)),
                        new Situacao.Posicao(new Position(462.01318f, 222.96263f, 102.69446f),new Rotation(0f, 0f, 1.265625f)),
                        new Situacao.Posicao(new Position(476.87473f, 81.125275f, 96.79712f),new Rotation(-0.0625f, 0f, -1.90625f)),
                    },
                },
                new Situacao()
                {
                    PosicaoFugitivo = new Situacao.Posicao(new Position(-656.7033f, -275.789f, 35.278442f),new Rotation(0.015625f, 0f, 0.515625f)),
                    PosicoesPoliciais = new List<Situacao.Posicao>()
                    {
                        new Situacao.Posicao(new Position(-601.5692f, -359.5121f, 34.469604f),new Rotation(0f, 0f, 1.578125f)),
                        new Situacao.Posicao( new Position(-658.0747f, -437.76263f, 34.33484f),new Rotation(-0.03125f, -0.015625f, -1.53125f)),
                        new Situacao.Posicao(new Position(-730.4308f, -371.74945f, 34.587524f),new Rotation(-0.046875f, 0.015625f, -0.453125f)),
                        new Situacao.Posicao(new Position(-824.7033f, -383.23517f, 38.07544f),new Rotation(0.09375f, 0.015625f, 1.140625f)),
                        new Situacao.Posicao(new Position(-932.54504f, -319.45056f, 38.648315f),new Rotation(-0.015625f, 0.046875f, -1.921875f)),
                        new Situacao.Posicao(new Position(-857.7099f, -259.76703f, 39.15381f),new Rotation(0.015625f, 0f, 2.703125f)),
                        new Situacao.Posicao(new Position(-610.2725f, -476.24176f, 34.2843f),new Rotation(0f, 0f, 1.625f)),
                        new Situacao.Posicao(new Position(-477.73187f, -340.41757f, 33.96411f),new Rotation(0f, 0f, 3f)),
                        new Situacao.Posicao(new Position(-433.04175f, -404.43954f, 32.41394f),new Rotation(0f, 0.03125f, -0.125f)),
                    }
                },
                new Situacao()
                {
                    PosicaoFugitivo = new Situacao.Posicao(new Position(-1217.8286f, -1129.1208f, 7.324585f),new Rotation(0.015625f, 0.046875f, 0.28125f)),
                    PosicoesPoliciais = new List<Situacao.Posicao>()
                    {
                        new Situacao.Posicao( new Position(-1207.2131f, -1166.3605f, 7.206543f),new Rotation(0f, 0.0625f, 0.28125f)),
                        new Situacao.Posicao(new Position(-1164.6725f, -1199.3011f, 3.432251f),new Rotation(0.09375f, 0f, 1.78125f)),
                        new Situacao.Posicao(new Position(-1272.3956f, -1239.5472f, 3.769287f),new Rotation(0.015625f, 0.03125f, 0.390625f)),
                        new Situacao.Posicao(new Position(-1279.3583f, -1283.2616f, 3.4659424f),new Rotation(0f, 0.046875f, -1.171875f)),
                        new Situacao.Posicao(new Position(-1195.2527f, -1369.9517f, 4.0893555f),new Rotation(0.046875f, 0.03125f, -1.109375f)),
                        new Situacao.Posicao(new Position(-1205.8418f, -1353.1252f, 3.9714355f),new Rotation(-0.015625f, 0.046875f, 2.046875f)),
                        new Situacao.Posicao( new Position(-1080.3561f, -1282.0483f, 5.201416f),new Rotation(0f, 0.0625f, 2.078125f)),
                        new Situacao.Posicao(new Position(-1175.5648f, -1224.4088f, 6.397827f), new Rotation(0f, 0f, 1.921875f)),
                        new Situacao.Posicao( new Position(-1071.9429f, -1015.0154f, 1.5787354f),new Rotation(0f, -0.0625f, 0.546875f)),
                    }
                },
                new Situacao()
                {
                    PosicaoHelicoptero = TipoPosicaoHelicoptero.SandyShores,
                    PosicaoFugitivo = new Situacao.Posicao(new Rotation(1635.0989f, 3765.389f, 34.40222f), new Rotation(0f, 0.015625f, 2.25f)),
                    PosicoesPoliciais = new List<Situacao.Posicao>()
                    {
                     new Situacao.Posicao(new Rotation(1690.5494f, 3808.576f, 34.60437f), new Rotation(0f, 0f, 2.21875f)),
                     new Situacao.Posicao(new Rotation(1715.3802f, 3792.0132f, 34.36853f), new Rotation(-0.015625f, 0.015625f, -2.515625f)),
                     new Situacao.Posicao(new Rotation(1730.2417f, 3755.1165f, 33.424927f), new Rotation(0f, 0.015625f, 2.109375f)),
                     new Situacao.Posicao(new Rotation(1765.1868f, 3759.0593f, 33.424927f), new Rotation(0f, 0f, -0.984375f)),
                     new Situacao.Posicao(new Rotation(1806.2109f, 3820.7078f, 33.239502f), new Rotation(0f, 0f, 0.515625f)),
                     new Situacao.Posicao(new Rotation(1771.622f, 3880.9583f, 34.098877f), new Rotation(0f, 0.015625f, 0.515625f)),
                     new Situacao.Posicao(new Rotation(1742.7561f, 3925.9648f, 34.48645f), new Rotation(0f, 0f, 2.140625f)),
                     new Situacao.Posicao(new Rotation(1703.3802f, 3901.0945f, 34.419067f), new Rotation(0f, 0f, 2.34375f)),
                     new Situacao.Posicao(new Rotation(1675.8066f, 3877.3977f, 34.40222f), new Rotation(0f, 0.015625f, 2.28125f)),
                    }
                },
                new Situacao()
                {
                    PosicaoHelicoptero = TipoPosicaoHelicoptero.PaletoBay,
                    PosicaoFugitivo = new Situacao.Posicao(new Rotation(-356.33408f, 6186.949f, 30.813232f), new Rotation(0f, 0.046875f, 2.390625f)),
                    PosicoesPoliciais = new List<Situacao.Posicao>()
                    {
                        new Situacao.Posicao(new Rotation(-320.53186f, 6222.646f, 30.86377f), new Rotation(0f, 0.015625f, 2.328125f)),
                        new Situacao.Posicao(new Rotation(-287.35385f, 6255.811f, 30.880615f), new Rotation(0f, 0f, 2.328125f)),
                        new Situacao.Posicao(new Rotation(-343.2791f, 6276.5933f, 30.813232f), new Rotation(0f, 0.046875f, 0.828125f)),
                        new Situacao.Posicao(new Rotation(-349.5956f, 6325.688f, 29.482056f), new Rotation(0f, 0.03125f, 2.328125f)),
                        new Situacao.Posicao(new Rotation(-128.38681f, 6430.0483f, 30.931152f), new Rotation(0f, 0.0625f, -2.296875f)),
                        new Situacao.Posicao(new Rotation(-23.696701f, 6500.822f, 30.880615f), new Rotation(0f, 0f, -0.765625f)),
                        new Situacao.Posicao(new Rotation(67.21319f, 6598.7734f, 30.98169f), new Rotation(0f, 0f, -0.796875f)),
                        new Situacao.Posicao(new Rotation(135.65274f, 6538.1143f, 31.032227f), new Rotation(0f, 0.046875f, -2.953125f)),
                        new Situacao.Posicao(new Rotation(-82.36484f, 6358.1274f, 31.065918f), new Rotation(0f, 0f, -2.359375f)),
                    }
                },
        };

        public static List<PedModel> SkinsFugitivo { get; set; } = new List<PedModel>()
        {
            PedModel.Claypain,
            PedModel.Franklin,
            PedModel.Trevor,
            PedModel.Michael,
        };

        public static List<VehicleModel> VeiculosFugitivo { get; set; } = new List<VehicleModel>()
        {
            VehicleModel.Schafter2,
            VehicleModel.Dominator3,
            VehicleModel.Premier,
        };
    }
}