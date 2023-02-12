using dotnet_etcd;

namespace etcd.Provider.Cluster.Extensions
{
    public static  class EtcdClientExtensions
    {
        public static  IEtcdCluster GetEtcdClient(this EtcdClient client, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            var cluster = new EtcdCluster(client)
            {
                Username = username,
                Password = password,
                CaCert = caCert,
                ClientCert = clientCert,
                ClientKey = clientKey,
                PublicRootCa = publicRootCa
            };
            return cluster;
        }

        public static IEtcdCluster GetEtcdClient(this HostAndPort client, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            var cluster = new EtcdCluster(client)
            {
                Username = username,
                Password = password,
                CaCert = caCert,
                ClientCert = clientCert,
                ClientKey = clientKey,
                PublicRootCa = publicRootCa
            };
            return cluster;
        }


        public static IEtcdCluster GetEtcdClient(this HostAndPort[] clients, string username = "etcduser", string password = "etcdsrv", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            var cluster = new EtcdCluster(clients)
            {
                Username = username,
                Password = password,
                CaCert = caCert,
                ClientCert = clientCert,
                ClientKey = clientKey,
                PublicRootCa = publicRootCa
            };
            return cluster;
        }
    }
}
