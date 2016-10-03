// dnlib: See LICENSE.txt for more info

using System;
using System.Diagnostics;
using dnlib.DotNet.Pdb.Managed;

namespace dnlib.DotNet.Pdb {
	/// <summary>
	/// A PDB document
	/// </summary>
	[DebuggerDisplay("{Url}")]
	public sealed class PdbDocument {
		/// <summary>
		/// Gets/sets the document URL
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Gets/sets the language GUID.
		/// </summary>
		public Guid Language { get; set; }

		/// <summary>
		/// Gets/sets the language vendor GUID.
		/// </summary>
		public Guid LanguageVendor { get; set; }

		/// <summary>
		/// Gets/sets the document type GUID.
		/// </summary>
		public Guid DocumentType { get; set; }

		/// <summary>
		/// Gets/sets the checksum algorithm ID
		/// </summary>
		public Guid CheckSumAlgorithmId { get; set; }

		/// <summary>
		/// Gets/sets the checksum
		/// </summary>
		public byte[] CheckSum { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public PdbDocument() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="symDoc">A <see cref="DbiDocument"/> instance</param>
		internal PdbDocument(DbiDocument symDoc) {
			if (symDoc == null)
				throw new ArgumentNullException("symDoc");
			this.Url = symDoc.URL;
			this.Language = symDoc.Language;
			this.LanguageVendor = symDoc.LanguageVendor;
			this.DocumentType = symDoc.DocumentType;
			this.CheckSumAlgorithmId = symDoc.CheckSumAlgorithmId;
			this.CheckSum = symDoc.CheckSum;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="url">Document URL</param>
		/// <param name="language">Language.</param>
		/// <param name="languageVendor">Language vendor.</param>
		/// <param name="documentType">Document type.</param>
		/// <param name="checkSumAlgorithmId">Checksum algorithm ID</param>
		/// <param name="checkSum">Checksum</param>
		public PdbDocument(string url, Guid language, Guid languageVendor, Guid documentType, Guid checkSumAlgorithmId, byte[] checkSum) {
			this.Url = url;
			this.Language = language;
			this.LanguageVendor = languageVendor;
			this.DocumentType = documentType;
			this.CheckSumAlgorithmId = checkSumAlgorithmId;
			this.CheckSum = checkSum;
		}

		/// <inheritdoc/>
		public override int GetHashCode() {
			return (Url ?? string.Empty).ToUpperInvariant().GetHashCode();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj) {
			var other = obj as PdbDocument;
			if (other == null)
				return false;
			return (Url ?? string.Empty).Equals(other.Url ?? string.Empty, StringComparison.OrdinalIgnoreCase);
		}
	}
}
