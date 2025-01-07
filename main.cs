using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace MavenInstaller
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string mavenVersion = "3.9.9"; // Specify the Maven version you want to install
            string installPath = $@"C:\Program Files\Maven\Maven{mavenVersion}";
            string zipFilePath = Path.Combine(Path.GetTempPath(), $"apache-maven-{mavenVersion}-bin.zip");
            string downloadUrl = $"https://downloads.apache.org/maven/maven-3/{mavenVersion}/binaries/apache-maven-{mavenVersion}-bin.zip";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Console.WriteLine("Downloading Maven...");
                    var response = await client.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();
                    await using (var fs = new FileStream(zipFilePath, FileMode.CreateNew))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                Console.WriteLine("Extracting Maven...");
                ZipFile.ExtractToDirectory(zipFilePath, installPath);

                Console.WriteLine("Setting MAVEN_HOME environment variable...");
                Environment.SetEnvironmentVariable("MAVEN_HOME", installPath, EnvironmentVariableTarget.Machine);

                string pathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
                if (!pathVariable.Contains(installPath + @"\bin"))
                {
                    Console.WriteLine("Updating PATH environment variable...");
                    Environment.SetEnvironmentVariable("PATH", pathVariable + $";{installPath}\\bin", EnvironmentVariableTarget.Machine);
                }

                Console.WriteLine("Maven installed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
        }
    }
}