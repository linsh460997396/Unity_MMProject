using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;
using UnityEngineInternal;
using Object = UnityEngine.Object;

/// <summary>
/// The NetworkPlayer is a data structure with which you can locate another player over the network.
/// </summary>
[RequiredByNativeCode(Optional = true)]
public struct NetworkPlayer
{
    internal int index;

    //
    // ժҪ:
    //     The IP address of this player.
    public string ipAddress
    {
        get
        {
            if (index == Internal_GetPlayerIndex())
            {
                return Internal_GetLocalIP();
            }

            return Internal_GetIPAddress(index);
        }
    }

    //
    // ժҪ:
    //     The port of this player.
    public int port
    {
        get
        {
            if (index == Internal_GetPlayerIndex())
            {
                return Internal_GetLocalPort();
            }

            return Internal_GetPort(index);
        }
    }

    //
    // ժҪ:
    //     The GUID for this player, used when connecting with NAT punchthrough.
    public string guid
    {
        get
        {
            if (index == Internal_GetPlayerIndex())
            {
                return Internal_GetLocalGUID();
            }

            return Internal_GetGUID(index);
        }
    }

    //
    // ժҪ:
    //     Returns the external IP address of the network interface.
    public string externalIP => Internal_GetExternalIP();

    //
    // ժҪ:
    //     Returns the external port of the network interface.
    public int externalPort => Internal_GetExternalPort();

    internal static NetworkPlayer unassigned
    {
        get
        {
            NetworkPlayer result = default(NetworkPlayer);
            result.index = -1;
            return result;
        }
    }

    public NetworkPlayer(string ip, int port)
    {
        Debug.LogError("Not yet implemented");
        index = 0;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern string Internal_GetIPAddress(int index);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern int Internal_GetPort(int index);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern string Internal_GetExternalIP();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern int Internal_GetExternalPort();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern string Internal_GetLocalIP();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern int Internal_GetLocalPort();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern int Internal_GetPlayerIndex();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern string Internal_GetGUID(int index);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern string Internal_GetLocalGUID();

    public static bool operator ==(NetworkPlayer lhs, NetworkPlayer rhs)
    {
        return lhs.index == rhs.index;
    }

    public static bool operator !=(NetworkPlayer lhs, NetworkPlayer rhs)
    {
        return lhs.index != rhs.index;
    }

    public override int GetHashCode()
    {
        return index.GetHashCode();
    }

    public override bool Equals(object other)
    {
        if (!(other is NetworkPlayer networkPlayer))
        {
            return false;
        }

        return networkPlayer.index == index;
    }

    //
    // ժҪ:
    //     Returns the index number for this network player.
    public override string ToString()
    {
        return index.ToString();
    }
}

[VisibleToOtherModules]
internal class GeneratedByOldBindingsGeneratorAttribute : Attribute
{
}

/// <summary>
/// The network class is at the heart of the network implementation and provides the core functions.
/// </summary>
public sealed class Network
{
    //
    // ժҪ:
    //     Set the password for the server (for incoming connections).
    public static extern string incomingPassword
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     Set the log level for network messages (default is Off).
    public static extern NetworkLogLevel logLevel
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     All connected players.
    public static extern NetworkPlayer[] connections
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
    }

    //
    // ժҪ:
    //     Get the local NetworkPlayer instance.
    public static NetworkPlayer player
    {
        get
        {
            NetworkPlayer result = default(NetworkPlayer);
            result.index = Internal_GetPlayer();
            return result;
        }
    }

