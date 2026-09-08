using ProTron.Buffer;
using ProTron.Geometry;
using ProTron.Objects;
using ProTron.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static ProTron.Geometry.Triangle;

namespace ProTron.Graphics
{
	public class Rasterizer
	{
		private const int TileSize = 16;
		private const int ParallelTileThreshold = 128;

		private readonly FrameBuffer _frameBuffer;
		private readonly DepthBuffer _depthBuffer;
		private readonly int _tileColumns;
		private readonly List<int>[] _tileBins;
		private readonly List<int> _occupiedTiles = new();
		private readonly List<OpaqueTriangleCommand> _opaqueTriangles = new();
		public RendererStats Stats { get; set; } = new();
		public float DepthBias { get; set; } = 0.0f;

		private readonly record struct RasterSetup(
			int MinX,
			int MaxX,
			int MinY,
			int MaxY,
			float RowW0,
			float RowW1,
			float RowW2,
			float W0Dx,
			float W1Dx,
			float W2Dx,
			float W0Dy,
			float W1Dy,
			float W2Dy,
			float RowDepth,
			float DepthDx,
			float DepthDy,
			float RowUOverZ,
			float RowVOverZ,
			float UOverZDx,
			float UOverZDy,
			float VOverZDx,
			float VOverZDy);

		private enum TileCoverage
		{
			Rejected,
			Partial,
			Full
		}

		private readonly record struct OpaqueTriangleCommand(
			RasterSetup Setup,
			uint BaseColor,
			Texture? Texture,
			TextureAddressMode AddressU,
			TextureAddressMode AddressV);

		private struct RasterCounters
		{
			public int DepthTests { get; set; }
			public int DepthRejected { get; set; }
			public int PixelsDrawn { get; set; }
		}

		public Rasterizer(
			FrameBuffer frameBuffer,
			DepthBuffer depthBuffer)
		{
			_frameBuffer = frameBuffer;
			_depthBuffer = depthBuffer;
			_tileColumns = (frameBuffer.Width + TileSize - 1) / TileSize;
			int tileRows = (frameBuffer.Height + TileSize - 1) / TileSize;
			_tileBins = new List<int>[checked(_tileColumns * tileRows)];

			for (int i = 0; i < _tileBins.Length; i++)
				_tileBins[i] = new List<int>();
		}

		public void BeginFrame()
		{
			for (int i = 0; i < _occupiedTiles.Count; i++)
				_tileBins[_occupiedTiles[i]].Clear();

			_occupiedTiles.Clear();
			_opaqueTriangles.Clear();
		}

		public bool TryQueueOpaqueTriangle(
			VertexOut v1,
			VertexOut v2,
			VertexOut v3,
			uint baseColor,
			Texture? texture,
			in TextureSampler sampler,
			MaterialBlendMode blendMode)
		{
			if (blendMode != MaterialBlendMode.Opaque ||
				(texture is not null && sampler.Filter != TextureFilterMode.Nearest))
			{
				return false;
			}

			if (!TryCreateRasterSetup(v1, v2, v3, out RasterSetup setup))
				return true;

			int commandIndex = _opaqueTriangles.Count;
			_opaqueTriangles.Add(new OpaqueTriangleCommand(
				setup,
				baseColor,
				texture,
				sampler.AddressU,
				sampler.AddressV));

			int minTileX = setup.MinX / TileSize;
			int maxTileX = setup.MaxX / TileSize;
			int minTileY = setup.MinY / TileSize;
			int maxTileY = setup.MaxY / TileSize;

			for (int tileY = minTileY; tileY <= maxTileY; tileY++)
			{
				int rowStart = tileY * _tileColumns;

				for (int tileX = minTileX; tileX <= maxTileX; tileX++)
				{
					int tileIndex = rowStart + tileX;
					List<int> bin = _tileBins[tileIndex];

					if (bin.Count == 0)
						_occupiedTiles.Add(tileIndex);

					bin.Add(commandIndex);
				}
			}

			return true;
		}

