using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using VidaPolicial.Entities;

namespace VidaPolicial
{
    public static class Functions
    {
        public static string Criptografar(string texto)
        {
            var encodedValue = Encoding.UTF8.GetBytes(texto);
            var encryptedPassword = SHA512.Create().ComputeHash(encodedValue);

            var sb = new StringBuilder();
            foreach (var caracter in encryptedPassword)
                sb.Append(caracter.ToString("X2"));

            return sb.ToString();
        }

        public static bool VerificarBanimento(IPlayer player, Banimento ban)
        {
            if (ban == null)
                return true;

            using (var context = new DatabaseContext())
            {
                if (ban.Expiracao != null)
                {
                    if (DateTime.Now > ban.Expiracao)
                    {
                        context.Banimentos.Remove(ban);
                        context.SaveChanges();
                        return true;
                    }
                }

                var staff = context.Usuarios.FirstOrDefault(x => x.Codigo == ban.UsuarioStaff)?.Nome;
                var strBan = ban.Expiracao == null ? " permanentemente." : $". Seu banimento expira em: {ban.Expiracao?.ToString()}";

                player.Emit("Server:BaseHTML", $"Você está banido{strBan}<br/>Data: <strong>{ban.Data}</strong><br/>Motivo: <strong>{ban.Motivo}</strong><br/>Staffer: <strong>{staff}</strong>");
            }

            return false;
        }

        public static string ObterIP(IPlayer player) => player != null ? player.Ip.Replace("::ffff:", string.Empty) : string.Empty;

        public static void EnviarMensagem(IPlayer player, TipoMensagem tipoMensagem, string mensagem, string cor = "#FFFFFF", bool notify = false)
        {
            var gradient = new int[3];
            var type = "info";
            var icone = string.Empty;

            if (tipoMensagem == TipoMensagem.Sucesso)
            {
                cor = Global.CorSucesso;
                gradient = new int[] { 110, 180, 105 };
                icone = "check";
                type = "success";
            }
            else if (tipoMensagem == TipoMensagem.Erro || tipoMensagem == TipoMensagem.Punicao)
            {
                cor = Global.CorErro;
                gradient = new int[] { 255, 106, 77 };
                icone = "alert";
                type = "danger";
            }
            else if (tipoMensagem == TipoMensagem.Titulo)
            {
                cor = "#B0B0B0";
                gradient = new int[] { 176, 176, 176 };
                icone = "info";
            }

            if (notify)
            {
                player.Emit("chat:notify", mensagem, type);
                return;
            }

            var matches = new Regex("{#.*?}").Matches(mensagem).ToList();

            foreach (Match x in matches)
                mensagem = mensagem.Replace(x.Value, $"{(matches.IndexOf(x) != 0 ? "</span>" : string.Empty)}<span style='color:{x.Value.Replace("{", string.Empty).Replace("}", string.Empty)}'>");

            if (matches.Count > 0)
                mensagem += "</span>";

            player.Emit("chat:sendMessage", mensagem, cor, tipoMensagem == TipoMensagem.Nenhum ? null : gradient, icone);
        }

        public static Usuario ObterUsuario(IPlayer player) => Global.Usuarios.FirstOrDefault(x => x.Player.HardwareIdHash == player.HardwareIdHash);

        public static void SalvarUsuario(Usuario u)
        {
            u.Player.SetDateTime(DateTime.Now);
            using var context = new DatabaseContext();
            u.DataUltimoAcesso = DateTime.Now;
            context.Usuarios.Update(u);
            context.SaveChanges();
        }

        public static void GravarLog(TipoLog tipo, string descricao, Usuario origem, Usuario destino)
        {
            using var context = new DatabaseContext();
            context.Logs.Add(new Log()
            {
                Data = DateTime.Now,
                Tipo = tipo,
                Descricao = descricao,
                UsuarioOrigem = origem.Codigo,
                IPOrigem = origem.IPUltimoAcesso,
                SocialClubOrigem = origem.SocialClubUltimoAcesso,
                HardwareIdHashOrigem = origem.HardwareIdHashUltimoAcesso,
                HardwareIdExHashOrigem = origem.HardwareIdExHashUltimoAcesso,
                UsuarioDestino = destino?.Codigo ?? 0,
                IPDestino = destino?.IPUltimoAcesso ?? string.Empty,
                SocialClubDestino = destino?.SocialClubUltimoAcesso ?? 0,
                HardwareIdHashDestino = destino?.HardwareIdHashUltimoAcesso ?? 0,
                HardwareIdExHashDestino = destino?.HardwareIdExHashUltimoAcesso ?? 0,
            });
            context.SaveChanges();
        }

