using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Drawing;
using System.ComponentModel.Composition;
using ProjectMercury.EffectEditor.PluginInterfaces;
using ProjectMercury;
using ProjectMercury.Modifiers;
using Microsoft.Xna.Framework;

[Export(typeof(IModifierPlugin))]
public class ModifierPlugin : IModifierPlugin {
	/// <summary>
	/// Name of the plugin.
	/// </summary>
	public string Name {
		get { return "TractorBeamModifier"; }
	}

	/// <summary>
	/// Display name for plugin that appears in the editor.
	/// </summary>
	public string DisplayName {
		get { return "Tractor Beam Modifier"; }
	}


	/// <summary>
	/// Icon that will be displayed alongside the plugin in the editor.
	/// </summary>
	public Icon DisplayIcon {
		get { return SystemIcons.Information; }
	}

	/// <summary>
	/// Description to be displayed in the editor.
	/// </summary>
	public string Description {
		get { return "Sputnik Tractor Beam Modifier emitter."; }
	}

	/// <summary>
	/// Author name.
	/// </summary>
	public string Author {
		get { return "Jeff McGlynn"; }
	}

	/// <summary>
	/// Library name for group of plugins.
	/// </summary>
	public string Library {
		get { return "Sputnik"; }
	}

	/// <summary>
	/// Group name for modifier to be placed in.
	/// </summary>
	public string Category {
		get { return "Forces & Deflectors"; }
	}

	/// <summary>
	/// Version number of plugin.
	/// </summary>
	public Version Version {
		get { return new Version(1, 0, 0, 0); }
	}

	/// <summary>
	/// Minimum version number of ProjectMercury that this plugin needs.  Not used at type of writing,
	/// so 3.1.0.0 (as recommended) is returned.
	/// </summary>
	public Version MinimumRequiredVersion {
		get { return new Version(3, 1, 0, 0); }
	}

	// This is the method that creates a default instance of your custom modifier class....
	public Modifier CreateDefaultInstance() {
		return new TractorBeamModifier();
	}
}
