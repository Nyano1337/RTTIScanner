﻿using System.Diagnostics.Contracts;

namespace RTTIScanner.ClassExtensions
{
	public static class IntPtrExtension
	{
		[Pure]
		public static bool IsNull(this IntPtr ptr)
		{
			return ptr == IntPtr.Zero;
		}

		[Pure]
		public static bool IsValid(this IntPtr ptr)
		{
#if RTTISCANNER64
			return ptr.IsInRange((IntPtr)0x10000, (IntPtr)long.MaxValue);
#else
			return ptr.IsInRange((IntPtr)0x10000, (IntPtr)int.MaxValue);
#endif
		}

		[Pure]
		public static IntPtr Add(this IntPtr lhs, IntPtr rhs)
		{
#if RTTISCANNER64
			return new IntPtr(lhs.ToInt64() + rhs.ToInt64());
#else
			return new IntPtr(lhs.ToInt32() + rhs.ToInt32());
#endif
		}

		[Pure]
		public static IntPtr Sub(this IntPtr lhs, IntPtr rhs)
		{
#if RTTISCANNER64
			return new IntPtr(lhs.ToInt64() - rhs.ToInt64());
#else
			return new IntPtr(lhs.ToInt32() - rhs.ToInt32());
#endif
		}

		[Pure]
		public static IntPtr Mul(this IntPtr lhs, IntPtr rhs)
		{
#if RTTISCANNER64
			return new IntPtr(lhs.ToInt64() * rhs.ToInt64());
#else
			return new IntPtr(lhs.ToInt32() * rhs.ToInt32());
#endif
		}

		[Pure]
		public static IntPtr Div(this IntPtr lhs, IntPtr rhs)
		{
			Contract.Requires(!rhs.IsNull());

#if RTTISCANNER64
			return new IntPtr(lhs.ToInt64() / rhs.ToInt64());
#else
			return new IntPtr(lhs.ToInt32() / rhs.ToInt32());
#endif
		}

		[Pure]
		public static int Mod(this IntPtr lhs, int mod)
		{
#if RTTISCANNER64
			return (int)(lhs.ToInt64() % mod);
#else
			return lhs.ToInt32() % mod;
#endif
		}

		[Pure]
		public static IntPtr Negate(this IntPtr ptr)
		{
#if RTTISCANNER64
			return new IntPtr(-ptr.ToInt64());
#else
			return new IntPtr(-ptr.ToInt32());
#endif
		}

		[Pure]
		public static bool IsInRange(this IntPtr address, IntPtr start, IntPtr end)
		{
#if RTTISCANNER64
			var val = (ulong)address.ToInt64();
			return (ulong)start.ToInt64() <= val && val <= (ulong)end.ToInt64();
#else
			var val = (uint)address.ToInt32();
			return (uint)start.ToInt32() <= val && val <= (uint)end.ToInt32();
#endif
		}

		[Pure]
		public static int CompareTo(this IntPtr lhs, IntPtr rhs)
		{
#if RTTISCANNER64
			return ((ulong)lhs.ToInt64()).CompareTo((ulong)rhs.ToInt64());
#else
			return ((uint)lhs.ToInt32()).CompareTo((uint)rhs.ToInt32());
#endif
		}

		[Pure]
		public static int CompareToRange(this IntPtr address, IntPtr start, IntPtr end)
		{
			if (IsInRange(address, start, end))
			{
				return 0;
			}
			return CompareTo(address, start);
		}

		/// <summary>
		/// Changes the behaviour of ToInt64 in x86 mode.
		/// IntPtr(int.MaxValue + 1) = (int)0x80000000 (-2147483648) = (long)0xFFFFFFFF80000000
		/// This method converts the value to (long)0x0000000080000000 (2147483648).
		/// </summary>
		/// <param name="ptr"></param>
		/// <returns></returns>
		[Pure]
		public static long ToInt64Bits(this IntPtr ptr)
		{
#if RTTISCANNER64
			return ptr.ToInt64();
#else
			var value = ptr.ToInt64();

			if (value < 0)
			{
				var intValue = ptr.ToInt32();
				if (value == intValue)
				{
					value = intValue & 0xFFFFFFFFL;
				}
			}

			return value;
#endif
		}

		[Pure]
		public static IntPtr From(int value)
		{
			return (IntPtr)value;
		}

		[Pure]
		public static IntPtr From(long value)
		{
#if RTTISCANNER64
			return (IntPtr)value;
#else
			return (IntPtr)unchecked((int)value);
#endif
		}
	}
}
