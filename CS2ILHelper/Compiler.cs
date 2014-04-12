using System;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace CS2ILHelper
{
	public class Compiler
	{
		private CSharpCodeProvider _provider;
		
		public bool Compile(string file, string output, string version, out JArray errors) {
			_provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v" + version[0] + "." + version[1] } });
			
			var src = System.IO.File.ReadAllText (file);
			var @params = new CompilerParameters() { CompilerOptions = "/target:exe /unsafe", GenerateExecutable = true, OutputAssembly = output };
			
			@params.IncludeDebugInformation = true;
			@params.TreatWarningsAsErrors = false;
			@params.WarningLevel = 3;
			
			string references = src.Split (new[] { "---CODE---" }, StringSplitOptions.None)[0];
			string code = "";
			
			@params.ReferencedAssemblies.Add ("mscorlib.dll");
			
			foreach(var @ref in references.Split(new[] { "\r\n" }, StringSplitOptions.None))
				code += "using " + @ref + ";" + Environment.NewLine;

			code += "namespace CS2ILStub " + Environment.NewLine +
				"{" + Environment.NewLine +
				"public class Stub" + Environment.NewLine +
				"{" + Environment.NewLine +
				"public static void Main(string[] args){}" + Environment.NewLine;
			code += src.Split (new[] { "---CODE---" }, StringSplitOptions.None)[1];
			code += "}" + Environment.NewLine + "}";
			
			var result = _provider.CompileAssemblyFromSource(@params, code);
			
			
			errors = new JArray();
			
			for(var i = 0;i < result.Errors.Count;i++)
				errors.Add (JObject.FromObject(new DataClasses.CodeBlock(i, result.Errors[i].ToString ())));
			return !result.Errors.HasErrors;
		}
		
	}
}