		public void FlushOpaqueTriangles()
		{
			#if !DEBUG
			if (_occupiedTiles.Count >= ParallelTileThreshold && Environment.ProcessorCount > 1)
			{
				Parallel.For(0, _occupiedTiles.Count, occupiedIndex =>
				{
					RasterCounters ignored = default;
					DrawFrameTile(_occupiedTiles[occupiedIndex], ref ignored);
				});
				return;
			}
			#endif

			RasterCounters counters = default;

			for (int i = 0; i < _occupiedTiles.Count; i++)
				DrawFrameTile(_occupiedTiles[i], ref counters);

			#if DEBUG
			Stats.AddRasterization(
				counters.DepthTests,
				counters.DepthRejected,
				counters.PixelsDrawn);
			#endif
		}

		public void DrawTriangleWireframe(VertexOut v1, VertexOut v2, VertexOut v3, Triangle triangle, uint baseColor)
		{
			if (triangle.HasEdge(TriangleEdges.AB))
				DrawLine(v1, v2, baseColor, DepthBias);

			if (triangle.HasEdge(TriangleEdges.BC))
				DrawLine(v2, v3, baseColor, DepthBias);

			if (triangle.HasEdge(TriangleEdges.CA))
				DrawLine(v3, v1, baseColor, DepthBias);
		}

		public void DrawFilledTriangle(
			VertexOut v1,
			VertexOut v2,
			VertexOut v3,
			uint baseColor,
			Texture? texture,
			in TextureSampler sampler,
			MaterialBlendMode blendMode,
			byte alphaCutoff)
		{
			if (!TryCreateRasterSetup(v1, v2, v3, out RasterSetup setup))
				return;

			int minX = setup.MinX;
			int maxX = setup.MaxX;
			int minY = setup.MinY;
			int maxY = setup.MaxY;
			float rowW0 = setup.RowW0;
			float rowW1 = setup.RowW1;
			float rowW2 = setup.RowW2;
			float w0dx = setup.W0Dx;
			float w1dx = setup.W1Dx;
			float w2dx = setup.W2Dx;
			float w0dy = setup.W0Dy;
			float w1dy = setup.W1Dy;
			float w2dy = setup.W2Dy;
			float rowDepth = setup.RowDepth;
			float depthDx = setup.DepthDx;
			float depthDy = setup.DepthDy;
			float rowUOverZ = setup.RowUOverZ;
			float rowVOverZ = setup.RowVOverZ;
			float uOverZDx = setup.UOverZDx;
			float uOverZDy = setup.UOverZDy;
			float vOverZDx = setup.VOverZDx;
			float vOverZDy = setup.VOverZDy;

			if (blendMode == MaterialBlendMode.Opaque)
			{
				if (texture is null)
				{
					DrawOpaqueSolid(setup, baseColor);
					return;
				}

				if (sampler.Filter == TextureFilterMode.Nearest)
				{
					DrawOpaqueNearest(setup, baseColor, texture, sampler);
					return;
				}
			}

			float mipLevel = texture is not null &&
				sampler.Filter == TextureFilterMode.Trilinear
				? EstimateMipLevel(
					v1,
					v2,
					v3,
					depthDx,
					depthDy,
					uOverZDx,
					uOverZDy,
					vOverZDx,
					vOverZDy,
					texture)
				: 0f;

			if (blendMode == MaterialBlendMode.Opaque && texture is not null)
			{
				DrawOpaqueFiltered(setup, baseColor, texture, sampler, mipLevel);
				return;
			}

			#if DEBUG
			int depthTests = 0;
			int depthRejected = 0;
			int pixelsDrawn = 0;
			#endif

			for (int y = minY; y <= maxY; y++)
			{
				float w0 = rowW0;
				float w1 = rowW1;
				float w2 = rowW2;
				float depth = rowDepth;
				float uOverZ = rowUOverZ;
				float vOverZ = rowVOverZ;

				for (int x = minX; x <= maxX; x++)
				{
					if (w0 >= 0 && w1 >= 0 && w2 >= 0 &&
						depth > 0f && float.IsFinite(depth))
					{
						#if DEBUG
						depthTests++;
						#endif
						if (_depthBuffer.Test(x, y, depth))
						{
							uint pixelColor = baseColor;

							if (texture is not null)
							{
								float z = 1f / depth;
								float u = uOverZ * z;
								float v = vOverZ * z;

								pixelColor = ColorUtils.Multiply(
									baseColor,
									texture.Sample(u, v, sampler, mipLevel));
							}

							byte alpha = ColorUtils.GetAlpha(pixelColor);

							if (blendMode == MaterialBlendMode.Cutout && alpha < alphaCutoff)
								goto AdvancePixel;

							if (blendMode == MaterialBlendMode.AlphaBlend)
							{
								if (alpha == 0)
									goto AdvancePixel;

								uint destination = _frameBuffer.GetPixelUnchecked(x, y);
								pixelColor = ColorUtils.AlphaBlend(pixelColor, destination);
							}
							else
							{
								_depthBuffer.Set(x, y, depth);
								pixelColor = ColorUtils.WithAlpha(pixelColor, 255);
							}

							_frameBuffer.PutPixelUnChecked(x, y, pixelColor);
							#if DEBUG
							pixelsDrawn++;
							#endif
						}
						else
						{
							#if DEBUG
							depthRejected++;
							#endif
						}
					}

				AdvancePixel:
					w0 += w0dx;
					w1 += w1dx;
					w2 += w2dx;

					depth += depthDx;
					uOverZ += uOverZDx;
					vOverZ += vOverZDx;
				}

				rowW0 += w0dy;
				rowW1 += w1dy;
				rowW2 += w2dy;

				rowDepth += depthDy;
				rowUOverZ += uOverZDy;
				rowVOverZ += vOverZDy;
			}

			#if DEBUG
			Stats.AddRasterization(depthTests, depthRejected, pixelsDrawn);
			#endif
		}

