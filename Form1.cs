using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Limilabs.FTP.Client;

namespace ListaTubeLe
{
    public partial class Form1 : Form
    {

        private DataSet dsResultado = new DataSet();
        private int tempo = 0;
        private DateTime HoraFim;
        private int NrVideos = 0;
        private int NrdoVideo = 0;
        private string Versao = "0.0";
        private Boolean LogCriado = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\ListaTubeLe.exe");
            var found = new AssemblyName(assembly.FullName);
            Versao = "Versão " + found.Version.Major + "." + found.Version.Minor + "." + found.Version.Build + "." + found.Version.Revision;

            // Puxar o XML atualizado do FTP
            try
            {
                using (Ftp client = new Ftp())
                {
                    client.Connect("ftp.tele-tudo.com");
                    client.Login("teletu76", "bPruC717y0");
                    string path = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + @"ListaTube.xml";
                    client.Download("/public_html/letube/FtpTrial-ListaTube.xml", path);
                    dsResultado.ReadXml(path);
                    NrVideos = dsResultado.Tables[0].Rows.Count;
                    MostraVideo(0);
                }

            }
            catch (Exception ex)
            {
                loga("Erro no FTP");
                throw;
            }
        }

        private void MostraVideo(int v)
        {
            string endereco = dsResultado.Tables[0].Rows[v][4].ToString();
            webBrowser1.Navigate(endereco);
            tempo = int.Parse(dsResultado.Tables[0].Rows[v][5].ToString());
            HoraFim = DateTime.Now.AddSeconds(tempo);
            timer1.Enabled = true;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string ver = webBrowser1.DocumentTitle + " Versão "+Versao;
            if (this.Text != ver)
            {
                this.Text = ver;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now > HoraFim)
            {
                timer1.Enabled = false;
                ChamaProxVideo();
            }
        }

        private void ChamaProxVideo()
        {
            NrdoVideo++;
            if (NrdoVideo>NrVideos)
            {
                MessageBox.Show("Acabaram-se os links", "ListaTubeLe", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else
            {
                MostraVideo(NrdoVideo);
            }            
        }

        private void loga(string Texto)
        {
            string Agora = DateTime.Now.ToLongTimeString();
            Console.WriteLine(Agora + " " + Texto);
            StreamWriter aLog;
            string CaminhoLog = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\log.log";
            if (LogCriado)
            {
                aLog = File.AppendText(CaminhoLog);
            }
            else
            {
                LogCriado = true;
                aLog = File.CreateText(CaminhoLog);
            }
            aLog.WriteLine(Texto);
            aLog.Close();
        }

    }

}
