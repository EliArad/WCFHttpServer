using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using WCFShared;

namespace WCFHttpWinService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                WCFConnect.OpenService();
                Console.WriteLine("Http service started at: http://localhost:8020/");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadKey();
                return;
            }
        }

        protected override void OnStop()
        {
            WCFConnect.Close();
        }
    }
}