		private bool TryCreateRasterSetup(
			VertexOut v1,
			VertexOut v2,
			VertexOut v3,
			out RasterSetup setup)
		{
			setup = default;

			if (!float.IsFinite(v1.Position.X) || !float.IsFinite(v1.Position.Y) ||
				!float.IsFinite(v2.Position.X) || !float.IsFinite(v2.Position.Y) ||
				!float.IsFinite(v3.Position.X) || !float.IsFinite(v3.Position.Y))
			{
				return false;
			}

			float minimumX = MathF.Min(v1.Position.X, MathF.Min(v2.Position.X, v3.Position.X));
			float maximumX = MathF.Max(v1.Position.X, MathF.Max(v2.Position.X, v3.Position.X));
			float minimumY = MathF.Min(v1.Position.Y, MathF.Min(v2.Position.Y, v3.Position.Y));
			float maximumY = MathF.Max(v1.Position.Y, MathF.Max(v2.Position.Y, v3.Position.Y));

			if (maximumX < 0f || maximumY < 0f ||
				minimumX > _frameBuffer.Width - 1 || minimumY > _frameBuffer.Height - 1)
			{
				return false;
			}

			int minX = (int)MathF.Max(0f, MathF.Floor(minimumX));
			int maxX = (int)MathF.Min(_frameBuffer.Width - 1, MathF.Ceiling(maximumX));
			int minY = (int)MathF.Max(0f, MathF.Floor(minimumY));
			int maxY = (int)MathF.Min(_frameBuffer.Height - 1, MathF.Ceiling(maximumY));
			float area = Edge(v1.Position, v2.Position, v3.Position);

			if (area == 0f || !float.IsFinite(area))
				return false;

			float invArea = 1f / area;
			float w0dx = (v3.Position.Y - v2.Position.Y) * invArea;
			float w0dy = -(v3.Position.X - v2.Position.X) * invArea;
			float w1dx = (v1.Position.Y - v3.Position.Y) * invArea;
			float w1dy = -(v1.Position.X - v3.Position.X) * invArea;
			float w2dx = (v2.Position.Y - v1.Position.Y) * invArea;
			float w2dy = -(v2.Position.X - v1.Position.X) * invArea;
			Vector2 p0 = new(minX + 0.5f, minY + 0.5f);
			float rowW0 = Edge(v2.Position, v3.Position, p0) * invArea;
			float rowW1 = Edge(v3.Position, v1.Position, p0) * invArea;
			float rowW2 = Edge(v1.Position, v2.Position, p0) * invArea;
			float rowDepth = rowW0 * v1.InverseDepth + rowW1 * v2.InverseDepth + rowW2 * v3.InverseDepth;
			float rowUOverZ = rowW0 * v1.UOverZ + rowW1 * v2.UOverZ + rowW2 * v3.UOverZ;
			float rowVOverZ = rowW0 * v1.VOverZ + rowW1 * v2.VOverZ + rowW2 * v3.VOverZ;
			float depthDx = w0dx * v1.InverseDepth + w1dx * v2.InverseDepth + w2dx * v3.InverseDepth;
			float depthDy = w0dy * v1.InverseDepth + w1dy * v2.InverseDepth + w2dy * v3.InverseDepth;
			float uOverZDx = w0dx * v1.UOverZ + w1dx * v2.UOverZ + w2dx * v3.UOverZ;
			float uOverZDy = w0dy * v1.UOverZ + w1dy * v2.UOverZ + w2dy * v3.UOverZ;
			float vOverZDx = w0dx * v1.VOverZ + w1dx * v2.VOverZ + w2dx * v3.VOverZ;
			float vOverZDy = w0dy * v1.VOverZ + w1dy * v2.VOverZ + w2dy * v3.VOverZ;

			setup = new RasterSetup(
				minX,
				maxX,
				minY,
				maxY,
				rowW0,
				rowW1,
				rowW2,
				w0dx,
				w1dx,
				w2dx,
				w0dy,
				w1dy,
				w2dy,
				rowDepth,
				depthDx,
				depthDy,
				rowUOverZ,
				rowVOverZ,
				uOverZDx,
				uOverZDy,
				vOverZDx,
				vOverZDy);
			return true;
		}

