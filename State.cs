using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

    struct State
    {
        public Vector2 position;
        public Vector2 velocity;  //Needs to be enforced that direction and velocity point in the same direction
        //Setting direction to be the arcTangent does not work since ship can be not moving.  You can't tell me the direction of <0,0>
        public float direction;  //is required to tell me the direction when ship isnt moving ie velocity = <0,0>
        public float maxSpeed;
        public float maxTurn;
    }