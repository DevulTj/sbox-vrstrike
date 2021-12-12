using Sandbox;
using Sandbox.UI;

namespace VRStrike;

[UseTemplate]
public class RadarPanel : WorldPanel
{
	// @ref
	public Panel Pulse { get; set; }
    public RadarPanel()
    {
		BindClass( "visible", () => ShouldDisplay() );
    }

	private bool ShouldDisplay()
	{
		var player = Local.Pawn as VRPlayerPawn;

		Vector3 dir = ( Position - player.EyePos ).Normal;
		float dot = Vector3.Dot( dir, Rotation.Forward );


		return dot < -0.5f;
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

	TimeSince LastPulse = 1;
	RealTimeUntil NextPulse = 0;

	public override void Tick()
    {
        base.Tick();

        if ( Local.Pawn is VRPlayerPawn player )
        {
            Transform = player.LeftHandEntity.Transform;

            //
            // Offsets
            //
            Rotation *= new Angles( 0, -90, 0 ).ToRotation();
            Position += Rotation.Forward * 1 + Rotation.Up * 3 - Rotation.Left * 0;
            WorldScale = 0.1f;
            Scale = 2.0f;

            PanelBounds = new Rect( 0, 0, 1080, 1080 );

			if ( Pulse != null )
			{
				var pulseProgress = (1 - NextPulse) * 100f;
				Pulse.Style.Width = Length.Percent( pulseProgress );
				Pulse.Style.Height = Length.Percent( pulseProgress );

				Pulse.Style.Opacity = 1 - ( 1 - NextPulse );

				if ( NextPulse <= 0 )
				{
					LastPulse = 0;
					NextPulse = 1;
				}
			}
        }
    }
}
