
using Sandbox;

namespace VRStrike;

public partial class Weapon : HoldableEntity
{
	public override string Model => "models/weapons/w_mac11.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		MoveType = MoveType.Physics;
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
	}

	//protected override Vector3 GetHoldPosition( VRHandEntity hand )
	//{
	//	return GetAttachment( "hold_point", false ).GetValueOrDefault().Position;
	//}
}