		private void DrawFrameTile(int tileIndex, ref RasterCounters counters)
		{
			int tileX = tileIndex % _tileColumns;
			int tileY = tileIndex / _tileColumns;
			int tileMinX = tileX * TileSize;
			int tileMinY = tileY * TileSize;
			int tileMaxX = System.Math.Min(tileMinX + TileSize - 1, _frameBuffer.Width - 1);
			int tileMaxY = System.Math.Min(tileMinY + TileSize - 1, _frameBuffer.Height - 1);
			List<int> bin = _tileBins[tileIndex];

			for (int i = 0; i < bin.Count; i++)
			{
				OpaqueTriangleCommand command = _opaqueTriangles[bin[i]];
				RasterSetup triangleSetup = command.Setup;
				int minX = System.Math.Max(tileMinX, triangleSetup.MinX);
				int maxX = System.Math.Min(tileMaxX, triangleSetup.MaxX);
				int minY = System.Math.Max(tileMinY, triangleSetup.MinY);
				int maxY = System.Math.Min(tileMaxY, triangleSetup.MaxY);
				RasterSetup tileSetup = SliceSetup(triangleSetup, minX, maxX, minY, maxY);

				if (command.Texture is Texture texture)
				{
					DrawOpaqueNearestTile(
						tileSetup,
						0,
						command.BaseColor,
						texture,
						command.AddressU,
						command.AddressV,
						ref counters);
				}
				else
				{
					DrawOpaqueSolidTile(
						tileSetup,
						0,
						ColorUtils.WithAlpha(command.BaseColor, 255),
						ref counters);
				}
			}
		}

		private static RasterSetup SliceSetup(
			in RasterSetup setup,
			int minX,
			int maxX,
			int minY,
			int maxY)
		{
			int offsetX = minX - setup.MinX;
			int offsetY = minY - setup.MinY;

			return setup with
			{
				MinX = minX,
				MaxX = maxX,
				MinY = minY,
				MaxY = maxY,
				RowW0 = setup.RowW0 + offsetX * setup.W0Dx + offsetY * setup.W0Dy,
				RowW1 = setup.RowW1 + offsetX * setup.W1Dx + offsetY * setup.W1Dy,
				RowW2 = setup.RowW2 + offsetX * setup.W2Dx + offsetY * setup.W2Dy,
				RowDepth = setup.RowDepth + offsetX * setup.DepthDx + offsetY * setup.DepthDy,
				RowUOverZ = setup.RowUOverZ + offsetX * setup.UOverZDx + offsetY * setup.UOverZDy,
				RowVOverZ = setup.RowVOverZ + offsetX * setup.VOverZDx + offsetY * setup.VOverZDy
			};
		}

