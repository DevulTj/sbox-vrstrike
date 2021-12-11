using Sandbox;
using Sandbox.UI;

namespace VRStrike;

[UseTemplate]
public class SpawnMenuPanel : WorldPanel
{
    public SpawnMenuPanel()
    {
		
    }

    public override void Tick()
    {
        base.Tick();

        if ( Local.Pawn is VRPlayerPawn player )
        {
            Transform = player.LeftHandEntity.Transform;

            //
            // Offsets
            //
            Rotation *= new Angles( -180, -90, 0 ).ToRotation();
            Position += Rotation.Forward * 2 + Rotation.Up * 3 - Rotation.Left * 5;
            WorldScale = 0.1f;
            Scale = 2.0f;

            PanelBounds = new Rect( 0, 0, 1080, 1080 );
        }
    }
}
