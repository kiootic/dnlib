// dnlib: See LICENSE.txt for more info

﻿using System;
using System.Collections.Generic;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed {
	sealed class DbiFunction {
		public uint Token { get; internal set; }
		public string Name { get; private set; }
		public PdbAddress Address { get; private set; }
		public DbiScope Root { get; private set; }
		public IList<DbiSourceLine> Lines { get; internal set; }

		public void Read(IImageStream stream, long recEnd) {
			stream.Position += 4;
			var end = stream.ReadUInt32();
			stream.Position += 4;
			var len = stream.ReadUInt32();
			stream.Position += 8;
			Token = stream.ReadUInt32();
			Address = PdbAddress.ReadAddress(stream);
			stream.Position += 1 + 2;
			Name = PdbReader.ReadCString(stream);

			stream.Position = recEnd;
			Root = new DbiScope("", Address.Offset, len);
			Root.Read(new RecursionCounter(), stream, end);
			FixOffsets(new RecursionCounter(), Root);
		}

		void FixOffsets(RecursionCounter counter, DbiScope scope) {
			if (!counter.Increment())
				return;

			scope.BeginOffset -= Address.Offset;
			scope.EndOffset -= Address.Offset;
			foreach (var child in scope.Children)
				FixOffsets(counter, child);

			counter.Decrement();
		}
	}
}