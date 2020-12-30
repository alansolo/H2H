using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServicioH2HSantander
{
    class MailEntry
    {
        public bool error;
        public string Pass;
        public string usuario;
        public string Cuenta;
        public string Host;
        public int Port;
        public bool SSL;
        public string Mail;
        public string MailCopia;
        public string MailCopiaOculta;
    }


    public class enviaNotificacion
    {
        private MailEntry Email_getConfiguracion()
        {
            MailEntry cgConfig = new MailEntry();

            try
            {
                cgConfig.Cuenta = ConfigurationManager.AppSettings["CuentaMail"].ToString();
                cgConfig.Host = ConfigurationManager.AppSettings["HostMail"].ToString();
                cgConfig.Port = int.Parse(ConfigurationManager.AppSettings["PortMail"].ToString());
                cgConfig.SSL = bool.Parse(ConfigurationManager.AppSettings["SSLMail"].ToString());
                cgConfig.error = false;
                cgConfig.usuario = ConfigurationManager.AppSettings["UsuarioMail"].ToString();
                cgConfig.Pass = ConfigurationManager.AppSettings["PassMail"].ToString();
                cgConfig.Mail = ConfigurationManager.AppSettings["EnvioMail"].ToString();
                cgConfig.MailCopia= ConfigurationManager.AppSettings["EnvioMailCopia"].ToString();
                cgConfig.MailCopiaOculta = ConfigurationManager.AppSettings["EnvioMailOculta"].ToString();
            }
            catch (Exception)
            {
                cgConfig.error = true;
            }

            return cgConfig;
        }

        public string enviaCorreo(string fileName, bool proceso, string mensaje)
        {
            try
            {
                FileInfo file = new FileInfo(fileName);

                MailEntry config = new MailEntry();
                config = Email_getConfiguracion();
                if (!config.error)
                {
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();

                    System.Net.Mail.MailMessage correo = new System.Net.Mail.MailMessage();
                    char delimiter = ';';
                    string[] emails = config.Mail.Split(delimiter);
                    foreach (string email in emails)
                    {
                        if (!string.IsNullOrEmpty(email.Replace("\r", "").Replace("\n", "")))
                        {
                            correo.To.Add(email);
                        }
                    }
                    correo.Subject = "Notificación de envió H2H Santander " + file.Name;
                    correo.IsBodyHtml = true;
                    correo.Priority = System.Net.Mail.MailPriority.High;

                    System.Net.Mail.AlternateView alterView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(Email_Template(mensaje, file.Name, DateTime.Now.ToString(), proceso == true ? "EnvioCorrecto.html" : "EnvioIncorrecto.html").ToString(), Encoding.UTF8, "text/html");

                    // string pathInicio = (Application.StartupPath.EndsWith("\\") ? Application.StartupPath : Application.StartupPath + "\\");
                    //System.Net.Mail.LinkedResource imgResource = new System.Net.Mail.LinkedResource(Path.Combine(pathInicio, "images/logo_PPG-ComexBaner.png"), "image/png")
                    //{
                    //    ContentId = "Email_Cid1",
                    //    TransferEncoding = System.Net.Mime.TransferEncoding.Base64
                    //};
                    //alterView.LinkedResources.Add(imgResource);

                    correo.AlternateViews.Add(alterView);
                    smtp.Credentials = new System.Net.NetworkCredential(config.usuario, config.Pass);
                    correo.From = new System.Net.Mail.MailAddress(config.Cuenta);


                    //if (!proceso)
                    //{
                    //    foreach (string itemCopia in config.MailCopiaOculta.Split(';'))
                    //    {
                    //        correo.CC.Add(itemCopia);
                    //    }
                    //}

                    foreach (string itemCopia in config.MailCopia.Split(';'))
                    {
                        if(!string.IsNullOrEmpty(itemCopia))
                        {
                            correo.Bcc.Add(itemCopia);
                        }
                        
                    }

                    foreach (string itemCopia in config.MailCopiaOculta.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(itemCopia))
                        {
                            correo.CC.Add(itemCopia);
                        }
                    }

                    smtp.Host = config.Host;
                    smtp.Port = config.Port;
                    smtp.EnableSsl = config.SSL;
                    smtp.Timeout = 5000;
                    smtp.Send(correo);
                    correo.Dispose();
                    return string.Empty;
                }

                else
                {
                    return "No se lograron cargar los valores del correo";

                }

            }
            catch (Exception ex)
            {
                return "No se logro enviar correo: " + ex.Message;
            }
            
        }

        private StringBuilder Email_Template(string Mensaje, string NombreArchivo,string FechaEnvio,string archivoMail)
        {
            StringBuilder strHtm = new StringBuilder();

            string startupPath = Application.StartupPath.Replace("\\bin\\Debug", "");
            strHtm.Append(File.ReadAllText(startupPath + "\\FormatosEmail\\" + archivoMail, Encoding.Default));

            if(!string.IsNullOrEmpty(Mensaje))
            {
                if (strHtm.ToString().Contains("#Mensaje"))
                {
                    strHtm.Replace("#Mensaje", Mensaje);
                }
            }

            if (!string.IsNullOrEmpty(NombreArchivo))
            {
                if (strHtm.ToString().Contains("#Archivo"))
                {
                    strHtm.Replace("#Archivo", NombreArchivo);
                }
            }

            if (!string.IsNullOrEmpty(FechaEnvio))
            {
                if (strHtm.ToString().Contains("#FechaEnvio"))
                {
                    strHtm.Replace("#FechaEnvio", FechaEnvio);
                }
            }
          
            return strHtm;
        }
   
    }
}
