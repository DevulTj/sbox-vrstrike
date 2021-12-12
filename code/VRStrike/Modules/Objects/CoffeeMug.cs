
using Sandbox;

namespace VRStrike;

[Library( "vrs_mug" )]
public partial class CoffeeMug : HoldableEntity
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
