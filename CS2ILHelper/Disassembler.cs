using System;
using Mono.Cecil;
using System.Text;
using System.Linq;
using Mono.Cecil.Mdb;

namespace CS2ILHelper
{
	public class Disassembler
	{
		public string DisassembleEntrypoint(string filename) {
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
				outString.AppendLine("  " + instr.ToString ());	
			}
			
			outString.AppendLine("}");
			
			return outString.ToString ();
		}
		
	}
}

