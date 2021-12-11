// © DevulTj 2021 - http://www.tferguson.co.uk/
// All Rights Reserved

using Sandbox;

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
        camSetup.Rotation = Local.Client.Pawn.EyeRot;
        camSetup.Position = Local.Client.Pawn.EyePos;

        return base.BuildCamera( camSetup );
    }
}
