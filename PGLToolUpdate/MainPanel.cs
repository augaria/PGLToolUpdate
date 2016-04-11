using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;


namespace PGLToolUpdate
{
    public partial class MainPanel : Form
    {
        string newVersion;
        public MainPanel(string newVersion)
        {
            try
            {
                InitializeComponent();
                
                this.newVersion = newVersion;
            }
            catch(Exception e0)
            {
                BugBox bb = new BugBox(e0.ToString());
                bb.ShowDialog();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                label1.Update();
                pictureBox1.Update();
                this.Show();

                //download the update instructions
                string url = "https://raw.githubusercontent.com/DearUnknown/PGLToolUpdate/master/PGLToolUpdate/VersionV1.0/";
                string instr = "updateActions.txt";
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(url + instr, "Update/updateActions.txt");

                //wait untill the main application is closed 
                int count = 0;
                bool waiting = true;
                while (waiting)
                {
                    if (count == 3)
                    {
                        BugBox bb = new BugBox("PGLData.exe一直被占用,请手动将其关闭");
                        bb.ShowDialog();
                        return;
                    }
                    try
                    {
                        FileStream fs = File.OpenWrite("PGLData.exe");
                        fs.Close();
                        waiting = false;
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(3000);
                        count++;
                    }
                }

                //read and follow the instructions
                FileStream aFile = new FileStream("Update/updateActions.txt", FileMode.Open);
                StreamReader sr = new StreamReader(aFile, Encoding.GetEncoding("UTF-8"));
                XmlDocument doc = new XmlDocument();
                doc.Load("PGLData.exe.config");
                string line = sr.ReadLine();
                while (line != null)
                {
                    //delete a file
                    if (line.Equals("/"))
                    {
                        File.Delete(sr.ReadLine());
                        line = sr.ReadLine();
                    }

                    //add new setttings to the configuration file
                    else if (line.Equals("xml"))
                    {
                        string key = sr.ReadLine();
                        string value = sr.ReadLine();
                        XmlNode target = doc.SelectSingleNode(@"//add[@key='"+key+"']");
                        if (target == null)
                        {
                            XmlNode root = doc.SelectSingleNode(@"//appSettings");
                            XmlElement xe1 = doc.CreateElement("add");
                            xe1.SetAttribute("key", key);
                            xe1.SetAttribute("value", value);
                            root.AppendChild(xe1);
                        }
                        line = sr.ReadLine(); 
                    }

                    //add new file or replace a file
                    else
                    {
                        myWebClient.DownloadFile(url + line, sr.ReadLine());
                        line = sr.ReadLine();
                    }
                }

                //update the version number
                XmlNode node = doc.SelectSingleNode(@"//add[@key='AppVersion']");
                XmlElement ele = (XmlElement)node;
                ele.SetAttribute("value", newVersion);
                doc.Save("PGLData.exe.config");

                //notice finish and start the main application
                label1.Text = "更新完成~~~";
                label1.Update();
                System.Threading.Thread.Sleep(1500);               
                Process.Start("PGLData.exe");
                Environment.Exit(0);
            }
            catch (Exception e1)
            {
                BugBox bb = new BugBox(e1.ToString());
                bb.ShowDialog();
            }
        }
    }
}
