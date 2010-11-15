using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sputnik
{
	class SaphereBoss : Boss
	{
		public SaphereBoss(GameEnvironment env) : base(env)
		{

		}

		public SaphereBoss(GameEnvironment env, SpawnPoint sp) : base(env, sp)
		{

		}

		public override void Update(float elapsedTime)
		{
			base.Update(elapsedTime);
		}
	}
}
