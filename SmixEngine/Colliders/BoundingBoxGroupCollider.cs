using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SmixEngine.Colliders
{
    public class BoundingBoxGroupCollider : Collider
    {
        public bool EnableBounce = true;
        public bool EnableRotate = true;
        public bool UseMeshBoundingBox = true;  //use bounding box around mesh

        private BoundingBox[] rawbbgroup;
        private BoundingBox[] bbgroup;

        public BoundingBox[] BBGroup
        {
            get { return bbgroup; }
            set
            {
                rawbbgroup = value;
                Update();
            }
        }

        // give a group of bounding boxes to serve as your collision detection
        public BoundingBoxGroupCollider(GameObject g, BoundingBox[] bbgroup) : base(g)
        {
            this.bbgroup = bbgroup;
            if (UseMeshBoundingBox)
                extractVerts();
            Update();
        }

        //use new world transform
        public void Update()
        {
            if (UseMeshBoundingBox)
            {
                Matrix transform = gameObject.Model.Meshes[0].ParentBone.Transform * gameObject.World;
                Vector3.Transform(rawVerts, ref transform, verts);

                bb = BoundingBox.CreateFromPoints(verts);
            }
            if(rawbbgroup != null)
            {
                //shift the bounding box to the enemy's new position
                Matrix translation = Matrix.CreateTranslation(gameObject.Pos);
                bbgroup = new BoundingBox[rawbbgroup.Length];
                for (int i = 0; i < rawbbgroup.Length; i++)
                {
                    bbgroup[i] = new BoundingBox(
                        Vector3.Transform(rawbbgroup[i].Min, translation),
                        Vector3.Transform(rawbbgroup[i].Max, translation)
                    );
                }
            }

        }

        private double greatestMagInt(double n)
        {
            return Math.Sign(n) * Math.Floor(Math.Abs(n));
        }

        public override bool checkCollision(GameObject e)
        {
            bool collision = false;

            BoundingBox enemyBB = e.Collider.bb;
            BoundingBox myBB = bb;

            //handle mesh bounding box collisions
            if(UseMeshBoundingBox && e.Collider.UseMeshBoundingBox)
            {
                collision |= checkCollisionBB2BB(bb, e.Collider.bb, e);
            }
            if (UseMeshBoundingBox)
            {
                //if I use mesh bb, then check with his group
                foreach (BoundingBox b in e.Collider.bbgroup)
                {
                    collision |= checkCollisionBB2BB(bb, b, e);
                }
            }
            if (e.Collider.UseMeshBoundingBox)
            {
                //if enemy uses mesh bb, then check his with my group
                foreach (BoundingBox b in bbgroup)
                {
                    collision |= checkCollisionBB2BB(b, e.Collider.bb, e);
                }
            }

            //now handle other bounding box collisions

            foreach (BoundingBox mybb in bbgroup)
            {
                foreach (BoundingBox enemybb in e.Collider.bbgroup)
                {
                    collision |= checkCollisionBB2BB(mybb, enemybb, e);
                }
            }

            return collision;

        }
        private bool checkCollisionBB2BB(BoundingBox myBB, BoundingBox enemyBB, GameObject e)
        {
            Vector3 enemyCenter = (enemyBB.Max + enemyBB.Min) / 2;
            float enemyRadius = (enemyBB.Max - enemyBB.Min).Length() / 2;
            Vector3 enemyDims = enemyBB.Max - enemyBB.Min;

            Vector3 myCenter = (myBB.Max + myBB.Min) / 2;
            Vector3 myDims = myBB.Max - myBB.Min;

            //if too far away to collide
            if ((enemyCenter - myCenter).Length() > (myDims + enemyDims).Length())
                return false;

            Boolean collision = false;

            if (myBB.Intersects(enemyBB))  //collision
            {
                Vector3 toEnemy = enemyCenter - myCenter;
                if (toEnemy.Length() == 0)
                {
                    return false;
                }

                //direction of intersection, must be scaled to my bounding box
                Vector3 scaledToEnemy = new Vector3(toEnemy.X / myDims.X, toEnemy.Y / myDims.Y, toEnemy.Z / myDims.Z);

                float maxDim = (float)Math.Max(Math.Max(Math.Abs(scaledToEnemy.X), Math.Abs(scaledToEnemy.Y)), Math.Abs(scaledToEnemy.Z)) * .999f;
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

                e.Pos = e.Pos + dist * 1.01f;

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
                        e.Vel = e.Vel.Length() * V * gameObject.Friction;
                }
                collision = true;
            }
            return collision;
        }
    }
}
