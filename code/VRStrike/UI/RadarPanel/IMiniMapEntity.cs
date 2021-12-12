using System.Collections.Generic;

namespace VRStrike;

public interface IMiniMapEntity
{
	public string GetMainClass();
	public bool Update( ref MiniMapDotBuilder info );
}

public class MiniMapDotBuilder
{
	public Dictionary<string, bool> Classes { get; set; } = new();
	public Vector3 Position { get; set; } = new();
	public Rotation Rotation { get; set; } = new();
	public string Text { get; set; } = "";
}
