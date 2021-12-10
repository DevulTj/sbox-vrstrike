
using Sandbox;

namespace VRStrike;

public partial class VRHandEntity : AnimEntity
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/coffeemug01.vmdl" );
		Tags.Add( "hand" );

		Scale = 0.2f;
	}
}
