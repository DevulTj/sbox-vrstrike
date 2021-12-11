
using Sandbox;
using System;

namespace VRStrike;

public partial class HoldableEntity : ModelEntity
{
	public virtual string Model => "";

	public bool IsBeingHeld { get; set; } = false;

	public override void Spawn()
	{
		base.Spawn();

		if ( !string.IsNullOrEmpty( Model ) )
		{
			SetModel( Model );
		}
	}

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
		Position = GetHoldPosition( hand );
		Rotation = GetHoldRotation( hand );
	}

	protected virtual Rotation GetHoldRotation( VRHandEntity hand )
	{
		return hand.HoldTransform.Rotation;
	}

	protected virtual Vector3 GetHoldPosition( VRHandEntity hand )
	{
		return hand.HoldTransform.Position;
	}

	public virtual void OnDrop( VRHandEntity hand )
	{
		IsBeingHeld = false;

		Velocity *= 2f;

		Parent = null;
	}
}
