using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using System;
using System.Linq;

namespace VidaPolicial
{
    public class Commands
    {
        #region Jogador
        [Command("ajuda")]
        public void CMD_ajuda(IPlayer player)
        {
            var p = Functions.ObterUsuario(player);

            var html = $@"<div class='box-header with-border'>
                <h3>{Global.NomeServidor} • Lista de Comandos<span onclick='closeView()' class='pull-right label label-danger'>X</span></h3> 
            </div>
        <div class='box-body' style='max-height:50vh;overflow-y:auto;overflow-x:hidden;'>
            <table class='table table-bordered table-striped'>
                <thead>
                    <tr>
                        <th>Categoria</th>
                        <th>Comando</th>
                        <th>Descrição</th>
                    </tr>
                </thead>
                <tbody>
                <tr>
                    <td>Teclas</td>
                    <td>F2</td>
                    <td>Exibe os players online  (mantenha pressionada)</td>
                </tr>
                <tr>
                    <td>Teclas</td>
                    <td>B</td>
                    <td>Liga/desliga GPS quando policial</td>
                </tr>
                <tr>
                    <td>Teclas</td>
                    <td>L</td>
                    <td>Tranca/destranca veículo próprio quando policial</td>
                </tr>
                <tr>
                    <td>Teclas</td>
                    <td>H</td>
                    <td>Algema quando policial</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/sobre</td>
                    <td>Visualiza sobre o servidor</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/ajuda</td>
                    <td>Visualiza os comandos servidor</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/skin</td>
                    <td>Seleciona sua skin quando policial</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/veiculo</td>
                    <td>Selecionar seu veículo quando policial</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/pm</td>
                    <td>Envia uma mensagem privada para outro jogador</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/r</td>
                    <td>Envia uma mensagem no rádio quando policial</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/desistir</td>
                    <td>Desiste de uma perseguição sendo policial ou bandido</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/timestamp</td>
                    <td>Ativa/desativa timestamp do chat</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/dm</td>
                    <td>Entra na arena DM</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/helicoptero /heli</td>
                    <td>Ativa/desativa opção por helicóptero nas perseguições</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/sair</td>
                    <td>Sai da arena DM</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/spec</td>
                    <td>Observa um policial em uma perseguição</td>
                </tr>
                <tr>
                    <td>Jogador</td>
                    <td>/specoff</td>
                    <td>Para de observar</td>
                </tr>";

            if (p.DataTerminoVIP.HasValue)
                html += $@"<tr>
                    <td>VIP</td>
                    <td>/pintura</td>
                    <td>Altera a pintura das armas nas perseguições</td>
                </tr>";

            if ((int)p.Staff >= (int)TipoStaff.Ajudante)
                html += $@"<tr>
                    <td>Ajudante</td>
                    <td>/kick</td>
                    <td>Expulsa o jogador</td>
                </tr>";

            if ((int)p.Staff >= (int)TipoStaff.Administrador)
                html += $@"<tr>
                    <td>Administrador</td>
                    <td>/ban</td>
                    <td>Bane o jogador</td>
                </tr>
                <tr>
                    <td>Administrador</td>
                    <td>/banoff</td>
                    <td>Bane o jogador offline</td>
                </tr>
                <tr>
                    <td>Administrador</td>
                    <td>/unban</td>
                    <td>Desbane o jogador</td>
                </tr>
                <tr>
                    <td>Administrador</td>
                    <td>/save</td>
                    <td>Exibe a posição do jogador/veículo no console (F8)</td>
                </tr>
                <tr>
                    <td>Administrador</td>
                    <td>/v</td>
                    <td>Cria um veículo</td>
                </tr>
                <tr>
                    <td>Administrador</td>
                    <td>/gmx</td>
                    <td>Salva as informações do servidor</td>
                </tr>
                <tr>
                    <td>Administrador</td>
                    <td>/rv</td>
                    <td>Remove um veículo</td>
                </tr>
                <tr>
                    <td>Administrador</td>
                    <td>/spec</td>
                    <td>Observa um jogador</td>
                </tr>";

            if ((int)p.Staff >= (int)TipoStaff.Diretor)
                html += $@"<tr>
                    <td>Diretor</td>
                    <td>/staff</td>
                    <td>Modifica o nível de staff de um jogador</td>
                </tr>
                <tr>
                    <td>Diretor</td>
                    <td>/vip</td>
                    <td>Atribui VIP para um jogador online</td>
                </tr>
                <tr>
                    <td>Diretor</td>
                    <td>/vipoff</td>
                    <td>Atribui VIP para um jogador offline</td>
                </tr>";

            html += $@"
                </tbody>
            </table>
            </div>";

            player.Emit("Server:BaseHTML", html);
        }

