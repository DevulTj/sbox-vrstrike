using Sandbox;
using Sandbox.UI;

namespace VRStrike;

[Library]
public class HudEntity : HudEntity<RootPanel>
{
	public SpawnMenuPanel SpawnMenu { get; set; }

	public HudEntity()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/code/VRStrike/UI/HudEntity.scss" );

		SpawnMenu = new SpawnMenuPanel();
	}
}
