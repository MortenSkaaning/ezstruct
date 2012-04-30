using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;

using Dia2Lib;

using System.Diagnostics;


namespace ezstruct
{   
    public partial class Form1 : Form
    {
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        // Gathered from http://msdn.microsoft.com/en-us/library/ha7skyz4.aspx for VS2010.
        private enum DataKind : uint
        {
            DataIsUnknown,
            DataIsLocal,
            DataIsStaticLocal,
            DataIsParam,
            DataIsObjectPtr,
            DataIsFileStatic,
            DataIsGlobal,
            DataIsMember,
            DataIsStaticMember,
            DataIsConstant
        };

        private enum LocationType : uint
        {
            LocIsNull,
            LocIsStatic,
            LocIsTLS,
            LocIsRegRel,
            LocIsThisRel,
            LocIsEnregistered,
            LocIsBitField,
            LocIsSlot,
            LocIsIlRel,
            LocInMetaData,
            LocIsConstant,
            LocTypeMax
        };

        // PLEASE CHECK MSDN IF THERE'S ACTUALLY 31 MEMBERS!
        private enum BasicType : uint
        {
            btNoType = 0,
            btVoid = 1,
            btChar = 2,
            btWChar = 3,
            btInt = 6,
            btUInt = 7,
            btFloat = 8,
            btBCD = 9,
            btBool = 10,
            btLong = 13,
            btULong = 14,
            btCurrency = 25,
            btDate = 26,
            btVariant = 27,
            btComplex = 28,
            btBit = 29,
            btBSTR = 30,
            btHresult = 31
            // THE END??!
        };

        private enum UdtKind : uint
        {
            UdtStruct,
            UdtClass,
            UdtUnion
        };


        [Serializable]
        class SBitAlloc
        {
            public int m_BeginBit; // bit offset from byte start
            public int m_EndBit; // bit offset to one bit past end of member
            public int m_FieldIdx = -2; // idx of class member.
        }

        [Serializable]
        class SByteAlloc
        {
            public int m_BeginBit; // bit offset from struct start
            public int m_EndBit; // bit offset to one bit past end of member
            public int m_FieldIdx = -2; // idx of class member. -1 if contains BitAllocs

            public List<SBitAlloc> m_BitAllocs;
        }

        [Serializable]
        class SField
        {
            public string m_Name;
            public int m_byteOffset;

            public int m_AlignBits; // needed alignment.
            public int m_SizeBits; // bits we actually store

            public bool m_IsStatic;
            public bool m_IsBitField;
            public bool m_IsBool;
            public bool m_IsPadding;
            public bool m_IsUnionMember;
            public bool m_IsArrayIndexType;

            // .PDB specific
            public uint m_SymbolId;
            public string m_typeName;
            public string m_locationType;
            public string m_symTagEnum;
            public string m_udtKind;
            public string m_dataKind;

            public SField()
            {
                m_Name = string.Empty;
                m_byteOffset = 0;
                m_AlignBits = 0;
                m_SizeBits = 0;

                m_IsBitField = false;
                m_IsBool = false;
                m_IsStatic = false;
                m_IsPadding = false;
                m_IsUnionMember = false;
                m_IsArrayIndexType = false;

                m_locationType = string.Empty;
                m_symTagEnum = string.Empty;
                m_typeName = string.Empty;
                m_udtKind = string.Empty;
                m_dataKind = string.Empty;
            }          
            
            // TODO split into things should be filtered out of lowlevel and things that cannot be processed.
            public bool FitForLowLevel( ref string reason)
            {
                if (m_IsStatic )
                {
                    reason = "Static fields not supported.";
                    return false;
                }

                if (m_SizeBits == 0)
                {
                    reason = "No storage.";
                    return false;
                }

                if (m_IsUnionMember)
                {
                    reason = "Unions not supported.";
                    return false;
                }

                if( m_IsBitField && (m_SizeBits > m_BitsPerByte) )
                {
                    reason = "Bitfields larger than " + m_BitsPerByte + " bit not supported.";
                    return false;
                }

                return true;
            }            
        }

        [Serializable]
        class StructInfo
        {
            public StructInfo()
            {
                m_name = string.Empty;
                m_declaredFieldBytes = 0;
                m_symbolId = 0;
                m_fields = new List<SField>();
            }

            public string m_name;
            public int m_declaredFieldBytes; // total bytes including static members.
            public int m_symbolId;

            public List<SField> m_fields;
        }

        [Serializable]
        class AnalysisResult
        {
            public bool m_validResult;

            // compiler layout
            public int m_compilerTotalFreeBits;
            public int m_compilerInstanceBits;

            // generated layout
            public int m_generatedTotalFreeBits;
            public int m_generatedInstanceBits;
        }

        static public int m_MinAlignmentBytes = 4;
        static public int m_BitsPerByte = 8;

        DataTable m_allStructsDataTable = null;

        struct OverViewGridRow
        {
            public object Name;
            public object Declared_bytes;
            public object Symbol_Id;
            public object Instance_bytes;
            public object Padding_bytes;
            public object Savable_bytes;
            public object Analyzed;
            public object __data;

            static public string AsName(string s) { return s.Replace("_", " "); }
        }

