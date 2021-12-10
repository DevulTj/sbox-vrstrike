using Sandbox;

namespace VRStrike;

public partial class VRController : WalkController
{
	public VRController()
	{
		Unstuck = new Unstuck( this );
	}

	/// <summary>
	/// This is temporary, get the hull size for the player's collision
	/// </summary>
	public override BBox GetHull()
	{
		Transform LocalHead = Pawn.Transform.ToLocal( Input.VR.Head );
		var girth = BodyGirth * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 ) + ( LocalHead.Position.WithZ( 0 ) * Rotation );
		var maxs = new Vector3( +girth, +girth, BodyHeight ) + ( LocalHead.Position.WithZ( 0 ) * Rotation );

		return new BBox( mins, maxs );
	}

	float BBoxBaseHeight = 0f;

	public override void UpdateBBox()
	{
		var girth = BodyGirth * 0.5f;

		BBoxBaseHeight = 0f;

		Transform LocalHead = Pawn.Transform.ToLocal( Input.VR.Head );

		var mins = ( new Vector3( -girth, -girth, BBoxBaseHeight ) + ( LocalHead.Position.WithZ( 0 ) * Rotation ) ) * Pawn.Scale;
		var maxs = ( new Vector3( +girth, +girth, BodyHeight ) + ( LocalHead.Position.WithZ( 0 ) * Rotation ) ) * Pawn.Scale;

		SetBBox( mins, maxs );
	}

	Rotation PlayerRot;
	Vector2 LeftJoystickDirection;

	public override void Simulate()
	{
		EyePosLocal = Vector3.Up * ( EyeHeight * Pawn.Scale );
		UpdateBBox();

		EyePosLocal += TraceOffset;
		EyeRot = PlayerRot;

		LeftJoystickDirection = Input.VR.LeftHand.Joystick.Value;

		RestoreGroundPos();

		CheckLadder();

		Swimming = Pawn.WaterLevel.Fraction > 0.6f;

		if ( !Swimming && !IsTouchingLadder )
		{
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

			BaseVelocity = BaseVelocity.WithZ( 0 );
		}

		if ( AutoJump ? Input.VR.RightHand.ButtonA.IsPressed : Input.VR.RightHand.ButtonA.IsPressed )
		{
			CheckJumpButton();
		}

		bool bStartOnGround = GroundEntity != null;
		if ( bStartOnGround )
		{
			Velocity = Velocity.WithZ( 0 );
			if ( GroundEntity != null )
			{
				ApplyFriction( GroundFriction * SurfaceFriction );
			}
		}

		WishVelocity = new Vector3( LeftJoystickDirection.y, -LeftJoystickDirection.x, 0 );
		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
		WishVelocity *= Input.VR.Head.Rotation.Angles().WithPitch( 0 ).ToRotation();

		if ( !Swimming && !IsTouchingLadder )
		{
			WishVelocity = WishVelocity.WithZ( 0 );
		}

		WishVelocity = WishVelocity.Normal * inSpeed;
		WishVelocity *= GetWishSpeed();

		bool bStayOnGround = false;
		if ( Swimming )
		{
			ApplyFriction( 1 );
			WaterMove();
		}
		else if ( IsTouchingLadder )
		{
			LadderMove();
		}
		else if ( GroundEntity != null )
		{
			bStayOnGround = true;
			WalkMove();
		}
		else
		{
			AirMove();
		}

		CategorizePosition( bStayOnGround );

		// FinishGravity
		if ( !Swimming && !IsTouchingLadder )
		{
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		}

		if ( GroundEntity != null )
		{
			Velocity = Velocity.WithZ( 0 );
		}

		SaveGroundPos();

		if ( Debug )
		{
			DebugOverlay.Box( Position + TraceOffset, mins, maxs, Color.Red );
			DebugOverlay.Box( Position, mins, maxs, Color.Blue );

			var lineOffset = 0;
			if ( Host.IsServer ) lineOffset = 10;

			DebugOverlay.ScreenText( lineOffset + 0, $"        Position: {Position}" );
			DebugOverlay.ScreenText( lineOffset + 1, $"        Velocity: {Velocity}" );
			DebugOverlay.ScreenText( lineOffset + 2, $"    BaseVelocity: {BaseVelocity}" );
			DebugOverlay.ScreenText( lineOffset + 3, $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]" );
			DebugOverlay.ScreenText( lineOffset + 4, $" SurfaceFriction: {SurfaceFriction}" );
			DebugOverlay.ScreenText( lineOffset + 5, $"    WishVelocity: {WishVelocity}" );
		}
	}

	/// <summary>
	/// Traces the current bbox and returns the result.
	/// liftFeet will move the start position up by this amount, while keeping the top of the bbox at the same
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceCapsule( start, end, liftFeet );
	}

	public TraceResult TraceCapsule( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		liftFeet += BodyGirth * 0.5f;
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Radius( BodyGirth / 4f )
					.HitLayer( CollisionLayer.All, false )
					.HitLayer( CollisionLayer.Solid, true )
					.HitLayer( CollisionLayer.GRATE, true )
					.HitLayer( CollisionLayer.PLAYER_CLIP, true )
					.Ignore( Pawn )
					.Run();

		tr.EndPos -= TraceOffset;
		return tr;
	}

	bool IsTouchingLadder = false;
	Vector3 LadderNormal;

	public override float GetWishSpeed()
	{
		if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

		return DefaultSpeed;
	}

	public override void CheckLadder()
	{
		var wishvel = new Vector3( LeftJoystickDirection.y, -LeftJoystickDirection.x, 0 );
		wishvel *= Rotation;
		wishvel = wishvel.Normal;

		if ( IsTouchingLadder )
		{
			if ( Input.VR.RightHand.ButtonA.IsPressed )
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;

				return;

			}
			else if ( GroundEntity != null && LadderNormal.Dot( wishvel ) > 0 )
			{
				IsTouchingLadder = false;

				return;
			}
		}

		Transform localHead = Pawn.Transform.ToLocal( Input.VR.Head );

		const float ladderDistance = 12.0f;
		var start = Input.VR.Head.Position - (Vector3.Up * localHead.Position.z);
		Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : wishvel) * ladderDistance;

		var pm = Trace.Ray( start, end )
					.Size( mins, maxs )
					.HitLayer( CollisionLayer.All, false )
					.HitLayer( CollisionLayer.LADDER, true )
					.Ignore( Pawn )
					.Run();

		IsTouchingLadder = false;

		if ( pm.Hit )
		{
			IsTouchingLadder = true;
			LadderNormal = pm.Normal;
		}
	}

	public override void LadderMove()
	{
		Velocity = Vector3.Up * DefaultSpeed * LeftJoystickDirection.y;
		Move();
	}

	void RestoreGroundPos()
	{
		if ( GroundEntity == null || GroundEntity.IsWorld )
			return;
	}

	void SaveGroundPos()
	{
		if ( GroundEntity == null || GroundEntity.IsWorld )
			return;

		//GroundTransform = GroundEntity.Transform.ToLocal( new Transform( Pos, Rot ) );
	}
}
