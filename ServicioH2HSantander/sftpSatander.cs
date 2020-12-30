using Rebex.Net;
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
    public class sftpSatander
    {
        public string EnvioSFTPSantander(string fileName, string dirEncryptH2H, string dirH2HSend)
        {

            try
            {
                Sftp sftp = new Sftp();
                string archivoKey = ConfigurationManager.AppSettings["DirFileH2H"];
                string usuario = ConfigurationManager.AppSettings["Usuario"];
                int puerto = Convert.ToInt32(ConfigurationManager.AppSettings["Puerto"]);
                string server = ConfigurationManager.AppSettings["Server"];
                string dirS = ConfigurationManager.AppSettings["DirSantander"];

                string startupPath = Application.StartupPath.Replace("\\bin\\Debug", "");

                if (File.Exists(startupPath + archivoKey))
                {
                    FileInfo file = new FileInfo(fileName);
                    sftp.Connect(server, puerto);
                    SshPrivateKey sk = new SshPrivateKey(startupPath + archivoKey, null);
                    sftp.Login(usuario, sk);
                    if (sftp.GetConnectionState().Connected)
                    {
                        string ruta = sftp.GetCurrentDirectory() + dirS;

                        if (sftp.DirectoryExists(ruta))
                        {
                            sftp.Upload(file.FullName, dirS, Rebex.IO.TraversalMode.NonRecursive, Rebex.IO.TransferMethod.Copy, Rebex.IO.ActionOnExistingFiles.OverwriteAll);

                            if (File.Exists(dirH2HSend + file.Name))
                            {
                                File.Delete(dirH2HSend + file.Name);
                            }
                            File.Move(file.FullName, dirH2HSend + file.Name);

                            sftp.Disconnect();
                            return string.Empty;
                        }
                        else
                        {
                            return "No existe el repositorio SFTP Santander Destino:" + dirS;                           
                        }
                        
                    }
                    else
                    {
                        return "La conexion SFTP no pudo generarse";
                    }
                }
                else
                {
                    return "No existe archivo Key para conexion SFTP";
                }
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
