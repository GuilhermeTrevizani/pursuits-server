using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using VidaPolicial.Entities;

namespace VidaPolicial
{
    public class Server : Resource
    {
        Timer TimerPrincipal { get; set; }
        Timer TimerSegundo { get; set; }
        BackgroundWorker BWVerificarPerseguicoes { get; set; }
        BackgroundWorker BWIniciarPerseguicoes { get; set; }

        public override void OnStart()
        {
            Alt.OnPlayerConnect += OnPlayerConnect;
            Alt.OnPlayerDisconnect += OnPlayerDisconnect;
            Alt.OnPlayerDead += OnPlayerDead;
            Alt.OnWeaponDamage += OnWeaponDamage;
            Alt.OnPlayerDamage += OnPlayerDamage;
            Alt.OnClient<IPlayer, string>("OnPlayerChat", OnPlayerChat);
            Alt.OnClient<IPlayer, string, string>("EntrarUsuario", EntrarUsuario);
            Alt.OnClient<IPlayer, string, string, string, string>("RegistrarUsuario", RegistrarUsuario);
            Alt.OnClient<IPlayer>("ListarPlayers", ListarPlayers);
            Alt.OnClient<IPlayer>("TrancarDestrancarVeiculo", TrancarDestrancarVeiculo);
            Alt.OnClient<IPlayer>("Algemar", Algemar);
            Alt.OnClient<IPlayer>("AtivarDesativarGPS", AtivarDesativarGPS);
            Alt.OnClient<IPlayer>("Atirou", Atirou);
            Alt.OnClient<IPlayer, string, string>("SelecionarVeiculo", SelecionarVeiculo);
            Alt.OnClient<IPlayer, string, string>("SelecionarSkin", SelecionarSkin);
            Alt.OnClient<IPlayer, string, string>("AtualizarInformacoes", AtualizarInformacoes);
            Alt.OnClient<IPlayer, string>("SelecionarPinturaArmas", SelecionarPinturaArmas);

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture =
                  CultureInfo.GetCultureInfo("pt-BR");

            var config = JsonConvert.DeserializeObject<Configuracao>(File.ReadAllText("settings.json"));
            Global.MaxPlayers = config.MaxPlayers;
            Global.ConnectionString = $"Server={config.DBHost};Database={config.DBName};Uid={config.DBUser};Password={config.DBPassword}";

            using var context = new DatabaseContext();
            Global.Parametros = context.Parametros.FirstOrDefault();

            BWVerificarPerseguicoes = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            BWVerificarPerseguicoes.DoWork += BWVerificarPerseguicoes_DoWork;
            BWVerificarPerseguicoes.RunWorkerCompleted += BWVerificarPerseguicoes_RunWorkerCompleted;

            BWIniciarPerseguicoes = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            BWIniciarPerseguicoes.DoWork += BWIniciarPerseguicoes_DoWork;

            TimerPrincipal = new Timer(60000);
            TimerPrincipal.Elapsed += TimerPrincipal_Elapsed;
            TimerPrincipal.Start();

            TimerSegundo = new Timer(1000);
            TimerSegundo.Elapsed += TimerSegundo_Elapsed;
            TimerSegundo.Start();
        }

        private void BWIniciarPerseguicoes_DoWork(object sender, DoWorkEventArgs e)
        {
            var users = Global.Usuarios.Where(x => x.Player.Dimension == 0).ToList();
            if (users.Count < 2)
                return;

            foreach (var u in users)
            {
                if (u.PosicaoSpec.HasValue)
                {
                    u.Player.Dimension = 0;
                    u.Player.SetSyncedMetaData("nametag", $"{u.Nome} [{u.ID}]");
                    u.Player.Spawn(u.PosicaoSpec.Value);
                    u.Player.Emit("UnspectatePlayer");
                    u.PosicaoSpec = null;
                }

                u.Player.RemoveAllWeapons();
                u.Player.SetSyncedMetaData("podeatirar", false);
                u.Player.Emit("setPlayerCanDoDriveBy", false);
                u.Player.Emit("toggleGameControls", false);
                u.Player.SetSyncedMetaData("congelar", true);
                u.Player.SetSyncedMetaData("tempo", string.Empty);
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"As perseguições serão preparadas em {{{Global.CorAmarelo}}}10 {{#FFFFFF}}segundos.");
            }

