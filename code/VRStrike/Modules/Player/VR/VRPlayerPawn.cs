
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
			Hand = VRHand.Left,
			Owner = this
		};
		LeftHandEntity.SetModel( "models/hands/alyx_hand_left.vmdl" );

		RightHandEntity = new()
		{
			Hand = VRHand.Right,
			Owner = this
		};
		RightHandEntity.SetModel( "models/hands/alyx_hand_right.vmdl" );

	}

	TimeSince LastConjured = 0;

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		SimulateTrackedObjects();
		SimulateHands();
		SimulateSnapRotation();

		if ( Host.IsServer )
		{
			if ( Input.VR.RightHand.ButtonA.IsPressed && LastConjured > 1f )
			{
				var dog = new Weapon();
				dog.Position = RightHandEntity.Position;

				LastConjured = 0;
				dog.Scale = 0.5f;
			}
		}
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

		ClientSimulateHands();
	}

	private void SimulateTrackedObjects()
	{
		foreach ( var tracked in Input.VR.TrackedObjects )
		{
			DebugOverlay.Text( tracked.Transform.Position, $"Tracking: {tracked.Type}" );
		}
	}
}
