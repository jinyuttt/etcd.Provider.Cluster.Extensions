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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

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
        readonly List<EtcdClientUrls> Urls = null;
        private readonly Dictionary<string, ClientMonitor> dic = null;
        private readonly ReaderWriterLockSlim readerWriter=null;
        EtcdClient _client=null;
        System.Timers.Timer timer = null;
        volatile int _index = 0;//地址获取，主从
        volatile int _Lindex = 0;//轮训
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

        /// <summary>
        /// 集群使用方式
        /// </summary>
        public ClusterUse ClusterUseType { get; set; }

        public EtcdCluster()
        {
            Urls = new List<EtcdClientUrls>();
            readerWriter = new ReaderWriterLockSlim();
            dic = new Dictionary<string, ClientMonitor>();

        }

       

        public EtcdCluster(EtcdClient client):this()
        {
            _client = client;
            StartTimerFulsh();
        }

        public EtcdCluster(HostAndPort client):this(new HostAndPort[] { client })
        {
          
        }

        public EtcdCluster(HostAndPort[] clients):this()
        {
            this.clients = clients;
            Init();
        }

        /// <summary>
        /// 初始化客户端
        /// </summary>
        private void Init()
        {
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < clients.Length; i++)
                {
                    try
                    {
                        var c = clients[i];
                        _client = CreateClient(c.Host, c.Port);
                        break;
                    }
                    catch
                    {

                    }
                }
                StartTimerFulsh();
            });
           
        }

        /// <summary>
        /// 开始获取地址
        /// </summary>
        private void StartTimerFulsh()
        {
            timer = new System.Timers.Timer(10000);//更新10s
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// 定时刷新节点地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MemberListResponse rsp;
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
                            HostAndPort host = new HostAndPort() { Host = addr[0], Port = int.Parse(addr[1]), Flage=addr[0]+addr[1] };
                            etcd.Urls.Add(host);
                        }
                        else if (addr.Length == 3)
                        {
                            HostAndPort host = new HostAndPort() { Host = addr[1], Port = int.Parse(addr[2]), Flage = addr[1] + addr[2] };
                            etcd.Urls.Add(host);
                        }

                    }
                    Urls.Add(etcd);
                    //

                }
                //
                if (ClusterUseType == ClusterUse.RoundRobin)
                {
                   
                    foreach (var c in Urls)
                    {
                        //每个客户端一个连接
                        foreach(var addr in c.Urls)
                        {
                            ClientMonitor monitor = null;
                            readerWriter.EnterWriteLock();
                            try
                            {
                                if (dic.TryGetValue(addr.Flage, out monitor))
                                {
                                    try
                                    {
                                        monitor.Client.Put("Test", "Test");
                                        break;
                                    }
                                    catch
                                    {
                                        dic.Remove(addr.Flage);
                                    }
                                }
                                else
                                {
                                    EtcdClient etcdClient = CreateClient(addr.Host, addr.Port);
                                    if (etcdClient != null)
                                    {
                                        monitor = new ClientMonitor() { Client = etcdClient };
                                        dic[addr.Flage] = monitor;
                                        break;
                                    }

                                }
                            }
                            finally
                            {
                                readerWriter.ExitWriteLock();
                            }

                        }
                        
                    }
                }
            }
        }

       /// <summary>
       /// 获取
       /// </summary>
       /// <returns></returns>
        public EtcdClient GetClient()
        {
            if (ClusterUseType == ClusterUse.Master_Slave)
            {
                try
                {
                    _client.Put("TestCon", "EtcdCluster");
                }
                catch
                {
                    _client.Dispose();
                    _client = null;
                    var c = GetEtcdClients(1);
                    if (c != null && c[0] != null)
                    {
                        _client.Dispose();
                        _client = c[0];
                    }

                }

                return _client;
            }
            else
            {
                if(readerWriter.TryEnterReadLock(100))
                {
                    try
                    {
                        var c = dic.Values.Skip(_Lindex++ / dic.Count).First();
                        if (c != null)
                        {
                            c.lastUse = DateTime.Now.Ticks;
                            c.UseNum++;
                            return c.Client;
                        }
                        return null;
                    }
                    finally
                    {
                        readerWriter.ExitReadLock();
                    }
                
                }
                return null;
            }
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private EtcdClient CreateClient(string host,int port)
        {
            try
            {
                var c = new EtcdClient(host, port, username, password, caCert, clientCert, clientKey, publicRootCa);
                c.Put("TestCon", "EtcdCluster");
                return c;
            }
            catch
            { return null; }
        }

        /// <summary>
        /// 获取节点地址
        /// </summary>
        /// <returns></returns>
        public List<EtcdClientUrls> GetClientUrls()
        {
            return new List<EtcdClientUrls>(Urls);
        }

        /// <summary>
        /// 获取连接的客户端
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
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
                            EtcdClient client = CreateClient(url.Host, url.Port);
                            if (client != null)
                            {
                                clients[index] = client;
                                index++;
                                break;//每个客户端连接一个
                            }
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
