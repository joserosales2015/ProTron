using System.Numerics;

namespace ProTron.Geometry
{
	public readonly struct VertexOut
	{
		public Vector2 Position { get; }

		// Profundidad inversa: 1/Z. Valores mayores están más cerca.
		public float InverseDepth { get; }

		// UV multiplicadas por 1/Z para la corrección de perspectiva.
		public float UOverZ { get; }
		public float VOverZ { get; }

		public VertexOut(
			Vector2 screenPosition,
			float inverseDepth,
			float uOverZ,
			float vOverZ)
		{
			Position = screenPosition;
			InverseDepth = inverseDepth;
			UOverZ = uOverZ;
			VOverZ = vOverZ;
		}
	}
}
