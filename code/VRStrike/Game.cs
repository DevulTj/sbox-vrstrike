global using Sandbox;

namespace VRStrike;

public partial class Game : Sandbox.Game
{
	public Game()
	{
		if ( IsServer )
		{
			_ = new HudEntity();
		}
	}

	public override void Spawn()
	{
		base.Spawn();
	}

	/// <summary>
	/// Client joined, create them a LabPawn and spawn them
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		client.Pawn = client.IsUsingVr ? new VRPlayerPawn() : new PlayerPawn();
		MoveToSpawnpoint( client.Pawn );
	}

	/// <summary>
	/// Don't do any in game input unless we're holding down RMB
	/// </summary>
	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

	}

	/// <summary>
	/// Put the camera at the pawn's eye
	/// </summary>
	public override CameraSetup BuildCamera( CameraSetup camSetup )
	{
		camSetup.Rotation = Local.Client.Pawn.EyeRotation;
		camSetup.Position = Local.Client.Pawn.EyePosition;

		return base.BuildCamera( camSetup );
	}

	[ConCmd.Server( "vrs_spawnmenu" )]
	public static void SpawnItem( string item )
	{
		var player = ConsoleSystem.Caller.Pawn as VRPlayerPawn;

		var obj = TypeLibrary.Create<HoldableEntity>( item );
		if ( !obj.IsValid() ) return;

		obj.Position = player.RightHandEntity.Position;
	}
}