        // types of OverViewGridRow.
        Dictionary<string, string> m_overViewGridRowTypes = new Dictionary<string, string> 
        {
            { "Name", "System.String" },
            { "Declared_bytes", "System.Int32" },
            { "Symbol_Id", "System.Int32" },
            { "Instance_bytes", "System.Int32" },
            { "Padding_bytes", "System.Int32" },
            { "Savable_bytes", "System.Int32" },            
            { "Analyzed", "System.String" },
            { "__data", "System.Object" },
        };
          
        struct DetailRow
        {
            public object Name;
            public object SizeBits;
            public object AlignBits;
            public object BeginBit;
            public object EndBit;
            public object ByteOffset;
        }



        //-----------------------------------------------------------------------------
        //-----------------------------------------------------------------------------
        //-----------------------------------------------------------------------------
        //-----------------------------------------------------------------------------
        //-----------------------------------------------------------------------------



        public Form1()
        {
            InitializeComponent();

            m_allStructsDataTable = CreateOverViewDataTable();

            overViewBindingSource.DataSource = m_allStructsDataTable;

            // Init overViewGrid columns.
            overViewGrid.DataSource = overViewBindingSource;

            // Make number columns small.
            foreach (var field in GetPublicFields<OverViewGridRow>())
            {
                int pixWidth = 60; // for numerical
                if (field.Name == "Name")
                    pixWidth = 200;
                if (field.Name == "__data")
                    pixWidth = 0;

                overViewGrid.Columns[OverViewGridRow.AsName(field.Name)].Width = pixWidth;
            }

            // Init layout data view columns.
            foreach (var field in GetPublicFields<DetailRow>() )
            {
                int pixWidth = 60; // for numerical
                if (field.Name == "Name")
                    pixWidth = 100;

                compilerDataView.Columns.Add(field.Name, field.Name);
                compilerDataView.Columns[field.Name].Width = pixWidth;

                computedLayoutView.Columns.Add(field.Name, field.Name);
                computedLayoutView.Columns[field.Name].Width = pixWidth;

            }

            // Init field details data view columns.
            foreach (var field in GetPublicFields<SField>() )
            {
                fieldsDetailView.Columns.Add(field.Name, field.Name.Replace("m_", "") );

                // Make number columns small.
                if(    field.Name.Contains("m_Is") 
                    || field.Name.ToLower().Contains("size") 
                    || field.Name.ToLower().Contains("byte")
                    || field.Name.ToLower().Contains("bit")
                    )
                {
                    fieldsDetailView.Columns[field.Name].Width = 50;
                }
            }
        }

        void PopulateDataTable(DataTable table, List<StructInfo> fields, Dictionary<StructInfo, AnalysisResult> results)
        {
            table.Rows.Clear();
            foreach (StructInfo info in fields)
            {
                OverViewGridRow rowData;
                rowData.Name = info.m_name;
                rowData.Declared_bytes = info.m_declaredFieldBytes; 
                rowData.Symbol_Id = info.m_symbolId;                
                rowData.__data = (Object)info;
                
                // pending analysis
                rowData.Instance_bytes = -1; 
                rowData.Padding_bytes = -1;
                rowData.Savable_bytes = -1;
                rowData.Analyzed = false;

                // Add analysis info
                if (results.ContainsKey(info))
                {
                    AnalysisResult result = results[info];
                    rowData.Instance_bytes = result.m_compilerInstanceBits / m_BitsPerByte;
                    rowData.Padding_bytes = result.m_compilerTotalFreeBits / m_BitsPerByte;
                    rowData.Savable_bytes = (result.m_compilerInstanceBits - result.m_generatedInstanceBits) / m_BitsPerByte;
                    rowData.Analyzed = result.m_validResult;
                }

                // Add row to table.
                DataRow row = table.NewRow();
                foreach (var field in GetPublicFields<OverViewGridRow>() )
                {
                    row[OverViewGridRow.AsName(field.Name)] = field.GetValue(rowData);
                }
                table.Rows.Add(row);
            }
        }

        DataTable CreateOverViewDataTable()
        {
            DataTable table = new DataTable("Symbols");            
            foreach (var field in GetPublicFields<OverViewGridRow>() )
            {
                DataColumn column = new DataColumn();
                column.ColumnName = OverViewGridRow.AsName(field.Name);
                column.ReadOnly = true;
                column.DataType = System.Type.GetType(m_overViewGridRowTypes[field.Name]);
                table.Columns.Add(column);
            }
            return table;
        }

        private int AlignUpward(int alignCandidate, int alignment)
        {
            int alignRemain = alignCandidate % alignment;
            if (alignRemain == 0)
            {
                return alignCandidate;
            }
            return alignCandidate + alignment - alignRemain;
        }

