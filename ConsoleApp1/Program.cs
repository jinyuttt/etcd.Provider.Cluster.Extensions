using System;
using System.Threading;
using dotnet_etcd;
using etcd.Provider.Cluster.Extensions;
namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            EtcdClient client = new EtcdClient("127.0.0.1", 2379);
            client.WatchRange("/ert/",new Action<WatchEvent[]>(p =>
            {
                 foreach(var x in p)
                {
                    Console.WriteLine(string.Format("{0},{1}:{2}", x.Type, x.Key, x.Value));
                }
            }));


            while (true)
            {
               var c= client.GetEtcdClient().GetClient();
                c.Put("/ert/1", "1111111");
                Thread.Sleep(10000);
                c.Put("/ert/2", "2222222");
                Thread.Sleep(10000);
                c.Put("/ert/3", "33333333");
                Console.ReadLine();
            }
        }
    }
}
