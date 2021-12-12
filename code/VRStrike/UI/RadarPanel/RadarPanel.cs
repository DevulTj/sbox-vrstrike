using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VRStrike;

public class MiniMapDot : Panel
{
	public IMiniMapEntity Entity { get; set; }

	public Label Text { get; set; }

	public MiniMapDot( IMiniMapEntity entity )
	{
		Entity = entity;

		Text = AddChild<Label>();
		Text.Text = "";
	}

	public void Apply( MiniMapDotBuilder info )
	{
		Text.Text = info.Text;

		foreach ( var kv in info.Classes )
		{
			SetClass( kv.Key, kv.Value );
		}
	}
}

[UseTemplate]
public class RadarPanel : WorldPanel
{
	// @ref
	public Panel Pulse { get; set; }

	protected List<MiniMapDot> Dots { get; set; } = new();

	public RadarPanel()
    {
		BindClass( "visible", () => ShouldDisplay() );
    }

	private bool ShouldDisplay()
	{
		var player = Local.Pawn as VRPlayerPawn;

		if ( player.LeftHandEntity.HeldObject.IsValid() ) return false;

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

	public float Range => 200f;

	public virtual Vector2 MiniMapSize => new Vector2( 1080, 1080 ) * 0.5;

	// @ref
	public Panel DotAnchor { get; set; }

	public void ClearDot( MiniMapDot Dot )
	{
		if ( Dot is null )
			return;
		if ( !Dots.Contains( Dot ) )
			return;

		Dots.Remove( Dot );
		Dot.Delete();

		return;
	}

	protected void ValidateEntity( IMiniMapEntity entity, bool updatePos )
	{
		var info = new MiniMapDotBuilder();
		var styleClass = entity.GetMainClass();
		var currentDot = Dots.Where( x => x.Entity == entity ).FirstOrDefault();

		if ( !entity.Update( ref info ) )
		{
			ClearDot( currentDot );
			return;
		}

		if ( currentDot is null )
		{
			currentDot = new MiniMapDot( entity )
			{
				Parent = DotAnchor
			};

			// Add style class
			currentDot.AddClass( styleClass );

			Dots.Add( currentDot );
		}

		if ( updatePos )
		{
			var player = Local.Pawn as VRPlayerPawn;
			var diff = info.Position - player.EyePos;

			var x = MiniMapSize.x / Range * diff.x * 0.5f;
			var y = MiniMapSize.y / Range * diff.y * 0.5f;
			var ang = MathF.PI / 180 * (player.LeftHandEntity.Rotation.Yaw() - 0f);
			var cos = MathF.Cos( ang );
			var sin = MathF.Sin( ang );


			var translatedX = x * cos + y * sin;
			var translatedY = y * cos - x * sin;

			currentDot.Style.Left = (MiniMapSize.x / 2f) + translatedX;
			currentDot.Style.Top = (MiniMapSize.y / 2f) - translatedY;
		}

		currentDot.Style.Opacity = 1 - (1 - NextPulse);
		currentDot.Apply( info );
	}


	public override void Tick()
	{
		base.Tick();

		UpdateMiniMapDots( false );

		if ( Local.Pawn is VRPlayerPawn player )
		{
			Transform = player.LeftHandEntity.Transform;

			//
			// Offsets
			//
			Rotation *= new Angles( 0, -90, 0 ).ToRotation();
			Position += Rotation.Forward * 1 + Rotation.Up * 3 - Rotation.Left * 0;
			WorldScale = 0.1f;
			Scale = 2f;

			PanelBounds = new Rect( 0, 0, 1080, 1080 );

			if ( Pulse != null )
			{
				var pulseProgress = (1 - NextPulse) * 100f;
				Pulse.Style.Width = Length.Percent( pulseProgress );
				Pulse.Style.Height = Length.Percent( pulseProgress );

				Pulse.Style.Opacity = 1 - (1 - NextPulse);

				if ( NextPulse <= 0 )
				{
					UpdateMiniMapDots( true );


					LastPulse = 0;
					NextPulse = 1;
				}
			}
		}
	}

	protected void UpdateMiniMapDots( bool updatePos )
	{
		var existingDots = Dots.Select( x => x.Entity ).ToList();

		Entity.All.OfType<IMiniMapEntity>()
						.Concat( existingDots )
						.ToList()
						.ForEach( x => ValidateEntity( x, updatePos ) );
	}
}
