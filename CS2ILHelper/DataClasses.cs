using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CS2ILHelper
{
	public static class DataClasses
	{
		public sealed class CodeBlock
		{
			[JsonProperty("index")]
			public int Index { get; set; }
			[JsonProperty("block")]
			public string Block { get; set; }
			
			public CodeBlock() {
			}
			
			public CodeBlock(int index, string block) {
				Index = index;
				Block = block;
			}
		}
		
		public sealed class CodeMap 
		{
			[JsonProperty("line")]
			public int Line { get; set; }
			[JsonProperty("instructionindexes")]
			public List<int> InstructionIndexes { get; set; }
			
			public CodeMap() {
			}
			
			public CodeMap(string[] source) {
				InstructionIndexes = new List<int>();	
			}
			
			public static CodeMap operator +(CodeMap map1, CodeMap map2) {
				if(map1.Line != map2.Line)
					throw new Exception("Code maps cannot be merged (line)");
				
				map1.InstructionIndexes.AddRange (map2.InstructionIndexes);
				return map1;
			}
		}	
	}
}

