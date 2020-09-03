using AltV.Net;
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

        public object Clone() => MemberwiseClone();

        public void IniciarTimer()
        {
            TimerInicio.Start();
        }

        private void TimerInicio_Elapsed(object sender, ElapsedEventArgs e)
        {
            Inicio = DataUltimoAvisoPosicao = DateTime.Now;

            foreach (var u in Global.Usuarios.Where(x => x.Player.Dimension == ID))
            {
                u.Player.SetSyncedMetaData("tempo", Inicio?.ToString("yyyy-MM-dd HH:mm:ss"));
                u.Player.SetSyncedMetaData("congelar", false);

                if (!u.Policial)
                {
                    u.Player.SetSyncedMetaData("podeatirar", true);
                    u.Player.Emit("setPlayerCanDoDriveBy", true);
                }

                foreach (var v in Alt.GetAllVehicles().Where(x => x.Dimension == ID))
                {
                    u.Player.Emit("vehicle:setVehicleEngineOn", v, true);
                    u.Player.Emit("toggleGameControls", true, v);
                }

                Functions.EnviarMensagem(u.Player, TipoMensagem.Sucesso, "A perseguição iniciou!");
            }

            TimerInicio?.Stop();
        }
    }
}