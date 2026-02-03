using System.Collections.Generic;
using Nebula;

namespace Game;

public partial class ScoreManager : NetNode
{
    public Pellets Pellets { get; set; }
    public List<Player> Players { get; } = [];

    public sealed override void _NetworkProcess(int _)
    {
        if (!Network.IsServer) return;

        CheckPelletCollisions();
        CheckPlayerCollisions();

        void CheckPelletCollisions()
        {
            Players.ForEach(player =>
            {
                var playerPos = player.GetWorldPosition();
                var playerRadius = player.GetCollisionRadius();

                for (var i = 0; i < Pellets.PelletPositions.Length; ++i)
                {
                    var pelletPos = Pellets.PelletPositions[i];
                    var sqrDist = pelletPos.DistanceSquaredTo(playerPos);
                    if (sqrDist < playerRadius)
                    {
                        ++player.Score;
                        Pellets.Respawn(i);
                    }
                }
            });
        }

        void CheckPlayerCollisions()
        {
            CheckCollisions(out var dead);
            DespawnTheDead();

            void CheckCollisions(out HashSet<Player> dead)
            {
                dead = [];
                for (var i = 0; i < Players.Count; ++i)
                {
                    var player1 = Players[i];
                    if (player1.MercyTime > 0) continue;
                    if (dead.Contains(player1)) continue;

                    var pos1 = player1.GetWorldPosition();
                    var radius1 = player1.GetCollisionRadius();

                    for (var j = i + 1; j < Players.Count; ++j)
                    {
                        var player2 = Players[j];
                        if (player2.MercyTime > 0) continue;
                        if (dead.Contains(player2)) continue;

                        var pos2 = player2.GetWorldPosition();
                        var radius2 = player2.GetCollisionRadius();

                        var sqrDist = pos1.DistanceSquaredTo(pos2);
                        if (sqrDist < radius1 + radius2)
                        {
                            var (bigger, smaller) = Compare(player1, player2);
                            bigger.Score += smaller.Score / 2;
                            dead.Add(smaller);
                        }
                    }
                }
            }

            void DespawnTheDead()
            {
                foreach (var player in dead)
                {
                    player.Network.Despawn();
                    Players.Remove(player);
                }
            }

            static (Player Bigger, Player Smaller) Compare(Player player1, Player player2)
                => player1.Score >= player2.Score ? (player1, player2) : (player2, player1);
        }
    }
}
