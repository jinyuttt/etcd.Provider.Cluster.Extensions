using System;
using dotnet_etcd;
using Ocelot.Provider.Etcd.Extensions;
namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            EtcdClient client = new EtcdClient("127.0.0.1", 2379);
            while (true)
            {
               var c= client.GetEtcdClient().GetClient();
                c.Put("ert", "1111111");
                Console.ReadLine();
                 var rsp=  c.Get("ert");
                Console.WriteLine(rsp.Kvs.Count);
            }
        }
    }
}
