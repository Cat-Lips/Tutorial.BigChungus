using System.Collections.Generic;
using Godot;
using Nebula;
using static Nebula.NetFunction;

namespace Game;

public partial class RPC : NetNode
{
    private readonly Dictionary<UUID, Node> MyPlayers = [];

    [NetFunction(Source = NetworkSources.Client, ExecuteOnCaller = false)]
    public void JoinGame()
    {
        var caller = Network.CurrentWorld.NetFunctionContext.Caller;
        var callerId = NetRunner.Instance.GetPeerId(caller);

        if (!IsExistingPlayer())
            CreateNewPlayer();

        bool IsExistingPlayer()
            => MyPlayers.TryGetValue(callerId, out var player) && IsInstanceValid(player);

        void CreateNewPlayer()
        {
            var player = World.NewPlayer();
            Network.CurrentWorld.Spawn(player, inputAuthority: caller);
            MyPlayers[callerId] = player;
        }
    }
}
