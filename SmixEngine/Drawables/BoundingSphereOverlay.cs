using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmixEngine.Drawables
{
    public class BoundingSphereBuffers
    {
        public VertexBuffer Vertices;
        public int VertexCount;
        public IndexBuffer Indices;
        public int PrimitiveCount;
    }
    class BoundingSphereOverlay
    {
        private static BoundingSphereBuffers CreateBoundingSphereBuffers(Vector3 center, float radius, GraphicsDevice graphicsDevice)
        {
            BoundingSphereBuffers boundingSphereBuffers = new BoundingSphereBuffers();

            boundingSphereBuffers.PrimitiveCount = 2015;
            boundingSphereBuffers.VertexCount = 2016;

            VertexBuffer vertexBuffer = new VertexBuffer(graphicsDevice,
                typeof(VertexPositionColor), boundingSphereBuffers.VertexCount,
                BufferUsage.WriteOnly);
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            
            for(float phi = -MathHelper.PiOver2; phi < MathHelper.PiOver2; phi += .1f)
            {
                for (float theta = 0; theta < MathHelper.TwoPi; theta += .1f)
                {
                    Vector3 toSurface = Vector3.Transform(Vector3.Forward * radius, Matrix.CreateFromYawPitchRoll(theta, phi, 0));
                    AddVertex(vertices, center+toSurface);
                }
            }
            vertexBuffer.SetData(vertices.ToArray());
            boundingSphereBuffers.Vertices = vertexBuffer;

            IndexBuffer indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, boundingSphereBuffers.VertexCount,
                BufferUsage.WriteOnly);
            indexBuffer.SetData(Enumerable.Range(0, boundingSphereBuffers.VertexCount).Select(i => (short)i).ToArray());
            boundingSphereBuffers.Indices = indexBuffer;

            return boundingSphereBuffers;
        }

        private static void AddVertex(List<VertexPositionColor> vertices, Vector3 position)
        {
            vertices.Add(new VertexPositionColor(position, Color.White));
        }

        private static void DrawBoundingSphere(BoundingSphereBuffers buffers, BasicEffect effect, GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
        {
            graphicsDevice.SetVertexBuffer(buffers.Vertices);
            graphicsDevice.Indices = buffers.Indices;

            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0,
                    buffers.VertexCount, 0, buffers.PrimitiveCount);
            }
        }

        public static void Draw(Vector3 center, float radius, GraphicsDevice g, Camera cam)
        {
            BasicEffect lineEffect = new BasicEffect(g);
            lineEffect.LightingEnabled = false;
            lineEffect.TextureEnabled = false;
            lineEffect.VertexColorEnabled = true;

            BoundingSphereBuffers bbb = CreateBoundingSphereBuffers(center, radius, g);

            DrawBoundingSphere(bbb, lineEffect, g, cam.getViewMatrix(), cam.getProjMatrix());
        }
    }
}
