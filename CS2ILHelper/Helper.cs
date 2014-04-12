using System;
using System.Threading;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CS2ILHelper
{
	public class Helper
	{
		public static void Main(string[] args) {			
			JArray errors;
			var success = new Compiler().Compile(args[0], args[1], args[2], out errors);
			var comments = args[3] == "on";
			
			if(!success)
			{
				Console.WriteLine(new JObject(new JProperty("Errors", errors)));
				return;
			}
			
			var disasm = new Disassembler();
			var disassembly = disasm.DisassembleMethod (args[1], comments);
			var codeMap = disasm.GetCodeMapping(args[1]);

			Console.WriteLine(new JObject(new JProperty("Disassembly", disassembly), new JProperty("CodeMap", codeMap), new JProperty("Errors", errors)));
			
			if(File.Exists (args[0]))
				File.Delete (args[0]);
			
			if(File.Exists (args[1]))
				File.Delete (args[1]);
			
			if(File.Exists (args[1] + ".mdb"))
				File.Delete(args[1] + ".mdb");
		}
	}
}