		private void DrawOpaqueSolid(in RasterSetup setup, uint baseColor)
		{
			uint opaqueColor = ColorUtils.WithAlpha(baseColor, 255);
			int tileCount = GetTileCount(setup);

			#if !DEBUG
			if (tileCount >= ParallelTileThreshold && Environment.ProcessorCount > 1)
			{
				RasterSetup setupCopy = setup;
				Parallel.For(0, tileCount, tileIndex =>
				{
					RasterCounters ignored = default;
					DrawOpaqueSolidTile(setupCopy, tileIndex, opaqueColor, ref ignored);
				});
				return;
			}
			#endif

			RasterCounters counters = default;

			for (int tileIndex = 0; tileIndex < tileCount; tileIndex++)
				DrawOpaqueSolidTile(setup, tileIndex, opaqueColor, ref counters);

			#if DEBUG
			Stats.AddRasterization(
				counters.DepthTests,
				counters.DepthRejected,
				counters.PixelsDrawn);
			#endif
		}

		private void DrawOpaqueNearest(
			in RasterSetup setup,
			uint baseColor,
			Texture texture,
			in TextureSampler sampler)
		{
			TextureAddressMode addressU = sampler.AddressU;
			TextureAddressMode addressV = sampler.AddressV;
			int tileCount = GetTileCount(setup);

			#if !DEBUG
			if (tileCount >= ParallelTileThreshold && Environment.ProcessorCount > 1)
			{
				RasterSetup setupCopy = setup;
				Parallel.For(0, tileCount, tileIndex =>
				{
					RasterCounters ignored = default;
					DrawOpaqueNearestTile(
						setupCopy,
						tileIndex,
						baseColor,
						texture,
						addressU,
						addressV,
						ref ignored);
				});
				return;
			}
			#endif

			RasterCounters counters = default;

			for (int tileIndex = 0; tileIndex < tileCount; tileIndex++)
			{
				DrawOpaqueNearestTile(
					setup,
					tileIndex,
					baseColor,
					texture,
					addressU,
					addressV,
					ref counters);
			}

			#if DEBUG
			Stats.AddRasterization(
				counters.DepthTests,
				counters.DepthRejected,
				counters.PixelsDrawn);
			#endif
		}

		private void DrawOpaqueSolidTile(
			in RasterSetup setup,
			int tileIndex,
			uint color,
			ref RasterCounters counters)
		{
			GetTileBounds(
				setup,
				tileIndex,
				out int minX,
				out int maxX,
				out int minY,
				out int maxY,
				out int offsetX,
				out int offsetY);

			float tileW0 = setup.RowW0 + offsetX * setup.W0Dx + offsetY * setup.W0Dy;
			float tileW1 = setup.RowW1 + offsetX * setup.W1Dx + offsetY * setup.W1Dy;
			float tileW2 = setup.RowW2 + offsetX * setup.W2Dx + offsetY * setup.W2Dy;
			TileCoverage coverage = ClassifyTile(
				setup, tileW0, tileW1, tileW2, maxX - minX + 1, maxY - minY + 1);

			if (coverage == TileCoverage.Rejected)
				return;

			bool fullyCovered = coverage == TileCoverage.Full;
			float rowW0 = tileW0;
			float rowW1 = tileW1;
			float rowW2 = tileW2;
			float rowDepth = setup.RowDepth + offsetX * setup.DepthDx + offsetY * setup.DepthDy;

			for (int y = minY; y <= maxY; y++)
			{
				float w0 = rowW0;
				float w1 = rowW1;
				float w2 = rowW2;
				float depth = rowDepth;

				for (int x = minX; x <= maxX; x++)
				{
					if ((fullyCovered || (w0 >= 0f && w1 >= 0f && w2 >= 0f)) && depth > 0f)
					{
						#if DEBUG
						counters.DepthTests++;
						#endif

						if (_depthBuffer.TestAndSet(x, y, depth))
						{
							_frameBuffer.PutPixelUnChecked(x, y, color);
							#if DEBUG
							counters.PixelsDrawn++;
							#endif
						}
						#if DEBUG
						else
						{
							counters.DepthRejected++;
						}
						#endif
					}

					w0 += setup.W0Dx;
					w1 += setup.W1Dx;
					w2 += setup.W2Dx;
					depth += setup.DepthDx;
				}

				rowW0 += setup.W0Dy;
				rowW1 += setup.W1Dy;
				rowW2 += setup.W2Dy;
				rowDepth += setup.DepthDy;
			}
		}

