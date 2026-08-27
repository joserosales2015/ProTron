using ProTron.Math;
using ProTron.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Core
{
	public static class RenderQueue
	{
		public static void SortFrontToBack(
			List<GameObject> objects,
			Camera camera)
		{
			objects.Sort((a, b) =>
			{
				Vector3f da =
					a.Transform.Position - camera.Transform.Position;

				Vector3f db =
					b.Transform.Position - camera.Transform.Position;

				float distA = Vector3f.Dot(da, da);
				float distB = Vector3f.Dot(db, db);

				return distA.CompareTo(distB);
			});
		}
	}
}
