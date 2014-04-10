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

namespace CS2ILHelper
{
	public class Disassembler
	{
		public string DisassembleMethod(string filename, bool comments) {
			
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
			
			foreach(var instr in method.Body.Instructions) {
				outString.AppendLine(string.Format ("  {0}{1}|{2}", instr.ToString (), (comments ? GenerateComment(instr, method) : null),
				                                    method.Body.Instructions.IndexOf (instr)));	
			}
			
			outString.AppendLine("}");
			return JsonConvert.SerializeObject(outString.ToString ());
		}
		
		public string GenerateComment(Instruction instr, MethodDefinition method) {
			if(instr.IsLdarg()) {
				var param = instr.ResolveParameter(method);
				return "\t // " + param.ParameterType.Name + " " + param.Name;
			}
			
			if(instr.IsStloc() || instr.IsLdloc()) {
				var local = instr.ResolveLocal(method.Body);
				return "\t // " + local.VariableType.Name + " " + local.Name;
			}
			
			return null;
		}
		
		sealed class CodeMap {
			public int Line;
			public List<Instruction> Instructions;
			public string[] Source;
			
			public CodeMap() {
			}
			
			public CodeMap(string[] source) {
				Instructions = new List<Instruction>();	
				Source = source;
			}
			
			public static CodeMap operator +(CodeMap map1, CodeMap map2) {
				if(map1.Line != map2.Line)
					throw new Exception("Code maps cannot be merged (line)");
				if(map1.Source != map2.Source)
					throw new Exception("Code maps cannot be merged (source)");
				
				map1.Instructions.AddRange (map2.Instructions);
				return map1;
			}
			
			public override string ToString (){
				var instructionParser = new StringBuilder();
				
				foreach(var instr in Instructions) {
					instructionParser.Append(instr.Offset + ",");
				}
				
				return string.Format (instructionParser.ToString ().TrimEnd (','));
			}
		}
		
		public string GetCodeMapping(string filename) {
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
			var codeMappings = new List<CodeMap>();
			var sourceCode = File.ReadAllLines("./files/test");

			CodeMap currentMap = null;
			
			foreach(var instr in method.Body.Instructions)
			{
				if(instr.SequencePoint != null)
				{
					if(currentMap != null)
						codeMappings.Add (currentMap);
					
					currentMap = new CodeMap(sourceCode);
					currentMap.Line = instr.SequencePoint.StartLine;
					currentMap.Instructions.Add (instr);
				} else {
					currentMap.Instructions.Add (instr);
				}
			}
			
			codeMappings.RemoveAll(x => x == null);
			MergeCodeMaps(codeMappings);
			
			var outString = new StringBuilder();
			
			foreach(var map in codeMappings) {
				outString.Append("_");
				outString.Append(map.Line -7);
				outString.Append("_");
				outString.Append("|");
				foreach(var instr in map.Instructions) {
					var idx = method.Body.Instructions.IndexOf (instr);
					outString.Append(idx+",");
				}
				outString.Remove (outString.Length -1, 1);
			}
			
			return JsonConvert.SerializeObject(outString.ToString());
		}
		
		// I'm sure there are better LINQy ways to do this...
		void MergeCodeMaps(List<CodeMap> maps) {
			for(var i = 0;i < maps.Count;i++) {
				var mergableMaps = new List<CodeMap>();
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