		private void DrawOpaqueNearestTile(
			in RasterSetup setup,
			int tileIndex,
			uint baseColor,
			Texture texture,
			TextureAddressMode addressU,
			TextureAddressMode addressV,
			ref RasterCounters counters)
		{
			GetTileBounds(
				setup,
				tileIndex,
				out int minX,
				out int maxX,
				out int minY,
				out int maxY,
				out int offsetX,
				out int offsetY);

			float tileW0 = setup.RowW0 + offsetX * setup.W0Dx + offsetY * setup.W0Dy;
			float tileW1 = setup.RowW1 + offsetX * setup.W1Dx + offsetY * setup.W1Dy;
			float tileW2 = setup.RowW2 + offsetX * setup.W2Dx + offsetY * setup.W2Dy;
			TileCoverage coverage = ClassifyTile(
				setup, tileW0, tileW1, tileW2, maxX - minX + 1, maxY - minY + 1);

			if (coverage == TileCoverage.Rejected)
				return;

			bool fullyCovered = coverage == TileCoverage.Full;
			float rowW0 = tileW0;
			float rowW1 = tileW1;
			float rowW2 = tileW2;
			float rowDepth = setup.RowDepth + offsetX * setup.DepthDx + offsetY * setup.DepthDy;
			float rowUOverZ = setup.RowUOverZ + offsetX * setup.UOverZDx + offsetY * setup.UOverZDy;
			float rowVOverZ = setup.RowVOverZ + offsetX * setup.VOverZDx + offsetY * setup.VOverZDy;

			for (int y = minY; y <= maxY; y++)
			{
				float w0 = rowW0;
				float w1 = rowW1;
				float w2 = rowW2;
				float depth = rowDepth;
				float uOverZ = rowUOverZ;
				float vOverZ = rowVOverZ;

				for (int x = minX; x <= maxX; x++)
				{
					if ((fullyCovered || (w0 >= 0f && w1 >= 0f && w2 >= 0f)) && depth > 0f)
					{
						#if DEBUG
						counters.DepthTests++;
						#endif

						if (_depthBuffer.TestAndSet(x, y, depth))
						{
							float z = FastReciprocal(depth);
							uint texel = texture.SampleNearestUnchecked(
								uOverZ * z,
								vOverZ * z,
								addressU,
								addressV);
							uint pixelColor = ColorUtils.MultiplyRgbOpaque(baseColor, texel);

							_frameBuffer.PutPixelUnChecked(x, y, pixelColor);
							#if DEBUG
							counters.PixelsDrawn++;
							#endif
						}
						#if DEBUG
						else
						{
							counters.DepthRejected++;
						}
						#endif
					}

					w0 += setup.W0Dx;
					w1 += setup.W1Dx;
					w2 += setup.W2Dx;
					depth += setup.DepthDx;
					uOverZ += setup.UOverZDx;
					vOverZ += setup.VOverZDx;
				}

				rowW0 += setup.W0Dy;
				rowW1 += setup.W1Dy;
				rowW2 += setup.W2Dy;
				rowDepth += setup.DepthDy;
				rowUOverZ += setup.UOverZDy;
				rowVOverZ += setup.VOverZDy;
			}
		}

		private static int GetTileCount(in RasterSetup setup)
		{
			int columns = (setup.MaxX - setup.MinX) / TileSize + 1;
			int rows = (setup.MaxY - setup.MinY) / TileSize + 1;
			return checked(columns * rows);
		}

