using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KellermanSoftware;
using KellermanSoftware.NetSFtpLibrary;
using System.Diagnostics;
using AlexPilotti.FTPS;
using System.Collections;
using AlexPilotti.FTPS.Client;

namespace FTPTestApp
{
    class Program
    {
        static private SFTP _sftp;
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            var logFile = File.AppendText("c:\\logs\\ftp directory test.txt");
            //sw.Start();
            //FTPResponse();
            //sw.Stop();
            //Console.WriteLine("FTPResponse " + sw.Elapsed.TotalSeconds);
            //sw.Reset();
            sw.Start();
            FTPClientTest(logFile);
            sw.Stop();
            logFile.WriteLine("FTPClient " + sw.Elapsed.TotalSeconds + "seconds");
            Console.WriteLine("FTPClient " + sw.Elapsed.TotalSeconds + "seconds");
            sw.Reset();
            sw.Start();
            KellermanConnect(logFile);
            sw.Stop();
            Console.WriteLine("Kellerman " + sw.Elapsed.TotalSeconds + "seconds");
            logFile.WriteLine("Kellerman " + sw.Elapsed.TotalSeconds + "seconds");
            logFile.Flush();
            logFile.Close();
            Console.ReadKey();
        }

        static void FTPResponse()
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://opticat5.net/RFormula1/postsynctest");
            request.EnableSsl = true;
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("username","password");

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            //Console.WriteLine(reader.ReadToEnd());
            var dirList = new List<string>();
            do
            {
                dirList.Add(reader.ReadLine());
            }while (!reader.EndOfStream);

            Console.WriteLine($"Directory List Complete, file count = {dirList.Count}");

            reader.Close();
            response.Close();
        }

        static void KellermanConnect(StreamWriter logFile)
        {
            _sftp = new SFTP();
            _sftp.HostAddress = "opticat5.net";
            //_sftp.CurrentDirectory = "ReceiverSC";
            _sftp.UserName = "username";
            _sftp.Password = "password";
            _sftp.Connect();

            var dirList = _sftp.GetAllFiles(".");
            Console.WriteLine($"Directory List Complete, file count = {dirList.Count}");
            logFile.WriteLine($"Directory List Complete, file count = {dirList.Count}");
            _sftp.Disconnect();
            _sftp.Dispose();
        }

        static void FTPClientTest(StreamWriter logFile)
        {
            var ftpClient = new AlexPilotti.FTPS.Client.FTPSClient();
            {
                ftpClient.Connect("opticat5.net", new NetworkCredential("username", "password"),AlexPilotti.FTPS.Client.ESSLSupportMode.Implicit);
                //ftpClient.SetCurrentDirectory("/RFormula1/postsynctest");
                var dirList = new List<AlexPilotti.FTPS.Common.DirectoryListItem>();
                GetFTPClientFiles(ftpClient,null, ref  dirList);
                Console.WriteLine($"Directory List Complete, file count = {dirList.Count}");
                logFile.WriteLine($"Directory List Complete, file count = {dirList.Count}");
            }
        }

        static void GetFTPClientFiles(FTPSClient ftp, string dir, ref List<AlexPilotti.FTPS.Common.DirectoryListItem> dirList)
        {
            if(!string.IsNullOrEmpty(dir))
            {
                ftp.SetCurrentDirectory(dir);
            }
            var localDirList = ftp.GetDirectoryList();
            dirList.AddRange(localDirList.Where(x => x.IsDirectory == false));
            foreach (var item in localDirList.Where(x => x.IsDirectory))
            {
                GetFTPClientFiles(ftp, item.Name, ref dirList);
            }
            ftp.SetCurrentDirectory("..");
        }
    }
}
