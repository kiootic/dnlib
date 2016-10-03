// dnlib: See LICENSE.txt for more info

using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb.Managed;
using dnlib.Threading;

namespace dnlib.DotNet.Pdb {
	/// <summary>
	/// PDB state for a <see cref="ModuleDef"/>
	/// </summary>
	public sealed class PdbState {
		readonly PdbReader reader;
		readonly Dictionary<PdbDocument, PdbDocument> docDict = new Dictionary<PdbDocument, PdbDocument>();
		MethodDef userEntryPoint;

#if THREAD_SAFE
		readonly Lock theLock = Lock.Create();
#endif

		/// <summary>
		/// Gets/sets the user entry point method.
		/// </summary>
		public MethodDef UserEntryPoint {
			get { return userEntryPoint; }
			set { userEntryPoint = value; }
		}

		/// <summary>
		/// Gets all PDB documents
		/// </summary>
		public IEnumerable<PdbDocument> Documents {
			get {
#if THREAD_SAFE
				theLock.EnterWriteLock(); try {
					return new List<PdbDocument>(docDict.Values);
				} finally { theLock.ExitWriteLock(); }
#else
				return docDict.Values;
#endif
			}
		}

		/// <summary>
		/// <c>true</c> if <see cref="Documents"/> is not empty
		/// </summary>
		public bool HasDocuments {
			get {
#if THREAD_SAFE
				theLock.EnterWriteLock(); try {
#endif
				return docDict.Count > 0;
#if THREAD_SAFE
				} finally { theLock.ExitWriteLock(); }
#endif
		
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public PdbState() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reader">A <see cref="PdbReader"/> instance</param>
		/// <param name="module">Owner module</param>
		public PdbState(PdbReader reader, ModuleDefMD module) {
			if (reader == null)
				throw new ArgumentNullException("reader");
			if (module == null)
				throw new ArgumentNullException("module");
			this.reader = reader;

			this.userEntryPoint = module.ResolveToken(reader.entryPt) as MethodDef;

			foreach (var doc in reader.documents.Values)
				Add_NoLock(new PdbDocument(doc));
		}

		/// <summary>
		/// Adds <paramref name="doc"/>
		/// </summary>
		/// <param name="doc">New document</param>
		/// <returns><paramref name="doc"/> if it wasn't inserted, or the already existing document
		/// if it was already inserted.</returns>
		public PdbDocument Add(PdbDocument doc) {
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
			return Add_NoLock(doc);
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
		}

		PdbDocument Add_NoLock(PdbDocument doc) {
			PdbDocument orig;
			if (docDict.TryGetValue(doc, out orig))
				return orig;
			docDict.Add(doc, doc);
			return doc;
		}

		/// <summary>
		/// Removes <paramref name="doc"/>
		/// </summary>
		/// <param name="doc">Document</param>
		/// <returns><c>true</c> if it was removed, <c>false</c> if it wasn't inserted.</returns>
		public bool Remove(PdbDocument doc) {
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
			return docDict.Remove(doc);
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
		}

		/// <summary>
		/// Returns an inserted <see cref="PdbDocument"/> instance or <c>null</c> if it's not been
		/// inserted yet.
		/// </summary>
		/// <param name="doc">A PDB document</param>
		/// <returns>The existing <see cref="PdbDocument"/> or <c>null</c> if it doesn't exist.</returns>
		public PdbDocument GetExisting(PdbDocument doc) {
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
			PdbDocument orig;
			docDict.TryGetValue(doc, out orig);
			return orig;
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
		}

		/// <summary>
		/// Removes all documents
		/// </summary>
		/// <returns></returns>
		public void RemoveAllDocuments() {
			RemoveAllDocuments(false);
		}

		/// <summary>
		/// Removes all documents and optionally returns them
		/// </summary>
		/// <param name="returnDocs"><c>true</c> if all the original <see cref="PdbDocument"/>s
		/// should be returned.</param>
		/// <returns>All <see cref="PdbDocument"/>s if <paramref name="returnDocs"/> is <c>true</c>
		/// or <c>null</c> if <paramref name="returnDocs"/> is <c>false</c>.</returns>
		public List<PdbDocument> RemoveAllDocuments(bool returnDocs) {
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
			var docs = returnDocs ? new List<PdbDocument>(docDict.Values) : null;
			docDict.Clear();
			return docs;
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
		}

		/// <summary>
		/// Initializes a <see cref="CilBody"/> with information found in the PDB file. The
		/// instructions in <paramref name="body"/> must have valid offsets. This method is
		/// automatically called by <see cref="ModuleDefMD"/> and you don't need to explicitly call
		/// it.
		/// </summary>
		/// <param name="body">Method body</param>
		/// <param name="methodRid">Method row ID</param>
		public void InitializeDontCall(CilBody body, uint methodRid) {
			if (reader == null || body == null)
				return;
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
			var method = reader.functions[0x06000000 + methodRid];
			if (method != null) {
				body.Scope = CreateScope(body, method.Root);
				AddSequencePoints(body, method);
			}
			//TODO: reader.GetSymAttribute()
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
		}

		void AddSequencePoints(CilBody body, DbiFunction method) {
			int instrIndex = 0;
			foreach(var seqPt in method.Lines) {
				var instr = GetInstruction(body.Instructions, (int)seqPt.Offset, ref instrIndex);
				if (instr == null)
					continue;
				var seqPoint = new SequencePoint() {
					Document = Add_NoLock(new PdbDocument(seqPt.Document)),
					StartLine = (int)seqPt.LineBegin,
					StartColumn = (int)seqPt.ColumnBegin,
					EndLine = (int)seqPt.LineEnd,
					EndColumn = (int)seqPt.ColumnEnd,
				};
				instr.SequencePoint = seqPoint;
			}
		}

		struct CreateScopeState {
			public DbiScope SymScope;
			public PdbScope PdbScope;
			public IList<DbiScope> Children;
			public int ChildrenIndex;
		}

		static PdbScope CreateScope(CilBody body, DbiScope symScope) {
			if (symScope == null)
				return null;

			// Don't use recursive calls
			var stack = new Stack<CreateScopeState>();
			var state = new CreateScopeState() { SymScope = symScope };
recursive_call:
			int instrIndex = 0;
			state.PdbScope = new PdbScope() {
				Start = GetInstruction(body.Instructions, (int)state.SymScope.BeginOffset, ref instrIndex),
				End   = GetInstruction(body.Instructions, (int)state.SymScope.EndOffset, ref instrIndex),
			};

			foreach (var symLocal in state.SymScope.Variables) {
				if (symLocal.AddressKind != SymAddressKind.ILOffset)
					continue;

				int localIndex = symLocal.AddressField1;
				if ((uint)localIndex >= (uint)body.Variables.Count)
					continue;
				var local = body.Variables[localIndex];
				local.Name = symLocal.Name;
				var attributes = symLocal.Attributes;
				if (attributes is int)
					local.PdbAttributes = (int)attributes;
				state.PdbScope.Variables.Add(local);
			}

			foreach (var ns in state.SymScope.Namespaces)
				state.PdbScope.Namespaces.Add(ns.Namespace);

			// Here's the now somewhat obfuscated for loop
			state.ChildrenIndex = 0;
			state.Children = state.SymScope.Children;
do_return:
			if (state.ChildrenIndex < state.Children.Count) {
				var child = state.Children[state.ChildrenIndex];
				stack.Push(state);
				state = new CreateScopeState() { SymScope = child };
				goto recursive_call;
			}

			if (stack.Count == 0)
				return state.PdbScope;

			// Return from recursive call, and execute the last part of the for loop
			var newPdbScope = state.PdbScope;
			state = stack.Pop();
			state.PdbScope.Scopes.Add(newPdbScope);
			state.ChildrenIndex++;
			goto do_return;
		}

		static Instruction GetInstruction(IList<Instruction> instrs, int offset, ref int index) {
			if (instrs.Count > 0 && offset > instrs[instrs.Count - 1].Offset)
				return null;
			for (int i = index; i < instrs.Count; i++) {
				var instr = instrs[i];
				if (instr.Offset < offset)
					continue;
				if (instr.Offset == offset) {
					index = i;
					return instr;
				}
				break;
			}
			for (int i = 0; i < index; i++) {
				var instr = instrs[i];
				if (instr.Offset < offset)
					continue;
				if (instr.Offset == offset) {
					index = i;
					return instr;
				}
				break;
			}
			return null;
		}
	}
}
