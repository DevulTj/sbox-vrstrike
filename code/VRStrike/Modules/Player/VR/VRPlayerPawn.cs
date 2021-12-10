
using Sandbox;

namespace VRStrike;

public partial class VRPlayerPawn : PlayerPawn
{
	// Hands
	[Net] public VRHandEntity LeftHandEntity { get; set; }
	[Net] public VRHandEntity RightHandEntity { get; set; }

	// Snap Rotation
	public virtual float SnapRotationDelay => 0.25f;
	public virtual float SnapRotationAngle => 45f;
	public virtual float SnapRotationDeadzone => 0.2f;

	[Net, Predicted] public TimeSince TimeSinceSnap { get; protected set; } = -1;

	public override void Spawn()
	{
		base.Spawn();

		Controller = new VRController();
		Camera = new VRCamera();

		LeftHandEntity = new()
		{
			Owner = this
		};

		RightHandEntity = new()
		{ 
			Owner = this
		};
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		SimulateTrackedObjects();
		SimulateHands();
		SimulateSnapRotation();
	}

	protected void SimulateSnapRotation()
	{
		var yawInput = Input.VR.RightHand.Joystick.Value.x;

		if ( TimeSinceSnap > SnapRotationDelay )
		{
			if ( yawInput > SnapRotationDeadzone )
			{
				Transform = Transform.RotateAround(
					Input.VR.Head.Position.WithZ( Position.z ),
					Rotation.FromAxis( Vector3.Up, -SnapRotationAngle )
				);
				TimeSinceSnap = 0;
			}
			else if ( yawInput < -SnapRotationDeadzone )
			{
				Transform = Transform.RotateAround(
					Input.VR.Head.Position.WithZ( Position.z ),
					Rotation.FromAxis( Vector3.Up, SnapRotationAngle )
				);
				TimeSinceSnap = 0;
			}

		}

		if ( yawInput > -SnapRotationDeadzone && yawInput < SnapRotationDeadzone )
		{
			TimeSinceSnap = SnapRotationDelay + 0.1f;
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
	}

	private void SimulateHands()
	{
		LeftHandEntity.Transform = Input.VR.LeftHand.Transform.WithScale( 0.2f );
		RightHandEntity.Transform = Input.VR.RightHand.Transform.WithScale( 0.2f );
	}

	private void SimulateTrackedObjects()
	{
		DebugDrawHand( Input.VR.LeftHand );
		DebugDrawHand( Input.VR.RightHand, false );

		foreach ( var tracked in Input.VR.TrackedObjects )
		{
			DebugOverlay.Text( tracked.Transform.Position, $"Tracking: {tracked.Type}" );
		}
	}

	private void DebugDrawHand( Input.VrHand hand, bool isLeft = true )
	{
		DebugOverlay.Box( hand.Transform.Position, hand.Transform.Rotation, -1, 1, IsServer ? Color.Red : Color.Green, 0.0f, true );
		DebugOverlay.Text( hand.Transform.Position, ( Camera as VRCamera ).DebugOutput( isLeft ), IsServer ? Color.White : Color.Yellow, 0.0f );
	}
}
