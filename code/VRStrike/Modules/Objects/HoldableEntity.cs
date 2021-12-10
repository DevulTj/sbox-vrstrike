
using Sandbox;

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
		Rotation = hand.HoldTransform.Rotation;
	}

	public virtual void OnDrop( VRHandEntity hand )
	{
		IsBeingHeld = false;

		Velocity *= 2f;

		Parent = null;
	}
}
