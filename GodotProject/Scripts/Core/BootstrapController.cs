using Godot;
using System.Threading.Tasks;

namespace FutbolJuego.Core
{
    public partial class BootstrapController : Control
    {
        [Export] private float splashDuration = 1.5f;

        public override async void _Ready()
        {
            await Task.Delay((int)(splashDuration * 1000));
            SceneNavigator.Instance?.GoToMainMenu();
        }
    }
}
