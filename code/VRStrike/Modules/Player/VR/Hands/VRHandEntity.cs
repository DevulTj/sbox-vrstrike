
using Sandbox;
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
	[Net] public bool IsGripping { get; set; } = false;

	public HoldableEntity HeldObject { get; private set; }
	public virtual float HandRadius => 10f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/coffeemug01.vmdl" );
		Tags.Add( "hand" );

		Scale = 0.2f;
	}

	protected virtual void HeldObjectDrop()
	{
		HeldObject?.OnDrop( this );
	}

	protected virtual void HeldObjectPickup()
	{
		HeldObject.OnPickup( this );
	}

	public void StartHoldingObject( HoldableEntity obj )
	{
		if ( obj.IsBeingHeld )
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
		var pos = Position;

		var ent = Physics.GetEntitiesInSphere( pos, HandRadius )
							.Where( x => x is HoldableEntity obj && !obj.IsBeingHeld )
							.OrderBy( x => x.Position.Distance( pos ) )
							.FirstOrDefault();

		return ent;
	}

	protected void ShowDebug()
	{
		DebugOverlay.Box( Transform.Position, Transform.Rotation, -1, 1, IsServer ? Color.Red : Color.Green, 0.0f, true );
		DebugOverlay.Text( Transform.Position, $"{HandInput.Joystick}", IsServer ? Color.White : Color.Yellow, 0.0f );
		DebugOverlay.Text( Transform.Position + Transform.Rotation.Down * 10f, $"{HandInput.Grip}", IsServer ? Color.White : Color.Yellow, 0.0f );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		ShowDebug();

		Transform = HandInput.Transform;
		IsGripping = HandInput.Grip > 0f;

		if ( IsGripping )
		{
			var entity = FindHoldableObject();

			if ( entity != HeldObject && entity.IsValid() )
			{
				StartHoldingObject( entity as HoldableEntity );
			}
		}

		HeldObject?.SimulateHeldObject( this );
	}
}
