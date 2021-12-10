// © DevulTj 2021 - http://www.tferguson.co.uk/
// All Rights Reserved

using Sandbox;

namespace VRStrike;

public partial class VRController : PawnController
{
	[Net] public float MaxWalkSpeed { get; set; } = 125f;

	public override void Simulate()
	{
		base.Simulate();

		SimulateMovement();
	}

	public override void FrameSimulate()
	{
		base.FrameSimulate();

	}

	public virtual void SimulateMovement()
	{
		var inputRotation = Input.Rotation.Angles().WithPitch( 0 ).ToRotation();

		Velocity = Velocity.AddClamped( inputRotation * new Vector3( Input.Forward, Input.Left, 0 ) * MaxWalkSpeed * 5 * Time.Delta, MaxWalkSpeed );
		Velocity = Velocity.Approach( 0, Time.Delta * MaxWalkSpeed * 3 );

		// Ensure we're on the floor
		Velocity = Velocity.WithZ( -160 );

		//
		// Move helper traces and slides along surfaces for us
		//
		MoveHelper helper = new MoveHelper( Position, Velocity );
		helper.Trace = helper.Trace.Size( 20 );

		helper.TryUnstuck();
		helper.TryMoveWithStep( Time.Delta, 30.0f );

		Position = helper.Position;
		Velocity = helper.Velocity;
	}
}
