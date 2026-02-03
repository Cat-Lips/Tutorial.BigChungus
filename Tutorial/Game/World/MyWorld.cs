using Nebula;

namespace Game;

public partial class MyWorld : NetNode3D
{
    #region Private

    internal UI UI => field ??= (UI)GetNode("UI");
    internal BG BG => field ??= (BG)GetNode("BG");
    internal RPC RPC => field ??= (RPC)GetNode("RPC");
    internal Pellets Pellets => field ??= (Pellets)GetNode("Pellets");
    internal ScoreManager Scoring => field ??= (ScoreManager)GetNode("Scoring");

    #endregion

    #region Godot

    public sealed override void _Ready()
    {
        InitClient();
        InitServer();

        void InitClient()
        {
            if (!Network.IsClient) return;

            UI.StartGame += RPC.JoinGame;
            Player.PlayerReady += OnPlayerReady;

            void OnPlayerReady(Player player)
            {
                UI.SetColor(player.Color);
                player.ScoreChanged += UI.SetScore;
                player.PlayerDead += () => UI.SetGameOver(player.Score);
            }
        }

        void InitServer()
        {
            if (!Network.IsServer) return;

            Scoring.Pellets = Pellets;
            Player.PlayerReady += Scoring.Players.Add;
        }
    }

    #endregion
}
