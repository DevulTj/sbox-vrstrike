
using Sandbox;
using System.Collections.Generic;

namespace VRStrike;

public partial class Weapon : HoldableEntity, IMiniMapEntity
{
	public override string ModelPath => "models/weapons/w_mac11.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		MoveType = MoveType.Physics;
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
	}

	protected virtual float MaxAmtOfHits => 2f;
	protected virtual float MaxRicochetAngle => 45f;
	protected float MaxPenetration => 20f;

	public virtual float FireRate => 1f / 20f;

	public TimeSince TimeSincePrimaryAttack = 0;

	protected Vector2 CurrentRecoil = Vector2.Zero;

	public virtual float XRecoilDieSpeed => 50f * 1.9f;
	public virtual float YRecoilDieSpeed => 70f * 1.9f;
	public virtual float XRecoilOnShot => Rand.Float( 5f, 7f );
	public virtual float YRecoilOnShot => Rand.Float( 4, 12f );

	protected virtual void SimulateRecoil()
	{
		bool shouldRemoveRecoil = false;
		if ( CurrentRecoil.x > 0 )
		{
			CurrentRecoil.x -= XRecoilDieSpeed * Time.Delta;
			shouldRemoveRecoil = true;
		}

		if ( CurrentRecoil.y > 0 )
		{
			CurrentRecoil.y -= YRecoilDieSpeed * Time.Delta;
			shouldRemoveRecoil = true;
		}

		if ( shouldRemoveRecoil )
			LocalRotation = new Angles( -CurrentRecoil.y, CurrentRecoil.x, 0 ).ToRotation();
	}

	public override bool SimulateHeldObject( VRHandEntity hand )
	{
		if ( hand.FingerData.IsTriggerDown() )
		{
			if ( CanPrimaryAttack() )
			{
				TimeSincePrimaryAttack = 0;
				PrimaryAttack();
			}
		}

		SimulateRecoil();

		return true;
	}

	private bool CanPrimaryAttack()
	{
		return TimeSincePrimaryAttack > FireRate;
	}


	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/swb/muzzle/flash_small.vpcf", this, "muzzle" );
		Particles.Create( "particles/swb/muzzle/barrel_smoke.vpcf", this, "muzzle" );
	}

	private void PrimaryAttack()
	{
		TimeSincePrimaryAttack = 0;

		//
		// Play the fire sound
		//
		PlaySound( "rust_smg.shoot" );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();

		//
		// Shoot some bullets
		//
		ShootBullet( 0.1f, 0.1f, 25, 1, 1, 100000f );

		//
		// Set animation property
		//
		// (Owner as AnimEntity).SetAnimBool( "b_attack", true );
	}

	protected virtual bool ShouldContinue( TraceResult tr, float angle = 0f )
	{
		float maxAngle = MaxRicochetAngle;

		if ( angle > maxAngle )
			return false;

		return true;
	}

	protected virtual Vector3 CalculateDirection( TraceResult tr, ref float hits )
	{
		if ( tr.Entity is GlassShard )
		{
			// Allow us to do another hit
			hits--;
			return tr.Direction;
		}

		return Vector3.Reflect( tr.Direction, tr.Normal ).Normal;
	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocet'ing or something.
	/// </summary>
	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		float currentAmountOfHits = 0;
		Vector3 _start = start;
		Vector3 _end = end;
		List<TraceResult> hits = new();

		Entity lastEnt = null;
		while ( currentAmountOfHits < MaxAmtOfHits )
		{
			currentAmountOfHits++;

			bool inWater = Map.Physics.IsPointWater( _start );
			var tr = Trace.Ray( _start, _end )
			.UseHitboxes()
			.HitLayer( CollisionLayer.Water, !inWater )
			.HitLayer( CollisionLayer.Debris )
			.Ignore( Owner )
			.Ignore( lastEnt.IsValid() ? lastEnt : this )
			.Size( radius )
			.Run();

			if ( tr.Hit )
				hits.Add( tr );

			lastEnt = tr.Entity;

			if ( tr.Entity is GlassShard )
			{
				_start = tr.EndPosition;
				_end = tr.EndPosition + ( tr.Direction * 5000 );
			}
			else
			{
				var reflectDir = CalculateDirection( tr, ref currentAmountOfHits );
				var angle = reflectDir.Angle( tr.Direction );

				_start = tr.EndPosition;
				_end = tr.EndPosition + ( reflectDir * 5000 );

				if ( !ShouldContinue( tr, angle ) )
					break;
			}
		}

		return hits;
	}

	public virtual float GetAddedSpread()
	{
		var radius = 0f;
		var player = Owner as PlayerPawn;

		return radius;
	}

	protected Transform GetMuzzle()
	{
		return GetAttachment( "muzzle", true ).GetValueOrDefault();
	}

	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount = 1, float bulletRange = 5000f )
	{
		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Rand.SetSeed( Time.Tick );

		spread += GetAddedSpread();

		CurrentRecoil.x += XRecoilOnShot;
		CurrentRecoil.y += YRecoilOnShot;

		for ( int i = 0; i < bulletCount; i++ )
		{
			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//

			int count = 0;
			var muzzle = GetMuzzle();
			foreach ( var tr in TraceBullet( muzzle.Position, muzzle.Rotation.Forward * bulletRange, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, muzzle.Rotation.Forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );

				SendTracer( tr.StartPosition, tr.EndPosition );
			}
		}
	}

	[ClientRpc]
	protected void SendTracer( Vector3 start, Vector3 end )
	{
		var tracer = Particles.Create( "particles/swb/tracer/tracer_large.vpcf" );
		tracer.SetPosition( 1, start );
		tracer.SetPosition( 2, end );
	}

	string IMiniMapEntity.GetMainClass()
	{
		return "object";
	}

	bool IMiniMapEntity.Update( ref MiniMapDotBuilder info )
	{
		if ( !this.IsValid() )
			return false;

		if ( LifeState != LifeState.Alive )
			return false;

		info.Text = "";
		info.Position = Position;

		return true;
	}
}