		private static void GetTileBounds(
			in RasterSetup setup,
			int tileIndex,
			out int minX,
			out int maxX,
			out int minY,
			out int maxY,
			out int offsetX,
			out int offsetY)
		{
			int columns = (setup.MaxX - setup.MinX) / TileSize + 1;
			int tileX = tileIndex % columns;
			int tileY = tileIndex / columns;
			offsetX = tileX * TileSize;
			offsetY = tileY * TileSize;
			minX = setup.MinX + offsetX;
			minY = setup.MinY + offsetY;
			maxX = System.Math.Min(minX + TileSize - 1, setup.MaxX);
			maxY = System.Math.Min(minY + TileSize - 1, setup.MaxY);
		}

		private static TileCoverage ClassifyTile(
			in RasterSetup setup,
			float w0,
			float w1,
			float w2,
			int width,
			int height)
		{
			float spanX = width - 1;
			float spanY = height - 1;
			GetEdgeRange(w0, setup.W0Dx, setup.W0Dy, spanX, spanY, out float min0, out float max0);
			GetEdgeRange(w1, setup.W1Dx, setup.W1Dy, spanX, spanY, out float min1, out float max1);
			GetEdgeRange(w2, setup.W2Dx, setup.W2Dy, spanX, spanY, out float min2, out float max2);

			if (max0 < 0f || max1 < 0f || max2 < 0f)
				return TileCoverage.Rejected;

			return min0 >= 0f && min1 >= 0f && min2 >= 0f
				? TileCoverage.Full
				: TileCoverage.Partial;
		}

