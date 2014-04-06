using System;
using System.Threading;
using System.IO;

namespace CS2ILHelper
{
	public class Helper
	{
		public static void Main(string[] args) {
			string errors;
			var success = new Compiler().Compile(args[0], args[1], out errors);
			
			if(!success)
			{
				Console.WriteLine("Compilation failed! Errors:" + Environment.NewLine + Environment.NewLine + errors);
				return;
			}
			
			var disasm = new Disassembler();
			Console.WriteLine (disasm.DisassembleEntrypoint (args[1]));
			
			if(File.Exists (args[0]))
				File.Delete (args[0]);
			
			if(File.Exists (args[1]))
				File.Delete (args[1]);
			
			if(File.Exists (args[1] + ".mdb"))
				File.Delete(args[1] + ".mdb");
		}
	}
}

