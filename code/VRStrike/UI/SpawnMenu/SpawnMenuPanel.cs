using Sandbox;
using Sandbox.UI;

namespace VRStrike;

[UseTemplate]
public class SpawnMenuPanel : WorldPanel
{
    public SpawnMenuPanel()
    {
		BindClass( "visible", () => ShouldDisplay() );
    }

	private bool ShouldDisplay()
	{
		var player = Local.Pawn as VRPlayerPawn;
		if ( player.LeftHandEntity.HeldObject.IsValid() ) return false;

		Vector3 dir = ( Position - player.EyePos ).Normal;
		float dot = Vector3.Dot( dir, Rotation.Forward );

		return dot <  -0.7f;
	}

	public override bool RayToLocalPosition( Ray ray, out Vector2 position, out float distance )
	{
		var ret = base.RayToLocalPosition( ray, out position, out distance );

		if ( position.x > 0f || position.y > 0f )
		{
			DebugOverlay.Line( ray.Origin, ray.Origin + ray.Direction * distance, Color.Red, 0, true );
		}

		return ret;
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