        private LinkedListNode<SByteAlloc> AllocByteField(SField field, int fieldsIdx, LinkedList<SByteAlloc> allocs)
        {
            if (allocs.Count == 0)
            {
                Debug.Assert(!field.m_IsBitField);
                SByteAlloc fieldAlloc = new SByteAlloc();
                fieldAlloc.m_BeginBit = 0;
                fieldAlloc.m_EndBit = field.m_SizeBits;
                fieldAlloc.m_FieldIdx = fieldsIdx;
                fieldAlloc.m_BitAllocs = new List<SBitAlloc>();
                Debug.Assert(fieldAlloc.m_EndBit % m_BitsPerByte == 0);

                LinkedListNode<SByteAlloc> fieldAllocNode = allocs.AddLast(fieldAlloc);
                return fieldAllocNode;
            }

            // Find first big-enough, aligned free space between allocations. Greedy approach guaranteed to work.. todo: prove that ;)
            int OldNumsAllocs = allocs.Count;
            for (LinkedListNode<SByteAlloc> cur = allocs.First; cur.Next != null && cur != allocs.Last; cur = cur.Next)
            {
                int freeBeginBit = cur.Value.m_EndBit;
                int alignBeginBit = AlignUpward(freeBeginBit, field.m_AlignBits);

                // check BeginBit okay
                int freeEndBit = cur.Next.Value.m_BeginBit;
                if (alignBeginBit >= freeEndBit) // '>=': we can write to freeEndBit.
                    continue; // per construction: alignBeginBit >= freeBeginBit.

                // check EndBit okay
                int alignEndBit = alignBeginBit + field.m_AlignBits;
                if (alignEndBit > freeEndBit) // '==' is okay since EndBit is not written to.
                    continue;

                // enough space, insert field.
                SByteAlloc fieldAlloc = new SByteAlloc();
                fieldAlloc.m_BeginBit = alignBeginBit;
                fieldAlloc.m_EndBit = alignEndBit;
                fieldAlloc.m_FieldIdx = fieldsIdx;
                fieldAlloc.m_BitAllocs = new List<SBitAlloc>();
                Debug.Assert(alignEndBit % m_BitsPerByte == 0);

                LinkedListNode<SByteAlloc> newAlloc = allocs.AddAfter(cur, fieldAlloc);
                return newAlloc;
            }

            // no in-between space available. Simply add to end because we always start with the largest allocations.
            {
                Debug.Assert(allocs.Count == OldNumsAllocs);
                Debug.Assert(allocs.Count != 0);

                int freeBeginBit = allocs.Last.Value.m_EndBit;
                int alignBeginBit = AlignUpward(freeBeginBit, field.m_AlignBits);

                SByteAlloc fieldAlloc = new SByteAlloc(); ;
                fieldAlloc.m_BeginBit = alignBeginBit;
                fieldAlloc.m_EndBit = alignBeginBit + field.m_SizeBits;
                fieldAlloc.m_FieldIdx = fieldsIdx;
                fieldAlloc.m_BitAllocs = new List<SBitAlloc>();

                LinkedListNode<SByteAlloc> newAlloc = allocs.AddLast(fieldAlloc);
                return newAlloc;
            }
        }

        private bool IsUnionMember(IDiaSymbol s)
        {
            if ((UdtKind)s.udtKind == UdtKind.UdtUnion)
                return true;

            if (s.classParent != null)
                return IsUnionMember(s.classParent);
            
            return false;
        }

        private bool DoesFieldsOverlap(SField a, SField b)
        {
            int a0 = a.m_byteOffset;
            int a1 = a.m_byteOffset + a.m_SizeBits/m_BitsPerByte;
            
            int b0 = b.m_byteOffset;
            int b1 = b.m_byteOffset + b.m_SizeBits/m_BitsPerByte;

            return a0 < b1 && b0 < a1; // from http://www.altdevblogaday.com/2011/09/21/checking-for-interval-overlap/, we just need non-end overlap.
        }

        private bool VerifyFieldsValid(List<SField> fields, ref string reason)
        {
            foreach( var f1 in fields)
            {
                foreach (var f2 in fields)
                {
                    if (f1 == f2)
                        continue;

                    // for some reason union types are not properly detected. Only allow bitfields to share offset.
                    // see .PDB symbol "_NT_TIB".
                    if ( !(f1.m_IsBitField && f2.m_IsBitField) && (f1.m_byteOffset == f2.m_byteOffset) )
                    {
                        reason = "Fields \"" + f1.m_Name + "\" and \"" + f2.m_Name + "\" share byteoffset (" + f1.m_byteOffset + "). Unions not properly detected or supported!";
                        return false;
                    }

                    if (DoesFieldsOverlap(f1, f2))
                    {
                        reason = "Fields \"" + f1.m_Name + "\" and \"" + f2.m_Name + "\" overlap!";
                        return false;
                    }
                }
            }
            return true;
        }

        private void Form1_Load(object sender, EventArgs BLABLABLBLBLBLBe)
        {
            Run();
        }

