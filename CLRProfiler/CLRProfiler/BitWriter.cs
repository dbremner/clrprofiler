/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  BitWriter
 *
 * Description: Bitstream writer
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CLRProfiler
{
	internal sealed class BitWriter
	{
		private int position, incurr;

		private readonly BinaryWriter bw;

		private readonly ulong[] buffer;

		private long bitsRecorded;

		internal BitWriter(Stream o)
		{
			bw = new BinaryWriter(o);
			buffer = new ulong[4096];
			buffer[0] = 0;
			position = incurr = 0;
			bitsRecorded = 0;
		}

		internal void WriteBits(ulong bits, int numBits)
		{
			int space = 64 - incurr;
			if(space >= numBits)
			{
				buffer[position] <<= numBits;
				buffer[position] |= (bits & ((1ul << numBits) - 1));
				incurr += numBits;
			}
			else
			{
				buffer[position] <<= space;
				buffer[position] |= (bits & ((1ul << numBits) - 1)) >> (numBits - space);
				if(++position >= 4096)
				{
					incurr = 0;
					Flush();
				}
				incurr = numBits - space;
				buffer[position] = bits & ((1ul << incurr) - 1);
			}
			bitsRecorded += numBits;
		}

		internal void Flush()
		{
			if(incurr != 0)
			{
				buffer[position++] <<= 64 - incurr;
			}
			// how to write an array of longs faster?
			for(int i = 0; i < position; i++)
			{
				bw.Write(buffer[i]);
			}
			position = incurr = 0;
		}

		[SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
		internal long Position => bitsRecorded;
	}
}
