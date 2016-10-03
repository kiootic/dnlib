// dnlib: See LICENSE.txt for more info

﻿using System;

namespace dnlib.DotNet.Pdb.Managed {
	sealed class DbiNamespace {
		public string Namespace { get; private set; }

		public DbiNamespace(string ns) {
			Namespace = ns;
		}
	}
}