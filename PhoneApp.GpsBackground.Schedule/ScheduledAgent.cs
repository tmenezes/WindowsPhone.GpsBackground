using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Scheduler;
using PhoneApp.GpsBackground.Util;

namespace PhoneApp.GpsBackground.Schedule
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        private IGeoPositionWatcher<GeoCoordinate> gpsWatcher;
        private static readonly List<GeoCoordinate> _pontosMapeados = new List<GeoCoordinate>();
        private static readonly PontosIsoStorage _pontosIsoStorage = new PontosIsoStorage();



        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }



        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            gpsWatcher = GetGpsWatcher();
            gpsWatcher.PositionChanged += gpsWatcher_PositionChanged;

            DateTime dataInicioLeitura = DateTime.Now;
            bool aguardarLeitura = true;
            while (aguardarLeitura)
            {
                Thread.Sleep(1000);
                aguardarLeitura = DateTime.Now < (dataInicioLeitura.AddSeconds(10)); // aguardar 10 segundos
            }


            gpsWatcher.PositionChanged -= gpsWatcher_PositionChanged;
            SalvarPontosMapeados(); // implementar aqui método para salvar pontos no IsolatedStorageFile
            NotifyComplete();
        }


        private void SalvarPontosMapeados()
        {
            _pontosIsoStorage.Write(_pontosMapeados);
            //var pontos = _pontosIsoStorage.Read();
        }

        private IGeoPositionWatcher<GeoCoordinate> GetGpsWatcher()
        {
            var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High) // definir aqui a precisao
                {
                    //MovementThreshold = 1 // leitura de 1 em 1 metro (definir aqui intervalo de metros para a leitura)
                };
            watcher.Start();
            return watcher;
        }

        void gpsWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var posicao = e.Position.Location;
            if (posicao != GeoCoordinate.Unknown)
            {
                _pontosMapeados.Add(posicao);
            }
        }
    }
}