		private static void GetEdgeRange(
			float start,
			float dx,
			float dy,
			float spanX,
			float spanY,
			out float minimum,
			out float maximum)
		{
			float x = dx * spanX;
			float y = dy * spanY;
			minimum = start + MathF.Min(0f, x) + MathF.Min(0f, y);
			maximum = start + MathF.Max(0f, x) + MathF.Max(0f, y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float FastReciprocal(float value)
		{
			float estimate = MathF.ReciprocalEstimate(value);
			return estimate * (2f - value * estimate);
		}

		private void DrawOpaqueFiltered(
			in RasterSetup setup,
			uint baseColor,
			Texture texture,
			in TextureSampler sampler,
			float mipLevel)
		{
			float rowW0 = setup.RowW0;
			float rowW1 = setup.RowW1;
			float rowW2 = setup.RowW2;
			float rowDepth = setup.RowDepth;
			float rowUOverZ = setup.RowUOverZ;
			float rowVOverZ = setup.RowVOverZ;
			TextureAddressMode addressU = sampler.AddressU;
			TextureAddressMode addressV = sampler.AddressV;
			bool useTrilinear = sampler.Filter == TextureFilterMode.Trilinear;
			#if DEBUG
			int depthTests = 0;
			int depthRejected = 0;
			int pixelsDrawn = 0;
			#endif

			for (int y = setup.MinY; y <= setup.MaxY; y++)
			{
				float w0 = rowW0;
				float w1 = rowW1;
				float w2 = rowW2;
				float depth = rowDepth;
				float uOverZ = rowUOverZ;
				float vOverZ = rowVOverZ;

				for (int x = setup.MinX; x <= setup.MaxX; x++)
				{
					if (w0 >= 0f && w1 >= 0f && w2 >= 0f && depth > 0f)
					{
						#if DEBUG
						depthTests++;
						#endif

						if (_depthBuffer.TestAndSet(x, y, depth))
						{
							float z = 1f / depth;
							float u = uOverZ * z;
							float v = vOverZ * z;
							uint texel = useTrilinear
								? texture.SampleTrilinearUnchecked(
									u, v, mipLevel, addressU, addressV)
								: texture.SampleBilinearUnchecked(
									u, v, addressU, addressV);
							uint pixelColor = ColorUtils.MultiplyRgbOpaque(baseColor, texel);

							_frameBuffer.PutPixelUnChecked(x, y, pixelColor);
							#if DEBUG
							pixelsDrawn++;
							#endif
						}
						else
						{
							#if DEBUG
							depthRejected++;
							#endif
						}
					}

					w0 += setup.W0Dx;
					w1 += setup.W1Dx;
					w2 += setup.W2Dx;
					depth += setup.DepthDx;
					uOverZ += setup.UOverZDx;
					vOverZ += setup.VOverZDx;
				}

				rowW0 += setup.W0Dy;
				rowW1 += setup.W1Dy;
				rowW2 += setup.W2Dy;
				rowDepth += setup.DepthDy;
				rowUOverZ += setup.UOverZDy;
				rowVOverZ += setup.VOverZDy;
			}

			#if DEBUG
			Stats.AddRasterization(depthTests, depthRejected, pixelsDrawn);
			#endif
		}

		private static float EstimateMipLevel(
			VertexOut v1,
			VertexOut v2,
			VertexOut v3,
			float depthDx,
			float depthDy,
			float uOverZDx,
			float uOverZDy,
			float vOverZDx,
			float vOverZDy,
			Texture texture)
		{
			const float oneThird = 1f / 3f;
			float depth = (v1.InverseDepth + v2.InverseDepth + v3.InverseDepth) * oneThird;

			if (depth <= 0f || !float.IsFinite(depth))
				return 0f;

			float uOverZ = (v1.UOverZ + v2.UOverZ + v3.UOverZ) * oneThird;
			float vOverZ = (v1.VOverZ + v2.VOverZ + v3.VOverZ) * oneThird;
			float inverseDepthSquared = 1f / (depth * depth);

			float duDx = (uOverZDx * depth - uOverZ * depthDx) * inverseDepthSquared;
			float duDy = (uOverZDy * depth - uOverZ * depthDy) * inverseDepthSquared;
			float dvDx = (vOverZDx * depth - vOverZ * depthDx) * inverseDepthSquared;
			float dvDy = (vOverZDy * depth - vOverZ * depthDy) * inverseDepthSquared;

			float footprintX = MathF.Sqrt(
				duDx * duDx * texture.Width * texture.Width +
				dvDx * dvDx * texture.Height * texture.Height);
			float footprintY = MathF.Sqrt(
				duDy * duDy * texture.Width * texture.Width +
				dvDy * dvDy * texture.Height * texture.Height);
			float footprint = MathF.Max(footprintX, footprintY);

			if (footprint <= 1f || !float.IsFinite(footprint))
				return 0f;

			return System.Math.Clamp(MathF.Log2(footprint), 0f, texture.MaxMipLevel);
		}

		public void DrawLine(VertexOut a, VertexOut b, uint color, float depthBias = 0f)
		{
			int x0 = (int)a.Position.X;
			int y0 = (int)a.Position.Y;

			int x1 = (int)b.Position.X;
			int y1 = (int)b.Position.Y;

			float z0 = a.InverseDepth;
			float z1 = b.InverseDepth;

			int dx = System.Math.Abs(x1 - x0);
			int dy = System.Math.Abs(y1 - y0);

			int steps = System.Math.Max(dx, dy);
			int currentStep = 0;

			if (steps == 0)
				steps = 1;

			int sx = x0 < x1 ? 1 : -1;
			int sy = y0 < y1 ? 1 : -1;

			int err = dx - dy;

			while (true)
			{
				float t = (float)currentStep / steps;
				float depth = (z0 + (z1 - z0) * t) - depthBias;

				if (x0 >= 0 &&
					x0 < _frameBuffer.Width &&
					y0 >= 0 &&
					y0 < _frameBuffer.Height)
				{
					Stats.IncrementDepthTests();
					if (_depthBuffer.TestAndSet(x0, y0, depth))
					{
						_frameBuffer.PutPixel(x0, y0, color);
						Stats.IncrementPixelsDrawn();
					}
					else
					{
						Stats.IncrementDepthRejected();
					}
				}

				if (x0 == x1 && y0 == y1)
					break;

				int e2 = 2 * err;

				if (e2 > -dy)
				{
					err -= dy;
					x0 += sx;
				}

				if (e2 < dx)
				{
					err += dx;
					y0 += sy;
				}

				currentStep++;
			}
		}

		private float Edge(Vector2 a, Vector2 b, Vector2 p)
		{
			return
				(p.X - a.X) * (b.Y - a.Y)
			  - (p.Y - a.Y) * (b.X - a.X);
		}
	}
}
