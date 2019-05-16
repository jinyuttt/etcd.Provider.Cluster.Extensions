#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Cluster.Extensions
* 项目描述 ：
* 类 名 称 ：ClientMonitor
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

using dotnet_etcd;
using System;
using System.Collections.Generic;
using System.Text;

namespace etcd.Provider.Cluster.Extensions
{

    /* ============================================================================== 
* 功能描述：ClientMonitor 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
   public class ClientMonitor
    {
        public EtcdClient Client { get; set; }
        public int UseNum = 0;
        public long lastUse = DateTime.Now.Ticks;
        public const long MSTicks = 10000000;
        public long IdleTime = 60;
        public bool TimeOut
        {
            get { return (DateTime.Now.Ticks - lastUse) / MSTicks > IdleTime; }
        }
    }
}
