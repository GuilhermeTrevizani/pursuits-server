using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
                if (ban.Expiracao.HasValue && DateTime.Now > ban.Expiracao)
                {
                    context.Banimentos.Remove(ban);
                    context.SaveChanges();
                    return true;
                }


                var usuario = context.Usuarios.FirstOrDefault(x => x.Codigo == ban.Usuario)?.Nome;
                var staff = context.Usuarios.FirstOrDefault(x => x.Codigo == ban.UsuarioStaff)?.Nome;
                var strBan = ban.Expiracao == null ? " permanentemente." : $". Seu banimento expira em: <strong>{ban.Expiracao}</strong>.";

                var html = $@"<div class='box-header with-border'>
                <h3>{Global.NomeServidor} • Banimento</h3> 
                </div>
                <div class='box-body' style='max-height:50vh;overflow-y:auto;overflow-x:hidden;'>
                Você está banido{strBan}<br/>
                Usuário: <strong>{usuario}</strong><br/>
                Data: <strong>{ban.Data}</strong><br/>
                Motivo: <strong>{ban.Motivo}</strong><br/>
                Staffer: <strong>{staff}</strong>
                </div>";

                player.Emit("Server:BaseHTML", html);
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

        public static Usuario ObterUsuario(IPlayer player) => Global.Usuarios.FirstOrDefault(x => x?.Player?.HardwareIdHash == player.HardwareIdHash);

        public static void SalvarUsuario(Usuario u)
        {
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
                    EnviarMensagem(player, TipoMensagem.Erro, $"O jogador não pode ser você.");
                    return null;
                }

                return p;
            }

            var ps = Global.Usuarios.Where(x => x.Nome.ToLower().Contains(idNome.ToLower())).ToList();
            if (ps.Count == 1)
            {
                if (!isPodeProprioPlayer && player == ps.FirstOrDefault().Player)
                {
                    EnviarMensagem(player, TipoMensagem.Erro, $"O jogador não pode ser você.");
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

        public static void SpawnarPlayer(IPlayer player)
        {
            var x = ObterUsuario(player);
            x.GPS = false;
            x.Policial = false;
            x.VeiculoPerseguicao = null;

            foreach (var u in Global.Usuarios)
            {
                u.Player.Emit("blip:remove", x.ID);
                player.Emit("blip:remove", u.ID);
            }

            player.SetDateTime(DateTime.Now);
            player.SetWeather(WeatherType.Clear);
            player.Model = (uint)PedModel.FreemodeMale01;
            player.Health = player.MaxHealth;
            player.Armor = player.MaxArmor;
            player.Dimension = 0;
            player.SetSyncedMetaData("congelar", false);
            player.SetSyncedMetaData("atirou", false);
            var tempo = Global.Perseguicoes.Select(y => y.Inicio).DefaultIfEmpty(DateTime.MinValue).Max();
            player.SetSyncedMetaData("tempo", tempo != DateTime.MinValue ? tempo?.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty);
            player.SetSyncedMetaData("podeatirar", true);
            player.Emit("setPlayerCanDoDriveBy", true);
            player.Emit("toggleGameControls", true);

            if (x.ArenaDM)
                SpawnarPlayerArenaDM(player, false);
            else
                player.Spawn(new Position(2451.5078f, 3768.4878f, 41.37805f));
        }

        public static void SpawnarPlayerArenaDM(IPlayer player, bool exibirAviso = true)
        {
            var posicoes = new List<Situacao.Posicao>()
            {
                new Situacao.Posicao(new Position(3074.7825f, -4784.4526f, 6.060791f), new Rotation(0f, 0f, -2.7705386f)),
                new Situacao.Posicao(new Position(3076.1143f, -4739.921f, 6.060791f), new Rotation(0f, 0f, 0.1978956f)),
                new Situacao.Posicao(new Position(3055.8462f, -4714.391f, 6.060791f), new Rotation(0f, 0f, 0.0494739f)),
                new Situacao.Posicao(new Position(3067.6748f, -4683.811f, 6.060791f), new Rotation(0f, 0f, 1.1873736f)),
            };

            var pos = posicoes[new Random().Next(0, 3)];

            var p = ObterUsuario(player);

            player.Spawn(pos.Position);
            player.Rotation = pos.Rotation;
            player.GiveWeapon(WeaponModel.PumpShotgun, 1000, false);
            player.SetWeaponTintIndex(WeaponModel.PumpShotgun, p.Pintura);
            player.GiveWeapon(WeaponModel.MicroSMG, 1000, false);
            player.SetWeaponTintIndex(WeaponModel.MicroSMG, p.Pintura);
            player.GiveWeapon(WeaponModel.Pistol, 1000, true);
            player.SetWeaponTintIndex(WeaponModel.Pistol, p.Pintura);

            p.ArenaDM = true;

            if (exibirAviso)
            {
                foreach (var u in Global.Usuarios.Where(x => x.Player.Dimension == 0))
                    EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"{{{Global.CorAmarelo}}}{p.Nome}{{#FFFFFF}} foi para a arena DM. {{{Global.CorAmarelo}}}(/dm)");
            }
        }

        public static bool ValidarEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                var m = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}