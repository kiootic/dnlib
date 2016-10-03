// dnlib: See LICENSE.txt for more info

﻿using System;
using System.Diagnostics.SymbolStore;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed {
	sealed class DbiDocument {
		public string URL { get; private set; }
		public Guid Language { get; private set; }
		public Guid LanguageVendor { get; private set; }
		public Guid DocumentType { get; private set; }
		public Guid CheckSumAlgorithmId { get; private set; }
		public byte[] CheckSum { get; private set; }

		public DbiDocument(string url) {
			URL = url;
			DocumentType = new Guid(0x5a869d0b, 0x6611, 0x11d3, 0xbd, 0x2a, 0x0, 0x0, 0xf8, 0x8, 0x49, 0xbd);
		}

		public void Read(IImageStream stream) {
			stream.Position = 0;
			Language = new Guid(stream.ReadBytes(0x10));
			LanguageVendor = new Guid(stream.ReadBytes(0x10));
			DocumentType = new Guid(stream.ReadBytes(0x10));
			CheckSumAlgorithmId = new Guid(stream.ReadBytes(0x10));

			var len = stream.ReadInt32();
			if (stream.ReadUInt32() != 0)
				throw new PdbException("Unexpected value");

			CheckSum = stream.ReadBytes(len);
		}
	}
}