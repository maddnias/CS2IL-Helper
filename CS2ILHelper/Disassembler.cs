using System;
using Mono.Cecil;
using System.Text;
using System.Linq;
using Mono.Cecil.Mdb;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using System.IO;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CS2ILHelper
{
	public class Disassembler
	{			
		public JArray DisassembleMethod(string filename, bool comments) {
			var asmDef = AssemblyDefinition.ReadAssembly (filename);
			asmDef.MainModule.ReadSymbols ();
			var method = asmDef.EntryPoint.DeclaringType.Methods[2];
			var outString = new StringBuilder();
			
			outString.Append (".method ");
			
			if(method.IsPublic)
				outString.Append("public ");
			else if(method.IsPrivate)
				outString.Append("private ");
			
			if(method.IsHideBySig)
				outString.Append("hidebysig ");
			
			if(method.IsStatic)
				outString.Append("static ");
			
			outString.Append(method.ReturnType.Name + " ");
			outString.Append(method.Name + "(");
			
			foreach(var param in method.Parameters)
				outString.AppendFormat("{0} {1}{2}", param.ParameterType.Name, param.Name, (param == method.Parameters.Last() ? ") " : ", "));
			
			if(method.IsIL)
				outString.Append("cil ");
			
			if(method.IsManaged)
				outString.AppendLine("managed");
			
			outString.AppendLine("{");
			outString.AppendLine ("  .maxstack " + method.Body.MaxStackSize);
			
			if(method.Body.HasVariables) {
				
				outString.AppendLine("  .locals init (");
				
				foreach(var @var in method.Body.Variables) {
						outString.AppendFormat("    [{0}] {1} {2}{3}", @var.Index, @var.VariableType.Name, @var.Name, 
					                       (@var == method.Body.Variables.Last() ? ")" + Environment.NewLine : "," + Environment.NewLine));
				}
			}
			
			var outDict = new JArray();
			var lines = outString.ToString ().Split (new[] { Environment.NewLine }, StringSplitOptions.None);
			
			foreach(var line in lines)
				outDict.Add(JObject.FromObject(new DataClasses.CodeBlock(-1, line )));
			
			var count = 0;
			foreach(var instr in method.Body.Instructions) {
				outDict.Add (JObject.FromObject(new DataClasses.CodeBlock(count++, 
				                        string.Format("  {0}{1}", instr.ToString (), (comments ? instr.GenerateComment(method) : null)))));
			}
			
			outDict.Add (JObject.FromObject(new DataClasses.CodeBlock(-1, "}")));
			
			return outDict;
		}
			
		public JArray GetCodeMapping(string filename) {
			ModuleDefinition modDef;
			
			using (var symbolStream = File.OpenRead(filename + ".mdb"))
			{
				var readerParameters = new ReaderParameters
				{
					ReadSymbols = true,
					SymbolReaderProvider = new MdbReaderProvider()
				};
				modDef = ModuleDefinition.ReadModule(filename, readerParameters);
			}
			
			var method = modDef.EntryPoint.DeclaringType.Methods[2];
			var codeMappings = new List<DataClasses.CodeMap>();
			var sourceCode = File.ReadAllLines("./files/test");

			DataClasses.CodeMap currentMap = null;
			
			foreach(var instr in method.Body.Instructions)
			{
				if(instr.SequencePoint != null)
				{
					if(currentMap != null)
						codeMappings.Add (currentMap);
					
					currentMap = new DataClasses.CodeMap(sourceCode);
					// Skip source overhead
					currentMap.Line = instr.SequencePoint.StartLine - 7;
					currentMap.InstructionIndexes.Add (method.Body.Instructions.IndexOf(instr));
				} else {
					currentMap.InstructionIndexes.Add (method.Body.Instructions.IndexOf(instr));
				}
			}
			
			codeMappings.RemoveAll(x => x == null);
			MergeCodeMaps(codeMappings);
			
			var outArr = new JArray();		
			foreach(var map in codeMappings) {
				outArr.Add (JObject.FromObject(map, new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
			}
			
			return outArr;
		}
		
		// I'm sure there are better LINQy ways to do this...
		void MergeCodeMaps(List<DataClasses.CodeMap> maps) {
			for(var i = 0;i < maps.Count;i++) {
				var mergableMaps = new List<DataClasses.CodeMap>();
				if((mergableMaps = maps.FindAll(x => x.Line == maps[i].Line)).Count > 1) {
					foreach(var mergableMap in mergableMaps.Where(x => x != maps[i])) {
						maps[i] += mergableMap;
						maps.Remove (mergableMap);
					}
				}
			}
		}
	}
}

