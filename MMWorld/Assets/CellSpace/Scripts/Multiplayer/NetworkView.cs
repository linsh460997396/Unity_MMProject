using System.Runtime.CompilerServices;
using UnityEngine;

//     The network view is the binding material of multiplayer games.
public sealed class NetworkView : Behaviour
{
    //
    // 摘要:
    //     The component the network view is observing.
    public extern Component observed
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // 摘要:
    //     The type of NetworkStateSynchronization set for this network view.
    public extern NetworkStateSynchronization stateSynchronization
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // 摘要:
    //     The ViewID of this network view.
    public NetworkViewID viewID
    {
        get
        {
            Internal_GetViewID(out var result);
            return result;
        }
        set
        {
            Internal_SetViewID(value);
        }
    }

    //
    // 摘要:
    //     The network group number of this network view.
    public extern int group
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        get;
        [MethodImpl(MethodImplOptions.InternalCall)]
        [GeneratedByOldBindingsGenerator]
        set;
    }

    //
    // 摘要:
    //     Is the network view controlled by this object?
    public bool isMine => viewID.isMine;

    //
    // 摘要:
    //     The NetworkPlayer who owns this network view.
    public NetworkPlayer owner => viewID.owner;

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void Internal_RPC(NetworkView view, string name, RPCMode mode, object[] args);

    private static void Internal_RPC_Target(NetworkView view, string name, NetworkPlayer target, object[] args)
    {
        INTERNAL_CALL_Internal_RPC_Target(view, name, ref target, args);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_CALL_Internal_RPC_Target(NetworkView view, string name, ref NetworkPlayer target, object[] args);

    //
    // 摘要:
    //     Call a RPC function on all connected peers.
    //
    // 参数:
    //   name:
    //
    //   mode:
    //
    //   args:
    public void RPC(string name, RPCMode mode, params object[] args)
    {
        Internal_RPC(this, name, mode, args);
    }

    //
    // 摘要:
    //     Call a RPC function on a specific player.
    //
    // 参数:
    //   name:
    //
    //   target:
    //
    //   args:
    public void RPC(string name, NetworkPlayer target, params object[] args)
    {
        Internal_RPC_Target(this, name, target, args);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private extern void Internal_GetViewID(out NetworkViewID viewID);

    private void Internal_SetViewID(NetworkViewID viewID)
    {
        INTERNAL_CALL_Internal_SetViewID(this, ref viewID);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern void INTERNAL_CALL_Internal_SetViewID(NetworkView self, ref NetworkViewID viewID);

    //
    // 摘要:
    //     Set the scope of the network view in relation to a specific network player.
    //
    // 参数:
    //   player:
    //
    //   relevancy:
    public bool SetScope(NetworkPlayer player, bool relevancy)
    {
        return INTERNAL_CALL_SetScope(this, ref player, relevancy);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern bool INTERNAL_CALL_SetScope(NetworkView self, ref NetworkPlayer player, bool relevancy);

    //
    // 摘要:
    //     Find a network view based on a NetworkViewID.
    //
    // 参数:
    //   viewID:
    public static NetworkView Find(NetworkViewID viewID)
    {
        return INTERNAL_CALL_Find(ref viewID);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [GeneratedByOldBindingsGenerator]
    private static extern NetworkView INTERNAL_CALL_Find(ref NetworkViewID viewID);
}