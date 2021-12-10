
using Sandbox;

namespace VRStrike;

public partial class HoldableEntity : ModelEntity
{
	public bool IsBeingHeld { get; set; } = false;

	public virtual bool SimulateHeldObject( VRHandEntity hand )
	{
		Transform = hand.Transform;
		Velocity = hand.Velocity;
		BaseVelocity = hand.BaseVelocity;

		return true;
	}

	public virtual void OnPickup( VRHandEntity hand )
	{
		IsBeingHeld = true;
	}

	public virtual void OnDrop( VRHandEntity hand )
	{
		IsBeingHeld = false;
	}
}
