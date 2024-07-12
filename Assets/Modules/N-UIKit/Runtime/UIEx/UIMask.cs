namespace UnityEngine.UI
{
	public class UIMask : MaskableGraphic
	{
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
		}
	}
}


