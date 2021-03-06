﻿#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Cluster.Extensions
* 项目描述 ：
* 类 名 称 ：HostAndPort
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



namespace etcd.Provider.Cluster.Extensions
{
    /* ============================================================================== 
* 功能描述：HostAndPort 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
  public  class HostAndPort
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public string Flage { get; set; }
    }
}
