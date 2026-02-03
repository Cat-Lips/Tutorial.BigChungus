using System;
using Godot;

namespace Game;

public partial class UI : CanvasLayer
{
    #region Private

    private Label FinalScore => field ??= (Label)GetNode("%FinalScore");
    private Label CurrentScore => field ??= (Label)GetNode("%CurrentScore");
    private Button PlayButton => field ??= (Button)GetNode("%PlayButton");
    private Button ExitButton => field ??= (Button)GetNode("%ExitButton");
    private Control StartScreen => field ??= (Container)GetNode("Start");
    private Control ScoreScreen => field ??= (Container)GetNode("Score");

    #endregion

    public event Action StartGame;

    public void SetScore(int score)
        => CurrentScore.Text = $"Score: {score}";

    public void SetColor(in Color color)
        => CurrentScore.AddThemeColorOverride("font_color", color);

    public void ResetColor()
        => CurrentScore.RemoveThemeColorOverride("font_color");

    public void SetGameOver(int score)
    {
        StartScreen.Visible = true;
        ScoreScreen.Visible = false;
        FinalScore.Text = $"Final Score: {score}";
    }

    #region Godot

    public sealed override void _Ready()
    {
        PlayButton.Pressed += OnPlay;
        ExitButton.Pressed += OnExit;

        void OnPlay()
        {
            StartScreen.Visible = false;
            ScoreScreen.Visible = true;
            StartGame?.Invoke();
            ResetColor();
            SetScore(0);
        }

        void OnExit()
            => GetTree().Quit();
    }

    #endregion
}
