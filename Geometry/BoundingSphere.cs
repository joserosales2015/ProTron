using ProTron.Math;

namespace ProTron.Geometry
{
	public readonly record struct BoundingSphere(
		Vector3f Center,
		float Radius);
}
