// dnlib: See LICENSE.txt for more info

namespace dnlib {
	/// <summary>
	/// dnlib settings
	/// </summary>
	public static class Settings {
		/// <summary>
		/// <c>true</c> if dnlib is thread safe. (<c>THREAD_SAFE</c> was defined during compilation)
		/// </summary>
		public static bool IsThreadSafe {
			get {
#if THREAD_SAFE
				return true;
#else
				return false;
#endif
			}
		}

		/// <summary>
		/// <c>true</c> if dnlib is has resource reader. (<c>NO_RESREAD</c> was not defined during compilation)
		/// </summary>
		public static bool HasResourceReader {
			get {
#if !NO_RESREAD
				return true;
#else
				return false;
#endif
			}
		}

		/// <summary>
		/// <c>true</c> if dnlib is has MMap support. (<c>NO_MMAP</c> was not defined during compilation)
		/// </summary>
		public static bool HasMMap {
			get {
#if !NO_MMAP
				return true;
#else
				return false;
#endif
			}
		}

		/// <summary>
		/// <c>true</c> if dnlib is has Crypto support. (<c>NO_CRYPTO</c> was not defined during compilation)
		/// </summary>
		public static bool HasCrypto {
			get {
#if !NO_CRYPTO
				return true;
#else
				return false;
#endif
			}
		}
	}
}
