using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServicioH2HSantander
{
    public class ComandosCMD
    {    
        private string EjecutaComando(string comando)
        {

            try

            {
                //System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + comando);
                //procStartInfo.RedirectStandardOutput = true;
                //procStartInfo.RedirectStandardError = true;
                //procStartInfo.UseShellExecute = false;
                //procStartInfo.CreateNoWindow = false;
                //System.Diagnostics.Process proc = new System.Diagnostics.Process();

                //proc.StartInfo = procStartInfo;
                //proc.Start();
                ////string result = proc.StandardOutput.ReadToEnd();

                //string result = proc.StandardError.ReadToEnd();
                //proc.Close();
                //proc.Dispose();

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C " + comando;
                process.StartInfo = startInfo;
                process.Start();
                string result = process.StandardError.ReadToEnd();
                process.Close();
                process.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                return "tError " + ex.Message;
            }
            
        }

        public string DecrypFileGnuGP(FileInfo file,string DirDecryptGnuGP, string DirEncryptGnuGP)
        {
            try
            {
                string cmd = "gpg --yes --output " + DirDecryptGnuGP + file.Name.Replace(".gpg", "") + " --decrypt " + "" + DirEncryptGnuGP + "" + file.Name;
                string Result = EjecutaComando(cmd);

                //if(!Result.ToUpper().Contains("GPG: FIRMADO"))
                //{
                //    return "tError: No se pudo realizar el decifrado del archivo: " + Result;
                //}

                Process[] processes = Process.GetProcessesByName("gpg");
                foreach (Process porc in processes)
                {
                    porc.Kill();
                }
                return Result;
            }
            catch (Exception ex)
            {
                return "tError " + ex.Message;
            }

            
        }

        public string EncryptFileH2H(FileInfo file, string DirApiH2HEncrypt ,string DirDecryptGnuGP, string DirEncryptH2H)
        {
            try
            {               
                string dirApi = DirApiH2HEncrypt + " ";
                string dirDecrypt =  DirDecryptGnuGP + file.Name +" "; 
                string dirEncrypt = DirEncryptH2H + file.Name.Replace(".TXT", ".in2");
                string cmd = dirApi + dirDecrypt + dirEncrypt;

              
                return EjecutaComando(cmd);
            }
            catch (Exception ex)
            {
                return "tError " + ex.Message;
            }   
        }
    }
}
