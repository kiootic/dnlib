﻿using System;
using System.Collections.Generic;
using dot10.DotNet.MD;

namespace dot10.DotNet {
	/// <summary>
	/// The table row can be referenced by a MD token
	/// </summary>
	public interface IMDTokenProvider {
		/// <summary>
		/// Returns the metadata token
		/// </summary>
		MDToken MDToken { get; }
	}

	/// <summary>
	/// Interface to access an <see cref="AssemblyRef"/> or an <see cref="AssemblyDef"/>
	/// </summary>
	public interface IAssembly : IFullName {
		/// <summary>
		/// The assembly version
		/// </summary>
		Version Version { get; set; }

		/// <summary>
		/// Assembly flags
		/// </summary>
		AssemblyFlags Flags { get; set; }

		/// <summary>
		/// Public key or public key token
		/// </summary>
		PublicKeyBase PublicKeyOrToken { get; }

		/// <summary>
		/// Simple assembly name
		/// </summary>
		UTF8String Name { get; set; }

		/// <summary>
		/// Locale, aka culture
		/// </summary>
		UTF8String Locale { get; set; }
	}

	static partial class Extensions {
		/// <summary>
		/// Checks whether <paramref name="asm"/> appears to be the core library (eg.
		/// mscorlib or System.Runtime)
		/// </summary>
		/// <param name="asm">The assembly</param>
		internal static bool IsCorLib(this IAssembly asm) {
			string asmName;
			return asm != null &&
				asm.PublicKeyOrToken != null &&
				UTF8String.IsNullOrEmpty(asm.Locale) &&
				((asmName = UTF8String.ToSystemStringOrEmpty(asm.Name).ToLowerInvariant()) == "mscorlib" ||
				asmName == "system.runtime");
		}
	}

	/// <summary>
	/// Interface to access a module def/ref
	/// </summary>
	public interface IModule {
		/// <summary>
		/// Gets the module name
		/// </summary>
		UTF8String Name { get; }
	}

	/// <summary>
	/// Interface to get the full name of a type, field, or method
	/// </summary>
	public interface IFullName {
		/// <summary>
		/// Gets the full name
		/// </summary>
		string FullName { get; }
	}

	/// <summary>
	/// Implemented by fields (<see cref="FieldDef"/> and <see cref="MemberRef"/>)
	/// </summary>
	public interface IField : IFullName {
	}

	/// <summary>
	/// Implemented by methods (<see cref="MethodDef"/>, <see cref="MemberRef"/> and <see cref="MethodSpec"/>)
	/// </summary>
	public interface IMethod : ITokenOperand, IFullName {
	}

	/// <summary>
	/// Implemented by tables that can be a token in the <c>ldtoken</c> instruction
	/// </summary>
	public interface ITokenOperand {
	}

	/// <summary>
	/// The table row can be referenced by a coded token
	/// </summary>
	public interface ICodedToken : IMDTokenProvider {
	}

	/// <summary>
	/// TypeDefOrRef coded token interface
	/// </summary>
	public interface ITypeDefOrRef : ICodedToken, IHasCustomAttribute, IMemberRefParent, IType, ITokenOperand {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int TypeDefOrRefTag { get; }
	}

	/// <summary>
	/// HasConstant coded token interface
	/// </summary>
	public interface IHasConstant : ICodedToken, IHasCustomAttribute, IFullName {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int HasConstantTag { get; }

		/// <summary>
		/// Gets/sets the constant value
		/// </summary>
		Constant Constant { get; set; }
	}

	/// <summary>
	/// HasCustomAttribute coded token interface
	/// </summary>
	public interface IHasCustomAttribute : ICodedToken {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int HasCustomAttributeTag { get; }
	}

	/// <summary>
	/// HasFieldMarshal coded token interface
	/// </summary>
	public interface IHasFieldMarshal : ICodedToken, IHasCustomAttribute, IHasConstant, IFullName {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int HasFieldMarshalTag { get; }

		/// <summary>
		/// Gets/sets the field marshal
		/// </summary>
		FieldMarshal FieldMarshal { get; set; }
	}

	/// <summary>
	/// HasDeclSecurity coded token interface
	/// </summary>
	public interface IHasDeclSecurity : ICodedToken, IHasCustomAttribute, IFullName {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int HasDeclSecurityTag { get; }

		/// <summary>
		/// Gets the permission sets
		/// </summary>
		IList<DeclSecurity> DeclSecurities { get; }
	}

	/// <summary>
	/// MemberRefParent coded token interface
	/// </summary>
	public interface IMemberRefParent : ICodedToken, IHasCustomAttribute, IFullName {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int MemberRefParentTag { get; }
	}

	/// <summary>
	/// HasSemantic coded token interface
	/// </summary>
	public interface IHasSemantic : ICodedToken, IHasCustomAttribute, IFullName {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int HasSemanticTag { get; }
	}

	/// <summary>
	/// MethodDefOrRef coded token interface
	/// </summary>
	public interface IMethodDefOrRef : ICodedToken, IHasCustomAttribute, ICustomAttributeType, IMethod {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int MethodDefOrRefTag { get; }
	}

	/// <summary>
	/// MemberForwarded coded token interface
	/// </summary>
	public interface IMemberForwarded : ICodedToken, IHasCustomAttribute, IFullName {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int MemberForwardedTag { get; }

		/// <summary>
		/// Gets/sets the impl map
		/// </summary>
		ImplMap ImplMap { get; set; }
	}

	/// <summary>
	/// Implementation coded token interface
	/// </summary>
	public interface IImplementation : ICodedToken, IHasCustomAttribute, IFullName {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int ImplementationTag { get; }
	}

	/// <summary>
	/// CustomAttributeType coded token interface
	/// </summary>
	public interface ICustomAttributeType : ICodedToken, IHasCustomAttribute {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int CustomAttributeTypeTag { get; }
	}

	/// <summary>
	/// ResolutionScope coded token interface
	/// </summary>
	public interface IResolutionScope : ICodedToken, IHasCustomAttribute, IFullName {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int ResolutionScopeTag { get; }
	}

	/// <summary>
	/// TypeOrMethodDef coded token interface
	/// </summary>
	public interface ITypeOrMethodDef : ICodedToken, IHasCustomAttribute, IHasDeclSecurity, IMemberRefParent, IFullName {
		/// <summary>
		/// The coded token tag
		/// </summary>
		int TypeOrMethodDefTag { get; }

		/// <summary>
		/// Gets the generic parameters
		/// </summary>
		IList<GenericParam> GenericParams { get; }
	}
}
