// Derived from https://github.com/Xiexe/Xiexes-Unity-Shaders
// with Xiexe's permission. For compatibility's sake, though,
// I've kept the namespaces seperate but similar. 
// SCSS multi gradients are expected to have 8/16 instead of 5 entries

using System.Collections.Generic;
using UnityEngine;
namespace SilentCelShading.Unity
{
	public class SCSSMultiGradient : ScriptableObject {
		public string uniqueName = "New Gradient";
		public List<Gradient> gradients = new List<Gradient>();
		public List<int> order = new List<int>();
	}
}