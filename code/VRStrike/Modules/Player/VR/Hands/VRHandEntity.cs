
using Sandbox;
using System;
using System.Linq;

namespace VRStrike;

public enum VRHand
{
	Left = 0,
	Right = 1
}

public partial class VRHandEntity : AnimEntity
{
	[Net] public VRHand Hand { get; set; } = VRHand.Left;
	[Net, Predicted] public bool IsGripping { get; set; } = false;
	[Net, Predicted] public TimeSince TimeSincePickup { get; set; } = -1;

	[Net, Predicted] public HoldableEntity HeldObject { get; private set; }
	public virtual float HandRadius => 10f;

	public Vector3 HoldOffset => Transform.Rotation.Right * 0f + Transform.Rotation.Forward * 0f + Transform.Rotation.Up * -0.1f;
	public Transform HoldTransform => Transform.WithPosition( Transform.Position + HoldOffset );

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "hand" );

		UsePhysicsCollision = true;
	}

	protected virtual void HeldObjectDrop()
	{
		HeldObject?.OnDrop( this );
	}

	protected virtual void HeldObjectPickup()
	{
		HeldObject.OnPickup( this );
		TimeSincePickup = 0;
	}

	public void StartHoldingObject( HoldableEntity obj )
	{
		if ( obj.IsBeingHeld )
			return;

		if ( TimeSincePickup < 1 )
			return;

		HeldObjectDrop();
		HeldObject = obj;
		HeldObjectPickup();
	}

	public Input.VrHand HandInput
	{ 
		get
		{
			return Hand switch
			{
				VRHand.Left => Input.VR.LeftHand,
				VRHand.Right => Input.VR.RightHand,
				_ => throw new System.Exception( "Invalid hand specified for VRHandEntity.GetInput" )
			};
		}
	}

	public Entity FindHoldableObject()
	{
		var pos = HoldTransform.Position;

		var ent = Physics.GetEntitiesInSphere( pos, HandRadius )
							.Where( x => x is HoldableEntity obj && !obj.IsBeingHeld )
							.OrderBy( x => x.Position.Distance( pos ) )
							.FirstOrDefault();

		return ent;
	}

	protected void ShowDebug()
	{
		DebugOverlay.Box( HoldTransform.Position, HoldTransform.Rotation, -1, 1, IsServer ? Color.Red : Color.Green, 0.0f, true );
		DebugOverlay.Text( HoldTransform.Position, $"{HandInput.Joystick.Value}", IsServer ? Color.White : Color.Yellow, 0.0f );
		DebugOverlay.Text( HoldTransform.Position + HoldTransform.Rotation.Down * .5f, $"{HandInput.Grip.Value}", IsServer ? Color.White : Color.Yellow, 0.0f );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		ShowDebug();



		Transform = HandInput.Transform.WithRotation( (HandInput.Transform.Rotation.RotateAroundAxis( Vector3.Right, -45f ) ) );

		IsGripping = HandInput.Grip > 0f;

		if ( Host.IsServer )
		{
			if ( IsGripping && !HeldObject.IsValid() )
			{
				var entity = FindHoldableObject();

				if ( entity != HeldObject && entity.IsValid() )
				{
					StartHoldingObject( entity as HoldableEntity );
				}
			}
			else if ( !IsGripping && HeldObject.IsValid() )
			{
				StopHoldingObject();
			}

			HeldObject?.SimulateHeldObject( this );
		}

		Animate();
	}

	private void Animate()
	{

		//UseAnimGraph = true;
		Log.Info( HasAnimGraph() );

		SetAnimBool( "bGrab", true );
		SetAnimInt( "BasePose", 1 );
		SetAnimInt( "BasePose", 1 );

		SetAnimFloat( "FingerCurl_Middle", HandInput.GetFingerCurl( 2 ) );
		SetAnimFloat( "FingerCurl_Ring", HandInput.GetFingerCurl( 3 ) );
		SetAnimFloat( "FingerCurl_Pinky", HandInput.GetFingerCurl( 4 ) );
		SetAnimFloat( "FingerCurl_Index", HandInput.GetFingerCurl( 1 ) );
		SetAnimFloat( "FingerCurl_Thumb", HandInput.GetFingerCurl( 0 ) );

	}

	private void StopHoldingObject()
	{
		HeldObjectDrop();
		HeldObject = null;
	}
}
