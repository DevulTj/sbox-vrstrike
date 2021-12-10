
using Sandbox;
using System.Linq;

namespace VRStrike;

public partial class VRPlayerPawn
{
	protected virtual void SimulateHands()
	{
		LeftHandEntity?.Simulate( Client );
		RightHandEntity?.Simulate( Client );
	}
	protected virtual void ClientSimulateHands()
	{
		LeftHandEntity?.FrameSimulate( Client );
		RightHandEntity?.FrameSimulate( Client );
	}
}
