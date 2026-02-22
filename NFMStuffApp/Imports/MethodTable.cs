using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WSystem.Runtime
{
    /*Fixed statements are not required in here as method tables are not part of GC and are not reallocated during runtime.*/
    public unsafe ref struct MethodTable
    {
        public static MethodTable Null => new MethodTable(IntPtr.Zero);

        private IntPtr pMt;

        internal MethodTable(IntPtr _pMT)
        {
            pMt = _pMT;
        }

        public bool IsNull => pMt == IntPtr.Zero;

        /// <summary>
        /// The low WORD of the first field is the component size for array and string types.
        /// </summary>
        public ushort ComponentSize
        {
            get
            {
                if (!HasComponentSize) return 0;
                return ((ushort*)pMt)[0]; //RuntimeHelpers.CoreCLR.cs #line 432 [FieldOffset(0)]
            }
        }

        /// <summary>
        /// The flags for the current method table (only for not array or string types).
        /// </summary>
        public uint Flags
        {
            get
            {
                if (HasComponentSize) return 0;
                if (IsNull) return 0;
                return ((uint*)pMt)[0]; //RuntimeHelpers.CoreCLR.cs #line 438 [FieldOffset(0)]
            }
        }

        private uint Flags_Internal
        {
            get
            {
                if (IsNull) return 0;
                return ((uint*)pMt)[0];
            }
        }

        /// <summary>
        /// The base size of the type (used when allocating an instance on the heap).
        /// </summary>
        public uint BaseSize
        {
            get
            {
                if(IsNull) return 0;
                return ((uint*)(pMt + 4))[0]; //RuntimeHelpers.CoreCLR.cs #line 444 [FieldOffset(4)]
            }
        }

        // See additional native members in methodtable.h, not needed here yet.
        // 0x8: m_wFlags2 (additional flags)
        // 0xA: m_wToken (class token if it fits in 16 bits)
        // 0xC: m_wNumVirtuals

        /// <summary>
        /// The number of interfaces implemented by the current type.
        /// </summary>
        public ushort InterfaceCount
        {
            get
            {
                if(IsNull) return 0;
                return ((ushort*)(pMt + 0x0E))[0]; //RuntimeHelpers.CoreCLR.cs #line 455 [FieldOffset(0x0E)]
            }
        }

        // For DEBUG builds, there is a conditional field here (see methodtable.h again).
        // 0x10: debug_m_szClassName (display name of the class, for the debugger)
        // This is likely only DEBUG builds of .NET itself, didn't test it so idk.
        // However it is not needed so don't care.

        /// <summary>
        /// A pointer to the parent method table for the current one.
        /// </summary>
        public MethodTable ParentMethodTable
        {
            get
            {
                if(IsNull) return Null;
                return new MethodTable(((IntPtr*)(pMt + CLRMethodTableConstants.ParentMethodTableOffset))[0]); //RuntimeHelpers.CoreCLR.cs #line 464 [FieldOffset(ParentMethodTableOffset)]
            }
        }

        // Additional conditional fields (see methodtable.h).
        // m_pLoaderModule
        // m_pWriteableData
        // union {
        //   m_pEEClass (pointer to the EE class)
        //   m_pCanonMT (pointer to the canonical method table)
        // }

        /// <summary>
        /// This element type handle is in a union with additional info or a pointer to the interface map.
        /// Which one is used is based on the specific method table being in used (so this field is not
        /// always guaranteed to actually be a pointer to a type handle for the element type of this type).
        /// </summary>
        public IntPtr ElementType
        {
            get
            {
                if(IsNull) return IntPtr.Zero;
                return ((IntPtr*)(pMt + CLRMethodTableConstants.ElementTypeOffset))[0]; //RuntimeHelpers.CoreCLR.cs #line 480 [FieldOffset(ElementTypeOffset)]
            }
        }

        /// <summary>
        /// This interface map is a union with a multipurpose slot, so should be checked before use.
        /// </summary>
        public IntPtr InterfaceMap
        {
            get
            {
                if (IsNull) return IntPtr.Zero;
                return ((IntPtr*)(pMt + CLRMethodTableConstants.InterfaceMapOffset))[0]; //RuntimeHelpers.CoreCLR.cs #line 486 [FieldOffset(InterfaceMapOffset)]
            }
        }

        public bool HasComponentSize
        {
            get
            {
                if(IsNull) return false;
                return (Flags_Internal & CLRMethodTableConstants.enum_flag_HasComponentSize) != 0;
            }
        }

        public bool ContainsGCPointers
        {
            get
            {
                if (IsNull) return false;
                return (Flags & CLRMethodTableConstants.enum_flag_ContainsPointers) != 0;
            }
        }

        public bool NonTrivialInterfaceCast
        {
            get
            {
                if (IsNull) return false;
                return (Flags & CLRMethodTableConstants.enum_flag_NonTrivialInterfaceCast) != 0;
            }
        }

        public bool HasTypeEquivalence
        {
            get
            {
                if (IsNull) return false;
                return (Flags & CLRMethodTableConstants.enum_flag_HasTypeEquivalence) != 0;
            }
        }

        public bool HasDefaultConstructor
        {
            get
            {
                if (IsNull) return false;
                return (Flags & (CLRMethodTableConstants.enum_flag_HasComponentSize | CLRMethodTableConstants.enum_flag_HasDefaultCtor)) == CLRMethodTableConstants.enum_flag_HasDefaultCtor;
            }
        }

        public bool IsMultiDimensionalArray
        {
            get
            {
                if (!HasComponentSize) return false;
                // See comment on RawArrayData for details
                return BaseSize > (uint)(3 * sizeof(IntPtr));
            }
        }

        public int MultiDimensionalArrayRank
        {
            get
            {
                if (!HasComponentSize) return 0;
                // See comment on RawArrayData for details
                return (int)((BaseSize - (uint)(3 * sizeof(IntPtr))) / (uint)(2 * sizeof(int)));
            }
        }

        public bool IsInterface
        {
            get
            {
                if(IsNull) return false;
                return (Flags & CLRMethodTableConstants.enum_flag_Category_Mask) == CLRMethodTableConstants.enum_flag_Category_Interface;
            }
        }

        public bool IsValueType
        {
            get
            {
                if (IsNull) return false;
                return (Flags & CLRMethodTableConstants.enum_flag_Category_ValueType_Mask) == CLRMethodTableConstants.enum_flag_Category_ValueType;
            }
        }

        public bool IsNullable
        {
            get
            {
                if (IsNull) return false;
                return (Flags & CLRMethodTableConstants.enum_flag_Category_Mask) == CLRMethodTableConstants.enum_flag_Category_Nullable;
            }
        }

        public bool IsByRefLike
        {
            get
            {
                if (IsNull) return false;
                return (Flags & (CLRMethodTableConstants.enum_flag_HasComponentSize | CLRMethodTableConstants.enum_flag_IsByRefLike)) == CLRMethodTableConstants.enum_flag_IsByRefLike;
            }
        }

        // Warning! UNLIKE the similarly named Reflection api, this method also returns "true" for Enums.
        public bool IsPrimitive
        {
            get
            {
                if (IsNull) return false;
                return (Flags & CLRMethodTableConstants.enum_flag_Category_Mask) is CLRMethodTableConstants.enum_flag_Category_PrimitiveValueType or CLRMethodTableConstants.enum_flag_Category_TruePrimitive;
            }
        }

        public bool HasInstantiation
        {
            get
            {
                if (IsNull) return false;
                return (Flags & CLRMethodTableConstants.enum_flag_HasComponentSize) == 0 && (Flags & CLRMethodTableConstants.enum_flag_GenericsMask) != CLRMethodTableConstants.enum_flag_GenericsMask_NonGeneric;
            }
        }

        public bool IsGenericTypeDefinition
        {
            get
            {
                if(IsNull) return false;
                return (Flags & (CLRMethodTableConstants.enum_flag_HasComponentSize | CLRMethodTableConstants.enum_flag_GenericsMask)) == CLRMethodTableConstants.enum_flag_GenericsMask_TypicalInst;
            }
        }

        public bool IsConstructedGenericType
        {
            get
            {
                if (IsNull) return false;
                uint genericsFlags = Flags & (CLRMethodTableConstants.enum_flag_HasComponentSize | CLRMethodTableConstants.enum_flag_GenericsMask);
                return genericsFlags == CLRMethodTableConstants.enum_flag_GenericsMask_GenericInst || genericsFlags == CLRMethodTableConstants.enum_flag_GenericsMask_SharedInst;
            }
        }

        public IntPtr AsPointer() => pMt;

        public override int GetHashCode() => pMt.GetHashCode();

        public static bool operator ==(MethodTable l, MethodTable r) => l.pMt == r.pMt;
        public static bool operator !=(MethodTable l, MethodTable r) => l.pMt != r.pMt;

        public static explicit operator IntPtr(MethodTable mt) => mt.pMt;
        public static implicit operator bool(MethodTable mt) => !mt.IsNull;

        public override bool Equals(object obj)
        {
            return false;
        }

        public bool Equals(MethodTable other)
        {
            return pMt == other.pMt;
        }
    }

    //RuntimeHelpers.CoreCLR.cs #line 488
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class CLRMethodTableConstants
    {
        static CLRMethodTableConstants()
        {
            Type runtimeHelpers = typeof(RuntimeHelpers);
            Type methodTableType = runtimeHelpers.Assembly.GetType("System.Runtime.CompilerServices.MethodTable");

            foreach(FieldInfo fi in methodTableType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
            {
                switch (fi.Name)
                {
                    case "ParentMethodTable":
                        {
                            ParentMethodTableOffset = fi.GetCustomAttribute<FieldOffsetAttribute>().Value;
                            continue;
                        }
                    case "ElementType":
                        {
                            ElementTypeOffset = fi.GetCustomAttribute<FieldOffsetAttribute>().Value;
                            continue;
                        }
                    case "InterfaceMap":
                        {
                            InterfaceMapOffset = fi.GetCustomAttribute<FieldOffsetAttribute>().Value;
                            continue;
                        }
                }
            }
        }

        // WFLAGS_LOW_ENUM
        internal const uint enum_flag_GenericsMask = 0x00000030;
        internal const uint enum_flag_GenericsMask_NonGeneric = 0x00000000; // no instantiation
        internal const uint enum_flag_GenericsMask_GenericInst = 0x00000010; // regular instantiation, e.g. List<String>
        internal const uint enum_flag_GenericsMask_SharedInst = 0x00000020; // shared instantiation, e.g. List<__Canon> or List<MyValueType<__Canon>>
        internal const uint enum_flag_GenericsMask_TypicalInst = 0x00000030; // the type instantiated at its formal parameters, e.g. List<T>
        internal const uint enum_flag_HasDefaultCtor = 0x00000200;
        internal const uint enum_flag_IsByRefLike = 0x00001000;

        // WFLAGS_HIGH_ENUM
        internal const uint enum_flag_ContainsPointers = 0x01000000;
        internal const uint enum_flag_HasComponentSize = 0x80000000;
        internal const uint enum_flag_HasTypeEquivalence = 0x02000000;
        internal const uint enum_flag_Category_Mask = 0x000F0000;
        internal const uint enum_flag_Category_ValueType = 0x00040000;
        internal const uint enum_flag_Category_Nullable = 0x00050000;
        internal const uint enum_flag_Category_PrimitiveValueType = 0x00060000; // sub-category of ValueType, Enum or primitive value type
        internal const uint enum_flag_Category_TruePrimitive = 0x00070000; // sub-category of ValueType, Primitive (ELEMENT_TYPE_I, etc.)
        internal const uint enum_flag_Category_ValueType_Mask = 0x000C0000;
        internal const uint enum_flag_Category_Interface = 0x000C0000;
        // Types that require non-trivial interface cast have this bit set in the category
        internal const uint enum_flag_NonTrivialInterfaceCast = 0x00080000 // enum_flag_Category_Array
                                                             | 0x40000000 // enum_flag_ComObject
                                                             | 0x00400000 // enum_flag_ICastable;
                                                             | 0x10000000 // enum_flag_IDynamicInterfaceCastable;
                                                             | 0x00040000; // enum_flag_Category_ValueType
        internal static readonly int ParentMethodTableOffset;
        internal static readonly int ElementTypeOffset;
        internal static readonly int InterfaceMapOffset;
    }
}
