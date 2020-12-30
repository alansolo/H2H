using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServicioH2HSantander
{

    static class Program
    {
        public static bool isDev = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            if (!isDev)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new ServicioH2HSantander()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                ServicioH2HSantander sq = new ServicioH2HSantander();
                sq.IniciaProcesoConsola();

            }
        }
    }
}
