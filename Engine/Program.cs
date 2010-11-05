using System;
using Sputnik;

#if WINDOWS || XBOX
static class Program {
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	static void Main(string[] args) {
		using (Controller game = new Controller()) {
			game.Run();
		}
	}
}
#endif
