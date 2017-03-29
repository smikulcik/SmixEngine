using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmixEngine
{
    /// <summary>
    /// Marks position of light source
    /// It has an update to move the source
    /// </summary>
    public class Lamp : GameObject
    {
        public Lamp(Vector3 position)
            : base(position, Quaternion.Identity, 1f, null)
        {
        }

        public override void Update(GameTime gameTime)
        {
            Pos = Vector3.Transform(Pos, Matrix.CreateRotationY(.2f * (float)gameTime.ElapsedGameTime.TotalSeconds));
        }
        public override void Draw(GameTime gameTime, Camera cam, Lamp lamp)
        {

            //Game1.Models["earth"].Draw(Matrix.CreateTranslation(Pos), cam.getViewMatrix(), cam.getProjMatrix());
        }
    }
}
