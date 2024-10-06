using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[System.Flags]
public enum EInputButton
{
    Jump = 0
}

public struct GameplayInput : INetworkInput
{
    public Vector2 MoveDirection;
    public NetworkButtons Buttons;
}

/// <summary>
/// PlayerInput handles accumulating player input from Unity and passes the accumulated input to Fusion.
/// </summary>
public sealed class FusionPlayerInput : NetworkBehaviour, IBeforeUpdate, IAfterTick, INetworkRunnerCallbacks
{
    public NetworkButtons PreviousButtons => _previousButtons;

    // PRIVATE MEMBERS
    [Networked]
    private NetworkButtons _previousButtons { get; set; }

    private GameplayInput _accumulatedInput;

    public InputActionReference jumpReference;
    public InputActionReference moveReference;

    // NetworkBehaviour INTERFACE

    void OnEnable()
    {
        jumpReference.action.Enable();
        moveReference.action.Enable();
    }

    public override void Spawned()
    {
        // Only local player needs networked properties (previous input buttons).
        // This saves network traffic by not synchronizing networked properties to other clients except local player.
        ReplicateToAll(false);
        ReplicateTo(Object.InputAuthority, true);

        if (HasInputAuthority == false)
            return;

        // Register to Fusion input poll callback
        if (Object.HasInputAuthority)
        {
            Runner.AddCallbacks(this);
            Debug.Log("CALLBACKS CALLED ON PLAYER INPUT");
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (runner == null)
            return;

        var networkEvents = runner.GetComponent<NetworkEvents>();
        if (networkEvents != null)
        {
            networkEvents.OnInput.RemoveListener(OnInput);
        }
    }

    // IBeforeUpdate INTERFACE
    void IBeforeUpdate.BeforeUpdate()
    {
        // This method is called BEFORE ANY FixedUpdateNetwork() and is used to accumulate input from Keyboard/Mouse.
        // Input accumulation is mandatory - this method is called multiple times before new forward FixedUpdateNetwork() - common if rendering speed is faster than Fusion simulation.
        if (HasInputAuthority == false)
            return;

        // Input is tracked only if the cursor is locked and runner should provide input
        if (Runner.ProvideInput == false)
        {
            _accumulatedInput = default;
            return;
        }

        var moveDirection = moveReference.action.ReadValue<Vector2>();
        _accumulatedInput.MoveDirection = moveDirection.normalized;
        if (moveDirection.normalized != Vector2.zero)
        {
            Debug.Log("$BeforeUpdate: FUSION PLAYER INPUT IS DETECTING MOVEMENT: " + moveDirection.normalized);
        }


        _accumulatedInput.Buttons.Set(EInputButton.Jump, jumpReference.action.IsPressed());
        if (jumpReference.action.IsPressed())
        {
            Debug.Log("$BeforeUpdate: FUSION PLAYER INPUT IS DETECTING JUMP BUTTON PRESS: " + jumpReference.action.IsPressed());
        }
    }

    // IAfterTick INTERFACE

    void IAfterTick.AfterTick()
    {
        _previousButtons = GetInput<GameplayInput>().GetValueOrDefault().Buttons;
    }

    public void OnInput(NetworkRunner runner, NetworkInput networkInput)
    {
        // Fusion polls accumulated input. This callback can be executed multiple times in a row if there is a performance spike.
        networkInput.Set(_accumulatedInput);
        if (_accumulatedInput.MoveDirection != Vector2.zero)
        {
            Debug.Log("$OnInput: FUSION PLAYER INPUT IS DETECTING MOVEMENT: " + _accumulatedInput.MoveDirection);
        }
        if (_accumulatedInput.Buttons.IsSet(EInputButton.Jump))
        {
            Debug.Log("$OnInput: FUSION PLAYER INPUT IS DETECTING JUMP BUTTON PRESS: " + _accumulatedInput.Buttons.IsSet(EInputButton.Jump));
        }
    }

    #region UnusedCallbacks
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    #endregion
}