// © DevulTj 2021 - http://www.tferguson.co.uk/
// All Rights Reserved

using Sandbox;
using System;

namespace VRStrike;

public partial class VRCamera : PlayerCamera
{
	public override void Build( ref CameraSetup setup )
	{
		setup.ZNear = 1f;
	}

	public virtual Vector2 GetJoystickInput( bool isLeft = true )
	{
		var input = isLeft ? Input.VR.LeftHand.Joystick : Input.VR.RightHand.Joystick;

		return new Vector2( input.Value.x, input.Value.y );
	}

	public override void BuildInput( InputBuilder builder )
	{
		var joystick = GetJoystickInput();
		builder.InputDirection.y = -joystick.x;
		builder.InputDirection.x = joystick.y;
	}
}
