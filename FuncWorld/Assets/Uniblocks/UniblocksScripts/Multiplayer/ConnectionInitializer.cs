using UnityEngine;

namespace Uniblocks
{
    //核心组件用法：Unity中随便新建一个空对象，挂载脚本

    /// <summary>
    /// 连接初始化（核心组件N0.3）
    /// </summary>
    public class ConnectionInitializer : MonoBehaviour
    {
        /// <summary>
        /// 网络配置下的联合体素块预制体
        /// </summary>
        public GameObject UniblocksNetworkPrefab;

        /// <summary>
        /// 唤醒时链接到服务器的状态开关
        /// </summary>
        public bool ConnectToServerOnAwake;

        /// <summary>
        /// 开始时链接到服务器的状态开关
        /// </summary>
        public bool StartServerOnAwake;

        /// <summary>
        /// 服务器IP
        /// </summary>
        public string ServerIP; 

        /// <summary>
        /// 服务器密码
        /// </summary>
        public string ServerPassword;

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port; 

        /// <summary>
        /// 服务器最大连接数
        /// </summary>
        public int MaxConnections;

        /// <summary>
        /// Network Address Translation（NAT，网络地址转换）功能。NAT 是一种网络技术，它允许将多个私有IP地址映射到一个公共IP地址，以实现网络安全和IP地址的有效利用。
        /// 在某些应用程序中，例如网络游戏或多玩家在线游戏，可能需要启用NAT功能以实现网络连接。在这种情况下，UseNat 字段可用于控制NAT功能的开关。
        /// </summary>
        public bool UseNat;

        ////在每帧更新后调用此方法
        void LateUpdate()
        {
            //检查是否要在启动时启动服务器或连接到服务器，并相应地调用StartServer()或ConnectToServer()方法
            if (StartServerOnAwake)
            {
                StartServer();
            }
            else if (ConnectToServerOnAwake)
            {
                ConnectToServer();
            }
            //将脚本禁用以避免重复执行
            this.enabled = false;
        }

        /// <summary>
        /// 使用指定的端口和密码初始化服务器。它使用Network.InitializeServer()方法在指定的端口上启动服务器，并指定最大连接数和是否使用 NAT 穿越
        /// </summary>
        public void StartServer()
        {
            Network.InitializeServer(MaxConnections, Port, UseNat);
        }

        /// <summary>
        /// 当服务器初始化完成后将调用此方法，它使用Network.Instantiate()方法在服务器上实例化一个名为“UniblocksNetworkPrefab”的对象
        /// </summary>
        void OnServerInitialized()
        {
            Network.Instantiate(UniblocksNetworkPrefab, transform.position, transform.rotation, 0); // instantiate UniblocksNetwork
        }

        /// <summary>
        /// 用指定的IP地址、端口和密码将客户端连接到服务器：执行Network.Connect(ServerIP, Port, ServerPassword)
        /// </summary>
        public void ConnectToServer()
        {
            Network.Connect(ServerIP, Port, ServerPassword);
        }

    }

}
