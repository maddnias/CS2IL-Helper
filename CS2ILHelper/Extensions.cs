using System;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace CS2ILHelper
{
	public static class Extensions
	{
		public static bool IsLdarg(this Instruction instr) {
			return instr.OpCode.Code == Code.Ldarg ||
			   instr.OpCode.Code == Code.Ldarga ||
			   instr.OpCode.Code == Code.Ldarga_S ||
			   instr.OpCode.Code == Code.Ldarg_0 ||
			   instr.OpCode.Code == Code.Ldarg_1 ||
			   instr.OpCode.Code == Code.Ldarg_2 ||
			   instr.OpCode.Code == Code.Ldarg_3 ||
			   instr.OpCode.Code == Code.Ldarg_S;
		}
		
		public static bool IsLdloc(this Instruction instr) {
			return instr.OpCode.Code == Code.Ldloc ||
			   instr.OpCode.Code == Code.Ldloca ||
			   instr.OpCode.Code == Code.Ldloca_S ||
			   instr.OpCode.Code == Code.Ldloc_0 ||
			   instr.OpCode.Code == Code.Ldloc_1 ||
			   instr.OpCode.Code == Code.Ldloc_2 ||
			   instr.OpCode.Code == Code.Ldloc_3 ||
			   instr.OpCode.Code == Code.Ldloc_S;
		}
		
		public static bool IsStloc(this Instruction instr) {
			return instr.OpCode.Code == Code.Stloc ||
			   instr.OpCode.Code == Code.Stloc_0 ||
			   instr.OpCode.Code == Code.Stloc_1 ||
			   instr.OpCode.Code == Code.Stloc_2 ||
			   instr.OpCode.Code == Code.Stloc_3 ||
			   instr.OpCode.Code == Code.Stloc_S;
		}
		
		public static VariableDefinition ResolveLocal(this Instruction instr, MethodBody body) {
			if(!instr.IsLdloc() && !instr.IsStloc())
				return null;
			if(!body.HasVariables)
				return null;
			
			if(instr.Operand != null)
				return instr.Operand as VariableDefinition;
			else
				switch(instr.OpCode.Code) {
					case Code.Stloc_0:
					case Code.Ldloc_0:
						return body.Variables[0];
					case Code.Stloc_1:
					case Code.Ldloc_1:
						return body.Variables[1];
					case Code.Stloc_2:
					case Code.Ldloc_2:
						return body.Variables[2];
					case Code.Stloc_3:
					case Code.Ldloc_3:
						return body.Variables[3];
			}
			
			return null;
		}
		
		public static ParameterDefinition ResolveParameter(this Instruction instr, MethodDefinition method) {
			if(!instr.IsLdarg () || !method.HasParameters)
				return null;
			
			if(instr.Operand != null)
				return instr.Operand as ParameterDefinition;
			else
				switch(instr.OpCode.Code) {
					case Code.Ldarg_0:
						return method.Parameters[0];
					case Code.Ldarg_1:
						return method.Parameters[1];
					case Code.Ldarg_2:
						return method.Parameters[2];
					case Code.Ldarg_3:
						return method.Parameters[3];
			}
			
			return null;
		}
	}
}

