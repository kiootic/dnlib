﻿using System.Collections.Generic;
using dot10.DotNet.MD;

#pragma warning disable 0429	// Disable unreachable code warnings for now...

namespace dot10.DotNet {
	/// <summary>
	/// Compares types, signatures, methods, fields, properties, events
	/// </summary>
	struct SigComparer {
		RecursionCounter recursionCounter;

		const bool compareDeclaringType = false;	//TODO: Should be an instance flag

		public bool Compare(IType a, IType b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			TypeDef tda = a as TypeDef, tdb = b as TypeDef;
			if (tda != null && tdb != null) {
				result = Compare(tda, tdb);
				goto exit;
			}
			TypeRef tra = a as TypeRef, trb = b as TypeRef;
			if (tra != null && trb != null) {
				result = Compare(tra, trb);
				goto exit;
			}
			TypeSpec tsa = a as TypeSpec, tsb = b as TypeSpec;
			if (tsa != null && tsb != null) {
				result = Compare(tsa, tsb);
				goto exit;
			}
			TypeSig sa = a as TypeSig, sb = b as TypeSig;
			if (sa != null && sb != null) {
				result = Compare(sa, sb);
				goto exit;
			}

			if (tda != null && trb != null)
				result = Compare(tda, trb);
			else if (tra != null && tdb != null)
				result = Compare(tdb, tra);
			else if (tda != null && tsb != null)
				result = Compare(tda, tsb);
			else if (tsa != null && tdb != null)
				result = Compare(tdb, tsa);
			else if (tda != null && sb != null)
				result = Compare(tda, sb);
			else if (sa != null && tdb != null)
				result = Compare(tdb, sa);
			else if (tra != null && tsb != null)
				result = Compare(tra, tsb);
			else if (tsa != null && trb != null)
				result = Compare(trb, tsa);
			else if (tra != null && sb != null)
				result = Compare(tra, sb);
			else if (sa != null && trb != null)
				result = Compare(trb, sa);
			else if (tsa != null && sb != null)
				result = Compare(tsa, sb);
			else if (sa != null && tsb != null)
				result = Compare(tsb, sa);
			else
				result = false;	// Should never be reached

exit:
			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(TypeRef a, TypeDef b) {
			return Compare(b, a);
		}

		public bool Compare(TypeDef a, TypeRef b) {
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result = false;

			if (UTF8String.CompareTo(a.Name, b.Name) != 0 || UTF8String.CompareTo(a.Namespace, b.Namespace) != 0)
				goto exit;

			var scope = b.ResolutionScope;
			var dtb = scope as TypeRef;
			if (dtb != null) {	// nested type
				result = Compare(a.DeclaringType, dtb);	// Compare enclosing types
				goto exit;
			}
			if (a.DeclaringType != null)
				goto exit;	// a is nested, b isn't

			IModule bMod = scope as IModule;
			if (bMod != null) {	// 'b' is defined in the same assembly as 'a'
				result = Compare((IModule)a.OwnerModule, (IModule)bMod);
				goto exit;
			}
			var bAsm = scope as AssemblyRef;
			if (bAsm != null) {
				var aMod = a.OwnerModule;
				result = aMod != null && Compare(aMod.Assembly, bAsm);
			}
exit:
			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(TypeSpec a, TypeDef b) {
			return Compare(b, a);
		}

		public bool Compare(TypeDef a, TypeSpec b) {
			if (a == null || b == null)
				return false;
			return Compare(a, b.TypeSig);
		}

		public bool Compare(TypeSig a, TypeDef b) {
			return Compare(b, a);
		}

		public bool Compare(TypeDef a, TypeSig b) {
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			var b2 = b as TypeDefOrRefSig;
			bool result = b2 != null && Compare(a, b2.TypeDefOrRef);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(TypeSpec a, TypeRef b) {
			return Compare(b, a);
		}

		public bool Compare(TypeRef a, TypeSpec b) {
			if (a == null || b == null)
				return false;
			return Compare(a, b.TypeSig);
		}

		public bool Compare(TypeSig a, TypeRef b) {
			return Compare(b, a);
		}

		public bool Compare(TypeRef a, TypeSig b) {
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			var b2 = b as TypeDefOrRefSig;
			bool result = b2 != null && Compare(a, b2.TypeDefOrRef);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(TypeSig a, TypeSpec b) {
			return Compare(b, a);
		}

		public bool Compare(TypeSpec a, TypeSig b) {
			if (a == null || b == null)
				return false;
			return Compare(a.TypeSig, b);
		}

		public bool Compare(TypeRef a, TypeRef b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			if (UTF8String.CompareTo(a.Name, b.Name) != 0 || UTF8String.CompareTo(a.Namespace, b.Namespace) != 0)
				result = false;
			else
				result = Compare(a.ResolutionScope, b.ResolutionScope);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(TypeDef a, TypeDef b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			if (UTF8String.CompareTo(a.Name, b.Name) != 0 || UTF8String.CompareTo(a.Namespace, b.Namespace) != 0)
				result = false;
			else if (!Compare(a.DeclaringType, b.DeclaringType))
				result = false;
			else
				result = Compare(a.OwnerModule, b.OwnerModule);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(TypeSpec a, TypeSpec b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = Compare(a.TypeSig, b.TypeSig);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(IResolutionScope a, IResolutionScope b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			TypeRef ea = a as TypeRef, eb = b as TypeRef;
			if (ea != null || eb != null) {	// if one of them is a TypeRef, the other one must be too
				result = Compare(ea, eb);
				goto exit;
			}
			IModule ma = a as IModule, mb = b as IModule;
			if (ma != null && mb != null) {	// only compare if both are modules
				result = Compare(ma, mb);
				goto exit;
			}
			AssemblyRef aa = a as AssemblyRef, ab = b as AssemblyRef;
			if (aa != null && ab != null) {	// only compare if both are assemblies
				result = Compare((IAssembly)aa, (IAssembly)ab);
				goto exit;
			}
			//TODO: Handle the case when one of them is a ModuleRef/ModuleDef and the other is an AssemblyRef
			result = false;

exit:
			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(IModule a, IModule b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			//TODO: Should we perform a case insensitive comparison here?
			bool result = UTF8String.CompareTo(a.Name, b.Name) == 0;

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(ModuleDef a, ModuleDef b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = Compare((IModule)a, (IModule)b) && Compare(a.Assembly, b.Assembly);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(IAssembly a, IAssembly b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			//TODO: Should we perform a case insensitive comparison here?
			bool result = UTF8String.CompareTo(a.Name, b.Name) == 0;

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(TypeSig a, TypeSig b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			if (a.ElementType != b.ElementType) {
				// Signatures must be identical. It's possible to have a U4 in a sig (short form
				// of System.UInt32), or a ValueType + System.UInt32 TypeRef (long form), but these
				// should not match in a sig (also the long form is invalid).
				result = false;
			}
			else {
				switch (a.ElementType) {
				case ElementType.Void:
				case ElementType.Boolean:
				case ElementType.Char:
				case ElementType.I1:
				case ElementType.U1:
				case ElementType.I2:
				case ElementType.U2:
				case ElementType.I4:
				case ElementType.U4:
				case ElementType.I8:
				case ElementType.U8:
				case ElementType.R4:
				case ElementType.R8:
				case ElementType.String:
				case ElementType.TypedByRef:
				case ElementType.I:
				case ElementType.U:
				case ElementType.Object:
				case ElementType.Sentinel:
					result = true;
					break;

				case ElementType.Ptr:
				case ElementType.ByRef:
				case ElementType.Array:
				case ElementType.SZArray:
				case ElementType.Pinned:
					result = Compare(a.Next, b.Next);
					break;

				case ElementType.ValueType:
				case ElementType.Class:
					result = Compare((a as ClassOrValueTypeSig).TypeDefOrRef, (b as ClassOrValueTypeSig).TypeDefOrRef);
					break;

				case ElementType.Var:
				case ElementType.MVar:
					result = (a as GenericSig).Number == (b as GenericSig).Number;
					break;

				case ElementType.GenericInst:
					var gia = (GenericInstSig)a;
					var gib = (GenericInstSig)b;
					if (!Compare(gia.GenericType, gib.GenericType))
						result = false;
					else
						result = Compare(gia.GenericArguments, gia.GenericArguments);
					break;

				case ElementType.FnPtr:
					result = Compare((a as FnPtrSig).MethodSig, (b as FnPtrSig).MethodSig);
					break;

				case ElementType.CModReqd:
				case ElementType.CModOpt:
					result = Compare((a as ModifierSig).Modifier, (b as ModifierSig).Modifier) && Compare(a.Next, b.Next);
					break;

				case ElementType.ValueArray:
					result = (a as ValueArraySig).Size == (b as ValueArraySig).Size && Compare(a.Next, b.Next);
					break;

				case ElementType.Module:
					result = (a as ModuleSig).Index == (b as ModuleSig).Index && Compare(a.Next, b.Next);
					break;

				case ElementType.End:
				case ElementType.R:
				case ElementType.Internal:
				default:
					result = false;
					break;
				}
			}

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(IList<TypeSig> a, IList<TypeSig> b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			if (a.Count != b.Count)
				result = false;
			else {
				int i;
				for (i = 0; i < a.Count; i++) {
					if (!Compare(a[i], b[i]))
						break;
				}
				result = i == a.Count;
			}

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(CallingConventionSig a, CallingConventionSig b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			if (a.GetCallingConvention() != b.GetCallingConvention())
				result = false;
			else {
				switch (a.GetCallingConvention() & CallingConvention.Mask) {
				case CallingConvention.Default:
				case CallingConvention.C:
				case CallingConvention.StdCall:
				case CallingConvention.ThisCall:
				case CallingConvention.FastCall:
				case CallingConvention.VarArg:
				case CallingConvention.Property:
					MethodBaseSig ma = a as MethodBaseSig, mb = b as MethodBaseSig;
					result = ma != null && mb != null && Compare(ma, mb);
					break;

				case CallingConvention.Field:
					FieldSig fa = a as FieldSig, fb = b as FieldSig;
					result = fa != null && fb != null && Compare(fa, fb);
					break;

				case CallingConvention.LocalSig:
					LocalSig la = a as LocalSig, lb = b as LocalSig;
					result = la != null && lb != null && Compare(la, lb);
					break;

				case CallingConvention.GenericInst:
					GenericInstMethodSig ga = a as GenericInstMethodSig, gb = b as GenericInstMethodSig;
					result = ga != null && gb != null && Compare(ga, gb);
					break;

				case CallingConvention.Unmanaged:
				case CallingConvention.NativeVarArg:
				default:
					result = false;
					break;
				}
			}

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(MethodBaseSig a, MethodBaseSig b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			if (a.GetCallingConvention() != b.GetCallingConvention())
				result = false;
			else if (Compare(a.RetType, b.RetType))
				result = false;
			else if (!Compare(a.Params, b.Params))
				result = false;
			else if (a.Generic && a.GenParamCount != b.GenParamCount)
				result = false;
			else {
				//TODO: There should be an option to ignore the params after the sentinel,
				//		and it should default to 'ignore them'.
				result = Compare(a.ParamsAfterSentinel, b.ParamsAfterSentinel);
			}

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(FieldSig a, FieldSig b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = a.GetCallingConvention() == b.GetCallingConvention() && Compare(a.Type, b.Type);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(LocalSig a, LocalSig b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = a.GetCallingConvention() == b.GetCallingConvention() && Compare(a.Locals, b.Locals);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(GenericInstMethodSig a, GenericInstMethodSig b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = a.GetCallingConvention() == b.GetCallingConvention() && Compare(a.GenericArguments, b.GenericArguments);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(IMethod a, IMethod b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			MethodDef mda = a as MethodDef, mdb = b as MethodDef;
			if (mda != null && mdb != null) {
				result = Compare(mda, mdb);
				goto exit;
			}
			MemberRef mra = a as MemberRef, mrb = b as MemberRef;
			if (mra != null && mrb != null) {
				result = Compare(mra, mrb);
				goto exit;
			}
			MethodSpec msa = a as MethodSpec, msb = b as MethodSpec;
			if (msa != null && msb != null) {
				result = Compare(msa, msb);
				goto exit;
			}
			if (mda != null && mrb != null) {
				result = Compare(mda, mrb);
				goto exit;
			}
			if (mra != null && mdb != null) {
				result = Compare(mdb, mra);
				goto exit;
			}
			result = false;
exit:
			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(MemberRef a, MethodDef b) {
			return Compare(b, a);
		}

		public bool Compare(MethodDef a, MemberRef b) {
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			//TODO: If a.IsPrivateScope, then you should probably always return false since Method
			//		tokens must be used to call the method.

			bool result = UTF8String.CompareTo(a.Name, b.Name) == 0 &&
					Compare(a.Signature, b.Signature) &&
					(!compareDeclaringType || Compare(a.DeclaringType, b.Class));

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(MethodDef a, MethodDef b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = UTF8String.CompareTo(a.Name, b.Name) == 0 &&
					Compare(a.Signature, b.Signature) &&
					(!compareDeclaringType || Compare(a.DeclaringType, b.DeclaringType));

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(MemberRef a, MemberRef b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = UTF8String.CompareTo(a.Name, b.Name) == 0 &&
					Compare(a.Signature, b.Signature) &&
					(!compareDeclaringType || Compare(a.Class, b.Class));

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(MethodSpec a, MethodSpec b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = Compare(a.Method, b.Method) && Compare(a.Instantiation, b.Instantiation);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(IMemberRefParent a, IMemberRefParent b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			ITypeDefOrRef ita = a as ITypeDefOrRef, itb = b as ITypeDefOrRef;
			if (ita != null && itb != null) {
				result = Compare(ita, itb);
				goto exit;
			}
			ModuleRef moda = a as ModuleRef, modb = b as ModuleRef;
			if (moda != null && modb != null) {
				result = Compare((IModule)moda, (IModule)modb);
				goto exit;
			}
			MethodDef ma = a as MethodDef, mb = b as MethodDef;
			if (ma != null && mb != null) {
				result = Compare(ma, mb);
				goto exit;
			}
			var td = a as TypeDef;
			if (td != null && modb != null) {
				result = CompareGlobal(td, modb);
				goto exit;
			}
			td = b as TypeDef;
			if (td != null && moda != null) {
				result = CompareGlobal(td, moda);
				goto exit;
			}
			var tr = a as TypeRef;
			if (tr != null && modb != null) {
				result = CompareGlobal(tr, modb);
				goto exit;
			}
			tr = b as TypeRef;
			if (tr != null && moda != null) {
				result = CompareGlobal(tr, moda);
				goto exit;
			}

			result = false;
exit:
			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(IField a, IField b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			FieldDef fa = a as FieldDef, fb = b as FieldDef;
			if (fa != null && fb != null) {
				result = Compare(fa, fb);
				goto exit;
			}
			MemberRef ma = a as MemberRef, mb = b as MemberRef;
			if (ma != null && mb != null) {
				result = Compare(ma, mb);
				goto exit;
			}
			if (fa != null && mb != null) {
				result = Compare(fa, mb);
				goto exit;
			}
			if (fb != null && ma != null) {
				result = Compare(fb, ma);
				goto exit;
			}

			result = false;
exit:
			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(FieldDef a, MemberRef b) {
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = UTF8String.CompareTo(a.Name, b.Name) == 0 &&
					Compare(a.Signature, b.Signature) &&
					(!compareDeclaringType || Compare(a.DeclaringType, b.Class));

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(FieldDef a, FieldDef b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			bool result = UTF8String.CompareTo(a.Name, b.Name) == 0 &&
					Compare(a.Signature, b.Signature) &&
					(!compareDeclaringType || Compare(a.DeclaringType, b.DeclaringType));

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(PropertyDef a, PropertyDef b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			//TODO: Also compare its declaring type if compareDeclaringType is true
			bool result = UTF8String.CompareTo(a.Name, b.Name) == 0 &&
					Compare(a.Type, b.Type);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		public bool Compare(EventDef a, EventDef b) {
			if (a == b)
				return true;
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;

			//TODO: Also compare its declaring type if compareDeclaringType is true
			bool result = UTF8String.CompareTo(a.Name, b.Name) == 0 &&
					Compare(a.Type, b.Type);

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		// Compares a with b, and a must be the global type
		private bool CompareGlobal(TypeDef a, ModuleRef b) {
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result;

			var aMod = a.OwnerModule;
			if (aMod == null)
				result = false;
			else if (aMod.Types.Count == 0 || aMod.Types[0] != a)
				result = false;	// 'a' is not the global type
			else if (!Compare((IModule)aMod, (IModule)b))
				result = false;
			else
				result = true;

			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		// Compares a with b, and a must be the global type
		private bool CompareGlobal(TypeRef a, ModuleRef b) {
			if (a == null || b == null)
				return false;
			if (recursionCounter.IncrementRecursionCounter())
				return false;
			bool result = false;

			var scope = a.ResolutionScope;
			if (scope == null || (scope is TypeRef))
				goto exit;
			var aMod = scope as IModule;
			if (aMod != null && !Compare(aMod, b))
					goto exit;
			result = IsGlobalType(a);
exit:
			recursionCounter.DecrementRecursionCounter();
			return result;
		}

		private static bool IsGlobalType(TypeRef a) {
			if (a == null)
				return false;
			var scope = a.ResolutionScope;
			if (scope == null)
				return false;
			if (scope is TypeRef)
				return false;
			// scope is AssemblyRef, ModuleDef, or ModuleRef

			//TODO: Resolve it and check whether TypeDef is the 1st type in its module
			// Until then, compare it by name
			return UTF8String.CompareTo(a.Name, new UTF8String("<Module>")) == 0;
		}
	}
}