        [Command("sobre")]
        public void CMD_sobre(IPlayer player)
        {
            var html = $@"<div class='box-header with-border'>
                <h3>{Global.NomeServidor} • Sobre<span onclick='closeView()' class='pull-right label label-danger'>X</span></h3> 
            </div>
            <div class='box-body' style='max-height:50vh;overflow-y:auto;overflow-x:hidden;'>
            <p>GTA V Pursuits é um servidor de perseguições policiais para o alt:V.</p>
            <p>A única regra, tanto no jogo quanto no Discord, é não usar modificações que deem vantagem sobre os outros jogadores e manter o respeito com todos.</p>
            <p>O nosso servidor visa não prender o jogador a fiscalização contínua da administração, por isso é desenvolvido para que o jogador apenas se divirta e o próprio script é a limitação e regras.</p>
            <p>O modo de jogo é planejado tentando ser o mais equilibrado possível, desde a criação dos mapas de perseguições, veículos, até mesmo na quantidade de policiais em uma perseguição.</p>
            <p>Uma perseguição é iniciada com no mínimo 2 jogadores, um fugindo e outro perseguindo. Uma perseguição só pode ter no máximo 10 jogadores, sendo 1 fugindo e outros 9 perseguindo. A cada 10 jogadores, uma nova perseguição é iniciada em uma outra dimensão, ocorrendo-as estas simultâneamente. Novas perseguições só são iniciadas se todas as perseguições ativas já tiverem sido encerradas.</p>
            <p>O bandido tem um tempo de 7 minutos para conseguir escapar ou matar todos os policiais em sua seção. O policial, antes do suspeito atirar, pode dar um taser no suspeito caso a janela do carro já esteja quebrada, fazendo com que este saia do carro. Basta então algemar o suspeito.</p>
            <p>Caso um policial mate outro policial ou mate o suspeito antes dos disparos serem liberados, ele será expulso e perderá 10 níveis.</p>            
            <p>Quando o jogador cai na água, ele tem 10 segundos para sair, caso contrário, começará a perder vida.</p>
            <p>Qualquer dúvida, utilize o nosso canal de suporte no Discord. Estaremos abertos também a sugestões e nos ajude a melhorar reportando os bugs do nosso sistema.</p>
            <p>Discord: <strong>https://discord.gg/TN2fWeQ</strong></p>
            </div>";
            player.Emit("Server:BaseHTML", html);
        }

        [Command("veiculo")]
        public void CMD_veiculo(IPlayer player) => player.Emit("AbrirSelecionarVeiculo");

        [Command("skin")]
        public void CMD_skin(IPlayer player) => player.Emit("AbrirSelecionarSkin");

        [Command("pm", "/pm (ID ou nome) (mensagem)", GreedyArg = true)]
        public void CMD_pm(IPlayer player, string idNome, string mensagem)
        {
            var p = Functions.ObterUsuario(player);

            var target = Functions.ObterUsuarioPorIdNome(player, idNome, false);
            if (target == null)
                return;

            Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"PM para {(!string.IsNullOrWhiteSpace(target.Cor) ? $"{{{target.Cor}}}" : string.Empty)}{target.Nome} {{#F2FF43}}[{target.ID}]: {mensagem}", "#F2FF43");
            Functions.EnviarMensagem(target.Player, TipoMensagem.Nenhum, $"PM de {(!string.IsNullOrWhiteSpace(p.Cor) ? $"{{{p.Cor}}}" : string.Empty)}{p.Nome} {{#F2FF43}}[{p.ID}]: {mensagem}", "#F0E90D");
        }

