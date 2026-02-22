using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace WSystem.Runtime
{
    /// <summary>
    /// Contains function to interact with the CLR (Common Language Runtime).
    /// </summary>
    public static unsafe class CLR
    {
        private const int s_ILG_sz = 16;
        private static readonly Type RuntimeType;
        private static readonly Type RuntimeFieldInfoType;
        private static readonly DynamicMethod DynM_Type_GetUnderlyingNativeHandle;
        private static readonly Func<Type, IntPtr> Type_GetUnderlyingNativeHandle;

        private static readonly DynamicMethod DynM_RuntimeHelpers_GetMethodTable;
        private static readonly Func<object, IntPtr> RuntimeHelpers_GetMethodTable;
        /*private static readonly PropertyInfo RuntimeFieldInfo_FieldAccessorPropertyInfo;
        private static readonly Func<object, object, object> FieldAcessor_GetValue;*/
        private static readonly Func<FieldInfo, int> RuntimeFieldHandle_GetInstanceFieldOffset;

        static CLR()
        {
            RuntimeType = typeof(CLR).GetType();
            RuntimeFieldInfoType = Type.GetType("System.Reflection.RtFieldInfo");
            /*RuntimeFieldInfo_FieldAccessorPropertyInfo = RuntimeFieldInfoType.GetProperty("FieldAccessor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            FieldAcessor_GetValue = RuntimeFieldInfo_FieldAccessorPropertyInfo.PropertyType.GetMethod("GetValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).CreateDelegate<Func<object, object, object>>();*/
            object obj =
            /*RuntimeFieldHandle_GetInstanceFieldOffset =*/ typeof(RuntimeFieldHandle).GetMethod("GetInstanceFieldOffset", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).CreateDelegate(typeof(Func<,>).MakeGenericType([RuntimeFieldInfoType, typeof(int)]));
            RuntimeFieldHandle_GetInstanceFieldOffset = Unsafe.As<Func<FieldInfo, int>>(obj);
            Type[] arr = new Type[1];
            arr[0] = typeof(Type);
            DynM_Type_GetUnderlyingNativeHandle = new DynamicMethod("Type_GetUnderlyingNativeHandle", typeof(IntPtr), arr, true);
            ILGenerator il = DynM_Type_GetUnderlyingNativeHandle.GetILGenerator(s_ILG_sz);
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(OpCodes.Callvirt, RuntimeType.GetMethod("GetUnderlyingNativeHandle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), null);
            il.Emit(OpCodes.Ret);
            Type_GetUnderlyingNativeHandle = DynM_Type_GetUnderlyingNativeHandle.CreateDelegate<Func<Type, IntPtr>>();
            
            arr[0] = typeof(object);
            Type runtimeHelpers = typeof(RuntimeHelpers);
            DynM_RuntimeHelpers_GetMethodTable = new DynamicMethod("RuntimeHelpers_GetMethodTable", typeof(IntPtr), arr, true);
            il = DynM_RuntimeHelpers_GetMethodTable.GetILGenerator(s_ILG_sz);
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(OpCodes.Call, runtimeHelpers.GetMethod("GetMethodTable", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, arr), null);
            il.Emit(OpCodes.Ret);
            RuntimeHelpers_GetMethodTable = DynM_RuntimeHelpers_GetMethodTable.CreateDelegate<Func<object, IntPtr>>();
        }

        public static T GetRandomData<T>() where T : unmanaged, allows ref struct
        {
            T t;
            Random.Shared.NextBytes(new Span<byte>(&t, Unsafe.SizeOf<T>()));
            return t;
        }

        /// <summary>
        /// Gets the <see cref="MethodTable"/> of the given <see cref="Type"/> <paramref name="type"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="MethodTable"/> of the given <see cref="Type"/> <paramref name="type"/>.
        /// </returns>
        public static MethodTable GetTypeMethodTable(Type type)
        {
            if (type is null) return MethodTable.Null;
            Debug.Assert(RuntimeType is not null);
            if (type.GetType() != RuntimeType) return MethodTable.Null;
            Debug.Assert(Type_GetUnderlyingNativeHandle is not null);
            return new MethodTable(Type_GetUnderlyingNativeHandle(type));
        }

        /// <summary>
        /// Gets the <see cref="MethodTable"/> of the given <see cref="object"/> <paramref name="obj"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="MethodTable"/> of the given <see cref="object"/> <paramref name="obj"/>.
        /// </returns>
        public static MethodTable GetMethodTable(object obj)
        {
            if (obj is null) return MethodTable.Null;
            Debug.Assert(RuntimeHelpers_GetMethodTable is not null);
            return new MethodTable(RuntimeHelpers_GetMethodTable(obj));
        }

        /// <summary>
        /// Gets the address of the given <paramref name="value"/>.
        /// </summary>
        /// <returns>
        /// The address of the <paramref name="value"/> data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr GetAddressOf<T>(ref T value) where T : allows ref struct
        {
            //No null checking, convert null objects to pointers aswell for consistency reasons.
            Type t = typeof(T);
            if(!t.IsValueType)
                return Unsafe.As<T, IntPtr>(ref value);
            else
                return (IntPtr)Unsafe.AsPointer<T>(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int GetArrayDataSize() => Unsafe.SizeOf<IntPtr>() * 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool IsAssignableNull(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            MethodTable mt = GetTypeMethodTable(type);
            if (mt.IsValueType && mt.IsNullable) return true;
            return !mt.IsValueType;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool ImplementsInterface<TInterface, T>(T value) where T : allows ref struct
        {
            if (typeof(T).IsValueType) return ImplementsInterface(typeof(T), typeof(TInterface));
            return ImplementsInterface(Unsafe.As<T, object>(ref value).GetType(), typeof(TInterface));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool ImplementsInterface<TInterface, T>()
        {
            return ImplementsInterface(typeof(T), typeof(TInterface));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool ImplementsInterface(Type type, Type interfaceType)
        {
            if (type is null || interfaceType is null) return false;
            MethodTable targetMt = GetTypeMethodTable(interfaceType);
            if(!targetMt.IsInterface) return false;
            MethodTable mt = GetTypeMethodTable(type);
            ushort interfaceCount = mt.InterfaceCount;
            if (interfaceCount <= 0) return false;
            IntPtr interfaceMap = mt.InterfaceMap;
            IntPtr targetInterfacePtr = targetMt.AsPointer();
            for(int i = 0; i < interfaceCount; i++)
            {
                if(Unsafe.ReadUnaligned<IntPtr>((interfaceMap + i * Unsafe.SizeOf<IntPtr>()).ToPointer()) == targetInterfacePtr)
                    return true;
            }
            return false;
        }

        //Following commented methods were crashing the CLR runtime engine during garbage collection.
        /*[EditorBrowsable(EditorBrowsableState.Never)]
        internal static unsafe void ChangeObjectType(object obj, Type newType)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentNullException.ThrowIfNull(newType);
            RawData dt = RawData.AsRawData(obj);
            ref IntPtr ptr = ref Unsafe.Subtract(ref Unsafe.As<byte, IntPtr>(ref dt.Data), 1);
            MethodTable newMtd = GetTypeMethodTable(newType);
            ptr = newMtd.AsPointer();
            GC.KeepAlive(obj);
            GC.KeepAlive(newType);
            return;

        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static unsafe NewType ChangeObjectType<NewType>(object obj) where NewType : class
        {
            ArgumentNullException.ThrowIfNull(obj);
            ChangeObjectType(obj, typeof(NewType));
            return Unsafe.As<NewType>(obj);
        }*/

        /// <summary>
        /// Does not return true size for arrays.
        /// </summary>
        /// <returns>The size of a given type when allocating instance on the heap.</returns>
        public static uint GetHeapSizeOfType(Type type)
        {
            if (type is null) return 0;
            return GetTypeMethodTable(type).BaseSize;
        }

        /// <summary>
        /// Returns a value indicating wether a <see langword="struct"/> is a <see langword="ref struct"/>.
        /// </summary>
        /// <typeparam name="T">A struct type.</typeparam>
        /// <returns>
        /// <see langword="true"/> if <typeparamref name="T"/> is a <see langword="ref struct"/>; <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool IsRefLikeStruct<T>() where T : struct, allows ref struct
        {
            Type t = typeof(T);
            return t.IsByRefLike;
        }

        /// <summary>
        /// Returns a value that indicates whether the specified type contains references.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the given type contains references or by-refs; otwherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Unlike <see cref="RuntimeHelpers.IsReferenceOrContainsReferences{T}"/>, this function does not return <see langword="true"/>
        /// if <typeparamref name="T"/> is a <see langword="class"/> type.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool ContainsReferences<T>() where T : allows ref struct
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Type t = typeof(T);
                if (!t.IsClass) return true;
                return GetTypeMethodTable(t).ContainsGCPointers;
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool ContainsReferences(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            return GetTypeMethodTable(type).ContainsGCPointers;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool IsReferenceOrContainsReferences(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            return GetTypeMethodTable(type).ContainsGCPointers || type.IsClass || type.IsInterface;
        }

        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> BitCast<T>(Span<T> span)
        {
            SpanData<T> data = SpanData<T>.From(ref span);
            SpanData<byte> byteData = new SpanData<byte>(ref Unsafe.As<T, byte>(ref data.Reference), data.Length * Unsafe.SizeOf<T>());
            return SpanData<byte>.BackToSpan(ref byteData);
        }*/

        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<byte> BitCast<T>(ReadOnlySpan<T> span)
        {
            ReadOnlySpanData<T> data = ReadOnlySpanData<T>.From(ref span);
            ReadOnlySpanData<byte> byteData = new ReadOnlySpanData<byte>(ref Unsafe.As<T, byte>(ref data.Reference), data.Length * Unsafe.SizeOf<T>());
            return ReadOnlySpanData<byte>.BackToSpan(ref byteData);
        }*/
        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref byte GetObjectDataReference(object obj)
        {
            if (obj is null) return ref Unsafe.NullRef<byte>();
            return ref Unsafe.As<RawData>(obj).Data;
        }*/
        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T MemberwiseCloneObject<T>(T value) where T : class
        {
            T result = (T)MemberCloner.MemberwiseClone(value);
            GC.KeepAlive(value);
            return result;
        }*/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe TTo UnsafeDelegateCast<TTo>(Delegate value) where TTo : Delegate
        {
            return Unsafe.As<Delegate, TTo>(ref value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool SafeDelegateCast<TTo>(Delegate value, out TTo result) where TTo : Delegate
        {
            result = default;
            Delegate cast = Delegate.CreateDelegate(typeof(TTo), value.Target, value.Method, false);
            if (cast is null) return false;
            result = Unsafe.As<TTo>(cast);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo GetPropertyBackingField(object obj, string propertyName)
        {
            if(obj is null) return null;
            return GetPropertyBackingField(obj.GetType(), propertyName);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetPropertyBackingField(object obj, string propertyName, out FieldInfo fieldInfo)
        {
            fieldInfo = null;
            if(obj is null) return false;
            return TryGetPropertyBackingField(obj.GetType(), propertyName, out fieldInfo);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo GetPropertyBackingField<T>(string propertyName) where T : allows ref struct
        {
            return GetPropertyBackingField(typeof(T), propertyName);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetPropertyBackingField<T>(string propertyName, out FieldInfo fieldInfo) where T : allows ref struct
        {
            return TryGetPropertyBackingField(typeof(T), propertyName, out fieldInfo);
        }


        /*public static FieldInfo GetPropertyBackingField(Type type, string propertyName)
        {
            if(type is null) return null;
            if (string.IsNullOrWhiteSpace(propertyName)) return null;
            PropertyInfo propInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if(propInfo is null) return null;
            return GetPropertyBackingField(type, propInfo);
        }
        public static bool TryGetPropertyBackingField(Type type, string propertyName, out FieldInfo fieldInfo)
        {
            fieldInfo = null;
            if (type is null) return false;
            if (string.IsNullOrWhiteSpace(propertyName)) return false;
            PropertyInfo propInfo = null;
            try
            {
                propInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }
            catch 
            { 
                propInfo = null;
            }
            if(propInfo is null) return false;
            return TryGetPropertyBackingField(type, propInfo, out fieldInfo);
        }*/

       /* public static FieldInfo GetPropertyBackingField(Type type, PropertyInfo propertyInfo)
        {
            if(propertyInfo is null) return null;
            BackingFieldAttribute att = propertyInfo.GetCustomAttribute<BackingFieldAttribute>();
            FieldInfo result = null;
            MethodInfo accessor = null;
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic;

            accessor = propertyInfo.GetMethod;
            if (accessor is null)
            {
                accessor = propertyInfo.SetMethod;
                if (accessor is null) return null;
            }
            if (accessor.IsStatic)
                flags |= BindingFlags.Static;
            else
                flags |= BindingFlags.Instance;

            if (att is not null)
            {
                if(!string.IsNullOrWhiteSpace(att.FieldName))
                {
                    result = type.GetField(att.FieldName, flags);
                    if(result is not null) return result;
                }
            }
            if (accessor.GetCustomAttribute<CompilerGeneratedAttribute>() is null) return null;
            result = type.GetField($"<{propertyInfo.Name}>k_BackingField");
            return result;
        }
        public static bool TryGetPropertyBackingField(Type type, PropertyInfo propertyInfo, out FieldInfo fi)
        {
            try
            {
                fi = GetPropertyBackingField(type, propertyInfo);
                return fi is not null;
            }
            catch
            {
                fi = null;
                return false;
            }
        }*/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRuntimeType(Type type)
        {
            if(type is null) return false;
            return type.GetType() == RuntimeType;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRuntimeFieldInfo(FieldInfo fieldInfo)
        {
            if(fieldInfo is null) return false;
            return fieldInfo.GetType() == RuntimeFieldInfoType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int OffsetOf<T>(string fieldName) where T : allows ref struct
        {
            return OffsetOf(typeof(T), fieldName);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int OffsetOf(Type type, string fieldName)
        {
            FieldInfo fi = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi is null) throw new ArgumentException($"Offset of field {fieldName} was not found.", nameof(fieldName));
            return OffsetOf(fi);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int OffsetOf(FieldInfo fieldInfo)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo);
            if (fieldInfo.IsStatic) throw new ArgumentException("Cannot get offset of a static field.", nameof(fieldInfo));
            if (!IsRuntimeFieldInfo(fieldInfo)) throw new NotSupportedException("Non-RuntimeFieldInfo fields are currently not supported.");
            return RuntimeFieldHandle_GetInstanceFieldOffset(fieldInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Cast<TAs>(object obj, out TAs cast)
        {
            cast = default(TAs);
            if(obj is TAs casted)
            {
                cast = casted;
                return true;
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAs Cast<TAs>(object obj)
        {
            if(obj is TAs casted) return casted;
            return default(TAs);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadBoxedValue<T>(object box, out T value) where T : struct
        {
            value = default(T);
            if(box is T val)
            {
                value = val;
                return true;
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadBoxedValue<T>(object box) where T : struct
        {
            return (T)box;
        }
        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WriteToBoxedValue<T>(object box, T value) where T : struct
        {
            if(box is T)
            {
                Unsafe.WriteUnaligned(ref GetObjectDataReference(box), value);
                return true;
            }
            return false;
        }*/
    }
}