        private void Run()
        {
            string filename = "";
            //filename = "C:\\Coding_Projects\\test\\Debug\\test.pdb";
            //filename = "C:\\Coding_Projects\\SPH\\MSPH\\Debug\\vc100.pdb";
            //filename = "C:\\Coding_Projects\\ezstruct\\ezstruct\\engine.pdb";

            // Popup dialog box.
            if (filename.Length == 0)
            {
                if (openPdbDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                filename = openPdbDialog.FileName;
            }

            // Create a DIA session 
            IDiaDataSource diaSource = new DiaSourceClass();
            IDiaSession diaSession;
            diaSource.loadDataFromPdb(filename);
            diaSource.openSession(out diaSession);

            // Get a list of all UDT symbols in the global scope 
            IDiaEnumSymbols results;
            diaSession.findChildren(diaSession.globalScope, SymTagEnum.SymTagUDT, null, 0, out results);

            // Prealloc storage.
            List<StructInfo> structs = new List<StructInfo>(results.count);
            for( int i=0, endSize = results.count; i!=endSize; ++i)            
                structs.Add( new StructInfo() );

            // Parse symbols.
            int numStored = 0;
            int numLoops = 0;
            foreach (IDiaSymbol sym in results)
            {
                // Show progress
                if (numLoops++ % 100 == 0)
                {
                    Trace.WriteLine("Parse " + numLoops + "/" + results.count);
                }

                StructInfo info = structs[numStored];
                if (GetSymbolStructInfo(sym, ref info))
                {
                    numStored++;
                }
            }

            // Remove excess elements.
            while (structs.Count != numStored)
                structs.RemoveAt(structs.Count-1);

            // Analyse all structs.
            Dictionary<StructInfo, AnalysisResult> analysisResults = AnalyseAllStructs(structs);

            PopulateDataTable(m_allStructsDataTable, structs, analysisResults);

            // Reset target
            overViewBindingSource.Filter = null;
        }

        private bool GetSymbolStructInfo(IDiaSymbol sym, ref StructInfo structInfo)
        {
            // easy debug
            //if (sym.name != "C")
            if (sym.name != "ZHM5SaveData"/* || sym.length != 65128*/)
            {
                //return false;
            }

            structInfo.m_name = sym.name;
            structInfo.m_declaredFieldBytes = (int)sym.length;
            structInfo.m_symbolId = (int)sym.symIndexId;

            IDiaEnumSymbols children;
            sym.findChildren(SymTagEnum.SymTagNull, null, 0, out children);
            foreach (IDiaSymbol child in children)
            {
                // easy debug
                if (false && child.name != "m_ContractsData")
                    continue;

                SField field = new SField();

                // typename
                IDiaSymbol childType = child.type; // cache
                if (childType == null)
                {
                    field.m_typeName = ((BasicType)child.baseType).ToString();
                }
                else
                {
                    field.m_typeName = ((BasicType)childType.baseType).ToString();
                    field.m_symTagEnum = ((SymTagEnum)child.symTag).ToString();
                }

                // V-Table
                if (child.symTag == (uint)SymTagEnum.SymTagVTable)
                {
                    field.m_Name = "[V-Table]";
                }

                // field size bits
                if (child.locationType == (uint)LocationType.LocIsBitField)
                {
                    field.m_SizeBits = (int)child.length;
                    field.m_AlignBits = field.m_SizeBits;

                    // TODO support bitfields larger than a byte!
                    field.m_IsBitField = true;
                }

                // disregard static fields.
                else if (child.locationType == (uint)LocationType.LocIsStatic)
                {
                    field.m_SizeBits = (int)child.length;
                    field.m_IsStatic = true;
                }

                // field size bytes
                else if (childType != null)
                {
                    field.m_SizeBits += m_BitsPerByte * (int)childType.length; // '+=' for moving bitfields with byte offset.
                    field.m_AlignBits = field.m_SizeBits;

                    string baseType = ((BasicType)childType.baseType).ToString();

                    // Get array align bits. Filter out non-valid sizes.
                    if (field.m_SizeBits != 0 && childType.arrayIndexType != null)
                    {
                        field.m_IsArrayIndexType = true;
                        field.m_AlignBits = field.m_SizeBits / (int)childType.count; // arrays are aligned to member alignement.
                    }
                }

                field.m_IsUnionMember = IsUnionMember(child);
                field.m_byteOffset = child.offset;

                if (field.m_Name == string.Empty)
                {
                    if (child.name == null)
                    {
                        field.m_Name = "[null]";
                    }
                    else
                    {
                        field.m_Name = child.name;
                    }
                }

                field.m_SymbolId = child.symIndexId;
                field.m_locationType = ((LocationType)child.locationType).ToString();
                field.m_udtKind = ((UdtKind)child.udtKind).ToString();
                field.m_dataKind = ((DataKind)child.dataKind).ToString();
                if (field.m_symTagEnum == string.Empty)
                {
                    field.m_symTagEnum = ((SymTagEnum)child.symTag).ToString();
                }

                // Ignore types that do not get storage.
                if (child.symTag == (uint)SymTagEnum.SymTagEnum
                    || child.symTag == (uint)SymTagEnum.SymTagTypedef )
                    continue;

                structInfo.m_fields.Add(field);
            }

            return true;
        }

        private Dictionary<StructInfo, AnalysisResult> AnalyseAllStructs(List<StructInfo> structs)
        {
            int numLoops = 0;
            Dictionary<StructInfo, AnalysisResult> results = new Dictionary<StructInfo, AnalysisResult>();
            foreach (StructInfo ref_info in structs)
            {
                // Show progress
                if( numLoops++ % 100 == 0 )
                {
                    Trace.WriteLine("Analyse " + numLoops + "/" + structs.Count);
                }

                // Working filtered info.
                StructInfo info = DeepClone(ref_info);

                // Result
                AnalysisResult result = new AnalysisResult();
                results[ref_info] = result;

                // Filter out unfit fields.
                StringBuilder b = new StringBuilder();
                for (int i = info.m_fields.Count - 1; i >= 0; --i)
                {
                    SField field = info.m_fields[i];
                    string reason = string.Empty;
                    if (!field.FitForLowLevel(ref reason))
                    {
                        b.AppendLine("Field \"" + field.m_Name + "\" disregarded:  " + reason);
                        info.m_fields.RemoveAt(i);
                    }
                }

                // Check if field layout is invalid.
                string rejectReason = null;
                if (!VerifyFieldsValid(info.m_fields, ref rejectReason))
                {
                    result.m_validResult = false;
                    continue;
                }
                result.m_validResult = true;

                // compiler layout (writes to 'result')
                {
                    LinkedList<SByteAlloc> layout = GetGeneratedLayoutFromStructInfo(info);
                    LinkedList<SByteAlloc> paddedLayout;
                    StructInfo paddedInfo;
                    int largestNonPadField = 0;
                    AddPaddingFieldsToAllocsAndInfo(layout, info, out paddedLayout, out paddedInfo,
                        out result.m_compilerTotalFreeBits, out result.m_compilerInstanceBits, out largestNonPadField);
                }

                // generated layout (writes to 'result')
                {
                    LinkedList<SByteAlloc> layout = ComputeAllocs(info.m_fields);
                    LinkedList<SByteAlloc> paddedLayout;
                    StructInfo paddedInfo;
                    int largestNonPadField;
                    AddPaddingFieldsToAllocsAndInfo(layout, info, out paddedLayout, out paddedInfo,
                        out result.m_generatedTotalFreeBits, out result.m_generatedInstanceBits, out largestNonPadField);
                }
            }

            return results;
        }

        private LinkedList<SByteAlloc> ComputeAllocs(List<SField> sourceFields)
        {
            // Sort largest byte allocations first so we can take a greedy approach.
            List<SField> permutedFields = DeepClone( sourceFields );
            permutedFields = permutedFields.OrderByDescending(elm => elm.m_AlignBits).ToList();

            // map indicies back
            Dictionary<SField, int> toNormalOrder = new Dictionary<SField, int>();
            foreach ( SField f in permutedFields)
            {
                for (int i = 0; i != sourceFields.Count; ++i )
                {
                    if ( f.m_Name == sourceFields[i].m_Name )
                    {
                        toNormalOrder[f] = i;
                        break;
                    }
                }                
            }

            LinkedList<SByteAlloc> allocs = new LinkedList<SByteAlloc>();
            if (sourceFields.Count == 0)
                return allocs;

            // Allocate byte fields.
            foreach ( SField permutedField in permutedFields)
            {
                if (permutedField.m_IsBitField)
                    continue;

                AllocByteField(permutedField, toNormalOrder[permutedField], allocs);
            }

            // Allocate bit fields.
            List<LinkedListNode<SByteAlloc>> bitAllocSubset = new List<LinkedListNode<SByteAlloc>>(); // cache of allocation nodes used for small allocations.            
            foreach (SField bitField in permutedFields)
            {
                if (!bitField.m_IsBitField)
                    continue;

                Debug.Assert(bitField.m_SizeBits > 0);

                // put smallField in free smallAllocs space.
                bool enoughSpace = false;
                for (int j = 0; j != bitAllocSubset.Count; ++j)
                {
                    LinkedListNode<SByteAlloc> byteNode = bitAllocSubset[j];
                    // check of byteNode has enough free space.
                    int freeBeginBit = byteNode.Value.m_BitAllocs.Last().m_EndBit;
                    int freeBits = byteNode.Value.m_EndBit - freeBeginBit;
                    if (freeBits < bitField.m_SizeBits)
                        continue;

                    SBitAlloc bitAlloc = new SBitAlloc(); ;
                    bitAlloc.m_BeginBit = freeBeginBit;
                    bitAlloc.m_EndBit = freeBeginBit + bitField.m_SizeBits;
                    bitAlloc.m_FieldIdx = toNormalOrder[bitField];

                    byteNode.Value.m_BitAllocs.Add(bitAlloc);
                    enoughSpace = true;
                    break;
                }

                // add new byte.
                if (!enoughSpace)
                {
                    SField byteField = new SField();
                    byteField.m_AlignBits = m_BitsPerByte;
                    byteField.m_SizeBits = m_BitsPerByte;
                    byteField.m_Name = string.Empty;

                    // add with special index.
                    LinkedListNode<SByteAlloc> byteNode = AllocByteField(byteField, -1, allocs);
                    bitAllocSubset.Add(byteNode);

                    SBitAlloc bitAlloc = new SBitAlloc();
                    bitAlloc.m_BeginBit = byteNode.Value.m_BeginBit;
                    bitAlloc.m_EndBit = bitAlloc.m_BeginBit + bitField.m_SizeBits;
                    bitAlloc.m_FieldIdx = toNormalOrder[bitField];

                    byteNode.Value.m_BitAllocs.Add(bitAlloc);
                }
            }

            return allocs;
        }

        private LinkedList<SByteAlloc> GetGeneratedLayoutFromStructInfo(StructInfo info)
        {
            LinkedList<SByteAlloc> byteAllocs = new LinkedList<SByteAlloc>();
            Dictionary<int, LinkedListNode<SByteAlloc>> bitsToSharedByte = new Dictionary<int, LinkedListNode<SByteAlloc>>(); // map byteOffset to byte alloc containing bits.

            for (int i = 0; i != info.m_fields.Count; ++i)
            {
                SField field = info.m_fields[i];

                if (field.m_IsBitField)
                {
                    SBitAlloc bitAlloc = new SBitAlloc();

                    // remember bitfields share the same byteoffset.
                    if (bitsToSharedByte.ContainsKey(field.m_byteOffset))
                    {
                        // continue for last bit in byteOffset byte.
                        SByteAlloc sharedByteAlloc = bitsToSharedByte[field.m_byteOffset].Value;
                        bitAlloc.m_BeginBit = sharedByteAlloc.m_BitAllocs.Last().m_EndBit;
                        sharedByteAlloc.m_BitAllocs.Add(bitAlloc);
                        Debug.Assert(sharedByteAlloc.m_FieldIdx == -1);
                    }
                    else
                    {
                        SByteAlloc byteAlloc = new SByteAlloc();
                        byteAlloc.m_BeginBit = field.m_byteOffset * m_BitsPerByte;
                        byteAlloc.m_EndBit = byteAlloc.m_BeginBit + m_BitsPerByte;
                        byteAlloc.m_FieldIdx = -1; // indicate bit alloctions.
                        byteAlloc.m_BitAllocs = new List<SBitAlloc>();
                        byteAlloc.m_BitAllocs.Add(bitAlloc);
                        Debug.Assert(byteAlloc.m_BeginBit < byteAlloc.m_EndBit);

                        LinkedListNode<SByteAlloc> allocNode = byteAllocs.AddLast(byteAlloc);
                        bitsToSharedByte.Add(field.m_byteOffset, allocNode);

                        // first bit alloc.
                        bitAlloc.m_BeginBit = field.m_byteOffset * m_BitsPerByte;
                    }

                    bitAlloc.m_EndBit = bitAlloc.m_BeginBit + field.m_SizeBits;
                    bitAlloc.m_FieldIdx = i;
                }
                else
                {
                    SByteAlloc byteAlloc = new SByteAlloc();
                    byteAlloc.m_BeginBit = field.m_byteOffset * m_BitsPerByte;
                    byteAlloc.m_EndBit = byteAlloc.m_BeginBit + field.m_SizeBits;
                    byteAlloc.m_FieldIdx = i;
                    byteAlloc.m_BitAllocs = new List<SBitAlloc>();
                    Debug.Assert(byteAlloc.m_BeginBit < byteAlloc.m_EndBit);
                    
                    byteAllocs.AddLast(byteAlloc);
                }
            }

            return byteAllocs;
        }

        private void AddPaddingFieldsToAllocsAndInfo(LinkedList<SByteAlloc> allocs, StructInfo info, out LinkedList<SByteAlloc> out_allocs, out StructInfo out_info, 
            out int totalFreeBits, out int instanceBits, out int largestNonPadFieldBits)
        {
            out_allocs = new LinkedList<SByteAlloc>();
            out_info = DeepClone(info);                    

            totalFreeBits = 0;
            largestNonPadFieldBits = 0;

            for (LinkedListNode<SByteAlloc> alloc = allocs.First; alloc != null; alloc = alloc.Next)
            {
                Debug.Assert(alloc.Value.m_FieldIdx != -2);

                // copy
                LinkedListNode<SByteAlloc> out_allocNode = out_allocs.AddLast(alloc.Value);

                if (alloc.Value.m_FieldIdx == -1)
                {
                    int highestEndBit = 0;
                    foreach (SBitAlloc bitAlloc in alloc.Value.m_BitAllocs)
                    {
                        highestEndBit = Math.Max(highestEndBit, bitAlloc.m_EndBit);
                        largestNonPadFieldBits = Math.Max(largestNonPadFieldBits, bitAlloc.m_EndBit - bitAlloc.m_BeginBit);
                    }

                    largestNonPadFieldBits = Math.Max(largestNonPadFieldBits, alloc.Value.m_EndBit - alloc.Value.m_BeginBit);

                    int freeBits = alloc.Value.m_EndBit - highestEndBit;
                    Debug.Assert(freeBits >= 0);
                    if( freeBits == 0 )
                        continue;
                    
                    SField padField = new SField();
                    padField.m_SizeBits = freeBits;
                    padField.m_AlignBits = padField.m_SizeBits; // use SizeBits to get greedy scheme. Use 1 to disable ordering of bitallocs.
                    padField.m_Name = "[bit pad]";
                    padField.m_IsPadding = true;
                    padField.m_IsBitField = true;

                    out_info.m_fields.Add(padField);

                    SBitAlloc padBits = new SBitAlloc();
                    padBits.m_BeginBit = highestEndBit;
                    padBits.m_EndBit = alloc.Value.m_EndBit;
                    padBits.m_FieldIdx = out_info.m_fields.Count-1;
                    Debug.Assert(padBits.m_BeginBit < padBits.m_EndBit);

                    out_allocNode.Value.m_BitAllocs.Add(padBits);

                    totalFreeBits += freeBits;
                }
                else
                {
                    if (alloc == allocs.Last)
                        break;

                    Debug.Assert(alloc.Value.m_FieldIdx >= 0);

                    largestNonPadFieldBits = Math.Max(largestNonPadFieldBits, alloc.Value.m_EndBit - alloc.Value.m_BeginBit);

                    SField field = out_info.m_fields[alloc.Value.m_FieldIdx];

                    int freeBeginBit = alloc.Value.m_EndBit;
                    int freeEndBit = alloc.Next.Value.m_BeginBit;
                
                    int freeBits = freeEndBit - freeBeginBit;
                    Debug.Assert(freeBits >= 0);
                    if (freeBits == 0)
                        continue;

                    SField padField = new SField();
                    padField.m_SizeBits = freeBits;
                    padField.m_AlignBits = field.m_AlignBits;
                    padField.m_Name = "[byte pad]";
                    padField.m_IsPadding = true;

                    out_info.m_fields.Add(padField);

                    SByteAlloc padByte = new SByteAlloc();
                    padByte.m_BeginBit = freeBeginBit;
                    padByte.m_EndBit = freeEndBit;                    
                    padByte.m_FieldIdx = out_info.m_fields.Count - 1;
                    Debug.Assert(padByte.m_BeginBit < padByte.m_EndBit);
                    Debug.Assert(freeEndBit % m_MinAlignmentBytes == 0);

                    out_allocs.AddLast(padByte);

                    totalFreeBits += freeBits;
                }
            }

            // compute tail waste due to alignment
            {
                int maxAlignBits = 0;
                int freeEndBit = 0;
                foreach (SByteAlloc alloc in allocs)
                {
                    // skip bitfields as they are smaller than minimum struct alignment.
                    if (alloc.m_FieldIdx == -1)
                        continue;

                    SField field = info.m_fields[alloc.m_FieldIdx];
                    maxAlignBits = Math.Max(maxAlignBits, field.m_AlignBits);
                    freeEndBit = Math.Max(freeEndBit, alloc.m_EndBit);
                }
                maxAlignBits = Math.Max(m_MinAlignmentBytes * m_BitsPerByte, maxAlignBits);

                int alignEndBit = AlignUpward(freeEndBit, maxAlignBits);
                instanceBits = alignEndBit;

                int freeBits = alignEndBit - freeEndBit;
                if (freeBits > 0)
                {
                    SField padField = new SField();
                    padField.m_SizeBits = freeBits;
                    padField.m_AlignBits = maxAlignBits;
                    padField.m_Name = "[byte pad]";
                    padField.m_IsPadding = true;

                    out_info.m_fields.Add(padField);

                    SByteAlloc padByte = new SByteAlloc();
                    padByte.m_BeginBit = freeEndBit;
                    padByte.m_EndBit = alignEndBit;                    
                    padByte.m_FieldIdx = out_info.m_fields.Count - 1;
                    Debug.Assert(padByte.m_BeginBit < padByte.m_EndBit);
                    Debug.Assert(padByte.m_EndBit % m_MinAlignmentBytes == 0);

                    out_allocs.AddLast(padByte);

                    totalFreeBits += freeBits;
                }
            }

            // overwrite input with result.
            allocs = out_allocs;
        }

        private void PaintStructDetailViews(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView view = sender as DataGridView;
            if (view == null)
                return;

            foreach (DataGridViewRow row in view.Rows)
            {
                DataGridViewCell firstCell = row.Cells[0];

                if (firstCell.Value.ToString() == "[byte pad]")
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.BackColor = Color.LightYellow;
                    }
                }

                if (firstCell.Value.ToString() == "[bit pad]")
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.BackColor = Color.LightYellow;
                    }
                }
            }
        }

        private System.Reflection.FieldInfo[] GetPublicFields<T>()
        {
            return typeof(T).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        }

        private void AddRowToView<T>(T row, System.Windows.Forms.DataGridView gridView)
        {
            List<object> dat = new List<object>();
            foreach (var field in GetPublicFields<T>() )
            {
                dat.Add(field.GetValue(row));
            }
            gridView.Rows.Add(dat.ToArray());
        }

        private void ClearLayoutPanes()
        {
            compilerDataView.Rows.Clear();
            text_compilerLayoutTotals.Clear();
            
            computedLayoutView.Rows.Clear();
            text_generatedLayoutTotals.Clear();
        }

        private void PopulateFieldDetailsView(StructInfo info, System.Windows.Forms.DataGridView gridView )
        {
            gridView.Rows.Clear();
            foreach (SField field in info.m_fields)
            {
                AddRowToView(field, gridView);
            }
        }

        private void PopulateGridView(LinkedList<SByteAlloc> allocs, StructInfo info, System.Windows.Forms.DataGridView gridView)
        {
            gridView.Rows.Clear();
            foreach (SByteAlloc byteAlloc in allocs)
            {
                // special case for bit allocs
                if (byteAlloc.m_FieldIdx == -1)
                {
                    foreach (SBitAlloc bitAlloc in byteAlloc.m_BitAllocs)
                    {
                        SField field = info.m_fields[bitAlloc.m_FieldIdx];

                        DetailRow row = new DetailRow();
                        row.SizeBits = field.m_SizeBits;
                        row.AlignBits = field.m_AlignBits;
                        row.BeginBit = bitAlloc.m_BeginBit;
                        row.EndBit = bitAlloc.m_EndBit;
                        row.Name = field.m_Name;
                        row.ByteOffset = field.m_byteOffset;

                        AddRowToView(row, gridView);
                    }
                }
                else
                {
                    Debug.Assert(byteAlloc.m_FieldIdx >= 0);

                    SField field = info.m_fields[byteAlloc.m_FieldIdx];

                    DetailRow row = new DetailRow();
                    row.SizeBits = field.m_SizeBits;
                    row.AlignBits = field.m_AlignBits;
                    row.BeginBit = byteAlloc.m_BeginBit;
                    row.EndBit = byteAlloc.m_EndBit;
                    row.Name = field.m_Name;
                    row.ByteOffset = field.m_byteOffset;

                    AddRowToView(row, gridView);
                }
            }
        }

        private void overViewGrid_SelectionChanged(object sender, EventArgs e)
        {
            ProcessSelectedStruct();
        }

       
        private void ProcessSelectedStruct()
        {
            if (overViewGrid.SelectedRows.Count == 0)
            {
                return;
            }

            DataGridViewRow selectedRow = overViewGrid.SelectedRows[0];
            DataRow myRow = (selectedRow.DataBoundItem as DataRowView).Row;
            StructInfo ref_info = (StructInfo)myRow[OverViewGridRow.AsName("__data")];

            Trace.WriteLine("selected " + ref_info.m_name);

            StructInfo info = DeepClone(ref_info);

            // Show field details
            PopulateFieldDetailsView(ref_info, fieldsDetailView);

            // Filter out unfit fields.
            StringBuilder b = new StringBuilder();
            for (int i = info.m_fields.Count - 1; i >= 0; --i )
            {
                SField field = info.m_fields[i];
                string reason = string.Empty;
                if( !field.FitForLowLevel(ref reason) )
                {
                    b.AppendLine("Field \"" + field.m_Name + "\" disregarded:  " + reason );
                    info.m_fields.RemoveAt(i);
                }
            }
            text_Warnings.Text = b.ToString();

            // Check if field layout is invalid.
            string rejectReason = null;
            if (!VerifyFieldsValid(info.m_fields, ref rejectReason))
            {
                ClearLayoutPanes();
                text_Warnings.Text = rejectReason;
                return;
            }

            // display layout from debug data source
            if (chk_createCompiledLayout.Checked)
            {
                LinkedList<SByteAlloc> layout = GetGeneratedLayoutFromStructInfo(info);
                
                if (chk_compiledLayoutPadding.Checked)
                {
                    int totalFreeBits;
                    int instanceBits;
                    int largestNonPadField;
                    LinkedList<SByteAlloc> paddedLayout;
                    StructInfo paddedInfo;
                    AddPaddingFieldsToAllocsAndInfo(layout, info, out paddedLayout, out paddedInfo, out totalFreeBits, out instanceBits, out largestNonPadField);
                    float imbalance = (instanceBits - totalFreeBits == 0) ? 0 : (float)largestNonPadField / (float)(instanceBits - totalFreeBits);
                    text_compilerLayoutTotals.Text = "Instance bits: " + instanceBits + ", pad bits: " + totalFreeBits + ", imbalance %: " + imbalance;
                    PopulateGridView(paddedLayout, paddedInfo, compilerDataView);
                }
                else
                {
                    PopulateGridView(layout, info, compilerDataView);
                }
            }
            else
            {
                compilerDataView.Rows.Clear();
            }

            // display computed layout.
            if (chk_createGenerateLayout.Checked)
            {
                LinkedList<SByteAlloc> layout = ComputeAllocs(info.m_fields);
                
                if (chk_generateLayoutPadding.Checked)
                {
                    int totalFreeBits;
                    int instanceBits;
                    int largestNonPadField;
                    LinkedList<SByteAlloc> paddedLayout;
                    StructInfo paddedInfo;
                    AddPaddingFieldsToAllocsAndInfo(layout, info, out paddedLayout, out paddedInfo, out totalFreeBits, out instanceBits, out largestNonPadField);
                    float imbalance = (instanceBits - totalFreeBits == 0) ? 0 : (float)largestNonPadField / (float)(instanceBits - totalFreeBits);
                    text_generatedLayoutTotals.Text = "Instance bits: " + instanceBits + ", pad bits: " + totalFreeBits + ", imbalance %: " + imbalance;
                    PopulateGridView(paddedLayout, paddedInfo, computedLayoutView);
                }
                else
                {
                    PopulateGridView(layout, info, computedLayoutView);
                }
            }
            else
            {
                computedLayoutView.Rows.Clear();
            }
        }

        private void chk_createCompiledLayout_CheckedChanged(object sender, EventArgs e)
        {
            ProcessSelectedStruct();
        }

        private void chk_compiledLayoutPadding_CheckedChanged(object sender, EventArgs e)
        {
            ProcessSelectedStruct();
        }

        private void chk_createGenerateLayout_CheckedChanged(object sender, EventArgs e)
        {
            ProcessSelectedStruct();
        }

        private void chk_generateLayoutPadding_CheckedChanged(object sender, EventArgs e)
        {
            ProcessSelectedStruct();
        }

        private string BuildSearchFilterQuery(string filter)
        {
            string[] words = filter.Trim().Split( new char[]{' '}, StringSplitOptions.RemoveEmptyEntries );
        
            if (words.Length == 0)
                return null;

            StringBuilder s = new StringBuilder();
            const string property = "Name";
            for (int i = 0; i != words.Length; ++i)
            {
                if( i > 0 )
                    s.Append(" AND ");
                s.Append( property + " LIKE '%" + words[i] + "%'" );
            }            
            return s.ToString();
        }

        private void text_overViewFilter_TextChanged(object sender, EventArgs e)
        {
            overViewBindingSource.Filter = BuildSearchFilterQuery(text_overViewFilter.Text);
        }
    }
}
