using System;
using System.Threading;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CS2ILHelper
{
	public class Helper
	{
		public static void Main(string[] args) {			
			string errors, sanitized;
			var success = new Compiler().Compile(args[0], args[1], args[2], out sanitized, out errors);
			var comments = args[3] == "on";
			
			if(!success)
			{
				Console.WriteLine("Compilation failed! Errors:" + Environment.NewLine + Environment.NewLine + errors);
				return;
			}
			
			var disasm = new Disassembler();
			var disassembly = disasm.DisassembleMethod (args[1], comments);
			var codeMap = disasm.GetCodeMapping(args[1]);

			Console.WriteLine (new JObject(new JProperty("Disassembly", disassembly), new JProperty("CodeMap", codeMap), new JProperty("Source", sanitized)));
			
			if(File.Exists (args[0]))
				File.Delete (args[0]);
			
			if(File.Exists (args[1]))
				File.Delete (args[1]);
			
			if(File.Exists (args[1] + ".mdb"))
				File.Delete(args[1] + ".mdb");
			
			if(File.Exists ("./files/source"))
				File.Delete ("./files/source");
		}
	}
}