            System.Threading.Thread.Sleep(10000);

            var situacao = Global.Situacoes.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            users = Global.Usuarios.Where(x => x.Player.Dimension == 0).OrderBy(x => Guid.NewGuid()).ToList();

            var random = new Random();
            var perseguicao = new Perseguicao()
            {
                ID = Global.Perseguicoes.Select(x => x.ID).DefaultIfEmpty(0).Max() + 1,
                Weather = (WeatherType)new List<uint> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 12, 13 }.OrderBy(x => Guid.NewGuid()).FirstOrDefault(),
                Horario = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, random.Next(0, 23), random.Next(0, 59), 0),
            };

            foreach (var u in users)
            {
                var index = users.IndexOf(u);
                if (index % 9 == 0 && index != 0 && users.Count - (Global.Perseguicoes.Count + 1) * 9 >= 2)
                {
                    Global.Perseguicoes.Add(perseguicao);
                    perseguicao.IniciarTimer();
                    perseguicao = (Perseguicao)perseguicao.Clone();
                    perseguicao = new Perseguicao()
                    {
                        ID = Global.Perseguicoes.Select(x => x.ID).DefaultIfEmpty(0).Max() + 1,
                    };
                }

                u.ArenaDM = false;
                u.Player.Emit("setPlayerCanDoDriveBy", false);
                u.Player.SetSyncedMetaData("podeatirar", false);
                u.Player.Emit("toggleGameControls", false);
                u.Player.SetSyncedMetaData("congelar", true);
                u.Player.SetWeather(perseguicao.Weather);
                u.Player.SetDateTime(perseguicao.Horario);
                u.Player.SetSyncedMetaData("tempo", string.Empty);
                u.Player.Dimension = perseguicao.ID;
                u.Policial = false;
                var pedModel = (uint)u.Skin;
                var pos = new Situacao.Posicao();
                var veh = u.Helicoptero && !perseguicao.TemHelicoptero && perseguicao.IDFugitivo != 0 && users.Count - Global.Perseguicoes.Count * 9 >= 3 ? "POLMAV" : u.Veiculo;
                if (veh == "POLMAV")
                    perseguicao.TemHelicoptero = true;

                if (perseguicao.IDFugitivo == 0)
                {
                    pedModel = (uint)Global.SkinsFugitivo.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                    pos = new Situacao.Posicao(situacao.PosicaoFugitivo.Position, situacao.PosicaoFugitivo.Rotation);
                    veh = Global.VeiculosFugitivo.OrderBy(x => Guid.NewGuid()).FirstOrDefault().ToString();
                    perseguicao.IDFugitivo = u.ID;
                    u.GPS = true;
                }
                else
                {
                    u.Policial = true;
                    if (veh == "POLMAV")
                    {
                        if (situacao.PosicaoHelicoptero == TipoPosicaoHelicoptero.LosSantos)
                            pos = new Situacao.Posicao(new Position(449.27472f, -981.33624f, 44.073975f), new Rotation(0f, 0f, 1.609375f));
                        else if (situacao.PosicaoHelicoptero == TipoPosicaoHelicoptero.SandyShores)
                            pos = new Situacao.Posicao(new Rotation(1770.0264f, 3239.7231f, 42.506958f), new Rotation(0.015625f, -0.03125f, -1.328125f));
                        else if (situacao.PosicaoHelicoptero == TipoPosicaoHelicoptero.PaletoBay)
                            pos = new Situacao.Posicao(new Rotation(-475.22638f, 5988.6724f, 31.723022f), new Rotation(0f, 0f, -0.671875f));
                    }
                    else
                    {
                        pos = situacao.PosicoesPoliciais[users.IndexOf(u) - 1];
                    }
                }

                u.Player.Spawn(pos.Position);
                u.Player.Model = pedModel;
                u.Player.GiveWeapon(WeaponModel.Pistol, 2000, false);
                u.Player.SetWeaponTintIndex(WeaponModel.Pistol, u.Pintura);
                u.Player.GiveWeapon(WeaponModel.PumpShotgun, 2000, false);
                u.Player.SetWeaponTintIndex(WeaponModel.PumpShotgun, u.Pintura);
                u.Player.GiveWeapon(WeaponModel.SMG, 2000, false);
                u.Player.SetWeaponTintIndex(WeaponModel.SMG, u.Pintura);
                if (u.Policial)
                {
                    u.Player.AddWeaponComponent(WeaponModel.Pistol, 0x359B7AAE);
                    u.Player.GiveWeapon(WeaponModel.Flashlight, 1, false);
                    u.Player.GiveWeapon(WeaponModel.StunGun, 1, false);
                    u.Player.GiveWeapon(WeaponModel.CarbineRifle, 2000, false);
                    u.Player.SetWeaponTintIndex(WeaponModel.CarbineRifle, u.Pintura);
                }
                else
                {
                    u.Player.GiveWeapon(WeaponModel.AssaultRifle, 2000, false);
                    u.Player.SetWeaponTintIndex(WeaponModel.AssaultRifle, u.Pintura);
                }
                u.Player.Health = u.Player.MaxHealth;
                u.Player.Armor = 0;
                u.VeiculoPerseguicao = Alt.CreateVehicle(veh, pos.Position, pos.Rotation);
                u.VeiculoPerseguicao.SetWindowOpened(0, true);
                if (!u.Policial)
                {
                    u.VeiculoPerseguicao.PrimaryColorRgb = new Rgba((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255), 255);
                    u.VeiculoPerseguicao.SecondaryColorRgb = u.VeiculoPerseguicao.PrimaryColorRgb;
                }
                u.VeiculoPerseguicao.ManualEngineControl = true;
                u.VeiculoPerseguicao.Dimension = perseguicao.ID;
                u.VeiculoPerseguicao.NumberplateText = u.Policial ? "LSPD" : "CRIME";
                u.VeiculoPerseguicao.LockState = VehicleLockState.Unlocked;
                u.Player.Emit("setPedIntoVehicle", u.VeiculoPerseguicao, -1, true);
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"A perseguição iniciará em {{{Global.CorAmarelo}}}10 {{#FFFFFF}}segundos.");
            }

            if (users.Count % 9 != 0)
            {
                Global.Perseguicoes.Add(perseguicao);
                perseguicao.IniciarTimer();
            }
        }

        private void BWVerificarPerseguicoes_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!BWIniciarPerseguicoes.IsBusy && Global.Perseguicoes.Count == 0)
                BWIniciarPerseguicoes.RunWorkerAsync();
        }

        private void BWVerificarPerseguicoes_DoWork(object sender, DoWorkEventArgs e)
        {
            for (var i = 0; i < Global.Perseguicoes.Count; i++)
            {
                var p = Global.Perseguicoes[i];

                var players = Global.Usuarios.Where(x => x.Player.Dimension == p.ID && x.VeiculoPerseguicao != null).ToList();
                var minutosPerseguicao = (DateTime.Now - (p.Inicio ?? DateTime.Now)).Minutes;
                if (players.GroupBy(x => x.Policial).Count() <= 1 || minutosPerseguicao >= 7)
                {
                    var veiculos = Alt.GetAllVehicles().Where(x => x.Dimension == p.ID);
                    foreach (var v in veiculos)
                        Alt.RemoveVehicle(v);

                    var msg = "A perseguição acabou. ";
                    var bandido = players.FirstOrDefault(y => !y.Policial);
                    if (bandido != null)
                    {
                        bandido.Level += bandido.DataTerminoVIP.HasValue ? 10 : 5;
                        msg += $"O bandido {{{Global.CorAmarelo}}}{bandido.Nome}{{#FFFFFF}} conseguiu escapar.";
                    }
                    else
                    {
                        msg += $"Os {{{Global.CorAmarelo}}}policiais{{#FFFFFF}} capturaram o bandido.";
                    }

                    Global.Perseguicoes.Remove(p);

                    foreach (var x in players)
                    {
                        if (bandido == null && x.Policial)
                            x.Level += x.DataTerminoVIP.HasValue ? 2 : 1;

                        Functions.EnviarMensagem(x.Player, TipoMensagem.Nenhum, msg);
                        if (!x.Player.IsDead)
                            Functions.SpawnarPlayer(x.Player);
                    }
                }
                else
                {
                    foreach (var x in players)
                    {
                        if (x.Player.IsInVehicle && x.Player.Vehicle.EngineOn)
                        {
                            if (x.Player.Vehicle.BodyHealth < 650 || x.Player.Vehicle.EngineHealth < 650)
                            {
                                x.Player.Emit("vehicle:setVehicleEngineOn", x.Player.Vehicle, false);
                                Functions.EnviarMensagem(x.Player, TipoMensagem.Erro, $"Seu veículo sofreu muito dano e o motor desabilitou.");
                            }
                        }

                        var areaName = string.Empty;
                        var zoneName = string.Empty;
                        var tipoBlip = 225;
                        var corBlip = 38;
                        if (!x.Policial)
                        {
                            tipoBlip = 229;
                            corBlip = 75;
                            x.GPS = minutosPerseguicao % 2 == 0;

                            if (x.GPS)
                            {
                                var minutosUltimoAvisoPosicao = (DateTime.Now - (p.DataUltimoAvisoPosicao ?? DateTime.Now)).Minutes;
                                if (minutosUltimoAvisoPosicao >= 1)
                                {
                                    areaName = x.AreaName;
                                    zoneName = x.ZoneName;
                                    p.DataUltimoAvisoPosicao = DateTime.Now;
                                }
                            }
                        }

                        foreach (var u in players)
                        {
                            if (u.Policial && !string.IsNullOrWhiteSpace(areaName) && !string.IsNullOrWhiteSpace(zoneName))
                                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"[RÁDIO] Jake Peralta: O bandido foi visto em {areaName} - {zoneName}.", "#FFFF9B");

                            u.Player.Emit("blip:remove", x.ID);

                            if (!x.GPS)
                                continue;

                            u.Player.Emit("blip:create", x.ID, tipoBlip, x.Nome, corBlip, x.Player.Position);
                        }
                    }
                }
            }
        }

        private bool OnWeaponDamage(IPlayer player, IEntity target, uint weapon, ushort damage, Position shotOffset, BodyPart bodyPart)
        {
            if (!(target is IPlayer playerTarget) || player.Dimension == 0)
                return true;

            var p = Functions.ObterUsuario(player);
            if (!p.Policial)
                return true;

            var pTarget = Functions.ObterUsuario(playerTarget);
            if (pTarget.Policial)
                return false;

            return true;
        }

        private void OnPlayerDamage(IPlayer player, IEntity attacker, uint weapon, ushort damage)
        {
            if (!(attacker is IPlayer playerAttacker) || player.Dimension == 0 || playerAttacker == player)
                return;

            var pAttacker = Functions.ObterUsuario(playerAttacker);
            if (!pAttacker.Policial)
                Atirou(playerAttacker);
        }

        private void OnPlayerDead(IPlayer player, IEntity killer, uint weapon)
        {
            var p = Functions.ObterUsuario(player);
            if (player.Dimension != 0)
            {
                var players = Global.Usuarios.Where(x => x.Player.Dimension == player.Dimension);

                if (killer is IPlayer playerKiller && playerKiller != player)
                {
                    var pKiller = Functions.ObterUsuario(playerKiller);

                    playerKiller.GetSyncedMetaData("podeatirar", out bool podeAtirar);
                    if ((pKiller.Policial && p.Policial) || (pKiller.Policial && !podeAtirar))
                    {
                        pKiller.Level -= 10;
                        foreach (var u in players)
                            Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"{{{Global.CorAmarelo}}}{pKiller.Nome} {{#FFFFFF}}matou um policial ou ele matou o suspeito antes de atirar e por isso perdeu {{{Global.CorAmarelo}}}10 {{#FFFFFF}}níveis.");
                        Functions.SpawnarPlayer(playerKiller);
                    }
                    else
                    {
                        pKiller.Level += p.DataTerminoVIP.HasValue ? 2 : 1;

                        if (!pKiller.Policial && p.Policial)
                            Atirou(playerKiller);
                    }

                    foreach (var u in players)
                        Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"{{{Global.CorAmarelo}}}{pKiller.Nome} {{#FFFFFF}}matou {{{Global.CorAmarelo}}}{p.Nome}{{#FFFFFF}}.");
                }
                else
                {
                    foreach (var u in players)
                        Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"{{{Global.CorAmarelo}}}{p.Nome} {{#FFFFFF}}se matou.");
                }
            }
            else if (p.ArenaDM)
            {
                if (killer is IPlayer playerKiller && playerKiller != player)
                {
                    playerKiller.Health = playerKiller.MaxHealth;
                    playerKiller.Armor = playerKiller.MaxArmor;
                    var pKiller = Functions.ObterUsuario(playerKiller);
                    foreach (var u in Global.Usuarios.Where(x => x.ArenaDM))
                        u.Player.Emit("displayAdvancedNotification", $"~r~{pKiller.Nome} ~w~matou ~r~{p.Nome}", "Arena DM", string.Empty, "CHAR_LESTER_DEATHWISH", 0, null, 0.5);
                }
            }

            Functions.SpawnarPlayer(player);
        }

        private void OnPlayerDisconnect(IPlayer player, string reason)
        {
            var x = Functions.ObterUsuario(player);
            if (x?.Codigo > 0)
            {
                foreach (var u in Global.Usuarios)
                {
                    u.Player.Emit("blip:remove", x.ID);
                    Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"{x.Nome} {{{Global.CorErro}}}saiu{{#FFFFFF}} do servidor.");
                }

                Functions.SalvarUsuario(x);
                Global.Usuarios.RemoveAll(y => y.ID == x.ID);
            }
        }

        private void OnPlayerConnect(IPlayer player, string reason)
        {
            player.SetSyncedMetaData("tempo", string.Empty);
            player.SetDateTime(DateTime.Now);
            player.SetWeather(WeatherType.Clear);
            player.Spawn(new Position(0f, 0f, 0f));

            using var context = new DatabaseContext();

            if (!Functions.VerificarBanimento(player,
                context.Banimentos.FirstOrDefault(x => x.SocialClub == (long)player.SocialClubId
                && x.HardwareIdHash == (long)player.HardwareIdHash
                && x.HardwareIdExHash == (long)player.HardwareIdExHash)))
                return;

            player.Emit("Server:Login",
                context.Usuarios.FirstOrDefault(x => x.SocialClubRegistro == (long)player.SocialClubId
                && x.HardwareIdHashRegistro == (long)player.HardwareIdHash
                && x.HardwareIdExHashRegistro == (long)player.HardwareIdExHash)?.Nome ?? string.Empty);
        }

        private void OnPlayerChat(IPlayer player, string mensagem)
        {
            if (mensagem[0] != '/')
            {
                var u = Functions.ObterUsuario(player);
                foreach (var x in Global.Usuarios)
                    Functions.EnviarMensagem(x.Player, TipoMensagem.Nenhum, $"{(!string.IsNullOrWhiteSpace(u.Cor) ? $"{{{u.Cor}}}" : string.Empty)}{u.Nome}{{#FFFFFF}}: {mensagem}");
                return;
            }

            try
            {
                var split = mensagem.Split(" ");
                var cmd = split[0].Replace("/", string.Empty).Trim().ToLower();
                var method = Assembly.GetExecutingAssembly().GetTypes()
                    .SelectMany(x => x.GetMethods())
                    .Where(x => x.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0
                    && (x.GetCustomAttribute<CommandAttribute>().Command.ToLower() == cmd
                        || x.GetCustomAttribute<CommandAttribute>().Alias.ToLower() == cmd))
                    .FirstOrDefault();
                if (method == null)
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"O comando {{{Global.CorAmarelo}}}{mensagem}{{#FFFFFF}} não existe. Digite {{{Global.CorAmarelo}}}/ajuda{{#FFFFFF}} para visualizar os comandos disponíveis.");
                    return;
                }

                var methodParams = method.GetParameters();
                var obj = Activator.CreateInstance(method.DeclaringType);
                var command = method.GetCustomAttribute<CommandAttribute>();

                var arr = new List<object>();

                var list = methodParams.ToList();
                foreach (var x in list)
                {
                    var index = list.IndexOf(x);
                    if (index == 0)
                    {
                        arr.Add(player);
                    }
                    else
                    {
                        if (split.Length <= index)
                            continue;

                        var p = split[index];

                        if (x.ParameterType == typeof(int))
                        {
                            int.TryParse(p, out int val);
                            if (val == 0 && p != "0")
                                continue;

                            arr.Add(val);
                        }
                        else if (x.ParameterType == typeof(string))
                        {
                            if (string.IsNullOrWhiteSpace(p))
                                continue;

                            if (command.GreedyArg && index + 1 == list.Count)
                                p = string.Join(" ", split.Skip(index).Take(split.Length - index));

                            arr.Add(p);
                        }
                        else if (x.ParameterType == typeof(float))
                        {
                            float.TryParse(p, out float val);
                            if (val == 0 && p != "0")
                                continue;

                            arr.Add(val);
                        }
                    }
                }

                if (methodParams.Length != arr.Count)
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Os parâmetros do comando não foram informados corretamente. Use: {{{Global.CorAmarelo}}}{command.HelpText}");
                    return;
                }

                method.Invoke(obj, arr.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(JsonConvert.SerializeObject(ex));
                Functions.EnviarMensagem(player, TipoMensagem.Erro, "Não foi possível interpretar o comando.");
            }
        }

        public override void OnStop()
        {
            TimerSegundo?.Stop();
            TimerPrincipal?.Stop();
            BWVerificarPerseguicoes?.CancelAsync();
            BWIniciarPerseguicoes?.CancelAsync();

            foreach (var x in Global.Usuarios)
                Functions.SalvarUsuario(x);
        }

        private void TimerPrincipal_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var x in Global.Usuarios)
                Functions.SalvarUsuario(x);
        }

        private void TimerSegundo_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!BWVerificarPerseguicoes.IsBusy && !BWIniciarPerseguicoes.IsBusy)
                BWVerificarPerseguicoes.RunWorkerAsync();
        }

        private void EntrarUsuario(IPlayer player, string usuario, string senha)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
            {
                player.Emit("Server:MostrarErro", "Verifique se todos os campos foram preenchidos corretamente.");
                return;
            }

            var senhaCriptografada = Functions.Criptografar(senha);
            using var context = new DatabaseContext();
            var user = context.Usuarios.FirstOrDefault(x => x.Nome == usuario && x.Senha == senhaCriptografada);
            if (user == null)
            {
                player.Emit("Server:MostrarErro", "Usuário ou senha inválidos.");
                return;
            }

            if (!Functions.VerificarBanimento(player, context.Banimentos.FirstOrDefault(x => x.Usuario == user.Codigo)))
                return;

            if (Global.Usuarios.Any(x => x?.Nome == usuario))
            {
                player.Emit("Server:MostrarErro", "Usuário já está logado.");
                return;
            }

            user.Player = player;
            user.DataUltimoAcesso = DateTime.Now;
            user.IPUltimoAcesso = Functions.ObterIP(player);
            user.SocialClubUltimoAcesso = (long)player.SocialClubId;
            user.HardwareIdHashUltimoAcesso = (long)player.HardwareIdHash;
            user.HardwareIdExHashUltimoAcesso = (long)player.HardwareIdExHash;
            user.ID = Functions.ObterNovoID();
            context.Usuarios.Update(user);

            Global.Usuarios.Add(user);

            var temRecorde = false;
            if (Global.Usuarios.Count > Global.Parametros.RecordeOnline)
            {
                Global.Parametros.RecordeOnline = Global.Usuarios.Count;
                temRecorde = true;
            }

            context.Parametros.Update(Global.Parametros);

            context.SaveChanges();

            foreach (var u in Global.Usuarios)
            {
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"{user.Nome} {{{Global.CorSucesso}}}entrou{{#FFFFFF}} no servidor.");

                if (temRecorde)
                    Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"O novo recorde de jogadores online é: {{{Global.CorSucesso}}}{Global.Parametros.RecordeOnline}{{#FFFFFF}}.");
            }

            player.SetSyncedMetaData("nametag", $"{user.Nome} [{user.ID}]");
            player.Emit("nametags:Config", true);
            player.Emit("Server:ConfirmarLogin");
            player.Emit("chat:activateTimeStamp", user.TimeStamp);
            Functions.SpawnarPlayer(player);
            Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Que bom te ver por aqui! Digite {{{Global.CorAmarelo}}}/sobre{{#FFFFFF}} para entender como tudo funciona e {{{Global.CorAmarelo}}}/ajuda{{#FFFFFF}} para visualizar os comandos.");
            if (user.DataTerminoVIP.HasValue)
                Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Seu VIP expira em {{{Global.CorAmarelo}}}{user.DataTerminoVIP}{{#FFFFFF}}.");

            if (!BWIniciarPerseguicoes.IsBusy && Global.Perseguicoes.Count > 0)
                Functions.EnviarMensagem(player, TipoMensagem.Nenhum, "As perseguições estão em andamento! Confira o tempo restante no canto inferior direito da tela.");
        }

        private void RegistrarUsuario(IPlayer player, string usuario, string email, string senha, string senha2)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(senha2))
            {
                player.Emit("Server:MostrarErro", "Verifique se todos os campos foram preenchidos corretamente.");
                return;
            }

            if (usuario.Contains(" "))
            {
                player.Emit("Server:MostrarErro", "Usuário não pode ter espaços.");
                return;
            }

            if (usuario.Length > 25)
            {
                player.Emit("Server:MostrarErro", "Usuário não pode ter mais que 25 caracteres.");
                return;
            }

            if (email.Length > 100)
            {
                player.Emit("Server:MostrarErro", "E-mail não pode ter mais que 100 caracteres.");
                return;
            }

            if (senha != senha2)
            {
                player.Emit("Server:MostrarErro", "Senhas não são iguais.");
                return;
            }

            if (!Functions.ValidarEmail(email))
            {
                player.Emit("Server:MostrarErro", "E-mail não está um formato válido.");
                return;
            }

            using (var context = new DatabaseContext())
            {
                if (context.Usuarios.Any(x => x.Nome == usuario))
                {
                    player.Emit("Server:MostrarErro", $"Usuário {usuario} já existe.");
                    return;
                }

                if (context.Usuarios.Any(x => x.Email == email))
                {
                    player.Emit("Server:MostrarErro", $"Email {email} está sendo utilizado.");
                    return;
                }

                var ip = Functions.ObterIP(player);
                var user = new Usuario()
                {
                    Nome = usuario,
                    Email = email,
                    Senha = Functions.Criptografar(senha),
                    SocialClubRegistro = (long)player.SocialClubId,
                    SocialClubUltimoAcesso = (long)player.SocialClubId,
                    IPRegistro = ip,
                    IPUltimoAcesso = ip,
                    HardwareIdHashRegistro = (long)player.HardwareIdHash,
                    HardwareIdExHashRegistro = (long)player.HardwareIdExHash,
                    HardwareIdHashUltimoAcesso = (long)player.HardwareIdHash,
                    HardwareIdExHashUltimoAcesso = (long)player.HardwareIdExHash,
                };
                context.Usuarios.Add(user);
                context.SaveChanges();
            }

            EntrarUsuario(player, usuario, senha);
        }

        private void ListarPlayers(IPlayer player)
        {
            var p = Functions.ObterUsuario(player);
            if (p == null)
                return;

            var personagens = Global.Usuarios.OrderBy(x => x.ID == p.ID ? 0 : 1).ThenBy(x => x.ID)
                .Select(x => new
                {
                    x.ID,
                    x.Nome,
                    x.Level,
                    Tipo = x.DataTerminoVIP.HasValue && x.Staff == TipoStaff.Jogador ? "VIP" : x.Staff.ToString(),
                    x.Player.Ping,
                    x.Cor
                }).ToList();
            player.Emit("Server:ListarPlayers", Global.NomeServidor, JsonConvert.SerializeObject(personagens), Global.Usuarios.Count(x => x.Staff != TipoStaff.Jogador));
        }

        private void TrancarDestrancarVeiculo(IPlayer player)
        {
            if (player.Dimension == 0)
                return;

            var p = Functions.ObterUsuario(player);
            if (!p.Policial || p.VeiculoPerseguicao == null)
                return;

            if ((player.IsInVehicle && player.Vehicle == p.VeiculoPerseguicao)
                || (!player.IsInVehicle && p.VeiculoPerseguicao.Position.Distance(player.Position) <= 3))
            {
                if (p.VeiculoPerseguicao.LockState == VehicleLockState.Locked)
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Sucesso, "Você destrancou o veículo.", notify: true);
                    p.VeiculoPerseguicao.LockState = VehicleLockState.Unlocked;
                }
                else
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Sucesso, "Você trancou o veículo.", notify: true);
                    p.VeiculoPerseguicao.LockState = VehicleLockState.Locked;
                }
            }
        }

        private void Algemar(IPlayer player)
        {
            if (player.Dimension == 0 || player.IsInVehicle)
                return;

            var p = Functions.ObterUsuario(player);
            if (!p.Policial || p.VeiculoPerseguicao == null)
                return;

            var usuarios = Global.Usuarios.Where(x => x.Player.Dimension == player.Dimension).ToList();
            var fugitivo = usuarios.FirstOrDefault(x => !x.Policial);
            if (fugitivo == null)
                return;

            fugitivo.Player.GetSyncedMetaData("atirou", out bool atirou);
            if (atirou || fugitivo.Player.IsInVehicle)
                return;

            if (player.Position.Distance(fugitivo.Player.Position) > 3)
                return;

            p.Level += p.DataTerminoVIP.HasValue ? 2 : 1;
            foreach (var u in usuarios)
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"{{{Global.CorAmarelo}}}{p.Nome}{{#FFFFFF}} algemou e capturou {{{Global.CorAmarelo}}}{fugitivo.Nome}{{#FFFFFF}}.");
            Functions.SpawnarPlayer(fugitivo.Player);
        }

        private void AtivarDesativarGPS(IPlayer player)
        {
            if (player.Dimension == 0)
                return;

            var p = Functions.ObterUsuario(player);
            if (!p.Policial || p.VeiculoPerseguicao == null)
                return;

            Functions.EnviarMensagem(player, TipoMensagem.Sucesso, $"Você {(p.GPS ? "des" : string.Empty)}ativou o GPS.", notify: true);
            p.GPS = !p.GPS;
        }

        private void Atirou(IPlayer player)
        {
            if (player.Dimension == 0)
                return;

            var p = Functions.ObterUsuario(player);
            if (p.Policial || p.VeiculoPerseguicao == null)
                return;

            player.GetSyncedMetaData("atirou", out bool atirou);
            if (atirou)
                return;

            Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Você {{{Global.CorAmarelo}}}atirou/atacou {{#FFFFFF}}e os disparos estão liberados.");
            player.SetSyncedMetaData("atirou", true);
            foreach (var u in Global.Usuarios.Where(x => x.Player.Dimension == player.Dimension && x.Policial))
            {
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"[RÁDIO] Jake Peralta: O bandido {p.Nome} atirou/atacou e os disparos estão liberados.", "#FFFF9B");
                u.Player.SetSyncedMetaData("podeatirar", true);
                u.Player.Emit("setPlayerCanDoDriveBy", true);
            }
        }

        private void SelecionarVeiculo(IPlayer player, string veiculo, string model)
        {
            veiculo = veiculo.Replace("~q~[VIP] ~s~", string.Empty);

            var p = Functions.ObterUsuario(player);
            if (model == "POLICE3" || model == "POLICESLICK" || model == "POLICEOLD" || model == "PSCOUT" || model == "POLRIOT"
                || model == "BEACHP" || model == "POLMERIT2" || model == "POLICE42" || model == "POLSPEEDO" || model == "LSPDB"
                || model == "PULICE" || model == "PULICE2" || model == "PULICE3" || model == "PULICE4")
            {
                if (!p.DataTerminoVIP.HasValue)
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Erro, $"O veículo {veiculo} ({model}) é apenas para VIPs. Para saber como se tornar um, leia o canal #como-doar em nosso Discord.");
                    return;
                }
            }

            p.Veiculo = model;
            Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Você selecionou o veículo {{{Global.CorAmarelo}}}{veiculo} ({model}){{#FFFFFF}} e será usado na próxima perseguição.");
        }

        private void SelecionarSkin(IPlayer player, string skin, string model)
        {
            var p = Functions.ObterUsuario(player);
            p.Skin = (long)Enum.GetValues(typeof(PedModel)).Cast<PedModel>().FirstOrDefault(x => x.ToString().ToLower() == model.ToLower());
            Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Você selecionou a skin {{{Global.CorAmarelo}}}{skin} ({model}){{#FFFFFF}} e será usada na próxima perseguição.");
        }

        private void AtualizarInformacoes(IPlayer player, string areaName, string zoneName)
        {
            var p = Functions.ObterUsuario(player);
            p.AreaName = areaName;
            p.ZoneName = zoneName;
        }

        private void SelecionarPinturaArmas(IPlayer player, string descricao)
        {
            byte.TryParse(descricao.Split('-')[0].Trim(), out byte pintura);
            var p = Functions.ObterUsuario(player);
            p.Pintura = pintura;
            Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Você selecionou a pintura {{{Global.CorAmarelo}}}{descricao}{{#FFFFFF}} e será usada na próxima perseguição.");
        }
    }
}