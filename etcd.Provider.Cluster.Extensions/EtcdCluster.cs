#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Cluster.Extensions
* 项目描述 ：
* 类 名 称 ：EtcdCluster
* 类 描 述 ：
* 命名空间 ：etcd.Provider.Cluster.Extensions
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using dotnet_etcd;
using Etcdserverpb;

namespace etcd.Provider.Cluster.Extensions
{
    /* ============================================================================== 
* 功能描述：EtcdCluster 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public  class EtcdCluster : IEtcdCluster,IDisposable
    {
        readonly List<EtcdClientUrls> Urls = new List<EtcdClientUrls>();
        EtcdClient _client;
       
        Timer timer = null;
        volatile int _index = 0;
        private HostAndPort[] clients;
        string username = "";
        string password = "";
        string caCert = "";
        string clientCert = "";
         string clientKey = "";
         bool publicRootCa = false;
        public string Username { get { return username; } set { username = value; } }
        public string Password { get { return password; } set { password = value; } }
        public string CaCert { get { return caCert; } set { caCert = value; } }
        public string ClientCert { get { return clientCert; } set { clientCert = value; } }
        public string ClientKey { get { return clientKey; } set { clientKey = value; } }
        public bool PublicRootCa { get { return publicRootCa; } set { publicRootCa = value; } }

        public EtcdCluster(EtcdClient client)
        {
            _client = client;
            TimerFulsh();
        }

        public EtcdCluster(HostAndPort client):this(new HostAndPort[] { client })
        {
          
        }

        public EtcdCluster(HostAndPort[] clients)
        {
            this.clients = clients;
            Init();
        }

        private void Init()
        {
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < clients.Length; i++)
                {
                    try
                    {
                        var c = clients[i];
                        _client = new EtcdClient(c.Host, c.Port);
                        break;
                    }
                    catch
                    {

                    }
                }
                TimerFulsh();
            });
           
        }

        private void TimerFulsh()
        {
            timer = new Timer(2000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MemberListResponse rsp = null;
            try
            {
               rsp= _client.MemberList(new Etcdserverpb.MemberListRequest());
            }catch
            {
                GetClient();
                return;//下次更新
            }
            lock (Urls)
            {
                Urls.Clear();
                foreach (var kv in rsp.Members)
                {
                    EtcdClientUrls etcd = new EtcdClientUrls();
                    foreach (var c in kv.ClientURLs)
                    {

                        string[] addr = c.Split(new char[] { ':','/' }, StringSplitOptions.RemoveEmptyEntries);
                        if (addr.Length == 2)
                        {
                            HostAndPort host = new HostAndPort() { Host = addr[0], Port = int.Parse(addr[1]) };
                            etcd.Urls.Add(host);
                        }
                        else if (addr.Length == 3)
                        {
                            HostAndPort host = new HostAndPort() { Host = addr[1], Port = int.Parse(addr[2]) };
                            etcd.Urls.Add(host);
                        }

                    }
                    Urls.Add(etcd);
                }
            }
        }

        public EtcdClient GetClient()
        {
            try
            {
                _client.Put("TestCon", "1111");
            }catch
            {
                _client.Dispose();
                _client = null;
                var c = GetEtcdClients(1);
                if(c!=null&&c[0]!=null)
                {
                    _client.Dispose();
                    _client = c[0];
                }
                
            }

            return _client;
        }

        public List<EtcdClientUrls> GetClientUrls()
        {
            return new List<EtcdClientUrls>(Urls);
        }

        public EtcdClient[] GetEtcdClients(int num)
        {
            EtcdClient[] clients = new EtcdClient[num];
            int index = 0;
            int start = index;
            lock (Urls)
            {
                if(Urls.Count==0)
                {
                    return clients;
                }
                while (true)
                {
                    var c = Urls[_index++ % Urls.Count];
                    foreach (var url in c.Urls)
                    {
                        try
                        {
                            EtcdClient client = new EtcdClient(url.Host, url.Port);
                            clients[index] = client;
                            index++;
                            break;//每个客户端连接一个
                        }
                        catch
                        {

                        }
                    }
                    if (index + 1 == num || _index == 2 * Urls.Count + start)
                    {
                        break;
                    }

                }
            }
            return clients;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
            timer = null;
            Urls.Clear();
            _client.Dispose();
        }
    }
}