        [Command("r", "/r (mensagem)", GreedyArg = true)]
        public void CMD_r(IPlayer player, string mensagem)
        {
            var p = Functions.ObterUsuario(player);
            if (!p.Policial)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não é um policial.");
                return;
            }

            foreach (var u in Global.Usuarios.Where(x => x.Player.Dimension == player.Dimension && x.Policial))
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"[RÁDIO] {p.Nome}: {mensagem}", "#FFFF9B");
        }

        [Command("desistir")]
        public void CMD_desistir(IPlayer player)
        {
            if (player.Dimension == 0)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não está em uma perseguição.");
                return;
            }

            var p = Functions.ObterUsuario(player);

            foreach (var u in Global.Usuarios.Where(x => x.Player.Dimension == player.Dimension))
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"{{{Global.CorAmarelo}}}{p.Nome}{{#FFFFFF}} desistiu da perseguição.");

            Functions.SpawnarPlayer(player);
        }

        [Command("timestamp")]
        public void CMD_timestamp(IPlayer player)
        {
            var p = Functions.ObterUsuario(player);
            p.TimeStamp = !p.TimeStamp;
            player.Emit("chat:activateTimeStamp", p.TimeStamp);
            Functions.EnviarMensagem(player, TipoMensagem.Sucesso, $"Você {(!p.TimeStamp ? "des" : string.Empty)}ativou o timestamp do chat.", notify: true);
        }

        [Command("helicoptero", Alias = "heli")]
        public void CMD_helicoptero(IPlayer player)
        {
            var p = Functions.ObterUsuario(player);
            p.Helicoptero = !p.Helicoptero;
            Functions.EnviarMensagem(player, TipoMensagem.Sucesso, $"Você {(!p.Helicoptero ? "des" : string.Empty)}ativou a opção pelo helicóptero nas perseguições.", notify: true);
        }

        [Command("dm")]
        public void CMD_dm(IPlayer player)
        {
            if (player.Dimension != 0)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você está em uma perseguição.");
                return;
            }

            var p = Functions.ObterUsuario(player);
            if (p.ArenaDM)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você já está na arena DM.");
                return;
            }

            Functions.SpawnarPlayerArenaDM(player);
        }

        [Command("sair")]
        public void CMD_sair(IPlayer player)
        {
            if (player.Dimension != 0)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você está em uma perseguição.");
                return;
            }

            var p = Functions.ObterUsuario(player);
            if (!p.ArenaDM)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não está na arena DM.");
                return;
            }

            p.ArenaDM = false;
            Functions.SpawnarPlayer(player);

            foreach (var u in Global.Usuarios.Where(x => x.Player.Dimension == 0))
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"{{{Global.CorAmarelo}}}{p.Nome}{{#FFFFFF}} saiu da arena DM. {{{Global.CorAmarelo}}}(/sair)");
        }

        [Command("spec", "/spec (ID ou nome)")]
        public void CMD_spec(IPlayer player, string idNome)
        {
            var p = Functions.ObterUsuario(player);
            if (player.Dimension != 0)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você está em uma perseguição.");
                return;
            }

            var target = Functions.ObterUsuarioPorIdNome(player, idNome, false);
            if (target == null)
                return;

            if (!target.Policial)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Jogador não é um policial.");
                return;
            }

            if (target.PosicaoSpec.HasValue)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Jogador está observando outro jogador.");
                return;
            }

            if (!p.PosicaoSpec.HasValue)
                p.PosicaoSpec = player.Position;

            player.Dimension = target.Player.Dimension;
            player.Spawn(new Position(target.Player.Position.X, target.Player.Position.Y, target.Player.Position.Z + 15));
            player.SetSyncedMetaData("nametag", string.Empty);
            player.Emit("SpectatePlayer", target.Player);
        }

        [Command("specoff")]
        public void CMD_specoff(IPlayer player)
        {
            var p = Functions.ObterUsuario(player);
            if (!p.PosicaoSpec.HasValue)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não está observando um jogador.");
                return;
            }

            player.Spawn(p.PosicaoSpec.Value);
            player.SetSyncedMetaData("nametag", $"{p.Nome} [{p.ID}]");
            p.PosicaoSpec = null;
            player.Emit("UnspectatePlayer");
        }
        #endregion

        #region VIP
        [Command("pintura")]
        public void CMD_pintura(IPlayer player)
        {
            var p = Functions.ObterUsuario(player);
            if (!p.DataTerminoVIP.HasValue)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não é VIP.");
                return;
            }

            player.Emit("AbrirSelecionarPinturaArmas");
        }
        #endregion

        #region Ajudante
        [Command("kick", "/kick (ID ou nome) (motivo)", GreedyArg = true)]
        public void CMD_kick(IPlayer player, string idNome, string motivo)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Ajudante)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            var target = Functions.ObterUsuarioPorIdNome(player, idNome, false);
            if (target == null)
                return;

            Functions.GravarLog(TipoLog.Staff, $"/kick {motivo}", p, target);
            foreach (var u in Global.Usuarios)
                Functions.EnviarMensagem(u.Player, TipoMensagem.Sucesso, $"{p.Nome} kickou {target.Nome}. Motivo: {motivo}");
            Functions.SalvarUsuario(target);
            target.Player.Kick($"{p.Nome} kickou você. Motivo: {motivo}");
        }
        #endregion Ajudante

        #region Administrador
        [Command("ban", "/ban (ID ou nome) (dias) (motivo)", GreedyArg = true)]
        public void CMD_ban(IPlayer player, string idNome, int dias, string motivo)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Administrador)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            var target = Functions.ObterUsuarioPorIdNome(player, idNome, false);
            if (target == null)
                return;

            using (var context = new DatabaseContext())
            {
                var ban = new Entities.Banimento()
                {
                    Data = DateTime.Now,
                    Expiracao = null,
                    Motivo = motivo,
                    Usuario = target.Codigo,
                    SocialClub = target.SocialClubUltimoAcesso,
                    HardwareIdHash = target.HardwareIdHashUltimoAcesso,
                    HardwareIdExHash = target.HardwareIdExHashUltimoAcesso,
                    UsuarioStaff = p.Codigo,
                };

                if (dias > 0)
                    ban.Expiracao = DateTime.Now.AddDays(dias);

                context.Banimentos.Add(ban);
                context.SaveChanges();
            }

            Functions.GravarLog(TipoLog.Staff, $"/ban {motivo}", p, target);
            var strBan = dias == 0 ? "permanentemente" : $"por {dias} dia{(dias > 1 ? "s" : string.Empty)}";
            foreach (var u in Global.Usuarios)
                Functions.EnviarMensagem(u.Player, TipoMensagem.Sucesso, $"{p.Nome} baniu {target.Nome} {strBan}. Motivo: {motivo}");
            Functions.SalvarUsuario(target);
            target.Player.Kick($"{p.Nome} baniu você {strBan}. Motivo: {motivo}");
        }

        [Command("banoff", "/banoff (nome do usuário) (dias) (motivo)", GreedyArg = true)]
        public void CMD_banoff(IPlayer player, string usuario, int dias, string motivo)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Administrador)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            using var context = new DatabaseContext();
            var user = context.Usuarios.FirstOrDefault(x => x.Nome == usuario);
            if (user == null)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, $"Usuário {usuario} não existe.");
                return;
            }

            var ban = new Entities.Banimento()
            {
                Data = DateTime.Now,
                Expiracao = null,
                Motivo = motivo,
                Usuario = user.Codigo,
                SocialClub = user.SocialClubUltimoAcesso,
                HardwareIdHash = user.HardwareIdHashUltimoAcesso,
                HardwareIdExHash = user.HardwareIdExHashUltimoAcesso,
                UsuarioStaff = p.Codigo,
            };

            if (dias > 0)
                ban.Expiracao = DateTime.Now.AddDays(dias);

            context.Banimentos.Add(ban);
            context.SaveChanges();

            Functions.GravarLog(TipoLog.Staff, $"/banoff {user.Nome} {motivo}", p, null);
            var strBan = dias == 0 ? "permanentemente" : $"por {dias} dia{(dias > 1 ? "s" : string.Empty)}";
            foreach (var u in Global.Usuarios)
                Functions.EnviarMensagem(u.Player, TipoMensagem.Sucesso, $"{p.Nome} baniu {user.Nome} {strBan}. Motivo: {motivo}");
        }

        [Command("unban", "/unban (usuário)")]
        public void CMD_unban(IPlayer player, string usuario)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Administrador)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            using (var context = new DatabaseContext())
            {
                var user = context.Usuarios.FirstOrDefault(x => x.Nome == usuario);
                if (user == null)
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Erro, $"Usuário {usuario} não existe.");
                    return;
                }

                var ban = context.Banimentos.FirstOrDefault(x => x.Usuario == user.Codigo);
                if (ban == null)
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Erro, $"Usuário {usuario} não está banido.");
                    return;
                }

                context.Banimentos.Remove(ban);
                context.SaveChanges();
            }

            Functions.EnviarMensagem(player, TipoMensagem.Sucesso, $"Você desbaniu {usuario}.");
            Functions.GravarLog(TipoLog.Staff, $"/unban {usuario}", p, null);
        }

        [Command("save")]
        public void CMD_save(IPlayer player)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Administrador)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            if (player.IsInVehicle)
            {
                player.Emit("alt:log", $"new Situacao.Posicao(new Rotation({player.Vehicle.Position.X.ToString().Replace(",", ".")}f, {player.Vehicle.Position.Y.ToString().Replace(",", ".")}f, {player.Vehicle.Position.Z.ToString().Replace(",", ".")}f), new Rotation({player.Vehicle.Rotation.Roll.ToString().Replace(",", ".")}f, {player.Vehicle.Rotation.Pitch.ToString().Replace(",", ".")}f, {player.Vehicle.Rotation.Yaw.ToString().Replace(",", ".")}f))");
            }
            else
            {
                player.Emit("alt:log", $"POS: {player.Position.X.ToString().Replace(",", ".")}f, {player.Position.Y.ToString().Replace(",", ".")}f, {player.Position.Z.ToString().Replace(",", ".")}f");
                player.Emit("alt:log", $"ROT: {player.Rotation.Roll.ToString().Replace(",", ".")}f, {player.Rotation.Pitch.ToString().Replace(",", ".")}f, {player.Rotation.Yaw.ToString().Replace(",", ".")}f");
            }
        }

        [Command("v", "/v (modelo)")]
        public void CMD_v(IPlayer player, string modelo)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Administrador)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            if (player.Dimension != 0)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não pode criar um veículo em uma perseguição.");
                return;
            }

            if (!Enum.GetValues(typeof(VehicleModel)).Cast<VehicleModel>().Any(x => x.ToString().ToLower() == modelo.ToLower())
                && !Enum.GetValues(typeof(ModeloVeiculo)).Cast<ModeloVeiculo>().Any(x => x.ToString().ToLower() == modelo.ToLower()))
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, $"Modelo {modelo} não existe.");
                return;
            }

            var veh = Alt.CreateVehicle(modelo, player.Position, player.Rotation);
            veh.Dimension = player.Dimension;
            veh.NumberplateText = "ADMIN";
            veh.EngineOn = true;
            player.Emit("setPedIntoVehicle", veh, -1);
        }

        [Command("rv")]
        public void CMD_rv(IPlayer player)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Administrador)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            if (player.Dimension != 0)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não pode remover um veículo em uma perseguição.");
                return;
            }

            if (!player.IsInVehicle)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não está em um veículo.");
                return;
            }

            Alt.RemoveVehicle(player.Vehicle);
        }

        [Command("gmx")]
        public void CMD_gmx(IPlayer player)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Diretor)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            foreach (var pl in Global.Usuarios)
            {
                Functions.EnviarMensagem(pl.Player, TipoMensagem.Sucesso, $"{p.Nome} salvou as informações do servidor.");
                Functions.SalvarUsuario(pl);
            }
        }

        [Command("aspec", "/aspec (ID ou nome)")]
        public void CMD_aspec(IPlayer player, string idNome)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Administrador)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            if (player.Dimension != 0)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você está em uma perseguição.");
                return;
            }

            var target = Functions.ObterUsuarioPorIdNome(player, idNome, false);
            if (target == null)
                return;

            if (target.PosicaoSpec.HasValue)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Jogador está observando outro jogador.");
                return;
            }

            if (!p.PosicaoSpec.HasValue)
                p.PosicaoSpec = player.Position;

            player.Dimension = target.Player.Dimension;
            player.Spawn(new Position(target.Player.Position.X, target.Player.Position.Y, target.Player.Position.Z + 15));
            player.SetSyncedMetaData("nametag", string.Empty);
            player.Emit("SpectatePlayer", target.Player);
        }
        #endregion Administrador

        #region Diretor
        [Command("staff", "/staff (ID ou nome) (nível)")]
        public void CMD_staff(IPlayer player, string idNome, int staff)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Diretor)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            if (!Enum.GetValues(typeof(TipoStaff)).Cast<TipoStaff>().Any(x => (int)x == staff))
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, $"Staff {staff} não existe.");
                return;
            }

            var target = Functions.ObterUsuarioPorIdNome(player, idNome, false);
            if (target == null)
                return;

            var stf = (TipoStaff)staff;
            target.Staff = stf;
            Functions.EnviarMensagem(target.Player, TipoMensagem.Sucesso, $"{p.Nome} alterou seu nível staff para {stf} [{staff}].");
            Functions.EnviarMensagem(player, TipoMensagem.Sucesso, $"Você alterou o nível staff de {target.Nome} para {stf} [{staff}].");
            Functions.GravarLog(TipoLog.Staff, $"/staff {staff}", p, target);
        }

        [Command("vip", "/vip (ID ou nome)")]
        public void CMD_vip(IPlayer player, string idNome)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Diretor)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            var target = Functions.ObterUsuarioPorIdNome(player, idNome);
            if (target == null)
                return;

            if (target.DataTerminoVIP.HasValue)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, $"Jogador já está com VIP.");
                return;
            }

            target.DataTerminoVIP = DateTime.Now.AddMonths(1);
            Functions.EnviarMensagem(target.Player, TipoMensagem.Sucesso, $"{p.Nome} te setou VIP até {target.DataTerminoVIP}.");
            Functions.EnviarMensagem(player, TipoMensagem.Sucesso, $"Você setou VIP para {target.Nome} até {target.DataTerminoVIP}.");
            Functions.GravarLog(TipoLog.Staff, $"/vip", p, target);
        }

        [Command("vipoff", "/vipoff (usuário)")]
        public void CMD_vipoff(IPlayer player, string usuario)
        {
            var p = Functions.ObterUsuario(player);
            if ((int)p.Staff < (int)TipoStaff.Diretor)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Você não possui autorização para usar esse comando.");
                return;
            }

            var user = Global.Usuarios.FirstOrDefault(x => x.Nome.ToLower() == usuario.ToLower());
            if (user != null)
            {
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Jogador está online. Use /vip.");
                return;
            }

            using (var context = new DatabaseContext())
            {
                var target = context.Usuarios.FirstOrDefault(x => x.Nome == usuario);

                if (target.DataTerminoVIP.HasValue)
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Erro, $"Jogador já está com VIP.");
                    return;
                }

                target.DataTerminoVIP = DateTime.Now.AddMonths(1);
                Functions.EnviarMensagem(player, TipoMensagem.Sucesso, $"Você setou VIP para {target.Nome} até {target.DataTerminoVIP}.");
                context.Usuarios.Update(target);
                context.SaveChanges();

                Functions.GravarLog(TipoLog.Staff, $"/vipoff {target.Nome}", p, null);
            }
        }
        #endregion Diretor
    }
}