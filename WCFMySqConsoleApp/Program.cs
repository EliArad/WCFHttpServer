using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using WCFMySqConnector;
using WCFShared;

namespace WCFMySqConsoleApp
{
    class Program
    {
      
        static void Main(string[] args)
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

            Console.ReadLine();
            WCFConnect.Close();

        }
         
        
    }
}
