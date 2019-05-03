using dotnet_etcd;
using System.Collections.Generic;

namespace etcd.Provider.Cluster.Extensions
{
  public  interface IEtcdCluster
    {
        EtcdClient GetClient();
        List<EtcdClientUrls> GetClientUrls();
        EtcdClient[] GetEtcdClients(int num);
    }
}
