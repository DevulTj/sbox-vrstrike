
using Sandbox;

namespace VRStrike;

public partial class Hotdog : HoldableEntity
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/coffeemug01.vmdl" );

		MoveType = MoveType.Physics;
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
	}
}
