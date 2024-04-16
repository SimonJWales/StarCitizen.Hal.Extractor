#define NONULL

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace unforge
{
    public class ClassMapping
    {
        public XmlNode? Node { get; set; }

        public ushort StructIndex { get; set; }

        public int RecordIndex { get; set; }
    }

    public class DataForge : IEnumerable
	{
        internal BinaryReader _br;

        internal bool IsLegacy { get; set; }

        internal int FileVersion { get; set; }

        internal DataForgeStructDefinition[] StructDefinitionTable { get; set; }

        internal DataForgePropertyDefinition[] PropertyDefinitionTable { get; set; }

        internal DataForgeEnumDefinition[] EnumDefinitionTable { get; set; }

        internal DataForgeDataMapping[] DataMappingTable { get; set; }

        internal DataForgeRecord[] RecordDefinitionTable { get; set; }

        internal DataForgeStringLookup[] EnumOptionTable { get; set; }

        internal DataForgeString[] ValueTable { get; set; }

        internal DataForgeReference[] Array_ReferenceValues { get; set; }

        internal DataForgeGuid[] Array_GuidValues { get; set; }

        internal DataForgeStringLookup[] Array_StringValues { get; set; }

        internal DataForgeLocale[] Array_LocaleValues { get; set; }

        internal DataForgeEnum[] Array_EnumValues { get; set; }

        internal DataForgeInt8[] Array_Int8Values { get; set; }

        internal DataForgeInt16[] Array_Int16Values { get; set; }

        internal DataForgeInt32[] Array_Int32Values { get; set; }

        internal DataForgeInt64[] Array_Int64Values { get; set; }

        internal DataForgeUInt8[] Array_UInt8Values { get; set; }

        internal DataForgeUInt16[] Array_UInt16Values { get; set; }

        internal DataForgeUInt32[] Array_UInt32Values { get; set; }

        internal DataForgeUInt64[] Array_UInt64Values { get; set; }

        internal DataForgeBoolean[] Array_BooleanValues { get; set; }

        internal DataForgeSingle[] Array_SingleValues { get; set; }

        internal DataForgeDouble[] Array_DoubleValues { get; set; }

        internal DataForgePointer[] Array_StrongValues { get; set; }

        internal DataForgePointer[] Array_WeakValues { get; set; }

        internal Dictionary<uint, string> ValueMap { get; set; }

        internal Dictionary<uint, List<XmlElement>> DataMap { get; set; }

        internal List<ClassMapping> Require_ClassMapping { get; set; }

        internal List<ClassMapping> Require_StrongMapping { get; set; }

        internal List<ClassMapping> Require_WeakMapping1 { get; set; }

        internal List<ClassMapping> Require_WeakMapping2 { get; set; }

        internal List<XmlElement> DataTable { get; set; }

        internal U[]? ReadArray<U>(int arraySize) where U : _DataForgeSerializable
        {
            if (arraySize == -1)
            {
                return null;
            }

            return (from i in Enumerable.Range(0, arraySize)
                    let data = (U)Activator.CreateInstance(typeof(U), this)
                    // let hack = data._index = i
                    select data).ToArray();
        }

		public DataForge(BinaryReader br, bool legacy = false)
		{
			_br = br;

			var temp00 = _br.ReadInt32();

			FileVersion = _br.ReadInt32();

			IsLegacy = legacy;

			Require_ClassMapping = new List<ClassMapping> { };

			Require_StrongMapping = new List<ClassMapping> { };

			Require_WeakMapping1 = new List<ClassMapping> { };

			Require_WeakMapping2 = new List<ClassMapping> { };


			if (!IsLegacy)
			{
				var atemp1 = _br.ReadUInt16();

				var atemp2 = _br.ReadUInt16();

				var atemp3 = _br.ReadUInt16();

				var atemp4 = _br.ReadUInt16();
			}

			var structDefinitionCount = _br.ReadInt32();

			var propertyDefinitionCount = _br.ReadInt32();

			var enumDefinitionCount = _br.ReadInt32();

			var dataMappingCount = _br.ReadInt32();

			var recordDefinitionCount = _br.ReadInt32();

			var booleanValueCount = _br.ReadInt32();

			var int8ValueCount = _br.ReadInt32();

			var int16ValueCount = _br.ReadInt32();

			var int32ValueCount = _br.ReadInt32();

			var int64ValueCount = _br.ReadInt32();

			var uint8ValueCount = _br.ReadInt32();

			var uint16ValueCount = _br.ReadInt32();

			var uint32ValueCount = _br.ReadInt32();

			var uint64ValueCount = _br.ReadInt32();

			var singleValueCount = _br.ReadInt32();

			var doubleValueCount = _br.ReadInt32();

			var guidValueCount = _br.ReadInt32();

			var stringValueCount = _br.ReadInt32();

			var localeValueCount = _br.ReadInt32();

			var enumValueCount = _br.ReadInt32();

			var strongValueCount = _br.ReadInt32();

			var weakValueCount = _br.ReadInt32();

			var referenceValueCount = _br.ReadInt32();

			var enumOptionCount = _br.ReadInt32();

			var textLength = _br.ReadUInt32();

			var unknown = (IsLegacy) ? 0 : _br.ReadUInt32();

			StructDefinitionTable = ReadArray<DataForgeStructDefinition>(structDefinitionCount)!;
			
            PropertyDefinitionTable = ReadArray<DataForgePropertyDefinition>(propertyDefinitionCount)!;
			
            EnumDefinitionTable = ReadArray<DataForgeEnumDefinition>(enumDefinitionCount)!;
			
            DataMappingTable = ReadArray<DataForgeDataMapping>(dataMappingCount)!;
			
            RecordDefinitionTable = ReadArray<DataForgeRecord>(recordDefinitionCount)!;

            Array_Int8Values = ReadArray<DataForgeInt8>(int8ValueCount)!;
            
            Array_Int16Values = ReadArray<DataForgeInt16>(int16ValueCount)!;
            
            Array_Int32Values = ReadArray<DataForgeInt32>(int32ValueCount)!;
            
            Array_Int64Values = ReadArray<DataForgeInt64>(int64ValueCount)!;
            
            Array_UInt8Values = ReadArray<DataForgeUInt8>(uint8ValueCount)!;
            
            Array_UInt16Values = ReadArray<DataForgeUInt16>(uint16ValueCount)!;
            
            Array_UInt32Values = ReadArray<DataForgeUInt32>(uint32ValueCount)!;
            
            Array_UInt64Values = ReadArray<DataForgeUInt64>(uint64ValueCount)!;
            
            Array_BooleanValues = ReadArray<DataForgeBoolean>(booleanValueCount)!;
            
            Array_SingleValues = ReadArray<DataForgeSingle>(singleValueCount)!;
            
            Array_DoubleValues = ReadArray<DataForgeDouble>(doubleValueCount)!;
            
            Array_GuidValues = ReadArray<DataForgeGuid>(guidValueCount)!;
            
            Array_StringValues = ReadArray<DataForgeStringLookup>(stringValueCount)!;
            
            Array_LocaleValues = ReadArray<DataForgeLocale>(localeValueCount)!;
            
            Array_EnumValues = ReadArray<DataForgeEnum>(enumValueCount)!;
            
            Array_StrongValues = ReadArray<DataForgePointer>(strongValueCount)!;
            
            Array_WeakValues = ReadArray<DataForgePointer>(weakValueCount)!;

            Array_ReferenceValues = ReadArray<DataForgeReference>(referenceValueCount)!;

            EnumOptionTable = ReadArray<DataForgeStringLookup>(enumOptionCount)!;

            var buffer = new List<DataForgeString> { };

            var maxPosition = _br.BaseStream.Position + textLength;

            var startPosition = _br.BaseStream.Position;

            ValueMap = new Dictionary<uint, string> { };

            while (_br.BaseStream.Position < maxPosition)
            {
                var offset = _br.BaseStream.Position - startPosition;

                var dfString = new DataForgeString(this);

                buffer.Add(dfString);

                ValueMap[(uint)offset] = dfString.Value;
            }

            ValueTable = buffer.ToArray();

            DataTable = new List<XmlElement> { };

            DataMap = new Dictionary<uint, List<XmlElement>> { };

            foreach (var dataMapping in DataMappingTable)
            {
                DataMap[dataMapping.StructIndex] = new List<XmlElement> { };

                var dataStruct = StructDefinitionTable[dataMapping.StructIndex];

                for (int i = 0; i < dataMapping.StructCount; i++)
                {
                    var node = dataStruct.Read(dataMapping.Name);

                    DataMap[dataMapping.StructIndex].Add(node);

                    DataTable.Add(node);
                }
            }

            foreach (var dataMapping in Require_ClassMapping)
            {
                if (dataMapping.StructIndex == 0xFFFF)
                {
#if NONULL
                    dataMapping.Node!.ParentNode.RemoveChild(dataMapping.Node);
#else
                    dataMapping.Item1.ParentNode.ReplaceChild(
                        this._xmlDocument.CreateElement("null"),
                        dataMapping.Item1);
#endif
                }
                else if (DataMap.ContainsKey(dataMapping.StructIndex) && 
                    DataMap[dataMapping.StructIndex].Count > dataMapping.RecordIndex)
                {
                    dataMapping.Node!.ParentNode.ReplaceChild(
                        DataMap[dataMapping.StructIndex][dataMapping.RecordIndex],
                        dataMapping.Node);
                }
                else
                {
                    var bugged = _xmlDocument.CreateElement("bugged");

                    var __class = _xmlDocument.CreateAttribute("__class");

                    var __index = _xmlDocument.CreateAttribute("__index");

                    __class.Value = $"{dataMapping.StructIndex:X8}";

                    __index.Value = $"{dataMapping.RecordIndex:X8}";

                    bugged.Attributes.Append(__class);

                    bugged.Attributes.Append(__index);

                    dataMapping.Node!.ParentNode.ReplaceChild(
                        bugged,
                        dataMapping.Node);
                }
            }
        }

        XmlDocument _xmlDocument = new XmlDocument();

		internal XmlElement CreateElement(string name) { return _xmlDocument.CreateElement(name); }

        internal XmlAttribute CreateAttribute(string name) { return _xmlDocument.CreateAttribute(name); }

        public string OuterXML
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_xmlDocument?.InnerXml)) Compile();

				return _xmlDocument!.OuterXml;
			}
		}

        public void Save(string filename)
        {
			if (string.IsNullOrWhiteSpace(_xmlDocument?.InnerXml)) Compile();
			
			var i = 0;

			foreach (var record in RecordDefinitionTable)
			{
				var fileReference = record.FileName;

				if (fileReference.Split('/').Length == 2) fileReference = fileReference.Split('/')[1];

				if (string.IsNullOrWhiteSpace(fileReference)) fileReference = string.Format(@"Dump\{0}_{1}.xml", record.Name, i++);

				var newPath = Path.Combine(Path.GetDirectoryName(filename), fileReference);

				if (!Directory.Exists(Path.GetDirectoryName(newPath))) Directory.CreateDirectory(Path.GetDirectoryName(newPath));

				XmlDocument doc = new XmlDocument { };

				doc.LoadXml(DataMap[record.StructIndex][record.VariantIndex].OuterXml);

				doc.Save(newPath);
			}

			_xmlDocument!.Save(filename);
        }

		internal void Compile()
		{
			var root = _xmlDocument.CreateElement("DataForge");

			_xmlDocument.AppendChild(root);

			foreach (var dataMapping in Require_StrongMapping)
			{
				var strong = Array_StrongValues[dataMapping.RecordIndex];

				if (strong.Index == 0xFFFFFFFF)
				{
#if NONULL
					dataMapping.Node!.ParentNode.RemoveChild(dataMapping.Node);
#else
                    dataMapping.Item1.ParentNode.ReplaceChild(
                        this._xmlDocument.CreateElement("null"),
                        dataMapping.Item1);
#endif
				}
				else
				{
					dataMapping.Node!.ParentNode.ReplaceChild(
						DataMap[strong.StructType][(int)strong.Index],
						dataMapping.Node);
				}
			}

			foreach (var dataMapping in Require_WeakMapping1)
			{
				var weak = Array_WeakValues[dataMapping.RecordIndex];

				var weakAttribute = dataMapping.Node;

				if (weak.Index == 0xFFFFFFFF)
				{
					weakAttribute!.Value = string.Format("0");
				}
				else
				{
					var targetElement = DataMap[weak.StructType][(int)weak.Index];

					weakAttribute!.Value = targetElement.GetPath();
				}
			}

			foreach (var dataMapping in Require_WeakMapping2)
			{
				var weakAttribute = dataMapping.Node;

				if (dataMapping.StructIndex == 0xFFFF)
				{
					weakAttribute!.Value = "null";
				}
				else if (dataMapping.RecordIndex == -1)
				{
					var targetElement = DataMap[dataMapping.StructIndex];

					weakAttribute!.Value = targetElement.FirstOrDefault()?.GetPath();
				}
				else
				{
					var targetElement = DataMap[dataMapping.StructIndex][dataMapping.RecordIndex];

					weakAttribute!.Value = targetElement.GetPath();
				}
			}

			var i = 0;
			foreach (var record in RecordDefinitionTable)
			{
				var fileReference = record.FileName;

				if (fileReference.Split('/').Length == 2)
				{
					fileReference = fileReference.Split('/')[1];
				}

				//if (!record.FileName.ToLowerInvariant()
                //  .Contains(record.Name.ToLowerInvariant()) &&
				//	!record.FileName.ToLowerInvariant().Contains(
                //	    record.Name.Split(
                //	        new[] { '.' },
                //	        StringSplitOptions.RemoveEmptyEntries).Last().ToLowerInvariant()))
				//{
				//	Console.WriteLine("Warning {0} doesn't match {1}", record.Name, record.FileName);
				//}

				if (string.IsNullOrWhiteSpace(fileReference))
				{
					fileReference = string.Format(@"Dump\{0}_{1}.xml", record.Name, i++);
				}

				if (record.Hash.HasValue && record.Hash != Guid.Empty)
				{
					var hash = CreateAttribute("__ref");

					hash.Value = $"{record.Hash}";

					DataMap[record.StructIndex][record.VariantIndex].Attributes.Append(hash);
				}

				if (!string.IsNullOrWhiteSpace(record.FileName))
				{
					var path = CreateAttribute("__path");

					path.Value = $"{record.FileName}";

					DataMap[record.StructIndex][record.VariantIndex].Attributes.Append(path);
				}
				
				DataMap[record.StructIndex][record.VariantIndex] = 
                    DataMap[record.StructIndex][record.VariantIndex].Rename(record.Name);

				root.AppendChild(DataMap[record.StructIndex][record.VariantIndex]);
			}
		}

		public Stream GetStream()
		{
			if (string.IsNullOrWhiteSpace(_xmlDocument?.InnerXml)) Compile();

			var outStream = new MemoryStream();

			_xmlDocument!.Save(outStream);

			return outStream;
		}

		public void GenerateSerializationClasses(
            string path = "AutoGen", 
            string assemblyName = "HoloXPLOR.Data.DataForge")
        {
            path = new DirectoryInfo(path).FullName;

            if (Directory.Exists(path) && 
                path != new DirectoryInfo(".").FullName)
            {
                Directory.Delete(path, true);

                //  comment: rework as async wait on completion
                while (Directory.Exists(path))
                {
                    Thread.Sleep(100);
                }
            }

            Directory.CreateDirectory(path);

            while (!Directory.Exists(path))
            {
                // TODO: rework as async wait on completion
                Thread.Sleep(100);
            }

            var sb = new StringBuilder();

            sb.AppendLine(@"using System.Xml.Serialization;");

            sb.AppendLine();

            sb.AppendFormat(@"namespace {0}", assemblyName);

            sb.AppendLine();

            sb.AppendLine(@"{");

            foreach (var enumDefinition in EnumDefinitionTable)
            {
                sb.Append(enumDefinition.Export());
            }
            sb.AppendLine(@"}");

            File.WriteAllText(Path.Combine(path, "Enums.cs"), sb.ToString());

            sb = new StringBuilder();

            sb.AppendLine(@"using System;");

            sb.AppendLine(@"using System.Xml.Serialization;");

            sb.AppendLine();

            sb.AppendFormat(@"namespace {0}", assemblyName);

            sb.AppendLine();

            sb.AppendLine(@"{");

            foreach (EDataType typeDefinition in Enum.GetValues(typeof(EDataType)))
            {
                var typeName = typeDefinition.ToString().Replace("var", "");
                switch (typeDefinition)
                {
                    case EDataType.varStrongPointer:
                    case EDataType.varClass: break;
                    case EDataType.varLocale:
                    case EDataType.varWeakPointer:
                        sb.AppendFormat(@"    public class _{0}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    {");
                        sb.AppendLine(@"        public String Value { get; set; }");
                        sb.AppendLine(@"    }");
                        break;
                    case EDataType.varReference:
                        sb.AppendFormat(@"    public class _{0}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    {");
                        sb.AppendLine(@"        public Guid Value { get; set; }");
                        sb.AppendLine(@"    }");
                        break;
                    default:
                        sb.AppendFormat(@"    public class _{0}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    {");
                        sb.AppendFormat(@"        public {0} Value {{ get; set; }}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    }");
                        break;
                }
            }
            sb.AppendLine(@"}");

            File.WriteAllText(Path.Combine(path, "Arrays.cs"), sb.ToString());

            foreach (var structDefinition in StructDefinitionTable)
            {
                var code = structDefinition.Export(assemblyName);

                File.WriteAllText(Path.Combine(
                    path, 
                    string.Format(
                        "{0}.cs", 
                        structDefinition.Name)), 
                    code);
            }
        }

		public IEnumerator GetEnumerator()
		{
			if (string.IsNullOrWhiteSpace(_xmlDocument?.InnerXml)) Compile();

			var i = 0;

			foreach (var record in RecordDefinitionTable)
			{
				var fileReference = record.FileName;

				if (fileReference.Split('/').Length == 2) fileReference = fileReference.Split('/')[1];

				if (string.IsNullOrWhiteSpace(fileReference)) fileReference = string.Format(@"Dump\{0}_{1}.xml", record.Name, i++);

				var newPath = fileReference;

				if (!Directory.Exists(Path.GetDirectoryName(newPath))) Directory.CreateDirectory(Path.GetDirectoryName(newPath));

				XmlDocument doc = new XmlDocument { };

				doc.LoadXml(DataMap[record.StructIndex][record.VariantIndex].OuterXml);

				yield return (FileName: newPath, XmlDocument: doc);
			}
		}

		public int Length => RecordDefinitionTable.Length;

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472
		public void CompileSerializationAssembly(String assemblyName = "HoloXPLOR.Data.DataForge")
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = false,
                OutputAssembly = String.Format("{0}.dll", assemblyName),
            };

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");

            List<String> source = new List<String> { };

            var sb = new StringBuilder();

            sb.AppendLine(@"using System.Xml.Serialization;");
            sb.AppendLine();
            sb.AppendFormat(@"namespace {0}", assemblyName);
            sb.AppendLine();
            sb.AppendLine(@"{");
            foreach (var enumDefinition in this.EnumDefinitionTable)
            {
                sb.Append(enumDefinition.Export());
            }
            sb.AppendLine(@"}");

            source.Add(sb.ToString());

            sb = new StringBuilder();

            sb.AppendLine(@"using System;");
            sb.AppendLine(@"using System.Xml.Serialization;");
            sb.AppendLine();
            sb.AppendFormat(@"namespace {0}", assemblyName);
            sb.AppendLine();
            sb.AppendLine(@"{");
            foreach (EDataType typeDefinition in Enum.GetValues(typeof(EDataType)))
            {
                var typeName = typeDefinition.ToString().Replace("var", "");
                switch (typeDefinition)
                {
                    case EDataType.varStrongPointer:
                    case EDataType.varClass: break;
                    case EDataType.varByte:
                        typeName = "UInt8";
                        sb.AppendFormat(@"    public class _{0}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    {");
                        sb.AppendLine(@"        public Byte Value { get; set; }");
                        sb.AppendLine(@"    }");
                        break;
                    case EDataType.varSByte:
                        typeName = "Int8";
                        sb.AppendFormat(@"    public class _{0}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    {");
                        sb.AppendLine(@"        public SByte Value { get; set; }");
                        sb.AppendLine(@"    }");
                        break;
                    case EDataType.varLocale:
                    case EDataType.varWeakPointer:
                        sb.AppendFormat(@"    public class _{0}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    {");
                        sb.AppendLine(@"        public String Value { get; set; }");
                        sb.AppendLine(@"    }");
                        break;
                    case EDataType.varReference:
                        sb.AppendFormat(@"    public class _{0}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    {");
                        sb.AppendLine(@"        public Guid Value { get; set; }");
                        sb.AppendLine(@"    }");
                        break;
                    default:
                        sb.AppendFormat(@"    public class _{0}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    {");
                        sb.AppendFormat(@"        public {0} Value {{ get; set; }}", typeName);
                        sb.AppendLine();
                        sb.AppendLine(@"    }");
                        break;
                }
            }
            sb.AppendLine(@"}");

            source.Add(sb.ToString());

            foreach (var structDefinition in this.StructDefinitionTable)
            {
                var code = structDefinition.Export(assemblyName);
                source.Add(code);
            }

            var result = provider.CompileAssemblyFromSource(parameters, source.ToArray());
        }
#endif
	}
}
