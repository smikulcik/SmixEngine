using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmixEngine.Colliders
{
    public class Collider
    {

        protected GameObject gameObject;
        protected Vector3[] rawVerts;
        protected Vector3[] verts;
        public BoundingBox bb;

        public Collider(GameObject g)
        {
            gameObject = g;
        }

        public virtual bool checkCollision(GameObject e)
        {
            return false;
        }


        protected void extractVerts()
        {
            int numVerts = 0;
            int offset = 0;
            foreach (ModelMesh mesh in gameObject.Model.Meshes)
            {

                //there may be multiple parts ... different materials etc...
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    numVerts += part.NumVertices;
                }
            }

            rawVerts = new Vector3[numVerts];
            verts = new Vector3[numVerts];
            foreach (ModelMesh mesh in gameObject.Model.Meshes)
            {

                //there may be multiple parts ... different materials etc...
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    int stride = part.VertexBuffer.VertexDeclaration.VertexStride;

                    //lets get the array of Vector3...  this will be an interlaced list of Position Normal Texture Position Normal Texture ....
                    Vector3[] tempVerts = new Vector3[part.NumVertices];

                    part.VertexBuffer.GetData(part.VertexOffset * stride, tempVerts, 0, tempVerts.Length, stride);

                    //the array of vertices are unique.  If a triangle reuses an exisiting vertex, it will not "reappear" in the list
                    //therefore we need to make a copy of each vertex used in every triangle...
                    ushort[] indices = new ushort[part.IndexBuffer.IndexCount];
                    part.IndexBuffer.GetData<ushort>(indices);

                    for (int i = 0; i < tempVerts.Length; i++)
                    {
                        rawVerts[offset] = tempVerts[i];
                        offset++;
                    }
                }
            }
        }
    }
}