        public static Usuario ObterUsuarioPorIdNome(IPlayer player, string idNome, bool isPodeProprioPlayer = true)
        {
            int.TryParse(idNome, out int id);
            var p = Global.Usuarios.FirstOrDefault(x => x.ID == id);
            if (p != null)
            {
                if (!isPodeProprioPlayer && player == p.Player)
                {
                    EnviarMensagem(player, TipoMensagem.Erro, $"O jogador não pode ser você!");
                    return null;
                }

                return p;
            }

            var ps = Global.Usuarios.Where(x => x.Nome.ToLower().Contains(idNome.ToLower())).ToList();
            if (ps.Count == 1)
            {
                if (!isPodeProprioPlayer && player == ps.FirstOrDefault().Player)
                {
                    EnviarMensagem(player, TipoMensagem.Erro, $"O jogador não pode ser você!");
                    return null;
                }

                return ps.FirstOrDefault();
            }

            if (ps.Count > 0)
            {
                EnviarMensagem(player, TipoMensagem.Erro, $"Mais de um jogador foi encontrado com a pesquisa: {idNome}");
                foreach (var pl in ps)
                    EnviarMensagem(player, TipoMensagem.Nenhum, $"[ID: {pl.ID}] {pl.Nome}");
            }
            else
            {
                EnviarMensagem(player, TipoMensagem.Erro, $"Nenhum jogador foi encontrado com a pesquisa: {idNome}");
            }

            return null;
        }

        public static int ObterNovoID()
        {
            for (var i = 1; i <= Global.MaxPlayers; i++)
            {
                if (Global.Usuarios.Any(x => x.ID == i))
                    continue;

                return i;
            }

            return 1;
        }

        public static void SpawnarPlayer(IPlayer player, bool busy = false)
        {
            var x = ObterUsuario(player);
            if (x.VeiculoPerseguicao != null)
            {
                x.GPS = false;
                x.Policial = false;
                x.VeiculoPerseguicao = null;

                foreach (var u in Global.Usuarios)
                {
                    u.Player.Emit("blip:remove", x.ID);
                    player.Emit("blip:remove", u.ID);
                }
            }

            player.Model = (uint)PedModel.Agent14;
            player.Armor = 0;
            player.Dimension = 0;
            player.Spawn(new Position(2451.5078f, 3768.4878f, 41.37805f));
            player.SetSyncedMetaData("congelar", false);
            player.SetSyncedMetaData("atirou", false);
            var tempo = Global.Perseguicoes.Select(y => y.Inicio).DefaultIfEmpty(DateTime.MinValue).Max();
            player.SetSyncedMetaData("tempo", tempo != DateTime.MinValue ? tempo?.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty);

            if (busy)
            {
                player.SetSyncedMetaData("podeatirar", false);
                player.Emit("setPlayerCanDoDriveBy", false);
                player.Emit("toggleGameControls", false);
            }
            else
            {
                player.GiveWeapon(WeaponModel.Pistol, 1000, true);
                player.SetSyncedMetaData("podeatirar", true);
                player.Emit("setPlayerCanDoDriveBy", true);
            }
        }

        public static void CarregarSituacoes()
        {
            Global.Situacoes = new List<Situacao>()
            {
                new Situacao()
                {
                    VeiculoFugitivo = VehicleModel.Schafter2,
                    PosicaoFugitivo = new Situacao.Posicao(new Position(-104.07033f, -1503.0593f, 33.188965f), new Rotation(0f, 0.015625f, 2.453125f)),
                    PosicoesPoliciais = new List<Situacao.Posicao>()
                    {
                        new Situacao.Posicao(new Position(-55.331867f, -1461.2307f, 31.554565f), new Rotation(0f, 0.046875f, 1.71875f)),
                        new Situacao.Posicao(new Position(-11.446152f, -1458.277f, 30.071777f), new Rotation(0f, 0.03125f, 1.609375f)),
                        new Situacao.Posicao(new Position(-29.986813f, -1502.9275f, 30.206543f),new Rotation(0.03125f, 0f, 0.90625f)),
                        new Situacao.Posicao( new Position(63.6f, -1387.4901f, 28.926025f),new Rotation(-0.015625f, 0f, 0.046875f)),
                        new Situacao.Posicao(new Position(26.10989f, -1357.2924f, 28.808105f),new Rotation(0f, 0.046875f, 1.546875f)),
                        new Situacao.Posicao(new Position(-58.074726f, -1357.3055f, 28.858643f),new Rotation(0f, 0.046875f, 1.515625f)),
                        new Situacao.Posicao(new Position(-146.47913f, -1373.3539f, 28.993408f),new Rotation(0f, 0.0625f, 2.125f)),
                        new Situacao.Posicao(new Position(-201.87692f, -1407.0198f, 30.745728f),new Rotation(0f, 0.046875f, 2.0625f)),
                        new Situacao.Posicao(new Position(-176.14944f, -1512.8044f, 32.835205f),new Rotation(-0.109375f, 0f, -0.703125f)),
                    },
                },
                new Situacao()
                {
                    VeiculoFugitivo = VehicleModel.Dominator3,
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
                    VeiculoFugitivo = VehicleModel.Schafter2,
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
                    VeiculoFugitivo = VehicleModel.Dominator3,
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
                }
            };
        }
    }
}