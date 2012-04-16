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
            public int m_AlignBits; // needed alignment.
            public int m_SizeBits; // bits we actually store
            public int m_byteOffset;
            public string m_Name;

            public bool m_IsStatic;
            public bool m_IsBitField;
            public bool m_IsBool;
            public bool m_IsPadding;
            public bool m_IsUnionMember;
            public bool m_IsArrayIndexType;

            // temps
            public string m_typeName;
            public string m_locationType;
            public string m_symTagEnum;
            public string m_udtKind;
            public string m_dataKind;

            public SField()
            {
                m_AlignBits = 0;
                m_byteOffset = 0;
                m_IsBitField = false;
                m_IsBool = false;
                m_IsStatic = false;
                m_Name = string.Empty;
                m_SizeBits = 0;
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
            public bool FitForLowLevel()
            {
                if (m_IsStatic 
                    || m_SizeBits == 0 
                    || m_IsUnionMember
                    || m_IsBitField && (m_SizeBits > m_BitsPerByte) ) // TODO arbitrary sized bitfields.
                    return false;
                return true;
            }            
        }

        [Serializable]
        class StructInfo
        {
            public string m_name;
            public int m_declaredFieldBytes; // total bytes including static members.
            public int m_instanceFieldBytes;
            public int m_instanceFieldBits;

            public List<SField> m_fields;
        }


        static public int m_MinAlignment = 4;
        static public int m_BitsPerByte = 8;

        DataTable m_table = null;

        struct DetailRow
        {
            public object Name;
            public object Bits;
            public object BeginBit;
            public object EndBit;
            public object ByteOffset;
        }

        public Form1()
        {
            InitializeComponent();

            m_table = CreateOverViewTable();

            // create columns.
            foreach (var field in typeof(DetailRow).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                compilerDataView.Columns.Add(field.Name, field.Name);
                computedLayoutView.Columns.Add(field.Name, field.Name);
            }

            //bindingSource1.DataSource = m_table;
            overViewGrid.DataSource = m_table;
        }

        void PopulateDataTable(DataTable table, List<StructInfo> fields)
        {
            table.Rows.Clear();
            foreach (StructInfo info in fields)
            {
                DataRow row = table.NewRow();
                row["__data"] = (Object)info;
                row["Symbol"] = info.m_name;
                row["Declared bytes"] = info.m_declaredFieldBytes;
                row["Instance bytes"] = info.m_instanceFieldBytes;
                row["Instance bits"] = info.m_instanceFieldBits;
                table.Rows.Add(row);
            }
        }

        DataTable CreateOverViewTable()
        {
            DataTable table = new DataTable("Symbols");

            DataColumn column = new DataColumn();
            column.ColumnName = "Symbol";
            column.ReadOnly = true;
            table.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Declared bytes";
            column.ReadOnly = true;
            column.DataType = System.Type.GetType("System.Int32");
            table.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Instance bytes";
            column.ReadOnly = true;
            column.DataType = System.Type.GetType("System.Int32");
            table.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Instance bits";
            column.ReadOnly = true;
            column.DataType = System.Type.GetType("System.Int32");
            table.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "__data";
            column.ReadOnly = true;
            column.DataType = System.Type.GetType("System.Object");
            table.Columns.Add(column);

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
                {
                    continue; // per construction: alignBeginBit >= freeBeginBit.
                }

                // check EndBit okay
                int alignEndBit = alignBeginBit + field.m_SizeBits;
                if (alignEndBit > freeEndBit) // '==' is okay since EndBit is not written to.
                {
                    continue;
                }

                // enough space, insert field.
                SByteAlloc fieldAlloc = new SByteAlloc();
                fieldAlloc.m_BeginBit = alignBeginBit;
                Debug.Assert(alignEndBit % m_BitsPerByte == 0);
                fieldAlloc.m_EndBit = alignEndBit;
                fieldAlloc.m_FieldIdx = fieldsIdx;
                fieldAlloc.m_BitAllocs = new List<SBitAlloc>();

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

        private bool lessThanByte(SField f)
        {
            return f.m_AlignBits < m_BitsPerByte && f.m_SizeBits < m_BitsPerByte;
        }

        private bool isFieldStatic(SField f)
        {
            return f.m_IsStatic;
        }

        private bool IsUnionMember(IDiaSymbol s)
        {
            if ((UdtKind)s.udtKind == UdtKind.UdtUnion)
                return true;

            if (s.classParent != null)
                return IsUnionMember(s.classParent);
            
            return false;
        }

        private bool VerifyFieldsValid(List<SField> fields, ref string reason)
        {
            foreach( var f1 in fields)
            {
                foreach (var f2 in fields)
                {
                    if (f1 == f2)
                        continue;

                    // for some reason union types are not properly detected.
                    // see .PDB symbol "_NT_TIB".
                    if (!f1.m_IsBitField && !f2.m_IsBitField && f1.m_byteOffset == f2.m_byteOffset)
                    {
                        reason = "fields \"" + f1.m_Name + "\" and \"" + f2.m_Name + "\" share byteoffset (" + f1.m_byteOffset + ").";
                        return false;
                    }
                }
            }
            return true;
        }

        private void Form1_Load(object sender, EventArgs BLABLABLBLBLBLBe)
        {
            //string filename = "";
            string filename = "C:\\Coding_Projects\\test\\Debug\\test.pdb";
            //string filename = "C:\\Coding_Projects\\SPH\\MSPH\\Debug\\vc100.pdb";

            if (filename.Length == 0)
            {
                if (openPdbDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                filename = openPdbDialog.FileName;
            }


            // create a DIA session 
            IDiaDataSource diaSource = new DiaSourceClass();
            IDiaSession diaSession;

            //string filename = openPdbDialog.FileName

            diaSource.loadDataFromPdb(filename);
            diaSource.openSession(out diaSession);

            // get a list of all compilands in the global scope 
            IDiaEnumSymbols results;
            diaSession.findChildren(diaSession.globalScope, SymTagEnum.SymTagUDT, null, 0, out results);

            List<StructInfo> structs = new List<StructInfo>();
            foreach (IDiaSymbol sym in results)
            {
                //if (sym.name == "C")
                if (sym.name == "SrcHeader")                
                {
                    StructInfo structInfo = new StructInfo();
                    structInfo.m_name = sym.name;
                    structInfo.m_declaredFieldBytes = (int)sym.length;
                    structInfo.m_instanceFieldBytes = 0;
                    structInfo.m_instanceFieldBits = 0;
                    structInfo.m_fields = new List<SField>();

                    IDiaEnumSymbols children;
                    sym.findChildren(SymTagEnum.SymTagNull, null, 0, out children);
                    foreach (IDiaSymbol child in children)
                    {
                        if (false && child.name != "FiberData")
                            continue;

                        SField field = new SField();

                        // typename
                        if (child.type == null)
                        {
                            field.m_typeName = ((BasicType)child.baseType).ToString();
                        }
                        else
                        {
                            field.m_typeName = ((BasicType)child.type.baseType).ToString();
                            field.m_symTagEnum = ((SymTagEnum)child.symTag).ToString();                            
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
                        else if (child.type != null)
                        {
                            field.m_SizeBits += m_BitsPerByte * (int)child.type.length;
                            field.m_AlignBits = field.m_SizeBits;

                            string baseType = ((BasicType)child.type.baseType).ToString();

                            if (child.type.arrayIndexType != null)
                            {
                                field.m_IsArrayIndexType = true;
                                field.m_AlignBits = field.m_SizeBits / (int)child.type.arrayIndexType.length; // arrays are aligned to member alignement.
                            }
                        }

                        field.m_IsUnionMember = IsUnionMember(child);
                        field.m_byteOffset = child.offset;
                        field.m_Name = (child.name == null) ? "[null]" : child.name;

                        field.m_locationType = ((LocationType)child.locationType).ToString();
                        field.m_udtKind = ((UdtKind)child.udtKind).ToString();
                        field.m_dataKind = ((DataKind)child.dataKind).ToString();
                        if (field.m_symTagEnum == string.Empty )
                        {
                            field.m_symTagEnum = ((SymTagEnum)child.symTag).ToString();
                        }

                        if (child.symTag == (uint)SymTagEnum.SymTagEnum)
                            continue;

                        structInfo.m_fields.Add(field);
                    }

                    structs.Add(structInfo);
                }
            }

            PopulateDataTable(m_table, structs);
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

                if (field.m_IsStatic) // todo filter data outside.
                    continue;

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
                    
                    byteAllocs.AddLast(byteAlloc);
                }
            }

            return byteAllocs;
        }

        private void AddPaddingFieldsToAllocsAndInfo(LinkedList<SByteAlloc> allocs, StructInfo info, out LinkedList<SByteAlloc> out_allocs, out StructInfo out_info, out int totalFreeBits, out int instanceBits)
        {
            out_allocs = new LinkedList<SByteAlloc>();
            out_info = DeepClone(info);                    

            totalFreeBits = 0;            
            for (LinkedListNode<SByteAlloc> alloc = allocs.First; alloc != null; alloc = alloc.Next)
            {
                // copy
                LinkedListNode<SByteAlloc> out_allocNode = out_allocs.AddLast(alloc.Value);

                Debug.Assert(alloc.Value.m_FieldIdx != -2);

                if (alloc.Value.m_FieldIdx == -1)
                {
                    int highestEndBit = 0;
                    foreach (SBitAlloc bitAlloc in alloc.Value.m_BitAllocs)
                        highestEndBit = Math.Max( highestEndBit, bitAlloc.m_EndBit );

                    int freeBits = alloc.Value.m_EndBit - highestEndBit;
                    Debug.Assert(freeBits >= 0);
                    if( freeBits == 0 )
                        continue;
                    
                    SField padField = new SField();
                    padField.m_SizeBits = freeBits;
                    padField.m_AlignBits = padField.m_SizeBits;
                    padField.m_Name = "[bit pad]";
                    padField.m_IsPadding = true;
                    padField.m_IsBitField = true;

                    out_info.m_fields.Add(padField);

                    SBitAlloc padBits = new SBitAlloc();
                    padBits.m_BeginBit = highestEndBit;
                    padBits.m_EndBit = alloc.Value.m_EndBit;
                    padBits.m_FieldIdx = out_info.m_fields.Count-1;

                    out_allocNode.Value.m_BitAllocs.Add(padBits);

                    totalFreeBits += freeBits;
                }
                else
                {
                    if (alloc == allocs.Last)
                        break;

                    Debug.Assert(alloc.Value.m_FieldIdx >= 0);

                    SField field = out_info.m_fields[alloc.Value.m_FieldIdx];

                    int freeBeginBit = alloc.Value.m_EndBit;
                    int freeEndBit = alloc.Next.Value.m_BeginBit;
                
                    int freeBits = freeEndBit - freeBeginBit;
                    Debug.Assert(freeBits >= 0);
                    if (freeBits == 0)
                        continue;

                    SField padField = new SField();
                    padField.m_SizeBits = freeBits;
                    padField.m_AlignBits = padField.m_SizeBits;
                    padField.m_Name = "[byte pad]";
                    padField.m_IsPadding = true;

                    out_info.m_fields.Add(padField);

                    SByteAlloc padByte = new SByteAlloc();
                    padByte.m_BeginBit = freeBeginBit;
                    padByte.m_EndBit = freeEndBit;
                    Debug.Assert(freeEndBit % m_MinAlignment == 0);
                    padByte.m_FieldIdx = out_info.m_fields.Count - 1;

                    out_allocs.AddLast(padByte);

                    totalFreeBits += freeBits;
                }
            }

            // compute tail waste due to alignment
            {
                int alignMax = 0;
                int freeEndBit = 0;
                foreach (SByteAlloc alloc in allocs)
                {
                    alignMax = Math.Max(alignMax, alloc.m_EndBit - alloc.m_BeginBit);
                    freeEndBit = Math.Max(freeEndBit, alloc.m_EndBit);
                }
                alignMax = Math.Max(m_MinAlignment, alignMax);

                int alignEndBit = AlignUpward(freeEndBit, alignMax);
                instanceBits = alignEndBit;

                int freeBits = alignEndBit - freeEndBit;
                if (freeBits > 0)
                {
                    SField padField = new SField();
                    padField.m_SizeBits = freeBits;
                    padField.m_AlignBits = padField.m_SizeBits;
                    padField.m_Name = "[byte pad]";
                    padField.m_IsPadding = true;

                    out_info.m_fields.Add(padField);

                    SByteAlloc padByte = new SByteAlloc();
                    padByte.m_BeginBit = freeEndBit;
                    padByte.m_EndBit = alignEndBit;
                    Debug.Assert( padByte.m_EndBit % m_MinAlignment == 0);
                    padByte.m_FieldIdx = out_info.m_fields.Count - 1;

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

        private void AddDetailRow(DetailRow row, System.Windows.Forms.DataGridView gridView)
        {
            List<object> dat = new List<object>();
            foreach (var field in typeof(DetailRow).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                dat.Add(field.GetValue(row));
            }
            gridView.Rows.Add(dat.ToArray());
        }

        private void PopulateGridView(LinkedList<SByteAlloc> allocs, StructInfo info, System.Windows.Forms.DataGridView gridView )
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
                        row.Bits = field.m_SizeBits;
                        row.BeginBit = bitAlloc.m_BeginBit;
                        row.EndBit = bitAlloc.m_EndBit;
                        row.Name = field.m_Name;
                        row.ByteOffset = field.m_byteOffset;

                        AddDetailRow(row, gridView);
                    }
                }
                else
                {
                    Debug.Assert(byteAlloc.m_FieldIdx >= 0);

                    SField field = info.m_fields[byteAlloc.m_FieldIdx];

                    DetailRow row = new DetailRow();
                    row.Bits = field.m_SizeBits;
                    row.BeginBit = byteAlloc.m_BeginBit;
                    row.EndBit = byteAlloc.m_EndBit;
                    row.Name = field.m_Name;
                    row.ByteOffset = field.m_byteOffset;

                    AddDetailRow(row, gridView);
                }
            }
        }

        private void overViewGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (overViewGrid.SelectedRows.Count == 0)
            {
                return;
            }

            DataGridViewRow selectedRow = overViewGrid.SelectedRows[0];
            DataRow myRow = (selectedRow.DataBoundItem as DataRowView).Row;
            StructInfo ref_info = (StructInfo)myRow["__data"];

            Trace.WriteLine("selected " + ref_info.m_name);

            StructInfo info = DeepClone(ref_info);

            // Check if field layout is valid.
            string rejectReason = null;
            if (!VerifyFieldsValid(info.m_fields, ref rejectReason))
            {
                text_Warnings.Text = rejectReason;
                return;
            }

            // Reject unfit fields.
            StringBuilder b = new StringBuilder();
            for (int i = info.m_fields.Count - 1; i >= 0; --i )
            {
                SField field = info.m_fields[i];
                if (!field.FitForLowLevel())
                {
                    b.AppendLine("field \"" + field.m_Name + "\" disregarded!");
                    info.m_fields.RemoveAt(i);
                }
            }
            text_Warnings.Text = b.ToString();


            // display layout from debug data source
            {
                LinkedList<SByteAlloc> layout = GetGeneratedLayoutFromStructInfo(info);


                int totalFreeBits;
                int instanceBits;
                LinkedList<SByteAlloc> paddedLayout;
                StructInfo paddedInfo;
                AddPaddingFieldsToAllocsAndInfo(layout, info, out paddedLayout, out paddedInfo, out totalFreeBits, out instanceBits);
                text_compilerDataTotals.Text = "Instance bits: " + instanceBits + ", padding: " + totalFreeBits;
                PopulateGridView(paddedLayout, paddedInfo, compilerDataView);



                //PopulateGridView(layout, info, compilerDataView);
            }

            // display computed layout.
            {
                LinkedList<SByteAlloc> layout = ComputeAllocs(info.m_fields);


                int totalFreeBits;
                int instanceBits;
                LinkedList<SByteAlloc> paddedLayout;
                StructInfo paddedInfo;
                AddPaddingFieldsToAllocsAndInfo(layout, info, out paddedLayout, out paddedInfo, out totalFreeBits, out instanceBits);
                text_computedDataTotals.Text = "Instance bits: " + instanceBits + ", padding: " + totalFreeBits;
                PopulateGridView(paddedLayout, paddedInfo, computedLayoutView);


//                PopulateGridView(layout, info, computedLayoutView);
            }
        }
    }
}
