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
            Alt.OnClient<IPlayer, bool>("PlayerDigitando", PlayerDigitando);
            Alt.OnClient<IPlayer>("Algemar", Algemar);
            Alt.OnClient<IPlayer>("AtivarDesativarGPS", AtivarDesativarGPS);
            Alt.OnClient<IPlayer>("Atirou", Atirou);
            Alt.OnClient<IPlayer, string>("SelecionarVeiculo", SelecionarVeiculo);
            Alt.OnClient<IPlayer, string>("SelecionarSkin", SelecionarSkin);
            Alt.OnClient<IPlayer, string, string>("AtualizarInformacoes", AtualizarInformacoes);

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture =
                  CultureInfo.GetCultureInfo("pt-BR");

            var config = JsonConvert.DeserializeObject<Configuracao>(File.ReadAllText("settingsvidapolicial.json"));
            Global.NomeServidor = "GTA V Pursuits";
            Global.Weather = WeatherType.Clear;
            Global.MaxPlayers = config.MaxPlayers;
            Global.ConnectionString = $"Server={config.DBHost};Database={config.DBName};Uid={config.DBUser};Password={config.DBPassword}";
            Global.Usuarios = new List<Usuario>();
            Global.Perseguicoes = new List<Perseguicao>();
            Functions.CarregarSituacoes();

            using var context = new DatabaseContext();
            Global.Parametros = context.Parametros.FirstOrDefault();

            Console.WriteLine($"{Global.NomeServidor} por Guilherme Trevizani (TR3V1Z4)");

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
                u.Player.RemoveAllWeapons();
                u.Player.SetSyncedMetaData("podeatirar", false);
                u.Player.Emit("setPlayerCanDoDriveBy", false);
                u.Player.Emit("toggleGameControls", false);
                u.Player.SetSyncedMetaData("congelar", true);
                u.Player.SetSyncedMetaData("tempo", string.Empty);
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"As perseguições serão preparadas em {{{Global.CorAmarelo}}}10 segundos{{#FFFFFF}}.");
            }

            System.Threading.Thread.Sleep(10000);

            users = Global.Usuarios.Where(x => x.Player.Dimension == 0).OrderBy(x => Guid.NewGuid()).ToList();

            var situacao = Global.Situacoes.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            var perseguicao = new Perseguicao()
            {
                ID = Global.Perseguicoes.Select(x => x.ID).DefaultIfEmpty(0).Max() + 1,
            };
            foreach (var u in users)
            {
                u.Player.Emit("toggleGameControls", false);
                u.Player.SetSyncedMetaData("congelar", true);

                var index = users.IndexOf(u);
                if (index % 10 == 0 && index != 0) 
                {
                    Global.Perseguicoes.Add(perseguicao);
                    perseguicao.IniciarTimer();
                    perseguicao = (Perseguicao)perseguicao.Clone();
                    perseguicao = new Perseguicao()
                    {
                        ID = Global.Perseguicoes.Select(x => x.ID).DefaultIfEmpty(0).Max() + 1,
                    };
                }

                u.Player.SetSyncedMetaData("tempo", string.Empty);
                u.Player.Dimension = perseguicao.ID;
                u.Policial = false;
                var pedModel = (uint)u.Skin;
                var pos = new Situacao.Posicao();
                var veh = u.Veiculo;

                if (perseguicao.IDFugitivo == 0)
                {
                    pedModel = (uint)PedModel.Claypain;
                    pos = new Situacao.Posicao(situacao.PosicaoFugitivo.Position, situacao.PosicaoFugitivo.Rotation);
                    veh = situacao.VeiculoFugitivo.ToString();
                    perseguicao.IDFugitivo = u.ID;
                    u.GPS = true;
                }
                else
                {
                    u.Policial = true;
                    pos = situacao.PosicoesPoliciais[users.IndexOf(u) - 1];
                }

                u.Player.Model = pedModel;
                u.Player.Position = pos.Position;
                u.Player.GiveWeapon(WeaponModel.Pistol, 2000, false);
                u.Player.GiveWeapon(WeaponModel.PumpShotgun, 2000, false);
                u.Player.GiveWeapon(WeaponModel.SMG, 2000, false);
                if (u.Policial)
                {
                    u.Player.GiveWeapon(WeaponModel.Flashlight, 1, false);
                    u.Player.GiveWeapon(WeaponModel.StunGun, 1, false);
                    u.Player.GiveWeapon(WeaponModel.CarbineRifle, 2000, false);
                }
                else
                {
                    u.Player.GiveWeapon(WeaponModel.AssaultRifle, 2000, false);
                }
                u.Player.Health = u.Player.MaxHealth;
                u.Player.Armor = 0;
                u.VeiculoPerseguicao = Alt.CreateVehicle(veh, pos.Position, pos.Rotation);
                u.VeiculoPerseguicao.ManualEngineControl = true;
                u.VeiculoPerseguicao.Dimension = perseguicao.ID;
                u.VeiculoPerseguicao.NumberplateText = u.Policial ? "LSPD" : "CRIME";
                u.VeiculoPerseguicao.LockState = VehicleLockState.Unlocked;
                u.Player.Emit("setPedIntoVehicle", u.VeiculoPerseguicao, -1, true);
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"A perseguição iniciará em {{{Global.CorAmarelo}}}10 segundos{{#FFFFFF}}.");
            }

            if (users.Count % 10 != 0)
            {
                Global.Perseguicoes.Add(perseguicao);
                perseguicao.IniciarTimer();
            }
        }

        private void BWVerificarPerseguicoes_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Global.Perseguicoes.Count > 0)
                return;

            if (!BWIniciarPerseguicoes.IsBusy)
                BWIniciarPerseguicoes.RunWorkerAsync();
        }

        private void BWVerificarPerseguicoes_DoWork(object sender, DoWorkEventArgs e)
        {
            for (var i = 0; i < Global.Perseguicoes.Count; i++)
            {
                var p = Global.Perseguicoes[i];

                var players = Global.Usuarios.Where(x => x.Player.Dimension == p.ID).ToList();
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
                        bandido.Level += 5;
                        msg += $"O bandido {bandido.Nome} conseguiu escapar!";
                    }
                    else
                    {
                        msg += "Os policiais capturaram o bandido!";
                    }

                    Global.Perseguicoes.Remove(p);

                    foreach (var x in players)
                    {
                        if (bandido == null && x.Policial)
                            x.Level++;

                        Functions.EnviarMensagem(x.Player, TipoMensagem.Sucesso, msg);
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
            if (player.Dimension != 0)
            {
                var p = Functions.ObterUsuario(player);
                var players = Global.Usuarios.Where(x => x.Player.Dimension == player.Dimension);

                if (killer is IPlayer playerKiller && playerKiller != player)
                {
                    var pKiller = Functions.ObterUsuario(playerKiller);

                    playerKiller.GetSyncedMetaData("podeatirar", out bool podeAtirar);
                    if ((pKiller.Policial && p.Policial) || (pKiller.Policial && !podeAtirar))
                    {
                        pKiller.Level -= 10;
                        foreach (var u in players)
                            Functions.EnviarMensagem(u.Player, TipoMensagem.Erro, $"{pKiller.Nome} matou um policial ou ele matou o suspeito antes de atirar.");
                        Functions.SalvarUsuario(pKiller);
                        playerKiller.Kick("Você matou um policial ou matou o suspeito antes de atirar e por isso perdeu 10 níveis.");
                    }
                    else
                    {
                        pKiller.Level++;

                        if (!pKiller.Policial && p.Policial)
                            Atirou(playerKiller);
                    }

                    foreach (var u in players)
                        Functions.EnviarMensagem(u.Player, TipoMensagem.Erro, $"{pKiller.Nome} matou {p.Nome}.");
                }
                else
                {
                    foreach (var u in players)
                        Functions.EnviarMensagem(u.Player, TipoMensagem.Erro, $"{p.Nome} se matou.");
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
            player.SetWeather(Global.Weather);
            player.Spawn(new Position(0f, 0f, -5f));

            using var context = new DatabaseContext();

            if (!Functions.VerificarBanimento(player,
                context.Banimentos.FirstOrDefault(x => (x.SocialClub == (long)player.SocialClubId && x.SocialClub != 0)
                || x.HardwareIdHash == (long)player.HardwareIdHash
                || x.HardwareIdExHash == (long)player.HardwareIdExHash)))
                return;

            player.Emit("Server:Login",
                context.Usuarios.FirstOrDefault(x => (x.SocialClubRegistro == (long)player.SocialClubId && x.SocialClubRegistro != 0)
                || x.HardwareIdHashRegistro == (long)player.HardwareIdHash
                || x.HardwareIdExHashRegistro == (long)player.HardwareIdExHash)?.Nome ?? string.Empty);
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
                    Functions.EnviarMensagem(player, TipoMensagem.Erro, $"O comando {mensagem} não existe. Digite /ajuda para visualizar os comandos disponíveis.");
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
                    Functions.EnviarMensagem(player, TipoMensagem.Erro, $"Os parâmetros do comando não foram informados corretamente. Use: {command.HelpText}");
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
            if (!BWVerificarPerseguicoes.IsBusy)
                BWVerificarPerseguicoes.RunWorkerAsync();
        }

        private void EntrarUsuario(IPlayer player, string usuario, string senha)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
            {
                player.Emit("Server:MostrarErro", "Verifique se todos os campos foram preenchidos corretamente!");
                return;
            }

            var senhaCriptografada = Functions.Criptografar(senha);
            using var context = new DatabaseContext();
            var user = context.Usuarios.FirstOrDefault(x => x.Nome == usuario && x.Senha == senhaCriptografada);
            if (user == null)
            {
                player.Emit("Server:MostrarErro", "Usuário ou senha inválidos!");
                return;
            }

            if (!Functions.VerificarBanimento(player, context.Banimentos.FirstOrDefault(x => x.Usuario == user.Codigo)))
                return;

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
                    Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"O novo recorde de jogadores online é: {{{Global.CorSucesso}}}{Global.Parametros.RecordeOnline}");
            }

            player.SetSyncedMetaData("nametag", $"{user.Nome} [{user.ID}]");
            player.Emit("nametags:Config", true);
            player.Emit("Server:ConfirmarLogin");
            player.Emit("chat:activateTimeStamp", user.TimeStamp);
            Functions.SpawnarPlayer(player, BWIniciarPerseguicoes.IsBusy);
            Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Que bom te ver por aqui! Digite {{{Global.CorAmarelo}}}/sobre{{#FFFFFF}} para entender como tudo funciona e {{{Global.CorAmarelo}}}/ajuda{{#FFFFFF}} para visualizar os comandos.");

            if (!BWIniciarPerseguicoes.IsBusy && Global.Perseguicoes.Count > 0)
                Functions.EnviarMensagem(player, TipoMensagem.Nenhum, "As perseguições estão em andamento! Confira o tempo restante no canto inferior direito da tela.");
        }

        private void RegistrarUsuario(IPlayer player, string usuario, string email, string senha, string senha2)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(senha2))
            {
                player.Emit("Server:MostrarErro", "Verifique se todos os campos foram preenchidos corretamente!");
                return;
            }

            if (usuario.Contains(" "))
            {
                player.Emit("Server:MostrarErro", "Usuário não pode ter espaços!");
                return;
            }

            if (usuario.Length > 25)
            {
                player.Emit("Server:MostrarErro", "Usuário não pode ter mais que 25 caracteres!");
                return;
            }

            if (email.Length > 100)
            {
                player.Emit("Server:MostrarErro", "Email não pode ter mais que 100 caracteres!");
                return;
            }

            if (senha != senha2)
            {
                player.Emit("Server:MostrarErro", "Senhas não são iguais!");
                return;
            }

            using (var context = new DatabaseContext())
            {
                if (context.Usuarios.Any(x => x.Nome == usuario))
                {
                    player.Emit("Server:MostrarErro", $"Usuário {usuario} já existe!");
                    return;
                }

                if (context.Usuarios.Any(x => x.Email == email))
                {
                    player.Emit("Server:MostrarErro", $"Email {email} já está sendo utilizado!");
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
                .Select(x => new { x.ID, x.Nome, x.Level, Tipo = x.Staff.ToString(), x.Player.Ping, x.Cor }).ToList();
            player.Emit("Server:ListarPlayers", Global.NomeServidor, JsonConvert.SerializeObject(personagens), Global.Usuarios.Count(x => x.Staff != TipoStaff.Jogador));
        }

        private void TrancarDestrancarVeiculo(IPlayer player)
        {
            if (player.Dimension == 0)
                return;

            var p = Functions.ObterUsuario(player);
            if (!p.Policial)
                return;

            if ((player.IsInVehicle && player.Vehicle == p.VeiculoPerseguicao)
                || (!player.IsInVehicle && p.VeiculoPerseguicao.Position.Distance(player.Position) <= 3))
            {
                if (p.VeiculoPerseguicao.LockState == VehicleLockState.Locked)
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Sucesso, "Você destrancou o veículo!", notify: true);
                    p.VeiculoPerseguicao.LockState = VehicleLockState.Unlocked;
                }
                else
                {
                    Functions.EnviarMensagem(player, TipoMensagem.Sucesso, "Você trancou o veículo!", notify: true);
                    p.VeiculoPerseguicao.LockState = VehicleLockState.Locked;
                }
            }
        }

        private void PlayerDigitando(IPlayer player, bool state) => player.SetSyncedMetaData("chatting", state);

        private void Algemar(IPlayer player)
        {
            if (player.Dimension == 0 || player.IsInVehicle)
                return;

            var p = Functions.ObterUsuario(player);
            if (!p.Policial)
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

            p.Level++;
            foreach (var u in usuarios)
                Functions.EnviarMensagem(u.Player, TipoMensagem.Sucesso, $"{p.Nome} algemou e capturou {fugitivo.Nome}.");
            Functions.SpawnarPlayer(fugitivo.Player);
        }

        private void AtivarDesativarGPS(IPlayer player)
        {
            if (player.Dimension == 0)
                return;

            var p = Functions.ObterUsuario(player);
            if (!p.Policial)
                return;

            Functions.EnviarMensagem(player, TipoMensagem.Sucesso, $"Você {(p.GPS ? "des" : string.Empty)}ativou o GPS!", notify: true);
            p.GPS = !p.GPS;
        }

        private void Atirou(IPlayer player)
        {
            if (player.Dimension == 0)
                return;

            var p = Functions.ObterUsuario(player);
            if (p.Policial)
                return;

            player.GetSyncedMetaData("atirou", out bool atirou);
            if (atirou)
                return;

            Functions.EnviarMensagem(player, TipoMensagem.Erro, $"Você atirou/atacou e os disparos estão liberados.");
            player.SetSyncedMetaData("atirou", true);
            foreach (var u in Global.Usuarios.Where(x => x.Player.Dimension == player.Dimension && x.Policial))
            {
                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"[RÁDIO] Jake Peralta: O bandido {p.Nome} atirou/atacou e os disparos estão liberados.", "#FFFF9B");
                u.Player.SetSyncedMetaData("podeatirar", true);
                u.Player.Emit("setPlayerCanDoDriveBy", true);
            }
        }

        private void SelecionarVeiculo(IPlayer player, string veiculo)
        {
            var p = Functions.ObterUsuario(player);
            p.Veiculo = veiculo;
            Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Você selecionou o veículo {{{Global.CorAmarelo}}}{veiculo}{{#FFFFFF}} e será usado na próxima perseguição!");
        }

        private void SelecionarSkin(IPlayer player, string skin)
        {
            var p = Functions.ObterUsuario(player);
            p.Skin = (long)Enum.GetValues(typeof(PedModel)).Cast<PedModel>().FirstOrDefault(x => x.ToString().ToLower() == skin.ToLower());
            Functions.EnviarMensagem(player, TipoMensagem.Nenhum, $"Você selecionou a skin {{{Global.CorAmarelo}}}{skin}{{#FFFFFF}} e será usada na próxima perseguição!");
        }

        private void AtualizarInformacoes(IPlayer player, string areaName, string zoneName)
        {
            var p = Functions.ObterUsuario(player);
            p.AreaName = areaName;
            p.ZoneName = zoneName;
        }
    }
}