using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Buffer
{
	public class DepthBuffer
	{
		private readonly float[] _depth;

		public int Width { get; }
		public int Height { get; }

		public DepthBuffer(int width, int height)
		{
			Width = width;
			Height = height;

			_depth = new float[width * height];
		}

		public void Clear()
		{
			Array.Fill(_depth, float.MinValue);
		}

		public float GetDepth(int x, int y)
		{
			return _depth[y * Width + x];
		}

		public void SetDepth(int x, int y, float depth)
		{
			_depth[y * Width + x] = depth;
		}

		public bool TestAndSet(int x, int y, float depth)
		{
			int index = y * Width + x;

			if (depth > _depth[index])
			{
				_depth[index] = depth;
				return true;
			}

			return false;
		}
	}
}
