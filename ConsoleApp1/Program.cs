using System;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd;
using etcd.Provider.Cluster.Extensions;
namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            EtcdClient client = new EtcdClient("127.0.0.1", 2379);
            Task.Factory.StartNew(() => {
                client.WatchRange("ert/", print);
            });

            Task.Factory.StartNew(() => {
                client.Watch("ert/1", new Action<WatchEvent[]>(p =>
                {
                    foreach (var x in p)
                    {
                        Console.WriteLine("watch_"+string.Format("{0},{1}:{2}", x.Type, x.Key, x.Value));
                    }
                }));
            });

            while (true)
            {
               var c= client.GetEtcdClient().GetClient();
                c.Put("ert/1", "1111111",null,DateTime.UtcNow.AddSeconds(3));
                Thread.Sleep(10000);
                c.Put("ert/2", "2222222");
                Thread.Sleep(10000);
                c.Put("ert/3", "33333333");
                Console.ReadLine();

                c.Delete("ert/2");
            }
        }

        private static void print(WatchEvent[] response)
        {
            foreach (WatchEvent e1 in response)
            {
                Console.WriteLine($"watch-{e1.Key}:{e1.Value}:{e1.Type}");
            }
        }
    }
}