    //
    // ժҪ:
    //     Returns true if your peer type is client.
    public static extern bool isClient
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
    }

    //
    // ժҪ:
    //     Returns true if your peer type is server.
    public static extern bool isServer
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
    }

    //
    // ժҪ:
    //     The status of the peer type, i.e. if it is disconnected, connecting, server or
    //     client.
    public static extern NetworkPeerType peerType
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
    }

    //
    // ժҪ:
    //     The default send rate of network updates for all Network Views.
    public static extern float sendRate
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     Enable or disable the processing of network messages.
    public static extern bool isMessageQueueRunning
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     Get the current network time (seconds).
    public static double time
    {
        get
        {
            Internal_GetTime(out var t);
            return t;
        }
    }

    //
    // ժҪ:
    //     Get or set the minimum number of ViewID numbers in the ViewID pool given to clients
    //     by the server.
    public static extern int minimumAllocatableViewIDs
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    [Obsolete("No longer needed. This is now explicitly set in the InitializeServer function call. It is implicitly set when calling Connect depending on if an IP/port combination is used (useNat=false) or a GUID is used(useNat=true).")]
    public static extern bool useNat
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     The IP address of the NAT punchthrough facilitator.
    public static extern string natFacilitatorIP
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     The port of the NAT punchthrough facilitator.
    public static extern int natFacilitatorPort
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     The IP address of the connection tester used in Network.TestConnection.
    public static extern string connectionTesterIP
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     The port of the connection tester used in Network.TestConnection.
    public static extern int connectionTesterPort
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     Set the maximum amount of connections/players allowed.
    public static extern int maxConnections
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     The IP address of the proxy server.
    public static extern string proxyIP
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     The port of the proxy server.
    public static extern int proxyPort
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     Indicate if proxy support is needed, in which case traffic is relayed through
    //     the proxy server.
    public static extern bool useProxy
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     Set the proxy server password.
    public static extern string proxyPassword
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // ժҪ:
    //     Initialize the server.
    //
    // ����:
    //   connections:
    //
    //   listenPort:
    //
    //   useNat:
    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    public static extern NetworkConnectionError InitializeServer(int connections, int listenPort, bool useNat);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern NetworkConnectionError Internal_InitializeServerDeprecated(int connections, int listenPort);

    //
    // ժҪ:
    //     Initialize the server.
    //
    // ����:
    //   connections:
    //
    //   listenPort:
    //
    //   useNat:
    [Obsolete("Use the IntializeServer(connections, listenPort, useNat) function instead")]
    public static NetworkConnectionError InitializeServer(int connections, int listenPort)
    {
        return Internal_InitializeServerDeprecated(connections, listenPort);
    }

    //
    // ժҪ:
    //     Initializes security layer.
    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    public static extern void InitializeSecurity();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern NetworkConnectionError Internal_ConnectToSingleIP(string IP, int remotePort, int localPort, [UnityEngine.Internal.DefaultValue("\"\"")] string password);

    [ExcludeFromDocs]
    private static NetworkConnectionError Internal_ConnectToSingleIP(string IP, int remotePort, int localPort)
    {
        string password = "";
        return Internal_ConnectToSingleIP(IP, remotePort, localPort, password);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern NetworkConnectionError Internal_ConnectToGuid(string guid, string password);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern NetworkConnectionError Internal_ConnectToIPs(string[] IP, int remotePort, int localPort, [UnityEngine.Internal.DefaultValue("\"\"")] string password);

    [ExcludeFromDocs]
    private static NetworkConnectionError Internal_ConnectToIPs(string[] IP, int remotePort, int localPort)
    {
        string password = "";
        return Internal_ConnectToIPs(IP, remotePort, localPort, password);
    }

    //
    // ժҪ:
    //     Connect to the specified host (ip or domain name) and server port.
    //
    // ����:
    //   IP:
    //
    //   remotePort:
    //
    //   password:
    [ExcludeFromDocs]
    public static NetworkConnectionError Connect(string IP, int remotePort)
    {
        string password = "";
        return Connect(IP, remotePort, password);
    }

    //
    // ժҪ:
    //     Connect to the specified host (ip or domain name) and server port.
    //
    // ����:
    //   IP:
    //
    //   remotePort:
    //
    //   password:
    public static NetworkConnectionError Connect(string IP, int remotePort, [UnityEngine.Internal.DefaultValue("\"\"")] string password)
    {
        return Internal_ConnectToSingleIP(IP, remotePort, 0, password);
    }

    //
    // ժҪ:
    //     This function is exactly like Network.Connect but can accept an array of IP addresses.
    //
    //
    // ����:
    //   IPs:
    //
    //   remotePort:
    //
    //   password:
    [ExcludeFromDocs]
    public static NetworkConnectionError Connect(string[] IPs, int remotePort)
    {
        string password = "";
        return Connect(IPs, remotePort, password);
    }

    //
    // ժҪ:
    //     This function is exactly like Network.Connect but can accept an array of IP addresses.
    //
    //
    // ����:
    //   IPs:
    //
    //   remotePort:
    //
    //   password:
    public static NetworkConnectionError Connect(string[] IPs, int remotePort, [UnityEngine.Internal.DefaultValue("\"\"")] string password)
    {
        return Internal_ConnectToIPs(IPs, remotePort, 0, password);
    }

    //
    // ժҪ:
    //     Connect to a server GUID. NAT punchthrough can only be performed this way.
    //
    // ����:
    //   GUID:
    //
    //   password:
    [ExcludeFromDocs]
    public static NetworkConnectionError Connect(string GUID)
    {
        string password = "";
        return Connect(GUID, password);
    }

    //
    // ժҪ:
    //     Connect to a server GUID. NAT punchthrough can only be performed this way.
    //
    // ����:
    //   GUID:
    //
    //   password:
    public static NetworkConnectionError Connect(string GUID, [UnityEngine.Internal.DefaultValue("\"\"")] string password)
    {
        return Internal_ConnectToGuid(GUID, password);
    }

    //
    // ժҪ:
    //     Connect to the host represented by a HostData structure returned by the Master
    //     Server.
    //
    // ����:
    //   hostData:
    //
    //   password:
    [ExcludeFromDocs]
    public static NetworkConnectionError Connect(HostData hostData)
    {
        string password = "";
        return Connect(hostData, password);
    }

    //
    // ժҪ:
    //     Connect to the host represented by a HostData structure returned by the Master
    //     Server.
    //
    // ����:
    //   hostData:
    //
    //   password:
    public static NetworkConnectionError Connect(HostData hostData, [UnityEngine.Internal.DefaultValue("\"\"")] string password)
    {
        if (hostData == null)
        {
            throw new NullReferenceException();
        }

        if (hostData.guid.Length > 0 && hostData.useNat)
        {
            return Connect(hostData.guid, password);
        }

        return Connect(hostData.ip, hostData.port, password);
    }

    //
    // ժҪ:
    //     Close all open connections and shuts down the network interface.
    //
    // ����:
    //   timeout:
    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    public static extern void Disconnect([UnityEngine.Internal.DefaultValue("200")] int timeout);

    [ExcludeFromDocs]
    public static void Disconnect()
    {
        int timeout = 200;
        Disconnect(timeout);
    }

    //
    // ժҪ:
    //     Close the connection to another system.
    //
    // ����:
    //   target:
    //
    //   sendDisconnectionNotification:
    public static void CloseConnection(NetworkPlayer target, bool sendDisconnectionNotification)
    {
        INTERNAL_CALL_CloseConnection(ref target, sendDisconnectionNotification);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_CALL_CloseConnection(ref NetworkPlayer target, bool sendDisconnectionNotification);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern int Internal_GetPlayer();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void Internal_AllocateViewID(out NetworkViewID viewID);

    //
    // ժҪ:
    //     Query for the next available network view ID number and allocate it (reserve).
    public static NetworkViewID AllocateViewID()
    {
        Internal_AllocateViewID(out var viewID);
        return viewID;
    }

    //
    // ժҪ:
    //     Network instantiate a prefab.
    //
    // ����:
    //   prefab:
    //
    //   position:
    //
    //   rotation:
    //
    //   group:
    [TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
    public static Object Instantiate(Object prefab, Vector3 position, Quaternion rotation, int group)
    {
        return INTERNAL_CALL_Instantiate(prefab, ref position, ref rotation, group);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern Object INTERNAL_CALL_Instantiate(Object prefab, ref Vector3 position, ref Quaternion rotation, int group);

    //
    // ժҪ:
    //     Destroy the object associated with this view ID across the network.
    //
    // ����:
    //   viewID:
    public static void Destroy(NetworkViewID viewID)
    {
        INTERNAL_CALL_Destroy(ref viewID);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_CALL_Destroy(ref NetworkViewID viewID);

    //
    // ժҪ:
    //     Destroy the object across the network.
    //
    // ����:
    //   gameObject:
    public static void Destroy(GameObject gameObject)
    {
        if (gameObject != null)
        {
            NetworkView component = gameObject.GetComponent<NetworkView>();
            if (component != null)
            {
                Destroy(component.viewID);
            }
            else
            {
                Debug.LogError("Couldn'transform destroy game object because no network view is attached to it.", gameObject);
            }
        }
    }

    //
    // ժҪ:
    //     Destroy all the objects based on view IDs belonging to this player.
    //
    // ����:
    //   playerID:
    public static void DestroyPlayerObjects(NetworkPlayer playerID)
    {
        INTERNAL_CALL_DestroyPlayerObjects(ref playerID);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_CALL_DestroyPlayerObjects(ref NetworkPlayer playerID);

    private static void Internal_RemoveRPCs(NetworkPlayer playerID, NetworkViewID viewID, uint channelMask)
    {
        INTERNAL_CALL_Internal_RemoveRPCs(ref playerID, ref viewID, channelMask);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_CALL_Internal_RemoveRPCs(ref NetworkPlayer playerID, ref NetworkViewID viewID, uint channelMask);

    //
    // ժҪ:
    //     Remove all RPC functions which belong to this player ID.
    //
    // ����:
    //   playerID:
    public static void RemoveRPCs(NetworkPlayer playerID)
    {
        Internal_RemoveRPCs(playerID, NetworkViewID.unassigned, uint.MaxValue);
    }

    //
    // ժҪ:
    //     Remove all RPC functions which belong to this player ID and were sent based on
    //     the given group.
    //
    // ����:
    //   playerID:
    //
    //   group:
    public static void RemoveRPCs(NetworkPlayer playerID, int group)
    {
        Internal_RemoveRPCs(playerID, NetworkViewID.unassigned, (uint)(1 << group));
    }

    //
    // ժҪ:
    //     Remove the RPC function calls accociated with this view ID number.
    //
    // ����:
    //   viewID:
    public static void RemoveRPCs(NetworkViewID viewID)
    {
        Internal_RemoveRPCs(NetworkPlayer.unassigned, viewID, uint.MaxValue);
    }

    //
    // ժҪ:
    //     Remove all RPC functions which belong to given group number.
    //
    // ����:
    //   group:
    public static void RemoveRPCsInGroup(int group)
    {
        Internal_RemoveRPCs(NetworkPlayer.unassigned, NetworkViewID.unassigned, (uint)(1 << group));
    }

    //
    // ժҪ:
    //     Set the level prefix which will then be prefixed to all network ViewID numbers.
    //
    //
    // ����:
    //   prefix:
    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    public static extern void SetLevelPrefix(int prefix);

    //
    // ժҪ:
    //     The last ping time to the given player in milliseconds.
    //
    // ����:
    //   player:
    public static int GetLastPing(NetworkPlayer player)
    {
        return INTERNAL_CALL_GetLastPing(ref player);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern int INTERNAL_CALL_GetLastPing(ref NetworkPlayer player);

    //
    // ժҪ:
    //     The last average ping time to the given player in milliseconds.
    //
    // ����:
    //   player:
    public static int GetAveragePing(NetworkPlayer player)
    {
        return INTERNAL_CALL_GetAveragePing(ref player);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern int INTERNAL_CALL_GetAveragePing(ref NetworkPlayer player);

    //
    // ժҪ:
    //     Enable or disables the reception of messages in a specific group number from
    //     a specific player.
    //
    // ����:
    //   player:
    //
    //   group:
    //
    //   enabled:
    public static void SetReceivingEnabled(NetworkPlayer player, int group, bool enabled)
    {
        INTERNAL_CALL_SetReceivingEnabled(ref player, group, enabled);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_CALL_SetReceivingEnabled(ref NetworkPlayer player, int group, bool enabled);

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void Internal_SetSendingGlobal(int group, bool enabled);

    private static void Internal_SetSendingSpecific(NetworkPlayer player, int group, bool enabled)
    {
        INTERNAL_CALL_Internal_SetSendingSpecific(ref player, group, enabled);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_CALL_Internal_SetSendingSpecific(ref NetworkPlayer player, int group, bool enabled);

    //
    // ժҪ:
    //     Enables or disables transmission of messages and RPC calls on a specific network
    //     group number.
    //
    // ����:
    //   group:
    //
    //   enabled:
    public static void SetSendingEnabled(int group, bool enabled)
    {
        Internal_SetSendingGlobal(group, enabled);
    }

    //
    // ժҪ:
    //     Enable or disable transmission of messages and RPC calls based on target network
    //     player as well as the network group.
    //
    // ����:
    //   player:
    //
    //   group:
    //
    //   enabled:
    public static void SetSendingEnabled(NetworkPlayer player, int group, bool enabled)
    {
        Internal_SetSendingSpecific(player, group, enabled);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void Internal_GetTime(out double t);

    //
    // ժҪ:
    //     Test this machines network connection.
    //
    // ����:
    //   forceTest:
    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    public static extern ConnectionTesterStatus TestConnection([UnityEngine.Internal.DefaultValue("false")] bool forceTest);

    [ExcludeFromDocs]
    public static ConnectionTesterStatus TestConnection()
    {
        bool forceTest = false;
        return TestConnection(forceTest);
    }

    //
    // ժҪ:
    //     Test the connection specifically for NAT punch-through connectivity.
    //
    // ����:
    //   forceTest:
    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    public static extern ConnectionTesterStatus TestConnectionNAT([UnityEngine.Internal.DefaultValue("false")] bool forceTest);

    [ExcludeFromDocs]
    public static ConnectionTesterStatus TestConnectionNAT()
    {
        bool forceTest = false;
        return TestConnectionNAT(forceTest);
    }

    //
    // ժҪ:
    //     Check if this machine has a public IP address.
    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    public static extern bool HavePublicAddress();
}

/// <summary>
/// The various test results the connection tester may return with.
/// </summary>
public enum ConnectionTesterStatus
{
    //
    // ժҪ:
    //     Some unknown error occurred.
    Error = -2,
    //
    // ժҪ:
    //     Test result undetermined, still in progress.
    Undetermined,
    [Obsolete("No longer returned, use newer connection tester enums instead.")]
    PrivateIPNoNATPunchthrough,
    [Obsolete("No longer returned, use newer connection tester enums instead.")]
    PrivateIPHasNATPunchThrough,
    //
    // ժҪ:
    //     Public IP address detected and game listen port is accessible to the internet.
    PublicIPIsConnectable,
    //
    // ժҪ:
    //     Public IP address detected but the port is not connectable from the internet.
    PublicIPPortBlocked,
    //
    // ժҪ:
    //     Public IP address detected but server is not initialized and no port is listening.
    PublicIPNoServerStarted,
    //
    // ժҪ:
    //     Port-restricted NAT type, can do NAT punchthrough to everyone except symmetric.
    LimitedNATPunchthroughPortRestricted,
    //
    // ժҪ:
    //     Symmetric NAT type, cannot do NAT punchthrough to other symmetric types nor port
    //     restricted type.
    LimitedNATPunchthroughSymmetric,
    //
    // ժҪ:
    //     Full cone type, NAT punchthrough fully supported.
    NATpunchthroughFullCone,
    //
    // ժҪ:
    //     Address-restricted cone type, NAT punchthrough fully supported.
    NATpunchthroughAddressRestrictedCone
}

/// <summary>
/// This is the data structure for holding individual host information.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[RequiredByNativeCode(Optional = true)]
public sealed class HostData
{
    private int m_Nat;

    private string m_GameType;

    private string m_GameName;

    private int m_ConnectedPlayers;

    private int m_PlayerLimit;

    private string[] m_IP;

    private int m_Port;

    private int m_PasswordProtected;

    private string m_Comment;

    private string m_GUID;

    //
    // ժҪ:
    //     Does this server require NAT punchthrough?
    public bool useNat
    {
        get
        {
            return m_Nat != 0;
        }
        set
        {
            m_Nat = (value ? 1 : 0);
        }
    }

    //
    // ժҪ:
    //     The type of the game (like "MyUniqueGameType").
    public string gameType
    {
        get
        {
            return m_GameType;
        }
        set
        {
            m_GameType = value;
        }
    }

    //
    // ժҪ:
    //     The name of the game (like John Doe's Game).
    public string gameName
    {
        get
        {
            return m_GameName;
        }
        set
        {
            m_GameName = value;
        }
    }

    //
    // ժҪ:
    //     Currently connected players.
    public int connectedPlayers
    {
        get
        {
            return m_ConnectedPlayers;
        }
        set
        {
            m_ConnectedPlayers = value;
        }
    }

    //
    // ժҪ:
    //     Maximum players limit.
    public int playerLimit
    {
        get
        {
            return m_PlayerLimit;
        }
        set
        {
            m_PlayerLimit = value;
        }
    }

    //
    // ժҪ:
    //     Server IP address.
    public string[] ip
    {
        get
        {
            return m_IP;
        }
        set
        {
            m_IP = value;
        }
    }

    //
    // ժҪ:
    //     Server port.
    public int port
    {
        get
        {
            return m_Port;
        }
        set
        {
            m_Port = value;
        }
    }

    //
    // ժҪ:
    //     Does the server require a password?
    public bool passwordProtected
    {
        get
        {
            return m_PasswordProtected != 0;
        }
        set
        {
            m_PasswordProtected = (value ? 1 : 0);
        }
    }

    //
    // ժҪ:
    //     A miscellaneous comment (can hold data).
    public string comment
    {
        get
        {
            return m_Comment;
        }
        set
        {
            m_Comment = value;
        }
    }

    //
    // ժҪ:
    //     The GUID of the host, needed when connecting with NAT punchthrough.
    public string guid
    {
        get
        {
            return m_GUID;
        }
        set
        {
            m_GUID = value;
        }
    }
}

/// <summary>
/// Possible status messages returned by Network.Connect and in MonoBehaviour.OnFailedToConnect|OnFailedToConnect in case the error was not immediate.
/// </summary>
public enum NetworkConnectionError
{
    //
    // ժҪ:
    //     No error occurred.
    NoError = 0,
    //
    // ժҪ:
    //     We presented an RSA public key which does not match what the system we connected
    //     to is using.
    RSAPublicKeyMismatch = 21,
    //
    // ժҪ:
    //     The server is using a password and has refused our connection because we did
    //     not set the correct password.
    InvalidPassword = 23,
    //
    // ժҪ:
    //     Connection attempt failed, possibly because of internal connectivity problems.
    ConnectionFailed = 15,
    //
    // ժҪ:
    //     The server is at full capacity, failed to connect.
    TooManyConnectedPlayers = 18,
    //
    // ժҪ:
    //     We are banned from the system we attempted to connect to (likely temporarily).
    ConnectionBanned = 22,
    //
    // ժҪ:
    //     We are already connected to this particular server (can happen after fast disconnect/reconnect).
    AlreadyConnectedToServer = 16,
    //
    // ժҪ:
    //     Cannot connect to two servers at once. Close the connection before connecting
    //     again.
    AlreadyConnectedToAnotherServer = -1,
    //
    // ժҪ:
    //     Internal error while attempting to initialize network interface. Socket possibly
    //     already in use.
    CreateSocketOrThreadFailure = -2,
    //
    // ժҪ:
    //     Incorrect parameters given to Connect function.
    IncorrectParameters = -3,
    //
    // ժҪ:
    //     No host target given in Connect.
    EmptyConnectTarget = -4,
    //
    // ժҪ:
    //     Client could not connect internally to same network NAT enabled server.
    InternalDirectConnectFailed = -5,
    //
    // ժҪ:
    //     The NAT target we are trying to connect to is not connected to the facilitator
    //     server.
    NATTargetNotConnected = 69,
    //
    // ժҪ:
    //     Connection lost while attempting to connect to NAT target.
    NATTargetConnectionLost = 71,
    //
    // ժҪ:
    //     NAT punchthrough attempt has failed. The cause could be a too restrictive NAT
    //     implementation on either endpoints.
    NATPunchthroughFailed = 73
}

/// <summary>
/// Describes different levels of log information the network layer supports.
/// </summary>
public enum NetworkLogLevel
{
    //
    // ժҪ:
    //     Only report errors, otherwise silent.
    Off = 0,
    //
    // ժҪ:
    //     Report informational messages like connectivity events.
    Informational = 1,
    //
    // ժҪ:
    //     Full debug level logging down to each individual message being reported.
    Full = 3
}

/// <summary>
/// Describes the status of the network interface peer type as returned by Network.peerType.
/// </summary>
public enum NetworkPeerType
{
    //
    // ժҪ:
    //     No client connection running. Server not initialized.
    Disconnected,
    //
    // ժҪ:
    //     Running as server.
    Server,
    //
    // ժҪ:
    //     Running as client.
    Client,
    //
    // ժҪ:
    //     Attempting to connect to a server.
    Connecting
}

/// <summary>
/// Attribute for setting up RPC functions.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[RequiredByNativeCode(Optional = true)]
public class RPC : Attribute
{
}

[VisibleToOtherModules]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, Inherited = false)]
internal class RequiredByNativeCodeAttribute : Attribute
{
    public string Name { get; set; }

    public bool Optional { get; set; }

    public RequiredByNativeCodeAttribute()
    {
    }

    public RequiredByNativeCodeAttribute(string name)
    {
        Name = name;
    }

    public RequiredByNativeCodeAttribute(bool optional)
    {
        Optional = optional;
    }

    public RequiredByNativeCodeAttribute(string name, bool optional)
    {
        Name = name;
        Optional = optional;
    }
}

[VisibleToOtherModules]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
internal class VisibleToOtherModulesAttribute : Attribute
{
    public VisibleToOtherModulesAttribute()
    {
    }

    public VisibleToOtherModulesAttribute(params string[] modules)
    {
    }
}

/// <summary>
/// Option for who will receive an RPC, used by NetworkView.RPC.
/// </summary>
public enum RPCMode
{
    //
    // ժҪ:
    //     Sends to the server only.
    Server = 0,
    //
    // ժҪ:
    //     Sends to everyone except the sender.
    Others = 1,
    //
    // ժҪ:
    //     Sends to everyone except the sender and adds to the buffer.
    OthersBuffered = 5,
    //
    // ժҪ:
    //     Sends to everyone.
    All = 2,
    //
    // ժҪ:
    //     Sends to everyone and adds to the buffer.
    AllBuffered = 6
}

/// <summary>
/// The NetworkViewID is a unique identifier for a network view instance in a multiplayer game.
/// </summary>
[RequiredByNativeCode(Optional = true)]
public struct NetworkViewID
{
    private int a;

    private int b;

    private int c;

    //
    // ժҪ:
    //     Represents an invalid network view ID.
    public static NetworkViewID unassigned
    {
        get
        {
            INTERNAL_get_unassigned(out var value);
            return value;
        }
    }

    //
    // ժҪ:
    //     True if instantiated by me.
    public bool isMine => Internal_IsMine(this);

    //
    // ժҪ:
    //     The NetworkPlayer who owns the NetworkView. Could be the server.
    public NetworkPlayer owner
    {
        get
        {
            Internal_GetOwner(this, out var player);
            return player;
        }
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_get_unassigned(out NetworkViewID value);

    internal static bool Internal_IsMine(NetworkViewID value)
    {
        return INTERNAL_CALL_Internal_IsMine(ref value);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern bool INTERNAL_CALL_Internal_IsMine(ref NetworkViewID value);

    internal static void Internal_GetOwner(NetworkViewID value, out NetworkPlayer player)
    {
        INTERNAL_CALL_Internal_GetOwner(ref value, out player);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_CALL_Internal_GetOwner(ref NetworkViewID value, out NetworkPlayer player);

    internal static string Internal_GetString(NetworkViewID value)
    {
        return INTERNAL_CALL_Internal_GetString(ref value);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern string INTERNAL_CALL_Internal_GetString(ref NetworkViewID value);

    internal static bool Internal_Compare(NetworkViewID lhs, NetworkViewID rhs)
    {
        return INTERNAL_CALL_Internal_Compare(ref lhs, ref rhs);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern bool INTERNAL_CALL_Internal_Compare(ref NetworkViewID lhs, ref NetworkViewID rhs);

    public static bool operator ==(NetworkViewID lhs, NetworkViewID rhs)
    {
        return Internal_Compare(lhs, rhs);
    }

    public static bool operator !=(NetworkViewID lhs, NetworkViewID rhs)
    {
        return !Internal_Compare(lhs, rhs);
    }

    public override int GetHashCode()
    {
        return a ^ b ^ c;
    }

    public override bool Equals(object other)
    {
        if (!(other is NetworkViewID rhs))
        {
            return false;
        }

        return Internal_Compare(this, rhs);
    }

    //
    // ժҪ:
    //     Returns a formatted string with details on this NetworkViewID.
    public override string ToString()
    {
        return Internal_GetString(this);
    }
}

/// <summary>
/// Different types of synchronization for the NetworkView component.
/// </summary>
public enum NetworkStateSynchronization
{
    //
    // ժҪ:
    //     No state data will be synchronized.
    Off,
    //
    // ժҪ:
    //     All packets are sent reliable and ordered.
    ReliableDeltaCompressed,
    //
    // ժҪ:
    //     Brute force unreliable state sending.
    Unreliable
}
