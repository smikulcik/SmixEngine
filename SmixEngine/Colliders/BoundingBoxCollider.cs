using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SmixEngine.Colliders
{
    public class BoundingBoxCollider : Collider
    {
        public bool EnableBounce = true;
        public bool EnableRotate = true;

        public BoundingBoxCollider(GameObject g) : base(g)
        {
            extractVerts();
            Update();
        }

        //use new world transform
        public void Update()
        {
            Matrix transform = gameObject.Model.Meshes[0].ParentBone.Transform * gameObject.World;
            Vector3.Transform(rawVerts, ref transform, verts);

            bb = BoundingBox.CreateFromPoints(verts);
        }
        
        private double greatestMagInt(double n)
        {
            return Math.Sign(n) * Math.Floor(Math.Abs(n));
        }

        public override bool checkCollision(GameObject e)
        {
            //A quad is defined by 6 vertices (3 per triangle).  Since we assume both triangles are on the
            // same plane, it is not required to check the collision with the other triangle, so skip by 6.

            BoundingBox enemyBB = e.Collider.bb;

            Vector3 enemyCenter = (enemyBB.Max + enemyBB.Min) / 2;
            float enemyRadius = (enemyBB.Max - enemyBB.Min).Length() / 2;
            Vector3 enemyDims = enemyBB.Max - enemyBB.Min;

            Vector3 myCenter = (bb.Max + bb.Min) / 2;
            Vector3 myDims = bb.Max - bb.Min;

            //if too far away to collide
            if ((enemyCenter - myCenter).Length() > (myDims + enemyDims).Length())
                return false;

            Boolean collision = false;
            
            if (bb.Intersects(enemyBB))  //collision
            {
                Vector3 toEnemy = enemyCenter - myCenter;
                if(toEnemy.Length() == 0)
                {
                    return false;
                }

                //direction of intersection, must be scaled to my bounding box
                Vector3 scaledToEnemy = new Vector3(toEnemy.X / myDims.X, toEnemy.Y / myDims.Y, toEnemy.Z / myDims.Z);

                float maxDim = (float)Math.Max(Math.Max(Math.Abs(scaledToEnemy.X), Math.Abs(scaledToEnemy.Y)), Math.Abs(scaledToEnemy.Z))*.999f;
                Vector3 n = Vector3.Normalize(
                    new Vector3(
                        (float)(greatestMagInt(scaledToEnemy.X / maxDim)),
                        (float)(greatestMagInt(scaledToEnemy.Y / maxDim)),
                        (float)(greatestMagInt(scaledToEnemy.Z / maxDim))
                    )
                );


                // box analog of myRad + eRad - (center - e.center);
                //this is how much we must correct the enemy position
                Vector3 dist = Math.Abs(Vector3.Dot((myDims + enemyDims) / 2, n)) * n - Vector3.Dot(toEnemy, n) * n;

                e.Pos = e.Pos + dist * 1.01f ;

                if (e.Collider.EnableRotate)
                {
                    // do some random rotations for fun
                    Random r = new Random();
                    Vector3 axis = Vector3.Normalize(Vector3.Cross(toEnemy, n));
                    if (axis.Length() > 0)
                        e.AngVel *= Quaternion.CreateFromAxisAngle(axis, ((float)r.NextDouble() - .5f) / 4);
                }

                if (e.Collider.EnableBounce)
                {
                    Vector3 V = e.Vel;
                    V.Normalize();
                    V = 2 * (Vector3.Dot(-V, n))
                             * n + V;
                    if (!float.IsNaN(e.Vel.Length()) && !float.IsNaN(V.X))
                        e.Vel = e.Vel.Length() * V * .8f;
                }
                collision = true;
                //return collision;
            }
            return collision;
        }
    }
}
