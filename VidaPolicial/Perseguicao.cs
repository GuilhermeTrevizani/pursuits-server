using AltV.Net;
using AltV.Net.Enums;
using System;
using System.Linq;
using System.Timers;

namespace VidaPolicial
{
    public class Perseguicao : ICloneable
    {
        public Perseguicao()
        {
            TimerInicio = new Timer(10000);
            TimerInicio.Elapsed += TimerInicio_Elapsed;
        }

        public int ID { get; set; }
        public DateTime? Inicio { get; set; } = null;
        public int IDFugitivo { get; set; }
        public Timer TimerInicio { get; set; }
        public DateTime? DataUltimoAvisoPosicao { get; set; } = null;
        public WeatherType Weather { get; set; }
        public DateTime Horario { get; set; }
        public bool TemHelicoptero { get; set; } = false;

        public object Clone() => MemberwiseClone();

        public void IniciarTimer()
        {
            TimerInicio.Start();
        }

        private void TimerInicio_Elapsed(object sender, ElapsedEventArgs e)
        {
            Inicio = DataUltimoAvisoPosicao = DateTime.Now;
            var users = Global.Usuarios.Where(x => x.Player.Dimension == ID);
            var fugitivo = users.FirstOrDefault(x => !x.Policial)?.Nome;
            var vehicles = Alt.GetAllVehicles().Where(x => x.Dimension == ID);

            foreach (var u in users)
            {
                u.Player.SetSyncedMetaData("tempo", Inicio?.ToString("yyyy-MM-dd HH:mm:ss"));
                u.Player.SetSyncedMetaData("congelar", false);

                if (!u.Policial)
                {
                    u.Player.SetSyncedMetaData("podeatirar", true);
                    u.Player.Emit("setPlayerCanDoDriveBy", true);
                }

                foreach (var v in vehicles)
                {
                    u.Player.Emit("vehicle:setVehicleEngineOn", v, true);
                    u.Player.Emit("toggleGameControls", true, v);
                }

                Functions.EnviarMensagem(u.Player, TipoMensagem.Nenhum, $"A perseguição iniciou. O bandido é: {{{Global.CorAmarelo}}}{fugitivo}{{#FFFFFF}}.");
            }

            TimerInicio?.Stop();
        }
    }
}