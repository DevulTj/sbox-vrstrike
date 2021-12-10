
using Sandbox;
using System;

namespace VRStrike;

public partial class HoldableEntity : ModelEntity
{
	public bool IsBeingHeld { get; set; } = false;

	public virtual bool SimulateHeldObject( VRHandEntity hand )
	{
		Velocity = hand.Velocity;
		BaseVelocity = hand.BaseVelocity;


		return true;
	}

	public virtual void OnPickup( VRHandEntity hand )
	{
		IsBeingHeld = true;
		Parent = hand;
		Position = hand.HoldTransform.Position;
		Rotation = GetHeldRotation( hand );
	}

	private Rotation GetHeldRotation( VRHandEntity hand )
	{
		return ( hand.HoldTransform.Rotation.Angles() + new Angles( 45, 0, 0 ) ).ToRotation();
	}

	public virtual void OnDrop( VRHandEntity hand )
	{
		IsBeingHeld = false;

		Velocity *= 2f;

		Parent = null;
	}
}
