using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ServicioH2HSantander
{
    public partial class ServicioH2HSantander : ServiceBase
    {
        private int eventId = 0;
        public static System.Timers.Timer timer = new System.Timers.Timer();
        private ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ServicioH2HSantander()
        {
            try
            {
                InitializeComponent();

                if (GeneraArchivoLog())
                {
                    log.Info("se inicio  GeneraArchivoLog correctamente");
                }
                else
                {
                    log.Error("Ocurrio un error GeneraArchivoLog");

                }

            }
            catch (Exception ex)
            {
                log.Error("Ocurrio un error en el servicio servicioRecepcionFE, " + ex.Message);
            }
        }

        private bool GeneraArchivoLog()
        {
            string strNombreArchivoConfigLog;
            FileInfo fi;
            bool existeConfiguracion = false;

            try
            {
                strNombreArchivoConfigLog = ConfigurationManager.AppSettings["rutaLog4Net"];

                if (strNombreArchivoConfigLog == string.Empty)
                {
                    throw new Exception("No se encontro la variable AppSettings 'rutaLog4Net'");
                }
                else
                {
                    string startupPath = Application.StartupPath.Replace("\\bin\\Debug", "");
                    strNombreArchivoConfigLog = startupPath + "\\" + strNombreArchivoConfigLog;

                    fi = new FileInfo(strNombreArchivoConfigLog);

                    if (fi != null && fi.Exists == true)
                    {
                        log4net.Config.XmlConfigurator.Configure(fi);
                        existeConfiguracion = true;
                    }
                    log.Info("-Ruta del archivo log4Net = " + strNombreArchivoConfigLog);

                }
                return existeConfiguracion;
            }
            catch (Exception ex)
            {
                registraEvento("Ocurrio un error en la consulta de la configuración del archivo log4Net, " + ex.Message, (int)EventLogEntryType.Error);
                return existeConfiguracion;
            }
        }

        private void registraEvento(string mensaje, int evento)
        {
            switch (evento)
            {
                case (int)EventLogEntryType.Information:
                    EventLog.WriteEntry(mensaje, EventLogEntryType.Information, eventId++);
                    break;
                case (int)EventLogEntryType.Error:
                    EventLog.WriteEntry(mensaje, EventLogEntryType.Error, eventId++);
                    break;
                case (int)EventLogEntryType.Warning:
                    EventLog.WriteEntry(mensaje, EventLogEntryType.Warning, eventId++);
                    break;
                default:
                    EventLog.WriteEntry(mensaje, EventLogEntryType.Warning, eventId++);
                    break;
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                log.Info("Servicio Iniciado OnStart");
                InicializaTemporizador();

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Warning, 1);

                log.Error("Ocurrio un error al iniciar el servicio RecepcionFE, " + ex.Message);
            }

        }

        private void InicializaTemporizador()
        {
            Int32 ms = Convert.ToInt32(ConfigurationManager.AppSettings["timeEjecuta"]);
            timer.Interval = ms;
            log.Info("Tiempo temporizador " + ms.ToString() + "");
            timer.Elapsed += new ElapsedEventHandler(this.IniciaProceso);
            timer.Enabled = true;

        }

        private void IniciaProceso(object sender, System.Timers.ElapsedEventArgs args)
        {
            DateTime horaInicioEjecucion = new DateTime();
            DateTime horaFinEjecucion = new DateTime();

            try
            {
                horaInicioEjecucion = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd ") + ConfigurationManager.AppSettings["horaInicioEjecucion"]);
                horaFinEjecucion = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd ") + ConfigurationManager.AppSettings["horaFinEjecucion"]);
            }
            catch(Exception ex)
            {
                log.Error("Erro obtener hora de inicio y fin de ejecucion " + ex.Message);
            }

            if ((horaInicioEjecucion.ToString("yyyy/MM/dd HH24:mm") == "0001/01/01 00:00" && horaFinEjecucion.ToString("yyyy/MM/dd HH24:mm") == "0001/01/01 00:00")
                || (DateTime.Now >= horaInicioEjecucion && DateTime.Now <= horaFinEjecucion))
            {
                timer.Stop();
                log.Info("Inicio IniciaProceso");
                try
                {
                    if (ConfiguracionAplicacion())
                    {
                        IniciaProcesoSantander();
                    }
                }
                catch (Exception ex)
                {

                    log.Error("Error IniciaProceso " + ex.Message);
                }
                finally
                {
                    log.Info("Fin IniciaProceso");
                    timer.Close();
                    timer.Start();

                }
            }
        }

        public void IniciaProcesoConsola()
        {
            GeneraArchivoLog();
            IniciaProceso(null,null);

        }

        private void IniciaProcesoSantander()
        {
            log.Info("Inicia Proceso General Santander...");

            DecryptGnuPFile();
            EncryptFileH2H();
            EnvioSFTP();

            log.Info("Fin Proceso General Santander...");
        }

        private void EnvioSFTP()
        {
            log.Info("Inicia Proceso Envio SFTP...");

            string DirEncryptH2H = ConfigurationManager.AppSettings["DirEncryptH2H"];
            string dirH2HSend = ConfigurationManager.AppSettings["DirEncryptH2HSend"];
            string MensajeSftpEnvio = ConfigurationManager.AppSettings["MensajeSftpEnvio"];
            string[] fileEntries = Directory.GetFiles(DirEncryptH2H);
            log.Info("Orchivos EncryptH2H obtenidos:" + fileEntries.Length.ToString());


            foreach (string filename in fileEntries)
            {
                FileInfo file = new FileInfo(filename);
                log.Info("Trabajando envio SFTP para el archivo" + file.Name);
                
                sftpSatander sSantander = new sftpSatander();
                string envioRespuesta = sSantander.EnvioSFTPSantander(filename, DirEncryptH2H, dirH2HSend);
                enviaNotificacion send = new enviaNotificacion();

                if (!string.IsNullOrEmpty(envioRespuesta))
                {
                    log.Error("El archivo  " + file.Name + " no pudo ser enviado a Santander: " + envioRespuesta);
                    
                    send.enviaCorreo(filename, false, MensajeSftpEnvio);
                }
                else
                {
                    log.Info("El archivo  " + file.Name + " Se envio a santander correctamente" + envioRespuesta);
                    send.enviaCorreo(filename, true, MensajeSftpEnvio);
                }
            }

            log.Info("Fin Proceso Envio SFTP...");
        }

        private void EncryptFileH2H()
        {
            try
            {
                log.Info("Inicio EncryptFileH2H...");

                log.Info("Obteniendo Archivos Encrypt...");

                string DirDecryptGnuGP = ConfigurationManager.AppSettings["DirDecryptGnuGP"];
                string DirEncryptH2H = ConfigurationManager.AppSettings["DirEncryptH2H"];
                string DirApiH2HEncrypt = ConfigurationManager.AppSettings["DirApiH2HEncrypt"];
                string MensajeH2HEncriptar = ConfigurationManager.AppSettings["MensajeH2HEncriptar"];
                string[] fileEntries = Directory.GetFiles(DirDecryptGnuGP);


                log.Info("Obtenidos " + fileEntries.Length.ToString() + "");

                foreach (string fileName in fileEntries)
                {
                    FileInfo file = new FileInfo(fileName);
                    ComandosCMD cmd = new ComandosCMD();
                    string respuesta = cmd.EncryptFileH2H(file, DirApiH2HEncrypt, DirDecryptGnuGP, DirEncryptH2H);
                    enviaNotificacion send = new enviaNotificacion();

                    if (respuesta != string.Empty || respuesta.Contains("tError"))
                    {
                        log.Error("El archivo " + file.Name + " no pudo ser encriptado con la api santander: " + respuesta);
                        send.enviaCorreo(fileName, false, MensajeH2HEncriptar);

                    }
                    else
                    {
                        log.Info("El archivo  " + file.Name + " fue encriptado con la api santander correctamente " + respuesta);
                    }
                }

                log.Info("Fin EncryptFileH2H...");
            }
            catch (Exception ex)
            {
                log.Info("EncryptFileH2H error:" + ex.Message);
            }
        }      

        private void DecryptGnuPFile()
        {
            try
            {
                log.Info("Inicio DecryptGnuPFile...");

                log.Info("Obteniendo Archivos Encrypt...");

                string DirDecryptGnuGP = ConfigurationManager.AppSettings["DirDecryptGnuGP"];
                string DirEncryptGnuGP = ConfigurationManager.AppSettings["DirEncryptGnuGP"];
                string DirEncryptGnuGPBack = ConfigurationManager.AppSettings["DirEncryptGnuGPBack"];
                string MensajeGnpDesencriptar = ConfigurationManager.AppSettings["MensajeGnpDesencriptar"];
                string[] fileEntries = Directory.GetFiles(DirEncryptGnuGP);

                log.Info("Obtenidos " + fileEntries.Length.ToString() + "");

                foreach (string fileName in fileEntries)
                {
                    FileInfo file = new FileInfo(fileName);
                    ComandosCMD cmd = new ComandosCMD();
                    string respuesta = cmd.DecrypFileGnuGP(file, DirDecryptGnuGP, DirEncryptGnuGP);
                    enviaNotificacion send = new enviaNotificacion();
                    if (respuesta.Contains("gpg: decrypt_message failed") || respuesta.Contains("tError"))
                    {
                        log.Error("El archivo GnuGP " + file.Name + " no pudo ser desencriptado: " + respuesta);
                        send.enviaCorreo(fileName, false, MensajeGnpDesencriptar);

                    }
                    else
                    {
                        if (File.Exists(DirEncryptGnuGPBack + file.Name))
                        {
                            File.Delete(DirEncryptGnuGPBack + file.Name);
                        }
                        File.Move(file.FullName, DirEncryptGnuGPBack + file.Name);

                        log.Info("El archivo GnuGP " + file.Name + " fue desencriptado correctamente y movido a la carpeta: " + DirEncryptGnuGPBack);
                    }
                }

                log.Info("Fin DecryptGnuPFile...");

            }
            catch (Exception ex)
            {
                log.Error("DecryptGnuPFile error:" + ex.Message);

            }
        }

        private bool ConfiguracionAplicacion()
        {
            try
            {
                Boolean configCorrecta = true;

                string startupPath = Application.StartupPath.Replace("\\bin\\Debug", "");
                string strArchivoLog = startupPath + "\\log\\";
                if (!Directory.Exists(strArchivoLog))
                    Directory.CreateDirectory(strArchivoLog);

                if (!Convert.ToBoolean(Convert.ToInt32(ConfigurationManager.AppSettings["sActivo"])))
                    configCorrecta = false;


                if (configCorrecta)
                    log.Info("Configuracion correcta de la aplicacion");
                else

                    log.Warn("Sin configuracion de la aplicacion,  o existe ventana de mantenimiento");

                return configCorrecta;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        protected override void OnStop()
        {
            try
            {
                log.Info("Servicio Detenido OnStop");
                timer.Stop();

            }
            catch (Exception ex)
            {
                log.Error("Ocurrio un error al detener el servicio H2HSandatnder, " + ex.Message);
            }

        }
    }
}
