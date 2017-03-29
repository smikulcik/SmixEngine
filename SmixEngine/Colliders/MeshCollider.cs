using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SmixEngine
{
    public class MeshCollider
    {
        GameObject gameObject;
        List<Vector3> verts;
        List<Vector3> norms;
        public BoundingBox bb;

        public bool EnableBounce = true;
        public bool EnableRotate = true;

        public MeshCollider(GameObject g)
        {
            gameObject = g;
            Update();
        }
        public void Update() {
            Vector3[] tempVerts;
            verts = new List<Vector3>();
            norms = new List<Vector3>();
            foreach (ModelMesh mesh in gameObject.Model.Meshes)
            {
                
                //there may be multiple parts ... different materials etc...
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    int stride = part.VertexBuffer.VertexDeclaration.VertexStride;

                    //lets get the array of Vector3...  this will be an interlaced list of Position Normal Texture Position Normal Texture ....
                    tempVerts = new Vector3[part.NumVertices];

                    part.VertexBuffer.GetData(part.VertexOffset * stride, tempVerts, 0, tempVerts.Length, stride);
                    
                    //the array of vertices are unique.  If a triangle reuses an exisiting vertex, it will not "reappear" in the list
                    //therefore we need to make a copy of each vertex used in every triangle...
                    ushort[] indices = new ushort[part.IndexBuffer.IndexCount];
                    part.IndexBuffer.GetData<ushort>(indices);
                    
                    Vector3 v = new Vector3();
                    for (int i = 0; i < indices.Length; i++)
                    {
                        v = tempVerts[indices[i]];
                        Vector3 transformedVert = Vector3.Transform(v, mesh.ParentBone.Transform * gameObject.World);
                        verts.Add(transformedVert);

                        //if we've added 3 new verts to the List of verts
                        //then add a new normal to the list
                        if (verts.Count % 3 == 0)
                        {
                            Vector3 Normal = Vector3.Cross(verts[verts.Count - 1] - verts[verts.Count - 3], verts[verts.Count - 2] - verts[verts.Count - 3]);
                            Normal.Normalize();
                            norms.Add(Normal);
                        }

                    }
                }
            }
            bb = BoundingBox.CreateFromPoints(verts);
        }
        
        public static bool SameSide(Vector3 p1, Vector3 p2, Vector3 a, Vector3 b)
        {
            Vector3 cp1 = Vector3.Cross(b - a, p1 - a);
            Vector3 cp2 = Vector3.Cross(b - a, p2 - a);
            if (Vector3.Dot(cp1, cp2) >= 0)
            {
                return true;
            }

            return false;
        }

        public static bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            if (SameSide(p, a, b, c) && SameSide(p, b, a, c) && SameSide(p, c, a, b))
            {
                return true;
            }

            return false;
        }

        public bool checkCollision(GameObject e)
        {
            //A quad is defined by 6 vertices (3 per triangle).  Since we assume both triangles are on the
            // same plane, it is not required to check the collision with the other triangle, so skip by 6.
            
            bool collision = false;
            for (int w = 0; w < verts.Count; w += 3)
            {


                
                Vector3 enemyCenter = (e.Collider.bb.Max + e.Collider.bb.Min) / 2;
                float enemyRadius = (e.Collider.bb.Max - e.Collider.bb.Min).Length() / 2;

                //sign of dist will determine which side of the wall we are on
                float dist = Vector3.Dot(norms[w / 3], (enemyCenter - verts[w]));

                Vector3 vertex = verts[w];


                if (Math.Abs(dist) < enemyRadius
                      && PointInTriangle(enemyCenter, verts[w], verts[w + 1], verts[w + 2]))  //collision
                {


                    if (dist < enemyRadius)
                    {
                        e.Pos = e.Pos + (norms[w / 3] * (enemyRadius - dist));
                    }
                    else if (dist < -enemyRadius)
                    {
                        e.Pos = e.Pos + (-norms[w / 3] * (enemyRadius + dist));
                    }

                    if (e.Collider.EnableRotate) {
                        Vector3 p2c = Vector3.Normalize(enemyCenter - (verts[w] + verts[w + 1] + verts[w + 2]) / 3 );
                        Vector3 tang = Vector3.Dot(norms[w / 3], p2c) * p2c;

                        e.AngVel = Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(p2c, tang)), e.Vel.Length() * .5f);
                    }

                    if (e.Collider.EnableBounce)
                    {
                        Vector3 V = e.Vel;
                        V.Normalize();
                        V = 2 * (Vector3.Dot(-V, norms[w / 3]))
                                 * norms[w / 3] + V;
                        if(!float.IsNaN(e.Vel.Length()) && !float.IsNaN(V.X))
                            e.Vel = e.Vel.Length() * V * .8f;
                    }
                    collision = true;
                    //return collision;
                }
            }
            return collision;
        }
    }
}
