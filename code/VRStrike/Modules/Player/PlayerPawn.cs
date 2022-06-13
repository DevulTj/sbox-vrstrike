namespace VRStrike;

public partial class PlayerPawn : Sandbox.Player
{
	public override void Spawn()
	{
		base.Spawn();

		Controller = new WalkController();
		CameraMode = new PlayerCamera();
	}
}
